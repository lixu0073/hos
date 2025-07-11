using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedOffersController : MonoBehaviour
{
    private static TimedOffersController instance;

    public UnityAction onTimedOffersUpdated = null;

    public List<TimedOfferNew> timedOffers = new List<TimedOfferNew>();

    private const string LAST_SHOW_ON_LOAD_TIME_PREF_NAME = "TimedOfferOnLoadTime";
    private const string LAST_SHOW_ON_LOAD_PRIORITY_PREF_NAME = "TimedOfferOnLoadPriority";
    private const string LAST_SHOW_ON_LOAD_CAMPAIGN_ID_PREF_NAME = "TimedOfferOnLoadCampaignID";

    private int lastShowOnLoadTime = 0;
    private int lastShowOnLoadPriority = -1;
    private string lastShowOnLoadCampaignID = "";

    public static TimedOffersController Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("There is no TimedOffersController instance on scene!");

            return instance;
        }
    }

    void Awake()
    {
        if (instance != null)
            Debug.LogWarning("There are possibly multiple instances of TimeOffersController on scene!");

        instance = this;
    }

    private void OnDisable()
    {
        for (int i = 0; i < timedOffers.Count; ++i)
        {
            if (timedOffers[i].endOfferCoroutine != null)
            {
                try
                { 
                    StopCoroutine(timedOffers[i].endOfferCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                timedOffers[i].endOfferCoroutine = null;
            }
        }
    }

    public void FetchTimedOffers()
    {
        Debug.LogError("Were in DDNA FetchTimedOffers");
    //    InvokeOnTimedOffersCountUpdated();
    //    if (TimedOffersDeltaConfig.timedOffersDecisionPoints == null)
    //    {
    //        DeltaDNAController.instance.OnTimedOffersConfigReceived -= FetchTimedOffers;
    //        DeltaDNAController.instance.OnTimedOffersConfigReceived += FetchTimedOffers;
    //        return;
    //    }

    //    int requestsCount = TimedOffersDeltaConfig.timedOffersDecisionPoints.Count;
    //    for (int i = 0; i < TimedOffersDeltaConfig.timedOffersDecisionPoints.Count; ++i)
    //    {
    //        string key = TimedOffersDeltaConfig.timedOffersDecisionPoints[i].offerDecisionPoint;
    //        int endDate = TimedOffersDeltaConfig.timedOffersDecisionPoints[i].offerEndDate;
    //        int orderNo = TimedOffersDeltaConfig.timedOffersDecisionPoints[i].offerOrderNo;
    //        int offerPriority = TimedOffersDeltaConfig.timedOffersDecisionPoints[i].offerPriority;

    //        if (endDate <= ServerTime.UnixTime(DateTime.UtcNow))
    //        {
    //            --requestsCount;
    //            HandleEndFetching(requestsCount);
    //            continue;
    //        }

    //        TimedOfferNew existingTimedOffer = timedOffers.Find((x) => x.decisionPoint == key);
    //        if (existingTimedOffer != null && existingTimedOffer.dpc != null)
    //        {
    //            --requestsCount;
    //            HandleEndFetching(requestsCount);
    //            continue;
    //        }

    //        if (existingTimedOffer != null && existingTimedOffer.dpc == null)
    //            timedOffers.Remove(existingTimedOffer);

    //        DecisionPointCalss decisionPointCalss = null;
    //        decisionPointCalss = DecisionPointCalss.RequestImageMessage(key,
    //        (imageMessage) =>
    //        {
    //            if (imageMessage == null)
    //            {
    //                --requestsCount;
    //                HandleEndFetching(requestsCount);

    //                return;
    //            }

    //            if (endDate <= ServerTime.UnixTime(DateTime.UtcNow))
    //            {
    //                TimedOffersDeltaConfig.timedOffersDecisionPoints.RemoveAt(TimedOffersDeltaConfig.timedOffersDecisionPoints.FindIndex((x) => x.offerDecisionPoint == key));

    //                --requestsCount;
    //                HandleEndFetching(requestsCount);

    //                return;
    //            }

    //            TimedOfferNew timedOffer = new TimedOfferNew()
    //            {
    //                dpc = decisionPointCalss,

    //                decisionPoint = key,
    //                timedOfferEndDate = endDate,
    //                offerOrderNo = orderNo,
    //                timedOfferPriority = offerPriority,
    //                iapProducts = GetIapProductsForOffer(imageMessage),
    //                isTickerOnPopup = IsTickerOnPopup(imageMessage)
    //            };

    //            float timeLeft = endDate - ServerTime.UnixTime(DateTime.UtcNow);
    //            timedOffer.endOfferCoroutine = StartCoroutine(EndOfferCoroutine(timeLeft, timedOffer));
    //            timedOffers.Add(timedOffer);

    //            --requestsCount;
    //            HandleEndFetching(requestsCount);
    //        },
    //        _handleException: (exception) =>
    //        {
    //            --requestsCount;
    //            HandleEndFetching(requestsCount);
    //        },
    //        _onAction: (imageMessage) =>
    //        {
    //            if (UIController.getHospital == null)
    //                return;

    //            UIController.getHospital.TimerOffersScreen.GetComponent<TimedOffersScreenInitializer>().ClosePopup();
    //        },
    //        _onDismiss: (imageMessage) =>
    //        {
    //            if (UIController.getHospital == null)
    //                return;

    //            UIController.getHospital.TimerOffersScreen.GetComponent<TimedOffersScreenInitializer>().ClosePopup();
    //        },
    //        _destroyAfterShow: true);

    //        existingTimedOffer = timedOffers.Find((x) => x.decisionPoint == key);
    //        if (existingTimedOffer != null && existingTimedOffer.dpc == null)
    //        {
    //            existingTimedOffer.dpc = decisionPointCalss;
    //            HandleEndFetching(requestsCount);
    //        }
    //    }
    }

    //private List<string> GetIapProductsForOffer(DeltaDNA.ImageMessage imageMessage)
    //{
    //    List<string> iapProducts = new List<string>();

    //    if (imageMessage.Parameters.ContainsKey("iapProduct"))
    //        iapProducts.Add(imageMessage.Parameters["iapProduct"].ToString());

    //    if (imageMessage.Parameters.ContainsKey("iapProduct2"))
    //        iapProducts.Add(imageMessage.Parameters["iapProduct2"].ToString());

    //    return iapProducts;
    //}

    //private bool IsTickerOnPopup(DeltaDNA.ImageMessage imageMessage)
    //{
    //    if (imageMessage.Parameters.ContainsKey("isTickerOnPopup"))
    //        return (bool)imageMessage.Parameters["isTickerOnPopup"];
    //    return false;
    //}

    //private void HandleEndFetching(int requestsLeft)
    //{
    //    if(requestsLeft > 0)
    //        return;

    //    InvokeOnTimedOffersCountUpdated();
    //}

    //public void InvokeOnTimedOffersCountUpdated()
    //{
    //    onTimedOffersUpdated?.Invoke();
    //}

    public void OnIAPSuccessBought(string productId)
    {
        //TODO
        //List<TimedOfferNew> offersToRemove = new List<TimedOfferNew>();

        //for (int i = 0; i < timedOffers.Count; ++i)
        //{
        //    for (int j = 0; j < timedOffers[i].iapProducts.Count; ++j)
        //    {
        //        if (timedOffers[i].iapProducts[j] == productId)
        //        {
        //            offersToRemove.Add(timedOffers[i]);
        //            break;
        //        }
        //    }
        //}

        //GameObject g = null;
        //for (int i = 0; i < offersToRemove.Count; ++i)
        //{
        //    if (offersToRemove[i].dpc.imageMessage != null)
        //    {
        //        g = offersToRemove[i].dpc.imageMessage.GetGameobject();
        //        if (g != null)
        //        {
        //            Destroy(g);
        //            Resources.UnloadAsset(g);
        //            Resources.UnloadUnusedAssets();
        //            if (DecisionPointCalss.imageMessageList.ContainsKey(offersToRemove[i].decisionPoint))
        //                DecisionPointCalss.imageMessageList.Remove(offersToRemove[i].decisionPoint);
        //            Debug.Log("Succesfuly deleted Imagemessage");
        //        }
        //    }
        //    try
        //    {
        //        if (offersToRemove[i].endOfferCoroutine != null)
        //            StopCoroutine(offersToRemove[i].endOfferCoroutine);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        //    }
        //    offersToRemove[i].endOfferCoroutine = null;
        //    timedOffers.Remove(offersToRemove[i]);
        //}
        //offersToRemove.Clear();
        //FetchTimedOffers();
    }

    //IEnumerator EndOfferCoroutine(float timeLeft, TimedOfferNew timedOffer)
    //{
    //    yield return new WaitForSeconds(timeLeft);
    //    if (timedOffer.dpc.imageMessage != null)
    //    {
    //        GameObject g = timedOffer.dpc.imageMessage.GetGameobject();
    //        if (g != null)
    //        {
    //            Destroy(g);
    //            Resources.UnloadAsset(g);
    //            Resources.UnloadUnusedAssets();
    //            if (DecisionPointCalss.imageMessageList.ContainsKey(timedOffer.decisionPoint))
    //                DecisionPointCalss.imageMessageList.Remove(timedOffer.decisionPoint);
    //        }
    //    }

    //    timedOffer.endOfferCoroutine = null;
    //    timedOffers.Remove(timedOffer);
    //    FetchTimedOffers();
    //}

    #region ShowOnLoad

    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt(LAST_SHOW_ON_LOAD_TIME_PREF_NAME, lastShowOnLoadTime);
        PlayerPrefs.SetInt(LAST_SHOW_ON_LOAD_PRIORITY_PREF_NAME, lastShowOnLoadPriority);
        PlayerPrefs.SetString(LAST_SHOW_ON_LOAD_CAMPAIGN_ID_PREF_NAME, lastShowOnLoadCampaignID);

        PlayerPrefs.Save();
    }

    public void LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(LAST_SHOW_ON_LOAD_TIME_PREF_NAME))
            lastShowOnLoadTime = PlayerPrefs.GetInt(LAST_SHOW_ON_LOAD_TIME_PREF_NAME);

        if (PlayerPrefs.HasKey(LAST_SHOW_ON_LOAD_PRIORITY_PREF_NAME))
            lastShowOnLoadPriority = PlayerPrefs.GetInt(LAST_SHOW_ON_LOAD_PRIORITY_PREF_NAME);

        if (PlayerPrefs.HasKey(LAST_SHOW_ON_LOAD_TIME_PREF_NAME))
            lastShowOnLoadCampaignID = PlayerPrefs.GetString(LAST_SHOW_ON_LOAD_CAMPAIGN_ID_PREF_NAME);
    }

    public void TryToShowOnLoad()
    {
        if (lastShowOnLoadCampaignID != TimedOffersDeltaConfig.currentTimedOffersConfigID)
        {
            lastShowOnLoadTime = 0;
            lastShowOnLoadPriority = -1;
            lastShowOnLoadCampaignID = TimedOffersDeltaConfig.currentTimedOffersConfigID;
        }

        if (ServerTime.UnixTime(DateTime.UtcNow) - lastShowOnLoadTime < TimedOffersDeltaConfig.showOnLoadInterval)
            return;

        if (timedOffers == null || timedOffers.Count == 0)
            return;

        onTimedOffersUpdated -= TryToShowOnLoad;
        IAPController.instance.onIapInitialized -= TryToShowOnLoad;
        
        ++lastShowOnLoadPriority;

        if (lastShowOnLoadPriority > GetMaxPriority())
            lastShowOnLoadPriority = 0;

        List<TimedOfferNew> offersToShow = timedOffers.FindAll((x) => x.timedOfferPriority == lastShowOnLoadPriority);
        if (offersToShow == null || offersToShow.Count == 0)
        {
            TryToShowOnLoad();
            return;
        }
        TimedOfferNew offerToShow = offersToShow[UnityEngine.Random.Range(0, offersToShow.Count)];

        StartTryToShowOnLoadCoroutine(timedOffers.IndexOf(offerToShow));
    }

    Coroutine tryToShowOnLoadCoroutine = null;

    private void StopTryToShowOnLoadCoroutine()
    {
        if (tryToShowOnLoadCoroutine != null)
        {
            try
            { 
                StopCoroutine(tryToShowOnLoadCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            tryToShowOnLoadCoroutine = null;
        }
    }

    private void StartTryToShowOnLoadCoroutine(int idToShow)
    {
        StopTryToShowOnLoadCoroutine();
        tryToShowOnLoadCoroutine = StartCoroutine(TryToShowOnLoadCoroutine(idToShow));
    }

    IEnumerator TryToShowOnLoadCoroutine(int idToShow)
    {
        while (ShouldWait())
        {
            yield return new WaitForSeconds(1);
        }

        UIController.getHospital.TimerOffersScreen.GetComponent<TimedOffersScreenInitializer>().Initialize(null, null, idToShow);

        lastShowOnLoadTime = (int)ServerTime.UnixTime(DateTime.UtcNow);
        SaveToPlayerPrefs();
    }

    public bool ShouldWait()
    {
        HospitalUIController uic = UIController.getHospital;
        TutorialUIController tuic = TutorialUIController.Instance;

        return (Input.touchCount > 0 || Input.GetMouseButton(0) ||
               (uic != null && (
                uic.hospitalSignPopup.gameObject.activeSelf
                || uic.LevelUpPopUp.gameObject.activeSelf
                || uic.bubbleBoyEntryOverlayUI.gameObject.activeSelf
                || uic.SettingsPopUp.gameObject.activeSelf
                || uic.DailyQuestWeeklyUI.isActiveAndEnabled
                || uic.unboxingPopUp.gameObject.activeSelf
                || uic.UpdateRewardPopUp.gameObject.activeSelf
                || uic.DailyQuestAndDailyRewardUITabController.gameObject.activeSelf
                || uic.EventCenterPopup.gameObject.activeSelf)) ||
                (tuic != null && (false //TODO (Zombie)
                                       //!tuic.IsAnyOfTutorialScreenClosedAndItsFreePlayStep() 
               )));
    }

    private int GetMaxPriority()
    {
        int maxPriority = 0;

        for (int i = 0; i < timedOffers.Count; ++i)
        {
            if (timedOffers[i].timedOfferPriority > maxPriority)
                maxPriority = timedOffers[i].timedOfferPriority;
        }

        return maxPriority;
    }

    #endregion
}
