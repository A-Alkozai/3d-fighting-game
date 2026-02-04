using UnityEngine;

public class CameraManager
{
    private Camera camera;
    private Transform cameraTransform;
    private Transform player1;
    private Transform player2;
    private ICameraMode activeMode;
    private ICameraMode defaultMode = new CombatCameraMode();

    public Transform Player1 => player1;
    public Transform Player2 => player2;
    public Transform CameraTransform => cameraTransform;
    public Camera Camera => camera;

    public CameraManager(Camera camera, Transform player1, Transform player2)
    {
        this.camera = camera;
        this.cameraTransform = camera.transform;
        this.player1 = player1;
        this.player2 = player2;
    }

    public void SetMode(ICameraMode newMode)
    {
        if (activeMode != null)
        {
            activeMode.Exit();
        }
        activeMode = newMode;
        activeMode.Enter();
    }

    public void SetDefaultMode()
    {
        SetMode(defaultMode);
    }

    public void Update()
    {
        if (activeMode == null)
        {
            activeMode = defaultMode;
        }

        activeMode.Update(this);
    }
}