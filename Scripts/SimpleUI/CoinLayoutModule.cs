using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinLayoutModule : IAPBaseLayoutModule
{
    public GameObject creativePrefab;

    public override void InitializeModule(IAPShopData shopData)
    {
        listOfOffers = new List<OfferUICard>();
        Dictionary<IAPShopCoinPackageID, CoinOfferUICard> prefabs = new Dictionary<IAPShopCoinPackageID, CoinOfferUICard>();
        foreach (GameObject item in elementsListPrefabs)
        {
            CoinOfferUICard offer = item.GetComponent<OfferUICard>() as CoinOfferUICard;
            if (offer == null)
            {
                continue;
            }
            prefabs.Add(offer.ID, offer);
        }

        foreach (KeyValuePair<IAPShopCoinPackageID, CoinOfferCard> pair in shopData.CoinCards)
        {
            if (pair.Value.IsCreative())
            {
                GameObject moduleElemenet = Instantiate(creativePrefab, gridLayout);
                OfferUICard cardUI = moduleElemenet.GetComponent<OfferUICard>();
                cardUI.model = shopData.CoinCards[pair.Key];
                cardUI.Initialize();
                cardUI.RefreshData();
                listOfOffers.Add(cardUI);
                continue;
            }

            if (prefabs.ContainsKey(pair.Key))
            {
                GameObject moduleElemenet = Instantiate(prefabs[pair.Key].gameObject, gridLayout);
                OfferUICard cardUI = moduleElemenet.GetComponent<OfferUICard>();
                cardUI.model = shopData.CoinCards[pair.Key];
                cardUI.Initialize();
                cardUI.RefreshData();
                listOfOffers.Add(cardUI);
            }
        }
    }

    public override bool IsModuleAccesibleToPlayer()
    {
        return true;
    }
}
