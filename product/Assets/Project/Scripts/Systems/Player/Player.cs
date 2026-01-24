using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] MoveAnimator moveAnimator;
    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private AnimationManager animationManager;
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
        animationManager = new AnimationManager(moveAnimator);
        moveExecutor = new MoveExecutor(stateManager, animationManager);
        moveSelector = new MoveSelector(inputBuffer, movesManager.GetMovesDatabase(),
                                        stateManager, moveExecutor);
                                        
        stateManager.AddState(PlayerStates.Idle);
        animationManager.LoadAnimations();
        movesManager.LoadMoves(animationManager.GetAnimationDatabase());
    }

    public void update()
    {
        moveSelector.Update();
        moveExecutor.Update();
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
    }
}