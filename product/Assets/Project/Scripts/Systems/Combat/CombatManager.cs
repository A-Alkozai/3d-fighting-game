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
                Debug.Log($"[CombatManager] Activated hitbox: {entry.HitboxId} at frame {currentFrame}");
            }
            else if (!shouldBeActive && isActive)
            {
                collisionBoxManager.DeactivateHitbox(entry.HitboxId);
                activeHitboxIds.Remove(entry.HitboxId);
                Debug.Log($"[CombatManager] Deactivated hitbox: {entry.HitboxId} at frame {currentFrame}");
            }
        }
    }

    private void DeactivateAll()
    {
        foreach (string id in activeHitboxIds)
        {
            collisionBoxManager.DeactivateHitbox(id);
        }
        activeHitboxIds.Clear();
        currentCombatData = null;
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