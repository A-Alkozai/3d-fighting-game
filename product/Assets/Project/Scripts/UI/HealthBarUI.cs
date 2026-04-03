using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    public void UpdateHealth(int current, int max)
    {
        float percentage = (float)current / max;
        fillImage.fillAmount = percentage;
        healthText.text = $"{current}";
    }
}