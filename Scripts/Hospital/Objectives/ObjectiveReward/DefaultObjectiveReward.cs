using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DefaultObjectiveReward
{
    public int Amount
    {
        get
        {
            return (int)(InnerAmount * RewardForTODOsCoins);
        }
        private set { }
    }

    private int InnerAmount;

    private Hospital.BalanceableFloat rewardForTODOsCoinsBalanceable;
    private float RewardForTODOsCoins
    {
        get
        {
            if(rewardForTODOsCoinsBalanceable == null)
            {
                rewardForTODOsCoinsBalanceable = Hospital.BalanceableFactory.CreateRewardForTODOsCoinsBalanceable();
            }
            return rewardForTODOsCoinsBalanceable.GetBalancedValue();
        }
    }

    public bool Claimed
    {
        get;
        private set;
    }

    public ResourceType RewardType
    {
        get;
        private set;
    }

    public DefaultObjectiveReward(ResourceType rewardType, bool claimed)
    {
        this.RewardType = rewardType;
        InnerAmount = AlgorithmHolder.GetObjectiveRewardAmount();
        this.Claimed = claimed;
    }

    public DefaultObjectiveReward(int amount, ResourceType rewardType, bool claimed)
    {
        this.RewardType = rewardType;
        InnerAmount = amount;
        this.Claimed = claimed;
    }

    public void Collect(Vector2 startPoint = default(Vector2), bool delayed = true)
    {
        if (Amount > 0 && !Claimed)
        {
            if (RewardType == ResourceType.Coin)
            {
                int currentAmount = Game.Instance.gameState().GetCoinAmount();
                GameState.Get().AddResource(ResourceType.Coin, Amount, EconomySource.LevelGoalReward, false);
                Claimed = true;

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Coin, startPoint, Amount, delayed ? 0 : 0.75f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().bbCoinSprite, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Coin, Amount, currentAmount);
                        });
                }
            }
            else if (RewardType == ResourceType.Diamonds)
            {
                int currentAmount = Game.Instance.gameState().GetDiamondAmount();
                GameState.Get().AddResource(ResourceType.Diamonds, Amount, EconomySource.LevelGoalReward, false);
                Claimed = true;

                if (startPoint != default(Vector2))
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Diamond, startPoint, Amount, delayed ? 0 : 0.5f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().bbDiamondSprite, null, () =>
                    {
                        GameState.Get().UpdateCounter(ResourceType.Diamonds, Amount, currentAmount);
                    });
                }
            }
        }
    }


    public static DefaultObjectiveReward Parse(string str)
    {
        var tmp = str.Split('^');

        return new DefaultObjectiveReward(int.Parse(tmp[1], System.Globalization.CultureInfo.InvariantCulture), (ResourceType)Enum.Parse(typeof(ResourceType), tmp[0]), bool.Parse(tmp[2]));
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(RewardType.ToString());
        builder.Append("^");
        builder.Append(Checkers.CheckedAmount(InnerAmount, 0, int.MaxValue, "DefaultObjectiveReward reward: ").ToString());
        builder.Append("^");
        builder.Append(Claimed.ToString());
        return builder.ToString();
    }

    public string GetName()
    {
        if (RewardType == ResourceType.Diamonds)
            return I2.Loc.ScriptLocalization.Get("DIAMONDS");
        else if (RewardType == ResourceType.Coin)
            return I2.Loc.ScriptLocalization.Get("COINS");
        else return "";
    }

    public Sprite GetSprite()
    {
        if (RewardType == ResourceType.Diamonds)
            return ResourcesHolder.Get().diamondSprite;
        else if (RewardType == ResourceType.Coin)
            return ResourcesHolder.Get().coinSprite;
        else return null;
    }
}

