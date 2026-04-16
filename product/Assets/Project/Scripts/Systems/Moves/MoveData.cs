using System;
using System.Collections.Generic;
using UnityEngine;

// JSON data for a single move - contains input sequence, required states, timing, and type
[Serializable]
public class MoveData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private string moveName;
    [SerializeField] private string description;
    [SerializeField] private string moveType;            // "movement", "attack", or "state"
    [SerializeField] private bool isLoop;                // If true, move repeats until input released
    [SerializeField] private int inputDelay;
    [SerializeField] private int branchDelay;            // Frame deadline to branch into the next move in a chain
    [SerializeField] private List<string> inputSequence; // Raw input names from JSON
    [SerializeField] private List<string> requiredStates;// States the player must be in to execute this move

    private int totalFrames;                              // Set at runtime from animation data
    private List<InputCommand> inputSequenceObj = new List<InputCommand>();   // Parsed input enums
    private List<PlayerStates> requiredStatesObj = new List<PlayerStates>(); // Parsed state enums

    public string Id => id;
    public string MoveName => moveName;
    public string Description => description;
    public string MoveType => moveType;
    public bool IsLoop => isLoop;
    public int TotalFrames => totalFrames;
    public int InputDelay => inputDelay;
    public int BranchDelay => branchDelay;
    public IReadOnlyList<InputCommand> InputSequence => inputSequenceObj;
    public IReadOnlyList<PlayerStates> RequiredStates => requiredStatesObj;

    // Parse raw string lists into enums
    public void InitialiseObjects()
    {
        InitialiseInputs();
        InitialiseStates();
    }

    // Set total frames from the matching animation's computed frame count
    public void LoadTotalFrames(AnimationData animationData)
    {
        totalFrames = animationData.TotalFrames;
    }

    // Convert input sequence strings (e.g. "Forward", "LeftPunch") to InputCommand enums
    private void InitialiseInputs()
    {
        foreach (string rawInputCommand in inputSequence)
        {
            if (Enum.TryParse<InputCommand>(rawInputCommand, out InputCommand inputCommand))
            {
                inputSequenceObj.Add(inputCommand);
            }
        }
    }

    // Convert required state strings (e.g. "Idle", "StandGuarding") to PlayerStates enums
    private void InitialiseStates()
    {
        foreach (string rawState in requiredStates)
        {
            if (Enum.TryParse<PlayerStates>(rawState, out PlayerStates playerState))
            {
                requiredStatesObj.Add(playerState);
            }
        }
    }
}