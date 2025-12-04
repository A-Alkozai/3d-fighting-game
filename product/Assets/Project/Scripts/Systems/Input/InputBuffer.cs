using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class InputBuffer
{
    private List<InputObject> inputBuffer = new List<InputObject>();
    private int attackTTL = 8;
    private int directionalTTL = 130;
    private int directionalOverride = 20;

    public void AddInput(InputObject input)
    {
        inputBuffer.Add(input);
        Debug.Log($"Added input: {input.GetInputCommand()}");

        if (!input.IsDirectional())
        {
            OverrideDirectional(inputBuffer.Count - 1);
        }
    }

    public void Remove(InputObject input)
    {
        Debug.Log($"Removed input: {input.GetInputCommand()}");
        inputBuffer.Remove(input);
    }

    public void RemoveInputAt(int index)
    {
        Debug.Log($"Removed input: {inputBuffer[index].GetInputCommand()}");
        inputBuffer.RemoveAt(index);
    }

    public void RemoveInputsByList(List<InputObject> expiredInputs)
    {
        foreach (InputObject expired in expiredInputs)
        {
            Debug.Log($"Removed input: {expired.GetInputCommand()}");
            inputBuffer.Remove(expired);
        }
    }

    public void RemoveExpiredInputs()
    {
        if (inputBuffer.Count == 0) { return; }

        List<InputObject> expiredInputs = new List<InputObject>();
        foreach (InputObject input in inputBuffer)
        {
            if (!input.IsDirectional() && input.GetFrame().GetFrameNumber() >= attackTTL)
            {
                expiredInputs.Add(input);
                List<InputObject> extraExpiredInputs = OverrideDirectional(inputBuffer.IndexOf(input), true);
                if (extraExpiredInputs != null)
                {
                    expiredInputs.AddRange(extraExpiredInputs);
                    expiredInputs = expiredInputs.Distinct().ToList();
                }
            }
            else if (input.IsDirectional() && input.GetFrame().GetFrameNumber() >= directionalTTL)
            {
                expiredInputs.Add(input);
            }
            else if (!input.IsDirectional())
            {
                break;
            }
        }
        RemoveInputsByList(expiredInputs);
    }

    public List<InputObject> OverrideDirectional(int index, bool isReturn = false)
    {
        List<InputObject> expiredInputs = new List<InputObject>();

        for (int i = index - 1; i >= 0; i--)
        {
            InputObject input = inputBuffer[i];
            if (input.IsDirectional() && input.GetFrame().GetFrameNumber() > directionalOverride)
            {
                expiredInputs.Add(input);
            }
            else if (!input.IsDirectional())
                break;
        }
        if (isReturn)
        {
            return expiredInputs;
        }
        RemoveInputsByList(expiredInputs);
        return null;
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

    public bool Contains(InputObject input)
    {
        return inputBuffer.Contains(input);
    }
}