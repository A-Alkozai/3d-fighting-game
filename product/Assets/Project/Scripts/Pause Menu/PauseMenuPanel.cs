using UnityEngine;
using UnityEngine.UI;

// In-game pause menu - shown when ESC is pressed during gameplay
// Provides continue, controls, options, and quit to main menu
public class PauseMenuPanel : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    private GameManager gameManager;

    public void Initialise(GameManager gameManager)
    {
        this.gameManager = gameManager;

        continueButton.onClick.AddListener(OnContinue);
        controlsButton.onClick.AddListener(OnControls);
        optionsButton.onClick.AddListener(OnOptions);
        quitButton.onClick.AddListener(OnQuit);
    }

    private void OnContinue()
    {
        gameManager.ResumeGame();
    }

    private void OnControls()
    {
        gameManager.ShowPauseSubPanel("controls");
    }

    private void OnOptions()
    {
        gameManager.ShowPauseSubPanel("options");
    }

    private void OnQuit()
    {
        gameManager.QuitToMenu();
    }
}