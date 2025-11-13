using System;
using System.Collections.Generic;
using UnityEngine;

public class MovesDatabase : BaseDatabase<MoveData>
{
    public MovesDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/moves.json";
    }

    public override void ReadJson()
    {
        base.ReadJson();
        foreach (var pair in dict)
        {
            pair.Value.InitialiseObjects(dict);
        }
        RemoveNonRootMoves();
    }

    public void RemoveNonRootMoves()
    {
        List<string> keyToRemove = new List<string>();
        foreach (var pair in dict)
        {
            if (pair.Value.PrevMove != null)
            {
                keyToRemove.Add(pair.Key);
            }
        }
        foreach (string key in keyToRemove)
        {
            dict.Remove(key);
        }
    }

    public List<MoveData> GetMoveByIDs(List<string> ids)
    {
        List<MoveData> moves = new List<MoveData>();
        foreach (string id in ids)
        {
            moves.Add(dict[id]);
        }
        return moves;
    }
}