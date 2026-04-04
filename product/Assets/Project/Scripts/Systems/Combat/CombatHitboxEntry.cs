using System;
using UnityEngine;

[Serializable]
public class CombatHitboxEntry
{
    [SerializeField] private string hitboxId;
    [SerializeField] private int startFrame;
    [SerializeField] private int endFrame;
    [SerializeField] private float sizeMultiplier;
    [SerializeField] private string attackHeight;

    public string HitboxId => hitboxId;
    public int StartFrame => startFrame;
    public int EndFrame => endFrame;
    public float SizeMultiplier => sizeMultiplier;

    public AttackHeight AttackHeight =>
        (AttackHeight)Enum.Parse(typeof(AttackHeight), attackHeight);
}