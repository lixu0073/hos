using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrWiseFriend : BaseFollower {

    public const string WISE_SAVE = "SuperWise";
    public const int WISE_LEVEL = 100;

    public DrWiseFriend()
    {
        saveID = WISE_SAVE;
        level = WISE_LEVEL;
        avatar = ResourcesHolder.Get().wiseAvatar;
    }

    public override void SetSave(PublicSaveModel publicSave)
    {
       
    }
}
