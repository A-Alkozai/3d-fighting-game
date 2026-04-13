using System.Collections.Generic;
using UnityEngine;

// AI input provider that commits to full action sequences
// Always doing something: approaching, retreating, sidestepping, crouching, attacking, etc.
public class AIInputProvider : IInputProvider
{
    private Player self;
    private Player opponent;

    // Hold input tracking (mimics LocalInputProvider tap/hold behavior)
    private InputObject currentHeldInput;
    private bool isHolding = false;
    private int holdFrames = 0;
    private int holdThreshold = 10;

    // Action queue system - AI commits to a sequence of actions
    private Queue<AIAction> actionQueue = new Queue<AIAction>();
    private AIAction currentAction;
    private int actionFramesRemaining = 0;

    // Distance thresholds
    private float closeRange = 1.8f;       // Close enough to attack
    private float midRange = 3.0f;         // Medium distance
    private float farRange = 5.0f;         // Far away, needs to close in

    // Weights for behavior selection (higher = more likely)
    private int aggressiveness = 60;       // How often the AI tries to attack vs defend

    public AIInputProvider(Player self, Player opponent)
    {
        this.self = self;
        this.opponent = opponent;
    }

    public List<InputObject> GetInputs()
    {
        var inputs = new List<InputObject>();

        // If we have a current action, execute it
        if (currentAction != null)
        {
            ExecuteCurrentAction(inputs);
            return inputs;
        }

        // If there are queued actions, start the next one
        if (actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            actionFramesRemaining = currentAction.DurationFrames;
            ExecuteCurrentAction(inputs);
            return inputs;
        }

        // No actions queued, decide what to do next
        DecideNextBehavior();

        // Start executing immediately if we queued something
        if (actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            actionFramesRemaining = currentAction.DurationFrames;
            ExecuteCurrentAction(inputs);
        }

        return inputs;
    }

    // Pick a behavior sequence based on distance and opponent state
    private void DecideNextBehavior()
    {
        float distance = Vector3.Distance(self.transform.position, opponent.transform.position);
        bool opponentAttacking = opponent.HasState(PlayerStates.Attacking);
        bool opponentStunned = opponent.HasState(PlayerStates.Stunned);
        int roll = Random.Range(0, 100);

        // Opponent is stunned - rush in and punish
        if (opponentStunned)
        {
            if (distance > closeRange)
            {
                QueueApproach(20);
            }
            QueueAttackCombo();
            return;
        }

        // Close range decisions
        if (distance <= closeRange)
        {
            CloseRangeDecision(roll, opponentAttacking);
            return;
        }

        // Mid range decisions
        if (distance <= midRange)
        {
            MidRangeDecision(roll, opponentAttacking);
            return;
        }

        // Far range decisions
        FarRangeDecision(roll);
    }

    // At close range: attack, block, sidestep, or backstep
    private void CloseRangeDecision(int roll, bool opponentAttacking)
    {
        // If opponent is attacking, favor defensive options
        if (opponentAttacking)
        {
            int defenseRoll = Random.Range(0, 100);
            if (defenseRoll < 40)
            {
                // Stand block (walk backwards briefly)
                QueueBackwalk(Random.Range(20, 50));
            }
            else if (defenseRoll < 65)
            {
                // Crouch block
                QueueCrouch(Random.Range(20, 45));
            }
            else if (defenseRoll < 85)
            {
                // Sidestep to evade
                QueueSidestep();
            }
            else
            {
                // Risky: attack through it
                QueueAttackCombo();
            }
            return;
        }

        // Opponent not attacking - be aggressive
        if (roll < aggressiveness)
        {
            QueueAttackCombo();
        }
        else if (roll < 75)
        {
            // Quick backstep then re-engage
            QueueBackwalk(Random.Range(15, 30));
            QueueApproach(Random.Range(15, 30));
            QueueAttackCombo();
        }
        else if (roll < 85)
        {
            // Sidestep then attack
            QueueSidestep();
            QueueAttackCombo();
        }
        else if (roll < 95)
        {
            // Crouch briefly (ducks highs) then attack
            QueueCrouch(Random.Range(15, 30));
            QueueAttackCombo();
        }
        else
        {
            // Just crouch for a bit
            QueueCrouch(Random.Range(20, 40));
        }
    }

