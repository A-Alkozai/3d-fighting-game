// Interface for camera behaviour modes - allows swapping between combat view, focus view, etc.
public interface ICameraMode
{
    void Enter();
    void Exit();
    void Update(CameraManager cameraManager);
}