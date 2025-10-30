using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecentInputsUI : MonoBehaviour
{
    [SerializeField] private GameObject recentInputPanel;
    [SerializeField] private GameObject inputPrefab;

    private List<GameObject> displayedInputs = new List<GameObject>();
    private int maxInputs;
    private float minSpacing = 30f;

    void Start()
    {
        CalculateInputLayout();
    }

    public void CalculateInputLayout()
    {
        RectTransform panelRect = recentInputPanel.GetComponent<RectTransform>();
        GameObject temp = Instantiate(inputPrefab, recentInputPanel.transform);
        RectTransform prefabRect = temp.GetComponent<RectTransform>();
        HorizontalLayoutGroup panelLayout = recentInputPanel.GetComponent<HorizontalLayoutGroup>();
        Canvas.ForceUpdateCanvases();

        if (panelRect != null && prefabRect != null)
        {
            float panelPadding = panelLayout.padding.left + panelLayout.padding.right;
            float panelWidth = panelRect.rect.width - panelPadding;
            float prefabWidth = prefabRect.rect.width;

            if (prefabWidth > 0)
            {
                maxInputs = Mathf.FloorToInt(panelWidth / prefabWidth);
                float totalSpacing = panelWidth - (maxInputs * prefabWidth);

                while ((totalSpacing / maxInputs) < minSpacing)
                {
                    maxInputs--;
                    totalSpacing += prefabWidth;
                }

                panelLayout.spacing = totalSpacing / maxInputs;
            }
        }

        DestroyImmediate(temp);
    }

    public void AddRecentInput(string newInput)
    {
        if (!GetIsActive()) { return; }

        RemoveExcessInputs();

        GameObject inputPrefabObj = Instantiate(inputPrefab, recentInputPanel.transform);
        inputPrefabObj.GetComponentInChildren<TextMeshProUGUI>().text = newInput;
        displayedInputs.Add(inputPrefabObj);
    }

    public void RemoveExcessInputs()
    {
        while (displayedInputs.Count >= maxInputs && displayedInputs.Count > 0)
        {
            Destroy(displayedInputs[0]);
            displayedInputs.RemoveAt(0);
        }
    }

    public void ClearRecentInputs()
    {
        foreach (GameObject input in displayedInputs)
        {
            Destroy(input);
        }
        displayedInputs.Clear();
    }

    public void SetActive(bool isActive)
    {
        recentInputPanel.SetActive(isActive);
        if (!isActive) ClearRecentInputs();
    }

    public bool GetIsActive() => recentInputPanel.activeSelf;
}
