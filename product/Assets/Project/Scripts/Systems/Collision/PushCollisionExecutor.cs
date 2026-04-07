using UnityEngine;

public class PushCollisionExecutor
{
    private StageCollision stageCollision;

    public PushCollisionExecutor(StageCollision stageCollision)
    {
        this.stageCollision = stageCollision;
    }

    public void Execute(ICollidable entityA, ICollidable entityB)
    {
        Bounds boundsA = entityA.GetBodyCollider().GetBounds();
        Bounds boundsB = entityB.GetBodyCollider().GetBounds();

        if (!boundsA.Intersects(boundsB)) return;

        float overlapX = Mathf.Min(boundsA.max.x, boundsB.max.x) 
                       - Mathf.Max(boundsA.min.x, boundsB.min.x);
        float overlapZ = Mathf.Min(boundsA.max.z, boundsB.max.z) 
                       - Mathf.Max(boundsA.min.z, boundsB.min.z);

        if (overlapX <= 0 || overlapZ <= 0) return;

        Transform transformA = entityA.GetTransform();
        Transform transformB = entityB.GetTransform();

        Vector3 pushAxis;
        float overlap;

        if (overlapX <= overlapZ)
        {
            pushAxis = Vector3.right;
            overlap = overlapX;
        }
        else
        {
            pushAxis = Vector3.forward;
            overlap = overlapZ;
        }

        float centerA = Vector3.Dot(transformA.position, pushAxis);
        float centerB = Vector3.Dot(transformB.position, pushAxis);
        float direction = (centerA < centerB) ? -1f : 1f;

        float halfOverlap = overlap / 2f;

        Vector3 pushA = pushAxis * direction * halfOverlap;
        Vector3 pushB = pushAxis * -direction * halfOverlap;

        // Test if push would cause wall overlap
        Vector3 testPosA = transformA.position + pushA;
        Vector3 testPosB = transformB.position + pushB;

        Bounds testBoundsA = new Bounds(
            new Vector3(testPosA.x, boundsA.center.y, testPosA.z), boundsA.size);
        Bounds testBoundsB = new Bounds(
            new Vector3(testPosB.x, boundsB.center.y, testPosB.z), boundsB.size);

        bool aHitsWall = stageCollision.IsOverlapping(testBoundsA);
        bool bHitsWall = stageCollision.IsOverlapping(testBoundsB);

        if (aHitsWall && !bHitsWall)
        {
            pushB = pushAxis * -direction * overlap;
            pushA = Vector3.zero;
        }
        else if (bHitsWall && !aHitsWall)
        {
            pushA = pushAxis * direction * overlap;
            pushB = Vector3.zero;
        }
        else if (aHitsWall && bHitsWall)
        {
            // Both at walls — don't push either
            return;
        }

        transformA.position += pushA;
        transformB.position += pushB;
    }
}