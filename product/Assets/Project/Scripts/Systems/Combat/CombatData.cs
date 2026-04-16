using System;
using System.Collections.Generic;
using UnityEngine;

// JSON data for one move's combat properties - contains whether it's blockable and all hitbox phase entries
[Serializable]
public class CombatData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private bool blockable;
    [SerializeField] private List<CombatHitboxEntry> hitboxEntries; // Each entry = one attack phase with its own timing/damage/height

    public string Id => id;
    public bool Blockable => blockable;
    public List<CombatHitboxEntry> HitboxEntries => hitboxEntries;
}