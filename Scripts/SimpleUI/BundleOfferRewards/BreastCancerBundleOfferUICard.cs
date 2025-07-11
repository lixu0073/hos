using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreastCancerBundleOfferUICard : BundleOfferUICard
{
    public override bool HasRequiredLevel()
    {
        if (model is BundleOfferCard)
        {
            BundleOfferCard bundleModel = model as BundleOfferCard;
            return bundleModel.IsAccesibleToPlayer();
        }
        return false;
    }

    public override void OnClick()
    {
        UIController.get.IAPShopUI.BreastCancerFoundation();
    }

    protected override void Collect() { }
    public override void Initialize()
    {
        base.Initialize();
    }
}
