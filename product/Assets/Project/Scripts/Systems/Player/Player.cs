using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] AnimationExecutor animationExecutor;
    [SerializeField] MovementExecutor movementExecutor;
    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private AnimationManager animationManager;
    private MovementManager movementManager;
    private MoveSelector moveSelector;
    private MoveExecutor moveExecutor;

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public MoveSelector GetMoveSelector()
    {
        return moveSelector;
    }

    public void start()
    {
        animationManager = new AnimationManager(animationExecutor);
        movementManager = new MovementManager(movementExecutor);
        moveExecutor = new MoveExecutor(stateManager, animationManager, movementManager);
        moveSelector = new MoveSelector(inputBuffer, movesManager.GetMovesDatabase(),
                                        stateManager, moveExecutor);

        stateManager.AddState(PlayerStates.Idle);
        animationManager.LoadAnimations();
        movesManager.LoadMoves(animationManager.GetAnimationDatabase());
        movementManager.LoadMovements();
    }

    public void update()
    {
        moveSelector.Update();
        moveExecutor.Update();
        movementExecutor.update(moveExecutor.FrameCounter);
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
    }
}