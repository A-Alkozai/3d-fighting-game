#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

public class ReportSectionPlayModeIntegrationTests
{
    [UnityTest]
    public IEnumerator INT_01_ValidInputSequenceSubmittedThroughInputManager_SelectsExpectedMoveId()
    {
        GameObject inputManagerObject = new GameObject("InputManager");
        GameObject playerObject = new GameObject("InputPipelinePlayer");

        try
        {
            InputManager inputManager = inputManagerObject.AddComponent<InputManager>();
            Player player = playerObject.AddComponent<Player>();

            InputBuffer inputBuffer = new InputBuffer();
            StateManager stateManager = new StateManager();
            stateManager.AddState(PlayerStates.Idle);

            MoveData idleMove = CreateMoveData(
                "idle",
                "state",
                new List<string>(),
                new List<string> { "Idle" }
            );
            MoveData specialMove = CreateMoveData(
                "dragon-punch",
                "attack",
                new List<string> { "Forward", "LeftPunch" },
                new List<string> { "Attacking" }
            );

            MovesDatabase movesDatabase = new MovesDatabase();
            SetField(movesDatabase, "dict", new Dictionary<string, MoveData>
            {
                ["idle"] = idleMove,
                ["dragon-punch"] = specialMove
            });
            movesDatabase.InitialiseTrees();

            MoveExecutor moveExecutor = new MoveExecutor(
                stateManager,
                new AnimationManager(null),
                new MovementManager(null)
            );
            MoveSelector moveSelector = new MoveSelector(inputBuffer, movesDatabase, stateManager, moveExecutor);
            moveSelector.FallbackMove();

            SetField(player, "inputBuffer", inputBuffer);
            SetField(player, "moveSelector", moveSelector);
            SetField(player, "playerId", 1);

            QueueInputProvider provider = new QueueInputProvider(new List<InputObject>
            {
                new InputObject(InputCommand.Forward, Key.D),
                new InputObject(InputCommand.LeftPunch, Key.I)
            });

            inputManager.AddInputToPlayerMap(provider, player);
            inputManager.update();
            moveSelector.Update();

            Assert.That(moveExecutor.CurrentMove, Is.Not.Null);
            Assert.That(moveExecutor.CurrentMove.Id, Is.EqualTo("dragon-punch"));
        }
        finally
        {
           Object.DestroyImmediate(inputManagerObject);
            Object.DestroyImmediate(playerObject);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator INT_02_SyntheticHitPayloadSubmittedThroughHitCollisionExecutor_ReducesDefenderHealthCorrectly()
    {
        GameObject defenderObject = new GameObject("CombatPipelineDefender");

        try
        {
            Player defender = defenderObject.AddComponent<Player>();
            SetField(defender, "playerId", 2);
            SetField(defender, "healthManager", new HealthManager(100));

            CombatHitboxEntry entry = CreateCombatHitboxEntry(
                "rightFist",
                "Mid",
                damage: 12,
                counterHitDamage: 18,
                hitStunFrames: 0,
                blockStunFrames: 0,
                hitEffect: "None",
                counterHitEffect: "None"
            );
            CombatData combatData = CreateCombatData("integration-hit", true, new List<CombatHitboxEntry> { entry });

            CollisionBox hitbox = CreateCollisionBoxWithId("rightFist");
            CollisionBox hurtbox = CreateCollisionBoxWithId("torso");
            IntegrationCombatAttacker attacker = new IntegrationCombatAttacker(1, combatData);

            HitCollisionData payload = new HitCollisionData(attacker, defender, hitbox, hurtbox, entry);
            HitCollisionExecutor executor = new HitCollisionExecutor(new CombatExecutor());

            executor.Execute(payload);

            Assert.That(defender.GetHealthManager().CurrentHealth, Is.EqualTo(88));
        }
        finally
        {
            Object.DestroyImmediate(defenderObject);
        }

        yield return null;
    }

    private sealed class QueueInputProvider : IInputProvider
    {
        private readonly Queue<List<InputObject>> batches = new Queue<List<InputObject>>();

        public QueueInputProvider(List<InputObject> initialBatch)
        {
            batches.Enqueue(initialBatch);
        }

        public List<InputObject> GetInputs()
        {
            if (batches.Count == 0)
            {
                return new List<InputObject>();
            }

            return batches.Dequeue();
        }
    }

    private sealed class IntegrationCombatAttacker : ICollidable
    {
        private readonly int playerId;
        private readonly CombatData combatData;

        public IntegrationCombatAttacker(int playerId, CombatData combatData)
        {
            this.playerId = playerId;
            this.combatData = combatData;
        }

        public int PlayerId => playerId;

        public List<CollisionBox> GetActiveHitboxes()
        {
            return new List<CollisionBox>();
        }

        public IEnumerable<CollisionBox> GetAllHurtboxes()
        {
            return new List<CollisionBox>();
        }

        public CollisionBox GetCollisionBox(string id)
        {
            return null;
        }

        public BodyCollider GetBodyCollider()
        {
            return null;
        }

        public Transform GetTransform()
        {
            return null;
        }

        public CombatData GetCombatData()
        {
            return combatData;
        }

        public CombatHitboxEntry GetActiveHitboxEntry(string hitboxId)
        {
            return null;
        }

        public bool HasState(PlayerStates state)
        {
            return false;
        }

        public void ReceiveCombatResult(CombatResult result)
        {
        }

        public string GetCurrentMoveId()
        {
            return "integration-hit";
        }
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = FindField(target.GetType(), fieldName);
        Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    private static FieldInfo FindField(System.Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static T CreateObject<T>(Dictionary<string, object> fieldValues) where T : new()
    {
        T instance = new T();
        foreach (KeyValuePair<string, object> pair in fieldValues)
        {
            SetField(instance, pair.Key, pair.Value);
        }

        return instance;
    }

    private static MoveData CreateMoveData(string id, string moveType, List<string> inputSequence, List<string> requiredStates)
    {
        MoveData move = CreateObject<MoveData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["moveName"] = id,
            ["description"] = $"{id} description",
            ["moveType"] = moveType,
            ["isLoop"] = false,
            ["inputDelay"] = 0,
            ["branchDelay"] = 0,
            ["inputSequence"] = inputSequence,
            ["requiredStates"] = requiredStates
        });
        move.InitialiseObjects();
        return move;
    }

    private static CombatData CreateCombatData(string id, bool blockable, List<CombatHitboxEntry> entries)
    {
        return CreateObject<CombatData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["blockable"] = blockable,
            ["hitboxEntries"] = entries
        });
    }

    private static CombatHitboxEntry CreateCombatHitboxEntry(
        string hitboxId,
        string attackHeight,
        int damage,
        int counterHitDamage,
        int hitStunFrames,
        int blockStunFrames,
        string hitEffect,
        string counterHitEffect)
    {
        return CreateObject<CombatHitboxEntry>(new Dictionary<string, object>
        {
            ["hitboxId"] = hitboxId,
            ["startFrame"] = 1,
            ["endFrame"] = 3,
            ["sizeMultiplier"] = 1f,
            ["attackHeight"] = attackHeight,
            ["damage"] = damage,
            ["counterHitDamage"] = counterHitDamage,
            ["hitStunFrames"] = hitStunFrames,
            ["blockStunFrames"] = blockStunFrames,
            ["knockback"] = 1f,
            ["hitEffect"] = hitEffect,
            ["counterHitEffect"] = counterHitEffect
        });
    }

    private static CollisionBox CreateCollisionBoxWithId(string id)
    {
        CollisionBox collisionBox = (CollisionBox)FormatterServices.GetUninitializedObject(typeof(CollisionBox));
        SetField(collisionBox, "id", id);
        return collisionBox;
    }
}
#endif