using UnityEngine;
using System.Collections.Generic;

public class MoveSelector
{
    // Objects
    private InputBuffer inputBuffer;
    private MovesDatabase movesDatabase;
    private StateManager stateManager;
    private FacingDirection facingDirection;

    // Variables
    private List<InputObject> activeInput = new List<InputObject>();
    private List<InputObject> heldInputs = new List<InputObject>();
    private List<InputObject> heldInputsWhileAttack = new List<InputObject>();
    private FrameCounter frameCounter;
    private InputObject prevNeutralInput;
    // Current Move
    private MoveData activeMove;
    // Tapped Input
    private MoveNode activeAttackNode;
    // Held Input
    private MoveNode activeMovementNode;

    // Getters
    public IReadOnlyList<InputObject> ActiveInput => activeInput;
    public MoveData ActiveMove => activeMove;
    public MoveNode ActiveAttackNode => activeAttackNode;
    public MoveNode ActiveMovementNode => activeMovementNode;

    public MoveSelector(InputBuffer inputBuffer, MovesDatabase movesDatabase, StateManager stateManager)
    {
        this.inputBuffer = inputBuffer;
        this.movesDatabase = movesDatabase;
        this.stateManager = stateManager;
        facingDirection = stateManager.GetFacingDirection();
    }

    public void Update()
    {
        ReadBuffer();
        ReadHeldInputs();
        UpdateFrame();
        UpdateHeldInputs();
    }

    public string DecideInputType(InputCommand input)
    {
        if (activeAttackNode != null)
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
        int index = 0;

        while (index < inputBuffer.Count())
        {
            InputObject input = inputBuffer.GetInputAt(index);
            NormaliseInput(input);
            InputCommand inputCommand = input.GetInputCommand();
            MoveNode newInputNode;
            string inputType = DecideInputType(inputCommand);

            if (inputType == "neutral")
            {
                prevNeutralInput = input;
                index++;
                continue;
            }

            if (prevNeutralInput != null)
            {
                newInputNode = GetNextNode(inputType, prevNeutralInput.GetInputCommand());
                newInputNode = GetNextNode(inputType, inputCommand, newInputNode);
            }
            else newInputNode = GetNextNode(inputType, inputCommand);

            if (newInputNode == null)
            {
                index--;
                break;
            }

            MoveData executableMove = activeMove;

            if (newInputNode.MoveDatas.Count > 0)
            {
                IReadOnlyList<MoveData> movesList = newInputNode.MoveDatas;
                executableMove = GetExecutableMove(movesList);

                if (executableMove == null)
                {
                    index--;
                    break;
                }
            }

            Execute(true, newInputNode, input, executableMove);
            index++;
        }
        RemoveBufferInputs(index - 1);
    }

    public void ReadHeldInputs()
    {
        if (activeAttackNode != null)
        {
            if (heldInputsWhileAttack.Count == 0) return;

            foreach (InputObject input in heldInputsWhileAttack)
            {
                NormaliseInput(input);
                InputCommand inputCommand = input.GetInputCommand();
                MoveNode newInputNode = movesDatabase.GetNextNode(inputCommand, activeAttackNode);

                if (newInputNode == null) return;

                MoveData executableMove = activeMove;

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
                NormaliseInput(input);
                InputCommand inputCommand = input.GetInputCommand();
                MoveNode newInputNode;
                string inputType = DecideInputType(inputCommand);

                if (prevNeutralInput != null)
                {
                    newInputNode = GetNextNode(inputType, prevNeutralInput.GetInputCommand());
                    newInputNode = GetNextNode(inputType, inputCommand, newInputNode);
                }
                else newInputNode = GetNextNode(inputType, inputCommand);

                if (newInputNode == null) break;

                MoveData executableMove = activeMove;

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
            if (activeAttackNode == null)
            {
                return movesDatabase.GetNextNode(inputCommand, movesDatabase.RootAttackNode);
            }
            return movesDatabase.GetNextNode(inputCommand, activeAttackNode);
        }
        if (activeMovementNode == null)
        {
            return movesDatabase.GetNextNode(inputCommand, movesDatabase.RootMovementNode);
        }
        return movesDatabase.GetNextNode(inputCommand, activeMovementNode);
    }

    public void Execute(bool isAttackNode, MoveNode newNode, InputObject input, MoveData move)
    {
        frameCounter = new FrameCounter();
        activeMove = move;
        activeInput.Add(input);
        prevNeutralInput = null;

        if (isAttackNode) { activeAttackNode = newNode; }
        else { activeMovementNode = newNode; }
    }

    public MoveData GetExecutableMove(IReadOnlyList<MoveData> moves)
    {
        foreach (MoveData move in moves)
        {
            bool canExecute = true;

            if (frameCounter != null && frameCounter.GetFrameNumber() > move.BranchDelay)
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
            if (canExecute) return move;
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

    public void AddHeldInput(InputObject heldInput)
    {
        if (activeAttackNode == null) { heldInputs.Add(heldInput); }
        else { heldInputsWhileAttack.Add(heldInput); }
    }

    public void ClearActiveMove()
    {
        activeAttackNode = null;
        activeMovementNode = null;
        activeMove = null;
        activeInput.Clear();
        frameCounter = null;
    }

    public void UpdateFrame()
    {
        if (frameCounter != null)
        {
            frameCounter.UpdateFrame();
        }
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