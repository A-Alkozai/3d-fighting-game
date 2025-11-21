using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class LocalInputProvider : IInputProvider
{
    private InputKeys inputKeys;
    private int holdThreshold = 50;
    private Dictionary<InputCommand, int> canHoldInput = new Dictionary<InputCommand, int>
        { {InputCommand.Up, 0}, {InputCommand.Down, 0},
          {InputCommand.Left, 0}, {InputCommand.Right, 0}};
    private Dictionary<InputCommand, InputObject> heldDownInputs = new Dictionary<InputCommand, InputObject>();

    public LocalInputProvider(InputKeys inputKeys)
    {
        this.inputKeys = inputKeys;
    }

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

            if (canHoldInput.ContainsKey(command))
            {
                // Change name to held down variant
                var name = command.ToString() + "Hold";
                InputCommand holdName = Enum.Parse<InputCommand>(name);

                // Initial press
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    InputObject heldInput = new InputObject(command, key); // Create tap object
                    heldDownInputs[holdName] = heldInput; // Save object
                    inputs.Add(heldInput); // Send object
                    continue;
                }

                // Released input -> TAP
                if (!Keyboard.current[key].isPressed && canHoldInput[command] < holdThreshold)
                {
                    canHoldInput[command] = 0; // Reset counter
                    continue;
                }

                // If threshold met -> convert tap to hold
                if (canHoldInput[command] == holdThreshold)
                {
                    heldDownInputs[holdName].GetFrame().DisableFrame(); // Disable initial tap object
                    inputs.Add(heldDownInputs[holdName]); // Send disabled object -> signal to remove from buffer
                    InputObject heldInput = new InputObject(holdName, key, true); // Create held input object
                    heldDownInputs[holdName] = heldInput; // Save object locally
                    inputs.Add(heldInput); // Send held object
                }

                // Release input -> HOLD
                if (!Keyboard.current[key].isPressed)
                {
                    canHoldInput[command] = 0; // Reset counter
                    heldDownInputs[holdName].GetFrame().DisableFrame(); // Disable hold object
                    inputs.Add(heldDownInputs[holdName]); // Send disabled object -> signal to remove
                    heldDownInputs.Remove(holdName); // Remove local hold object
                    continue;
                }

                // If key held down
                if (Keyboard.current[key].isPressed)
                {
                    // Currently HELD input
                    if (canHoldInput[command] > holdThreshold)
                    {
                        heldDownInputs[holdName].GetFrame().UpdateFrame();
                    }
                    canHoldInput[command]++; // Increment counter
                }
            }
            else if (Keyboard.current[key].wasPressedThisFrame)
            {
                inputs.Add(new InputObject(command, key));
            }
        }

        return inputs;
    }
}
