using System;
using System.Collections.Generic;
using UnityEngine;

// Loads animation data from JSON and computes runtime frame counts for each animation
public class AnimationDatabase : BaseDatabase<AnimationData>
{

    public AnimationDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/animations.json";
    }

    // After loading JSON, compute totalFrames for every animation using actual clip lengths
    public void AddTotalFrames(AnimationExecutor animationExecutor)
    {
        foreach (var pair in dict)
        {
            pair.Value.InitialiseTotalFrames(animationExecutor);
        }
    }

    public AnimationData GetAnimationData(string id)
    {
        return dict[id];
    }

}