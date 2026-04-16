using System.Collections.Generic;
using UnityEngine;

// Applies movement velocities to the player's transform each frame based on the current move's data
// Converts local movement (x=forward/back, y=up/down, z=left/right) to world-space using player rotation
public class MovementExecutor : MonoBehaviour
{
    private MovementData currentMovement;
    private Transform transform;

    void Awake()
    {
        transform = GetComponent<Transform>();
    }

    // Called each logic frame — apply movement if one is set
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
    // Movement data uses local axes: x = forward/backward, y = up/down, z = left/right (sidestep)
    // Convert to world-space using the player's current facing direction
    public void PlayMovement(FrameCounter frameCounter)
    {
        int currentFrame = frameCounter.GetFrameNumber();
        if (currentMovement.Movements.ContainsKey(currentFrame))
        {
            MovementObject movement = currentMovement.Movements[currentFrame];
            Vector3 localVector = movement.Vector;

            // Convert local movement to world-space:
            // localVector.x = forward/backward (along player's forward)
            // localVector.y = up/down (always world Y)
            // localVector.z = sidestep left/right (along player's right)
            Vector3 worldVector = (transform.forward * localVector.x)
                                + (Vector3.up * localVector.y)
                                + (transform.right * localVector.z);

            transform.position += worldVector;
        }
    }
}