using UnityEngine;
using UnityEngine.UI;

// In-game pause menu shown when ESC is pressed during gameplay.
// Provides continue, controls, options, and quit to main menu buttons.
public class PauseMenuPanel : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    private GameManager gameManager;

    // Wire up all buttons to their respective actions
    public void Initialise(GameManager gameManager)
    {
        this.gameManager = gameManager;

        continueButton.onClick.AddListener(OnContinue);
        controlsButton.onClick.AddListener(OnControls);
        optionsButton.onClick.AddListener(OnOptions);
        quitButton.onClick.AddListener(OnQuit);
    }

    // Resume gameplay and hide the pause menu
    private void OnContinue()
    {
        gameManager.ResumeGame();
    }

    // Open the controls rebinding panel from the pause menu
    private void OnControls()
    {
        gameManager.ShowPauseSubPanel("controls");
    }

    // Open the options panel from the pause menu
    private void OnOptions()
    {
        gameManager.ShowPauseSubPanel("options");
    }

    // Quit the current match and return to the main menu
    private void OnQuit()
    {
        gameManager.QuitToMenu();
    }
}