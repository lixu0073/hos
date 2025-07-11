using UnityEngine;
using System.Collections;
using Hospital;
using SimpleUI;
using System.Text;
using System;

public class BubbleBoyRewardBooster : BubbleBoyReward
{
    protected int boosterID;

    public BubbleBoyRewardBooster()
    {
        this.rewardType = BubbleBoyPrizeType.Booster;
    }

    public BubbleBoyRewardBooster(int boosterID, int amount)
    {
        this.boosterID = boosterID;
        this.amount = amount;
        this.rewardType = BubbleBoyPrizeType.Booster;

        if (boosterID>=0 && ResourcesHolder.Get() != null)
        {
            this.sprite = ResourcesHolder.Get().boosterDatabase.boosters[this.boosterID].icon;
        }
    }

    public BubbleBoyRewardBooster(int boosterID)
    {
        this.boosterID = boosterID;
        this.amount = 1;
        this.rewardType = BubbleBoyPrizeType.Booster;

        if (boosterID >= 0)
        {
            this.sprite = ResourcesHolder.Get().boosterDatabase.boosters[this.boosterID].icon;
        }
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(boosterID, 0, int.MaxValue, "BubbleBoyReward boosterId: ").ToString());
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split(';');
            boosterID = int.Parse(save[2], System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public override void Collect(float delay = 0f)
    {
        if (boosterID >= 0 && amount > 0)
        {
            for (int i = 0; i < amount; i++)
                HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(boosterID, EconomySource.BubbleBoy);
        }

        base.Collect(delay);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0f)
    {
        base.SpawnParticle(startPoint);

        if (boosterID >= 0 && amount > 0)
        {
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Booster, startPoint, amount, delay, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon, null, () =>
            {
            });
        }
    }

    public int GetBoosterID() {
        return boosterID;
    }

    public override bool IsAccesibleByPlayer()
    {
        return true;
    }
}
