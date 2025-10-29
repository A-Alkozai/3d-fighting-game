using UnityEngine.InputSystem;

public class InputObject
{
    private InputCommand inputCommand;
    private Key inputKey;
    private FrameCounter frame;

    public InputObject(InputCommand inputCommand, Key inputKey)
    {
        this.inputCommand = inputCommand;
        this.inputKey = inputKey;
        this.frame = new FrameCounter();
    }

    public InputCommand GetInputCommand()
    {
        return inputCommand;
    }

    public Key GetInputKey()
    {
        return inputKey;
    }

    public FrameCounter GetFrame()
    {
        return frame;
    }
}