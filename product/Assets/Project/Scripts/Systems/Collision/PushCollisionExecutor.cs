public class PushCollisionExecutor
{
    public void Execute(ICollidable entityA, ICollidable entityB, bool overlapping)
    {
        entityA.SetMovementBlocked(overlapping);
        entityB.SetMovementBlocked(overlapping);
    }
}