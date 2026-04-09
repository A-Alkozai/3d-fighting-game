using UnityEngine;
using System.Collections.Generic;

// Reads input from the buffer and held inputs, traverses the move trees to find matching moves,
// checks state/branchDelay requirements, and tells MoveExecutor what to play
public class MoveSelector
{
    private InputBuffer inputBuffer;
    private MovesDatabase movesDatabase;
    private StateManager stateManager;
    private MoveExecutor moveExecutor;
    private FacingDirection facingDirection;

    private List<InputObject> activeInput = new List<InputObject>();               // Inputs consumed by the current move
    private List<InputObject> heldInputs = new List<InputObject>();                // Currently held directional inputs
    private List<InputObject> heldInputsWhileAttack = new List<InputObject>();     // Held inputs queued during an attack

    public IReadOnlyList<InputObject> ActiveInput => activeInput;
    public bool IsStateMove => moveExecutor.IsStateMove;
    public MoveData ActiveMove => moveExecutor.CurrentMove;
    public MoveNode ActiveAttackNode => moveExecutor.ActiveAttackNode;
    public MoveNode ActiveMovementNode => moveExecutor.ActiveMovementNode;
    public InputObject PrevNeutralInput => moveExecutor.PrevNeutralInput;
    public FrameCounter FrameCounter => moveExecutor.FrameCounter;

    public MoveSelector(InputBuffer inputBuffer, MovesDatabase movesDatabase,
                        StateManager stateManager, MoveExecutor moveExecutor)
    {
        this.inputBuffer = inputBuffer;
        this.movesDatabase = movesDatabase;
        this.stateManager = stateManager;
        this.moveExecutor = moveExecutor;
        facingDirection = stateManager.GetFacingDirection();
    }

    // Main update: process buffered inputs, then held inputs, then set fallback if nothing active
    public void Update()
    {
        ReadBuffer();
        ReadHeldInputs();
        FallbackMove();
    }

    // Determine which tree to search based on the input command
    // If already in an attack chain, all inputs route to the attack tree
    public string DecideInputType(InputCommand input)
    {
        // Mid-attack chain: everything goes through the attack tree
        if (ActiveAttackNode != null)
        {
            return "attack";
        }

        // Attack buttons always go to the attack tree
        if (input == InputCommand.LeftPunch || input == InputCommand.RightPunch ||
            input == InputCommand.LeftKick  || input == InputCommand.RightKick  ||
            input == InputCommand.RageArt)
        { return "attack"; }

        // Held directionals and up/down go to the movement tree
        if (input == InputCommand.BackwardHold || input == InputCommand.ForwardHold ||
            input == InputCommand.UpHold       || input == InputCommand.DownHold    ||
            input == InputCommand.Up           || input == InputCommand.Down)
        { return "movement"; }

        // Tap directionals (forward/backward) are neutral - stored as context for compound inputs
        return "neutral";
    }

    // Process buffered inputs one at a time, traversing the move tree for each
    public void ReadBuffer()
    {
        if (inputBuffer.Count() == 0)
            return;

        int index = 0;
        int readInputs = 0;

        while (index < inputBuffer.Count())
        {
            InputObject input = inputBuffer.GetInputAt(index);

            // Stop at pending inputs (still determining tap vs hold)
            if (input.IsPending())
                break;

            // Convert Left/Right to Forward/Backward based on facing direction
            NormaliseInput(input);
            InputCommand inputCommand = input.GetInputCommand();
            MoveNode newInputNode;
            string inputType = DecideInputType(inputCommand);

            // Neutral inputs are saved as context (e.g. Forward before LeftPunch = Forward+LeftPunch)
            if (inputType == "neutral")
            {
                moveExecutor.SetPrevNeutralInput(input);
                index++;
                continue;
            }

            // If there's a previous neutral input, try the compound path first (neutral → this input)
            if (ExistsPrevNeutralInput() && PrevNeutralInput != null)
            {
                newInputNode = GetNextNode(inputType, PrevNeutralInput.GetInputCommand());
                if (newInputNode == null)
                {
                    break;
                }
                newInputNode = GetNextNode(inputType, inputCommand, newInputNode);
            }
            else
            {
                newInputNode = GetNextNode(inputType, inputCommand);
            }

            // No matching node in the tree - stop processing
            if (newInputNode == null)
                break;

            MoveData executableMove = ActiveMove;

            // If this node has moves attached, check if any are executable (state + branchDelay)
            if (newInputNode.MoveDatas.Count > 0)
            {
                IReadOnlyList<MoveData> movesList = newInputNode.MoveDatas;
                executableMove = GetExecutableMove(movesList);

                if (executableMove == null)
                    break;
            }

            Execute(true, newInputNode, input, executableMove);
            index++;
            readInputs++;
        }
        // Remove consumed inputs from the buffer
        RemoveBufferInputs(readInputs - 1);
    }

