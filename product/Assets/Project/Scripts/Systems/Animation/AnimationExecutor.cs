using UnityEngine;

// Low-level animation player - uses Animator with an override controller to crossfade clips
public class AnimationExecutor : MonoBehaviour
{
    private Animator animator;
    private AnimatorOverrideController overrideController;
    private string currentAnimation = "Idle";

    void Awake()
    {
        animator = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    // Crossfade to a new clip if it's different from the current one
    public void PlayAnimation(string clipName, float speed, float crossfade = 0.1f)
    {
        if (currentAnimation != clipName)
        {
            currentAnimation = clipName;
            animator.speed = speed;
            animator.CrossFade(clipName, crossfade);
        }
    }

    // Search all clips on the animator to find the length of a clip by name
    public float GetClipLength(string clipName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return -1f;
    }
}