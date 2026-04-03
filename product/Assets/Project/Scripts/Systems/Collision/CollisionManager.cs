using System.Collections.Generic;
using UnityEngine;

public class CollisionManager
{
    private ICollidable player1;
    private ICollidable player2;

    private HitCollisionExecutor hitCollisionExecutor;
    private PushCollisionExecutor pushCollisionExecutor;

    private HashSet<string> activeHits = new HashSet<string>();

    public CollisionManager(ICollidable player1, ICollidable player2,
                            CombatExecutor combatExecutor)
    {
        this.player1 = player1;
        this.player2 = player2;
        this.hitCollisionExecutor = new HitCollisionExecutor(combatExecutor);
        this.pushCollisionExecutor = new PushCollisionExecutor();
    }

    public void Update()
    {
        CheckHits(player1, player2);
        CheckHits(player2, player1);
        CheckPush(player1, player2);
        CleanupExpiredHits();
    }

    private void CheckHits(ICollidable attacker, ICollidable defender)
    {
        List<CollisionBox> hitboxes = attacker.GetActiveHitboxes();
        if (hitboxes.Count == 0) return;

        // One hit per move per defender
        string moveId = attacker.GetCurrentMoveId();
        string hitId = $"{attacker.PlayerId}_{moveId}_{defender.PlayerId}";
        if (activeHits.Contains(hitId)) return;

        IEnumerable<CollisionBox> hurtboxes = defender.GetAllHurtboxes();

        foreach (CollisionBox hitbox in hitboxes)
        {
            foreach (CollisionBox hurtbox in hurtboxes)
            {
                if (!hitbox.GetHitboxBounds().Intersects(hurtbox.GetHurtboxBounds()))
                    continue;

                activeHits.Add(hitId);

                HitCollisionData data = new HitCollisionData(
                    attacker,
                    defender,
                    hitbox,
                    hurtbox
                );

                hitCollisionExecutor.Execute(data);
                return; // one hit registered for this entire move
            }
        }
    }

    private void CheckPush(ICollidable entityA, ICollidable entityB)
    {
        Bounds boundsA = entityA.GetBodyCollider().GetBounds();
        Bounds boundsB = entityB.GetBodyCollider().GetBounds();

        bool overlapping = boundsA.Intersects(boundsB);

        pushCollisionExecutor.Execute(entityA, entityB, overlapping);
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