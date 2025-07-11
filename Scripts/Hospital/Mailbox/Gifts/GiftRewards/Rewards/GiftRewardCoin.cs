using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardCoin : GiftReward
{
    public GiftRewardCoin(int amount) : base(amount)
    {
        this.amount = amount;
        rewardType = GiftRewardType.Coin;
    }

    public override void Collect()
    {
        Game.Instance.gameState().AddCoins(amount, EconomySource, true);
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get("COINS");
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().boxOpeningRewardAssets.goldStack;
    }

    protected override void CollectAnimation()
    {
        int currentCoinReward = Game.Instance.gameState().GetCoinAmount() - amount;
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, GetStartPoint(), amount, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), GetSprite(), null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Coin, amount, currentCoinReward);
        });
    }
}
