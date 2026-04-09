using System;
using System.Collections.Generic;
using UnityEngine;

// Data for a single animation: which clip to play, at what speed, and whether it loops
[Serializable]
public class AnimationData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private string clip;
    [SerializeField] private bool isLoop;
    [SerializeField] private float speed;
    private int totalFrames; // Computed at runtime from clip length and speed

    public string Id => id;
    public string Clip => clip;
    public bool IsLoop => isLoop;
    public float Speed => speed;
    public int TotalFrames => totalFrames;

    // Calculate how many logic frames (at 60fps) this animation takes to play
    public void InitialiseTotalFrames(AnimationExecutor animationExecutor)
    {
        float clipDuration = animationExecutor.GetClipLength(clip);
        totalFrames = (int) Math.Ceiling(60f * clipDuration / speed);
        Debug.Log(id);
        Debug.Log(totalFrames);
    }
        
}