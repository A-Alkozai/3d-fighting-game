using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class LocalInputProvider : IInputProvider
{
    private InputKeys inputKeys;
    private int holdThreshold = 10;
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

                // If key is not pressed
                if (!Keyboard.current[key].isPressed && canHoldInput[command] == 0)
                {
                    continue;
                }

                // Initial press
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    InputObject heldInput = new InputObject(command, key, true); // Create pending input object
                    heldDownInputs[holdName] = heldInput; // Save object
                    inputs.Add(heldInput); // Send object
                    continue;
                }

                // Released TAP input
                if (!Keyboard.current[key].isPressed && canHoldInput[command] < holdThreshold)
                {
                    canHoldInput[command] = 0; // Reset counter
                    heldDownInputs[holdName].SetIsHeld(false); // Change Pending input -> Tap
                    heldDownInputs.Remove(holdName); // Remove local input object
                    continue;
                }

                // If threshold met -> convert Tap to Held
                if (canHoldInput[command] == holdThreshold)
                {
                    heldDownInputs[holdName].ChangeInputCommand(holdName); // Change tap command -> held command
                    heldDownInputs[holdName].GetFrame().ResetFrame(); // Reset frame count
                    heldDownInputs[holdName].SetIsHeld(true); // Change Pending input -> Held
                    inputs.Add(heldDownInputs[holdName]); // Signal to remove from buffer + UI update
                }

                // Release HELD input
                if (!Keyboard.current[key].isPressed)
                {
                    canHoldInput[command] = 0; // Reset counter
                    heldDownInputs[holdName].GetFrame().DisableFrame(); // Disable held input object
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
