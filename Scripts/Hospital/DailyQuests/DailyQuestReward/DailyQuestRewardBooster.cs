using UnityEngine;
using System.Collections.Generic;
using System;

public class DailyQuestRewardBooster : DailyQuestReward
{
    private int boosterID;

    public DailyQuestRewardBooster(DailyQuestRewardsBoosters boosterReward, int amount):base(amount)
    {
        if (boosterReward == DailyQuestRewardsBoosters.HappyHourBooster)
            boosterID = 7;
        else
            boosterID = 8;

        this.rewardType = DailyQuestRewardType.Booster;
    }

    public override void Collect()
    {
        for (int i = 0; i < amount; i++)
            Hospital.HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(boosterID, EconomySource.DailyQuestReward);
    }

    public int GetBoosterId()
    {
        return boosterID;
    }

    public enum DailyQuestRewardsBoosters
    {
        PremiumBooster,
        HappyHourBooster,
    }
}
