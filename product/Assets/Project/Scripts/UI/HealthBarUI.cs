using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Displays a single health bar - fill image + text showing current HP
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    // Update the bar fill amount and text to reflect current health
    public void UpdateHealth(int current, int max)
    {
        float percentage = (float)current / max;
        fillImage.fillAmount = percentage;
        healthText.text = $"{current}";
    }
}