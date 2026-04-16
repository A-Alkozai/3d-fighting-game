using System.Collections.Generic;

// A node in the move input tree - each node can have children keyed by InputCommand,
// and may hold one or more MoveData entries (moves that execute when this node is reached)
public class MoveNode
{
    private Dictionary<InputCommand, MoveNode> nextNodes = new();   // Children: input → next node
    private List<MoveData> moveDatas = new List<MoveData>();        // Moves available at this node
    private MoveNode prevNode;                                      // Parent node (for backtracking)

    public IReadOnlyDictionary<InputCommand, MoveNode> NextNodes => nextNodes;
    public IReadOnlyList<MoveData> MoveDatas => moveDatas;
    public MoveNode PrevNode => prevNode;

    public void AddChildNode(InputCommand input, MoveNode moveNode)
    {
        nextNodes[input] = moveNode;
    }

    // A node can hold multiple moves (e.g. same input but different required states)
    public void AddMoveData(MoveData moveData)
    {
        moveDatas.Add(moveData);
    }

    public void SetPrevNode(MoveNode node)
    {
        prevNode = node;
    }
}