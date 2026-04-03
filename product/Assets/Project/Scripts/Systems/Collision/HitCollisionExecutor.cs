using UnityEngine;

public class HitCollisionExecutor : ICollisionExecutor<HitCollisionData>
{
    private CombatExecutor combatExecutor;

    public HitCollisionExecutor(CombatExecutor combatExecutor)
    {
        this.combatExecutor = combatExecutor;
    }

    public void Execute(HitCollisionData data)
    {
        Debug.Log($"[HIT] P{data.Attacker.PlayerId} {data.Hitbox.Id} → " +
                  $"P{data.Defender.PlayerId} {data.Hurtbox.Id}");

        combatExecutor.ProcessHit(data);
    }
}