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
    [SerializeField] private int damage;
    [SerializeField] private int counterHitDamage;
    [SerializeField] private int hitStunFrames;
    [SerializeField] private int blockStunFrames;
    [SerializeField] private float knockback;
    [SerializeField] private string hitEffect;
    [SerializeField] private string counterHitEffect;

    public string HitboxId => hitboxId;
    public int StartFrame => startFrame;
    public int EndFrame => endFrame;
    public float SizeMultiplier => sizeMultiplier;
    public int Damage => damage;
    public int CounterHitDamage => counterHitDamage;
    public int HitStunFrames => hitStunFrames;
    public int BlockStunFrames => blockStunFrames;
    public float Knockback => knockback;

    public AttackHeight AttackHeight =>
        (AttackHeight)Enum.Parse(typeof(AttackHeight), attackHeight);

    public HitEffect HitEffect =>
        (HitEffect)Enum.Parse(typeof(HitEffect), hitEffect);

    public HitEffect CounterHitEffect =>
        (HitEffect)Enum.Parse(typeof(HitEffect), counterHitEffect);
}