using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DailyRewardModel
{
    public bool isClaimed;
    private BaseGiftableResource reward;

    public DailyRewardModel(BaseGiftableResource reward)
    {
        this.reward = reward;
        isClaimed = false;
    }

    public void CollectReward(bool updateCounterInstant, bool spawnParticle)
    {
        reward.Collect(updateCounterInstant);
        isClaimed = true;
        if ((spawnParticle || !updateCounterInstant) && !(reward.rewardType == BaseGiftableResourceFactory.BaseResroucesType.bundle))
        {
            reward.SpawnParticle(Vector2.zero);
        }
    }

    public BaseGiftableResource GetDailyRewardGift()
    {
        return reward;
    }
}
