using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hospital;
using Hospital.Connectors;

public class PharmacyGlobalOffer : MonoBehaviour {

    public Button button;
    public Image discountBadge;
    public Image buyerAvatar;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI hospitalNameText;
    public TextMeshProUGUI buyerLevelText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI soldText;
    public TextMeshProUGUI amountText;
    
    [SerializeField]
    private Image[] helpBadgesBackgrounds = null;

    public Image[] toGrayscale;
    public Image visitedIndicator;


    public void SetSoldState(bool isSold)
    {
        SetGrayscale(isSold);
        soldText.gameObject.SetActive(isSold);
        priceText.transform.parent.gameObject.SetActive(!isSold);
    }

    public void SetVisited(bool isVisited)
    {
        visitedIndicator.gameObject.SetActive(isVisited);
    }

    public void SetGrayscale(bool isGrayscale)
    {
        if (gameObject == null)
            return;
        int length = toGrayscale.Length;
        Material grayscaleMaterial = ResourcesHolder.Get().GrayscaleMaterial;
        if (isGrayscale)
        {
            for (int i = 0; i < length; i++)
                toGrayscale[i].material = grayscaleMaterial;
        }
        else
        {
            for (int i = 0; i < length; i++)
                toGrayscale[i].material = null;
        }
    }

    public async void CheckingAvailability(PharmacyOrderAdvertised order)
    {
        if (!DefaultConfigurationProvider.GetConfigCData().GlobalOffersCheckingOfferState)
            return;

        try
        {
            var result = await PharmacyOrderConnector.LoadAdvertisedAsync(order);
            if(result == null || result.bought || result.bougthBuyWise)
            {
                SetSoldState(true);
                if (order != null)
                {
                    if (result == null)
                    {
                        order.bought = true;
                        order.bougthBuyWise = false;
                    }
                    else
                    {
                        order.bought = result.bought;
                        order.bougthBuyWise = result.bougthBuyWise;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void SetHelpBadges(bool isPlantationHelpRequested, bool isEpidemyHelpRequested, bool isTreatmentHelpRequested)
    {
        if (helpBadgesBackgrounds.Length < 3)
        {
            Debug.LogError("not enough help badges");
            return;
        }

        for (int i = 0; i < helpBadgesBackgrounds.Length; ++i)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[i].gameObject, false);
        }

        int freeSlotID = 0;

        if (isPlantationHelpRequested)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[freeSlotID].gameObject, true);
            UIController.SetImageSpriteSecure(helpBadgesBackgrounds[freeSlotID], ResourcesHolder.Get().plantationBadgeBackground);
            ++freeSlotID;
        }

        if (isEpidemyHelpRequested)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[freeSlotID].gameObject, true);
            UIController.SetImageSpriteSecure(helpBadgesBackgrounds[freeSlotID], ResourcesHolder.Get().epidemyBadgeBackground);
            ++freeSlotID;
        }

        if (isTreatmentHelpRequested && TreatmentRoomHelpController.HasTreatmentRoomHelpFeatureMinLevel)
        {
            UIController.SetGameObjectActiveSecure(helpBadgesBackgrounds[freeSlotID].gameObject, true);
            UIController.SetImageSpriteSecure(helpBadgesBackgrounds[freeSlotID], ResourcesHolder.Get().treatmentBadgeBackground);
            ++freeSlotID;
        }
    }
}
