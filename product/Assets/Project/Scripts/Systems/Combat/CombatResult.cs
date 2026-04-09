// Immutable result of a combat evaluation - sent to the defender to apply damage and effects
public class CombatResult
{
    public HitOutcome Outcome { get; }
    public int Damage { get; }
    public int StunFrames { get; }
    public float Knockback { get; }   // Not yet applied - TODO
    public HitEffect Effect { get; }

    public CombatResult(HitOutcome outcome, int damage, int stunFrames,
                        float knockback, HitEffect effect)
    {
        Outcome = outcome;
        Damage = damage;
        StunFrames = stunFrames;
        Knockback = knockback;
        Effect = effect;
    }
}