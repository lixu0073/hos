using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeOfferCard : BundleOfferCard
{
    public string IapProductId;
    public string DecisionPoint;

    public CreativeOfferCard(int costInDiamonds, IAPShopBundleID ID, IAPShopSection section, int offerOrder, int sectionOrder, string iapProductId = null, string decisionPoint = null, IAPShopBundleColor Color = IAPShopBundleColor.none) : base(costInDiamonds, ID, section, offerOrder, sectionOrder, Color)
    {
        IapProductId = iapProductId;
        DecisionPoint = decisionPoint;
    }

    public override void Buy()
    {
        IAPController.instance.BuyProductID(IapProductId);
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
