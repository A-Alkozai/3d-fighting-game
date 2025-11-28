using UnityEngine;

public class Player : MonoBehaviour
{
    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private MoveSelector moveSelector;
    private MoveExecutor moveExecutor;

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
        stateManager.AddState(PlayerStates.Idle);
        movesManager.LoadMoves();
        moveExecutor = new MoveExecutor(stateManager);
        moveSelector = new MoveSelector(inputBuffer, movesManager.GetMovesDatabase(),
                                        stateManager, moveExecutor);

    }

    public void update()
    {
        moveSelector.Update();
        moveExecutor.Update();
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
    }
}