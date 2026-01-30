using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovementObject
{
    [SerializeField] private List<int> frame;
    [SerializeField] private float dx;
    [SerializeField] private float dy;
    [SerializeField] private float dz;

    private Vector3 vector;
    private int totalFrames;

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

    public void InitialiseVector()
    {
        vector = new Vector3(dx, dy, dz)/totalFrames;
    }
}