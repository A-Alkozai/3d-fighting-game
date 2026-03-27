using System;
using UnityEngine;

[Serializable]
public class CombatHitboxEntry
{
    [SerializeField] private string hitboxId;
    [SerializeField] private int startFrame;
    [SerializeField] private int endFrame;
    [SerializeField] private float sizeMultiplier;

    public string HitboxId => hitboxId;
    public int StartFrame => startFrame;
    public int EndFrame => endFrame;
    public float SizeMultiplier => sizeMultiplier;
}