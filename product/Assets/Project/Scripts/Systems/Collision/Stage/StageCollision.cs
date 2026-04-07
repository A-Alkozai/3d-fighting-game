using System.Collections.Generic;
using UnityEngine;

public class StageCollision
{
    private List<BoxCollider> wallColliders = new List<BoxCollider>();

    public StageCollision(Transform wallParent)
    {
        // Gather all BoxColliders from children
        foreach (Transform child in wallParent)
        {
            BoxCollider col = child.GetComponent<BoxCollider>();
            if (col != null)
            {
                wallColliders.Add(col);
            }
        }

        Debug.Log($"[StageCollision] Found {wallColliders.Count} wall colliders");
    }

    public void ResolvePlayer(Transform player, Bounds bodyBounds)
    {
        foreach (BoxCollider wall in wallColliders)
        {
            Bounds wallBounds = wall.bounds;

            if (!bodyBounds.Intersects(wallBounds)) continue;

            // Calculate overlap on each axis
            float overlapX = Mathf.Min(bodyBounds.max.x, wallBounds.max.x) 
                           - Mathf.Max(bodyBounds.min.x, wallBounds.min.x);
            float overlapY = Mathf.Min(bodyBounds.max.y, wallBounds.max.y) 
                           - Mathf.Max(bodyBounds.min.y, wallBounds.min.y);
            float overlapZ = Mathf.Min(bodyBounds.max.z, wallBounds.max.z) 
                           - Mathf.Max(bodyBounds.min.z, wallBounds.min.z);

            if (overlapX <= 0 || overlapY <= 0 || overlapZ <= 0) continue;

            // Push along smallest overlap axis
            Vector3 pos = player.position;

            if (overlapX <= overlapZ)
            {
                // Push on X
                float direction = (bodyBounds.center.x < wallBounds.center.x) ? -1f : 1f;
                pos.x += direction * overlapX;
            }
            else
            {
                // Push on Z
                float direction = (bodyBounds.center.z < wallBounds.center.z) ? -1f : 1f;
                pos.z += direction * overlapZ;
            }

            player.position = pos;

            // Recalculate bounds after push for next wall check
            bodyBounds = new Bounds(
                new Vector3(pos.x, bodyBounds.center.y, pos.z),
                bodyBounds.size
            );
        }
    }

    public bool IsOverlapping(Bounds bodyBounds)
    {
        foreach (BoxCollider wall in wallColliders)
        {
            if (bodyBounds.Intersects(wall.bounds))
                return true;
        }
        return false;
    }

    public List<BoxCollider> GetWallColliders()
    {
        return wallColliders;
    }
}