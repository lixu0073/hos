using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleUI;
using UnityEngine;

class MedicineResourceGiftableResource : BaseGiftableResource
{
    private MedicineRef medicine;

    public MedicineResourceGiftableResource(MedicineRef medicine, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        this.medicine = medicine;
        localizationKey = ResourcesHolder.Get().GetMedicineInfos(this.medicine).Name;
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.medicine;
    }

    public override void Collect(bool updateCounter)
    {
        Game.Instance.gameState().AddResource(medicine, amount, true, economySource);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        bool isTank = medicine.IsMedicineForTankElixir();

        UIController.get.storageCounter.AddLater(amount, isTank);

        if (onEnd == null)
        {
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, startPoint, amount, delay, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(medicine), onStart, () =>
            {
                UIController.get.storageCounter.Remove(amount, isTank);
            });
        }
        else
        {
            OnEvent newCombinedAction = () => UIController.get.storageCounter.Remove(amount, isTank);
            newCombinedAction += onEnd;
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, startPoint, amount, delay, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(medicine), onStart, newCombinedAction);
        }

    }

    public MedicineRef GetMedicine()
    {
        return medicine;
    }

    public override Sprite GetIconForGift()
    {
        return ResourcesHolder.Get().GetMedicineInfos(medicine).image;
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        if (!(resource is MedicineResourceGiftableResource))
        {
            return false;
        }
        return medicine.type == ((MedicineResourceGiftableResource)resource).medicine.type && medicine.id == ((MedicineResourceGiftableResource)resource).medicine.id;
    }
}
