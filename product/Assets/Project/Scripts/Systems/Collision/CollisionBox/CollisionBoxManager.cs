using System.Collections.Generic;
using UnityEngine;

// Manages all collision boxes and the body collider for one player
// Handles crouching transitions and hitbox activation/deactivation
public class CollisionBoxManager
{
    private CollisionBoxDatabase collisionBoxDatabase;
    private BodyColliderDatabase bodyColliderDatabase;
    private StateManager stateManager;
    private bool isCrouching = false; // Tracks current stance to detect transitions

    public CollisionBoxManager(StateManager stateManager)
    {
        this.stateManager = stateManager;
        collisionBoxDatabase = new CollisionBoxDatabase();
        bodyColliderDatabase = new BodyColliderDatabase();
    }

    // Load JSON data and create all collision objects on the player's bones
    public void Load(Transform body, Dictionary<string, Transform> bones)
    {
        collisionBoxDatabase.ReadJson();
        collisionBoxDatabase.Initialise(bones);

        bodyColliderDatabase.ReadJson();
        bodyColliderDatabase.Initialise(body);
    }

    // Check if crouch state changed and switch all boxes between standing/crouching sizes
    public void Update()
    {
        bool currentlyCrouching = stateManager.HasState(PlayerStates.Crouching);

        if (currentlyCrouching != isCrouching)
        {
            isCrouching = currentlyCrouching;

            foreach (CollisionBox box in collisionBoxDatabase.GetAllCollisionBoxes().Values)
            {
                if (isCrouching)
                    box.SetCrouching();
                else
                    box.SetStanding();
            }
        }
    }

    // Activate a specific hitbox by bone ID (default size)
    public void ActivateHitbox(string id)
    {
        CollisionBox box = collisionBoxDatabase.GetCollisionBox(id);
        if (box != null) box.ActivateHitbox();
    }

    // Activate a specific hitbox with a combat-defined size multiplier
    public void ActivateHitbox(string id, float sizeMultiplier)
    {
        CollisionBox box = collisionBoxDatabase.GetCollisionBox(id);
        if (box != null) box.ActivateHitbox(sizeMultiplier);
    }

    public void DeactivateHitbox(string id)
    {
        CollisionBox box = collisionBoxDatabase.GetCollisionBox(id);
        if (box != null) box.DeactivateHitbox();
    }

    // Turn off all hitboxes (used when a move ends or is cancelled)
    public void DeactivateAllHitboxes()
    {
        foreach (CollisionBox box in collisionBoxDatabase.GetAllCollisionBoxes().Values)
        {
            box.DeactivateHitbox();
        }
    }

    // Return only the hitboxes that are currently enabled (mid-attack)
    public List<CollisionBox> GetActiveHitboxes()
    {
        List<CollisionBox> active = new List<CollisionBox>();
        foreach (CollisionBox box in collisionBoxDatabase.GetAllCollisionBoxes().Values)
        {
            if (box.HitboxActive) active.Add(box);
        }
        return active;
    }

    // Return all hurtboxes (always active, used for receiving hits)
    public IEnumerable<CollisionBox> GetAllHurtboxes()
    {
        return collisionBoxDatabase.GetAllCollisionBoxes().Values;
    }

    public BodyCollider GetBodyCollider()
    {
        return bodyColliderDatabase.GetBodyCollider();
    }

    public CollisionBox GetCollisionBox(string id)
    {
        return collisionBoxDatabase.GetCollisionBox(id);
    }

    // Draw all collision volumes as wireframe cubes in the editor
    public void OnDrawGizmos()
    {
        BodyCollider body = bodyColliderDatabase?.GetBodyCollider();
        if (body != null) body.DrawGizmos();

        var boxes = collisionBoxDatabase?.GetAllCollisionBoxes();
        if (boxes != null)
        {
            foreach (CollisionBox box in boxes.Values)
            {
                box.DrawGizmos();
            }
        }
    }
}