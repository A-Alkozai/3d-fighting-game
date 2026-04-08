using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundSelectionPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button rounds1Button;
    [SerializeField] private Button rounds2Button;
    [SerializeField] private Button rounds3Button;
    [SerializeField] private Button rounds4Button;
    [SerializeField] private Button rounds5Button;

    private MainMenuManager menuManager;

    public void Initialise(MainMenuManager menuManager)
    {
        this.menuManager = menuManager;

        titleText.text = "FIRST TO";

        rounds1Button.onClick.AddListener(() => OnRoundsSelected(1));
        rounds2Button.onClick.AddListener(() => OnRoundsSelected(2));
        rounds3Button.onClick.AddListener(() => OnRoundsSelected(3));
        rounds4Button.onClick.AddListener(() => OnRoundsSelected(4));
        rounds5Button.onClick.AddListener(() => OnRoundsSelected(5));
    }

    private void OnRoundsSelected(int rounds)
    {
        menuManager.OnRoundsSelected(rounds);
    }
}