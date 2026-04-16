#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public static class CodebaseTestHelpers
{
    public static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = FindField(target.GetType(), fieldName);
        Assert.NotNull(field, $"Field '{fieldName}' was not found on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    public static T GetField<T>(object target, string fieldName)
    {
        FieldInfo field = FindField(target.GetType(), fieldName);
        Assert.NotNull(field, $"Field '{fieldName}' was not found on {target.GetType().Name}.");
        return (T)field.GetValue(target);
    }

    public static object Invoke(object target, string methodName, params object[] args)
    {
        MethodInfo method = FindMethod(target.GetType(), methodName, args.Length);
        Assert.NotNull(method, $"Method '{methodName}' was not found on {target.GetType().Name}.");
        return method.Invoke(target, args);
    }

    public static T CreateInstance<T>(Dictionary<string, object> fieldValues = null) where T : new()
    {
        T instance = new T();
        if (fieldValues != null)
        {
            foreach (KeyValuePair<string, object> pair in fieldValues)
            {
                SetField(instance, pair.Key, pair.Value);
            }
        }

        return instance;
    }

    public static string WriteTempJson(string fileName, string json)
    {
        string dir = Path.Combine(Path.GetTempPath(), "codebase-tests");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    public static AnimationExecutor CreateAnimationExecutorWithClip(string clipName, float clipLengthSeconds)
    {
        string root = "Assets/__GeneratedCodebaseTests";
        EnsureEditorFolder(root);

        string clipPath = $"{root}/{Guid.NewGuid()}_{clipName}.anim";
        string controllerPath = $"{root}/{Guid.NewGuid()}_{clipName}.controller";

        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60f;
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"),
            AnimationCurve.Linear(0f, 0f, clipLengthSeconds, 1f)
        );
        AssetDatabase.CreateAsset(clip, clipPath);


        clip.name = clipName;
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddMotion(clip);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject go = new GameObject($"AnimationExecutor_{clipName}");
        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        AnimationExecutor executor = go.AddComponent<AnimationExecutor>();
        Invoke(executor, "Awake");
        return executor;
    }

    public static AnimationData MakeAnimationData(string id, string clip, bool isLoop, float speed)
    {
        return CreateInstance<AnimationData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["clip"] = clip,
            ["isLoop"] = isLoop,
            ["speed"] = speed
        });
    }

    public static CombatHitboxEntry MakeCombatHitboxEntry(
        string hitboxId = "rightFist",
        int startFrame = 1,
        int endFrame = 3,
        float sizeMultiplier = 1f,
        string attackHeight = "Mid",
        int damage = 10,
        int counterHitDamage = 15,
        int hitStunFrames = 12,
        int blockStunFrames = 6,
        float knockback = 1f,
        string hitEffect = "Hitstun",
        string counterHitEffect = "Launch")
    {
        return CreateInstance<CombatHitboxEntry>(new Dictionary<string, object>
        {
            ["hitboxId"] = hitboxId,
            ["startFrame"] = startFrame,
            ["endFrame"] = endFrame,
            ["sizeMultiplier"] = sizeMultiplier,
            ["attackHeight"] = attackHeight,
            ["damage"] = damage,
            ["counterHitDamage"] = counterHitDamage,
            ["hitStunFrames"] = hitStunFrames,
            ["blockStunFrames"] = blockStunFrames,
            ["knockback"] = knockback,
            ["hitEffect"] = hitEffect,
            ["counterHitEffect"] = counterHitEffect
        });
    }

    public static CombatData MakeCombatData(string id, bool blockable, List<CombatHitboxEntry> entries)
    {
        return CreateInstance<CombatData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["blockable"] = blockable,
            ["hitboxEntries"] = entries
        });
    }

    public static BodyColliderData MakeBodyColliderData(string id = "bodyCollider")
    {
        return CreateInstance<BodyColliderData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["size"] = new Vector3(1f, 2f, 1f),
            ["offset"] = Vector3.zero,
            ["center"] = Vector3.zero
        });
    }

    public static CollisionBoxData MakeCollisionBoxData(string id = "rightFist")
    {
        return CreateInstance<CollisionBoxData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["standingSize"] = new Vector3(1f, 1f, 1f),
            ["standingOffset"] = Vector3.zero,
            ["crouchingSize"] = new Vector3(0.5f, 0.5f, 0.5f),
            ["crouchingOffset"] = new Vector3(0f, -0.5f, 0f),
            ["resetRotation"] = true
        });
    }

    public static MovementObject MakeMovementObject(List<int> frames, float dx, float dy, float dz)
    {
        return CreateInstance<MovementObject>(new Dictionary<string, object>
        {
            ["frame"] = frames,
            ["dx"] = dx,
            ["dy"] = dy,
            ["dz"] = dz
        });
    }

    public static MovementData MakeMovementData(string id, List<MovementObject> movements)
    {
        MovementData data = CreateInstance<MovementData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["movements"] = movements
        });
        data.InitialiseObjects();
        return data;
    }

    public static MoveData MakeMoveData(
        string id,
        string moveType,
        List<string> inputSequence,
        List<string> requiredStates,
        bool isLoop = false,
        int inputDelay = 0,
        int branchDelay = 0)
    {
        MoveData move = CreateInstance<MoveData>(new Dictionary<string, object>
        {
            ["id"] = id,
            ["moveName"] = id,
            ["description"] = $"{id} description",
            ["moveType"] = moveType,
            ["isLoop"] = isLoop,
            ["inputDelay"] = inputDelay,
            ["branchDelay"] = branchDelay,
            ["inputSequence"] = inputSequence,
            ["requiredStates"] = requiredStates
        });
        move.InitialiseObjects();
        return move;
    }

    public static void SetMoveTotalFrames(MoveData move, int totalFrames)
    {
        SetField(move, "totalFrames", totalFrames);
    }

    public static Dictionary<string, Transform> MakeBoneMap(params string[] names)
    {
        Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
        foreach (string name in names)
        {
            bones[name] = new GameObject(name).transform;
        }

        return bones;
    }

    public static GameObject MakeButtonObject(out Button button)
    {
        GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        button = go.GetComponent<Button>();
        return go;
    }

    public static TextMeshProUGUI MakeTmpText(string name = "TMP")
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        return go.GetComponent<TextMeshProUGUI>();
    }

    public static Image MakeImage(string name = "Image")
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        return go.GetComponent<Image>();
    }

    private static FieldInfo FindField(Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static MethodInfo FindMethod(Type type, string methodName, int parameterCount)
    {
        while (type != null)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                if (method.Name == methodName && method.GetParameters().Length == parameterCount)
                {
                    return method;
                }
            }

            type = type.BaseType;
        }

        return null;
    }

    private static void EnsureEditorFolder(string assetPath)
    {
        string[] segments = assetPath.Split('/');
        string current = segments[0];

        for (int i = 1; i < segments.Length; i++)
        {
            string next = $"{current}/{segments[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[i]);
            }

            current = next;
        }
    }
}

