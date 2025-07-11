using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IAPShopController : MonoBehaviour
{
    private bool IsInitialized = false;
    IAPShopData IAPShopData;
    private const int INVISIBE = -1;
    private int FeatureOffersAccesibleToPlayer = 0;
    private int SpecialOffersAccesibleToPlayer = 0;

    public void Initialize()
    {
        if (IsInitialized)
            return;
        IAPShopData = new IAPShopData();
        SetupSectionsOrderAndVisibility();
        SetupSpecialOffers();
        SetupCoinPackages();
        GameState.OnLevelUp += GameState_OnLevelUp;
        if (Hospital.HospitalAreasMapController.Map.playgroud.ExternalHouseState != Hospital.ExternalRoom.EExternalHouseState.enabled)
        {
            Hospital.Playground.OnKidsUnWrap += Playground_OnKidsUnWrap;
        }
        UIController.get.IAPShopUI.Initialize(IAPShopData, CheckBundleAccessability);
        IsInitialized = true;
        ReferenceHolder.Get().lootBoxManager.OnLootBoxUpdated -= LootBoxManager_OnLootBoxUpdated;
        ReferenceHolder.Get().lootBoxManager.OnLootBoxUpdated += LootBoxManager_OnLootBoxUpdated;
    }

    private void LootBoxManager_OnLootBoxUpdated(Hospital.LootBox.LootBoxData lootBoxData)
    {
        SetupSpecialOffers();
        UIController.get.IAPShopUI.Reinitialize(IAPShopData);

    }

    private void Playground_OnKidsUnWrap(object sender, EventArgs e)
    {
        CheckBundleAccessability();
        UIController.get.IAPShopUI.RefreshLayouts();
        Hospital.Playground.OnKidsUnWrap -= Playground_OnKidsUnWrap;
    }

    private void GameState_OnLevelUp()
    {
        CheckBundleAccessability();
        UIController.get.IAPShopUI.RefreshLayouts();
    }

    public void CheckBundleAccessability()
    {
        FeatureOffersAccesibleToPlayer = 0;
        SpecialOffersAccesibleToPlayer = 0;
        foreach (KeyValuePair<IAPShopBundleID, BundleOfferCard> item in IAPShopData.FeatureCards)
        {
            if (item.Value.IsAccesibleToPlayer())
            {
                FeatureOffersAccesibleToPlayer++;
            }
        }
        foreach (KeyValuePair<IAPShopBundleID, BundleOfferCard> item in IAPShopData.SpecialOfferCards)
        {
            if (item.Value.IsAccesibleToPlayer())
            {
                SpecialOffersAccesibleToPlayer++;
            }
        }
    }

    #region Sections

    private void SetupSectionsOrderAndVisibility()
    {
        IAPShopSection[] Orderedsection = new IAPShopSection[CountSections()];
        if (Orderedsection.Length > 0)
        {
            int index = 0;
            foreach (KeyValuePair<IAPShopSection, int> item in IAPShopConfig.sections.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value))
            {
                if (item.Value != INVISIBE)
                {
                    Orderedsection[index] = item.Key;
                    index++;
                }
            }
        }
        else
        {
            Orderedsection = new IAPShopSection[3];
            Orderedsection[0] = IAPShopSection.sectionDiamonds;
            Orderedsection[1] = IAPShopSection.sectionCoins;
            Orderedsection[2] = IAPShopSection.sectionSpecialOffers;
        }
        IAPShopData.SetIAPShopModuleOrder(Orderedsection);
    }

    private int CountSections()
    {
        int counter = 0;
        foreach (KeyValuePair<IAPShopSection, int> item in IAPShopConfig.sections)
        {
            if (item.Value != INVISIBE)
            {
                counter++;
            }
        }
        return counter;
    }

    #endregion

    public IAPShopSection[] GetIAPShopModuleOrder()
    {
        return IAPShopData.Orderedsection;
    }



    private void SetupSpecialOffers()
    {
        IAPShopData.SetBundleOfferData(IAPShopConfig.bundles);
    }

    private void SetupCoinPackages()
    {
        IAPShopData.SetCoinPackages(IAPShopConfig.coinPackages);
    }

    public bool IsFeaturedOffersExist()
    {
        if (IAPShopData.FeatureCards == null || IAPShopData.FeatureCards.Count == 0)
        {
            return false;
        }
        else if (FeatureOffersAccesibleToPlayer <= 0)
        {
            return false;
        }
        return true;
    }

    public bool IsSpecialOffersExist()
    {
        if (IAPShopData.SpecialOfferCards == null || IAPShopData.SpecialOfferCards.Count == 0)
        {
            return false;
        }
        else if (SpecialOffersAccesibleToPlayer <= 0)
        {
            return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        GameState.OnLevelUp -= GameState_OnLevelUp;
        Hospital.Playground.OnKidsUnWrap -= Playground_OnKidsUnWrap;
        ReferenceHolder.Get().lootBoxManager.OnLootBoxUpdated -= LootBoxManager_OnLootBoxUpdated;
    }
}
