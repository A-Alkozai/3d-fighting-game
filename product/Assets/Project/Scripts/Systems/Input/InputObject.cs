using UnityEngine.InputSystem;

public class InputObject
{
    private InputCommand inputCommand;
    private Key inputKey;
    private FrameCounter frame;
    private bool isHeld;

    public InputObject(InputCommand inputCommand, Key inputKey, bool isHeld = false)
    {
        this.inputCommand = inputCommand;
        this.inputKey = inputKey;
        this.isHeld = isHeld;
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

    public bool IsHeld()
    {
        return isHeld;
    }

    public void ChangeInputCommand(InputCommand newInputCommand)
    {
        inputCommand = newInputCommand;
    }
}