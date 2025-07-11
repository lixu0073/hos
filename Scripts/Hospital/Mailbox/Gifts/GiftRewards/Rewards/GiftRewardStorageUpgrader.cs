using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardStorageUpgrader : GiftReward
{
    MedicineRef storageUpgraderItem;

    public GiftRewardStorageUpgrader(int amount, int SpecialItemIndex) : base(amount)
    {
        this.amount = amount;
        storageUpgraderItem = new MedicineRef(MedicineType.Special, SpecialItemIndex);
        rewardType = GiftRewardType.StorageUpgrader;
    }

    public override void Collect()
    {
        Game.Instance.gameState().AddResource(storageUpgraderItem, amount, true, EconomySource);
    }

    public MedicineRef GetRewardMedicineRef()
    {
        return storageUpgraderItem;
    }

    public override string GetName()
    {
        return ResourcesHolder.Get().GetNameForCure(storageUpgraderItem);
    }

    public override Sprite GetSprite()
    {
        return ResourcesHolder.Get().GetSpriteForCure(storageUpgraderItem);
    }

    protected override void CollectAnimation()
    {
        bool isTankElixir = storageUpgraderItem.IsMedicineForTankElixir();
        UIController.get.storageCounter.AddManyLater(amount, isTankElixir, true);

        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, GetStartPoint(), amount, 0, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1.5f, 1.5f, 1), GetSprite(), () =>
        {
        }, () =>
        {
            UIController.get.storageCounter.Remove(amount, isTankElixir, true);
        });
    }
}
