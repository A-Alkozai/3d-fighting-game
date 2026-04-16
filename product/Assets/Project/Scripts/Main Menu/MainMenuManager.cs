using UnityEngine;

// Handles panel switching for the main menu, and launching/returning from the game
public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject roundSelectionPanel;
    [SerializeField] private GameObject gameUIPanel;

    [Header("Game References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject player1Object;
    [SerializeField] private GameObject player2Object;
    [SerializeField] private Camera gameCamera;

    private ControlsPanel controlsPanelScript;
    private RoundSelectionPanel roundSelectionPanelScript;
    private InputKeys inputKeys;
    private bool gameStarted = false;

    void Awake()
    {
        // Create input keys (loads saved bindings from PlayerPrefs) and initialise sub-panels
        inputKeys = new InputKeys();
        controlsPanelScript = controlsPanel.GetComponent<ControlsPanel>();
        controlsPanelScript.Initialise(inputKeys, this);

        OptionsPanel optionsPanelScript = optionsPanel.GetComponent<OptionsPanel>();
        optionsPanelScript.Initialise(this, uiManager);

        roundSelectionPanelScript = roundSelectionPanel.GetComponent<RoundSelectionPanel>();
        roundSelectionPanelScript.Initialise(this);
    }

    void Start()
    {
        // Start on the main menu with game objects hidden
        ShowMainMenu();
        SetGameActive(false);
    }

    // Called by GameManager when a match ends and we need to go back to menu
    public void ReturnFromGame()
    {
        ShowMainMenu();
        SetGameActive(false);
    }

    // Play button opens the round selection screen
    public void OnPlayPressed()
    {
        mainMenuPanel.SetActive(false);
        roundSelectionPanel.SetActive(true);
    }

    // Called when the player picks how many rounds to win - starts the game
    public void OnRoundsSelected(int roundsToWin)
    {
        roundSelectionPanel.SetActive(false);
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        SetGameActive(true);

        // StartGame handles both first-time and repeat launches
        gameManager.StartGame(inputKeys, roundsToWin);

        Debug.Log($"[MainMenu] Game started - First to {roundsToWin}");
    }

    // Show the controls rebinding panel
    public void OnControlsPressed()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
        controlsPanelScript.Refresh();
    }

    // Show the options panel (placeholder for now)
    public void OnOptionsPressed()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    // Any sub-panel's back button returns here
    public void OnBackToMenu()
    {
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        roundSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Quit the application (also stops play mode in the editor)
    public void OnQuitPressed()
    {
        Debug.Log("[MainMenu] Quitting...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Activate only the main menu panel, hide everything else
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        roundSelectionPanel.SetActive(false);
        gameUIPanel.SetActive(false);
    }

    // Show or hide the player game objects (not destroyed, just toggled)
    private void SetGameActive(bool active)
    {
        player1Object.SetActive(active);
        player2Object.SetActive(active);
    }

    public InputKeys GetInputKeys()
    {
        return inputKeys;
    }
}