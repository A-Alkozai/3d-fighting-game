using UnityEngine;

// Loads movement data from JSON and initialises per-frame velocity lookups
public class MovementDatabase : BaseDatabase<MovementData>
{
    public MovementDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/movement.json";
    }

    public override void ReadJson()
    {
        base.ReadJson();
        Debug.Log(dict);
        // After loading, convert raw frame lists into per-frame dictionaries
        foreach (var pair in dict)
        {
            Debug.Log(pair);
            pair.Value.InitialiseObjects();
        }
    }

    // Returns null if no movement data exists for this move
    public MovementData GetMovementData(string id)
    {
        if (dict.ContainsKey(id))
        {
            return dict[id];
        }
        return null;
    }
}