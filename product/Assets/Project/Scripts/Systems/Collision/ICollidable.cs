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
    void SetMovementBlocked(bool blocked);
}