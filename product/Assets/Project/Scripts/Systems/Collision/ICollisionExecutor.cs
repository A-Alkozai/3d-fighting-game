// Generic interface for collision executors - takes collision data and processes it
public interface ICollisionExecutor<T>
{
    void Execute(T data);
}