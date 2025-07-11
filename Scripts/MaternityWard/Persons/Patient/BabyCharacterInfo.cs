using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyCharacterInfo : BaseCharacterInfo
{
    public override string GetLikesString()
    {
        return I2.Loc.ScriptLocalization.Get("BABY_LIKES/LIKES_" + Likes);
    }

    public override string GetDislikesString()
    {
        return "";
    }
}