    // Process held directional inputs (e.g. holding forward to walk/run)
    public void ReadHeldInputs()
    {
        // During an attack chain: check if held inputs can extend the chain
        if (ActiveAttackNode != null)
        {
            if (heldInputsWhileAttack.Count == 0) return;

            foreach (InputObject input in heldInputsWhileAttack)
            {
                InputCommand inputCommand = input.GetInputCommand();
                MoveNode newInputNode = movesDatabase.GetNextNode(inputCommand, ActiveAttackNode);

                if (newInputNode == null) return;

                MoveData executableMove = ActiveMove;

                if (newInputNode.MoveDatas.Count > 0)
                {
                    IReadOnlyList<MoveData> movesList = newInputNode.MoveDatas;
                    executableMove = GetExecutableMove(movesList);
                    if (executableMove == null) break;
                }
                Execute(true, newInputNode, input, executableMove);
            }
        }
        // No attack active: process held inputs as movement
        else
        {
            // Move queued attack-held inputs back to the regular held list
            if (heldInputsWhileAttack.Count > 0)
            {
                heldInputs.AddRange(heldInputsWhileAttack);
                heldInputsWhileAttack.Clear();
            }
            
            int index = 0;

            while (index < heldInputs.Count)
            {
                InputObject input = heldInputs[index];
                InputCommand inputCommand = input.GetInputCommand();
                MoveNode newInputNode;
                string inputType = DecideInputType(inputCommand);

                // Try compound path with neutral input if one exists
                if (ExistsPrevNeutralInput() && PrevNeutralInput != null)
                {
                    newInputNode = GetNextNode(inputType, PrevNeutralInput.GetInputCommand());
                    newInputNode = GetNextNode(inputType, inputCommand, newInputNode);
                }
                else newInputNode = GetNextNode(inputType, inputCommand);

                if (newInputNode == null) break;

                MoveData executableMove = ActiveMove;

                if (newInputNode.MoveDatas.Count > 0)
                {
                    IReadOnlyList<MoveData> movesList = newInputNode.MoveDatas;
                    executableMove = GetExecutableMove(movesList);

                    if (executableMove == null) break;
                }

                Execute(false, newInputNode, input, executableMove);
                index++;
            }
        }
    }

    // When no move is active or only a fallback is playing, pick the appropriate idle/stance move
    public void FallbackMove()
    {
        if (ActiveMove == null || IsStateMove)
        {
            // Clean up sidestepping state when it finishes
            if (stateManager.HasState(PlayerStates.Sidestepping))
            {
                stateManager.RemoveState(PlayerStates.Sidestepping);
                stateManager.AddState(PlayerStates.Idle);
            }

            // Pick fallback based on current state priority
            MoveData move = null;
            if (stateManager.HasState(PlayerStates.Falling))
            {
                move = movesDatabase.GetMoveById("falling");
            }
            else if (stateManager.HasState(PlayerStates.Lying))
            {
                move = movesDatabase.GetMoveById("lying");
            }
            else if (stateManager.HasState(PlayerStates.Rising))
            {
                move = movesDatabase.GetMoveById("rising");
            }
            else if (stateManager.HasState(PlayerStates.Crouching))
            {
                move = movesDatabase.GetMoveById("crouching");
            }
            else if (stateManager.HasState(PlayerStates.Idle))
            {
                move = movesDatabase.GetMoveById("idle");
            }
            ExecuteFallback(move);
        }
    }

    // Navigate to the next node in the appropriate tree (attack or movement)
    // If a currentNode is provided, traverse from there; otherwise use the active node or root
    public MoveNode GetNextNode(string inputType, InputCommand inputCommand, MoveNode currentNode = null)
    {
        if (currentNode != null)
        {
            return movesDatabase.GetNextNode(inputCommand, currentNode);
        }
        if (inputType == "attack")
        {
            if (ActiveAttackNode == null)
            {
                return movesDatabase.GetNextNode(inputCommand, movesDatabase.RootAttackNode);
            }
            return movesDatabase.GetNextNode(inputCommand, ActiveAttackNode);
        }
        if (ActiveMovementNode == null)
        {
            return movesDatabase.GetNextNode(inputCommand, movesDatabase.RootMovementNode);
        }
        return movesDatabase.GetNextNode(inputCommand, ActiveMovementNode);
    }

