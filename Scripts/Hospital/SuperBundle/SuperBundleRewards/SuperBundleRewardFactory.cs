using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SuperBundleRewardFactory
{

	public static void Load(string unparsedRewards, SuperBundlePackage package, List<ISuperBundleReward> rewards)
    {
        string[] unparsedArrayRewards = unparsedRewards.Split('*');
        foreach(string unparsedReward in unparsedArrayRewards)
        {
            try
            {
                ISuperBundleReward reward = GetRewardInstance(unparsedReward);
                if(reward is SuperBundleRewardCoin || reward is SuperBundleRewardDiamond)
                {
                    package.SetMainReward(reward);
                }
                else
                {
                    rewards.Add(reward);
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Parse Error: " + e.Message);
            }
        }
    }

    public static List<ISuperBundleReward> Parse(string unparsedRewards)
    {
        List<ISuperBundleReward> rewards = new List<ISuperBundleReward>();
        string[] unparsedArrayRewards = unparsedRewards.Split('*');
        foreach (string unparsedReward in unparsedArrayRewards)
        {
            try
            {
                rewards.Add(GetRewardInstance(unparsedReward));
            }
            catch (Exception e)
            {
                Debug.LogError("Reward Parse Error: " + e.Message);
            }
        }
        return rewards;
    }

    private static ISuperBundleReward GetRewardInstance(string unparsedReward)
    {
        string[] unparsedArrayReward = unparsedReward.Split('!');
        if (unparsedArrayReward.Length != 3)
            throw new Exception("Incorrect length of reward array");

        if (!Enum.IsDefined(typeof(DailyQuestRewardType), unparsedArrayReward[0]))
            throw new Exception("Invalid type of reward");

        DailyQuestRewardType rewardType = (DailyQuestRewardType)Enum.Parse(typeof(DailyQuestRewardType), unparsedArrayReward[0]);

        switch(rewardType)
        {
            case DailyQuestRewardType.Coin:
                return SuperBundleRewardCoin.GetInstance(unparsedArrayReward);
            case DailyQuestRewardType.Diamond:
                return SuperBundleRewardDiamond.GetInstance(unparsedArrayReward);
            case DailyQuestRewardType.Booster:
                return SuperBundleRewardBooster.GetInstance(unparsedArrayReward);
            case DailyQuestRewardType.Medicine:
                return SuperBundleRewardMedicine.GetInstance(unparsedArrayReward);
            case DailyQuestRewardType.Decoration:
                return SuperBundleRewardDecoration.GetInstance(unparsedArrayReward);
            case DailyQuestRewardType.PositiveEnergy:
                return SuperBundleRewardPositiveEnergy.GetInstance(unparsedArrayReward);
        }
        throw new Exception("Invalid reward type");
    }

}
