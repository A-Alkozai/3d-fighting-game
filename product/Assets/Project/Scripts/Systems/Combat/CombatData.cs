using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private string attackHeight;
    [SerializeField] private int damage;
    [SerializeField] private int counterHitDamage;
    [SerializeField] private bool blockable;
    [SerializeField] private int hitStunFrames;
    [SerializeField] private int blockStunFrames;
    [SerializeField] private float knockback;
    [SerializeField] private string hitEffect;
    [SerializeField] private string counterHitEffect;
    [SerializeField] private List<CombatHitboxEntry> hitboxEntries;

    public string Id => id;
    public int Damage => damage;
    public int CounterHitDamage => counterHitDamage;
    public bool Blockable => blockable;
    public int HitStunFrames => hitStunFrames;
    public int BlockStunFrames => blockStunFrames;
    public float Knockback => knockback;
    public List<CombatHitboxEntry> HitboxEntries => hitboxEntries;

    public AttackHeight AttackHeight =>
        (AttackHeight)Enum.Parse(typeof(AttackHeight), attackHeight);

    public HitEffect HitEffect =>
        (HitEffect)Enum.Parse(typeof(HitEffect), hitEffect);

    public HitEffect CounterHitEffect =>
        (HitEffect)Enum.Parse(typeof(HitEffect), counterHitEffect);
}