    // Tell MoveExecutor to start/change to the chosen move, track the input, set loop if needed
    public void Execute(bool isAttackNode, MoveNode newNode, InputObject input, MoveData move)
    {
        moveExecutor.SetCurrentMove(move, newNode, isAttackNode);
        activeInput.Add(input);
        if (move.IsLoop)
        {
            moveExecutor.SetLoopInput(input);
        }
    }

    // Set a fallback move - skip if the same fallback is already playing
    public void ExecuteFallback(MoveData move)
    {
        if (ActiveMove != null && move != null && move.Id == ActiveMove.Id)
            return;
        moveExecutor.SetFallback(move);
    }

    // Find the first move in the list that passes state and branchDelay checks
    public MoveData GetExecutableMove(IReadOnlyList<MoveData> moves)
    {
        foreach (MoveData move in moves)
        {
            bool canExecute = true;

            // BranchDelay: if current move is past the branch window, this chain move can't execute
            if (FrameCounter != null && FrameCounter.GetFrameNumber() >= move.BranchDelay
                                     && move.BranchDelay != 0)
            {
                canExecute = false;
                continue;
            }

            // Check all required states - if any are blocked, this move can't execute
            foreach (PlayerStates state in move.RequiredStates)
            {
                if (!stateManager.CanToggleState(state))
                {
                    canExecute = false;
                    break;
                }
            }

            if (canExecute)
                return move;
        }
        return null;
    }

    // Convert raw Left/Right inputs to Forward/Backward based on which way the player is facing
    public void NormaliseInput(InputObject input)
    {
        InputCommand command = input.GetInputCommand();
        InputCommand normalisedCommand = command;
        FacingDirection facingRight = FacingDirection.Right;

        if (facingDirection == facingRight)
        {
            // Facing right: Right = Forward, Left = Backward
            switch (command)
            {
                case InputCommand.Right:
                    normalisedCommand = InputCommand.Forward;
                    break;
                case InputCommand.RightHold:
                    normalisedCommand = InputCommand.ForwardHold;
                    break;
                case InputCommand.Left:
                    normalisedCommand = InputCommand.Backward;
                    break;
                case InputCommand.LeftHold:
                    normalisedCommand = InputCommand.BackwardHold;
                    break;
            }
            input.ChangeInputCommand(normalisedCommand);
            return;
        }
        else
        {
            // Facing left: Left = Forward, Right = Backward (reversed)
            switch (command)
            {
                case InputCommand.Right:
                    normalisedCommand = InputCommand.Backward;
                    break;
                case InputCommand.RightHold:
                    normalisedCommand = InputCommand.BackwardHold;
                    break;
                case InputCommand.Left:
                    normalisedCommand = InputCommand.Forward;
                    break;
                case InputCommand.LeftHold:
                    normalisedCommand = InputCommand.ForwardHold;
                    break;
            }
            input.ChangeInputCommand(normalisedCommand);
        }
    }

    // Check if the previous neutral input is still in the buffer - if not, clear it
    public bool ExistsPrevNeutralInput()
    {
        if (PrevNeutralInput != null && !inputBuffer.Contains(PrevNeutralInput))
        {
            moveExecutor.SetPrevNeutralInput(null);
            return false;
        }
        return true;
    }

    // Add a held input to the appropriate list based on whether an attack is active
    public void AddHeldInput(InputObject heldInput)
    {
        NormaliseInput(heldInput);
        if (ActiveAttackNode == null) { heldInputs.Add(heldInput); }
        else { heldInputsWhileAttack.Add(heldInput); }
    }

    // Force-cancel the current move and clear all consumed inputs
    public void ClearActiveMove()
    {
        moveExecutor.CancelMove();
        activeInput.Clear();
    }

    // Remove consumed inputs from the buffer (called after ReadBuffer processes them)
    public void RemoveBufferInputs(int index)
    {
        for (int i = index; i >= 0; i--)
        {
            inputBuffer.RemoveInputAt(i);
        }
    }

    // Remove held inputs whose frame counter is -1 (key was released)
    public void UpdateHeldInputs()
    {
        for (int i = heldInputs.Count - 1; i >= 0; i--)
        {
            if (heldInputs[i].GetFrame().GetFrameNumber() == -1)
            {
                heldInputs.RemoveAt(i);
            }
        }

        for (int i = heldInputsWhileAttack.Count - 1; i >= 0; i--)
        {
            if (heldInputsWhileAttack[i].GetFrame().GetFrameNumber() == -1)
            {
                heldInputsWhileAttack.RemoveAt(i);
            }
        }
    }

}