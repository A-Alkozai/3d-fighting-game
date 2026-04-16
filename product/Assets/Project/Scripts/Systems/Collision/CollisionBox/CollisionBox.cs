using UnityEngine;

// A single collision box attached to a bone - has a hurtbox (always active) and a hitbox (toggled on during attacks)
public class CollisionBox
{
    private string id;
    private GameObject hurtboxObject;
    private GameObject hitboxObject;
    private BoxCollider hurtboxCollider;
    private BoxCollider hitboxCollider;
    private CollisionBoxData data;

    public string Id => id;
    public bool HitboxActive => hitboxObject.activeSelf;

    public CollisionBox(string id, Transform bone, CollisionBoxData data)
    {
        this.id = id;
        this.data = data;

        InitialiseHurtbox(bone);
        InitialiseHitbox(bone);
    }

    // Create the hurtbox child object - always active, used to receive hits
    public void InitialiseHurtbox(Transform bone)
    {
        hurtboxObject = new GameObject(id + "_Hurtbox");
        hurtboxObject.transform.SetParent(bone);
        if (data.ResetRotation)
        {
            hurtboxObject.transform.localRotation = Quaternion.identity;
        }
        hurtboxObject.transform.localPosition = data.StandingOffset;
        hurtboxObject.layer = LayerMask.NameToLayer("Hurtbox");
        hurtboxCollider = hurtboxObject.AddComponent<BoxCollider>();
        hurtboxCollider.size = data.StandingSize;
        hurtboxCollider.isTrigger = true;
    }

    // Create the hitbox child object - starts disabled, activated during attack phases
    public void InitialiseHitbox(Transform bone)
    {
        hitboxObject = new GameObject(id + "_Hitbox");
        hitboxObject.transform.SetParent(bone);
        if (data.ResetRotation)
        {
            hitboxObject.transform.localRotation = Quaternion.identity;
        }
        hitboxObject.transform.localPosition = data.StandingOffset;
        hitboxObject.layer = LayerMask.NameToLayer("Hitbox");
        hitboxCollider = hitboxObject.AddComponent<BoxCollider>();
        hitboxCollider.size = data.StandingSize;
        hitboxCollider.isTrigger = true;
        hitboxObject.SetActive(false);
    }

    // Switch to standing collision sizes and offsets
    public void SetStanding()
    {
        hurtboxObject.transform.localPosition = data.StandingOffset;
        hurtboxCollider.size = data.StandingSize;
        hitboxObject.transform.localPosition = data.StandingOffset;
        hitboxCollider.size = data.StandingSize;
    }

    // Switch to crouching collision sizes and offsets (smaller/lower)
    public void SetCrouching()
    {
        hurtboxObject.transform.localPosition = data.CrouchingOffset;
        hurtboxCollider.size = data.CrouchingSize;
        hitboxObject.transform.localPosition = data.CrouchingOffset;
        hitboxCollider.size = data.CrouchingSize;
    }

    // Enable the hitbox at its default size
    public void ActivateHitbox()
    {
        hitboxObject.SetActive(true);
    }

    // Enable the hitbox with a size multiplier (from CombatHitboxEntry)
    public void ActivateHitbox(float sizeMultiplier)
    {
        hitboxObject.SetActive(true);
        hitboxCollider.size = hurtboxCollider.size * sizeMultiplier;
    }

    public void DeactivateHitbox()
    {
        hitboxObject.SetActive(false);
    }

    public void ActivateHurtbox()
    {
        hurtboxObject.SetActive(true);
    }

    public void DeactivateHurtbox()
    {
        hurtboxObject.SetActive(false);
    }

    public Bounds GetHitboxBounds()
    {
        return hitboxCollider.bounds;
    }

    public Bounds GetHurtboxBounds()
    {
        return hurtboxCollider.bounds;
    }

    // Draw wireframe boxes in the editor for debugging collision volumes
    public void DrawGizmos()
    {
        if (hurtboxObject != null && hurtboxObject.activeSelf)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = hurtboxObject.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hurtboxCollider.center, hurtboxCollider.size);
        }

        if (hitboxObject != null && hitboxObject.activeSelf)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = hitboxObject.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hitboxCollider.center, hitboxCollider.size);
        }
    }
}