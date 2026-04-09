// Data container for a single hit collision event - passed from CollisionManager to CombatExecutor
public class HitCollisionData
{
    public ICollidable Attacker { get; }
    public ICollidable Defender { get; }
    public CollisionBox Hitbox { get; }
    public CollisionBox Hurtbox { get; }
    public CombatHitboxEntry HitboxEntry { get; } // The specific phase entry that caused this hit

    public HitCollisionData(ICollidable attacker, ICollidable defender,
                            CollisionBox hitbox, CollisionBox hurtbox,
                            CombatHitboxEntry hitboxEntry)
    {
        Attacker = attacker;
        Defender = defender;
        Hitbox = hitbox;
        Hurtbox = hurtbox;
        HitboxEntry = hitboxEntry;
    }
}