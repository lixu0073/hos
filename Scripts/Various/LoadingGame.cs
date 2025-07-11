using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using IsoEngine;
using AssetBundles;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using Balaso;
using Hospital.Connectors;
using UnityEngine.Networking;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Analytics;
using Unity.Services.Core.Environments;
using System.Threading.Tasks;
#if UNITY_IOS
using Facebook.Unity;
#endif

namespace Hospital
{
    public class LoadingGame : MonoBehaviour, LoadingGame.IProgressable
    {
        public interface IProgressable
        {
            void SetProgressValue(float value);
            void SetProgressTitle(string title);
            int GetMinProgress();
            int GetMaxProgress();
            void GoNextOperation();
            AlertPopupController GetAlertPopup();
            void ProcessIfInternetConnection(LoadingGame.Callback callback);
            string GetBundleURLDir(bool checkSmallAB = false);
            void AplyUserLanguage();
            void AplyMainFontReferences();
            void StopIntoAnimationCoroutine();
            void ShowIntro();
            Queue<BasePartOperation> GetOperations();
            void SetPlayButtonAction(UnityAction action);
            void SetStartingPanelActive(bool setActive);
        }
#pragma warning disable 0649
        [SerializeField] private bool isIphoneXDevToggle;
        [SerializeField] public AudioSource musicSource;
        [SerializeField] private AlertPopupController alertPopup;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private EngineController engine;
        [SerializeField] private TextMeshProUGUI supportCodeText;
        [SerializeField] private TextMeshProUGUI devText;
        [SerializeField] private RectTransform ProgressBarRect;
        [SerializeField] private RectTransform SupportCodeRect;
        [SerializeField] public RectTransform TipTextRect;
        [SerializeField] private GameObject startingPanel;
        [SerializeField] private Button PlayButton;
        [SerializeField] private GameObject splashArt;
        [SerializeField] private Sprite[] musicSprites;
        [SerializeField] private Sprite[] soundSprites;
        [SerializeField] private TextMeshProUGUI musicText;
        [SerializeField] private TextMeshProUGUI soundText;
        [SerializeField] private Image musicImage;
        [SerializeField] private Image soundImage;
#pragma warning restore 0649
        // For Intro animations //
        Coroutine animCorountineCheck;

        public delegate void Callback();

        // consts //
        private static readonly string BundleURLDir = "https://d12m2666yx93mm.cloudfront.net/";
        //private static readonly string DevelopBundleURLDir = "https://s3-eu-west-1.amazonaws.com/cpgmhdevelop/";

        // Game Version //
        public static string version
        {
            get { return DeveloperParametersController.Instance().parameters.GameVersion; }
            private set { }
        }

        // Asset Bundles Versions //
        public static int MainSceneABVersion
        {
            get { return DeveloperParametersController.Instance().parameters.MainSceneAssetBundleVersion; }
            private set { }
        }

        public static int MaternitySceneABVersion
        {
            get { return DeveloperParametersController.Instance().parameters.MaternitySceneAssetBundleVersion; }
            private set { }
        }

        public static int FontsABVersion
        {
            get { return DeveloperParametersController.Instance().parameters.FontsAssetBundleVersion; }
            private set { }
        }

        public static readonly string MainSceneABName = "main_scene_ab";
        public static readonly string MaternitySceneABName = "maternity_scene_ab";
        public static readonly string FontsABName = "fonts_ab";

        // Audio switcher //
        public bool splashAudio;

        private AudioSource loadingAudio;

#pragma warning disable 0649
        // IPhoneX //
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private RuntimeAnimatorController iphonexIntroAnimator;
#pragma warning restore 0649
        // Intro //
        private GameObject intro;
        private ResourceRequest introResourceRequest;
#pragma warning disable 0649
        [SerializeField] private GameObject ProgressBarContainer;
        [SerializeField] private TextMeshProUGUI TipText;
#pragma warning restore 0649
        // Langs //
        private string defaultLanguage = "English";
        private string userLanguage;

        // Fonts //
        public static bool AreFontsChecked = false;

        // Asset Bundles references //
        public static AssetBundle bundle = null;
        public static AssetBundle maternityBundle = null;

        async void Awake()
        {
            splashAudio = true;
#if NTT
            PlayerPrefs.SetInt("NTT", 1);
            PlayerPrefs.Save();
#endif
            DefaultConfigurationProvider.LoadConfigurations();
            await InitUnityServices();
            await AnalyticsController.instance.Init();
        }

        private async Task InitUnityServices()
        {
            string userID = PlayerPrefs.GetString("DDSDK_USER_ID");
            UnityServices.ExternalUserId = userID;
            var options = new InitializationOptions();
            options.SetEnvironmentName("development");
            await UnityServices.InitializeAsync(options);
         
            // StartCoroutine(LoadingRemoteConfig());
        }

        //private IEnumerator LoadingRemoteConfig()
        //{
        //    if (TryGetComponent<RemoteSettingsManager>(out RemoteSettingsManager remoteSettingsMgr))
        //        yield return remoteSettingsMgr.Init();
        //    else
        //        Debug.LogError("RemoteSettingManager component missing in LoadingGame object");
        //}

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        // Asset Bundles Methods //

        public static string GetRootURL()
        {
            // return DevelopBundleURLDir;
            return BundleURLDir;
        }

        public string GetBundleURLDir(bool checkSmallAB = false)
        {
            return GetRootURL() + GetSmallPath(checkSmallAB) + AssetBundles.Utility.GetPlatformName() + "/";
        }

