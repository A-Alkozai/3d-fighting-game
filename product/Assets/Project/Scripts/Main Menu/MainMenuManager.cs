using UnityEngine;

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
    [SerializeField] private GameObject player1Object;
    [SerializeField] private GameObject player2Object;
    [SerializeField] private Camera gameCamera;

    private ControlsPanel controlsPanelScript;
    private RoundSelectionPanel roundSelectionPanelScript;
    private InputKeys inputKeys;
    private bool gameStarted = false;

    void Awake()
    {
        inputKeys = new InputKeys();
        controlsPanelScript = controlsPanel.GetComponent<ControlsPanel>();
        controlsPanelScript.Initialise(inputKeys, this);

        OptionsPanel optionsPanelScript = optionsPanel.GetComponent<OptionsPanel>();
        optionsPanelScript.Initialise(this);

        roundSelectionPanelScript = roundSelectionPanel.GetComponent<RoundSelectionPanel>();
        roundSelectionPanelScript.Initialise(this);
    }

    void Start()
    {
        ShowMainMenu();
        SetGameActive(false);
    }

    public void ReturnFromGame()
    {
        ShowMainMenu();
        SetGameActive(false);
    }

    public void OnPlayPressed()
    {
        mainMenuPanel.SetActive(false);
        roundSelectionPanel.SetActive(true);
    }

    public void OnRoundsSelected(int roundsToWin)
    {
        roundSelectionPanel.SetActive(false);
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        SetGameActive(true);

        // Always call StartGame — it handles fresh vs repeat internally
        gameManager.StartGame(inputKeys, roundsToWin);

        Debug.Log($"[MainMenu] Game started — First to {roundsToWin}");
    }

    public void OnControlsPressed()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
        controlsPanelScript.Refresh();
    }

    public void OnOptionsPressed()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnBackToMenu()
    {
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        roundSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnQuitPressed()
    {
        Debug.Log("[MainMenu] Quitting...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        controlsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        roundSelectionPanel.SetActive(false);
        gameUIPanel.SetActive(false);
    }

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