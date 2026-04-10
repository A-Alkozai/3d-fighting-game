using UnityEngine;
using UnityEngine.UI;

// Options panel with toggle for debug input display
// Can be opened from main menu or pause menu
public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Toggle recentInputsToggle;

    private MainMenuManager menuManager;
    private UIManager uiManager;
    private GameManager gameManager;
    private bool openedFromPause = false;
    private bool initialised = false;

    public void Initialise(MainMenuManager menuManager, UIManager uiManager)
    {
        this.menuManager = menuManager;
        this.uiManager = uiManager;

        // Only add listeners once to prevent duplicate calls
        if (!initialised)
        {
            backButton.onClick.AddListener(OnBack);
            recentInputsToggle.onValueChanged.AddListener(OnRecentInputsToggled);
            initialised = true;
        }

        // Sync toggle state with current setting if UIManager is available
        if (uiManager != null)
        {
            recentInputsToggle.isOn = uiManager.GetRecentInputsActive();
        }
    }

    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void SetOpenedFromPause(bool fromPause)
    {
        openedFromPause = fromPause;
    }

    private void OnRecentInputsToggled(bool isOn)
    {
        if (uiManager != null)
        {
            uiManager.SetRecentInputsActive(isOn);
        }
    }

    private void OnBack()
    {
        if (openedFromPause && gameManager != null)
        {
            gameManager.ReturnToPauseMenu();
        }
        else if (menuManager != null)
        {
            menuManager.OnBackToMenu();
        }
    }
}