using UnityEngine;
using UnityEngine.UI;

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