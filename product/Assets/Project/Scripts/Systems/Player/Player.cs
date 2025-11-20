using UnityEngine;

public class Player : MonoBehaviour
{
    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private MoveSelector moveSelector;

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public MoveSelector GetMoveSelector()
    {
        return moveSelector;
    }

    public void start()
    {
        movesManager.LoadMoves();
        moveSelector = new MoveSelector(inputBuffer, movesManager.GetMovesDatabase(), stateManager);
    }

    public void update()
    {
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
        moveSelector.Update();
    }
}