    // At mid range: approach, poke, sidestep, or hold ground
    private void MidRangeDecision(int roll, bool opponentAttacking)
    {
        if (opponentAttacking)
        {
            // Back away or sidestep to whiff punish
            if (Random.Range(0, 100) < 50)
            {
                QueueBackwalk(Random.Range(20, 40));
                // After they whiff, rush in
                QueueApproach(Random.Range(15, 25));
                QueueAttackCombo();
            }
            else
            {
                QueueSidestep();
                QueueAttackCombo();
            }
            return;
        }

        if (roll < 45)
        {
            // Walk in and attack
            QueueApproach(Random.Range(25, 50));
            QueueAttackCombo();
        }
        else if (roll < 60)
        {
            // Dash in aggressively (forward tap then hold for run)
            QueueRun(Random.Range(20, 40));
            QueueAttackCombo();
        }
        else if (roll < 75)
        {
            // Sidestep then approach
            QueueSidestep();
            QueueApproach(Random.Range(20, 35));
        }
        else if (roll < 85)
        {
            // Cautious: walk back a bit
            QueueBackwalk(Random.Range(15, 30));
        }
        else if (roll < 92)
        {
            // Crouch at range (mind game)
            QueueCrouch(Random.Range(20, 40));
            QueueApproach(Random.Range(20, 30));
        }
        else
        {
            // Just walk forward
            QueueApproach(Random.Range(30, 60));
        }
    }

    // At far range: close the distance
    private void FarRangeDecision(int roll)
    {
        if (roll < 40)
        {
            // Run in
            QueueRun(Random.Range(30, 60));
            QueueAttackCombo();
        }
        else if (roll < 70)
        {
            // Walk forward
            QueueApproach(Random.Range(40, 80));
        }
        else if (roll < 85)
        {
            // Sidestep then approach (angle the approach)
            QueueSidestep();
            QueueApproach(Random.Range(30, 50));
        }
        else
        {
            // Walk forward then crouch (feint)
            QueueApproach(Random.Range(20, 40));
            QueueCrouch(Random.Range(15, 25));
            QueueApproach(Random.Range(20, 40));
        }
    }

    // --- Action Queueing Methods ---

    // Walk towards opponent
    private void QueueApproach(int frames)
    {
        actionQueue.Enqueue(new AIAction(AIActionType.WalkForward, frames));
    }

    // Walk away from opponent (also blocks)
    private void QueueBackwalk(int frames)
    {
        actionQueue.Enqueue(new AIAction(AIActionType.WalkBackward, frames));
    }

    // Crouch (also crouch blocks)
    private void QueueCrouch(int frames)
    {
        actionQueue.Enqueue(new AIAction(AIActionType.Crouch, frames));
    }

    // Sidestep (randomly pick up or down)
    private void QueueSidestep()
    {
        bool away = Random.Range(0, 100) < 50;
        actionQueue.Enqueue(new AIAction(
            away ? AIActionType.SidestepAway : AIActionType.SidestepTowards, 30));
    }

