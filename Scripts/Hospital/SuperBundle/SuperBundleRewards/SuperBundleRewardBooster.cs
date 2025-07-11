using UnityEngine;
using System.Collections;
using System;
using Hospital;

public class SuperBundleRewardBooster : BubbleBoyRewardBooster, ISuperBundleReward
{
    public static SuperBundleRewardBooster GetInstance(string[] unparsedArrayData)
    {
        int amount = int.Parse(unparsedArrayData[2], System.Globalization.CultureInfo.InvariantCulture);
        if (amount < 1)
            throw new Exception("Reward amount < 1");
        return new SuperBundleRewardBooster(int.Parse(unparsedArrayData[1], System.Globalization.CultureInfo.InvariantCulture), amount);
    }

    public Sprite GetSprite()
    {
        if(ResourcesHolder.Get() == null)
            sprite = ResourcesHolder.Get().boosterDatabase.boosters[this.boosterID].icon;
        return sprite;
    }

    public override string ToString()
    {
        return "{type: " + this.GetType().Name + ", boosterId: " + boosterID + ", amount: " + amount + "}";
    }

    public override void Collect(float delay = 0f)
    {
        if (boosterID >= 0 && amount > 0)
        {
            for (int i = 0; i < amount; i++)
            {
                HospitalAreasMapController.Map.boosterManager.AddBooster(boosterID, EconomySource.IAPShopBundle);
            }
            SpawnParticle(new Vector2(0, -130), delay);
        }
    }

    public SuperBundleRewardBooster(int boosterId, int amount) : base(boosterId, amount) {}

}
