using System.Collections.Generic;

public struct StateRules
{
    public PlayerStates Blocks;
    public PlayerStates Overrides;
    public PlayerStates Next;

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
            Next = PlayerStates.Falling
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
            Next = PlayerStates.Rising
        },
        [PlayerStates.Lying] = new StateRules
        {
            Next = PlayerStates.Rising
        },
        [PlayerStates.Rising] = new StateRules
        {
            Blocks = DisableMovement | InAir,
            Next = PlayerStates.Idle,
        },

        [PlayerStates.Attacking] = new StateRules
        {
            Blocks = PlayerStates.Stunned | PlayerStates.Recovery,
            Overrides = PlayerStates.Idle | PlayerStates.Walking | PlayerStates.Running | PlayerStates.RunMomentum
                                        | PlayerStates.Dashing | PlayerStates.Rising | AnyGuard,
        },

        [PlayerStates.Stunned] = new StateRules
        {
            Overrides = PlayerStates.Walking | PlayerStates.Running | PlayerStates.RunMomentum | PlayerStates.Sidestepping
                                            | PlayerStates.Dashing | PlayerStates.Rising | PlayerStates.Jumping
                                            | PlayerStates.Rolling | PlayerStates.Recovery | AnyGuard,
        },
    };

    public static StateRules GetStateRule(PlayerStates state)
    {
        if (stateRules.TryGetValue(state, out StateRules rule))
            return rule;
        return new StateRules();
    }
}