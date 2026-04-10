using UnityEngine;

// Manages the player's state bitmask - handles entering/exiting states with override and fallback logic
public class StateManager
{
    private PlayerStates playerStates = new PlayerStates();
    private FacingDirection facingDirection = FacingDirection.Right;
    private PlayerStates prevBaseState = PlayerStates.Idle; // Remembered so we can fall back after transient states

    // Check if a state can be activated - blocked if KO/Stunned or if any blocking states are active
    public bool CanToggleState(PlayerStates state)
    {
        if (HasState(PlayerStates.KO | PlayerStates.Stunned))
        { return false; }

        PlayerStates blocks = StateRules.GetStateRule(state).Blocks;

        if (blocks == 0) { return true; }

        if (HasState(blocks)) { return false; }

        return true;
    }

    // Add a state and remove any states it overrides (e.g. Attacking overrides Idle + guard states)
    public void EnterState(PlayerStates state)
    {
        if (state == PlayerStates.Grounded) return; // Grounded is not managed through state rules
        PlayerStates overrides = StateRules.GetStateRule(state).Overrides;
        AddState(state);
        RemoveState(overrides);
    }

    // Remove a state and transition to its Next state, or fall back to prevBaseState if no Next defined
    public void ExitState(PlayerStates state)
    {
        if (state == PlayerStates.Grounded) return;
        PlayerStates nextState = StateRules.GetStateRule(state).Next;
        RemoveState(state);
        if (nextState != 0)
        {
            AddState(nextState);
        }
        else
        {
            // Always fall back to previous base state (Idle or Lying)
            AddState(prevBaseState);
        }
    }

    private bool IsBaseState(PlayerStates state)
    {
        return state == PlayerStates.Idle 
            || state == PlayerStates.Crouching 
            || state == PlayerStates.Lying;
    }

    // Add a state to the bitmask - only Idle and Lying update the remembered base state
    // (Crouching deliberately does NOT update prevBaseState - bug fix)
    public void AddState(PlayerStates state)
    {
        playerStates |= state;
        if (state == PlayerStates.Idle || state == PlayerStates.Lying)
        {
            prevBaseState = state;
        }
    }

    // Remove a state from the bitmask
    public void RemoveState(PlayerStates state)
    {
        playerStates &= ~state;
    }

    // Check if any of the given state flags are active
    public bool HasState(PlayerStates state)
    {
        return (playerStates & state) != 0;
    }

    // Clear all states (used on KO, stun entry, round reset)
    public void ResetState()
    {
        playerStates = 0;
    }

    public FacingDirection GetFacingDirection()
    {
        return facingDirection;
    }

    public void SetFacingDirection(FacingDirection direction)
    {
        facingDirection = direction;
    }
}