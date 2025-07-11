using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public abstract class DailyQuestReward 
{
    internal int amount;
    internal DailyQuestRewardType rewardType;

    internal DailyQuestReward(int amount)
    {
        this.amount = amount;
    }

    public virtual void Collect()
    {

    }
}

public enum DailyQuestRewardType
{
    Diamond,
    Decoration,
    Booster,
    Medicine,
    Coin,
    PositiveEnergy
}
