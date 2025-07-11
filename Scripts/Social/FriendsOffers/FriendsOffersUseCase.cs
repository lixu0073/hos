using Hospital;
using Hospital.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FriendsOffersUseCase : MonoBehaviour
{
    private List<PharmacyOrderAdvertised> resultOffers = new List<PharmacyOrderAdvertised>();

    private LinkedList<string> queryAdvertisedIDs = new LinkedList<string>();
    private LinkedList<string> queuedAdvertisedIDs = new LinkedList<string>();
    private List<PharmacyOrderAdvertised> otherAdvertisedOffers = new List<PharmacyOrderAdvertised>();
    private int advertisedRequestCounter = 0;

    private LinkedList<string> queryStandardIDs = new LinkedList<string>();
    private LinkedList<string> queuedStandardIDs = new LinkedList<string>();
    private List<PharmacyOrderAdvertised> otherStandardOffers = new List<PharmacyOrderAdvertised>();
    private int standardRequestCounter = 0;

    private List<PublicSaveModel> publicSaves = new List<PublicSaveModel>();

    #region static
    private static FriendsOffersUseCase instance;

    public static FriendsOffersUseCase Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of FriendsOffersUseCase was found on scene!");
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
            Debug.LogWarning("Multiple instances of FriendsOffersUseCase entrypoint were found!");
        instance = this;
    }

    #endregion

    #region Delegates

    public delegate void OnSuccessOffersGet(List<PharmacyOrderAdvertised> offers);
    public delegate void OnSuccessOfferGet(PharmacyOrderAdvertised offer);

    #endregion

    public void GetFriendsOffers(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        List<string> saves = new List<string>();
        saves = FriendsDataZipper.GetFbAndIGFWithoutWise()
            .Select(f => f.SaveID)
            .ToList();
        publicSaves.Clear();
        if (saves.Count == 0)
            GetOffersBySaveIDs(saves, maxOffers, maxLevel, onSuccess, onFailure);
        else
            GetPublicSaves(saves, maxOffers, maxLevel, onSuccess, onFailure);
    }

    public class IDContainer : CacheManager.IGetPublicSave
    {
        string id;

        public IDContainer(string id)
        {
            this.id = id;
        }

        public string GetSaveID()
        {
            return id;
        }
    }

    private void GetPublicSaves(List<string> saves, int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        List<string> filteredSaves = new List<string>();
        List<CacheManager.IGetPublicSave> ids = new List<CacheManager.IGetPublicSave>();
        foreach (string saveID in saves)
        {
            ids.Add(new IDContainer(saveID));
        }
        CacheManager.BatchPublicSavesWithResults(ids, (publicSaves) =>
        {
            foreach (PublicSaveModel publicSave in publicSaves)
            {
                if (publicSave.HasStandardOffer())
                {
                    this.publicSaves.Add(publicSave);
                }
                if (publicSave.HasAdvertisedOffer())
                {
                    filteredSaves.Add(publicSave.SaveID);
                }
            }
            GetOffersBySaveIDs(filteredSaves, maxOffers, maxLevel, onSuccess, onFailure);
        }, onFailure, true);
    }

    private void GetOffersBySaveIDs(List<string> saveIDs, int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        otherAdvertisedOffers.Clear();
        resultOffers.Clear();
        queryAdvertisedIDs.Clear();
        queuedAdvertisedIDs.Clear();
        if (saveIDs.Count == 0)
        {
            OnSuccessAdvertisedOffersDownloaded(maxOffers, maxLevel, onSuccess, onFailure);
            return;
        }
        saveIDs = saveIDs.OrderBy(a => Guid.NewGuid()).ToList();
        foreach (string saveID in saveIDs)
        {
            queuedAdvertisedIDs.AddLast(saveID);
        }
        GetFriendsAdvertisedOffersIteration(maxOffers, maxLevel, onSuccess, onFailure);
    }

    #region Advertised Offers

    private void OnSuccessAdvertisedOffersDownloaded(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        foreach (PharmacyOrderAdvertised offer in resultOffers)
            offer.isFriendOffer = true;
        if (resultOffers.Count < maxOffers && publicSaves.Count > 0)
        {
            GetStandardOffers(maxOffers, maxLevel, onSuccess, onFailure);
            return;
        }
        onSuccess?.Invoke(resultOffers);
    }

    private void TryToAddOtherAdvertisedOffers(int maxOffers)
    {
        if (otherAdvertisedOffers.Count > 0)
        {
            otherAdvertisedOffers = otherAdvertisedOffers.OrderBy(a => Guid.NewGuid()).ToList();
            foreach (PharmacyOrderAdvertised offer in otherAdvertisedOffers)
            {
                if (resultOffers.Count == maxOffers)
                    break;
                resultOffers.Add(offer);
            }
        }
    }

    private void GetFriendsAdvertisedOffersIteration(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        queryAdvertisedIDs.Clear();
        int numberOfOffersToGet = maxOffers - resultOffers.Count;
        if (numberOfOffersToGet < 1)
        {
            OnSuccessAdvertisedOffersDownloaded(maxOffers, maxLevel, onSuccess, onFailure);
            return;
        }
        if (queuedAdvertisedIDs.Count == 0)
        {
            TryToAddOtherAdvertisedOffers(maxOffers);
            OnSuccessAdvertisedOffersDownloaded(maxOffers, maxLevel, onSuccess, onFailure);
            return;
        }
        for (int i = 0; i < numberOfOffersToGet; ++i)
        {
            if (queuedAdvertisedIDs.Count == 0)
                break;
            string saveID = queuedAdvertisedIDs.First();
            queuedAdvertisedIDs.RemoveFirst();
            queryAdvertisedIDs.AddFirst(saveID);
        }

        advertisedRequestCounter = queryAdvertisedIDs.Count;

        foreach (string saveID in queryAdvertisedIDs)
        {
            GetSingleAdvertisedOffer(saveID, maxLevel, (offer) => {
                if (offer != null)
                    resultOffers.Add((PharmacyOrderAdvertised)offer);
                CheckingAdvertisedRequestsEnd(maxOffers, maxLevel, onSuccess, onFailure);
            }, (ex) => {
                CheckingAdvertisedRequestsEnd(maxOffers, maxLevel, onSuccess, onFailure);
            });
        }
    }

    private void CheckingAdvertisedRequestsEnd(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        --advertisedRequestCounter;
        if (advertisedRequestCounter < 1)
        {
            GetFriendsAdvertisedOffersIteration(maxOffers, maxLevel, onSuccess, onFailure);
        }
    }

    private async void GetSingleAdvertisedOffer(string saveID, int maxLevel, OnSuccessOfferGet onSuccess = null, OnFailure onFailure = null)
    {
        try
        {
            var result = await PharmacyOrderConnector.FromQueryAndGetNextSetAdvertisedAsync(saveID, maxLevel);
            if (result != null && result.Count() > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, result.Count());
                PharmacyOrderAdvertised offer = result[randomIndex];
                result.RemoveAt(randomIndex);
                foreach (PharmacyOrderAdvertised order in result)
                    otherAdvertisedOffers.Add(order);
                onSuccess?.Invoke(offer);
            }
            else
            {
                onSuccess?.Invoke(null);
            }
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }

    #endregion

    #region Standard Offers

    private void CheckingStandardRequestsEnd(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        --standardRequestCounter;
        if (standardRequestCounter < 1)
        {
            GetFriendsStandardOffersIteration(maxOffers, maxLevel, onSuccess, onFailure);
        }
    }

    private void GetStandardOffers(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        queryStandardIDs.Clear();
        queuedStandardIDs.Clear();
        otherStandardOffers.Clear();

        publicSaves = publicSaves.OrderBy(a => Guid.NewGuid()).ToList();
        foreach (PublicSaveModel publicSave in publicSaves)
        {
            queuedStandardIDs.AddLast(publicSave.SaveID);
        }
        GetFriendsStandardOffersIteration(maxOffers, maxLevel, onSuccess, onFailure);
    }

    private void GetFriendsStandardOffersIteration(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        queryStandardIDs.Clear();
        int numberOfOffersToGet = maxOffers - resultOffers.Count;
        if (numberOfOffersToGet < 1)
        {
            OnSuccessStandardOffersDownloaded(maxOffers, maxLevel, onSuccess, onFailure);
            return;
        }
        if (queuedStandardIDs.Count == 0)
        {
            TryToAddOtherStandardOffers(maxOffers);
            OnSuccessStandardOffersDownloaded(maxOffers, maxLevel, onSuccess, onFailure);
            return;
        }
        for (int i = 0; i < numberOfOffersToGet; ++i)
        {
            if (queuedStandardIDs.Count == 0)
                break;
            string saveID = queuedStandardIDs.First();
            queuedStandardIDs.RemoveFirst();
            queryStandardIDs.AddFirst(saveID);
        }

        standardRequestCounter = queryStandardIDs.Count;

        foreach (string saveID in queryStandardIDs)
        {
            GetSingleStandardOffer(saveID, maxLevel, (offer) => {
                if (offer != null)
                    resultOffers.Add((PharmacyOrderAdvertised)offer);
                CheckingStandardRequestsEnd(maxOffers, maxLevel, onSuccess, onFailure);
            }, (ex) => {
                CheckingStandardRequestsEnd(maxOffers, maxLevel, onSuccess, onFailure);
            });
        }
    }

    private void OnSuccessStandardOffersDownloaded(int maxOffers, int maxLevel, OnSuccessOffersGet onSuccess = null, OnFailure onFailure = null)
    {
        foreach (PharmacyOrderAdvertised offer in resultOffers)
            offer.isFriendOffer = true;
        onSuccess?.Invoke(resultOffers);
    }

    private void TryToAddOtherStandardOffers(int maxOffers)
    {
        if (otherStandardOffers.Count > 0)
        {
            otherStandardOffers = otherStandardOffers.OrderBy(a => Guid.NewGuid()).ToList();
            foreach (PharmacyOrderAdvertised offer in otherStandardOffers)
            {
                if (resultOffers.Count == maxOffers)
                    break;
                resultOffers.Add(offer);
            }
        }
    }

    private async void GetSingleStandardOffer(string saveID, int maxLevel, OnSuccessOfferGet onSuccess = null, OnFailure onFailure = null)
    {
        try
        {
            var result = await PharmacyOrderConnector.FromQueryAndGetNextSetStandardAsync(saveID, maxLevel);
            if (result != null && result.Count() > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, result.Count());
                PharmacyOrderStandard offer = result[randomIndex];
                result.RemoveAt(randomIndex);
                foreach (PharmacyOrderStandard order in result)
                    otherStandardOffers.Add(order.ToAdvertised());
                onSuccess?.Invoke(offer.ToAdvertised());
            }
            else
            {
                onSuccess?.Invoke(null);
            }
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }

    #endregion
}
