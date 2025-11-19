using UnityEngine;

public class Player : MonoBehaviour
{
    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private InputInterpreter inputInterpreter;

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public InputInterpreter GetInputInterpreter()
    {
        return inputInterpreter;
    }

    public void start()
    {
        movesManager.LoadMoves();
        inputInterpreter = new InputInterpreter(inputBuffer, movesManager.GetMovesDatabase(), stateManager);
    }

    public void update()
    {
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
        inputInterpreter.Update();
    }
}