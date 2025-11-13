using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveData : IIdentifiable
{
    [SerializeField] private string id;
    [SerializeField] private string moveName;
    [SerializeField] private string description;

    [SerializeField] private int startupFrames;
    [SerializeField] private int activeFrames;
    [SerializeField] private int recoveryFrames;
    [SerializeField] private int totalFrames;

    [SerializeField] private int inputDelay;
    [SerializeField] private int branchDelay;

    [SerializeField] private List<string> inputSequence;
    [SerializeField] private List<string> branchMoves;
    [SerializeField] private List<string> requiredStates;

    private List<InputCommand> inputSequenceObj = new List<InputCommand>();
    private List<PlayerStates> requiredStatesObj = new List<PlayerStates>();

    private MoveData prevMove;
    private Dictionary<string, MoveData> nextMoves = new Dictionary<string, MoveData>();

    public string Id => id;
    public string MoveName => moveName;
    public string Description => description;

    public int StartupFrames => startupFrames;
    public int ActiveFrames => activeFrames;
    public int RecoveryFrames => recoveryFrames;
    public int TotalFrames => totalFrames;

    public int InputDelay => inputDelay;
    public int BranchDelay => branchDelay;

    public IReadOnlyList<InputCommand> InputSequence => inputSequenceObj;
    public IReadOnlyList<PlayerStates> RequiredStates => requiredStatesObj;
    public MoveData PrevMove => prevMove;
    public IReadOnlyDictionary<string, MoveData> NextMoves => nextMoves;

    internal void SetPrevMove(MoveData move)
    {
        prevMove = move;
    }

    public void InitialiseObjects(Dictionary<string, MoveData> dict)
    {
        InitialiseBranches(dict);
        InitialiseInputs();
        InitialiseStates();
    }

    private void InitialiseBranches(Dictionary<string, MoveData> dict)
    {
        foreach (string branchId in branchMoves)
        {
            if (dict.TryGetValue(branchId, out MoveData branchMove))
            {
                nextMoves[branchId] = branchMove;
                branchMove.SetPrevMove(this);
            }
            else
            {
                Debug.LogWarning($"Move '{Id}' contains unknown branch '{branchId}'");
            }
        }
    }

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