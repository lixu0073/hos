using UnityEngine;
using System.Collections;
using System;

public class RewardsForThreeTask : DailyQuestRewardFactory
{
    MedicineRef medicineAsReward;
    int GoldAmount;
    int DiamondAmount;

    public override RewardPackage GetRewardPackage(int dayNumber)
    {
        rewardPackage = new RewardPackage(dayNumber);
        GetAllAvaiableMedicine();
        GetAllCurrentlyNeededMedicine();
        medicineAsReward = GetMedicine();
        GoldAmount = AmountOfGoldAsReward(dayNumber);
        DiamondAmount = AmountOfDiamondsAsReward();
        DailyQuestReward rewardForOneDailyTask = new DailyQuestRewardMedicine(medicineAsReward, AmountOfMmedicineAsReward(medicineAsReward));
        DailyQuestReward rewardForSecondDailyTask = new DailyQuestRewardCoin(GoldAmount);
        DailyQuestReward rewardForThirdDailyTask = new DailyQuestRewardDiamond(DiamondAmount);
        rewardPackage.RewardListInPackage.Add(rewardForOneDailyTask);
        rewardPackage.RewardListInPackage.Add(rewardForSecondDailyTask);
        rewardPackage.RewardListInPackage.Add(rewardForThirdDailyTask);
        rewardPackage.SetRewardQuality(RewardPackage.RewardQuality.Starx3);

        return rewardPackage;
    }
}