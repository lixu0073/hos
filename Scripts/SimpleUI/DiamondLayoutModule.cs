using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondLayoutModule : IAPBaseLayoutModule
{
    public override void InitializeModule(IAPShopData shopData)
    {
        listOfOffers = new List<OfferUICard>();
        foreach (GameObject item in elementsListPrefabs)
        {
            GameObject moduleElemenet = Instantiate(item, gridLayout);
            OfferUICard cardUI = moduleElemenet.GetComponent<OfferUICard>();
            if (cardUI == null)
            {
                continue;
            }
            cardUI.Initialize();
            cardUI.RefreshData();
            listOfOffers.Add(cardUI);
        }
    }

    public override bool IsModuleAccesibleToPlayer()
    {
        return true;
    }
}
