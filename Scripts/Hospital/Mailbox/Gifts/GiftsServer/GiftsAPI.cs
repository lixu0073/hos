using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hospital;
using Hospital.Connectors;
using MovementEffects;
using ScriptableEventSystem;


public class GiftSentEventArgs : BaseNotificationEventArgs
{
    public readonly string targetID;

    public GiftSentEventArgs(string targetID)
    {
        this.targetID = targetID;
    }
}

public class GiftsAPI : MonoBehaviour
{
    #region static
    private static GiftsAPI instance;

    public static GiftsAPI Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of GiftsAPI was found on scene!");
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
            Debug.LogWarning("Multiple instances of GiftsAPI entrypoint were found!");
        instance = this;
    }
    #endregion

    #region Params
#pragma warning disable 0649
    [SerializeField] GameEvent giftSentEvent;
#pragma warning restore 0649
    IEnumerator<float> getGiftsCoroutine = null;
    List<GiftModel> gifts = new List<GiftModel>();

    int IntervalInSeconds
    {
        get { return DefaultConfigurationProvider.GetConfigCData().GiftsRefreshIntervalInSeconds; }
    }

    public int MaxGifts { get { return maxGifts; } private set { } }

    int maxGifts { get { return DefaultConfigurationProvider.GetConfigCData().GiftsMax; } }

    public int GiftsFeatureMinLevel { get { return giftsFeatureMinLevel; } private set { } }

    int giftsFeatureMinLevel { get { return DefaultConfigurationProvider.GetConfigCData().GiftsFeatureMinLevel; } }

    public bool IsFeatureUnlocked()
    {
        return Game.Instance.gameState().GetHospitalLevel() >= GiftsFeatureMinLevel;
    }
    #endregion

    #region API
    public delegate void OnSuccessGiversGet(List<Giver> givers);
    public delegate void OnSuccessGiftsGet(List<GiftModel> gifts);

    public List<GiftModel> GetGifts()
    {
        return gifts;
    }

    public void GetGivers(List<GiftModel> gifts, OnSuccessGiversGet onSuccess, OnFailure onFailure)
    {
        List<Giver> givers = new List<Giver>();

        if (gifts.Count == 0)
        {
            onSuccess?.Invoke(givers);
            return;
        }
        List<CacheManager.IGetPublicSave> ids = new List<CacheManager.IGetPublicSave>();
        foreach (GiftModel giftModel in gifts)
        {
            if (!string.IsNullOrEmpty(giftModel.SaveID))
            {
                Giver giver = new Giver(giftModel.SaveID, giftModel);
                ids.Add(giver);
                givers.Add(giver);
            }
        }
        CacheManager.BatchPublicSavesWithResults(ids, (saves) =>
        {
            foreach (Giver giver in givers)
            {
                foreach (PublicSaveModel save in saves)
                {
                    if (save.SaveID == giver.GetSaveID())
                        giver.SetSave(save);
                }
            }
            onSuccess?.Invoke(givers);
        }, (ex) =>
        {
            onFailure?.Invoke(ex);
        });
    }

    public void StartGetGiftsInInterval(OnSuccessGiftsGet onSuccess, OnFailure onFailure)
    {
        if (VisitingController.Instance.IsVisiting)
            return;
        RunGetGiftsCoroutine(onSuccess, onFailure);
    }

    public void StopGetGiftsInterval()
    {
        KillGetGiftsCoroutine();
    }

    public void SendGift(string targetID, OnSuccess onSuccess, OnFailure onFailure)
    {
        SendGift(targetID, false, () => { onSuccess(); giftSentEvent.Invoke(this, new GiftSentEventArgs(targetID)); }, onFailure);
    }

    public void SendThankYouGift(string targetID, OnSuccess onSuccess, OnFailure onFailure)
    {
        SendGift(targetID, true, onSuccess, onFailure);
    }

    public async void DeleteGifts(List<GiftModel> giftsToDelete, OnSuccessGiftsGet onSuccess = null, OnFailure onFailure = null)
    {
        try
        {
            if (giftsToDelete.Count > 0)
            {
                await GiftConnector.DeleteAsync(giftsToDelete);
                var deletedIDs = new HashSet<string>(giftsToDelete.Select(gift => gift.ID));
                gifts.RemoveAll(gift => deletedIDs.Contains(gift.ID));
            }
            onSuccess?.Invoke(gifts);
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }
    #endregion

    private async void SendGift(string targetID, bool isThankYouGift, OnSuccess onSuccess, OnFailure onFailure)
    {
        try
        {
            var result = await GiftConnector.QueryAndGetRemainingAsync(targetID);
            if (result == null || result.Count() < maxGifts)
            {
                GiftModel giftModel = new GiftModel()
                {
                    TargetSaveID = targetID,
                    ID = ServerTime.getTime().ToString(),
                    SaveID = CognitoEntry.SaveID,
                    IsThankYouGift = isThankYouGift
                };
                await GiftConnector.SaveAsync(giftModel);
            }
            onSuccess?.Invoke();
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }

    public async void FetchGifts(OnSuccessGiftsGet onSuccess, OnFailure onFailure)
    {
        try
        {
            var giftsFromServer = await GiftConnector.QueryAndGetRemainingAsync(CognitoEntry.SaveID);
            if (giftsFromServer != null && giftsFromServer.Count() > gifts.Count)
            {
                gifts.Clear();
                gifts = giftsFromServer;
            }
            if (onSuccess != null)
                onSuccess.Invoke(gifts);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in getting gifts from server: " + e.Message);
            onFailure?.Invoke(e);
        }
    }

    IEnumerator<float> GetGiftsCoroutine(OnSuccessGiftsGet onSuccess, OnFailure onFailure)
    {
        while (true)
        {
            if (gifts.Count >= maxGifts)
            {
                onSuccess?.Invoke(gifts);
            }
            else
            {
                FetchGifts(onSuccess, onFailure);
            }

            yield return Timing.WaitForSeconds(IntervalInSeconds);
        }
    }

    private void RunGetGiftsCoroutine(OnSuccessGiftsGet onSuccess, OnFailure onFailure)
    {
        if (getGiftsCoroutine == null)
        {
            getGiftsCoroutine = Timing.RunCoroutine(GetGiftsCoroutine(onSuccess, onFailure));
        }
    }

    private void KillGetGiftsCoroutine()
    {
        if (getGiftsCoroutine != null)
        {
            Timing.KillCoroutine(getGiftsCoroutine);
            getGiftsCoroutine = null;
        }
    }
}
