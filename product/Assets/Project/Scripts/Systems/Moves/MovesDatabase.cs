using System;
using System.Collections.Generic;
using UnityEngine;

public class MovesDatabase : BaseDatabase<MoveData>
{
    private MoveNode rootMoveNode = new MoveNode();

    public MoveNode RootMoveNode => rootMoveNode;

    public MovesDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/moves.json";
    }

    public override void ReadJson()
    {
        base.ReadJson();
        foreach (var pair in dict)
        {
            pair.Value.InitialiseObjects();
        }
        InitialiseMoveTree();
    }

    public void InitialiseMoveTree()
    {
        foreach (var pair in dict)
        {
            IReadOnlyList<InputCommand> inputs = pair.Value.InputSequence;
            MoveNode targetNode = CreateNodePath(rootMoveNode, inputs);
            targetNode.AddMoveData(pair.Value);
        }
    }

    public MoveNode CreateNodePath(MoveNode node, IReadOnlyList<InputCommand> inputs)
    {
        int index = 0;
        while (index < inputs.Count)
        {
            if (!node.NextNodes.ContainsKey(inputs[index]))
            {
                MoveNode newNode = new MoveNode();
                newNode.SetPrevNode(node);
                node.AddChildNode(inputs[index], newNode);
            }
            node = node.NextNodes[inputs[index]];
            index++;
        }
        return node;
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