using UnityEngine;
using System;
using Hospital.Connectors;

namespace Hospital
{
    public class AnalyticsManager : MonoBehaviour
    {
        #region Static

        private static AnalyticsManager instance;

        public static AnalyticsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("There is no AnalyticsManager instance on scene!");
                }
                return instance;
            }
        }

        #endregion

        #region Behaviour

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of AnalyticsManager on scene!");
            }
            instance = this;
        }

        #endregion

        #region Params

        private AnalyticsModel data;
        private AnalyticsModel Data
        {
            get
            {
                GlobalDataHolder holder = GlobalDataHolder.instance;
                if (holder != null)
                {
                    if(holder.analiticData != null)
                    {
                        return holder.analiticData;
                    }
                }
                return data;
            }
            set
            {
                GlobalDataHolder holder = GlobalDataHolder.instance;
                if (holder != null)
                {
                    holder.analiticData = value;
                }
                data = value;
            }
        }

        #endregion

        #region Api

        public enum Type
        {
            BEFORE_MAIN_SCENE_LOADED,
            ON_MAIN_SCENE_LOADED,
            ON_LAUNCH,
            REPORT_ON_LOADING_SCENE,
            REPORT_ON_MAIN_SCENE,
            TIME
        }

        public void OnMainSceneLoaded()
        {
            Send(Type.ON_MAIN_SCENE_LOADED);
        }

        public void OnLaunched()
        {
            Send(Type.ON_LAUNCH);
        }

        public void Send(Type type, float assetBundleProgressStatus = 0, bool assetBundleDownloading = false, float assetBundleDownloadingTime = 0, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (Data == null)
            {
                return;
            }
            switch (type)
            {
                case Type.BEFORE_MAIN_SCENE_LOADED:
                    SendAnalyticsData(type, assetBundleProgressStatus, assetBundleDownloading, assetBundleDownloadingTime, false, false, false, onSuccess, onFailure);
                    break;
                case Type.ON_MAIN_SCENE_LOADED:
                    SendAnalyticsData(type, 100, false, assetBundleDownloadingTime, false, false, true, onSuccess, onFailure);
                    break;
                case Type.ON_LAUNCH:
                    SendAnalyticsData(type, 100, false, assetBundleDownloadingTime, false, true, true, onSuccess, onFailure);
                    break;
                case Type.REPORT_ON_LOADING_SCENE:
                    SendAnalyticsData(type, assetBundleProgressStatus, assetBundleDownloading, assetBundleDownloadingTime, true, false, false, onSuccess, onFailure);
                    break;
                case Type.REPORT_ON_MAIN_SCENE:
                    SendAnalyticsData(type, assetBundleProgressStatus, assetBundleDownloading, assetBundleDownloadingTime, true, false, true, onSuccess, onFailure);
                    break;
                case Type.TIME:
                    SendAnalyticsData(type, assetBundleProgressStatus, assetBundleDownloading, assetBundleDownloadingTime, false, false, false, onSuccess, onFailure);
                    break;
            }
        }

        public bool IsFirstLaunch()
        {
            return PlayerPrefs.GetInt("FirstLaunch") == 0;
        }

        public void SetFirstLaunched()
        {
            PlayerPrefs.SetInt("FirstLaunch", 1);
        }

        private async void SendAnalyticsData(Type type, float assetBundleProgressStatus, bool assetBundleDownloading, float assetBundleDownloadingTime, bool reported, bool Launched, bool enteredMainScene, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            Data.CognitoID = CognitoEntry.UserID;
            Data.type = (int)type;
            Data.GameVersion = int.Parse(LoadingGame.version, System.Globalization.CultureInfo.InvariantCulture);
            if (type != Type.ON_LAUNCH && type != Type.ON_MAIN_SCENE_LOADED)
            {
                Data.CurrentTime = DateTime.UtcNow.ToString();
                Data.AssetBundleProgressStatus = assetBundleProgressStatus;
                Data.AssetBundleDownloading = assetBundleDownloading;
                Data.AssetBundleDownloadingTime = assetBundleDownloadingTime;
                Data.FirstLaunch = IsFirstLaunch();
                Data.DiskSpaceInBytes = Caching.defaultCache.spaceFree; //Caching.spaceFree;
                Data.MemorySize = SystemInfo.systemMemorySize;
                Data.OSVersion = SystemInfo.operatingSystem;
                Data.DeviceModel = SystemInfo.deviceModel;
                Data.LanguageCode = Application.systemLanguage.ToString();
                Data.CountryCode = Application.systemLanguage.ToString();
                Data.ViaWiFi = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
                Data.Reported = reported;
                Data.EnteredMainScene = enteredMainScene;
            }
            if(type == Type.ON_MAIN_SCENE_LOADED || type == Type.ON_LAUNCH)
            {
                Data.EnteredMainScene = true;
            }
            Data.Launched = Launched;

            try
            {
                await AnalyticsConnector.SaveAsync(Data);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void LoadAnalyticsData()
        {
            try
            {
                var result = await AnalyticsConnector.LoadAsync(CognitoEntry.UserID);
                this.Data = result == null ? new AnalyticsModel() : result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #endregion
    }
}
