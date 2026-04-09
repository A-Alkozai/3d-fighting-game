using System;
using System.Collections.Generic;
using UnityEngine;

// JSON data for a movement pattern - contains a list of per-frame velocity entries
[Serializable]
public class MovementData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private List<MovementObject> movements;
    private Dictionary<int, MovementObject> movementsObj = new Dictionary<int, MovementObject>(); // Frame → movement lookup

    public string Id => id;
    public IReadOnlyDictionary<int, MovementObject> Movements => movementsObj;

    public void InitialiseObjects()
    {
        InitialiseMovements();
    }

    // Convert the list of MovementObjects into a per-frame dictionary
    // If a movement has two frame values, it spans that range (e.g. [5, 15] = frames 5 through 15)
    private void InitialiseMovements()
    {
        foreach (MovementObject movement in movements)
        {
            if (movement.Frames.Count == 2)
            {
                int startFrame = movement.Frames[0];
                int endFrame = movement.Frames[1];

                // Register this movement for every frame in the range
                for (int i = startFrame; i <= endFrame; i++)
                {
                    movementsObj.Add(i, movement);
                }
                movement.SetTotalFrames(endFrame - startFrame + 1);
            }
            else
            {
                // Single-frame movement
                movementsObj.Add(movement.Frames[0], movement);
                movement.SetTotalFrames(1);
            }
            // Pre-calculate the per-frame velocity vector
            movement.InitialiseVector();
        }
    }
}