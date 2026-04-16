using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// Stores recent inputs as a list with per-input frame counters and expiry logic
// Attack inputs expire quickly (8 frames), directional inputs last longer (130 frames)
public class InputBuffer
{
    private List<InputObject> inputBuffer = new List<InputObject>();
    private int attackTTL = 8;             // Frames before an attack input expires
    private int directionalTTL = 130;      // Frames before a directional input expires
    private int directionalOverride = 20;  // Directional inputs older than this get removed when an attack arrives

    // Add a new input to the buffer
    public void AddInput(InputObject input)
    {
        inputBuffer.Add(input);
        Debug.Log($"Added input: {input.GetInputCommand()}");

        // When an attack input arrives, clean up old directional inputs before it
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

    // Remove inputs that have been in the buffer too long
    public void RemoveExpiredInputs()
    {
        if (inputBuffer.Count == 0) { return; }

        List<InputObject> expiredInputs = new List<InputObject>();
        foreach (InputObject input in inputBuffer)
        {
            // Attack inputs expire after attackTTL frames, also clean up directionals near them
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
            // Directional inputs expire after directionalTTL frames
            else if (input.IsDirectional() && input.GetFrame().GetFrameNumber() >= directionalTTL)
            {
                expiredInputs.Add(input);
            }
            // Stop at first non-expired attack input (buffer is ordered)
            else if (!input.IsDirectional())
            {
                break;
            }
        }
        RemoveInputsByList(expiredInputs);
    }

    // Remove old directional inputs that appear before an attack input in the buffer
    // When isReturn is true, returns the list instead of removing (used during expiry)
    public List<InputObject> OverrideDirectional(int index, bool isReturn = false)
    {
        List<InputObject> expiredInputs = new List<InputObject>();

        // Walk backwards from the attack input, removing stale directionals
        for (int i = index - 1; i >= 0; i--)
        {
            InputObject input = inputBuffer[i];
            if (input.IsDirectional() && input.GetFrame().GetFrameNumber() > directionalOverride)
            {
                expiredInputs.Add(input);
            }
            else if (!input.IsDirectional())
                break; // Stop at the previous attack input
        }
        if (isReturn)
        {
            return expiredInputs;
        }
        RemoveInputsByList(expiredInputs);
        return null;
    }

    // Increment the frame counter on every input in the buffer (called once per logic frame)
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

    public void Clear()
    {
        inputBuffer.Clear();
    }
}