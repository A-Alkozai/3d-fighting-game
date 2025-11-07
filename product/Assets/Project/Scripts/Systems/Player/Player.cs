using UnityEngine;

public class Player : MonoBehaviour
{
    private InputBuffer inputBuffer = new InputBuffer();

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public void update()
    {
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
    }
}