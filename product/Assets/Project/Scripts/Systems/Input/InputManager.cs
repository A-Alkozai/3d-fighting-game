using UnityEngine;
using System.Collections.Generic;

// Routes inputs from providers to their mapped players each frame
// Also feeds the debug input display UI
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

    // Poll all input providers and route their inputs to the correct player
    public void update()
    {
        if (inputToPlayerMap.Count == 0) { return; }

        foreach (var inputPlayerPair in inputToPlayerMap)
        {
            List<InputObject> recievedInputs = inputPlayerPair.Key.GetInputs();
            if (recievedInputs.Count == 0) { return; }

            foreach (InputObject input in recievedInputs)
            {
                // Held input that was in the buffer - move it to the held inputs list
                if (!input.IsPending() && input.IsHeld() && input.GetFrame().GetFrameNumber() != -1)
                {
                    inputPlayerPair.Value.GetMoveSelector().AddHeldInput(input);
                    inputPlayerPair.Value.GetInputBuffer().Remove(input);
                }
                // Held input released (frame == -1) - signal to clean up
                else if (!input.IsPending() && input.IsHeld())
                {
                    inputPlayerPair.Value.GetMoveSelector().UpdateHeldInputs();
                }
                // Normal input or pending - add to buffer
                else
                {
                    inputPlayerPair.Value.GetInputBuffer().AddInput(input);
                }

                // Update debug UI if this is a local player
                if (inputPlayerPair.Key is LocalInputProvider && recentInputsUI.GetIsActive())
                {
                    recentInputsUI.AddRecentInput(input);
                }
            }
        }
    }
}