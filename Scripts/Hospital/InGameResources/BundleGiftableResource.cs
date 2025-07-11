using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleUI;
using UnityEngine;

public class BundleGiftableResource : BaseGiftableResource
{
    private const float delyBetweenParticles = 0.6f;
    private List<BaseGiftableResource> bundledGifts;
    private Hospital.BundledRewardTypes bundleType;
    private SingleBoxSpriteData.BundledResourceSprite boxSpriteType;


    public BundleGiftableResource(List<BaseGiftableResource> bundledGifts, int amount, BaseResourceSpriteData.SpriteType spriteType, EconomySource economySource, string packageNameLocKey, Hospital.BundledRewardTypes bundleType, SingleBoxSpriteData.BundledResourceSprite boxSpriteType) : base(amount, spriteType, economySource)
    {
        rewardType = BaseGiftableResourceFactory.BaseResroucesType.bundle;
        localizationKey = packageNameLocKey;


        this.bundledGifts = bundledGifts;
        this.bundleType = bundleType;
        this.boxSpriteType = boxSpriteType;
    }

    public override void Collect(bool updateCounter)
    {
        for (int i = 0; i < amount; i++)
        {
            foreach (BaseGiftableResource gift in bundledGifts)
            {
                gift.Collect(false);
            }
        }
        ((HospitalCasesManager)Hospital.AreaMapController.Map.casesManager).DailyRewardBundledGift = true;
        ((HospitalCasesManager)Hospital.AreaMapController.Map.casesManager).AddBundleGift(this, economySource);
        UIController.getHospital.unboxingPopUp.OpenDailyRewardPopup();
    }

    public override Sprite GetMainImageForGift()
    {
        return ResourcesHolder.GetHospital().bundledPackagesReferences.GetIconForBox(boxSpriteType);
    }

    public override void SpawnParticle(Vector2 startPoint, float delay = 0, OnEvent onStart = null, OnEvent onEnd = null)
    {
        //float startingdelay = delay;
        //foreach (BaseGiftableResource gift in bundledGifts)
        //{
        //    gift.SpawnParticle(startPoint, startingdelay, onStart, onEnd);
        //    startingdelay += delyBetweenParticles;
        //}
    }

    public Hospital.BundledRewardTypes GetGiftBoxType()
    {
        return bundleType;
    }

    public SingleBoxSpriteData.BundledResourceSprite GetBoxImageCoverType()
    {
        return boxSpriteType;
    }

    public List<BaseGiftableResource> GetBundledGifts()
    {
        return bundledGifts;
    }

    public override Sprite GetIconForGift()
    {
        return null;
    }

    public override bool IsSameAs(BaseGiftableResource resource)
    {
        return false;
    }
}
