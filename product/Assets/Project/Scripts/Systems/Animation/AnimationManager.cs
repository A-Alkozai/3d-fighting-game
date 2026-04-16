using UnityEngine;

// Bridge between MoveExecutor and AnimationExecutor - looks up animation data by move ID and plays it
public class AnimationManager
{
    private AnimationDatabase animationDatabase = new AnimationDatabase();
    private AnimationExecutor animationExecutor;

    public AnimationManager(AnimationExecutor animationExecutor)
    {
        this.animationExecutor = animationExecutor;
    }

    // Load animation JSON and compute frame counts from actual clip lengths
    public void LoadAnimations()
    {
        animationDatabase.ReadJson();
        animationDatabase.AddTotalFrames(animationExecutor);
    }

    public AnimationDatabase GetAnimationDatabase()
    {
        return animationDatabase;
    }

    // Look up animation data by ID and tell the executor to play it
    public void PlayAnimation(string animationId)
    {
        AnimationData data = animationDatabase.GetAnimationData(animationId);
        animationExecutor.PlayAnimation(data.Clip, data.Speed);
    }

    // Get how many logic frames an animation takes (used by MoveExecutor to know when a move ends)
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