        private string GetSmallPath(bool checkSmallAB = false)
        {
            if (!checkSmallAB)
                return "";
#if UNITY_IOS
            if (SystemInfo.systemMemorySize <= 1024)
                return "small/";
            return "";
#else
            return "";
#endif
        }

        void Start()
        {
#if UNITY_IOS
            // Registering callback to get the user response to IDFA question
            AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
            if (AppTrackingTransparency.TrackingAuthorizationStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
                AppTrackingTransparency.RequestTrackingAuthorization();
            else
            {
                // AF SDK initialization
                AppsFlyerController.instance.AppsFlyerSetCustomerUserId(AnalyticsController.GetUserID());
                AppsFlyerController.instance.InitAppsFlyer();
            }
            // FB SDK initialization
            if (!FB.IsInitialized)
            {
                Debug.LogFormat("<color=yellow> LoadingGame::Start FB Initialized </color>");
                FB.Init(onInitCompleteCallback, onHideUnityCallback);
            }
            if (SystemInfo.deviceModel == "iPhone10,3" || SystemInfo.deviceModel == "iPhone10,6" || SystemInfo.deviceModel == "iPhone11,2" || SystemInfo.deviceModel == "iPhone11,4" || SystemInfo.deviceModel == "iPhone11,6" || SystemInfo.deviceModel == "iPhone11,8")
            {
                ProgressBarRect.localPosition += new Vector3(0, 15, 0);
                SupportCodeRect.localPosition += new Vector3(-84.7f, 31.586f, 0);
                TipTextRect.localPosition += new Vector3(0, 26.6f, 0);
            }
#elif UNITY_EDITOR
            if (isIphoneXDevToggle)
            {
                ProgressBarRect.localPosition += new Vector3(0, 15, 0);
                SupportCodeRect.localPosition += new Vector3(-84.7f, 31.586f, 0);
                TipTextRect.localPosition += new Vector3(0, 26.6f, 0);
            }
#endif
            CognitoEntry.OnUserIDRetrieval -= CognitoEntry_OnRetrieval;
            CognitoEntry.OnUserIDRetrieval += CognitoEntry_OnRetrieval;
            CognitoEntry.AuthCognito();

            AnalyticsController.instance.ReportBug("loadinggame.start");

            AplyDefaultLanguage();

            //#if UNITY_ANDROID
            //            AreFontsChecked = true;
            //            AplyUserLanguage();
            //#endif

            if (IsCriticalErrorOccured())
            {
                AnalyticsController.instance.ReportBug("loadinggame.criticalerror");

                StartCoroutine(alertPopup.Open(AlertType.CRITICAL_LOCAL, null, null, () =>
                {
                    alertPopup.button.RemoveAllOnClickListeners();
                    alertPopup.button.onClick.AddListener(delegate
                    {
                        if (GlobalDataHolder.instance != null)
                            GlobalDataHolder.instance.IsCriticalErrorOccured = false;
                        alertPopup.NoSmoothExit();
                        Start();
                    });
                    return;
                }));
                return;
            }

            Debug.LogFormat("<color=magenta>[FACEBOOK] SDK: {0}</color>", Facebook.Unity.FacebookSdkVersion.Build);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SetMusicButton();
            SetSoundButton();
            SetUpOperations();
        }

#if UNITY_IOS
        private void onInitCompleteCallback()
        {
            FB.Mobile.SetAutoLogAppEventsEnabled(LoadingGame.IDFA_Status);
            FB.Mobile.SetAdvertiserIDCollectionEnabled(LoadingGame.IDFA_Status);
            Debug.LogFormat("<color=yellow> IDFAStatus: </color>" + LoadingGame.IDFA_Status);
        }

        private void onHideUnityCallback(bool isGameShown)
        {
            Time.timeScale = isGameShown ? 1 : 0;
        }
#endif

        #region IDFA
        /// <summary>
        /// Identifier For Advertising. Used on iOS to track the user.
        /// </summary>
        public static bool IDFA_Status = false;

        // Callback to manage the event when the user selects the IDFA option in the game
        private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status)
        {
#if UNITY_IOS
            IDFA_Status = status == AppTrackingTransparency.AuthorizationStatus.AUTHORIZED;

            // Obtain IDFA
            Debug.Log($"IDFA: {AppTrackingTransparency.IdentifierForAdvertising()}");

            // AF SDK initialization
            AppsFlyerController.instance.AppsFlyerSetCustomerUserId(AnalyticsController.GetUserID());
            AppsFlyerController.instance.InitAppsFlyer();
#endif
        }

        #endregion


        #region SUPPORT CODE

        private void CognitoEntry_OnRetrieval(string cognito)
        {
            string supportCode = GenerateSupportCode(cognito);
            supportCodeText.text = "Id: " + supportCode;
            TryToSendSupportCodeToAnalytics(supportCode);
            CacheManager.TryToSaveFirstLaunchDate(cognito);
        }

        public static void TryToSendSupportCodeToAnalytics(string supportCode)
        {
            AnalyticsGeneralParameters.supportCode = supportCode;
        }

        public static string GenerateSupportCode(string input)
        {
            StringBuilder sb = new StringBuilder();
            string[] inputArray = input.Split(':');
            if (inputArray.Length > 1)
            {
                inputArray = inputArray[1].Split('-');
                for (int i = 0; i < inputArray.Length; ++i)
                {
                    sb.Append(inputArray[i].Substring(0, 3));
                }

                return sb.ToString();
            }

            return "";
        }

        #endregion

        #region IPROGRESSABLE

