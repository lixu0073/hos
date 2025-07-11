using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.Mediation.UnityAds;
using System;
using ScriptableEventSystem;

using Debug = AdmobLogger;
using GoogleMobileAds.Common;
using GoogleMobileAds.Api.Mediation.IronSource;

/// <summary>
/// AdMob广告控制器，负责管理Google AdMob激励视频广告的初始化、加载、显示和回调处理。
/// 支持iOS和Android平台，集成了多种广告中介平台（UnityAds、IronSource等）。
/// </summary>
public class AdMobController : MonoBehaviour
{
    private const string OR_ELSE = "unexpected_platform";

    private const string IPHONE_AD_UNIT_ID = "ca-app-pub-6858306130116605/7799967771";
    private const string ANDROID_AD_UNIT_ID = "ca-app-pub-6858306130116605/9837127692"; //"ca-app-pub-6858306130116605~9837127692"; // TEST "ca-app-pub-3940256099942544~3347511713"; //OLD "ca-app-pub-6858306130116605~7761628693";

    private AdsController.AdType admobAdType;
    private RewardedAd rewardBasedVideo;
    private event Action<AdsController.AdType> onSuccess;
#if UNITY_IOS
    private event Action raiseSuccess;
    private bool onCloseRaiseSuccess = false;
#endif

    private bool rewardPlayer = false;
    private bool isCanceled = true;

    public static AdMobController Instance = null;

    private List<Action> toCallOnMainThread = new List<Action>();

#pragma warning disable 0649
    [SerializeField] private GameEvent adMobAdCanceled;
#pragma warning restore 0649

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        try
        {
            MobileAds.Initialize(HandleInitCompleteAction);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Instance = null;
        }
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // main thread.
        // We use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() => {
            UnityAds.SetGDPRConsentMetaData(true);
            IronSource.SetConsent(true);

            Debug.Log("Inited AdMob");

            var adUnitId = OR_ELSE;
#if UNITY_ANDROID
            adUnitId = ANDROID_AD_UNIT_ID;
#elif UNITY_IOS
            adUnitId = IPHONE_AD_UNIT_ID;
#endif

            rewardBasedVideo = new RewardedAd(adUnitId);
            // Called when an ad request has successfully loaded.
            rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
            // Called when the user should be rewarded for watching a video.
            rewardBasedVideo.OnUserEarnedReward += HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;

            RequestRewardBasedVideo();

        });
    }

    /// <summary>
    /// Hack because unity doesn't support multithreading, which AdMob events use
    /// </summary>
    private void Update()
    {
        if (toCallOnMainThread.Count > 0)
        {
            foreach (var action in toCallOnMainThread)
                action();

            toCallOnMainThread.Clear();
        }

        if (rewardPlayer)
        {
            rewardPlayer = false;
            if (onSuccess != null)
                onSuccess(admobAdType);
            else
                AdsController.instance.RewardPlayer(admobAdType);
        }
#if UNITY_IOS
        if (onCloseRaiseSuccess && raiseSuccess != null)
        {
            onCloseRaiseSuccess = false;
            raiseSuccess.Invoke();
        }
#endif
    }

    private void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(HandleRewardBasedVideoLoadedOnTheMainThread(sender,args));
    }

    private IEnumerator HandleRewardBasedVideoLoadedOnTheMainThread(object sender, EventArgs args) {
        Debug.Log("HandleRewardBasedVideoLoaded event received :: " + args.ToString());
        yield return null;
    }

    private void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(HandleRewardBasedVideoFailedToLoadOnTheMainThread(sender,args));
    }

    private IEnumerator HandleRewardBasedVideoFailedToLoadOnTheMainThread(object sender, AdFailedToLoadEventArgs args)
    {
        toCallOnMainThread.Add(() => { this.InvokeDelayed(this.RequestRewardBasedVideo, 10.0f); });
        Debug.LogError("HandleRewardBasedVideoFailedToLoad event received with message: " + args.LoadAdError.GetMessage());
        yield return null;
    }

    private void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(HandleRewardBasedVideoOpenedOnTheMainThread(sender,args));
    }

    private IEnumerator HandleRewardBasedVideoOpenedOnTheMainThread(object sender, EventArgs args)
    {
        toCallOnMainThread.Add(SoundsController.Instance.PauseSoundsAndMusic);
        isCanceled = true;
        Debug.Log("HandleRewardBasedVideoOpened event received :: " + args.ToString());

        yield return null;
    }

    private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(HandleRewardBasedVideoClosedOnTheMainThread(sender,args));
    }

    private IEnumerator HandleRewardBasedVideoClosedOnTheMainThread(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoClosed event received :: " + args.ToString());

        toCallOnMainThread.Add(this.RequestRewardBasedVideo);
        if (isCanceled)
            toCallOnMainThread.Add(ExitPopups);

        isCanceled = true;
        toCallOnMainThread.Add(UIController.get.preloaderView.Exit);
        toCallOnMainThread.Add(RestoreSoundAndMusic);

        adMobAdCanceled.Invoke(this, null);
        yield return null;
    }

    private void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
