using System.Collections.Generic;
using UnityEngine;

// Applies movement velocities to the player's transform each frame based on the current move's data
public class MovementExecutor : MonoBehaviour
{
    private MovementData currentMovement;
    private Transform transform;

    void Awake()
    {
        transform = GetComponent<Transform>();
    }

    // Called each logic frame - apply movement if one is set
    public void update(FrameCounter frameCounter)
    {
        if (currentMovement is not null)
        {
            PlayMovement(frameCounter);
        }
    }

    public void SetMovement(MovementData movementData)
    {
        currentMovement = movementData;
    }

    // Look up the current frame in the movement data and apply the velocity
    public void PlayMovement(FrameCounter frameCounter)
    {
        int currentFrame = frameCounter.GetFrameNumber();
        if (currentMovement.Movements.ContainsKey(currentFrame))
        {
            MovementObject movement = currentMovement.Movements[currentFrame];
            Vector3 vector = movement.Vector;
            transform.position += vector;
        }
    }
}