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
        foreach (MoveData move in list)
        {
            move.InitialiseObjects();
        }
    }

    public List<MoveData> GetMoveByIDs(List<string> ids)
    {
        List<MoveData> moves = new List<MoveData>();
        foreach (MoveData move in list)
        {
            foreach (string id in ids)
            {
                if (move.Id == id)
                {
                    moves.Add(move);
                }
            }
        }
        return moves;
    }

    public List<MoveData> GetMoveByInput(InputCommand input)
    {
        List<MoveData> moves = new List<MoveData>();
        foreach (MoveData move in list)
        {
            if (move.InputSequence[0] == input)
            {
                moves.Add(move);
            }
        }
        return moves;
    }
}