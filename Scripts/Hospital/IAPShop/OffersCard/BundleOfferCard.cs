using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleOfferCard : OfferCard
{
    public IAPShopBundleID ID;
    public IAPShopBundleColor Color;
    protected IAPShopSection section;
    protected int offerOrder;
    protected int sectionOrder;
    public List<BubbleBoyReward> multipleRewards = new List<BubbleBoyReward>();

    public BundleOfferCard(int costInDiamonds, IAPShopBundleID ID, IAPShopSection section, int offerOrder, int sectionOrder, IAPShopBundleColor Color = IAPShopBundleColor.none) : base(costInDiamonds)
    {
        this.ID = ID;
        this.Color = Color;
        this.section = section;
        this.offerOrder = offerOrder;
        this.sectionOrder = sectionOrder;
    }

    public override void Buy()
    {
        Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.IAPShopBundle);
        AnalyticsController.instance.ReportBuyShopOffer(GetAnalyticData());
    }

    protected override BaseAnalyticParams GetAnalyticData()
    {
        return new ShopBundleOfferAnalyticParams(offerOrder, sectionOrder, costInDiamonds, section, ID, Color);
    }

    public void FillMultipleRewards(BubbleBoyReward reward)
    {
        if (reward != null)
        {
            multipleRewards.Add(reward);
        }
    }

    public List<BubbleBoyReward> GetRewardsFromBundle()
    {
        return multipleRewards;
    }

    public virtual bool IsAccesibleToPlayer()
    {
        switch (ID)
        {
            case IAPShopBundleID.bundleBreastCancerDeal:
                return false;
            case IAPShopBundleID.bundlePositiveEnergy50:
                return Hospital.AreaMapController.Map.playgroud.CanAddPositiveEnergy();
            case IAPShopBundleID.bundleShovels9:
                return Game.Instance.gameState().GetHospitalLevel() >= 15;
            case IAPShopBundleID.bundleSpecialPack:
                if (multipleRewards == null || multipleRewards.Count <= 0)
                {
                    return false;
                }
                for (int i = 0; i < multipleRewards.Count; i++)
                {
                    if (!multipleRewards[i].IsAccesibleByPlayer())
                    {
                        return false;
                    }
                }
                return true;
            case IAPShopBundleID.bundleSuperBundle4:
                if (multipleRewards == null || multipleRewards.Count <= 0)
                {
                    return false;
                }
                for (int i = 0; i < multipleRewards.Count; i++)
                {
                    if (!multipleRewards[i].IsAccesibleByPlayer())
                    {
                        return false;
                    }
                }
                return true;
            case IAPShopBundleID.bundleTapjoy:
                return false;
            default:
                return false;
        }
    }
}
