using System.Collections.Generic;

// Defines per-state rules: which states block entry, which get overridden, and what to transition to next
public struct StateRules
{
    public PlayerStates Blocks;     // Can't enter this state if any of these are active
    public PlayerStates Overrides;  // These states get removed when this state is entered
    public PlayerStates Next;       // Transition to this state when exiting (0 = fall back to prevBaseState)

    // Commonly used state groups for readability
    private static PlayerStates DisableMovement = PlayerStates.Stunned | PlayerStates.Recovery | PlayerStates.Attacking;
    private static PlayerStates InAir = PlayerStates.Airborne | PlayerStates.Falling | PlayerStates.Jumping;
    private static PlayerStates CannotWalk = PlayerStates.Lying | PlayerStates.Crouching | PlayerStates.Dashing
                                                                | PlayerStates.Sidestepping;
    private static PlayerStates AnyGuard = PlayerStates.StandGuarding | PlayerStates.CrouchGuarding;

    private static readonly Dictionary<PlayerStates, StateRules> stateRules = new()
    {
        [PlayerStates.Walking] = new StateRules
        {
            Blocks = DisableMovement | InAir | CannotWalk,
            Overrides = PlayerStates.Idle | PlayerStates.Running | PlayerStates.RunMomentum,
        },
        [PlayerStates.Dashing] = new StateRules
        {
            Blocks = DisableMovement | InAir | PlayerStates.Lying | PlayerStates.Crouching,
            Overrides = PlayerStates.Idle | PlayerStates.Walking | PlayerStates.Running | PlayerStates.RunMomentum
                                        | PlayerStates.Sidestepping | AnyGuard,
        },
        [PlayerStates.Running] = new StateRules
        {
            Blocks = DisableMovement | InAir | CannotWalk,
            Overrides = PlayerStates.Idle | PlayerStates.Walking | AnyGuard,
        },
        [PlayerStates.Jumping] = new StateRules
        {
            Blocks = DisableMovement | InAir | CannotWalk,
            Overrides = PlayerStates.Idle | PlayerStates.Walking | AnyGuard,
        },
        [PlayerStates.Sidestepping] = new StateRules
        {
            Blocks = DisableMovement | InAir | PlayerStates.Lying | PlayerStates.Jumping
                    | PlayerStates.Crouching | PlayerStates.Walking | PlayerStates.Running
                    | PlayerStates.RunMomentum | PlayerStates.Rising,
            Overrides = PlayerStates.Idle | AnyGuard,
        },
        [PlayerStates.Rolling] = new StateRules
        {
            Blocks = DisableMovement | InAir | PlayerStates.Rising,
        },

        [PlayerStates.Crouching] = new StateRules
        {
            Blocks = DisableMovement | InAir | PlayerStates.Lying,
            Overrides = PlayerStates.Idle | PlayerStates.Walking | PlayerStates.Running 
                        | PlayerStates.RunMomentum | PlayerStates.StandGuarding,
            Next = PlayerStates.Rising  // Exiting crouch goes through Rising first
        },
        [PlayerStates.Lying] = new StateRules
        {
            Next = PlayerStates.Rising  // Getting up from the ground
        },
        [PlayerStates.Rising] = new StateRules
        {
            Blocks = DisableMovement | InAir,
            Next = PlayerStates.Idle,   // After rising, return to Idle
        },

        [PlayerStates.Attacking] = new StateRules
        {
            Blocks = PlayerStates.Stunned | PlayerStates.Recovery,
            Overrides = PlayerStates.Idle | PlayerStates.Walking | PlayerStates.Running | PlayerStates.RunMomentum
                                        | PlayerStates.Dashing | PlayerStates.Rising | AnyGuard,
        },

        [PlayerStates.Stunned] = new StateRules
        {
            // Stunned overrides almost everything - player loses control
            Overrides = PlayerStates.Walking | PlayerStates.Running | PlayerStates.RunMomentum | PlayerStates.Sidestepping
                                            | PlayerStates.Dashing | PlayerStates.Rising | PlayerStates.Jumping
                                            | PlayerStates.Rolling | PlayerStates.Recovery | AnyGuard,
        },
    };

    // Look up rules for a state - returns empty rules if the state has no specific rules
    public static StateRules GetStateRule(PlayerStates state)
    {
        if (stateRules.TryGetValue(state, out StateRules rule))
            return rule;
        return new StateRules();
    }
}