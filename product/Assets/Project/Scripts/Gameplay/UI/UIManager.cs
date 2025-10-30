using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RecentInputsUI recentInputsUI;

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
}
