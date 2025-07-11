using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventRewardModel
{
    public bool IsClaimed;
    private BaseGiftableResource reward;

    private const char CLAIMED_TAG = '=';

    public GlobalEventRewardModel(BaseGiftableResource reward)
    {
        this.reward = reward;
        this.IsClaimed = false;
    }

    public void CollectReward(bool updateCounterInstant, bool spawnParticle)
    {
        reward.Collect(updateCounterInstant);
        IsClaimed = true;
        if ((spawnParticle || !updateCounterInstant) && !(reward.rewardType == BaseGiftableResourceFactory.BaseResroucesType.bundle))
        {
            reward.SpawnParticle(Vector2.zero);
        }
    }

    public BaseGiftableResource GetGlobalEventGift()
    {
        return reward;
    }    

    public string SaveToString()
    {
        return reward.GiftableToString() + CLAIMED_TAG + IsClaimed.ToString();
    }

    public static GlobalEventRewardModel Parse(string rewardToParse)
    {
        string[] parts = rewardToParse.Split(CLAIMED_TAG);

        BaseGiftableResource result;
        
        result = BaseGiftableResourceFactory.CreateGiftableFromString(parts[0], EconomySource.GlobalEventContribution);
        GlobalEventRewardModel res = new GlobalEventRewardModel(result);

        bool claimed;
        if (parts.Length > 1 && bool.TryParse(parts[1], out claimed))
        {
            res.IsClaimed = claimed;
        }        

        return res;
    }
}