#if UNITY_ANDROID
        UnityMainThreadDispatcher.Instance().Enqueue(HandleRewardBasedVideoRewardedOnTheMainThread(sender, args));
#elif UNITY_IOS
        string type = args.Type;
        double amount = args.Amount;

        onCloseRaiseSuccess = true;
        raiseSuccess = new Action(() =>
        {
            if (onSuccess != null)
                onSuccess(admobAdType);
            else
            {
                AdsController.instance.RewardPlayer(admobAdType);
            }
        });

        toCallOnMainThread.Add(UIController.get.preloaderView.Exit);
        isCanceled = false;
#endif
    }

    private IEnumerator HandleRewardBasedVideoRewardedOnTheMainThread(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;

        rewardPlayer = true;

        toCallOnMainThread.Add(this.RequestRewardBasedVideo);
        toCallOnMainThread.Add(UIController.get.preloaderView.Exit);
        isCanceled = false;

        yield return null;
    }

    private void RequestRewardBasedVideo()
    {
        Debug.Log("In RequestRewardBasedVideo");
        // Create an empty ad request.
        try
        {
            AdRequest.Builder builder = new AdRequest.Builder();
            AdRequest request = builder.Build();
            // Load the rewarded video ad with the request.
            rewardBasedVideo.LoadAd(request);
        }
        catch (Exception e)
        {
            Debug.LogFormat("Ad not loaded: {0}", e.Message);
            this.InvokeDelayed(this.RequestRewardBasedVideo, 120f);
        }
    }

    public void ShowAd(AdsController.AdType adType, Action<AdsController.AdType> onSuccess)
    {
        Debug.Log("In ShowAd()");
        Debug.Log("rewardBasedVideo.IsLoaded : " + rewardBasedVideo.IsLoaded());
        Debug.Log("isCanceled : " + isCanceled);

        if (rewardBasedVideo.IsLoaded())
        {
            admobAdType = adType;
            SetOnSuccess(onSuccess);
            rewardBasedVideo.Show();
        }
        else
        {
            Debug.LogError("Failed to load ad from AdMob");
            toCallOnMainThread.Add(UIController.get.preloaderView.Exit);
        }
    }

    private void SetOnSuccess(Action<AdsController.AdType> onSuccess)
    {
        this.onSuccess = (AdsController.AdType adType) =>
        {
            Debug.Log("Loaded ad");
            Game.Instance.gameState().IncrementAdsWatched();
            onSuccess(adType);
        };
    }

    public bool IsAdReady()
    {
        return rewardBasedVideo.IsLoaded();
    }

    // Run it on main UnityThread
    private void ExitPopups()
    {
        if (isCanceled)
        {
            try
            {
                UIController.getHospital.BillboardAdPopUp.Exit();
                UIController.getHospital.bubbleBoyEntryOverlayUI.ButtonExit();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    // Run it on main UnityThread
    private void RestoreSoundAndMusic()
    {
        if (SoundsController.Instance != null)
        {
            if (SoundsController.Instance.IsMusicEnabled())
                SoundsController.Instance.UnmuteMusic();
            if (SoundsController.Instance.IsSoundEnabled())
                SoundsController.Instance.UnmuteSound();
        }
    }
}