        public void SetPlayButtonAction(UnityAction action)
        {
            PlayButton.RemoveAllOnClickListeners();
            PlayButton.onClick.AddListener(action);
        }

        public void SetStartingPanelActive(bool setActive)
        {
            startingPanel.SetActive(setActive);
            splashArt.SetActive(!setActive);
        }

        public void SetProgressValue(float value)
        {
            progressFill.fillAmount = value;
        }

        public void SetProgressTitle(string title)
        {
            progressText.text = title;
        }

        void SetMusicButton()
        {
            bool musicEnabled = PlayerPrefs.GetInt("music_muted") == 0;

            if (musicEnabled)
            {
                musicText.text = "MUSIC ON";
                musicImage.sprite = musicSprites[0];
            }
            else
            {
                musicText.text = "MUSIC OFF";
                musicImage.sprite = musicSprites[1];
            }

            musicSource.enabled = musicEnabled;
        }

        void SetSoundButton()
        {
            bool soundEnabled = PlayerPrefs.GetInt("sound_muted") == 0;
            if (soundEnabled)
            {
                soundText.text = "SOUND ON";
                soundImage.sprite = soundSprites[0];
            }
            else
            {
                soundText.text = "SOUND OFF";
                soundImage.sprite = soundSprites[1];
            }
        }

        public void OnToggleSoundButtonClick()
        {
            int isSoundMuted = PlayerPrefs.GetInt("sound_muted");
            isSoundMuted = isSoundMuted > 0 ? 0 : 1;
            PlayerPrefs.SetInt("sound_muted", isSoundMuted);
            SetSoundButton();
        }

        public void OnToggleMusicButtonClick()
        {
            int isMusicMuted = PlayerPrefs.GetInt("music_muted");
            isMusicMuted = isMusicMuted > 0 ? 0 : 1;
            PlayerPrefs.SetInt("music_muted", isMusicMuted);
            SetMusicButton();
        }

        public int GetMinProgress()
        {
            return 0;
        }

        public int GetMaxProgress()
        {
            return 90;
        }

        public Queue<BasePartOperation> GetOperations()
        {
            return operations;
        }

        public AlertPopupController GetAlertPopup()
        {
            return alertPopup;
        }

        #endregion

        #region SET UP OPERATIONS

        private Queue<BasePartOperation> operations = new Queue<BasePartOperation>();
        private BasePartOperation operation;

        private void SetUpOperations()
        {
            operations.Clear();
            if (DeveloperParametersController.Instance().parameters.UseAssetBundles)
            {
                float lastEndProgress = 1;
                operations.Enqueue(new CheckingVersionOperation(this));
                operations.Enqueue(new ShowStartPanelOperation(this));
#if UNITY_IOS
                operations.Enqueue(new CheckAssetBundleToDownloadSizeOperation(this));
#endif
                //TODO LOAD CONFIGS
                //operations.Enqueue(new LoadDeltaConfigsOperation(this));
                operations.Enqueue(new LoadGameBundlesOperation(this));

                BasePartOperation downloadFontsOperation = new DownloadFontsOperation(this)
                {
                    StartProgress = 1
                };
                downloadFontsOperation.EndProgress = downloadFontsOperation.ToDownload() ? 5 : 1;
                lastEndProgress = downloadFontsOperation.EndProgress;
                operations.Enqueue(downloadFontsOperation);

                BasePartOperation downloadMaternitySceneOperation = new DownloadMaternitySceneOperation(this)
                {
                    StartProgress = lastEndProgress
                };
                downloadMaternitySceneOperation.EndProgress = downloadMaternitySceneOperation.ToDownload()
                    ? (GetPlatform() == Platform.IOS ? 40 : 80)
                    : lastEndProgress;
                downloadMaternitySceneOperation.EndProgressText = downloadMaternitySceneOperation.ToDownload()
                    ? (GetPlatform() == Platform.IOS ? 40 : 100)
                    : lastEndProgress;
                lastEndProgress = downloadMaternitySceneOperation.EndProgress;
                operations.Enqueue(downloadMaternitySceneOperation);

                if (GetPlatform() == Platform.IOS)
                {
                    BasePartOperation downloadMainSceneOperation = new DownloadMainSceneOperation(this)
                    {
                        StartProgress = lastEndProgress
                    };
                    downloadMainSceneOperation.EndProgress =
                        downloadMainSceneOperation.ToDownload() ? 80 : lastEndProgress;
                    lastEndProgress = downloadMainSceneOperation.EndProgress;
                    downloadMainSceneOperation.EndProgressText = 100;
                    operations.Enqueue(downloadMainSceneOperation);
                }

                operations.Enqueue(new ShowIntroOperation(this));
                operations.Enqueue(new LoadNativeFontOperation(this));
                BasePartOperation loadMainSceneOperation = new LoadMainSceneOperation(this)
                {
                    StartProgress = lastEndProgress,
                    EndProgress = 90
                };
                operations.Enqueue(loadMainSceneOperation);
            }
            else
            {
                operations.Enqueue(new ShowStartPanelOperation(this));
                operations.Enqueue(new LoadFontsFromResourcesOperation(this));
                operations.Enqueue(new CheckingVersionOperation(this));
                //TODO LOAD CONFIGS
                //operations.Enqueue(new LoadDeltaConfigsOperation(this));
                operations.Enqueue(new LoadGameBundlesOperation(this));
                operations.Enqueue(new ShowIntroOperation(this));
                operations.Enqueue(new LoadNativeFontOperation(this));
                BasePartOperation loadMainSceneOperation = new LoadMainSceneOperation(this)
                {
                    StartProgress = 0,
                    EndProgress = 90
                };
                operations.Enqueue(loadMainSceneOperation);
            }

            GoNextOperation();
        }

