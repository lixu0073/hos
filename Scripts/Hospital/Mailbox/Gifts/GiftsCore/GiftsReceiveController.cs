using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftsReceiveController
{
    #region Static

    private static GiftsReceiveController instance = null;

    public static GiftsReceiveController Instance
    {
        get
        {
            if (instance == null)
                instance = new GiftsReceiveController();
            return instance;
        }
    }

    #endregion

    #region Events

    public delegate void OnGiversUpdate(List<Giver> gifts);
    public static event OnGiversUpdate onGiversUpdate;

    #endregion

    #region Params

    private WiseGiftController wiseGiftController = new WiseGiftController();

    #endregion

    #region PrivateFields
    private GiftRewardGenerator giftRewardGenerator;
    #endregion

    #region AIP

    public void UpdateLastBuyOrLastRefreshWithSomeGiftsTime()
    {
        wiseGiftController.UpdateLastBuyOrLastRefreshWithSomeGiftsTime();
    }

    public bool ShouldAddGiftFromWise()
    {
        return wiseGiftController.ShouldAddGiftFromWise();
    }

    public void Initialize(bool isVisiting)
    {
        GiftsAPI.Instance.StopGetGiftsInterval();
        if (!isVisiting)
        {
            GiftsAPI.Instance.StartGetGiftsInInterval((gifts) =>
            {
                if (gifts.Count > 0)
                    UpdateLastBuyOrLastRefreshWithSomeGiftsTime();
                if (onGiversUpdate != null)
                {
                    GiftsAPI.Instance.GetGivers(gifts, (givers) =>
                    {
                        onGiversUpdate.Invoke(givers);
                    }, (ex) =>
                    {
                        Debug.LogError(ex.Message);
                        onGiversUpdate.Invoke(new List<Giver>());
                    });
                }

            }, (ex) =>
            {
                HandleError(ex);
            });
        }
        else
        {
            onGiversUpdate?.Invoke(new List<Giver>());
        }
        GiftRewardGeneratorData giftGeneData = ReferenceHolder.GetHospital().giftRewardGeneratorParser.GetGiftRewardGeneratorData();
        giftRewardGenerator = new GiftRewardGenerator(giftGeneData);
    }

    public void CollectAll(List<Giver> giversList, bool addWise)
    {
        if (giversList.Count + (addWise ? 1 : 0) == 0)
        {
            return;
        }
        if (giftRewardGenerator == null)
        {
            GiftRewardGeneratorData giftGeneData = ReferenceHolder.GetHospital().giftRewardGeneratorParser.GetGiftRewardGeneratorData();
            giftRewardGenerator = new GiftRewardGenerator(giftGeneData);
        }

        int giftsAmount = (int)Mathf.Clamp(giversList.Count, 0, GiftsAPI.Instance.MaxGifts - (addWise ? 1 : 0));

        GiftReward wiseGift = null;

        if (addWise)
        {
            wiseGift = giftRewardGenerator.GenerateReward();
        }

        for (int i = 0; i < giftsAmount; ++i)
        {
            giversList[i].reward = giftRewardGenerator.GenerateReward();
            AnalyticsController.instance.ReportGiftRecieved(giversList[i].reward);
        }

        AddAllGifts(giversList, wiseGift);
        ShowGifts(giversList, wiseGift);


        GiftsAPI.Instance.DeleteGifts(GiftsAPI.Instance.GetGifts(), (gifts) =>
        {
            UpdateLastBuyOrLastRefreshWithSomeGiftsTime();
            if (onGiversUpdate != null)
            {
                GiftsAPI.Instance.GetGivers(gifts, (givers) =>
                {
                    onGiversUpdate.Invoke(givers);
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                    onGiversUpdate.Invoke(new List<Giver>());
                });
            }
        }, (ex) =>
        {
            HandleError(ex);
        });
    }

    public void FetchGifts()
    {
        GiftsAPI.Instance.FetchGifts((gifts) =>
        {
            if (onGiversUpdate != null)
            {
                GiftsAPI.Instance.GetGivers(gifts, (givers) =>
                {
                    onGiversUpdate.Invoke(givers);
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                    onGiversUpdate.Invoke(new List<Giver>());
                });
            }
        }, (ex) =>
        {
            HandleError(ex);
        });
    }

    private void AddAllGifts(List<Giver> giversList, GiftReward wiseGift)
    {
        if (wiseGift != null)
        {
            wiseGift.Collect();
        }

        int giftsAmount = (int)Mathf.Clamp(giversList.Count, 0, GiftsAPI.Instance.MaxGifts - (wiseGift != null ? 1 : 0));

        for (int i = 0; i < giftsAmount; ++i)
        {
            giversList[i].reward.Collect();
        }
    }

    private void ShowGifts(List<Giver> giversList, GiftReward wiseGift)
    {
        UIController.getHospital.unboxingPopUp.OpenGiftFromFriend(giversList, wiseGift);
    }

    #endregion

    #region Methods

    private void HandleError(Exception ex)
    {
        Debug.LogError(ex.Message);
    }

    #endregion

    public void TestRandomGift()
    {
        GiftReward gift = giftRewardGenerator.GenerateReward();
        Debug.LogError("Type " + gift.rewardType + ",gift amount " + gift.amount);
    }
}
