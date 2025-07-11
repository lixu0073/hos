using Hospital;
using Hospital.Connectors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MigrationOffersUseCase : MonoBehaviour
{

    #region static
    private static MigrationOffersUseCase instance;

    public static MigrationOffersUseCase Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of MigrationOffersUseCase was found on scene!");
            return instance;
        }

    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of MigrationOffersUseCase entrypoint were found!");
        }
        instance = this;
    }

    #endregion

    public delegate void OnSuccess();
    public delegate void OnFailure(Exception ex);

    public async void Execute(OnSuccess onSuccess = null, OnFailure onFailure = null)
    {
        if(CacheManager.IsOffersMigrationComplete())
        {
            onSuccess?.Invoke();
            return;
        }
        string SaveID = CognitoEntry.SaveID;

        var oldTask = UpdateOldOffers(SaveID);
        var actualTask = UpdateActualOffers(SaveID);

        try
        {
            await Task.WhenAll(oldTask, actualTask);
            bool successOld = oldTask.Status == TaskStatus.RanToCompletion;
            bool successActual = actualTask.Status == TaskStatus.RanToCompletion;
            if (successOld && successActual)
            {
                CacheManager.SetOffersMigrationComplete();
                onSuccess?.Invoke();
            }
        }
        catch (Exception e)
        {
            if (oldTask.Exception != null)
                Debug.LogError("Could not update old offers: " + oldTask.Exception.Message);
            if (actualTask.Exception != null)
                Debug.LogError("Could not update actual offers: " + actualTask.Exception.Message);
            onFailure?.Invoke(e);
        }
    }

    private List<PharmacyOrderAdvertised> PrepareActualOffers(List<PharmacyOrderAdvertised> actualOffers)
    {
        List<PharmacyOrderAdvertised> offersToUpdate = new List<PharmacyOrderAdvertised>();
        foreach(PharmacyOrderAdvertised offer in actualOffers)
        {
            if(string.IsNullOrEmpty(offer.UUID))
            {
                Guid guid = Guid.NewGuid();
                offer.UUID = guid.ToString();
            }
            offersToUpdate.Add(offer);
        }
        return offersToUpdate;
    }

    private List<PharmacyOrderAdvertised> MapOffers(List<OldPharmacyOrderAdvertised> oldOffers)
    {
        List<PharmacyOrderAdvertised> offers = new List<PharmacyOrderAdvertised>();
        foreach (OldPharmacyOrderAdvertised oldOffer in oldOffers)
        {
            PharmacyOrderAdvertised offer = new PharmacyOrderAdvertised();
            offer.UserID = oldOffer.UserID;
            offer.expirationDate = oldOffer.expirationDate;
            offer.ID = oldOffer.ID;
            offer.requiredLevel = oldOffer.requiredLevel;
            offer.medicine = oldOffer.medicine;
            offer.amount = oldOffer.amount;
            offer.pricePerUnit = oldOffer.pricePerUnit;
            offer.bought = oldOffer.bought;
            offer.bougthBuyWise = oldOffer.bougthBuyWise;
            offer.buyerSaveID = oldOffer.buyerSaveID;
            offer.sortOrder = oldOffer.sortOrder;
            offer.UUID = oldOffer.UUID;
            if(string.IsNullOrEmpty(offer.UUID))
            {
                Guid guid = Guid.NewGuid();
                offer.UUID = guid.ToString();
            }

            offers.Add(offer);
        }
        return offers;
    }

    private async Task UpdateActualOffers(string saveID)
    {
        var actualOffers = await PharmacyOrderConnector.QueryAndGetRemainingAdvertisedAsync(saveID);
        if(actualOffers.Count == 0)
            return;
        actualOffers = PrepareActualOffers(actualOffers);
        if (actualOffers.Count == 0)
            return;
        await PharmacyOrderConnector.PutAdvertisedAsync(actualOffers);
    }

    private async Task UpdateOldOffers(string saveID)
    {
        var oldOffers = await PharmacyOrderConnector.QueryAndGetRemainingOldAdvertisedAsync(saveID);
        if(oldOffers.Count == 0)
            return;
        var putNewTask = PharmacyOrderConnector.PutAdvertisedAsync(MapOffers(oldOffers));
        var deleteOldTask = PharmacyOrderConnector.DeleteOldAdvertisedAsync(oldOffers);
        await Task.WhenAll(putNewTask, deleteOldTask);
    }
}
