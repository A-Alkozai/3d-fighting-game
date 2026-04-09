using UnityEngine;
using UnityEngine.UI;

// Placeholder options panel - only has a back button for now
public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Button backButton;
    private MainMenuManager menuManager;

    public void Initialise(MainMenuManager menuManager)
    {
        this.menuManager = menuManager;
        backButton.onClick.AddListener(OnBack);
    }

    private void OnBack()
    {
        menuManager.OnBackToMenu();
    }
}