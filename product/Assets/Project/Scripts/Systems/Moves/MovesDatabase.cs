using System;
using System.Collections.Generic;
using UnityEngine;

public class MovesDatabase : BaseDatabase<MoveData>
{
    private MoveNode rootAttackNode = new MoveNode();
    private MoveNode rootMovementNode = new MoveNode();

    public MoveNode RootAttackNode => rootAttackNode;
    public MoveNode RootMovementNode => rootMovementNode;

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
        InitialiseTrees();
    }

    public void InitialiseTrees()
    {
        foreach (var pair in dict)
        {
            IReadOnlyList<InputCommand> inputs = pair.Value.InputSequence;
            MoveNode targetNode;
            if (pair.Value.MoveType == "movement")
            {
                targetNode = CreateNodePath(rootMovementNode, inputs);
            }
            else
            {
                targetNode = CreateNodePath(rootAttackNode, inputs);
            }
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

    public MoveNode GetNextNode(InputCommand input, MoveNode node)
    {
        if (node.NextNodes.ContainsKey(input))
        {
            return node.NextNodes[input];
        }
        return null;
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