        public void GoNextOperation()
        {
            operation = operations.Count > 0 ? operations.Dequeue() : null;
            if (operation != null)
                operation.Execute();
            else
                Debug.LogError("Completed!");
        }

        #endregion

        #region PART OPERATIONS

        public class LoadNativeFontOperation : BasePartOperation
        {
            public LoadNativeFontOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                if (BundleManager.NativeFont == null)
                    BundleManager.NativeFont = (Font)Resources.Load("NotoSans-SemiBold", typeof(Font));
                handler.GoNextOperation();
            }

            public override void ExecuteIfConnection()
            {
            }
        }

        public class ShowIntroOperation : BasePartOperation
        {
            public ShowIntroOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("LOADING"));
                if (PlayerPrefs.GetInt("IntroSeen") == 0 && DefaultConfigurationProvider.GetConfigCData().ShowIntro)
                    handler.ShowIntro();
                else
                    handler.GoNextOperation();
            }

            public override void ExecuteIfConnection()
            {
            }
        }

        public class LoadMainSceneOperation : BasePartOperation
        {
            AsyncOperation asyncOperation = null;

            public LoadMainSceneOperation(MonoBehaviour progressView) : base(progressView)
            {
                // Changes Canvas Scaler Match balance between Width and Heigh to make it fit better on screen
                CanvasScalerAdjuster.BalanceWidthHeight(0.2f);
            }

            public override void Execute()
            {
                handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("LOADING"));
                handler.StopIntoAnimationCoroutine();
                asyncOperation = SceneManager.LoadSceneAsync("MainScene");
            }

            public override void OnUpdate()
            {
                if (asyncOperation != null)
                {
                    handler.SetProgressValue(LoadingGame.Normalize(StartProgress, EndProgress,
                        asyncOperation.progress));
                    if (asyncOperation.isDone)
                        LoadingScreenAnimationController.instance.SaveLoadingScreenProgress();
                }
            }

            public override void ExecuteIfConnection()
            {
            }
        }

        public class DownloadMaternitySceneOperation : BaseDownloadAssetBundleOperation
        {
            public DownloadMaternitySceneOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                if (ToDownload())
                    ProcessIfInternetConnection();
                else
                {
                    handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("LOADING"));
                    handler.GoNextOperation();
                }
            }

            protected override IEnumerator DoOperation()
            {
                yield return WaitForCachingReady();
                if (maternityBundle != null)
                    handler.GoNextOperation();
                else
                    yield return Download();
            }

            public override string GetAssetBundleName()
            {
                return MaternitySceneABName;
            }

            protected override int GetAssetBundleVersion()
            {
                return MaternitySceneABVersion;
            }

            protected override void OnSuccess(AssetBundle assetBundle)
            {
                maternityBundle = assetBundle;
                handler.GoNextOperation();
            }

            protected override bool CheckSmallAssetBundle()
            {
#if UNITY_ANDROID
                return false;
#else
                return true;
#endif
            }
        }

        public class DownloadMainSceneOperation : BaseDownloadAssetBundleOperation
        {
            public DownloadMainSceneOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            protected override IEnumerator DoOperation()
            {
                yield return WaitForCachingReady();
                if (LoadingGame.bundle != null)
                    handler.GoNextOperation();
                else
                    yield return Download();
            }

            public override string GetAssetBundleName()
            {
                return MainSceneABName;
            }

            protected override int GetAssetBundleVersion()
            {
                return MainSceneABVersion;
            }

            protected override void OnSuccess(AssetBundle assetBundle)
            {
                LoadingGame.bundle = assetBundle;
                handler.GoNextOperation();
            }

            protected override bool CheckSmallAssetBundle()
            {
#if UNITY_ANDROID
                return false;
#else
                return true;
#endif
            }
        }

        public class DownloadFontsOperation : BaseDownloadAssetBundleOperation
        {
            public DownloadFontsOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override string GetAssetBundleName()
            {
                return FontsABName;
            }

            protected override int GetAssetBundleVersion()
            {
                return FontsABVersion;
            }

            protected override void OnSuccess(AssetBundle assetBundle)
            {
                BundleManager.Instance.fontsBundle = assetBundle;
                AreFontsChecked = true;
                handler.AplyMainFontReferences();
                handler.AplyUserLanguage();
                handler.GoNextOperation();
            }

            protected override IEnumerator DoOperation()
            {
                yield return WaitForCachingReady();
                if (BundleManager.Instance.fontsBundle != null)
                {
                    AreFontsChecked = true;
                    handler.AplyUserLanguage();
                    handler.GoNextOperation();
                }
                else
                    yield return Download();
            }
        }

        public abstract class BaseDownloadAssetBundleOperation : BasePartOperation
        {
            protected UnityEngine.Networking.UnityWebRequest www = null;

            public BaseDownloadAssetBundleOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            Coroutine _doOperation;

            private void OnDisable()
            {
                try
                {
                    if (_doOperation != null)
                        context.StopCoroutine(_doOperation);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
            }

            public override bool ToDownload()
            {
                return !Caching.IsVersionCached(GetUrl(), new Hash128(0u, 0u, 0u, (uint)GetAssetBundleVersion()));
            }

            public override void Execute()
            {
                if (!ToDownload())
                    handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("LOADING"));

                ProcessIfInternetConnection();
            }

            public override void OnUpdate()
            {
                if (ToDownload() && www != null)
                {
                    handler.SetProgressValue(LoadingGame.Normalize(StartProgress, EndProgress, www.downloadProgress));
                    handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("DOWNLOADING") + "... " +
                                             (int)(LoadingGame.Normalize(StartProgressText, EndProgressText,
                                                 www.downloadProgress) * 100) + "%");
                }
            }

            public override void ExecuteIfConnection()
            {
                _doOperation = context.StartCoroutine(DoOperation());
            }

            protected IEnumerator WaitForCachingReady()
            {
                while (!Caching.ready)
                {
                    yield return null;
                }
            }

            private string GetUrl()
            {
                return handler.GetBundleURLDir(CheckSmallAssetBundle()) + AssetBundles.Utility.GetPlatformName() + "/" +
                       GetAssetBundleName() + "/" + GetAssetBundleVersion() + "/" + GetAssetBundleName();
            }

            protected IEnumerator Download()
            {
                string url = GetUrl();
                Debug.LogError("Starting loading/downloading " + url);
                using (www = UnityWebRequestAssetBundle.GetAssetBundle(url, (uint)GetAssetBundleVersion(), 0))
                {
                    www.SendWebRequest();
#if UNITY_ANDROID
                    while (!www.isDone && www.error == null)
                    {
                        yield return null;
                    }
#else
                    yield return www;
#endif
                    if (string.IsNullOrEmpty(www.error))
                        OnSuccess(DownloadHandlerAssetBundle.GetContent(www));
                    else
                    {
                        Debug.LogError("WWW " + GetAssetBundleName() + " download had an error: " + www.error);
                        www = null;
                        ProcessIfInternetConnection();
                    }
                }
            }

            protected abstract void OnSuccess(AssetBundle assetBundle);
            public abstract string GetAssetBundleName();
            protected abstract int GetAssetBundleVersion();
            protected abstract IEnumerator DoOperation();

            public float GetAssetBundleSize()
            {
                DefaultConfigurationProvider.GetConfigCData().AssetBundleSizes.TryGetValue(GetAssetBundleName(), out var valueToReturn);
                return valueToReturn;
            }

            protected virtual bool CheckSmallAssetBundle()
            {
                return false;
            }
        }

        public class LoadFontsFromResourcesOperation : BasePartOperation
        {
            public LoadFontsFromResourcesOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("LOADING"));
                LoadingGame.AreFontsChecked = true;
                handler.AplyUserLanguage();
                handler.GoNextOperation();
            }

            public override void ExecuteIfConnection()
            {
            }
        }

        public class LoadGameBundlesOperation : BasePartOperation
        {
            private List<GameAssetBundleInfo> gameAssetBundlesInfos = new List<GameAssetBundleInfo>();

            public LoadGameBundlesOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("CHECKING") + "...");
                handler.SetProgressValue(StartProgress);
                ProcessIfInternetConnection();
            }

            public override void ExecuteIfConnection()
            {
                context.StartCoroutine(LoadGameBundles());
            }

            IEnumerator LoadGameBundles()
            {
                gameAssetBundlesInfos.Clear();
                if (DefaultConfigurationProvider.GetConfigCData().GameAssetBundles != null)
                {
                    foreach (KeyValuePair<string, string> unparsedGameAssetBundleInfo in DefaultConfigurationProvider.GetConfigCData().GameAssetBundles)
                    {
                        try
                        {
                            if (unparsedGameAssetBundleInfo.Key != "hospitalSign")
                                gameAssetBundlesInfos.Add(new GameAssetBundleInfo(unparsedGameAssetBundleInfo.Key,
                                    unparsedGameAssetBundleInfo.Value));
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError(exception.Message);
                        }
                    }
                }

                foreach (GameAssetBundleInfo info in gameAssetBundlesInfos)
                {
                    BaseGameAssetBundle gameAssetBundle = ObjectFactory.GetGameAssetBundelObject(info.className);
                    if (gameAssetBundle == null)
                    {
                        Debug.LogError("Class " + info.className + " no found!");
                        continue;
                    }

                    gameAssetBundle.Initialize();
                    yield return context.StartCoroutine(gameAssetBundle.LoadContentFromResources());
                }

                handler.GoNextOperation();
            }
        }

        //public class LoadDeltaConfigsOperation : BasePartOperation
        //{
        //    public LoadDeltaConfigsOperation(MonoBehaviour progressView) : base(progressView)
        //    {
        //    }

        //    public override void Execute()
        //    {
        //        handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("CHECKING") + "...");
        //        handler.SetProgressValue(StartProgress);
        //        ProcessIfInternetConnection();
        //    }

        //    public override void ExecuteIfConnection()
        //    {
        //        Debug.LogError("2");
        //        DeltaDNAController.instance.RequestConfigs(() =>
        //            {
        //                Debug.Log("Delta configs loaded successfully");
        //                handler.GoNextOperation();
        //            },
        //            (ex) =>
        //            {
        //                Debug.LogError("Failed to load delta configs. Exception: " + ex);
        //                handler.GoNextOperation();
        //            });
        //    }
        //}

        public class CheckingVersionOperation : BasePartOperation
        {
            public CheckingVersionOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("CHECKING") + "...");
                handler.SetProgressValue(StartProgress);
                ProcessIfInternetConnection();
            }

