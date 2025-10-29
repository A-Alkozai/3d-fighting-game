using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    private InputContext inputContext;
    private Dictionary<IInputProvider, Player> inputToPlayerMap = new Dictionary<IInputProvider, Player>();

    public void AddInputToPlayerMap(IInputProvider inputProvider, Player player)
    {
        inputToPlayerMap.Add(inputProvider, player);
    }

    public void update()
    {
        if (inputToPlayerMap.Count == 0) { return; }

        foreach (var inputPlayerPair in inputToPlayerMap)
        {
            List<InputObject> recievedInputs = inputPlayerPair.Key.GetInputs();
            if (recievedInputs.Count == 0) { return; }

            foreach (InputObject input in recievedInputs)
            {
                inputPlayerPair.Value.GetInputBuffer().AddInput(input);
                if (inputPlayerPair.Key is LocalInputProvider)
                {
                    Debug.Log("Added Input: " + input.GetInputCommand());
                }
            }
        }
    }
}