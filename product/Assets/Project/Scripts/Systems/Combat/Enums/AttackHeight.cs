// Height level of an attack - determines which guard stance can block it
public enum AttackHeight
{
    High,        // Blocked by StandGuarding, whiffs against crouching
    Mid,         // Blocked by StandGuarding
    Low,         // Blocked by CrouchGuarding
    SpecialMid,  // Blocked by either StandGuarding or CrouchGuarding
    Unblockable  // Cannot be blocked
}