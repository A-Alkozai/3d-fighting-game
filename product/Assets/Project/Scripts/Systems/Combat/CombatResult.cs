public class CombatResult
{
    public HitOutcome Outcome { get; }
    public int Damage { get; }
    public int StunFrames { get; }
    public float Knockback { get; }
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