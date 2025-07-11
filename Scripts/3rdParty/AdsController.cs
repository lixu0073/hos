using UnityEngine;
using System;
using System.Collections.Generic;

using Debug = AdmobLogger;

/// <summary>
/// 广告控制器，统一管理游戏中所有类型的广告（激励视频、横幅广告等）。
/// 提供广告配置管理、广告奖励倍数计算和广告显示接口。
/// </summary>
public class AdsController : MonoBehaviour
{
    public static AdsController instance;

    Dictionary<AdType, AdsConfig> adConfigs;

    //public delegate void OnSuccessVitaminCollectorAdWatch();
    //public static event OnSuccessVitaminCollectorAdWatch onSuccessVitaminCollectorAdWatch;

    private const string adRewardMultiplierKey = "adRewardMultiplier";

    private int adRewardMultiplier = 10;

    public int AdRewardMultiplier
    {
        get { return adRewardMultiplier; }
        private set { }
    }

    private const int defaultAdRewardMultiplier = 10;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateDefaultConfigs();
    }

    private void Start()
    {
        SetAdConfigs(DefaultConfigurationProvider.GetAdPointCDatas(Game.Instance.gameState().GetHospitalLevel()));
    }

    private Hospital.BalanceableInt rewardedAdMedicineProductionCooldownBalanceable;

    private int RewardedAdMedicineProductionCooldown
    {
        get
        {
            if (rewardedAdMedicineProductionCooldownBalanceable == null)
                rewardedAdMedicineProductionCooldownBalanceable = Hospital.BalanceableFactory.CreateRewardAdMedicineProductionCooldownBalanceable();

            return rewardedAdMedicineProductionCooldownBalanceable.GetBalancedValue();
        }
    }

    private Hospital.BalanceableInt rewardedAdCoinsAmountMultiplierBalanceable;

    private int RewardedAdCoinsAmount
    {
        get
        {
            if (rewardedAdCoinsAmountMultiplierBalanceable == null)
                rewardedAdCoinsAmountMultiplierBalanceable = Hospital.BalanceableFactory.CreateCoinsForAdsAmountBalanceable();

            return rewardedAdCoinsAmountMultiplierBalanceable.GetBalancedValue();
        }
    }

    private Hospital.BalanceableInt rewardedAdDiamondsAmountMultiplierBalanceable;

    private int RewardedAdDiamondsAmount
    {
        get
        {
            if (rewardedAdDiamondsAmountMultiplierBalanceable == null)
                rewardedAdDiamondsAmountMultiplierBalanceable = Hospital.BalanceableFactory.CreateDiamondsForAdsAmountBalanceable();

            return rewardedAdDiamondsAmountMultiplierBalanceable.GetBalancedValue();
        }
    }

    void CreateDefaultConfigs()
    {
        //create configs for ad types and set their default values
        adConfigs = new Dictionary<AdType, AdsConfig>();
        adConfigs.Add(AdType.rewarded_ad_billboard, new AdsConfig(AdType.rewarded_ad_billboard, 180, -1, 1));
        adConfigs.Add(AdType.rewarded_ad_coins, new AdsConfig(AdType.rewarded_ad_coins, 300, -1, 25));
        adConfigs.Add(AdType.rewarded_ad_bubbleboy, new AdsConfig(AdType.rewarded_ad_bubbleboy));
        adConfigs.Add(AdType.rewarded_ad_dailyquest, new AdsConfig(AdType.rewarded_ad_dailyquest));
        adConfigs.Add(AdType.rewarded_ad_vitamin_collector, new AdsConfig(AdType.rewarded_ad_vitamin_collector));
        adConfigs.Add(AdType.rewarded_ad_medicine_production, new AdsConfig(AdType.rewarded_ad_medicine_production, 30));
        adConfigs.Add(AdType.rewarded_ad_missed_daily_reward, new AdsConfig(AdType.rewarded_ad_missed_daily_reward));
    }

    public bool IsAdAvailable(AdType adType)
    {
        if (adConfigs[adType].IsOnCooldown() || adConfigs[adType].IsOnDailyLimit())
            return false;
        return AdMobController.Instance.IsAdReady();
    }

    public bool IsAdMobAdAvailable()
    {
        return AdMobController.Instance.IsAdReady();
    }

    public bool IsAdOnCooldown(AdType adType)
    {
        return adConfigs[adType].IsOnCooldown();
    }

    public bool IsAdOnDailyLimit(AdType adType)
    {
        return adConfigs[adType].IsOnDailyLimit();
    }

    public long GetSecondsToNextAd(AdType adType)
    {
        return adConfigs[adType].GetScondsToNextAd();
    }

    public delegate void OnAdSuccessLoad();
    public delegate void OnAdFailureLoad(Exception e = null);
    //public static event OnAdSuccessLoad onAdSuccessLoad;
    //public static event OnAdFailureLoad onAdFailureLoad;

    public void ShowAd(AdType adType, OnAdSuccessLoad onSuccess = null, OnAdFailureLoad onFailure = null)
    {
        if (DefaultConfigurationProvider.GetConfigCData().AddAdsRewardFromSaveLogic)
        {
            Game.Instance.gameState().SetIsAdRewardActive(false);
            Game.Instance.gameState().SetAdType(adType.ToString());
        }

        adConfigs[adType].Increment();

        AnalyticsController.instance.ReportShowAd(adType);

#if UNITY_EDITOR
        onSuccess?.Invoke();
        RewardPlayer(adType);
#else
        UIController.get.preloaderView.Open(PreloaderView.PreloadViewMode.Ads);
        AdMobController.Instance.ShowAd(adType, (type) => { RewardPlayer(type); if (onSuccess != null) onSuccess(); } );
#endif
    }

    public int GetRawRewardAmount(AdType adType)
    {
        return (int)adConfigs[adType].rewardAmount;
    }

    public int GetRewardAmount(AdType adType)
    {
        if (adType == AdType.rewarded_ad_coins)
            return RewardedAdCoinsAmount;

        if (adType == AdType.rewarded_ad_billboard)
            return RewardedAdDiamondsAmount;

        return GetRawRewardAmount(adType);
    }

    public void RewardPlayer(AdType adType)
    {
        Debug.Log("in RewardPlayer");
        if (DefaultConfigurationProvider.GetConfigCData().AddAdsRewardFromSaveLogic)
        {
            Game.Instance.gameState().SetIsAdRewardActive(false);
        }
        Debug.Log("in RewardPlayer before switch" + adType.ToString());
        switch (adType)
        {
            case AdType.rewarded_ad_billboard:
                if (ReferenceHolder.GetHospital() != null)
                    ReferenceHolder.GetHospital().BillboardAd.RewardPlayer(true);
                break;
            case AdType.rewarded_ad_bubbleboy:
                UIController.getHospital.bubbleBoyMinigameUI.PopMore();
                break;
            case AdType.rewarded_ad_dailyquest:
                UIController.getHospital.ReplaceDailyTaskPopup.ReplaceVideoAdReward();
                break;
            case AdType.rewarded_ad_coins:
                UIController.get.BillboardAdPopUp.ShowRewardContentCoin();
                /*int coinAmount = videoForCoinsReward;
                GameState.Get().AddCoins(coinAmount, EconomySource.RewardedVideo, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, coinAmount, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), IAPController.instance.coinsSprite, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Coin, coinAmount);
                });*/
                break;
            case AdType.rewarded_ad_missed_daily_reward:
                //ReferenceHolder.GetHospital().DailyRewardController.CollecRewardDueToAd();
                break;
            case AdType.rewarded_ad_vitamin_collector:
                UIController.get.vitaminsMakerRefillmentPopup.RewardPlayerForWatchingAd();
                break;
            case AdType.rewarded_ad_medicine_production:
                break;
            default:
                break;
        }
    }

    public void SetAdConfigs(List<AdPointCData> adPointCDatas)
    {
        foreach (var data in adPointCDatas)
        {
            AdType adType = (AdType)Enum.Parse(typeof(AdType),data.AdPoint);
            adConfigs[adType].cooldown = data.AdCooldownSeconds;
            if (adType == AdType.rewarded_ad_coins)
            {
                if  (data.AdRewardMultiplier != 0)
                {
                    AdRewardMultiplier = data.AdRewardMultiplier;
                }
                else
                {
                    adRewardMultiplier = defaultAdRewardMultiplier;
                }
            }
            else
            {
                adConfigs[adType].rewardAmount = data.AdRewardAmount;
            }
            adConfigs[adType].sessionLimit = data.AdSessionLimit;
            adConfigs[adType].dailyLimit = data.AdDailyLimit;
        }

        //AdType adType = (AdType)Enum.Parse(typeof(AdType), parameters["adPoint"].ToString());
        //if (parameters.ContainsKey("adCooldownSeconds"))
        //    adConfigs[adType].cooldown = int.Parse(parameters["adCooldownSeconds"].ToString(), System.Globalization.CultureInfo.InvariantCulture);

        //if (adType == AdType.rewarded_ad_coins)
        //{
        //    if (parameters.ContainsKey(adRewardMultiplierKey))
        //        adRewardMultiplier = int.Parse(parameters[adRewardMultiplierKey].ToString());
        //    else
        //        adRewardMultiplier = defaultAdRewardMultiplier;
        //}
        //else
        //{
        //    if (parameters.ContainsKey("adRewardAmount"))
        //        adConfigs[adType].rewardAmount = int.Parse(parameters["adRewardAmount"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
        //}

        //if (parameters.ContainsKey("adDailyLimit"))
        //    adConfigs[adType].dailyLimit = int.Parse(parameters["adDailyLimit"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
        //if (parameters.ContainsKey("adSesionLimit"))
        //    adConfigs[adType].sessionLimit = int.Parse(parameters["adSesionLimit"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    //public void SetAdConfig(Dictionary<string, object> parameters)
    //{
    //    AdType adType = (AdType)Enum.Parse(typeof(AdType), parameters["adPoint"].ToString());
    //    if (parameters.ContainsKey("adCooldownSeconds"))
    //        adConfigs[adType].cooldown = int.Parse(parameters["adCooldownSeconds"].ToString(), System.Globalization.CultureInfo.InvariantCulture);

    //    if (adType == AdType.rewarded_ad_coins)
    //    {
    //        if (parameters.ContainsKey(adRewardMultiplierKey))
    //            adRewardMultiplier = int.Parse(parameters[adRewardMultiplierKey].ToString());
    //        else
    //            adRewardMultiplier = defaultAdRewardMultiplier;
    //    }
    //    else
    //    {
    //        if (parameters.ContainsKey("adRewardAmount"))
    //            adConfigs[adType].rewardAmount = int.Parse(parameters["adRewardAmount"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
    //    }

    //    if (parameters.ContainsKey("adDailyLimit"))
    //        adConfigs[adType].dailyLimit = int.Parse(parameters["adDailyLimit"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
    //    if (parameters.ContainsKey("adSesionLimit"))
    //        adConfigs[adType].sessionLimit = int.Parse(parameters["adSesionLimit"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
    //}

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            foreach (KeyValuePair<AdType, AdsConfig> config in adConfigs)
            {
                adConfigs[config.Key].Save();
            }
            PlayerPrefs.Save();
        }
    }

    public enum AdType
    {
        //ad type name must be the same as decisionPoint and adPoint which triggers this ad type
        //every new ad type should be added to adConfigs dictionary
        rewarded_ad_billboard,
        rewarded_ad_bubbleboy,
        rewarded_ad_dailyquest,
        rewarded_ad_coins,
        rewarded_ad_vitamin_collector,
        rewarded_ad_medicine_production,
        rewarded_ad_missed_daily_reward,
    }

    class AdsConfig
    {
        public long cooldown = 0;
        public long dailyLimit = 45;
        public long rewardAmount = 1;
        public long sessionLimit = 10; //Why is this here? It's not used

        AdType adType;
        long lastShowDate = 0;
        int adsToday = 0;
        int lastShowDay = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adType"></param>
        /// <param name="cooldown"></param>
        /// <param name="dailyLimit">
        /// Set -1 to for no limit.
        /// </param>
        /// <param name="rewardAmount"></param>
        public AdsConfig(AdType adType, int cooldown = 0, int dailyLimit = -1, int rewardAmount = 0)
        {
            this.adType = adType;
            this.cooldown = cooldown;
            this.dailyLimit = dailyLimit;
            this.rewardAmount = rewardAmount;

            Load();
        }

        void Load()
        {
            lastShowDate = (long)PlayerPrefs.GetInt(adType.ToString() + "_lastShowDate");
            adsToday = PlayerPrefs.GetInt(adType.ToString() + "_adsToday");
            lastShowDay = PlayerPrefs.GetInt(adType.ToString() + "_lastShowDay");
        }

        public void Save()
        {
            PlayerPrefs.SetInt(adType.ToString() + "_lastShowDate", (int)lastShowDate);
            PlayerPrefs.SetInt(adType.ToString() + "_adsToday", adsToday);
            PlayerPrefs.SetInt(adType.ToString() + "_lastShowDay", lastShowDay);
        }

        public long GetScondsToNextAd()
        {
            return lastShowDate + cooldown - ServerTime.UnixTime(DateTime.UtcNow);
        }

        public bool IsOnCooldown()
        {
            if (ServerTime.UnixTime(DateTime.UtcNow) > lastShowDate + cooldown)
                return false;
            return true;
        }

        public bool IsOnDailyLimit()
        {
            if (lastShowDay != DateTime.UtcNow.Day)
            {     //check if the day changed. If yes - reset ads today
                adsToday = 0;
                lastShowDay = DateTime.UtcNow.Day;
            }

            if (dailyLimit == -1 || adsToday < dailyLimit)
                return false;

            return true;
        }

        public void Increment()
        {
            ++adsToday;
            lastShowDate = ServerTime.UnixTime(DateTime.UtcNow);
            lastShowDay = DateTime.UtcNow.Day;
        }
    }

    [ContextMenu("Save ad status")]
    private void SaveAdStatus()
    {
        foreach (KeyValuePair<AdType, AdsConfig> config in adConfigs)
        {
            adConfigs[config.Key].Save();
        }
        PlayerPrefs.Save();
    }
}