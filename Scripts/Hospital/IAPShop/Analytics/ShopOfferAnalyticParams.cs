using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopOfferAnalyticParams : BaseAnalyticParams
{
    protected const string SHOP_OFFER_ID = "shopOfferID";
    private const string SHOP_OFFER_ORDER = "shopOfferOrder";
    private const string SHOP_SECTION_ORDER = "shopSectionOrder";
    private const string SHOP_SECTION_NAME = "shopSectionName";
    private const string SHOP_DIAMONDS_COST = "shopDiamondsCost";

    private int offerOrder;
    private int sectionOrder;
    private int diamondsCost;
    private IAPShopSection section;

    public ShopOfferAnalyticParams(int offerOrder, int sectionOrder, int diamondsCost, IAPShopSection section) : base()
    {
        this.offerOrder = offerOrder;
        this.sectionOrder = sectionOrder;
        this.diamondsCost = diamondsCost;
        this.section = section;
    }

    public override void Initialize()
    {
        parameters.Add(SHOP_OFFER_ORDER, offerOrder);
        parameters.Add(SHOP_SECTION_ORDER, sectionOrder);
        parameters.Add(SHOP_DIAMONDS_COST, diamondsCost);
        parameters.Add(SHOP_SECTION_NAME, section.ToString());
    }
}
