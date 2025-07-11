using UnityEngine;
using System.Collections;
using System;

public class RewardsForTwoDailyTask : DailyQuestRewardFactory
{
    MedicineRef medicineAsReward;
    int GoldAmount;
    

    public override RewardPackage GetRewardPackage(int dayNumber)
    {
        rewardPackage = new RewardPackage(dayNumber);
        GetAllAvaiableMedicine();
        GetAllCurrentlyNeededMedicine();
        medicineAsReward = GetMedicine();
        GoldAmount = AmountOfGoldAsReward(dayNumber);
        DailyQuestReward rewardForOneDailyTask = new DailyQuestRewardMedicine(medicineAsReward, AmountOfMmedicineAsReward(medicineAsReward));
        DailyQuestReward rewardForSecondDailyTask = new DailyQuestRewardCoin(GoldAmount);
        rewardPackage.RewardListInPackage.Add(rewardForOneDailyTask);
        rewardPackage.RewardListInPackage.Add(rewardForSecondDailyTask);
        rewardPackage.SetRewardQuality(RewardPackage.RewardQuality.Starx2);

        return rewardPackage;
    }
}