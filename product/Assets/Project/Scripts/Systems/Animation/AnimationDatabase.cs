using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDatabase : BaseDatabase<AnimationData>
{

    public AnimationDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/animations.json";
    }

    public AnimationData GetAnimationData(string id)
    {
        return dict[id];
    }

}