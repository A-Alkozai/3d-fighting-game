using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class LocalInputProvider : IInputProvider
{
    private InputKeys inputKeys;

    public LocalInputProvider(InputKeys inputKeys)
    {
        this.inputKeys = inputKeys;
    }

    public List<InputObject> GetInputs()
    {
        var inputs = new List<InputObject>();

        foreach (InputCommand command in Enum.GetValues(typeof(InputCommand)))
        {
            Key key = inputKeys.keybinds[command];

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                inputs.Add(new InputObject(command, key));
            }
        }

        return inputs;
    }
}
