using System.Collections.Generic;
using UnityEngine;

// Loads collision box data from JSON and creates runtime CollisionBox instances for each bone
public class CollisionBoxDatabase : BaseDatabase<CollisionBoxData>
{
    private Dictionary<string, CollisionBox> collisionBoxes = new Dictionary<string, CollisionBox>();

    public CollisionBoxDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/collisions.json";
    }

    // Create a CollisionBox for each bone that has matching data in the JSON
    public void Initialise(Dictionary<string, Transform> bones)
    {
        foreach (var pair in bones)
        {
            string id = pair.Key;
            Transform bone = pair.Value;

            dict.TryGetValue(id, out CollisionBoxData data);
            if (data == null)
            {
                Debug.LogWarning($"No collision data for: {id}");
                continue;
            }

            CollisionBox box = new CollisionBox(id, bone, data);
            collisionBoxes[id] = box;
            Debug.Log($"[CollisionBoxDatabase] Created: {id}");
        }
    }

    public CollisionBox GetCollisionBox(string id)
    {
        collisionBoxes.TryGetValue(id, out CollisionBox box);
        return box;
    }

    public Dictionary<string, CollisionBox> GetAllCollisionBoxes()
    {
        return collisionBoxes;
    }
}