using UnityEngine;

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
        foreach (var pair in dict)
        {
            Debug.Log(pair);
            pair.Value.InitialiseObjects();
        }
    }

    public MovementData GetMovementData(string id)
    {
        if (dict.ContainsKey(id))
        {
            return dict[id];
        }
        return null;
    }
}