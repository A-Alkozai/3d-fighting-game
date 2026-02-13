using System;
using UnityEngine;

[Serializable]
public class BodyColliderData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private Vector3 size;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 center;

    public string Id => id;
    public Vector3 Size => size;
    public Vector3 Offset => offset;
    public Vector3 Center => center;
}