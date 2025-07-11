using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardBooster : GiftReward
{

    private int boosterID;

    public GiftRewardBooster(int amount, int boosterID) : base(amount)
    {
        this.boosterID = boosterID;
        rewardType = GiftRewardType.Booster;
    }

    public int GetBoosterId()
    {
        return boosterID;
    }

    public override void Collect()
    {
        for (int i = 0; i < amount; i++)
            Hospital.AreaMapController.Map.boosterManager.AddBooster(boosterID, EconomySource);
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo);
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon;
    }

    protected override void CollectAnimation()
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Booster, GetStartPoint(), amount, 0, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1.3f, 1.3f, 1), GetSprite(),
        () => { },
        () => { }
        );
    }
}
