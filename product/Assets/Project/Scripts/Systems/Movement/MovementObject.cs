using System;
using System.Collections.Generic;
using UnityEngine;

// A single movement entry - defines a velocity (dx, dy, dz) applied over a range of frames
[Serializable]
public class MovementObject
{
    [SerializeField] private List<int> frame;  // [startFrame, endFrame] or [singleFrame]
    [SerializeField] private float dx;
    [SerializeField] private float dy;
    [SerializeField] private float dz;

    private Vector3 vector;      // Pre-calculated per-frame velocity
    private int totalFrames;     // How many frames this movement spans

    public List<int> Frames => frame;
    public float Dx => dx;
    public float Dy => dy;
    public float Dz => dz;
    public Vector3 Vector => vector;
    public int TotalFrames => totalFrames;

    public void SetTotalFrames(int totalFrames)
    {
        this.totalFrames = totalFrames;
    }

    // Divide total displacement by frame count to get per-frame velocity
    public void InitialiseVector()
    {
        vector = new Vector3(dx, dy, dz)/totalFrames;
    }
}