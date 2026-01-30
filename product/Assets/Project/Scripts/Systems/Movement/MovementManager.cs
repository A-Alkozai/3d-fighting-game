public class MovementManager
{
    private MovementDatabase movementDatabase = new MovementDatabase();
    private MovementExecutor movementExecutor;


    public MovementManager(MovementExecutor movementExecutor)
    {
        this.movementExecutor = movementExecutor;
    }

    public void LoadMovements()
    {
        movementDatabase.ReadJson();
    }

    public void SetMovement(string id)
    {
        MovementData movementData = movementDatabase.GetMovementData(id);
        movementExecutor.SetMovement(movementData);
    }

    public void CancelMovement()
    {
        movementExecutor.SetMovement(null);
    }
}