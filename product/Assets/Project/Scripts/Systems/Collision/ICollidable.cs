using System.Collections.Generic;
using UnityEngine;

// Interface that both players implement so the collision system can query them generically
public interface ICollidable
{
    int PlayerId { get; }
    List<CollisionBox> GetActiveHitboxes();          // Currently attacking hitboxes
    IEnumerable<CollisionBox> GetAllHurtboxes();     // All hurtboxes (always active)
    CollisionBox GetCollisionBox(string id);
    BodyCollider GetBodyCollider();                   // For push/stage collision
    Transform GetTransform();
    CombatData GetCombatData();                       // Combat data for the current move
    CombatHitboxEntry GetActiveHitboxEntry(string hitboxId); // Phase entry for a specific hitbox
    bool HasState(PlayerStates state);
    void ReceiveCombatResult(CombatResult result);    // Apply damage/stun from a hit
    string GetCurrentMoveId();
}