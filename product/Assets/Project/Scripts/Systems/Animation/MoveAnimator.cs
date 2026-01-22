using UnityEngine;

public class MoveAnimator : MonoBehaviour
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

    public void PlayAnimation(string clipName, float speed, float crossfade = 0.1f)
    {
        if (currentAnimation != clipName)
        {
            currentAnimation = clipName;
            animator.speed = speed;
            animator.CrossFade(clipName, crossfade);
        }
    }

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
