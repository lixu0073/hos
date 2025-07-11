using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardPositiveEnergy : GiftReward
{
    private MedicineRef med;

    public GiftRewardPositiveEnergy(int amount) : base(amount)
    {
        this.amount = amount;
        med = MedicineRef.Parse("16(00)");
        rewardType = GiftRewardType.PositiveEnergy;
    }

    public override void Collect()
    {
        Game.Instance.gameState().AddPositiveEnergy(amount, EconomySource);
    }

    public override string GetName()
    {
        return ResourcesHolder.Get().GetNameForCure(med);
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().GetSpriteForCure(med);
    }

    protected override void CollectAnimation()
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.PositiveEnergy, GetStartPoint(), amount, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), GetSprite(), null, null);
    }
}
