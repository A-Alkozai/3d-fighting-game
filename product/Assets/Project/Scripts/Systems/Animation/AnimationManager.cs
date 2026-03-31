using UnityEngine;

public class AnimationManager
{
    private AnimationDatabase animationDatabase = new AnimationDatabase();
    private AnimationExecutor animationExecutor;

    public AnimationManager(AnimationExecutor animationExecutor)
    {
        this.animationExecutor = animationExecutor;
    }

    public void LoadAnimations()
    {
        animationDatabase.ReadJson();
        animationDatabase.AddTotalFrames(animationExecutor);
    }

    public AnimationDatabase GetAnimationDatabase()
    {
        return animationDatabase;
    }

    public void PlayAnimation(string animationId)
    {
        AnimationData data = animationDatabase.GetAnimationData(animationId);
        animationExecutor.PlayAnimation(data.Clip, data.Speed);
    }

    public int GetAnimationFrames(string animationId)
    {
        AnimationData data = animationDatabase.GetAnimationData(animationId);
        if (data == null)
        {
            Debug.LogWarning($"[AnimationManager] No animation found for: {animationId}");
            return 60;
        }
        return data.TotalFrames;
    }
}