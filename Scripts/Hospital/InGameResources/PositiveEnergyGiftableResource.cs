using SimpleUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PositiveEnergyGiftableResource : BaseGiftableResource
{
    public PositiveEnergyGiftableResource(int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        localizationKey = "POSITIVE_ENERGY";
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.positiveEnergy;
    }

    public override void Collect(bool updateCounter)
    {
        Game.Instance.gameState().AddResource(ResourceType.PositiveEnergy, amount, economySource, updateCounter);
    }

    public override Sprite GetIconForGift()
    {
        return ResourcesHolder.Get().PositiveEnergyIcon;
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.PositiveEnergy, startPoint, amount, delay, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[4], onStart, onEnd);
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        return resource is PositiveEnergyGiftableResource;
    }
}
