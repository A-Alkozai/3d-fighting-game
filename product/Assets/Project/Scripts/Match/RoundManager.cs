using UnityEngine;

public class RoundManager
{
    public enum MatchState
    {
        WaitingToStart,
        Countdown,
        Fighting,
        KOPause,
        RoundReset,
        MatchOver
    }

    private int roundsToWin;
    private int currentRound = 1;
    private int p1Wins = 0;
    private int p2Wins = 0;
    private MatchState state = MatchState.WaitingToStart;
    private int stateTimer = 0;

    private int countdownDuration = 180;
    private int fightFlashDuration = 60;
    private int koPauseDuration = 180;
    private int resetDuration = 60;
    private int matchOverDuration = 300; // 5 seconds at 60fps

    private int countdownPhase = 0;
    private bool matchOverTimerDone = false;

    public MatchState State => state;
    public int CurrentRound => currentRound;
    public int P1Wins => p1Wins;
    public int P2Wins => p2Wins;
    public int RoundsToWin => roundsToWin;
    public int CountdownPhase => countdownPhase;
    public bool InputLocked => state != MatchState.Fighting;
    public bool MatchOverTimerDone => matchOverTimerDone;

    public RoundManager(int roundsToWin)
    {
        this.roundsToWin = roundsToWin;
    }

    public void StartMatch()
    {
        currentRound = 1;
        p1Wins = 0;
        p2Wins = 0;
        matchOverTimerDone = false;
        BeginCountdown();
    }

    public void BeginCountdown()
    {
        state = MatchState.Countdown;
        stateTimer = 0;
        countdownPhase = 3;
    }

    public void Update()
    {
        switch (state)
        {
            case MatchState.Countdown:
                UpdateCountdown();
                break;

            case MatchState.Fighting:
                break;

            case MatchState.KOPause:
                UpdateKOPause();
                break;

            case MatchState.RoundReset:
                UpdateRoundReset();
                break;

            case MatchState.MatchOver:
                UpdateMatchOver();
                break;
        }
    }

    private void UpdateCountdown()
    {
        stateTimer++;

        int framesPerNumber = countdownDuration / 3;

        if (stateTimer <= framesPerNumber)
        {
            countdownPhase = 3;
        }
        else if (stateTimer <= framesPerNumber * 2)
        {
            countdownPhase = 2;
        }
        else if (stateTimer <= countdownDuration)
        {
            countdownPhase = 1;
        }
        else if (stateTimer <= countdownDuration + fightFlashDuration)
        {
            countdownPhase = -1;
        }
        else
        {
            countdownPhase = 0;
            state = MatchState.Fighting;
            Debug.Log("[RoundManager] FIGHT!");
        }
    }

    public void OnPlayerKO(int loserPlayerId)
    {
        if (state != MatchState.Fighting) return;

        if (loserPlayerId == 1)
            p2Wins++;
        else
            p1Wins++;

        state = MatchState.KOPause;
        stateTimer = 0;

        Debug.Log($"[RoundManager] KO! P1 Wins: {p1Wins} | P2 Wins: {p2Wins}");
    }

    private void UpdateKOPause()
    {
        stateTimer++;

        if (stateTimer >= koPauseDuration)
        {
            if (p1Wins >= roundsToWin || p2Wins >= roundsToWin)
            {
                state = MatchState.MatchOver;
                stateTimer = 0;
                Debug.Log($"[RoundManager] Match Over! Winner: P{(p1Wins >= roundsToWin ? 1 : 2)}");
            }
            else
            {
                state = MatchState.RoundReset;
                stateTimer = 0;
                currentRound++;
            }
        }
    }

    private void UpdateRoundReset()
    {
        stateTimer++;

        if (stateTimer >= resetDuration)
        {
            BeginCountdown();
        }
    }

    private void UpdateMatchOver()
    {
        stateTimer++;

        if (stateTimer >= matchOverDuration)
        {
            matchOverTimerDone = true;
            Debug.Log("[RoundManager] Returning to menu");
        }
    }

    public int GetMatchWinner()
    {
        if (p1Wins >= roundsToWin) return 1;
        if (p2Wins >= roundsToWin) return 2;
        return 0;
    }

    public string GetCountdownText()
    {
        if (countdownPhase > 0) return countdownPhase.ToString();
        if (countdownPhase == -1) return "FIGHT!";
        return "";
    }

    public string GetKOText()
    {
        if (state == MatchState.KOPause) return "KO";
        if (state == MatchState.MatchOver)
        {
            int winner = GetMatchWinner();
            return $"PLAYER {winner} WINS!";
        }
        return "";
    }
}