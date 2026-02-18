using UnityEngine;

public class BodyColliderDatabase : BaseDatabase<BodyColliderData>
{
    private BodyCollider bodyCollider;

    public BodyColliderDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/bodycollider.json";
    }

    public void Initialise(Transform body)
    {
        dict.TryGetValue("bodyCollider", out BodyColliderData data);
        if (data == null)
        {
            Debug.LogError("No body collider data found");
            return;
        }

        bodyCollider = new BodyCollider(body, data);
        Debug.Log("Body collider created!");
    }

    public BodyCollider GetBodyCollider()
    {
        return bodyCollider;
    }
}