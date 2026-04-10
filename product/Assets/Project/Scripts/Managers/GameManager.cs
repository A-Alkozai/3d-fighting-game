using UnityEngine;
using UnityEngine.InputSystem;

// Central game loop — owns all core systems, runs the fixed timestep update, manages match flow
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

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject pauseControlsPanel;
    [SerializeField] private GameObject pauseOptionsPanel;

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

    private PauseMenuPanel pauseMenuScript;
    private ControlsPanel pauseControlsScript;
    private OptionsPanel pauseOptionsScript;

    private int gameFPS = 60;
    private float logicTimer = 0f;
    private float logicDeltaTime = 1f / 60f;
    private bool gameActive = false;
    private bool koReported = false;
    private bool initialized = false;
    private bool isPaused = false;

    private RoundManager.MatchState prevState;

    // Called by MainMenuManager — sets up systems on first call, resets for a new match every call
    public void StartGame(InputKeys inputKeys, int roundsToWin)
    {
        this.inputKeys = inputKeys;
        Application.targetFrameRate = gameFPS;

        // First-time setup
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

            // Initialise pause menu panels
            pauseMenuScript = pauseMenuPanel.GetComponent<PauseMenuPanel>();
            pauseMenuScript.Initialise(this);

            // Pause controls needs InputKeys to build the binding rows
            pauseControlsScript = pauseControlsPanel.GetComponent<ControlsPanel>();
            pauseControlsScript.Initialise(inputKeys, null);
            pauseControlsScript.SetGameManager(this);

            // Pause options needs UIManager to toggle recent inputs
            pauseOptionsScript = pauseOptionsPanel.GetComponent<OptionsPanel>();
            pauseOptionsScript.Initialise(null, uiManager);
            pauseOptionsScript.SetGameManager(this);

            initialized = true;
        }

        // Reset players for fresh match
        player1.ResetForRound();
        player2.ResetForRound();

        // Fresh combat/collision systems each match
        combatExecutor = new CombatExecutor();
        collisionManager = new CollisionManager(player1, player2, combatExecutor, stageCollision);

        // Fresh round manager
        roundManager = new RoundManager(roundsToWin);
        roundHUD.Initialise(roundsToWin);
        roundHUD.ClearCenterText();
        roundManager.StartMatch();

        // Lock input during countdown
        player1.SetInputLocked(true);
        player2.SetInputLocked(true);

        // Reset game state
        gameActive = true;
        isPaused = false;
        koReported = false;
        logicTimer = 0f;
        prevState = RoundManager.MatchState.Countdown;

        // Make sure pause panels are hidden
        pauseMenuPanel.SetActive(false);
        pauseControlsPanel.SetActive(false);
        pauseOptionsPanel.SetActive(false);

        Debug.Log($"[GameManager] Match started — First to {roundsToWin}");
    }

    void Update()
    {
        if (!gameActive) return;

        // Check for ESC to toggle pause
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
            return;
        }

        // Don't run game logic while paused
        if (isPaused) return;

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

            player1.FaceOpponent(player2.transform);
            player2.FaceOpponent(player1.transform);

            CheckKO();
        }

        UpdateHUD();
        uiManager.update();
        cameraManager.Update();
        logicTimer += Time.deltaTime;
    }

    // Freeze the game and show the pause menu
    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        pauseControlsPanel.SetActive(false);
        pauseOptionsPanel.SetActive(false);
        Debug.Log("[GameManager] Game paused");
    }

    // Hide all pause panels and resume gameplay
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        pauseControlsPanel.SetActive(false);
        pauseOptionsPanel.SetActive(false);
        Debug.Log("[GameManager] Game resumed");
    }

    // Show controls or options from the pause menu
    public void ShowPauseSubPanel(string panel)
    {
        pauseMenuPanel.SetActive(false);

        if (panel == "controls")
        {
            pauseControlsScript.SetOpenedFromPause(true);
            pauseControlsScript.Refresh();
            pauseControlsPanel.SetActive(true);
        }
        else if (panel == "options")
        {
            pauseOptionsScript.SetOpenedFromPause(true);
            // Re-sync the toggle with the current state before showing
            pauseOptionsScript.Initialise(null, uiManager);
            pauseOptionsPanel.SetActive(true);
        }
    }

    // Return from a sub-panel back to the pause menu
    public void ReturnToPauseMenu()
    {
        pauseControlsPanel.SetActive(false);
        pauseOptionsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    // Quit from pause menu back to the main menu
    public void QuitToMenu()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        pauseControlsPanel.SetActive(false);
        pauseOptionsPanel.SetActive(false);
        ReturnToMenu();
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