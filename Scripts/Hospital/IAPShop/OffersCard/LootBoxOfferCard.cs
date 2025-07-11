using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxOfferCard : CreativeOfferCard
{
    public LootBoxOfferCard(int costInDiamonds, IAPShopBundleID ID, IAPShopSection section, int offerOrder, int sectionOrder, string iapProductId = null, string decisionPoint = null, IAPShopBundleColor Color = IAPShopBundleColor.none) : base(costInDiamonds, ID, section, offerOrder, sectionOrder, iapProductId, decisionPoint, Color)
    {

    }

    public override void Buy()
    {
        ReferenceHolder.Get().lootBoxManager.StartPurchase();
    }

    public override bool IsCreative()
    {
        return true;
    }

    protected override BaseAnalyticParams GetAnalyticData()
    {
        return null;
    }

    public override bool IsAccesibleToPlayer()
    {
        return true;
    }
}
