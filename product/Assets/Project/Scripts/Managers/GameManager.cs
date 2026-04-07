using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] InputManager inputManager;
    [SerializeField] new Camera camera;
    [SerializeField] Player player1;
    [SerializeField] Player player2;

    [Header("Stage")]
    [SerializeField] private Transform stageWallsParent;

    private IInputProvider inputProvider1;
    private IInputProvider inputProvider2;
    private CameraManager cameraManager;
    private CollisionManager collisionManager;
    private CombatExecutor combatExecutor;
    private StageCollision stageCollision;
    private HealthBarUI healthBarP1;
    private HealthBarUI healthBarP2;
    private InputKeys inputKeys;

    private int gameFPS = 60;
    private float logicTimer = 0f;
    private float logicDeltaTime = 1f / 60f;
    private bool gameActive = false;

    public void StartGame(InputKeys inputKeys)
    {
        this.inputKeys = inputKeys;
        Application.targetFrameRate = gameFPS;

        inputProvider1 = new LocalInputProvider(inputKeys);
        inputManager.AddInputToPlayerMap(inputProvider1, player1);
        inputManager.AddRecentInputsUI(uiManager.GetRecentInputsUI());

        player1.start();
        player2.start();

        cameraManager = new CameraManager(camera, player1.transform, player2.transform);
        cameraManager.SetMode(new CombatCameraMode());

        combatExecutor = new CombatExecutor();
        stageCollision = new StageCollision(stageWallsParent);
        collisionManager = new CollisionManager(player1, player2, combatExecutor, stageCollision);

        healthBarP1 = uiManager.GetHealthBarP1();
        healthBarP2 = uiManager.GetHealthBarP2();

        gameActive = true;
        logicTimer = 0f;
    }

    void Update()
    {
        if (!gameActive) return;

        inputManager.update();

        while (logicTimer >= logicDeltaTime)
        {
            logicTimer -= logicDeltaTime;
            player1.update();
            player2.update();
            collisionManager.Update();
        }

        healthBarP1.UpdateHealth(player1.GetHealthManager().CurrentHealth,
                                 player1.GetHealthManager().MaxHealth);
        healthBarP2.UpdateHealth(player2.GetHealthManager().CurrentHealth,
                                 player2.GetHealthManager().MaxHealth);

        uiManager.update();
        cameraManager.Update();
        logicTimer += Time.deltaTime;
    }
}