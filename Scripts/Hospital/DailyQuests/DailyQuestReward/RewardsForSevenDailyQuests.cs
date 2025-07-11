using UnityEngine;
using System.Collections;
using System;

public class RewardsForSevenDailyQuests : DailyQuestRewardFactory
{
    int GoldAmount;
    int DiamondAmount;
    int ShovelAmount;
    int DecorationAmount;
    int BoosterAmount;

    public override RewardPackage GetRewardPackage(int dayNumber)
    {
        rewardPackage = new RewardPackage(dayNumber);
        GoldAmount = 1500;
        DiamondAmount = 3;
        ShovelAmount = 1;
        DecorationAmount = 1;
        BoosterAmount = 1;
        DailyQuestReward goldReward = new DailyQuestRewardCoin(GoldAmount);
        DailyQuestReward diamondReward = new DailyQuestRewardDiamond(DiamondAmount);
        DailyQuestReward itemReward = new DailyQuestRewardMedicine(GameState.Get().GetRandomSpecial(GameState.DrawShovelSource.DailyQuest), ShovelAmount);
        DailyQuestReward decorationReward = new DailyQuestRewardDecoration(DecorationAmount);
        DailyQuestReward boosterReward = GameState.RandomNumber(0, 2) == 0 ? 
            new DailyQuestRewardBooster(DailyQuestRewardBooster.DailyQuestRewardsBoosters.HappyHourBooster, BoosterAmount) : new DailyQuestRewardBooster(DailyQuestRewardBooster.DailyQuestRewardsBoosters.PremiumBooster, BoosterAmount);
        rewardPackage.RewardListInPackage.Add(goldReward);
        rewardPackage.RewardListInPackage.Add(diamondReward);
        rewardPackage.RewardListInPackage.Add(itemReward);
        rewardPackage.RewardListInPackage.Add(decorationReward);
        rewardPackage.RewardListInPackage.Add(boosterReward);
        rewardPackage.SetRewardQuality(RewardPackage.RewardQuality.SuperGrand);
        return rewardPackage;
        //
    }
}
