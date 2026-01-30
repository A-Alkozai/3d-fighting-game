using System.Collections.Generic;
using UnityEngine;

public class MovementExecutor : MonoBehaviour
{
    private MovementData currentMovement;
    private Transform transform;
    // private Vector3 vector;

    void Awake()
    {
        transform = GetComponent<Transform>();
    }

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