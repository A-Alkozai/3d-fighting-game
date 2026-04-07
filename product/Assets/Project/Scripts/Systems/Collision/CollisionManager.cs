using System.Collections.Generic;
using UnityEngine;

public class CollisionManager
{
    private ICollidable player1;
    private ICollidable player2;

    private HitCollisionExecutor hitCollisionExecutor;
    private PushCollisionExecutor pushCollisionExecutor;
    private StageCollision stageCollision;

    private HashSet<string> activeHits = new HashSet<string>();

    public CollisionManager(ICollidable player1, ICollidable player2,
                            CombatExecutor combatExecutor, StageCollision stageCollision)
    {
        this.player1 = player1;
        this.player2 = player2;
        this.hitCollisionExecutor = new HitCollisionExecutor(combatExecutor);
        this.stageCollision = stageCollision;
        this.pushCollisionExecutor = new PushCollisionExecutor(stageCollision);
    }

    public void Update()
    {
        CheckHits(player1, player2);
        CheckHits(player2, player1);
        ResolvePush();
        ResolveStage();
        CleanupExpiredHits();
    }

    private void CheckHits(ICollidable attacker, ICollidable defender)
    {
        List<CollisionBox> hitboxes = attacker.GetActiveHitboxes();
        if (hitboxes.Count == 0) return;

        string moveId = attacker.GetCurrentMoveId();

        foreach (CollisionBox hitbox in hitboxes)
        {
            CombatHitboxEntry entry = attacker.GetActiveHitboxEntry(hitbox.Id);
            if (entry == null) continue;

            string hitId = $"{attacker.PlayerId}_{moveId}_{entry.StartFrame}_{defender.PlayerId}";

            if (activeHits.Contains(hitId)) continue;

            foreach (CollisionBox hurtbox in defender.GetAllHurtboxes())
            {
                if (hitbox.GetHitboxBounds().Intersects(hurtbox.GetHurtboxBounds()))
                {
                    HitCollisionData data = new HitCollisionData(attacker, defender, hitbox, hurtbox, entry);
                    activeHits.Add(hitId);
                    hitCollisionExecutor.Execute(data);
                    return;
                }
            }
        }
    }

    private void ResolvePush()
    {
        pushCollisionExecutor.Execute(player1, player2);
    }

    private void ResolveStage()
    {
        stageCollision.ResolvePlayer(
            player1.GetTransform(), 
            player1.GetBodyCollider().GetBounds()
        );
        stageCollision.ResolvePlayer(
            player2.GetTransform(), 
            player2.GetBodyCollider().GetBounds()
        );
    }

    private void CleanupExpiredHits()
    {
        activeHits.RemoveWhere(hitId =>
        {
            string[] parts = hitId.Split('_');
            string attackerId = parts[0];
            string moveId = parts[1];

            ICollidable attacker = attackerId == player1.PlayerId.ToString() ? player1 : player2;
            string currentMoveId = attacker.GetCurrentMoveId();

            return currentMoveId == null || currentMoveId != moveId;
        });
    }
}