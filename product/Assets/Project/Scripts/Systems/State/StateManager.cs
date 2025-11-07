using UnityEngine;

public class StateManager
{

    private PlayerStates playerStates = new PlayerStates();
    private FacingDirection facingDirection = new FacingDirection();
    private PlayerStates prevBaseState = PlayerStates.Idle;

    public bool CanToggleState(PlayerStates state)
    {
        if (HasState(PlayerStates.KO | PlayerStates.Stunned))
        { return false; }

        PlayerStates blocks = StateRules.GetStateRule(state).Blocks;
        
        if (blocks == 0) { return true; }

        if (HasState(blocks)) { return false; }

        return true;
    }

    public void EnterState(PlayerStates state)
    {
        PlayerStates overrides = StateRules.GetStateRule(state).Overrides;

        AddState(state);
        Debug.Log("Entered State: " + state);
        RemoveState(overrides);
    }

    public void ExitState(PlayerStates state)
    {
        PlayerStates nextState = StateRules.GetStateRule(state).Next;

        RemoveState(state);
        Debug.Log("Exited State: " + state);
        if (nextState != 0)
        {
            AddState(nextState);
        }
        else
        {
            AddState(prevBaseState);
        }
    }

    public void AddState(PlayerStates state)
    {
        playerStates |= state;
        Debug.Log("Added State: " + state);
        if (state == PlayerStates.Idle || state == PlayerStates.Lying || state == PlayerStates.Crouching)
        {
            prevBaseState = state;
        }
    }

    public void RemoveState(PlayerStates state)
    {
        playerStates &= ~state;
        Debug.Log("Removed State: " + state);
    }

    public bool HasState(PlayerStates state)
    {
        return (playerStates & state) != 0;
    }

    public void ResetState()
    {
        playerStates = 0;
    }
}