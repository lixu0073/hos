using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hospital;
using SimpleUI;
using UnityEngine;

class BoosterResource : BaseGiftableResource
{
    private int boosterID;

    public BoosterResource(int boosterID, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource) : base(amount, spriteType, economySource)
    {
        this.boosterID = boosterID;
        localizationKey = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo;
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.booster;
    }

    public override void Collect(bool updateCounter)
    {
        AreaMapController.Map.boosterManager.AddBooster(boosterID, economySource);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Booster, startPoint, amount, delay, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), GetIconForGift(), null, null);
    }

    public int GetBoosterID()
    {
        return boosterID;
    }

    public override Sprite GetIconForGift()
    {
        return ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon;
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        if (!(resource is BoosterResource))
        {
            return false;
        }
        return boosterID == ((BoosterResource)resource).GetBoosterID();
    }
}
