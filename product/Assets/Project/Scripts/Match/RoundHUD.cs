using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundHUD : MonoBehaviour
{
    [Header("Center Text")]
    [SerializeField] private TMP_Text centerText;

    [Header("Round Counter")]
    [SerializeField] private TMP_Text roundCounterText;

    [Header("Win Circles")]
    [SerializeField] private Transform p1CirclesParent;
    [SerializeField] private Transform p2CirclesParent;
    [SerializeField] private GameObject circlePrefab;

    [Header("Colors")]
    [SerializeField] private Color32 emptyCircleColor = new Color32(207, 207, 207, 255);
    [SerializeField] private Color32 wonCircleColor = new Color32(80, 185, 15, 255);
    [SerializeField] private Color countdownColor = Color.white;
    [SerializeField] private Color fightColor = Color.red;
    [SerializeField] private Color koColor = Color.red;

    private List<Image> p1Circles = new List<Image>();
    private List<Image> p2Circles = new List<Image>();
    private int roundsToWin;

    public void Initialise(int roundsToWin)
    {
        this.roundsToWin = roundsToWin;

        // Clear old circles
        ClearCircles();

        // Spawn circles for each player
        for (int i = 0; i < roundsToWin; i++)
        {
            GameObject c1 = Instantiate(circlePrefab, p1CirclesParent);
            Image img1 = c1.GetComponent<Image>();
            img1.color = emptyCircleColor;
            p1Circles.Add(img1);

            GameObject c2 = Instantiate(circlePrefab, p2CirclesParent);
            Image img2 = c2.GetComponent<Image>();
            img2.color = emptyCircleColor;
            p2Circles.Add(img2);
        }

        centerText.text = "";
        UpdateRoundCounter(1);
    }

    private void ClearCircles()
    {
        foreach (Transform child in p1CirclesParent)
            Destroy(child.gameObject);
        foreach (Transform child in p2CirclesParent)
            Destroy(child.gameObject);

        p1Circles.Clear();
        p2Circles.Clear();
    }

    public void UpdateWins(int p1Wins, int p2Wins)
    {
        for (int i = 0; i < p1Circles.Count; i++)
        {
            p1Circles[i].color = (i < p1Wins) ? wonCircleColor : emptyCircleColor;
        }
        for (int i = 0; i < p2Circles.Count; i++)
        {
            p2Circles[i].color = (i < p2Wins) ? wonCircleColor : emptyCircleColor;
        }
    }

    public void UpdateRoundCounter(int round)
    {
        roundCounterText.text = $"Round {round}";
    }

    public void ShowCountdown(string text)
    {
        centerText.text = text;
        centerText.fontSize = 120;

        if (text == "FIGHT!")
        {
            centerText.color = fightColor;
            centerText.fontSize = 140;
        }
        else
        {
            centerText.color = countdownColor;
        }
    }

    public void ShowKO(string text)
    {
        centerText.text = text;
        centerText.fontSize = 150;
        centerText.color = koColor;
    }

    public void ClearCenterText()
    {
        centerText.text = "";
    }
}