[Serializable]
public class DummyIdentifiable : IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;
}

public class DummyDatabase : BaseDatabase<DummyIdentifiable>
{
    public DummyDatabase(string path)
    {
        filePath = path;
    }

    public IReadOnlyDictionary<string, DummyIdentifiable> Items => dict;
}

public class RecordingCameraMode : ICameraMode
{
    public bool EnterCalled;
    public bool ExitCalled;
    public bool UpdateCalled;

    public void Enter()
    {
        EnterCalled = true;
    }

    public void Exit()
    {
        ExitCalled = true;
    }

    public void Update(CameraManager cameraManager)
    {
        UpdateCalled = true;
    }
}

public class RecordingInputProvider : IInputProvider
{
    private readonly List<InputObject> inputs;

    public RecordingInputProvider(List<InputObject> inputs)
    {
        this.inputs = inputs;
    }

    public List<InputObject> GetInputs()
    {
        return inputs;
    }
}

public class FakeCollidable : ICollidable
{
    public int PlayerId { get; set; }
    public List<CollisionBox> ActiveHitboxes { get; } = new List<CollisionBox>();
    public List<CollisionBox> Hurtboxes { get; } = new List<CollisionBox>();
    public BodyCollider BodyCollider { get; set; }
    public Transform CachedTransform { get; set; }
    public CombatData CurrentCombatData { get; set; }
    public Dictionary<string, CombatHitboxEntry> ActiveEntries { get; } = new Dictionary<string, CombatHitboxEntry>();
    public PlayerStates States { get; set; }
    public CombatResult LastResult { get; private set; }
    public string CurrentMoveId { get; set; }

    public List<CollisionBox> GetActiveHitboxes() => ActiveHitboxes;
    public IEnumerable<CollisionBox> GetAllHurtboxes() => Hurtboxes;
    public CollisionBox GetCollisionBox(string id) => ActiveHitboxes.Find(x => x.Id == id) ?? Hurtboxes.Find(x => x.Id == id);
    public BodyCollider GetBodyCollider() => BodyCollider;
    public Transform GetTransform() => CachedTransform;
    public CombatData GetCombatData() => CurrentCombatData;
    public CombatHitboxEntry GetActiveHitboxEntry(string hitboxId) => ActiveEntries.TryGetValue(hitboxId, out CombatHitboxEntry entry) ? entry : null;
    public bool HasState(PlayerStates state) => (States & state) != 0;
    public void ReceiveCombatResult(CombatResult result) => LastResult = result;
    public string GetCurrentMoveId() => CurrentMoveId;
}
#endif
