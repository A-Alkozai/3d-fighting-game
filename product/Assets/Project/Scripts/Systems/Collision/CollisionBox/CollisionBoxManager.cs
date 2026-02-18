using System.Collections.Generic;
using UnityEngine;

public class CollisionBoxManager
{
    private CollisionBoxDatabase collisionBoxDatabase;
    private BodyColliderDatabase bodyColliderDatabase;
    private StateManager stateManager;
    private bool isCrouching = false;

    public CollisionBoxManager(StateManager stateManager)
    {
        this.stateManager = stateManager;
        collisionBoxDatabase = new CollisionBoxDatabase();
        bodyColliderDatabase = new BodyColliderDatabase();
    }

    public void Load(Transform body, Dictionary<string, Transform> bones)
    {
        collisionBoxDatabase.ReadJson();
        collisionBoxDatabase.Initialise(bones);

        bodyColliderDatabase.ReadJson();
        bodyColliderDatabase.Initialise(body);
    }

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

    public void ActivateHitbox(string id)
    {
        CollisionBox box = collisionBoxDatabase.GetCollisionBox(id);
        if (box != null) box.ActivateHitbox();
    }

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

    public void DeactivateAllHitboxes()
    {
        foreach (CollisionBox box in collisionBoxDatabase.GetAllCollisionBoxes().Values)
        {
            box.DeactivateHitbox();
        }
    }

    public List<CollisionBox> GetActiveHitboxes()
    {
        List<CollisionBox> active = new List<CollisionBox>();
        foreach (CollisionBox box in collisionBoxDatabase.GetAllCollisionBoxes().Values)
        {
            if (box.HitboxActive) active.Add(box);
        }
        return active;
    }

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