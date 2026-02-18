using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICollidable
{
    [Header("Bone References")]
    [SerializeField] private Transform body;
    [SerializeField] private Transform headBone;
    [SerializeField] private Transform bodyBone;
    [SerializeField] private Transform rightArmUpperBone;
    [SerializeField] private Transform rightArmLowerBone;
    [SerializeField] private Transform rightFistBone;
    [SerializeField] private Transform leftArmUpperBone;
    [SerializeField] private Transform leftArmLowerBone;
    [SerializeField] private Transform leftFistBone;
    [SerializeField] private Transform rightLegUpperBone;
    [SerializeField] private Transform rightLegLowerBone;
    [SerializeField] private Transform rightFootBone;
    [SerializeField] private Transform leftLegUpperBone;
    [SerializeField] private Transform leftLegLowerBone;
    [SerializeField] private Transform leftFootBone;

    [Header("Executors")]
    [SerializeField] AnimationExecutor animationExecutor;
    [SerializeField] MovementExecutor movementExecutor;

    [Header("Player Info")]
    [SerializeField] private int playerId;

    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private CollisionBoxManager collisionBoxManager;
    private AnimationManager animationManager;
    private MovementManager movementManager;
    private MoveSelector moveSelector;
    private MoveExecutor moveExecutor;

    public int PlayerId => playerId;

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

        collisionBoxManager = new CollisionBoxManager(stateManager);
        collisionBoxManager.Load(body, GetBones());
    }

    public void update()
    {
        moveSelector.Update();
        moveExecutor.Update();
        movementExecutor.update(moveExecutor.FrameCounter);
        collisionBoxManager.Update();
        inputBuffer.UpdateFrameCounter();
        inputBuffer.RemoveExpiredInputs();
    }

    private Dictionary<string, Transform> GetBones()
    {
        return new Dictionary<string, Transform>
        {
            { "head", headBone },
            { "body", bodyBone },
            { "rightArmUpper", rightArmUpperBone },
            { "rightArmLower", rightArmLowerBone },
            { "rightFist", rightFistBone },
            { "leftArmUpper", leftArmUpperBone },
            { "leftArmLower", leftArmLowerBone },
            { "leftFist", leftFistBone },
            { "rightLegUpper", rightLegUpperBone },
            { "rightLegLower", rightLegLowerBone },
            { "rightFoot", rightFootBone },
            { "leftLegUpper", leftLegUpperBone },
            { "leftLegLower", leftLegLowerBone },
            { "leftFoot", leftFootBone }
        };
    }

    public List<CollisionBox> GetActiveHitboxes()
    {
        return collisionBoxManager.GetActiveHitboxes();
    }

    public IEnumerable<CollisionBox> GetAllHurtboxes()
    {
        return collisionBoxManager.GetAllHurtboxes();
    }

    public CollisionBox GetCollisionBox(string id)
    {
        return collisionBoxManager.GetCollisionBox(id);
    }

    public BodyCollider GetBodyCollider()
    {
        return collisionBoxManager.GetBodyCollider();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void SetMovementBlocked(bool blocked)
    {
        // movementExecutor.SetBlocked(blocked);
    }

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public MoveSelector GetMoveSelector()
    {
        return moveSelector;
    }

    void OnDrawGizmos()
    {
        if (collisionBoxManager != null)
        {
            collisionBoxManager.OnDrawGizmos();
        }
    }
}