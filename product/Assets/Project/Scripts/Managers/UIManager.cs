using UnityEngine;

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

    public void update()
    {
        recentInputsUI.SetActive(isActiveRecentInputsUI);
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