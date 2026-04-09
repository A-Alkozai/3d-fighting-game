using UnityEngine.InputSystem;

// Represents a single input event - tracks command, key, frame age, and whether it's held/pending
public class InputObject
{
    private InputCommand inputCommand;
    private Key inputKey;
    private FrameCounter frame;   // Tracks how many frames since this input was created
    private bool isHeld;          // True if this is a held-down directional input
    private bool isPending;       // True while waiting to determine if this is a tap or hold

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

    // Resolve a pending input as either a tap (isHeld=false) or a hold (isHeld=true)
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

    // Returns true for directional inputs (movement), false for attacks
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

    // Used to convert raw Left/Right to normalised Forward/Backward based on facing direction
    public void ChangeInputCommand(InputCommand newInputCommand)
    {
        inputCommand = newInputCommand;
    }
}