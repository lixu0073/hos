using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class IAPBaseLayoutModule : MonoBehaviour
{
    public TextMeshProUGUI moduleName;
    public List<GameObject> elementsListPrefabs;
    public RectTransform gridLayout;
    public List<OfferUICard> listOfOffers;
    public abstract void InitializeModule(IAPShopData data);
    public abstract bool IsModuleAccesibleToPlayer();


    public virtual void RefreshData()
    {
        for (int i = 0; i < listOfOffers.Count; i++)
        {
            listOfOffers[i].RefreshData();
        }
    }

    public void RefreshLayouts()
    {
        for (int i = 0; i < listOfOffers.Count; i++)
        {
            listOfOffers[i].RefreshLayout();
        }
        ToggleModuleVisitlibity(IsModuleAccesibleToPlayer());
    }

    public void ToggleModuleVisitlibity(bool visible)
    {
        if (gameObject.activeSelf == !visible)
        {
            gameObject.SetActive(visible);
        }
        if (visible == false)
        {
            Vector2 currentSizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
            Vector2 newSizeDelta = new Vector2(currentSizeDelta.x, 0);
            gameObject.GetComponent<RectTransform>().sizeDelta = newSizeDelta;

        }

    }

    
}