    // Forward tap then hold to trigger run
    private void QueueRun(int frames)
    {
        // Tap forward first (neutral input for the run sequence)
        actionQueue.Enqueue(new AIAction(AIActionType.TapForward, 1));
        // Small gap so the tap registers before the hold
        actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 2));
        // Then hold forward to run
        actionQueue.Enqueue(new AIAction(AIActionType.WalkForward, frames));
    }

    // Pick a random attack or combo sequence
    private void QueueAttackCombo()
    {
        int roll = Random.Range(0, 100);

        if (roll < 15)
        {
            // Single jab
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
        }
        else if (roll < 30)
        {
            // Jab into elbow (LeftPunch, LeftPunch with branchDelay)
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 3));
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
        }
        else if (roll < 42)
        {
            // Right punch into elbow uppercut (RightPunch, LeftPunch)
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.RightPunch));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 3));
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
        }
        else if (roll < 52)
        {
            // Punch chain (LP, RP, LP, RP)
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 3));
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.RightPunch));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 3));
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 3));
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.RightPunch));
        }
        else if (roll < 62)
        {
            // Left kick
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftKick));
        }
        else if (roll < 72)
        {
            // Right kick
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.RightKick));
        }
        else if (roll < 82)
        {
            // Double kick (LK, RK)
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftKick));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 3));
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.RightKick));
        }
        else if (roll < 90)
        {
            // Right punch solo
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.RightPunch));
        }
        else
        {
            // Jab then wait (single poke, safe)
            actionQueue.Enqueue(new AIAction(AIActionType.Attack, 5, InputCommand.LeftPunch));
            actionQueue.Enqueue(new AIAction(AIActionType.Nothing, 15));
        }

        // Small recovery window after any attack so the AI doesn't chain behaviors instantly
        actionQueue.Enqueue(new AIAction(AIActionType.Nothing, Random.Range(8, 20)));
    }

    // --- Action Execution ---

    // Process the current action for this frame
    private void ExecuteCurrentAction(List<InputObject> inputs)
    {
        actionFramesRemaining--;

        switch (currentAction.Type)
        {
            case AIActionType.WalkForward:
                HandleHoldAction(InputCommand.Forward, inputs);
                break;

            case AIActionType.WalkBackward:
                HandleHoldAction(InputCommand.Backward, inputs);
                break;

            case AIActionType.Crouch:
                HandleHoldAction(InputCommand.Down, inputs);
                break;

            case AIActionType.SidestepAway:
                HandleSidestep(InputCommand.Up, inputs);
                break;

            case AIActionType.SidestepTowards:
                HandleSidestep(InputCommand.Down, inputs);
                break;

            case AIActionType.TapForward:
                // Single tap, no hold logic needed
                ReleaseHeldInput(inputs);
                InputCommand rawFwd = ConvertToRawDirection(InputCommand.Forward);
                inputs.Add(new InputObject(rawFwd, UnityEngine.InputSystem.Key.None));
                break;

            case AIActionType.Attack:
                // Release any held input before attacking
                ReleaseHeldInput(inputs);
                if (actionFramesRemaining == currentAction.DurationFrames - 1)
                {
                    // Only send the attack input on the first frame of this action
                    inputs.Add(new InputObject(currentAction.AttackCommand, UnityEngine.InputSystem.Key.None));
                }
                break;

            case AIActionType.Nothing:
                // Release any held direction so we return to idle
                ReleaseHeldInput(inputs);
                break;
        }

        // Action finished, clean up
        if (actionFramesRemaining <= 0)
        {
            // If this was a hold action, release the held input
            if (currentAction.Type == AIActionType.WalkForward ||
                currentAction.Type == AIActionType.WalkBackward ||
                currentAction.Type == AIActionType.Crouch)
            {
                ReleaseHeldInput(inputs);
            }
            currentAction = null;

            // Re-evaluate mid-sequence if opponent state changed drastically
            if (ShouldInterrupt())
            {
                actionQueue.Clear();
            }
        }
    }

    // Handle directional hold actions (walk forward, walk backward, crouch)
    // Mimics the LocalInputProvider tap -> hold threshold -> hold pattern
    private void HandleHoldAction(InputCommand direction, List<InputObject> inputs)
    {
        InputCommand rawDirection = ConvertToRawDirection(direction);

        // First frame of this hold action - start the hold
        if (!isHolding || currentHeldInput == null)
        {
            // Release any previous held input from a different direction
            ReleaseHeldInput(inputs);
            StartHoldInput(rawDirection, inputs);
            return;
        }

        // Already holding - check if it's the same direction
        InputCommand currentRaw = ConvertToRawDirection(direction);
        InputCommand heldCommand = currentHeldInput.GetInputCommand();

        // Strip "Hold" suffix for comparison
        string heldBase = heldCommand.ToString().Replace("Hold", "");
        string targetBase = currentRaw.ToString().Replace("Hold", "");

        if (heldBase != targetBase)
        {
            // Different direction - release old, start new
            ReleaseHeldInput(inputs);
            StartHoldInput(rawDirection, inputs);
            return;
        }

        // Same direction - keep holding
        holdFrames++;
        UpdateHoldState(inputs);
    }

    // Sidesteps are a tap (not a hold), so just send a single tap input
    private void HandleSidestep(InputCommand direction, List<InputObject> inputs)
    {
        // Only send on the first frame
        if (actionFramesRemaining == currentAction.DurationFrames - 1)
        {
            ReleaseHeldInput(inputs);
            InputCommand rawDir = ConvertToRawDirection(direction);
            inputs.Add(new InputObject(rawDir, UnityEngine.InputSystem.Key.None));
        }
    }

    // Check if we should cancel remaining queued actions (opponent did something unexpected)
    private bool ShouldInterrupt()
    {
        // Interrupt if opponent just started attacking and we're doing something aggressive
        bool opponentAttacking = opponent.HasState(PlayerStates.Attacking);
        bool opponentStunned = opponent.HasState(PlayerStates.Stunned);

        // If opponent is stunned, drop defensive plans and go in
        if (opponentStunned && actionQueue.Count > 0)
        {
            return true;
        }

        // Random chance to re-evaluate (keeps the AI less predictable)
        if (Random.Range(0, 100) < 15)
        {
            return true;
        }

        return false;
    }

    // --- Hold Input Management (mirrors LocalInputProvider) ---

    // Start holding a directional input
    private void StartHoldInput(InputCommand rawCommand, List<InputObject> inputs)
    {
        InputObject newInput = new InputObject(rawCommand, UnityEngine.InputSystem.Key.None, true);
        currentHeldInput = newInput;
        isHolding = true;
        holdFrames = 1;
        inputs.Add(newInput);
    }

    // Update hold state each frame (handles tap to hold transition at threshold)
    private void UpdateHoldState(List<InputObject> inputs)
    {
        if (currentHeldInput == null) return;

        if (holdFrames == holdThreshold)
        {
            // Convert tap to hold variant
            InputCommand command = currentHeldInput.GetInputCommand();
            string holdName = command.ToString() + "Hold";

            if (System.Enum.TryParse<InputCommand>(holdName, out InputCommand holdCommand))
            {
                currentHeldInput.ChangeInputCommand(holdCommand);
                currentHeldInput.GetFrame().ResetFrame();
                currentHeldInput.SetIsHeld(true);
                inputs.Add(currentHeldInput);
            }
        }
        else if (holdFrames > holdThreshold)
        {
            currentHeldInput.GetFrame().UpdateFrame();
        }
    }

    // Release a held directional input
    private void ReleaseHeldInput(List<InputObject> inputs)
    {
        if (!isHolding || currentHeldInput == null) return;

        if (holdFrames < holdThreshold)
        {
            currentHeldInput.SetIsHeld(false);
        }
        else
        {
            currentHeldInput.GetFrame().DisableFrame();
            inputs.Add(currentHeldInput);
        }

        currentHeldInput = null;
        isHolding = false;
        holdFrames = 0;
    }

    // Convert normalised Forward/Backward/Up/Down to raw Left/Right/Up/Down
    // LocalInputProvider sends raw keys, MoveSelector normalises them
    private InputCommand ConvertToRawDirection(InputCommand command)
    {
        FacingDirection facing = GetFacingFromPosition();

        if (facing == FacingDirection.Right)
        {
            if (command == InputCommand.Forward) return InputCommand.Right;
            if (command == InputCommand.Backward) return InputCommand.Left;
        }
        else
        {
            if (command == InputCommand.Forward) return InputCommand.Left;
            if (command == InputCommand.Backward) return InputCommand.Right;
        }

        // Up and Down don't change based on facing
        return command;
    }

    // Determine facing direction from relative positions
    private FacingDirection GetFacingFromPosition()
    {
        float diff = opponent.transform.position.x - self.transform.position.x;
        return diff >= 0 ? FacingDirection.Right : FacingDirection.Left;
    }
}

// Represents a single AI action with a type, duration, and optional attack command
public class AIAction
{
    public AIActionType Type;
    public int DurationFrames;
    public InputCommand AttackCommand;

    public AIAction(AIActionType type, int durationFrames, InputCommand attackCommand = InputCommand.LeftPunch)
    {
        Type = type;
        DurationFrames = durationFrames;
        AttackCommand = attackCommand;
    }
}

// All possible AI action types
public enum AIActionType
{
    Nothing,           // Idle, release all inputs
    WalkForward,       // Hold forward
    WalkBackward,      // Hold backward (also stand blocks)
    Crouch,            // Hold down (also crouch blocks)
    SidestepAway,      // Tap up
    SidestepTowards,   // Tap down
    TapForward,        // Single forward tap (used for run startup)
    Attack             // Press an attack button
}