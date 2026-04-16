using System;
using UnityEngine;

// JSON data for a collision box: standing/crouching sizes and offsets per bone
[Serializable]
public class CollisionBoxData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private Vector3 standingSize;
    [SerializeField] private Vector3 standingOffset;
    [SerializeField] private Vector3 crouchingSize;
    [SerializeField] private Vector3 crouchingOffset;
    [SerializeField] private bool resetRotation; // If true, ignore bone rotation for this box

    public string Id => id;
    public Vector3 StandingSize => standingSize;
    public Vector3 StandingOffset => standingOffset;
    public Vector3 CrouchingSize => crouchingSize;
    public Vector3 CrouchingOffset => crouchingOffset;
    public bool ResetRotation => resetRotation;
}