using System;
using System.Collections.Generic;
using UnityEngine;

public class MovesDatabase : BaseDatabase<MoveData>
{
    private MoveNode rootAttackNode = new MoveNode();
    private MoveNode rootMovementNode = new MoveNode();
    private AnimationDatabase animationDatabase;

    public MoveNode RootAttackNode => rootAttackNode;
    public MoveNode RootMovementNode => rootMovementNode;

    public MovesDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/moves.json";
    }

    public void AddAnimationDatabase(AnimationDatabase animationDatabase)
    {
        this.animationDatabase = animationDatabase;
    }

    public override void ReadJson()
    {
        base.ReadJson();
        foreach (var pair in dict)
        {
            pair.Value.InitialiseObjects();
            pair.Value.LoadTotalFrames(animationDatabase.GetAnimationData(pair.Key));
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
            else if (pair.Value.MoveType == "attack")
            {
                targetNode = CreateNodePath(rootAttackNode, inputs);
            }
            else continue;
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

    public MoveData GetMoveById(string id)
    {
        return dict[id];
    }
}