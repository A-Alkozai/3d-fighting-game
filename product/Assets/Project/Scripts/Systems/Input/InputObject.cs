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
        frame = new FrameCounter();
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

    public bool IsDirectional()
    {
        if (inputCommand == InputCommand.Left    || inputCommand == InputCommand.Right ||
            inputCommand == InputCommand.Up      || inputCommand == InputCommand.Down ||
            inputCommand == InputCommand.Forward || inputCommand == InputCommand.Backward)
        {
            return true;
        }
        return false;
    }

    public void ChangeInputCommand(InputCommand newInputCommand)
    {
        inputCommand = newInputCommand;
    }
}