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

    [Header("Player Settings")]
    [SerializeField] private int playerId;

    private InputBuffer inputBuffer = new InputBuffer();
    private MovesManager movesManager = new MovesManager();
    private StateManager stateManager = new StateManager();
    private CollisionBoxManager collisionBoxManager;
    private AnimationManager animationManager;
    private MovementManager movementManager;
    private HealthManager healthManager;
    private CombatManager combatManager;  
    private MoveSelector moveSelector;
    private MoveExecutor moveExecutor;

    private int stunTimer = 0;
    private bool isStunned = false;
    private bool isKO = false;
    private bool koFalling = false;
    private int koFallTimer = 0;

    public int PlayerId => playerId;

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

    public CombatData GetCombatData()
    {
        MoveData currentMove = moveExecutor.CurrentMove;  // ← property, not method
        if (currentMove == null) return null;
        return combatManager.GetCombatDatabase().GetCombatData(currentMove.Id);
    }

    public bool HasState(PlayerStates state)
    {
        return stateManager.HasState(state);
    }

    public void ReceiveCombatResult(CombatResult result)
    {
        if (result.Outcome == HitOutcome.Whiff) return;

        healthManager.TakeDamage(result.Damage);

        if (healthManager.IsDead)
        {
            EnterKO();
            return;
        }

        if (result.StunFrames > 0)
        {
            if (result.Outcome == HitOutcome.Blocked)
            {
                EnterStun(result.StunFrames, "block-flinch");
            }
            else
            {
                EnterStun(result.StunFrames, "block-flinch");
            }
        }

        Debug.Log($"[Player P{playerId}] {result.Outcome} | " +
                $"Damage: {result.Damage} | Stun: {result.StunFrames} | " +
                $"HP: {healthManager.CurrentHealth}");
    }

    private void EnterKO()
    {
        if (moveExecutor.CurrentMove != null)
        {
            moveExecutor.CancelMove();
        }

        isStunned = false;
        stunTimer = 0;
        isKO = true;
        koFalling = true;

        stateManager.ResetState();
        stateManager.AddState(PlayerStates.KO);

        // Get falling-ko animation length in frames for timer
        koFallTimer = animationManager.GetAnimationFrames("falling-ko");
        animationManager.PlayAnimation("falling-ko");

        Debug.Log($"[Player P{playerId}] KO!");
    }

    private void EnterStun(int frames, string animationId)
    {
        if (moveExecutor.CurrentMove != null)
        {
            moveExecutor.CancelMove();
        }

        stunTimer = frames;
        isStunned = true;

        stateManager.ResetState();
        stateManager.AddState(PlayerStates.Stunned);

        animationManager.PlayAnimation(animationId);

        Debug.Log($"[Player P{playerId}] Entering stun for {frames} frames ({animationId})");
    }

    private void ExitStun()
    {
        isStunned = false;
        stunTimer = 0;

        stateManager.RemoveState(PlayerStates.Stunned);
        stateManager.AddState(PlayerStates.Idle);

        Debug.Log($"[Player P{playerId}] Exiting stun");
    }

    public string GetCurrentMoveId()
    {
        return moveExecutor.CurrentMove?.Id;
    }

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public MoveSelector GetMoveSelector()
    {
        return moveSelector;
    }

    public HealthManager GetHealthManager()
    {
        return healthManager;
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
        collisionBoxManager = new CollisionBoxManager(stateManager);
        collisionBoxManager.Load(body, GetBones());
        combatManager = new CombatManager(moveExecutor, collisionBoxManager);
        combatManager.LoadCombat();
        healthManager = new HealthManager(100);
    }

    public void update()
    {
        if (isKO)
        {
            if (koFalling)
            {
                koFallTimer--;
                if (koFallTimer <= 0)
                {
                    koFalling = false;
                    animationManager.PlayAnimation("ko");
                }
            }
            return;
        }

        if (isStunned)
        {
            stunTimer--;
            if (stunTimer <= 0)
            {
                ExitStun();
            }
            return;
        }

        // Normal update
        moveSelector.Update();
        moveExecutor.Update();
        combatManager.Update();
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

    void OnDrawGizmos()
    {
        if (collisionBoxManager != null)
        {
            collisionBoxManager.OnDrawGizmos();
        }
    }
}