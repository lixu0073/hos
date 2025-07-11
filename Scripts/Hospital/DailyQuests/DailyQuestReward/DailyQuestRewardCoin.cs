using UnityEngine;
using System.Collections.Generic;
using System;

public class DailyQuestRewardCoin : DailyQuestReward
{
    public DailyQuestRewardCoin(int amount):base(amount)
    {
        this.rewardType = DailyQuestRewardType.Coin;
    }

    public override void Collect()
    {
        GameState.Get().AddCoins(amount, EconomySource.DailyQuestReward, false);
    }
}
