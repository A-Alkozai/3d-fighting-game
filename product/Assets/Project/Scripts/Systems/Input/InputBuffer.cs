using UnityEngine;
using System.Collections.Generic;

public class InputBuffer
{
    private List<InputObject> inputBuffer = new List<InputObject>();
    private int inputTTL = 6;

    public void AddInput(InputObject input)
    {
        inputBuffer.Add(input);
    }

    public void RemoveInput(InputObject input)
    {
        inputBuffer.Remove(input);
    }

    public void RemoveExpiredInputs()
    {
        if (inputBuffer.Count == 0) { return; }

        List<InputObject> expiredInputs = new List<InputObject>();
        foreach (InputObject input in inputBuffer)
        {
            if (input.GetFrame().GetFrameNumber() >= inputTTL)
            {
                expiredInputs.Add(input);
            }
            else { break; }
        }
        foreach (InputObject expired in expiredInputs)
        {
            inputBuffer.Remove(expired);
        }
    }

    public void UpdateFrameCounter()
    {
        if (inputBuffer.Count <= 0) { return; }

        foreach (InputObject input in inputBuffer)
        {
            input.GetFrame().UpdateFrame();
        }

    }

    public List<InputObject> GetInputBuffer()
    {
        return inputBuffer;
    }
}