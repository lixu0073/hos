using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardMixture : GiftReward
{
    MedicineRef medicine;

    public GiftRewardMixture(int amount, MedicineRef medicine) : base(amount)
    {
        this.amount = amount;
        this.medicine = medicine;
        rewardType = GiftRewardType.Mixture;
    }

    public override void Collect()
    {
        Game.Instance.gameState().AddResource(medicine, amount, true, EconomySource);
    }

    public MedicineRef GetRewardMedicineRef()
    {
        return medicine;
    }

    public override string GetName()
    {
        return ResourcesHolder.Get().GetNameForCure(medicine);
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().GetSpriteForCure(medicine);
    }

    protected override void CollectAnimation()
    {
        bool isTankElixir = medicine.IsMedicineForTankElixir();
        UIController.get.storageCounter.AddManyLater(amount, isTankElixir, true);

        GiftType giftType = GiftType.Special;
        if (medicine.type != MedicineType.Special)
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
