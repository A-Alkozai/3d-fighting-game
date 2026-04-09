// The result of evaluating a hit against the defender's state
public enum HitOutcome
{
    NormalHit,   // Clean hit
    CounterHit,  // Hit while defender was attacking (bonus damage/stun)
    Blocked,     // Defender was guarding correctly
    Whiff        // Attack missed (e.g. high attack vs crouching defender)
}