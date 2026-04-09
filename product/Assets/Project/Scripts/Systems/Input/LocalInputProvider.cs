using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

// Reads keyboard input each frame and produces InputObjects with tap/hold detection for directional keys
public class LocalInputProvider : IInputProvider
{
    private InputKeys inputKeys;
    private int holdThreshold = 10; // Frames before a directional press becomes a "hold"

    // Tracks how many frames each directional key has been held
    private Dictionary<InputCommand, int> canHoldInput = new Dictionary<InputCommand, int>
        { {InputCommand.Up, 0}, {InputCommand.Down, 0},
          {InputCommand.Left, 0}, {InputCommand.Right, 0}};

    // Stores the InputObject for directional keys currently being held down
    private Dictionary<InputCommand, InputObject> heldDownInputs = new Dictionary<InputCommand, InputObject>();

    public LocalInputProvider(InputKeys inputKeys)
    {
        this.inputKeys = inputKeys;
    }

    // Check all bound keys and return any new/changed inputs this frame
    public List<InputObject> GetInputs()
    {
        var inputs = new List<InputObject>();

        foreach (InputCommand command in Enum.GetValues(typeof(InputCommand)))
        {
            Key key;
            if (inputKeys.keybinds.ContainsKey(command))
            {
                key = inputKeys.keybinds[command];
            }
            else continue;

            // Directional keys have special tap/hold logic
            if (canHoldInput.ContainsKey(command))
            {
                // Build the hold variant name (e.g. Left → LeftHold)
                var name = command.ToString() + "Hold";
                InputCommand holdName = Enum.Parse<InputCommand>(name);

                // Key not pressed and counter is zero - nothing happening
                if (!Keyboard.current[key].isPressed && canHoldInput[command] == 0)
                {
                    continue;
                }

                // First frame the key is pressed - create a pending input (unknown if tap or hold yet)
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    InputObject heldInput = new InputObject(command, key, true);
                    heldDownInputs[holdName] = heldInput;
                    inputs.Add(heldInput);
                    continue;
                }

                // Key released before threshold - it was a tap
                if (!Keyboard.current[key].isPressed && canHoldInput[command] < holdThreshold)
                {
                    canHoldInput[command] = 0;
                    heldDownInputs[holdName].SetIsHeld(false);
                    heldDownInputs.Remove(holdName);
                    continue;
                }

                // Threshold reached - convert the pending input from tap to hold
                if (canHoldInput[command] == holdThreshold)
                {
                    heldDownInputs[holdName].ChangeInputCommand(holdName);
                    heldDownInputs[holdName].GetFrame().ResetFrame();
                    heldDownInputs[holdName].SetIsHeld(true);
                    inputs.Add(heldDownInputs[holdName]); // Signal to move from buffer to held list
                }

                // Key released after threshold - end the hold
                if (!Keyboard.current[key].isPressed)
                {
                    canHoldInput[command] = 0;
                    heldDownInputs[holdName].GetFrame().DisableFrame(); // Mark as disabled (-1)
                    inputs.Add(heldDownInputs[holdName]); // Signal to clean up
                    heldDownInputs.Remove(holdName);
                    continue;
                }

                // Key still held - keep counting frames
                if (Keyboard.current[key].isPressed)
                {
                    if (canHoldInput[command] > holdThreshold)
                    {
                        heldDownInputs[holdName].GetFrame().UpdateFrame();
                    }
                    canHoldInput[command]++;
                }
            }
            // Non-directional keys (attacks) - simple press detection, no hold logic
            else if (Keyboard.current[key].wasPressedThisFrame)
            {
                inputs.Add(new InputObject(command, key));
            }
        }

        return inputs;
    }
}