using UnityEngine;
using System.Collections;
using SimpleUI;
using System;

public class BubbleBoyRewardDiamond : BubbleBoyReward
{

    public BubbleBoyRewardDiamond()
    {
        this.rewardType = BubbleBoyPrizeType.Diamond;
    }

    public BubbleBoyRewardDiamond(int amount)
    {
        if (ResourcesHolder.Get() != null)
            this.sprite = ResourcesHolder.GetHospital().bbDiamondSprite;
        this.rewardType = BubbleBoyPrizeType.Diamond;
        this.amount = amount;
    }

    public override void Collect(float delay = 0f)
    {
        if (amount > 0)
        {
            GameState.Get().AddResource(ResourceType.Diamonds, amount, EconomySource.BubbleBoy, false);
        }

        base.Collect(delay);
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get("DIAMONDS");
    }

    public override bool IsAccesibleByPlayer()
    {
        return true;
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0f)
    {
        base.SpawnParticle(startPoint);

        if (amount > 0)
        {
            int currentAmount = Game.Instance.gameState().GetDiamondAmount() - amount;
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, startPoint, amount, delay, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), sprite, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Diamonds, amount, currentAmount);
            });
        }
    }

}
