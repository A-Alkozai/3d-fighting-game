using UnityEngine;
using UnityEngine.UI;

// Options panel with a toggle for the debug input display
// Can be opened from the main menu or the in-game pause menu
public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Toggle recentInputsToggle;

    private MainMenuManager menuManager;
    private UIManager uiManager;
    private GameManager gameManager;
    private bool openedFromPause = false;  // Tracks which menu to return to on back
    private bool initialised = false;      // Prevents adding button listeners more than once

    // Store references and wire up the toggle and back button
    // menuManager can be null if opened from the pause menu
    // uiManager can be null if opened from the main menu before game starts (unlikely but safe)
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

        // Sync toggle state with the current setting if UIManager is available
        if (uiManager != null)
        {
            recentInputsToggle.isOn = uiManager.GetRecentInputsActive();
        }
    }

    // Store a reference to GameManager so we can return to the pause menu
    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    // Set whether this panel was opened from the pause menu
    public void SetOpenedFromPause(bool fromPause)
    {
        openedFromPause = fromPause;
    }

    // Toggle the recent inputs debug UI on or off
    private void OnRecentInputsToggled(bool isOn)
    {
        if (uiManager != null)
        {
            uiManager.SetRecentInputsActive(isOn);
        }
    }

    // Return to whichever menu opened this panel (pause menu or main menu)
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