using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardGenerator
{
    private GiftRewardGeneratorData giftRewardGeneratorData;
    private GiftRewardContext giftRewardContext;

    public GiftRewardGenerator(GiftRewardGeneratorData giftRewardGeneratorData)
    {
        this.giftRewardGeneratorData = giftRewardGeneratorData;
        giftRewardContext = new GiftRewardContext(giftRewardGeneratorData);
    }

    public GiftReward GenerateReward()
    {
        GiftReward giftToReturn;
        giftToReturn = giftRewardContext.GetConcreteGiftReward(GetRandomGiftRewardType());
        
        if (giftToReturn != null)
        {
            return giftToReturn;
        }
        else
        {
            Debug.LogError("There was error in founding giftType. To not send null, coin reward was choosen.");
            giftToReturn = giftRewardContext.GetConcreteGiftReward(GiftRewardType.Coin);
            return giftToReturn;
        }
    }

    private GiftRewardType GetRandomGiftRewardType()
    {
        GiftRewardType giftToReturn;
        int random = Mathf.FloorToInt(GameState.RandomFloat(0, 1) * 100);
        int cumlative = 0;
        foreach (KeyValuePair<GiftRewardType, int> item in giftRewardGeneratorData.GetRewardProbabilityMap())
        {
            cumlative += item.Value;
            if (random < cumlative)
            {
                giftToReturn = item.Key;
                return giftToReturn;
            }
        }
        return GiftRewardType.Default;
    }
}
