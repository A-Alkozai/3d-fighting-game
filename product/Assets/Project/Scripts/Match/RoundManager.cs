using UnityEngine;

// State machine that manages round flow: countdown → fighting → KO → next round or match over
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
    private int stateTimer = 0;                  // Frame counter within the current state

    // Duration constants (in frames at 60fps)
    private int countdownDuration = 180;         // 3 seconds for 3-2-1
    private int fightFlashDuration = 60;         // 1 second to show "FIGHT!"
    private int koPauseDuration = 180;           // 3 seconds to show KO before transitioning
    private int resetDuration = 60;              // 1 second pause between rounds
    private int matchOverDuration = 300;         // 5 seconds to show winner before returning to menu

    private int countdownPhase = 0;              // 3, 2, 1, -1 (FIGHT!), 0 (done)
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

    // Reset all round/win tracking and start the first countdown
    public void StartMatch()
    {
        currentRound = 1;
        p1Wins = 0;
        p2Wins = 0;
        matchOverTimerDone = false;
        BeginCountdown();
    }

    // Enter countdown state and reset its timer
    public void BeginCountdown()
    {
        state = MatchState.Countdown;
        stateTimer = 0;
        countdownPhase = 3;
    }

    // Called once per logic frame - advances whichever state is active
    public void Update()
    {
        switch (state)
        {
            case MatchState.Countdown:
                UpdateCountdown();
                break;

            case MatchState.Fighting:
                // Nothing to tick - gameplay is handled by Player/CollisionManager
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

    // Tick through 3-2-1 countdown, then flash "FIGHT!", then enter Fighting state
    private void UpdateCountdown()
    {
        stateTimer++;

        int framesPerNumber = countdownDuration / 3; // Each number gets equal screen time

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
            countdownPhase = -1; // -1 signals "FIGHT!" text
        }
        else
        {
            countdownPhase = 0;
            state = MatchState.Fighting;
            Debug.Log("[RoundManager] FIGHT!");
        }
    }

    // Called by GameManager when a player's health hits 0
    public void OnPlayerKO(int loserPlayerId)
    {
        if (state != MatchState.Fighting) return;

        // Award win to the other player
        if (loserPlayerId == 1)
            p2Wins++;
        else
            p1Wins++;

        state = MatchState.KOPause;
        stateTimer = 0;

        Debug.Log($"[RoundManager] KO! P1 Wins: {p1Wins} | P2 Wins: {p2Wins}");
    }

    // Wait after KO, then either end the match or start the next round
    private void UpdateKOPause()
    {
        stateTimer++;

        if (stateTimer >= koPauseDuration)
        {
            // Check if either player has won enough rounds
            if (p1Wins >= roundsToWin || p2Wins >= roundsToWin)
            {
                state = MatchState.MatchOver;
                stateTimer = 0;
                Debug.Log($"[RoundManager] Match Over! Winner: P{(p1Wins >= roundsToWin ? 1 : 2)}");
            }
            else
            {
                // More rounds to play
                state = MatchState.RoundReset;
                stateTimer = 0;
                currentRound++;
            }
        }
    }

    // Brief pause before starting the next round's countdown
    private void UpdateRoundReset()
    {
        stateTimer++;

        if (stateTimer >= resetDuration)
        {
            BeginCountdown();
        }
    }

    // Show the winner for a few seconds, then signal to return to menu
    private void UpdateMatchOver()
    {
        stateTimer++;

        if (stateTimer >= matchOverDuration)
        {
            matchOverTimerDone = true;
            Debug.Log("[RoundManager] Returning to menu");
        }
    }

    // Returns 1 or 2 for the winner, 0 if no one has won yet
    public int GetMatchWinner()
    {
        if (p1Wins >= roundsToWin) return 1;
        if (p2Wins >= roundsToWin) return 2;
        return 0;
    }

    // Returns the string to show in the center of the screen during countdown
    public string GetCountdownText()
    {
        if (countdownPhase > 0) return countdownPhase.ToString();
        if (countdownPhase == -1) return "FIGHT!";
        return "";
    }

    // Returns the string to show during KO pause or match over
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