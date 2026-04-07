using System.Collections.Generic;
using UnityEngine;

public interface ICollidable
{
    int PlayerId { get; }
    List<CollisionBox> GetActiveHitboxes();
    IEnumerable<CollisionBox> GetAllHurtboxes();
    CollisionBox GetCollisionBox(string id);
    BodyCollider GetBodyCollider();
    Transform GetTransform();
    CombatData GetCombatData();
    CombatHitboxEntry GetActiveHitboxEntry(string hitboxId);
    bool HasState(PlayerStates state);
    void ReceiveCombatResult(CombatResult result);
    string GetCurrentMoveId();
}