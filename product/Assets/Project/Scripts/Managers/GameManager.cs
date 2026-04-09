using UnityEngine;

// Central game loop - owns all core systems, runs the fixed timestep update, manages match flow
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
    private float logicTimer = 0f;               // Accumulator for fixed timestep
    private float logicDeltaTime = 1f / 60f;     // One logic frame = 1/60th of a second
    private bool gameActive = false;              // Gates the entire Update loop
    private bool koReported = false;              // Prevents reporting the same KO twice in one round
    private bool initialized = false;             // True after first-time system setup

    private RoundManager.MatchState prevState;    // Tracks state transitions for HandleStateTransitions

    // Called by MainMenuManager - sets up systems on first call, resets for a new match every call
    public void StartGame(InputKeys inputKeys, int roundsToWin)
    {
        this.inputKeys = inputKeys;
        Application.targetFrameRate = gameFPS;

        // First-time setup: create input, players, camera, stage collision, UI refs
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

        // Reset players to starting state
        player1.ResetForRound();
        player2.ResetForRound();

        // Fresh combat and collision systems each match (clears hit tracking)
        combatExecutor = new CombatExecutor();
        collisionManager = new CollisionManager(player1, player2, combatExecutor, stageCollision);

        // Fresh round manager with chosen rounds-to-win
        roundManager = new RoundManager(roundsToWin);
        roundHUD.Initialise(roundsToWin);
        roundHUD.ClearCenterText();
        roundManager.StartMatch();

        // Lock both players during the opening countdown
        player1.SetInputLocked(true);
        player2.SetInputLocked(true);

        // Reset match-level flags
        gameActive = true;
        koReported = false;
        logicTimer = 0f;
        prevState = RoundManager.MatchState.Countdown;

        Debug.Log($"[GameManager] Match started - First to {roundsToWin}");
    }

    void Update()
    {
        if (!gameActive) return;

        // Poll input every visual frame (before fixed steps)
        inputManager.update();

        // Fixed timestep loop - all game logic runs at exactly 60fps
        while (logicTimer >= logicDeltaTime)
        {
            logicTimer -= logicDeltaTime;

            // Round state machine (countdown, fighting, KO pause, etc.)
            roundManager.Update();
            HandleStateTransitions();

            // If match-over timer expired, go back to menu
            if (roundManager.MatchOverTimerDone)
            {
                ReturnToMenu();
                return;
            }

            // Core gameplay: players then collision
            player1.update();
            player2.update();
            collisionManager.Update();

            // Check if either player just got KO'd this frame
            CheckKO();
        }

        // Visual updates run every render frame (not fixed)
        UpdateHUD();
        uiManager.update();
        cameraManager.Update();
        logicTimer += Time.deltaTime;
    }

    // Detect when RoundManager changes state and apply side effects (lock input, reset players, etc.)
    private void HandleStateTransitions()
    {
        RoundManager.MatchState currentState = roundManager.State;

        if (currentState != prevState)
        {
            switch (currentState)
            {
                case RoundManager.MatchState.Fighting:
                    // Unlock players when the fight begins
                    player1.SetInputLocked(false);
                    player2.SetInputLocked(false);
                    koReported = false;
                    roundHUD.ClearCenterText();
                    break;

                case RoundManager.MatchState.KOPause:
                    // Lock input while showing the KO
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    break;

                case RoundManager.MatchState.RoundReset:
                    // Reset players to starting positions and health for the next round
                    player1.ResetForRound();
                    player2.ResetForRound();
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    roundHUD.UpdateWins(roundManager.P1Wins, roundManager.P2Wins);
                    roundHUD.UpdateRoundCounter(roundManager.CurrentRound);
                    roundHUD.ClearCenterText();
                    break;

                case RoundManager.MatchState.Countdown:
                    // Keep players locked during countdown
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    break;

                case RoundManager.MatchState.MatchOver:
                    // Lock input and show final win tally
                    player1.SetInputLocked(true);
                    player2.SetInputLocked(true);
                    roundHUD.UpdateWins(roundManager.P1Wins, roundManager.P2Wins);
                    break;
            }

            prevState = currentState;
        }
    }

    // Report the first KO detected this round to the RoundManager
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

    // Push current health values and round state text to the HUD every render frame
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

    // End the match, reset everything, and hand control back to the main menu
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