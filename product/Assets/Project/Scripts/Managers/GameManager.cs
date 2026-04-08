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

    [Header("Menu")]
    [SerializeField] private MainMenuManager mainMenuManager;

    private IInputProvider inputProvider1;
    private IInputProvider inputProvider2;
    private CameraManager cameraManager;
    private CollisionManager collisionManager;
    private CombatExecutor combatExecutor;
    private StageCollision stageCollision;
    private RoundManager roundManager;
    private HealthBarUI healthBarP1;
    private HealthBarUI healthBarP2;
    private RoundHUD roundHUD;
    private InputKeys inputKeys;

    private int gameFPS = 60;
    private float logicTimer = 0f;
    private float logicDeltaTime = 1f / 60f;
    private bool gameActive = false;
    private bool koReported = false;
    private bool initialized = false;

    private RoundManager.MatchState prevState;

    public void StartGame(InputKeys inputKeys, int roundsToWin)
    {
        this.inputKeys = inputKeys;
        Application.targetFrameRate = gameFPS;

        // Only initialize systems once
        if (!initialized)
        {
            inputProvider1 = new LocalInputProvider(inputKeys);
            inputManager.AddInputToPlayerMap(inputProvider1, player1);
            inputManager.AddRecentInputsUI(uiManager.GetRecentInputsUI());

            player1.start();
            player2.start();

            cameraManager = new CameraManager(camera, player1.transform, player2.transform);
            cameraManager.SetMode(new CombatCameraMode());

            stageCollision = new StageCollision(stageWallsParent);

            healthBarP1 = uiManager.GetHealthBarP1();
            healthBarP2 = uiManager.GetHealthBarP2();
            roundHUD = uiManager.GetRoundHUD();

            initialized = true;
        }

        // Reset players for fresh match
        player1.ResetForRound();
        player2.ResetForRound();

        // Create fresh combat/collision systems each match
        combatExecutor = new CombatExecutor();
        collisionManager = new CollisionManager(player1, player2, combatExecutor, stageCollision);

        // Create fresh round manager
        roundManager = new RoundManager(roundsToWin);
        roundHUD.Initialise(roundsToWin);
        roundHUD.ClearCenterText();
        roundManager.StartMatch();

        // Lock input during countdown
        player1.SetInputLocked(true);
        player2.SetInputLocked(true);

        // Reset game state
        gameActive = true;
        koReported = false;
        logicTimer = 0f;
        prevState = RoundManager.MatchState.Countdown;

        Debug.Log($"[GameManager] Match started — First to {roundsToWin}");
    }

    void Update()
    {
        if (!gameActive) return;

        inputManager.update();

        while (logicTimer >= logicDeltaTime)
        {
            logicTimer -= logicDeltaTime;

            roundManager.Update();
            HandleStateTransitions();

            if (roundManager.MatchOverTimerDone)
            {
                ReturnToMenu();
                return;
            }

            player1.update();
            player2.update();
            collisionManager.Update();

            CheckKO();
        }

        UpdateHUD();
        uiManager.update();
        cameraManager.Update();
        logicTimer += Time.deltaTime;
    }

    private void HandleStateTransitions()
    {
        RoundManager.MatchState currentState = roundManager.State;

        if (currentState != prevState)
        {
            switch (currentState)
            {
                case RoundManager.MatchState.Fighting:
                    player1.SetInputLocked(false);
                    player2.SetInputLocked(false);
                    koReported = false;
                    roundHUD.ClearCenterText();
                    break;

                case RoundManager.MatchState.KOPause:
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    break;

                case RoundManager.MatchState.RoundReset:
                    player1.ResetForRound();
                    player2.ResetForRound();
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    roundHUD.UpdateWins(roundManager.P1Wins, roundManager.P2Wins);
                    roundHUD.UpdateRoundCounter(roundManager.CurrentRound);
                    roundHUD.ClearCenterText();
                    break;

                case RoundManager.MatchState.Countdown:
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    break;

                case RoundManager.MatchState.MatchOver:
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    roundHUD.UpdateWins(roundManager.P1Wins, roundManager.P2Wins);
                    break;
            }

            prevState = currentState;
        }
    }

    private void CheckKO()
    {
        if (roundManager.State != RoundManager.MatchState.Fighting) return;
        if (koReported) return;

        if (player1.IsKO)
        {
            roundManager.OnPlayerKO(1);
            koReported = true;
        }
        else if (player2.IsKO)
        {
            roundManager.OnPlayerKO(2);
            koReported = true;
        }
    }

    private void UpdateHUD()
    {
        healthBarP1.UpdateHealth(player1.GetHealthManager().CurrentHealth,
                                 player1.GetHealthManager().MaxHealth);
        healthBarP2.UpdateHealth(player2.GetHealthManager().CurrentHealth,
                                 player2.GetHealthManager().MaxHealth);

        RoundManager.MatchState state = roundManager.State;

        if (state == RoundManager.MatchState.Countdown)
        {
            roundHUD.ShowCountdown(roundManager.GetCountdownText());
        }
        else if (state == RoundManager.MatchState.KOPause ||
                 state == RoundManager.MatchState.MatchOver)
        {
            roundHUD.ShowKO(roundManager.GetKOText());
        }
    }

    private void ReturnToMenu()
    {
        gameActive = false;

        player1.ResetForRound();
        player2.ResetForRound();

        player1.gameObject.SetActive(false);
        player2.gameObject.SetActive(false);

        roundHUD.ClearCenterText();

        mainMenuManager.ReturnFromGame();

        Debug.Log("[GameManager] Returned to menu");
    }
}