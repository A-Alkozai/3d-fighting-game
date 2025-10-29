using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    IInputProvider inputProvider;

    public void AddInputProvider(IInputProvider inputProvider)
    {
        this.inputProvider = inputProvider;
    }

    public void update()
    {
        if (inputProvider == null) { return; }

        List<InputObject> receivedInputs = inputProvider.GetInputs();
        if (receivedInputs.Count == 0) { return; }

        foreach (InputObject input in receivedInputs)
        {
            Debug.Log("Added Input: " + input.GetInputCommand());
        }
    }
}