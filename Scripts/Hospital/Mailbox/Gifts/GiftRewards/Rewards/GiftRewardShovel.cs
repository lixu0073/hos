using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardShovel : GiftReward
{
    MedicineRef shovel;

    public GiftRewardShovel(int amount) : base(amount)
    {
        this.amount = amount;
        shovel = new MedicineRef(MedicineType.Special, 3);
        rewardType = GiftRewardType.Shovel;
    }

    public override void Collect()
    {
        Game.Instance.gameState().AddResource(shovel, amount, true, EconomySource);
    }

    public override string GetName()
    {
        return ResourcesHolder.Get().GetNameForCure(shovel);
    }

    public MedicineRef GetRewardMedicineRef()
    {
        return shovel;
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().GetSpriteForCure(shovel);
    }

    protected override void CollectAnimation()
    {
        bool isTankElixir = shovel.IsMedicineForTankElixir();
        UIController.get.storageCounter.AddManyLater(amount, isTankElixir, true);

        GiftType giftType = GiftType.Special;
        if (shovel.type != MedicineType.Special)
        {
            giftType = GiftType.Medicine;
        }

        ReferenceHolder.Get().giftSystem.CreateGiftParticle(giftType, GetStartPoint(), amount, 0, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1.5f, 1.5f, 1), GetSprite(), () =>
        {
        }, () =>
        {
            UIController.get.storageCounter.Remove(amount, isTankElixir, true);
        });
    }
}