//            private string GetVersionURL(out string versionURL)
//            {
//                //
//                //versionURL = RemoteSettingsManager.versionURL;
//                versionURL = DefaultConfigurationProvider.GetGeneralConfig().versionURL;
//                // Sanity check in case that Remote config doesn't work as expected
//                //if (string.IsNullOrEmpty(versionURL))
//                //    versionURL = "https://versionsupport.queserve.net/api/1/mh/";

//                // Environment: prod / qa / dev
//#if MH_RELEASE
//#if MH_QA
//                versionURL += "qa/";
//#else
//                versionURL += "prod/";
//#endif
//#else
//                versionURL += "dev/";
//#endif
//                // Platform: iOS / Android
//#if UNITY_ANDROID
//                versionURL += "android/";
//#elif UNITY_IOS
//                versionURL += "ios/";
//#endif
//                // Game Version
//                versionURL += UnityEngine.Application.version;

//                return versionURL;
//            }

//            class VersionData
//            {
//                public float TimeStamp;
//                public bool supported;
//            }

            public override void ExecuteIfConnection()
            {
                try
                {
                   // var config = await ConfigConnector.LoadAsync("version");
                    bool isInMaintenance = DefaultConfigurationProvider.GetConfigCData().IsInMaintenanceMode();

                    if (isInMaintenance)
                    {
                        BundleManager.Instance.StartCoroutine(handler.GetAlertPopup().Open(AlertType.MAINTENANCE, null,
                            null, () =>
                            {
                                AddRetryListenerExecution();
                                return;
                            }));
                        return;
                    }

                    //BundleManager.Instance.config = config;

                    //QUESERVE NOT USED

                    // Ask whether the game version is supported or needs to be updated
                    // POST option:
                    //      WWWForm form = new WWWForm();
                    //      form.AddField("version", Application.version.ToString());

//                    GetVersionURL(out string versionURL);

//                    Debug.LogFormat("<color=#FF7F33>versionURL: " + versionURL + "</color>");

//                    VersionData dataVer = null;
//                    bool connectionRequestError = false;
//                    //using (UnityWebRequest www = UnityWebRequest.Post(versionURL, form)) // POST version
//                    using (UnityWebRequest www = UnityWebRequest.Get(versionURL)) // GET version
//                    {
//                        www.downloadHandler = new DownloadHandlerBuffer();
//                        await www.SendWebRequest();
//                        Debug.Log("Result: " + www.result + " Error: " + www.error + " Response: " +
//                                  www.downloadHandler.text);
//                        if (www.result != UnityWebRequest.Result.Success)
//                        {
//                            Debug.LogError("Getting version error: " + www.error);
//                            if (www.result == UnityWebRequest.Result.ConnectionError)
//                                connectionRequestError = true;
//                        }
//                        else
//                        {
//                            dataVer = JsonUtility.FromJson<VersionData>(www.downloadHandler.text);
//                            Debug.LogFormat("<color=#FF7F33>Is Version</color> supported: " +
//                                           (dataVer.supported ? "<color=green>" : "<color=red>") + dataVer.supported +
//                                           "</color>");
//                        }
//                    }
//#if UNITY_EDITOR
//                    if (dataVer != null)
//                    {
//                        dataVer.supported = true;
//                        Debug.Log("Set supported");
//                    }
//#endif
//                    if (dataVer == null || dataVer.supported || connectionRequestError)
//                        handler.GoNextOperation();
//                    else
//                        ShowUpdateGameNativeDialog();
                    handler.GoNextOperation();
                }
                catch (Exception e)
                {
                    Debug.LogError("Getting version error: " + e + " Message: " + e.Message + " " + e.StackTrace + " " + e.Source + " " + e.InnerException + " " + e.Data + " " + e.TargetSite + " " + e.HelpLink + " " + e.HResult + " " + e.ToString());
                    BundleManager.Instance.StartCoroutine(handler.GetAlertPopup()
                        .Open(AlertType.SERVER_CONNECTION_PROBLEM, e.Message, e,
                            () => { AddRetryListenerExecution(); }));
                }
            }

            private void ShowUpdateGameNativeDialog()
            {
                BundleManager.Instance.StartCoroutine(handler.GetAlertPopup().Open(AlertType.NEW_VERSION, null, null,
                    () =>
                    {
                        handler.GetAlertPopup().button.onClick.AddListener(delegate
                        {
#if UNITY_IOS
                            Application.OpenURL("https://itunes.apple.com/us/app/id951697341");
#else
                            Application.OpenURL(
                                "https://play.google.com/store/apps/details?id=com.cherrypickgames.myhospital");
#endif
                        });
                    }));
            }
        }

        public class ShowStartPanelOperation : BasePartOperation
        {
            public ShowStartPanelOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                float bundleSize = 0.0f;
                Queue<BasePartOperation> operations = handler.GetOperations();
                foreach (BasePartOperation operation in operations)
                {
                    if (operation is BaseDownloadAssetBundleOperation)
                    {
                        BaseDownloadAssetBundleOperation downloadOperation =
                            operation as BaseDownloadAssetBundleOperation;
                        if (downloadOperation.ToDownload())
                            bundleSize += downloadOperation.GetAssetBundleSize();
                    }
                }

                if (DeveloperParametersController.Instance().parameters.ForceToShowStartingPanel || bundleSize > 0)
                {
                    handler.SetStartingPanelActive(true);
                    handler.SetPlayButtonAction(new UnityAction(delegate
                    {
                        handler.SetStartingPanelActive(false);
                        handler.GoNextOperation();
                    }));
                }
                else
                    handler.GoNextOperation();
            }

            public override void ExecuteIfConnection()
            {
                throw new NotImplementedException();
            }
        }

        public class CheckAssetBundleToDownloadSizeOperation : BasePartOperation
        {
            public CheckAssetBundleToDownloadSizeOperation(MonoBehaviour progressView) : base(progressView)
            {
            }

            public override void Execute()
            {
                float bundleSize = 0.0f;
                Queue<BasePartOperation> operations = handler.GetOperations();
                foreach (BasePartOperation operation in operations)
                {
                    if (operation is BaseDownloadAssetBundleOperation)
                    {
                        BaseDownloadAssetBundleOperation downloadOperation =
                            operation as BaseDownloadAssetBundleOperation;
                        if (downloadOperation.ToDownload())
                            bundleSize += downloadOperation.GetAssetBundleSize();
                    }
                }

                if (bundleSize == 0)
                    handler.GoNextOperation();
                else
                    NativeAlerts.ShowNativeAlert(
                        String.Format(I2.Loc.ScriptLocalization.Get("DOWNLOAD_PROMPT"), bundleSize),
                        I2.Loc.ScriptLocalization.Get("OK"), I2.Loc.ScriptLocalization.Get("INFO_TITLE"),
                        handler.GoNextOperation);
            }

            public override void ExecuteIfConnection()
            {
                throw new NotImplementedException();
            }
        }

        public abstract class BasePartOperation
        {
            private float startProgress = 0;

            public float StartProgress
            {
                get { return startProgress; }
                set
                {
                    startProgress = value;
                    StartProgressText = value;
                }
            }

            private float endProgress = 0;

            public float EndProgress
            {
                get { return endProgress; }
                set
                {
                    endProgress = value;
                    EndProgressText = value;
                }
            }

            public float StartProgressText = 0;
            public float EndProgressText = 0;

            protected IProgressable handler;
            protected MonoBehaviour context;

            public BasePartOperation(MonoBehaviour progressView)
            {
                this.context = progressView;
                this.handler = (IProgressable)progressView;
            }

            public virtual bool ToDownload()
            {
                return false;
            }

            public abstract void Execute();
            public abstract void ExecuteIfConnection();

            public virtual void OnUpdate()
            {
            }

            protected virtual void ProcessIfInternetConnection()
            {
                handler.ProcessIfInternetConnection(ExecuteIfConnection);
            }

            protected void AddRetryListenerExecution()
            {
                handler.GetAlertPopup().button.onClick.AddListener(delegate
                {
                    handler.GetAlertPopup().button.onClick.RemoveAllListeners();
                    handler.GetAlertPopup().Exit();
                    ProcessIfInternetConnection();
                });
            }
        }

        #endregion

        // Error handling //
        private bool IsCriticalErrorOccured()
        {
            return GlobalDataHolder.instance != null && GlobalDataHolder.instance.IsCriticalErrorOccured;
        }

        void Update()
        {
            if (operation != null)
                operation.OnUpdate();
        }

        public static float Normalize(float min, float max, float progress)
        {
            return (min + ((max - min) * progress)) / 100;
        }

        // Intro //

        #region INTRO

        public void StopIntoAnimationCoroutine()
        {
            if (animCorountineCheck != null)
            {
                try
                {
                    StopCoroutine(animCorountineCheck);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }

                animCorountineCheck = null;
            }
        }

        public void ShowIntro()
        {
            AnalyticsController.instance.ReportBug("loadinggame.intro");
            StartCoroutine(LoadIntro());
        }

        IEnumerator LoadIntro()
        {
            introResourceRequest = Resources.LoadAsync("IntroPanel", typeof(GameObject));
            yield return introResourceRequest;
            intro = (Instantiate(introResourceRequest.asset, canvasTransform) as GameObject);

            RectTransform introRT = intro.GetComponent<RectTransform>();
            introRT.localScale = Vector3.one;
            introRT.offsetMin = Vector2.zero;
            introRT.offsetMax = Vector2.zero;
#if UNITY_IOS
            //if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX)
            //{
            //   intro.GetComponent<Animator>().runtimeAnimatorController = iphonexIntroAnimator;
            //}
            if (SystemInfo.deviceModel == "iPhone10,3" || SystemInfo.deviceModel == "iPhone10,6" || SystemInfo.deviceModel == "iPhone11,2" || SystemInfo.deviceModel == "iPhone11,4" || SystemInfo.deviceModel == "iPhone11,6" || SystemInfo.deviceModel == "iPhone11,8")
            {
                intro.GetComponent<Animator>().runtimeAnimatorController = iphonexIntroAnimator;
            }
#elif UNITY_EDITOR
            if (isIphoneXDevToggle)
                intro.GetComponent<Animator>().runtimeAnimatorController = iphonexIntroAnimator;
#endif
            float ratio = Camera.main.aspect;
            if (ratio > 2f) // 18:10, iPhoneX, S9+ and other special model aspect ratios
                CanvasScalerAdjuster
                    .BalanceWidthHeight(0.6f); // Changes Canvas Scaler Match balance between Width and Heigh
            else if (ratio > 1.61f) // 16:9
                CanvasScalerAdjuster
                    .BalanceWidthHeight(0.95f); // Changes Canvas Scaler Match balance between Width and Heigh
            else if (ratio > 1.35f) // 16:10 
                CanvasScalerAdjuster
                    .BalanceWidthHeight(0.97f); // Changes Canvas Scaler Match balance between Width and Heigh
            else // 4:3 or similar
                CanvasScalerAdjuster
                    .BalanceWidthHeight(1.0f); // Changes Canvas Scaler Match balance between Width and Heigh

            intro.SetActive(true);
            animCorountineCheck = StartCoroutine(onComplete());
        }

        IEnumerator onComplete()
        {
            bool isAnyActive;
            while (true)
            {
                isAnyActive = false;

                if (!intro)
                {
                    CanvasScalerAdjuster
                        .BalanceWidthHeight(0.0f); // Changes Canvas Scaler Match balance between Width and Heigh
                    break;
                }

                foreach (Transform t in intro.transform)
                {
                    isAnyActive |= t.gameObject.activeSelf;
                }

                if (!isAnyActive)
                {
                    intro.GetComponent<Image>().enabled = false;
                    MarkIntoSaved();
                    break;
                }

                yield return null;
            }

            UnloadIntro();
            GoNextOperation();
        }

        void UnloadIntro()
        {
            if (intro)
                Destroy(intro);
            if (BundleManager.Instance.introBundle != null)
                BundleManager.Instance.introBundle.Unload(false);

            Resources.UnloadUnusedAssets();
        }

        public void IntroOver()
        {
            MarkIntoSaved();
            UnloadIntro();
        }

        private void MarkIntoSaved()
        {
            PlayerPrefs.SetInt("IntroSeen", 1);
        }

        #endregion

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        #region Utils

        private enum Platform
        {
            IOS,
            ANDROID
        }

        private Platform GetPlatform()
        {
#if UNITY_ANDROID
            return Platform.ANDROID;
#else
            return Platform.IOS;
#endif
        }

        public delegate void ConnectionState(bool isConnected);

        public void HasInternetConnection(ConnectionState connectionStateCallback)
        {
            engine.AddTask(() => { connectionStateCallback?.Invoke(AccountManager.HasInternetConnection()); });
        }

        public Callback callback;

        public void ProcessIfInternetConnection(Callback callback)
        {
            this.callback = callback;
            HasInternetConnection((isConnected) =>
            {
                if (isConnected)
                    callback?.Invoke();
                else
                {
                    if (AreFontsChecked)
                        AplyUserLanguage();

                    alertPopup.Open(AlertType.NO_INTERNET_CONNECTION);
                    alertPopup.button.RemoveAllOnClickListeners();
                    alertPopup.button.onClick.AddListener(delegate
                    {
                        alertPopup.OnFinishedAnimating -= AlertPopup_OnFinishedAnimating;
                        alertPopup.OnFinishedAnimating += AlertPopup_OnFinishedAnimating;
                        alertPopup.NoSmoothExit();
                    });
                }
            });
        }

        private void AlertPopup_OnFinishedAnimating()
        {
            if (callback != null)
                ProcessIfInternetConnection(callback);
        }

        #endregion

        private void ShowProgressBar()
        {
            TipText.gameObject.GetComponent<LoadingHintController>().ShowHints();
            ProgressBarContainer.SetActive(true);
        }

        // Langs //

        #region Langs

        private void AplyDefaultLanguage()
        {
            if (I2.Loc.LocalizationManager.CurrentLanguage != defaultLanguage)
                PlayerPrefs.SetString("userLanguage", I2.Loc.LocalizationManager.CurrentLanguage);

            LanguageManager.instance.ApplyLanguage(defaultLanguage);
        }

        public void AplyUserLanguage()
        {
            string userLang = PlayerPrefs.GetString("userLanguage");
            if (!string.IsNullOrEmpty(userLang))
                LanguageManager.instance.ApplyLanguage(userLang);
        }

        public void AplyMainFontReferences()
        {
            if (I2.Loc.LocalizationManager.Sources.Count > 0)
            {
                TMP_FontAsset fAssetNoStroke = I2.Loc.LocalizationManager.Sources[0].Assets[0] as TMP_FontAsset;
                fAssetNoStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][noStroke]Cyrylic") as TMP_FontAsset);
                fAssetNoStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][noStroke]Japanese") as TMP_FontAsset);
                fAssetNoStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][noStroke]Korean") as TMP_FontAsset);
                fAssetNoStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][noStroke]SC") as TMP_FontAsset);
                fAssetNoStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][noStroke]TC") as TMP_FontAsset);
                fAssetNoStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][noStroke]Thai") as TMP_FontAsset);
                TMP_FontAsset fAssetStroke = I2.Loc.LocalizationManager.Sources[0].Assets[1] as TMP_FontAsset;
                fAssetStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][stroke]Cyrylic") as TMP_FontAsset);
                fAssetStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][stroke]Japanese") as TMP_FontAsset);
                fAssetStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][stroke]Korean") as TMP_FontAsset);
                fAssetStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][stroke]SC") as TMP_FontAsset);
                fAssetStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][stroke]TC") as TMP_FontAsset);
                fAssetStroke.fallbackFontAssetTable.Add(
                    BundleManager.Instance.fontsBundle.LoadAsset("[new][stroke]Thai") as TMP_FontAsset);
            }
        }

        #endregion
    }
}


// Request Awaiter extension
public readonly struct UnityWebRequestAwaiter : System.Runtime.CompilerServices.INotifyCompletion
{
    private readonly UnityWebRequestAsyncOperation _asyncOperation;

    public bool IsCompleted => _asyncOperation.isDone;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOperation) => _asyncOperation = asyncOperation;

    public void OnCompleted(Action continuation) => _asyncOperation.completed += _ => continuation();

    public UnityWebRequest GetResult() => _asyncOperation.webRequest;
}

public static class ExtensionMethods
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}