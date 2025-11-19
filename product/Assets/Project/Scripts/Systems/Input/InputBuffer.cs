using UnityEngine;
using System.Collections.Generic;

public class InputBuffer
{
    private List<InputObject> inputBuffer = new List<InputObject>();
    private int inputTTL = 8;

    public void AddInput(InputObject input)
    {
        inputBuffer.Add(input);
    }

    public void RemoveInputAt(int index)
    {
        inputBuffer.RemoveAt(index);
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

    public InputObject GetInputAt(int index)
    {
        if (index >= inputBuffer.Count || index < 0)
        {
            return null;
        }
        return inputBuffer[index];
    }

    public int Count()
    {
        return inputBuffer.Count;
    }

    public List<InputObject> GetInputBuffer()
    {
        return inputBuffer;
    }
}