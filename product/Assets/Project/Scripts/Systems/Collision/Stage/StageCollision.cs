using System.Collections.Generic;
using UnityEngine;

// Handles player-vs-wall collision using BoxColliders placed as children of a wall parent object
public class StageCollision
{
    private List<BoxCollider> wallColliders = new List<BoxCollider>();

    // Collect all BoxColliders from the wall parent's children
    public StageCollision(Transform wallParent)
    {
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

    // Push a player out of any walls they overlap with
    public void ResolvePlayer(Transform player, Bounds bodyBounds)
    {
        foreach (BoxCollider wall in wallColliders)
        {
            Bounds wallBounds = wall.bounds;

            if (!bodyBounds.Intersects(wallBounds)) continue;

            // Calculate how much the player overlaps the wall on each axis
            float overlapX = Mathf.Min(bodyBounds.max.x, wallBounds.max.x) 
                           - Mathf.Max(bodyBounds.min.x, wallBounds.min.x);
            float overlapY = Mathf.Min(bodyBounds.max.y, wallBounds.max.y) 
                           - Mathf.Max(bodyBounds.min.y, wallBounds.min.y);
            float overlapZ = Mathf.Min(bodyBounds.max.z, wallBounds.max.z) 
                           - Mathf.Max(bodyBounds.min.z, wallBounds.min.z);

            if (overlapX <= 0 || overlapY <= 0 || overlapZ <= 0) continue;

            // Push along the axis with the smallest overlap (least disruptive correction)
            Vector3 pos = player.position;

            if (overlapX <= overlapZ)
            {
                float direction = (bodyBounds.center.x < wallBounds.center.x) ? -1f : 1f;
                pos.x += direction * overlapX;
            }
            else
            {
                float direction = (bodyBounds.center.z < wallBounds.center.z) ? -1f : 1f;
                pos.z += direction * overlapZ;
            }

            player.position = pos;

            // Recalculate bounds after push so the next wall check uses the corrected position
            bodyBounds = new Bounds(
                new Vector3(pos.x, bodyBounds.center.y, pos.z),
                bodyBounds.size
            );
        }
    }

    // Check if a given bounds overlaps any wall (used by push collision to detect wall proximity)
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