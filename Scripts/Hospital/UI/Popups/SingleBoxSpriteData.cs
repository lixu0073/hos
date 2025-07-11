using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SingleBoxSpriteData
{
    public enum BundledResourceSprite
    {
        none,
        dailyRewardBoxWeek1,
        goodieBox,
        specialBox,
        premiumBox,
        pinkBox,
    }

    public BundledResourceSprite bundleReourceType;
    public Sprite GeneralIcon;
    public Sprite TopCover;
    public Sprite BottomBox;
}
