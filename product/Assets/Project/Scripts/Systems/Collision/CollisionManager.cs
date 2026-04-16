using System.Collections.Generic;
using UnityEngine;

// Orchestrates all collision checks each frame: hit detection, player push, and stage bounds
public class CollisionManager
{
    private ICollidable player1;
    private ICollidable player2;

    private HitCollisionExecutor hitCollisionExecutor;
    private PushCollisionExecutor pushCollisionExecutor;
    private StageCollision stageCollision;

    // Tracks which hits have already connected to prevent the same phase hitting twice
    // Key format: "{attackerId}_{moveId}_{startFrame}_{defenderId}"
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

    // Run all collision checks in order: hits, push apart, stage walls, cleanup
    public void Update()
    {
        CheckHits(player1, player2);
        CheckHits(player2, player1);
        ResolvePush();
        ResolveStage();
        CleanupExpiredHits();
    }

    // Check if any of the attacker's active hitboxes overlap the defender's hurtboxes
    private void CheckHits(ICollidable attacker, ICollidable defender)
    {
        List<CollisionBox> hitboxes = attacker.GetActiveHitboxes();
        if (hitboxes.Count == 0) return;

        string moveId = attacker.GetCurrentMoveId();

        foreach (CollisionBox hitbox in hitboxes)
        {
            CombatHitboxEntry entry = attacker.GetActiveHitboxEntry(hitbox.Id);
            if (entry == null) continue;

            // Build a unique ID for this hit phase so it only registers once
            string hitId = $"{attacker.PlayerId}_{moveId}_{entry.StartFrame}_{defender.PlayerId}";

            // Skip if this phase already hit this defender
            if (activeHits.Contains(hitId)) continue;

            foreach (CollisionBox hurtbox in defender.GetAllHurtboxes())
            {
                if (hitbox.GetHitboxBounds().Intersects(hurtbox.GetHurtboxBounds()))
                {
                    HitCollisionData data = new HitCollisionData(attacker, defender, hitbox, hurtbox, entry);
                    activeHits.Add(hitId);
                    hitCollisionExecutor.Execute(data);
                    return; // One hit per attacker per frame is enough
                }
            }
        }
    }

    // Push players apart if their body colliders overlap (skip if either is KO'd)
    private void ResolvePush()
    {
        if (player1.HasState(PlayerStates.KO) || player2.HasState(PlayerStates.KO))
            return;

        pushCollisionExecutor.Execute(player1, player2);
    }

    // Keep players inside the stage walls (skip KO'd players so they stay where they fell)
    private void ResolveStage()
    {
        if (!player1.HasState(PlayerStates.KO))
        {
            stageCollision.ResolvePlayer(player1.GetTransform(),
                                        player1.GetBodyCollider().GetBounds());
        }

        if (!player2.HasState(PlayerStates.KO))
        {
            stageCollision.ResolvePlayer(player2.GetTransform(),
                                        player2.GetBodyCollider().GetBounds());
        }
    }

    // Remove hit tracking entries for moves that are no longer active
    private void CleanupExpiredHits()
    {
        activeHits.RemoveWhere(hitId =>
        {
            string[] parts = hitId.Split('_');
            string attackerId = parts[0];
            string moveId = parts[1];

            ICollidable attacker = attackerId == player1.PlayerId.ToString() ? player1 : player2;
            string currentMoveId = attacker.GetCurrentMoveId();

            // Remove if the attacker is no longer performing this move
            return currentMoveId == null || currentMoveId != moveId;
        });
    }
}