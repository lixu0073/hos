using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBundleOfferAnalyticParams : ShopOfferAnalyticParams
{
    private const string SHOP_OFFER_COLOR = "shopOfferColor";

    private IAPShopBundleID ID;
    private IAPShopBundleColor Color;

    public ShopBundleOfferAnalyticParams(int offerOrder, int sectionOrder, int diamondsCost, IAPShopSection section, IAPShopBundleID ID, IAPShopBundleColor color) : base(offerOrder, sectionOrder, diamondsCost, section)
    {
        this.ID = ID;
        Color = color;
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        parameters.Add(SHOP_OFFER_COLOR, Color.ToString());
        parameters.Add(SHOP_OFFER_ID, ID.ToString());
    }

}
