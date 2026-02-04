using UnityEngine;

public class FocusCameraMode : ICameraMode
{
    // Camera info
    private bool focusOnPlayer1;                // Camera focus on p1 or p2
    private Vector3 offset;                     // Relative to players forward direction
    private float height;
    private float aimHeight;
    private float smoothSpeed;

    // Local Trackers
    private Vector3 currentPosition;
    private Vector3 positionVelocity = Vector3.zero;
    private bool initialised = false;

    public FocusCameraMode(bool focusOnPlayer1, Vector3 offset, float height, 
                           float aimHeight, float smoothSpeed = 5f)
    {
        this.focusOnPlayer1 = focusOnPlayer1;
        this.offset = offset;
        this.height = height;
        this.aimHeight = aimHeight;
        this.smoothSpeed = smoothSpeed;
    }

    public void Enter()
    {
        initialised = false;
    }

    public void Exit() { }

    public void Update(CameraManager cameraManager)
    {
        // Labels p1 and p2 based on camera focus
        Transform focus;
        Transform other;

        if (focusOnPlayer1)
        {
            focus = cameraManager.Player1;
            other = cameraManager.Player2;
        }
        else
        {
            focus = cameraManager.Player2;
            other = cameraManager.Player1;
        }

        // Direction of focused player
        Vector3 forward = focus.forward;
        Vector3 right = focus.right;

        // Calculate target position relative to focused player
        Vector3 targetPosition = focus.position
            + (forward * offset.z)
            + (right * offset.x)
            + (Vector3.up * height);

        // Frame 0 => Snap to position
        if (!initialised)
        {
            currentPosition = targetPosition;
            initialised = true;
        }
        // Frame 1+ => Apply smoothing
        else
        {
            currentPosition = Vector3.SmoothDamp(
                currentPosition,
                targetPosition,
                ref positionVelocity,
                1f / smoothSpeed
            );
        }

        // Apply new position
        Transform cam = cameraManager.CameraTransform;
        cam.position = currentPosition;

        // Apply rotation
        Vector3 lookAtPoint = focus.position + (Vector3.up * aimHeight);
        cam.rotation = Quaternion.LookRotation(lookAtPoint - cam.position);
    }
}