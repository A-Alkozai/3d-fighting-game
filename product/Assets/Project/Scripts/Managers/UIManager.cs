using UnityEngine;

// Holds references to all UI components and provides getters for other systems
public class UIManager : MonoBehaviour
{
    [SerializeField] private RecentInputsUI recentInputsUI;
    [SerializeField] private HealthBarUI healthBarP1;
    [SerializeField] private HealthBarUI healthBarP2;
    [SerializeField] private RoundHUD roundHUD;

    private bool isActiveRecentInputsUI = true;

    void Start()
    {
        recentInputsUI.SetActive(isActiveRecentInputsUI);
    }

    // Called every frame from GameManager to keep the debug input UI visibility in sync
    public void update()
    {
        recentInputsUI.SetActive(isActiveRecentInputsUI);
    }

    // Called by OptionsPanel when the recent inputs toggle changes
    public void SetRecentInputsActive(bool active)
    {
        isActiveRecentInputsUI = active;
        recentInputsUI.SetActive(active);
    }

    // Returns the current state of the recent inputs toggle
    public bool GetRecentInputsActive()
    {
        return isActiveRecentInputsUI;
    }

    public RecentInputsUI GetRecentInputsUI()
    {
        return recentInputsUI;
    }

    public HealthBarUI GetHealthBarP1()
    {
        return healthBarP1;
    }

    public HealthBarUI GetHealthBarP2()
    {
        return healthBarP2;
    }

    public RoundHUD GetRoundHUD()
    {
        return roundHUD;
    }
}