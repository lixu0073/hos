using UnityEngine;
using System.Collections;
using SimpleUI;
using System;

public class BubbleBoyRewardCoin : BubbleBoyReward {

    public BubbleBoyRewardCoin()
    {
        this.rewardType = BubbleBoyPrizeType.Coin;
    }

    public BubbleBoyRewardCoin(int amount = 0)
    {
        if(ResourcesHolder.Get() != null)
		    this.sprite = ResourcesHolder.Get().bbCoinSprite;
        this.rewardType = BubbleBoyPrizeType.Coin;
        this.amount = amount;
    }

    public override void Collect(float delay = 0f)
    {
        if (amount > 0)
        {
            Game.Instance.gameState().AddResource(ResourceType.Coin, amount, EconomySource.BubbleBoy, false);
        }

        base.Collect(delay);
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get("COINS");
    }

    public override bool IsAccesibleByPlayer()
    {
        return true;
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0f)
    {
        base.SpawnParticle(startPoint);

        int coinAmountBeforeReward = Game.Instance.gameState().GetCoinAmount() - amount;
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, startPoint, amount, delay, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), sprite, null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Coin, amount, coinAmountBeforeReward);
        });
    }
}
