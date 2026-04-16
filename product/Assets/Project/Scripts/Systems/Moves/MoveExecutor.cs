using System.Collections.Generic;
using UnityEngine;

// Executes the current move: manages frame counting, animation playback, state transitions, and move switching
public class MoveExecutor
{
    private StateManager stateManager;
    private AnimationManager animationManager;
    private MovementManager movementManager;

    private bool isStateMove = false;        // True if the current move is a fallback (idle, crouching, etc.)
    private MoveData currentMove;
    private MoveNode activeAttackNode;       // Current position in the attack tree
    private MoveNode activeMovementNode;     // Current position in the movement tree
    private InputObject loopInput;           // The held input keeping a looping move alive
    private InputObject prevNeutralInput;    // Last neutral (directional) input before an attack
    private FrameCounter frameCounter;       // Tracks which frame of the current move we're on

    public bool IsStateMove => isStateMove;
    public MoveData CurrentMove => currentMove;
    public MoveNode ActiveAttackNode => activeAttackNode;
    public MoveNode ActiveMovementNode => activeMovementNode;
    public InputObject PrevNeutralInput => prevNeutralInput;
    public FrameCounter FrameCounter => frameCounter;

    public MoveExecutor(StateManager stateManager, AnimationManager animationManager,
                        MovementManager movementManager)
    {
        this.stateManager = stateManager;
        this.animationManager = animationManager;
        this.movementManager = movementManager;
    }

    // Called each logic frame - advance the current move and tick the frame counter
    public void Update()
    {
        RunCurrentMove();
        UpdateFrame();
    }

    // Handle move playback: start animation on frame 0, end non-loop moves when done, restart loop moves
    private void RunCurrentMove()
    {
        if (currentMove == null)
            return;

        int frameNumber = frameCounter.GetFrameNumber();

        // Frame 0: trigger the animation and movement pattern
        if (frameNumber == 0)
        {
            animationManager.PlayAnimation(currentMove.Id);
            movementManager.SetMovement(currentMove.Id);
        }

        // Non-looping move: cancel when total frames exceeded
        if (!currentMove.IsLoop && frameNumber >= currentMove.TotalFrames)
        {
            CancelMove();
        }
        // Looping move: restart on completion, or cancel if the held input was released
        else if (currentMove.IsLoop)
        {
            if (loopInput != null && loopInput.GetFrame().GetFrameNumber() == -1)
            {
                // Held input was released - stop looping
                CancelMove();
                loopInput = null;
            }
            else if (frameNumber >= currentMove.TotalFrames)
            {
                // Loop: reset frame counter to replay from the start
                frameCounter = new FrameCounter();
            }
        }
    }

    // Switch from one move to another (deactivates old states, activates new ones)
    private void ChangeMove(MoveData newMove)
    {
        DeactivateStates(currentMove.RequiredStates);

        currentMove = newMove;
        frameCounter = new FrameCounter();
        prevNeutralInput = null;

        ActivateStates(currentMove.RequiredStates);
        Debug.Log($"Updated Move: {newMove.Id}");
    }

    // Begin a move from scratch (no previous move to deactivate)
    private void StartMove(MoveData newMove)
    {
        currentMove = newMove;
        frameCounter = new FrameCounter();
        prevNeutralInput = null;
        ActivateStates(currentMove.RequiredStates);
        Debug.Log($"Starting Move: {newMove.Id}");
    }

    // Enter all states required by a move (e.g. Attacking, StandGuarding)
    private void ActivateStates(IReadOnlyList<PlayerStates> states)
    {
        foreach (PlayerStates state in states)
        {
            Debug.Log($"[MoveExecutor] Activating state: {state}");
            stateManager.EnterState(state);
        }
    }

    // Exit all states that were required by the ending move
    private void DeactivateStates(IReadOnlyList<PlayerStates> states)
    {
        foreach (PlayerStates state in states)
        {
            stateManager.ExitState(state);
        }
    }

    // Called by MoveSelector when a new move is chosen from the input tree
    // Handles starting vs changing moves, and tracks which tree node we're on
    public void SetCurrentMove(MoveData newMove, MoveNode newNode, bool isAttackNode)
    {
        if (isStateMove) isStateMove = false;
        
        if (currentMove == null)
        {
            StartMove(newMove);
        }
        else if (currentMove.Id != newMove.Id)
        {
            ChangeMove(newMove);
        }

        // Track position in the appropriate tree (attack or movement)
        // Clear the other tree position - can't be in both at once
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

    // Set a fallback move (idle, crouching, etc.) - skips if already playing the same move
    public void SetFallback(MoveData newMove)
    {
        if (newMove == null) return;
        if (currentMove != null && currentMove.Id == newMove.Id) return;
        StartMove(newMove);
        isStateMove = true;
    }

    public void SetPrevNeutralInput(InputObject input)
    {
        prevNeutralInput = input;
    }

    // Store the held input that keeps a looping move alive (released = move cancels)
    public void SetLoopInput(InputObject input)
    {
        loopInput = input;
    }

    // Fully cancel the current move: deactivate states, stop movement, clear all tracking
    public void CancelMove()
    {
        DeactivateStates(currentMove.RequiredStates);
        movementManager.CancelMovement();
        activeAttackNode = null;
        activeMovementNode = null;
        currentMove = null;
        frameCounter = null;
        prevNeutralInput = null;
    }

    // Tick the frame counter forward by one (called after RunCurrentMove)
    private void UpdateFrame()
    {
        if (frameCounter != null)
        {
            frameCounter.UpdateFrame();
        }
    }
}