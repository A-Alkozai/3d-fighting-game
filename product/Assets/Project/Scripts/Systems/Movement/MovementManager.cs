// Bridge between MoveExecutor and MovementExecutor - looks up movement data by move ID
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

    // Set the active movement pattern by move ID (e.g. "walk-forwards", "single-jab")
    public void SetMovement(string id)
    {
        MovementData movementData = movementDatabase.GetMovementData(id);
        movementExecutor.SetMovement(movementData);
    }

    // Clear the active movement (player stops moving)
    public void CancelMovement()
    {
        movementExecutor.SetMovement(null);
    }
}