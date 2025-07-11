using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SimpleUI;

public class GlobalEventBoosterRewardPackage : GlobalEventRewardPackage {

    public int boosterID { get; protected set; }

    public GlobalEventBoosterRewardPackage() : base()
    {
        this.rewardType = GlobalEventRewardType.Booster;
    }

    public GlobalEventBoosterRewardPackage(int boosterID, int amount)
    {
        this.boosterID = boosterID;
        this.amount = amount;
        this.rewardType = GlobalEventRewardType.Booster;
    }


    public override void Collect(Vector2 startPoint = default(Vector2), bool instantOpen = false)
    {
        if (!this.claimed && boosterID >= 0)
        {
            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                    HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(boosterID, EconomySource.BubbleBoy);

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Booster, startPoint, amount, .75f, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon, null, () =>
                    {
                    });
                }
            }

            this.claimed = true;
        }
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo);
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().boosterDatabase.boosters[this.boosterID].icon;
    }

    public override void LoadFromString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return;

        base.LoadFromString(saveString);

        var tmp = saveString.Split('?');

        if (tmp!=null && tmp.Length>2)
            boosterID = int.Parse((tmp[3]), System.Globalization.CultureInfo.InvariantCulture);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("?");
        builder.Append(boosterID);
        return builder.ToString();
    }
}
