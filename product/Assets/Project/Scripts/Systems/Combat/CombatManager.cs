using System.Collections.Generic;
using UnityEngine;

// Per-player combat manager - activates/deactivates hitboxes each frame based on the current move's combat data
public class CombatManager
{
    private CombatDatabase combatDatabase = new CombatDatabase();
    private MoveExecutor moveExecutor;
    private CollisionBoxManager collisionBoxManager;

    private CombatData currentCombatData;
    private string currentMoveId;
    private HashSet<string> activeHitboxIds = new HashSet<string>();                        // Which hitboxes are currently enabled
    private Dictionary<string, CombatHitboxEntry> activeHitboxEntries = new Dictionary<string, CombatHitboxEntry>(); // Maps hitbox ID to its phase entry
    private Dictionary<int, List<CombatHitboxEntry>> phases = new Dictionary<int, List<CombatHitboxEntry>>();        // Entries grouped by start frame

    public CombatManager(MoveExecutor moveExecutor, CollisionBoxManager collisionBoxManager)
    {
        this.moveExecutor = moveExecutor;
        this.collisionBoxManager = collisionBoxManager;
    }

    public void LoadCombat()
    {
        combatDatabase.ReadJson();
    }

    // Each frame: check if the move changed, then activate/deactivate hitboxes based on current frame
    public void Update()
    {
        MoveData currentMove = moveExecutor.CurrentMove;
        if (currentMove == null) return;

        string moveId = currentMove.Id;
        int currentFrame = moveExecutor.FrameCounter.GetFrameNumber();

        // If the move changed, clear old hitboxes and load new combat data
        if (moveId != currentMoveId)
        {
            DeactivateAll();
            currentMoveId = moveId;
            currentCombatData = combatDatabase.GetCombatData(moveId);
            BuildPhases();
        }

        if (currentCombatData == null) return;

        // Activate hitboxes that should be on this frame, deactivate ones that shouldn't
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

    // Group hitbox entries by their start frame (used for phase-level hit tracking)
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

    // Turn off all active hitboxes and clear tracking (used on move change or cancel)
    public void DeactivateAll()
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

    // Look up the active phase entry for a specific hitbox (used by CollisionManager during hit checks)
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