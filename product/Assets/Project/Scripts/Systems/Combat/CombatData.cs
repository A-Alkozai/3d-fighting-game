using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private bool blockable;
    [SerializeField] private List<CombatHitboxEntry> hitboxEntries;

    public string Id => id;
    public bool Blockable => blockable;
    public List<CombatHitboxEntry> HitboxEntries => hitboxEntries;
}