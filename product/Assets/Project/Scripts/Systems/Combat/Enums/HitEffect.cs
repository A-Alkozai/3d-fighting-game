// What happens to the defender on hit - used to trigger the correct reaction
public enum HitEffect
{
    None,
    Hitstun,    // Brief stagger, recoverable
    Knockdown,  // Sends defender to the ground
    Launch,     // Pops defender into the air
    Screw,      // Aerial spin (combo extender)
    WallSplat   // Slams defender against a wall
}