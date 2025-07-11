using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardDiamond : GiftReward
{
    public GiftRewardDiamond(int amount) : base(amount)
    {
        this.amount = amount;
        rewardType = GiftRewardType.Diamond;
    }

    public override void Collect()
    {
        Game.Instance.gameState().AddDiamonds(amount, EconomySource, true);
    }

    public override string GetName()
    {
        return I2.Loc.ScriptLocalization.Get("DIAMONDS");
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().boxOpeningRewardAssets.diamondsChest;
    }

    protected override void CollectAnimation()
    {
        int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount() - amount;
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, GetStartPoint(), amount, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), GetSprite(), null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, amount, currentDiamondAmount);
        });
    }

}
