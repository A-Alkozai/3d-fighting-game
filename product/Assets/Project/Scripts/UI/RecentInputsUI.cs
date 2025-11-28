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
    private Color32 defaultInputColour = new Color32(219, 219, 219, 255);
    private Color32 holdingInputColour = new Color32(60, 116, 135, 255);
    private Color32 releasedInputColour = new Color32(84, 196, 210, 255);

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

                while ((totalSpacing / (maxInputs - 1)) < minSpacing && maxInputs > 1)
                {
                    maxInputs--;
                    totalSpacing += prefabWidth;
                }

                panelLayout.spacing = totalSpacing / (maxInputs - 1);
            }
        }

        DestroyImmediate(temp);
    }

    public void AddRecentInput(InputObject newInput)
    {
        if (!GetIsActive()) // If UI inactive
            return;

        string inputKey = newInput.GetInputKey().ToString();

        if (!newInput.IsPending() && newInput.IsHeld() && newInput.GetFrame().GetFrameNumber() <= 0)
        {
            GameObject latestMatch = null;

            for (int i = 0; i < displayedInputs.Count; i++)
            {
                GameObject currentKey = displayedInputs[i];
                string text = currentKey.GetComponentInChildren<TextMeshProUGUI>().text;
                if (text == inputKey)
                {
                    latestMatch = currentKey;
                }
            }

            if (latestMatch == null)
                return;

            Image[] imagesList = latestMatch.GetComponentsInChildren<Image>(true);
            Color keyColour = imagesList[2].color;

            if (newInput.GetFrame().GetFrameNumber() == 0)
            {
                imagesList[2].color = holdingInputColour;
            }
            else if (newInput.GetFrame().GetFrameNumber() == -1)
            {
                if (keyColour == holdingInputColour)
                {
                    imagesList[2].color = releasedInputColour;
                }
            }
            return;
        }

        RemoveExcessInputs();

        GameObject inputPrefabObj = Instantiate(inputPrefab, recentInputPanel.transform);
        inputPrefabObj.GetComponentInChildren<TextMeshProUGUI>().text = inputKey;

        Image[] images = inputPrefabObj.GetComponentsInChildren<Image>(true);
        images[2].color = defaultInputColour;
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
