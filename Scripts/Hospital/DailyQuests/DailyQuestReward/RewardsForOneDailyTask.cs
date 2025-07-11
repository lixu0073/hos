using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Hospital;

public class RewardsForOneDailyTask : DailyQuestRewardFactory {

    private MedicineRef medicineAsReward;

    public override RewardPackage GetRewardPackage(int dayNumber)
    {
        rewardPackage = new RewardPackage(dayNumber);
        GetAllAvaiableMedicine();
        GetAllCurrentlyNeededMedicine();
        medicineAsReward = GetMedicine();
        DailyQuestReward rewardForOneDailyTask = new DailyQuestRewardMedicine(medicineAsReward, AmountOfMmedicineAsReward(medicineAsReward));
        rewardPackage.RewardListInPackage.Add(rewardForOneDailyTask);
        rewardPackage.SetRewardQuality(RewardPackage.RewardQuality.Starx1);

        return rewardPackage;
    }
}