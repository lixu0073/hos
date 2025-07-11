using UnityEngine;
using System.Collections;
using System;

public class RewardsForThreeDailyQuests : DailyQuestRewardFactory
{
    int GoldAmount;
    int DiamondAmount;
    int ShovelAmount;
    int DecorationAmount;

    public override RewardPackage GetRewardPackage(int dayNumber)
    {
        rewardPackage = new RewardPackage(dayNumber);
        GoldAmount = 1000;
        DiamondAmount = 1;
        ShovelAmount = 1;
        DecorationAmount = 1;
        DailyQuestReward goldReward = new DailyQuestRewardCoin(GoldAmount);
        DailyQuestReward diamondReward = new DailyQuestRewardDiamond(DiamondAmount);
        DailyQuestReward itemReward = new DailyQuestRewardMedicine(GameState.Get().GetRandomSpecial(GameState.DrawShovelSource.DailyQuest), ShovelAmount);
        DailyQuestReward decorationReward = new DailyQuestRewardDecoration(DecorationAmount);
        rewardPackage.RewardListInPackage.Add(goldReward);
        rewardPackage.RewardListInPackage.Add(diamondReward);
        rewardPackage.RewardListInPackage.Add(itemReward);
        rewardPackage.RewardListInPackage.Add(decorationReward);
        rewardPackage.SetRewardQuality(RewardPackage.RewardQuality.Super);
        return rewardPackage;
        //
    }
}
