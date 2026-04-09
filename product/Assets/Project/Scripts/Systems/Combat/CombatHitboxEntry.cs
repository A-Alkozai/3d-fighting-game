using System;
using UnityEngine;

// One phase of an attack - defines which hitbox is active, when, and what damage/effects it deals
// A single move can have multiple entries (e.g. jab-elbow has a fist phase and an elbow phase)
[Serializable]
public class CombatHitboxEntry
{
    [SerializeField] private string hitboxId;          // Which bone's hitbox to activate
    [SerializeField] private int startFrame;           // Frame this hitbox becomes active
    [SerializeField] private int endFrame;             // Frame this hitbox deactivates
    [SerializeField] private float sizeMultiplier;     // Scale the hitbox size for this phase
    [SerializeField] private string attackHeight;      // High/Mid/Low/SpecialMid/Unblockable (parsed to enum)
    [SerializeField] private int damage;
    [SerializeField] private int counterHitDamage;
    [SerializeField] private int hitStunFrames;        // How long defender is stunned on normal hit
    [SerializeField] private int blockStunFrames;      // How long defender is stunned on block
    [SerializeField] private float knockback;
    [SerializeField] private string hitEffect;         // Hitstun/Knockdown/Launch etc. (parsed to enum)
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

    // Parse string from JSON to enum at access time
    public AttackHeight AttackHeight =>
        (AttackHeight)Enum.Parse(typeof(AttackHeight), attackHeight);

    public HitEffect HitEffect =>
        (HitEffect)Enum.Parse(typeof(HitEffect), hitEffect);

    public HitEffect CounterHitEffect =>
        (HitEffect)Enum.Parse(typeof(HitEffect), counterHitEffect);
}