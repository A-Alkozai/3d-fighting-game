using UnityEngine;
using System.Collections.Generic;

public class MoveSelector
{
    // Objects
    private InputBuffer inputBuffer;
    private MovesDatabase movesDatabase;
    private StateManager stateManager;
    private MoveExecutor moveExecutor;
    private FacingDirection facingDirection;

    // Variables
    private List<InputObject> activeInput = new List<InputObject>();
    private List<InputObject> heldInputs = new List<InputObject>();
    private List<InputObject> heldInputsWhileAttack = new List<InputObject>();

    // Getters
    public IReadOnlyList<InputObject> ActiveInput => activeInput;
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

    public void Update()
    {
        ReadBuffer();
        ReadHeldInputs();
    }

    public string DecideInputType(InputCommand input)
    {
        if (ActiveAttackNode != null)
        {
            return "attack";
        }

        if (input == InputCommand.LeftPunch || input == InputCommand.RightPunch ||
            input == InputCommand.LeftKick  || input == InputCommand.RightKick  ||
            input == InputCommand.RageArt)
        { return "attack"; }

        if (input == InputCommand.BackwardHold || input == InputCommand.ForwardHold ||
            input == InputCommand.UpHold       || input == InputCommand.DownHold    ||
            input == InputCommand.Up           || input == InputCommand.Down)
        { return "movement"; }
        return "neutral";
    }

    public void ReadBuffer()
    {
        if (inputBuffer.Count() == 0)
            return;

        int index = 0;
        int readInputs = 0;

        while (index < inputBuffer.Count())
        {
            InputObject input = inputBuffer.GetInputAt(index);
            NormaliseInput(input);
            InputCommand inputCommand = input.GetInputCommand();
            MoveNode newInputNode;
            string inputType = DecideInputType(inputCommand);

            if (inputType == "neutral")
            {
                moveExecutor.SetPrevNeutralInput(input);
                index++;
                continue;
            }

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

            if (newInputNode == null)
                break;

            MoveData executableMove = ActiveMove;

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
        RemoveBufferInputs(readInputs - 1);
    }

    public void ReadHeldInputs()
    {
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
        else
        {
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

    public MoveNode GetNextNode(string inputType, InputCommand inputCommand, MoveNode currentNode=null)
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

    public void Execute(bool isAttackNode, MoveNode newNode, InputObject input, MoveData move)
    {
        moveExecutor.SetCurrentMove(move, newNode, isAttackNode);
        activeInput.Add(input);
        if (move.IsLoop)
        {
            moveExecutor.SetLoopInput(input);
        }
    }

    public MoveData GetExecutableMove(IReadOnlyList<MoveData> moves)
    {
        foreach (MoveData move in moves)
        {
            bool canExecute = true;
            if (FrameCounter != null && FrameCounter.GetFrameNumber() >= move.BranchDelay
                                     && move.BranchDelay != 0)
            {
                canExecute = false;
                continue;
            }

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

    public void NormaliseInput(InputObject input)
    {
        InputCommand command = input.GetInputCommand();
        InputCommand normalisedCommand = command;
        FacingDirection facingRight = FacingDirection.Right;

        if (facingDirection == facingRight)
        {
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

    public bool ExistsPrevNeutralInput()
    {
        if (PrevNeutralInput != null && !inputBuffer.Contains(PrevNeutralInput))
        {
            moveExecutor.SetPrevNeutralInput(null);
            return false;
        }
        return true;
    }

    public void AddHeldInput(InputObject heldInput)
    {
        NormaliseInput(heldInput);
        if (ActiveAttackNode == null) { heldInputs.Add(heldInput); }
        else { heldInputsWhileAttack.Add(heldInput); }
    }

    public void ClearActiveMove()
    {
        moveExecutor.CancelMove();
        activeInput.Clear();
    }

    public void RemoveBufferInputs(int index)
    {
        for (int i = index; i >= 0; i--)
        {
            inputBuffer.RemoveInputAt(i);
        }
    }

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