using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BaseResourceSpriteData
{

    public enum SpriteType
    {
        dynamic,
        coin1,
        coin2,
        coin3,
        coin4,
        coin5,
        coin6,
        diamond1,
        diamond2,
        diamond3,
        diamond4,
        diamond5,
        diamond6,
        positiveEnergy1,
    }

    public SpriteType spriteType;
    public Sprite sprite;

}
