using UnityEngine;

// Default camera mode - positions camera to the side of both players, tracking their midpoint
public class CombatCameraMode : ICameraMode
{
    // How far the camera sits from the players
    private float minDistance = 2.5f;
    private float maxDistance = 6f;
    private float distanceMultiplier = 1.2f;

    // Camera vertical positioning
    private float height = 1.5f;          // Camera height offset above midpoint
    private float targetHeight = 1.0f;     // Height the camera looks at (slightly lower than camera)
    private float cameraSide = -1f;        // Which side of the fight line the camera sits on

    // Smoothing speeds (higher smoothTime = slower/smoother transitions)
    private float zoomSmoothTime = 0.5f;
    private float rotationSmoothSpeed = 50f;
    private float directionSmoothTime = 0.5f;
    private float midpointSmoothTime = 0.3f;

    // Internal tracking values
    private bool initialised = false;
    private float currentDistance;
    private Vector3 currentDirection;
    private Vector3 currentMidpoint;
    private Vector3 lockedDirection;       // Prevents camera flipping when players cross sides

    // SmoothDamp velocity refs (required by Unity's SmoothDamp, not manually set)
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

        // Calculate the point between both players and the line connecting them
        Vector3 midpoint = (p1.position + p2.position) / 2f;
        Vector3 fightDirection = (p2.position - p1.position);
        float playerDistance = fightDirection.magnitude;
        fightDirection.Normalize();

        // Camera distance scales with player separation, clamped to min/max
        float targetDistance = Mathf.Clamp(
            playerDistance * distanceMultiplier,
            minDistance,
            maxDistance
        );

        // Camera sits perpendicular to the line between players
        Vector3 targetDirection = Vector3.Cross(
            fightDirection, Vector3.up).normalized;
        targetDirection *= cameraSide;

        // If players cross each other, keep the camera on the same side to avoid flipping
        if (initialised && Vector3.Dot(targetDirection, lockedDirection) < 0f)
        {
            targetDirection = -targetDirection;
        }

        // First frame: snap to target values with no smoothing
        if (!initialised)
        {
            currentMidpoint = midpoint;
            currentDistance = targetDistance;
            currentDirection = targetDirection;
            lockedDirection = targetDirection;
            initialised = true;
        }
        // Subsequent frames: smooth all values for fluid camera movement
        else
        {
            currentMidpoint = Vector3.SmoothDamp(
                currentMidpoint,
                midpoint,
                ref midpointVelocity,
                midpointSmoothTime
            );

            currentDistance = Mathf.SmoothDamp(
                currentDistance,
                targetDistance,
                ref distanceVelocity,
                zoomSmoothTime
            );

            currentDirection = Vector3.SmoothDamp(
                currentDirection,
                targetDirection,
                ref directionVelocity,
                directionSmoothTime
            );
            currentDirection.Normalize();
        }

        // Position the camera: midpoint + offset in perpendicular direction + height
        cam.position = currentMidpoint
            + (currentDirection * currentDistance)
            + (Vector3.up * height);

        // Rotate camera to look at the midpoint (at target height)
        Vector3 lookAtPoint = currentMidpoint + (Vector3.up * targetHeight);
        Vector3 lookDirection = lookAtPoint - cam.position;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        cam.rotation = Quaternion.Slerp(
            cam.rotation,
            targetRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}