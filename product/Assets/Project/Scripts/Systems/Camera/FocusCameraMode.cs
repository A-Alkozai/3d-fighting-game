using UnityEngine;

// Camera mode that follows one specific player, positioned relative to their facing direction
public class FocusCameraMode : ICameraMode
{
    private bool focusOnPlayer1;     // True = follow P1, false = follow P2
    private Vector3 offset;          // Position offset relative to the focused player's forward/right
    private float height;
    private float aimHeight;         // Height on the player to look at
    private float smoothSpeed;

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
        // Pick which player to focus on and which is the other
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

        Vector3 forward = focus.forward;
        Vector3 right = focus.right;

        // Build target position using the focused player's local axes
        Vector3 targetPosition = focus.position
            + (forward * offset.z)
            + (right * offset.x)
            + (Vector3.up * height);

        // First frame: snap, subsequent frames: smooth
        if (!initialised)
        {
            currentPosition = targetPosition;
            initialised = true;
        }
        else
        {
            currentPosition = Vector3.SmoothDamp(
                currentPosition,
                targetPosition,
                ref positionVelocity,
                1f / smoothSpeed
            );
        }

        Transform cam = cameraManager.CameraTransform;
        cam.position = currentPosition;

        // Always look at the focused player at aim height
        Vector3 lookAtPoint = focus.position + (Vector3.up * aimHeight);
        cam.rotation = Quaternion.LookRotation(lookAtPoint - cam.position);
    }
}