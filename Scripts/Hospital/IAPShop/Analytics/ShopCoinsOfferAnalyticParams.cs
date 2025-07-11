using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCoinsOfferAnalyticParams : ShopOfferAnalyticParams
{
    private const string SHOP_OFFER_COIN_PACKAGE_MODIFICATOR = "shopOfferCoinPackageMultiplier";

    private IAPShopCoinPackageID ID;
    private float modificator;

    public ShopCoinsOfferAnalyticParams(int offerOrder, int sectionOrder, int diamondsCost, IAPShopSection section, IAPShopCoinPackageID ID, float modificator) : base(offerOrder, sectionOrder, diamondsCost, section)
    {
        this.ID = ID;
        this.modificator = modificator;
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        parameters.Add(SHOP_OFFER_COIN_PACKAGE_MODIFICATOR, modificator);
        parameters.Add(SHOP_OFFER_ID, ID.ToString());
    }

}
