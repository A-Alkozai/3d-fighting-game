using UnityEngine;

public class CombatCameraMode : ICameraMode
{
    // Camera distance from players
    private float minDistance = 3f;
    private float maxDistance = 8f;
    private float distanceMultiplier = 1.2f;

    // Camera info
    private float height = 2f;
    private float targetHeight = 1.0f;
    private float cameraSide = -1f;

    // Smoothing times (higher = smoother)
    private float zoomSmoothTime = 0.5f;
    private float rotationSmoothSpeed = 5f;
    private float directionSmoothTime = 0.5f;
    private float midpointSmoothTime = 0.3f;

    // Local Trackers
    private bool initialised = false;
    private float currentDistance;
    private Vector3 currentDirection;
    private Vector3 currentMidpoint;
    private Vector3 lockedDirection;

    // Camera Velocity (for SmoothDamp)
    private float distanceVelocity = 0f;
    private Vector3 directionVelocity = Vector3.zero;
    private Vector3 midpointVelocity = Vector3.zero;

    public void Enter()
    {
        initialised = false;
    }

    public void Exit() { }

    public void Update(CameraManager cameraManager)
    {
        Transform p1 = cameraManager.Player1;
        Transform p2 = cameraManager.Player2;
        Transform cam = cameraManager.CameraTransform;

        // Raw values
        Vector3 midpoint = (p1.position + p2.position) / 2f;
        Vector3 fightDirection = (p2.position - p1.position);
        float playerDistance = fightDirection.magnitude;
        fightDirection.Normalize();

        // Target distance
        float targetDistance = Mathf.Clamp(             // Clamp ensures value is between min and max
            playerDistance * distanceMultiplier,
            minDistance,
            maxDistance
        );

        // Target direction
        Vector3 targetDirection = Vector3.Cross(        // Gives perpendicular horizontal vector of p1 => p2
            fightDirection, Vector3.up).normalized;
        targetDirection *= cameraSide;                  // Camera side determines vector direction

        // Prevent camera flip when players cross
        if (initialised && Vector3.Dot(targetDirection, lockedDirection) < 0f)
        {
            targetDirection = -targetDirection;
        }

        // Frame 0 => no smoothing
        if (!initialised)
        {
            currentMidpoint = midpoint;
            currentDistance = targetDistance;
            currentDirection = targetDirection;
            lockedDirection = targetDirection;
            initialised = true;
        }
        // Frame 1+ => Apply smoothing
        else
        {
            // Midpoint => when players move
            currentMidpoint = Vector3.SmoothDamp(
                currentMidpoint,
                midpoint,
                ref midpointVelocity,
                midpointSmoothTime
            );

            // Camera Distance => When players move
            currentDistance = Mathf.SmoothDamp(
                currentDistance,
                targetDistance,
                ref distanceVelocity,
                zoomSmoothTime
            );

            // Camera Direction => When players rotate
            currentDirection = Vector3.SmoothDamp(
                currentDirection,
                targetDirection,
                ref directionVelocity,
                directionSmoothTime
            );
            currentDirection.Normalize();
        }

        // Apply final position
        cam.position = currentMidpoint
            + (currentDirection * currentDistance)
            + (Vector3.up * height);

        // Apply final rotation
        Vector3 lookAtPoint = currentMidpoint + (Vector3.up * targetHeight);       // Vector of targetHeight from midpoint
        Vector3 lookDirection = lookAtPoint - cam.position;                        // Vector direction of camera => lookAtPoint

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);        // Gives rotation that faces lookDirection
        cam.rotation = Quaternion.Slerp(
            cam.rotation,                                                          // Where camera is facing
            targetRotation,                                                        // Where camera should face
            rotationSmoothSpeed * Time.deltaTime                                   // How much to blend (0.0 - 1.0)
        );
    }
}