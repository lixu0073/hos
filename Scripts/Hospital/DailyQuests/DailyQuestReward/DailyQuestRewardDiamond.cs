using UnityEngine;
using System.Collections.Generic;
using System;

public class DailyQuestRewardDiamond : DailyQuestReward
{
    public DailyQuestRewardDiamond(int amount):base(amount)
    {
        this.rewardType = DailyQuestRewardType.Diamond;
    }

    public override void Collect()
    {
        GameState.Get().AddDiamonds(amount, EconomySource.DailyQuestReward, false);
    }
}
