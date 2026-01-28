using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovementData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private List<MovementObject> movements;
    private Dictionary<int, MovementObject> movementsObj = new Dictionary<int, MovementObject>();

    public string Id => id;
    public IReadOnlyDictionary<int, MovementObject> Movements => movementsObj;

    public void InitialiseObjects()
    {
        InitialiseMovements();
    }

    private void InitialiseMovements()
    {
        foreach (MovementObject movement in movements)
        {
            if (movement.Frames.Count == 2)
            {
                int startFrame = movement.Frames[0];
                int endFrame = movement.Frames[1];

                for (int i = startFrame; i <= endFrame; i++)
                {
                    movementsObj.Add(i, movement);
                }
                movement.SetTotalFrames(endFrame - startFrame + 1);
            }
            else
            {
                movementsObj.Add(movement.Frames[0], movement);
                movement.SetTotalFrames(1);
            }
            movement.InitialiseVector();
        }
        Debug.Log($"ID: {id}");
        foreach (var pair in movementsObj)
        {
            Debug.Log($"Frame: {pair.Key}");
            Debug.Log($"dx: {pair.Value.Dx}, dy: {pair.Value.Dy}, dz: {pair.Value.Dz}");
            Debug.Log($"Total Frames: {pair.Value.TotalFrames}");
            Debug.Log($"Vector3: {pair.Value.Vector}");
        }
    }
}