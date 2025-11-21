using System.Collections.Generic;
using UnityEngine;

public class MoveExecutor
{
    private StateManager stateManager;

    private MoveData currentMove;
    private MoveNode activeAttackNode;
    private MoveNode activeMovementNode;
    private InputObject loopInput;
    private InputObject prevNeutralInput;
    private FrameCounter frameCounter;

    public MoveData CurrentMove => currentMove;
    public MoveNode ActiveAttackNode => activeAttackNode;
    public MoveNode ActiveMovementNode => activeMovementNode;
    public InputObject PrevNeutralInput => prevNeutralInput;
    public FrameCounter FrameCounter => frameCounter;

    public MoveExecutor(StateManager stateManager)
    {
        this.stateManager = stateManager;
    }

    public void Update()
    {
        RunCurrentMove();
        UpdateFrame();
    }

    private void RunCurrentMove()
    {
        if (currentMove == null)
        {
            // Get a state-driven move;
            return;
        }

        int frameNumber = frameCounter.GetFrameNumber();

        if (!currentMove.IsLoop && frameNumber >= currentMove.TotalFrames)
        {
            CancelMove();
        }
        else if (currentMove.IsLoop)
        {
            if (loopInput != null && loopInput.GetFrame().GetFrameNumber() == -1)
            {
                CancelMove();
                loopInput = null;
            }
            else if (frameNumber >= currentMove.TotalFrames)
            {
                frameCounter = new FrameCounter();
            }
        }
    }

    private void ChangeMove(MoveData newMove)
    {
        // Update all other move related data here
        DeactivateStates(currentMove.RequiredStates);

        currentMove = newMove;
        frameCounter = new FrameCounter();
        prevNeutralInput = null;

        ActivateStates(currentMove.RequiredStates);
        Debug.Log($"Updated Move: {newMove.Id}");
    }

    private void StartMove(MoveData newMove)
    {
        currentMove = newMove;
        frameCounter = new FrameCounter();
        prevNeutralInput = null;
        ActivateStates(currentMove.RequiredStates);
        Debug.Log($"Starting Move: {newMove.Id}");
        // Activate other move related objects
    }

    private void ActivateStates(IReadOnlyList<PlayerStates> states)
    {
        foreach (PlayerStates state in states)
        {
            stateManager.EnterState(state);
        }
    }

    private void DeactivateStates(IReadOnlyList<PlayerStates> states)
    {
        foreach (PlayerStates state in states)
        {
            stateManager.ExitState(state);
        }
    }

    public void SetCurrentMove(MoveData newMove, MoveNode newNode, bool isAttackNode)
    {
        if (currentMove == null)
        {
            StartMove(newMove);
        }
        else if (currentMove.Id != newMove.Id)
        {
            ChangeMove(newMove);
        }
        if (isAttackNode)
        {
            activeAttackNode = newNode;
            activeMovementNode = null;
        }
        else
        {
            activeMovementNode = newNode;
            activeAttackNode = null;
        }
    }

    public void SetPrevNeutralInput(InputObject input)
    {
        prevNeutralInput = input;
    }

    public void SetLoopInput(InputObject input)
    {
        loopInput = input;
    }

    public void CancelMove()
    {
        DeactivateStates(currentMove.RequiredStates);
        activeAttackNode = null;
        activeMovementNode = null;
        currentMove = null;
        frameCounter = null;
        prevNeutralInput = null;
        Debug.Log("Idle");
    }

    private void UpdateFrame()
    {
        if (frameCounter != null)
        {
            frameCounter.UpdateFrame();
        }
    }
}