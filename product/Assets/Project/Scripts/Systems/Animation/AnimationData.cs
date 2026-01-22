using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private string clip;
    [SerializeField] private bool isLoop;
    [SerializeField] private float speed;
    private int totalFrames;

    public string Id => id;
    public string Clip => clip;
    public bool IsLoop => isLoop;
    public float Speed => speed;
    public int TotalFrames => totalFrames;

    public void InitialiseTotalFrames(MoveAnimator moveAnimator)
    {
        float clipDuration = moveAnimator.GetClipLength(clip);
        totalFrames = (int) Math.Ceiling(60f * clipDuration / speed);
        Debug.Log(id);
        Debug.Log(totalFrames);
    }
        
}