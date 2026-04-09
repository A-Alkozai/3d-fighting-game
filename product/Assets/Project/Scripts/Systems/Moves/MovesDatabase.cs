using System;
using System.Collections.Generic;
using UnityEngine;

// Loads move data from JSON, links it to animation data for frame counts,
// and builds input trees for move lookup (separate trees for attacks and movements)
public class MovesDatabase : BaseDatabase<MoveData>
{
    private MoveNode rootAttackNode = new MoveNode();    // Root of the attack input tree
    private MoveNode rootMovementNode = new MoveNode();  // Root of the movement input tree
    private AnimationDatabase animationDatabase;

    public MoveNode RootAttackNode => rootAttackNode;
    public MoveNode RootMovementNode => rootMovementNode;

    public MovesDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/moves.json";
    }

    // Must be set before ReadJson so moves can look up their animation frame counts
    public void AddAnimationDatabase(AnimationDatabase animationDatabase)
    {
        this.animationDatabase = animationDatabase;
    }

    // Load JSON, parse input/state strings to enums, set total frames from animation data, build trees
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

    // Build the input trees - each move's input sequence becomes a path in the appropriate tree
    // Move data is attached to the final node of the path
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
            else continue; // "state" type moves aren't in the tree - they're fallbacks
            targetNode.AddMoveData(pair.Value);
        }
    }

    // Walk the input sequence through the tree, creating nodes as needed, return the final node
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

    // Traverse one step in the tree - returns null if no child exists for this input
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