using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class LocalInputProvider : IInputProvider
{
    private InputKeys inputKeys;
    private int holdThreshold = 60;
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
                var name = command.ToString() + "Hold";
                InputCommand holdName = Enum.Parse<InputCommand>(name);

                if (!Keyboard.current[key].isPressed && canHoldInput[command] == 0) continue;
                if (!Keyboard.current[key].isPressed && canHoldInput[command] < holdThreshold)
                {
                    inputs.Add(new InputObject(command, key));
                    canHoldInput[command] = 0;
                }
                else if (!Keyboard.current[key].isPressed)
                {
                    canHoldInput[command] = 0;
                    heldDownInputs[holdName].GetFrame().DisableFrame();
                    inputs.Add(heldDownInputs[holdName]);
                    heldDownInputs.Remove(holdName);
                }

                if (Keyboard.current[key].isPressed)
                {
                    canHoldInput[command]++;
                }

                if (canHoldInput[command] == holdThreshold)
                {
                    InputObject heldInput = new InputObject(holdName, key, true);
                    heldDownInputs[holdName] = heldInput;
                    inputs.Add(heldInput);
                }
                
                else if (canHoldInput[command] > holdThreshold)
                {
                    heldDownInputs[holdName].GetFrame().UpdateFrame();
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
