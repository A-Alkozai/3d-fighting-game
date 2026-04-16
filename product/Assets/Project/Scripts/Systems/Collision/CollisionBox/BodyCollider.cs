using UnityEngine;

// Runtime body collider used for push resolution between players
// Uses a trigger BoxCollider - all collision is handled manually, not by Unity physics
public class BodyCollider
{
    private GameObject bodyColliderObject;
    private BoxCollider collider;

    // Create a child object on the body transform with a trigger BoxCollider
    public BodyCollider(Transform body, BodyColliderData data)
    {
        bodyColliderObject = new GameObject("BodyCollider");
        bodyColliderObject.transform.SetParent(body);
        bodyColliderObject.transform.localRotation = Quaternion.identity;
        bodyColliderObject.transform.localPosition = data.Offset;

        collider = bodyColliderObject.AddComponent<BoxCollider>();
        collider.size = data.Size;
        collider.center = data.Center;
        collider.isTrigger = true;
    }

    // Returns world-space bounds for manual overlap checks
    public Bounds GetBounds()
    {
        return collider.bounds;
    }

    public void DrawGizmos()
    {
        if (bodyColliderObject == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = bodyColliderObject.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(collider.center, collider.size);
    }
}