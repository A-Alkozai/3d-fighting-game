using UnityEngine;

public class HitCollisionExecutor : ICollisionExecutor<HitCollisionData>
{
    public void Execute(HitCollisionData data)
    {
        Debug.Log($"[HIT] P{data.Attacker.PlayerId} {data.Hitbox.Id} → " +
                  $"P{data.Defender.PlayerId} {data.Hurtbox.Id}");

        // Future: attackSystem.ProcessHit(data);
    }
}