using System.Collections.Generic;
using UnityEngine;

public class FeatureLayoutModule : IAPBaseLayoutModule
{
#pragma warning disable 0649
    [SerializeField]
    private GameObject CreativeOfferPrefab;
#pragma warning restore 0649

    public override void InitializeModule(IAPShopData shopData)
    {
        listOfOffers = new List<OfferUICard>();
        Dictionary<IAPShopBundleID, BundleOfferUICard> prefabs = new Dictionary<IAPShopBundleID, BundleOfferUICard>();
        foreach (GameObject item in elementsListPrefabs)
        {
            BundleOfferUICard offer = item.GetComponent<OfferUICard>() as BundleOfferUICard;
            if (offer == null)
            {
                continue;
            }
            prefabs.Add(offer.ID, offer);
        }

        foreach (KeyValuePair<IAPShopBundleID, BundleOfferCard> pair in shopData.FeatureCards)
        {
            if (pair.Value.IsCreative())
            {
                GameObject moduleElemenet = Instantiate(CreativeOfferPrefab, gridLayout);
                OfferUICard cardUI = moduleElemenet.GetComponent<OfferUICard>();
                cardUI.model = shopData.FeatureCards[pair.Key];
                cardUI.Initialize();
                cardUI.RefreshData();
                listOfOffers.Add(cardUI);
                continue;
            }

            if (prefabs.ContainsKey(pair.Key))
            {
                GameObject moduleElemenet = Instantiate(prefabs[pair.Key].gameObject, gridLayout);
                OfferUICard cardUI = moduleElemenet.GetComponent<OfferUICard>();
                cardUI.model = shopData.FeatureCards[pair.Key];
                cardUI.Initialize();
                cardUI.RefreshData();
                listOfOffers.Add(cardUI);
            }
        }
    }

    public override bool IsModuleAccesibleToPlayer()
    {
        return ReferenceHolder.Get().IAPShopController.IsFeaturedOffersExist();
    }

    public override void RefreshData()
    {
        base.RefreshData();

    }
}
