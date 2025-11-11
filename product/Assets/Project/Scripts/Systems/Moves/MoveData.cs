using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveData
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

    [SerializeField] private List<InputCommand> inputSequence;
    [SerializeField] private List<string> branchMoves;
    [SerializeField] private List<PlayerStates> requiredStates;

    public string Id => id;
    public string MoveName => moveName;
    public string Description => description;

    public int StartupFrames => startupFrames;
    public int ActiveFrames => activeFrames;
    public int RecoveryFrames => recoveryFrames;
    public int TotalFrames => totalFrames;

    public int InputDelay => inputDelay;
    public int BranchDelay => branchDelay;

    public IReadOnlyList<InputCommand> InputSequence => inputSequence;
    public IReadOnlyList<string> BranchMoves => branchMoves;
    public IReadOnlyList<PlayerStates> RequiredStates => requiredStates;
}