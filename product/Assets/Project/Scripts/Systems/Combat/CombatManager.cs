using System.Collections.Generic;
using UnityEngine;

public class CombatManager
{
    private CombatDatabase combatDatabase = new CombatDatabase();
    private MoveExecutor moveExecutor;
    private CollisionBoxManager collisionBoxManager;

    private CombatData currentCombatData;
    private string currentMoveId;
    private HashSet<string> activeHitboxIds = new HashSet<string>();
    private Dictionary<string, CombatHitboxEntry> activeHitboxEntries = new Dictionary<string, CombatHitboxEntry>();
    private Dictionary<int, List<CombatHitboxEntry>> phases = new Dictionary<int, List<CombatHitboxEntry>>();

    public CombatManager(MoveExecutor moveExecutor, CollisionBoxManager collisionBoxManager)
    {
        this.moveExecutor = moveExecutor;
        this.collisionBoxManager = collisionBoxManager;
    }

    public void LoadCombat()
    {
        combatDatabase.ReadJson();
    }

    public void Update()
    {
        MoveData currentMove = moveExecutor.CurrentMove;
        if (currentMove == null) return;

        string moveId = currentMove.Id;
        int currentFrame = moveExecutor.FrameCounter.GetFrameNumber();

        if (moveId != currentMoveId)
        {
            DeactivateAll();
            currentMoveId = moveId;
            currentCombatData = combatDatabase.GetCombatData(moveId);
            BuildPhases();
        }

        if (currentCombatData == null) return;

        foreach (CombatHitboxEntry entry in currentCombatData.HitboxEntries)
        {
            bool shouldBeActive = currentFrame >= entry.StartFrame
                                  && currentFrame <= entry.EndFrame;
            bool isActive = activeHitboxIds.Contains(entry.HitboxId);

            if (shouldBeActive && !isActive)
            {
                collisionBoxManager.ActivateHitbox(entry.HitboxId, entry.SizeMultiplier);
                activeHitboxIds.Add(entry.HitboxId);
                activeHitboxEntries[entry.HitboxId] = entry;
            }
            else if (!shouldBeActive && isActive)
            {
                collisionBoxManager.DeactivateHitbox(entry.HitboxId);
                activeHitboxIds.Remove(entry.HitboxId);
                activeHitboxEntries.Remove(entry.HitboxId);
            }
        }
    }

    private void BuildPhases()
    {
        phases.Clear();
        if (currentCombatData == null) return;

        foreach (CombatHitboxEntry entry in currentCombatData.HitboxEntries)
        {
            if (!phases.ContainsKey(entry.StartFrame))
            {
                phases[entry.StartFrame] = new List<CombatHitboxEntry>();
            }
            phases[entry.StartFrame].Add(entry);
        }
    }

    private void DeactivateAll()
    {
        foreach (string id in activeHitboxIds)
        {
            collisionBoxManager.DeactivateHitbox(id);
        }
        activeHitboxIds.Clear();
        activeHitboxEntries.Clear();
        phases.Clear();
        currentCombatData = null;
    }

    public int GetPhaseIndex(CombatHitboxEntry entry)
    {
        return entry.StartFrame;
    }

    public CombatHitboxEntry GetActiveHitboxEntry(string hitboxId)
    {
        if (activeHitboxEntries.TryGetValue(hitboxId, out CombatHitboxEntry entry))
            return entry;
        return null;
    }

    public CombatData GetCurrentCombatData()
    {
        return currentCombatData;
    }

    public CombatDatabase GetCombatDatabase()
    {
        return combatDatabase;
    }
}