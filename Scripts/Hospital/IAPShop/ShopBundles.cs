using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBundles
{
    public Dictionary<IAPShopBundleID, IAPShopBundleData> bundles;
    public ShopBundles(Dictionary<IAPShopBundleID, IAPShopBundleData> specialOffer)
    {
        bundles = specialOffer;
    }

    public bool IsBundleSpecialOffer(IAPShopBundleID ID)
    {
        return bundles[ID].orderInSpecialOffersSection != -1;
    }

    public bool IsBundleFeatureOffer(IAPShopBundleID ID)
    {
        return bundles[ID].orderInFeaturesSection != -1;
    }
}
