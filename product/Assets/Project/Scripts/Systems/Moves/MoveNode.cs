using System.Collections.Generic;

public class MoveNode
{
    private Dictionary<InputCommand, MoveNode> nextNodes = new();
    private List<MoveData> moveDatas = new List<MoveData>();
    private MoveNode prevNode;

    public IReadOnlyDictionary<InputCommand, MoveNode> NextNodes => nextNodes;
    public IReadOnlyList<MoveData> MoveDatas => moveDatas;
    public MoveNode PrevNode => prevNode;

    public void AddChildNode(InputCommand input, MoveNode moveNode)
    {
        nextNodes[input] = moveNode;
    }

    public void AddMoveData(MoveData moveData)
    {
        moveDatas.Add(moveData);
    }

    public void SetPrevNode(MoveNode node)
    {
        prevNode = node;
    }

}
