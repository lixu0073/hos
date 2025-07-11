using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardContext
{
    private Dictionary<GiftRewardType, GiftRewardStrategies> strategies;

    public GiftRewardContext(GiftRewardGeneratorData data)
    {
        GiftRewardStrategies standardStrategy = new StandardGiftRewardStrategy(data);
        GiftRewardStrategies mixtureStrategy = new MixtureGiftRewardStrategy(data);

        strategies = new Dictionary<GiftRewardType, GiftRewardStrategies>()
        {
            {GiftRewardType.Coin, standardStrategy},
            {GiftRewardType.Diamond, standardStrategy },
            {GiftRewardType.Mixture, mixtureStrategy },
            {GiftRewardType.PositiveEnergy, standardStrategy },
            {GiftRewardType.Shovel, standardStrategy },
            {GiftRewardType.StorageUpgrader, standardStrategy }
        };
    }

    public GiftReward GetConcreteGiftReward(GiftRewardType giftType)
    {
        GiftRewardStrategies dailyTaskStrategy;

        if (strategies.TryGetValue(giftType, out dailyTaskStrategy))
        {
            return dailyTaskStrategy.GetGiftReward(giftType);
        }
        Debug.LogError("No strategy found.It will return null");
        return null;
    }
}
