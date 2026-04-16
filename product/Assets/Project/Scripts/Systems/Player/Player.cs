using System.Collections.Generic;
using UnityEngine;

// The main player class - implements ICollidable for the collision system
// Owns all per-player managers and runs the per-frame update pipeline
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

    // Per-player systems
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

    // Hit reaction state
    private int stunTimer = 0;
    private bool isStunned = false;
    private bool isKO = false;
    private bool koFalling = false;       // True during the falling-ko animation
    private int koFallTimer = 0;          // Frames remaining in the fall animation
    private bool inputLocked = false;     // Set by GameManager during countdown/KO
    private Vector3 spawnPosition;        // Saved on start() for round resets
    private Quaternion spawnRotation;

    public int PlayerId => playerId;
    public bool IsKO => isKO;

    // --- ICollidable implementation ---

    public List<CollisionBox> GetActiveHitboxes()
    {
        return collisionBoxManager.GetActiveHitboxes();
    }

    public CombatHitboxEntry GetActiveHitboxEntry(string hitboxId)
    {
        return combatManager.GetActiveHitboxEntry(hitboxId);
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

    // Return combat data for the current move (null if no attack is active)
    public CombatData GetCombatData()
    {
        MoveData currentMove = moveExecutor.CurrentMove;
        if (currentMove == null) return null;
        return combatManager.GetCombatDatabase().GetCombatData(currentMove.Id);
    }

    public bool HasState(PlayerStates state)
    {
        return stateManager.HasState(state);
    }

    // Lock/unlock input (used during countdown, KO pause, etc.)
    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    // Called by CombatExecutor when this player gets hit - apply damage, stun, or KO
    public void ReceiveCombatResult(CombatResult result)
    {
        if (result.Outcome == HitOutcome.Whiff) return;

        healthManager.TakeDamage(result.Damage);

        if (healthManager.IsDead)
        {
            EnterKO();
            return;
        }

        // Apply stun frames (both blocked and normal hits use block-flinch animation for now)
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

    // Trigger KO state: cancel current move, play falling animation, then loop KO animation
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

        koFallTimer = animationManager.GetAnimationFrames("falling-ko");
        animationManager.PlayAnimation("falling-ko");

        Debug.Log($"[Player P{playerId}] KO!");
    }

    // Enter stun: cancel move, play stun animation, count down frames
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

    // Exit stun: return to Idle state
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

    // Manual initialisation (called by GameManager, not Unity's Start)
    public void start()
    {
        // Save spawn position for round resets
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        // Create all per-player systems
        animationManager = new AnimationManager(animationExecutor);
        movementManager = new MovementManager(movementExecutor);
        moveExecutor = new MoveExecutor(stateManager, animationManager, movementManager);
        moveSelector = new MoveSelector(inputBuffer, movesManager.GetMovesDatabase(),
                                        stateManager, moveExecutor);

        // Start in Idle state
        stateManager.AddState(PlayerStates.Idle);

        // Load all JSON data
        animationManager.LoadAnimations();
        movesManager.LoadMoves(animationManager.GetAnimationDatabase());
        movementManager.LoadMovements();

        // Create collision boxes on bones and body collider
        collisionBoxManager = new CollisionBoxManager(stateManager);
        collisionBoxManager.Load(body, GetBones());

        // Create combat manager and load combat data
        combatManager = new CombatManager(moveExecutor, collisionBoxManager);
        combatManager.LoadCombat();

        healthManager = new HealthManager(100);
    }

    // Reset everything for a new round - position, health, states, moves, hitboxes
    public void ResetForRound()
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        healthManager.Reset();

        isKO = false;
        koFalling = false;
        koFallTimer = 0;
        isStunned = false;
        stunTimer = 0;
        inputLocked = false;

        if (moveExecutor.CurrentMove != null)
        {
            moveExecutor.CancelMove();
        }

        stateManager.ResetState();
        stateManager.AddState(PlayerStates.Idle);

        inputBuffer.Clear();

        combatManager.DeactivateAll();

        animationManager.PlayAnimation("idle");

        Debug.Log($"[Player P{playerId}] Reset for new round");
    }

    // Main per-frame update - called by GameManager inside the fixed timestep loop
    public void update()
    {
        // KO: count down fall animation, then switch to KO loop
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
            return; // No other updates during KO
        }

        // Stunned: count down stun timer, then exit stun
        if (isStunned)
        {
            stunTimer--;
            if (stunTimer <= 0)
            {
                ExitStun();
            }
            return; // No other updates during stun
        }

        // Input locked (countdown, etc.): only run fallback move so idle animation plays
        if (inputLocked)
        {
            moveSelector.FallbackMove();
            moveExecutor.Update();
            return;
        }

        // Normal update pipeline
        moveSelector.Update();           // Read inputs, pick moves
        moveExecutor.Update();           // Advance current move, play animations
        combatManager.Update();          // Activate/deactivate hitboxes per frame
        movementExecutor.update(moveExecutor.FrameCounter);  // Apply movement velocity
        collisionBoxManager.Update();    // Handle standing/crouching collision transitions
        inputBuffer.UpdateFrameCounter();// Age all buffered inputs
        inputBuffer.RemoveExpiredInputs();// Clean up old inputs
    }

    // Build a dictionary mapping bone names to their transforms (used by CollisionBoxDatabase)
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

    // Rotate to face the opponent on the horizontal plane and update facing direction for input normalization
    public void FaceOpponent(Transform opponent)
    {
        if (isKO) return;

        Vector3 direction = opponent.position - transform.position;
        direction.y = 0; // Only rotate horizontally

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);

            // Update facing direction so input normalization (Forward/Backward) stays correct
            if (direction.x >= 0)
                stateManager.SetFacingDirection(FacingDirection.Right);
            else
                stateManager.SetFacingDirection(FacingDirection.Left);
        }
    }

    // Draw collision boxes in the editor scene view for debugging
    void OnDrawGizmos()
    {
        if (collisionBoxManager != null)
        {
            collisionBoxManager.OnDrawGizmos();
        }
    }
}