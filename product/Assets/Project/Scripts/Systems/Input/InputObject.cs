using UnityEngine.InputSystem;

public class InputObject
{
    private InputCommand inputCommand;
    private Key inputKey;
    private FrameCounter frame;
    private bool isHeld;
    private bool isPending;

    public InputObject(InputCommand inputCommand, Key inputKey, bool isPending = false)
    {
        this.inputCommand = inputCommand;
        this.inputKey = inputKey;
        this.isPending = isPending;
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

    public void SetIsHeld(bool isHeld)
    {
        isPending = false;
        this.isHeld = false;
        
        if (isHeld)
        {
            this.isHeld = true;
        }
    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public bool IsPending()
    {
        return isPending;
    }

    public bool IsDirectional()
    {
        if (inputCommand == InputCommand.Left || inputCommand == InputCommand.Right ||
            inputCommand == InputCommand.Up || inputCommand == InputCommand.Down ||
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