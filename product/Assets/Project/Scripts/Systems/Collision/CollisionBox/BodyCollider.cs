using UnityEngine;

public class BodyCollider
{
    private GameObject bodyColliderObject;
    private BoxCollider collider;

    public BodyCollider(Transform body, BodyColliderData data)
    {
        bodyColliderObject = new GameObject("BodyCollider");
        bodyColliderObject.transform.SetParent(body);
        bodyColliderObject.transform.localRotation = Quaternion.identity;
        bodyColliderObject.transform.localPosition = data.Offset;

        collider = bodyColliderObject.AddComponent<BoxCollider>();
        collider.size = data.Size;
        collider.center = data.Center;
        collider.isTrigger = true;  // We handle collision manually, trigger avoids Unity resolving it
    }

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