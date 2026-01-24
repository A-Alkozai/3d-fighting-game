using UnityEngine;

public class AnimationManager
{
    private AnimationDatabase animationDatabase = new AnimationDatabase();
    private MoveAnimator moveAnimator;

    public AnimationManager(MoveAnimator moveAnimator)
    {
        this.moveAnimator = moveAnimator;
    }

    public void LoadAnimations()
    {
        animationDatabase.ReadJson();
        animationDatabase.AddTotalFrames(moveAnimator);
    }

    public AnimationDatabase GetAnimationDatabase()
    {
        return animationDatabase;
    }

    public void PlayAnimation(string animationId)
    {
        AnimationData data = animationDatabase.GetAnimationData(animationId);
        moveAnimator.PlayAnimation(data.Clip, data.Speed);
    }
}