using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    private RecentInputsUI recentInputsUI;
    private Dictionary<IInputProvider, Player> inputToPlayerMap = new Dictionary<IInputProvider, Player>();

    public void AddInputToPlayerMap(IInputProvider inputProvider, Player player)
    {
        inputToPlayerMap.Add(inputProvider, player);
    }

    public void AddRecentInputsUI(RecentInputsUI recentInputsUI)
    {
        this.recentInputsUI = recentInputsUI;
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
                if (input.IsHeld())
                {
                    inputPlayerPair.Value.GetInputInterpreter().AddHeldInput(input);
                }
                else
                {
                    inputPlayerPair.Value.GetInputBuffer().AddInput(input);
                }

                if (inputPlayerPair.Key is LocalInputProvider && recentInputsUI.GetIsActive())
                {
                    recentInputsUI.AddRecentInput(input);
                }
            }
        }
    }
}