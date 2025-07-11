using UnityEngine;
using Helpshift;
using Hospital;
using System;
//using HelpshiftExample;
using UnityEditor;
using System.Collections.Generic;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
#if !UNITY_EDITOR
using System.Collections.Generic;
#endif

public class HelpShiftManager : MonoBehaviour
{
    const string API_KEY = "1aa0c89a46ff36177a26a22d581f8e68";
    const string domainName = "kuuhubb.helpshift.com";
#if UNITY_ANDROID
    const string AppId = "kuuhubb_platform_20180227155659575-a23bd71e47772f8";
#elif UNITY_IOS
    const string AppId = "kuuhubb_platform_20180227072731732-f95ba42e88e7941";
#endif

    private HelpshiftSdk _help;
    public HSEventsListener HSEventsListener = new HSEventsListener();

    public static HelpShiftManager Instance = null;

    private void Awake()
    {
        Instance = this;
        Init();
    }

    /// <summary>
    /// Initializes HelpShift SDK with given settings
    /// </summary>
    public void Init()
    {
        Debug.LogFormat("<color=magenta>[HELPSHIFT] SDK: {0}</color>", HelpshiftSdk.PLUGIN_VERSION);
#if !UNITY_EDITOR
        _help = HelpshiftSdk.GetInstance();
        var installDictionary = new Dictionary<string, object>(); 
        installDictionary.Add(HelpshiftSdk.ENABLE_INAPP_NOTIFICATION, true); // values true/false default true
        installDictionary.Add(HelpshiftSdk.ENABLE_LOGGING, false); // Possible options:  true/false default false            
#if UNITY_ANDROID
        installDictionary.Add(HelpshiftSdk.SCREEN_ORIENTATION, -1); // Possible options:  SCREEN_ORIENTATION_LANDSCAPE=0, SCREEN_ORIENTATION_PORTRAIT=1, SCREEN_ORIENTATION_UNSPECIFIED = -1
        installDictionary.Add(HelpshiftSdk.NOTIFICATION_LARGE_ICON, "iconbig");
        installDictionary.Add(HelpshiftSdk.NOTIFICATION_ICON, "iconsmall");
        _help.Install(AppId, domainName, installDictionary);
#elif UNITY_IOS
        _help.Install(AppId, domainName, installDictionary);
#endif
        _help.SetHelpshiftEventsListener(HSEventsListener);
        HSEventsListener.HelpShiftSdkInstance = _help;

        //Possible values are "userId", ""userEmail" , "userName", "userAuthToken"
        Dictionary<string, string> userDetails = new Dictionary<string, string>
                                        {
                                            { "userId", CognitoEntry.UserID/*DeltaDNA.DDNA.Instance.UserID*/ }
                                        };
        _help.Login(userDetails);
#endif
    }

    private string GetSpenderTag(int IAPsCount)
    {
        if (IAPsCount > 0 && IAPsCount <= 2)
            return "SPENDER";
        if (IAPsCount > 2 && IAPsCount <= 6)
            return "FREQUENT SPENDER";
        if (IAPsCount > 6)
            return "SUPER SPENDER";
        return null;
    }

    /// <summary>
    /// Builds configuration for Helpshift SDK calls
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, object> GetApiConfig()
    {
        Dictionary<string, object> configDictionary = new Dictionary<string, object>
        {            
            { "enableContactUs", "ALWAYS" }, // Possible values are "ALWAYS" / "AFTER_VIEWING_FAQS" / "AFTER_MARKING_ANSWER_UNHELPFUL" / "NEVER".
            { "fullPrivacy", false } // Possible options:  true, false
        };

        Dictionary<string, object> configMap = new Dictionary<string, object>
        { };
        configMap.Add("User Cognito", CognitoEntry.UserID);
        configMap.Add("Save ID", CognitoEntry.SaveID);
        configMap.Add("Application version", Application.version);
        configMap.Add("Level", Game.Instance.gameState().GetHospitalLevel().ToString());
        configMap.Add("Coin Count", Game.Instance.gameState().GetCoinAmount().ToString());
        configMap.Add("Hearts Count", Game.Instance.gameState().GetDiamondAmount().ToString());
        configMap.Add("Positive Energy Count", Game.Instance.gameState().PositiveEnergyAmount.ToString());
        configMap.Add("Facebook ID", AccountManager.Instance.IsFacebookConnected ? AccountManager.FacebookID : "NULL");
        configMap.Add("GameCenter ID", AccountManager.Instance.IsGameCenterConnected ? Social.localUser.id : "NULL");
        configMap.Add("SupportCode", LoadingGame.GenerateSupportCode(CognitoEntry.UserID));
        int IAPsCount = Game.Instance.gameState().GetIAPEasterCount() + Game.Instance.gameState().GetIAPValentineCount() + Game.Instance.gameState().GetIAPPurchasesCount();
        configMap.Add("IAPs Count", IAPsCount);
        configMap.Add("Coins from IAP", Game.Instance.gameState().CoinAmountFromIAP);
        configMap.Add("Hearts from IAP", Game.Instance.gameState().DiamondAmountFromIAP);
        configMap.Add("Last Successful Purchase", string.IsNullOrEmpty(Game.Instance.gameState().GetLastSuccessfulPurchase()) ? "NULL" : Game.Instance.gameState().GetLastSuccessfulPurchase());
        configMap.Add("FirstSession", string.IsNullOrEmpty(CacheManager.GetFirstLaunchDate()) ? "NULL" : CacheManager.GetFirstLaunchDate());
        configMap.Add("MemorySize", SystemInfo.systemMemorySize);
        if (IAPsCount > 0)
            configMap.Add("IAPs", new string[] { GetSpenderTag(IAPsCount) });            

        /*List<string> tags = new List<string>();
        if (GameManager.instance.PlayerManager.GetMatch3Level() < 30)
        {
            tags.Add("early_player");
        }
        else if (GameManager.instance.PlayerManager.GetCurrencyItemAmount(CurrencyItemType.Keys) > 25)
        {
            tags.Add("match3_player");
        }
        else
        {
            tags.Add("story_player");
        }

        var spent = GameManager.instance.PlayerManager.GetTotalCurrencySpent();
        if (spent < 1)
        {
            tags.Add("free_player");
        }        

        configMap.Add("hs-tags", tags.ToArray());*/
        configDictionary.Add("customMetadata", configMap);
        return configDictionary;
    }

    /// <summary>
    /// Sets places where player has been so we can target Helpshift articles with them if needed
    /// </summary>
    /// <param name="crumbName"></param>
    public void LeaveBreadCrumb(string crumbName)
    {
#if UNITY_EDITOR
        Debug.LogWarning("HS: LeaveBreadCrumb Not available in editor mode");
        return;
#else
        _help.LeaveBreadcrumb(crumbName);
#endif
    }

    /// <summary>
    /// Opens Helpshift FAQ
    /// </summary>
    public void ShowFAQs()
    {
#if UNITY_EDITOR
        Debug.LogWarning("HS: ShowFAQs Not available in editor mode");
        return;
#else
        _help.ShowFAQs(GetApiConfig());
#endif
    }

    /// <summary>
    /// Opens Helpshift FAQ to selected section
    /// </summary>
    public void ShowFAQSection(string sectionId)
    {
#if UNITY_EDITOR
        Debug.LogWarning("HS: ShowFAQSection Not available in editor mode");
        return;
#else
        _help.ShowFAQSection(sectionId, GetApiConfig());
#endif
    }

    /// <summary>
    /// Opens Helpshift FAQ to selected FAQ question
    /// </summary>
    public void ShowSingleFAQ(string FAQId)
    {
#if UNITY_EDITOR
        Debug.LogWarning("HS: ShowSingleFAQ Not available in editor mode");
        return;
#else
        _help.ShowSingleFAQ(FAQId, GetApiConfig());
#endif
    }

    /// <summary>
    /// Opens Helpshift Support chat and passes the provided error to Helpshift
    /// </summary>
    public void ShowConversation(string error)
    {
#if UNITY_EDITOR
        Debug.LogWarning("HS: ShowConversation Not available in editor mode");
        return;
#else
        var config = GetApiConfig();
        var tags = (Dictionary<string, object>)config["customMetadata"];
        tags.Add("ClientError", error);
        _help.ShowConversation(config);
#endif
    }

    /// <summary>
    /// Opens Helpshift Support chat
    /// </summary>
    public void ShowConversation()
    {
#if UNITY_EDITOR
        Debug.LogWarning("HS: ShowConversation Not available in editor mode");
        return;
#else
        _help.ShowConversation(GetApiConfig());
#endif
    }

    /// <summary>
    /// Triggered on Helpshift popup close
    /// </summary>
    public void helpshiftSupportSessionClosed()
    {
        Debug.Log("helpshift support session closed");
        RequestUnreadMessageCount();
    }

    /// <summary>
    /// Triggered on Helpshift chat end
    /// </summary>
    public void conversationEnded()
    {
        Debug.Log("helpshift support conversation ended");
        RequestUnreadMessageCount();
    }

    /// <summary>
    /// Fetches the unread messages count from local or remote based on shouldFetchFromServer flag.
    /// The result of unread count will be passed over the IHelpshiftEventsListener#HandleHelpshiftEvent method
    /// </summary>
    /// <param name="online">Should fetch unread message count from server.</param>
    public void RequestUnreadMessageCount(bool online = true)
    {
#if UNITY_EDITOR
        return;
#else
        _help.RequestUnreadMessageCount(online);
#endif
    }

} // End HelpShiftManager


public class HSEventsListener : IHelpshiftEventsListener
{
    public event Action<string> OnNotificationAmountChanged;
    public string PreviousNotificationCount = "0";
    public HelpshiftSdk HelpShiftSdkInstance;

    public void AuthenticationFailedForUser(HelpshiftAuthenticationFailureReason reason)
    {
        //Authentication failure. Currently we do not use authentication.
    }

    public void HandleHelpshiftEvent(string eventName, Dictionary<string, object> eventData)
    {
        if (eventName.Equals(HelpshiftEvent.RECEIVED_UNREAD_MESSAGE_COUNT))
        {
            if (eventData.ContainsKey(HelpshiftEvent.DATA_MESSAGE_COUNT))
            {
                OnNotificationAmountChanged?.Invoke(eventData[HelpshiftEvent.DATA_MESSAGE_COUNT].ToString());
                PreviousNotificationCount = eventData[HelpshiftEvent.DATA_MESSAGE_COUNT].ToString();
            }
        }
        if (eventName.Equals(HelpshiftEvent.SDK_SESSION_STARTED))
        {
            //Session started
            HelpShiftSdkInstance.RequestUnreadMessageCount(true);
        }
        if (eventName.Equals(HelpshiftEvent.SDK_SESSION_ENDED))
        {
            HelpShiftSdkInstance.RequestUnreadMessageCount(true);
        }
        if (eventName.Equals(HelpshiftEvent.CONVERSATION_START))
        {
            if (eventData.ContainsKey(HelpshiftEvent.DATA_MESSAGE))
            {
                Debug.Log("Conversation started with text: " + eventData[HelpshiftEvent.DATA_MESSAGE]);
            }

            if (eventName.Equals(HelpshiftEvent.CONVERSATION_REOPENED))
            {
                Debug.Log("Conversation Reopened");
            }
            if (eventName.Equals(HelpshiftEvent.CONVERSATION_END))
            {
                Debug.Log("Conversation Ended");
                HelpShiftSdkInstance.RequestUnreadMessageCount(true);
            }
        }
    }
}

/*
    //******************************************* OLD VERSION *************************************************
    void Start()
    {
        Instance = this;
        Debug.LogFormat("<color=magenta>[HELPSHIFT] SDK: {0}</color>", HelpshiftConfig.pluginVersion);
        Debug.Log("HelpShiftManager::Start - Instance: " + Instance + " help: " + help);
#if !UNITY_EDITOR 
        if (help != null)
            help.install(API_KEY, domainName, AppId);
        else
        {
            _help = HelpshiftSdk.getInstance();
            Debug.LogError("HelpShiftManager::Start - help == null ==> " + _help);
            help.install(API_KEY, domainName, AppId);
            Debug.LogError("HelpShiftManager::Start - help installed!");
        }
#endif
    }

    private string GetSpenderTag(int IAPsCount)
    {
        if (IAPsCount > 0 && IAPsCount <= 2)
            return "SPENDER";
        if (IAPsCount > 2 && IAPsCount <= 6)
            return "FREQUENT SPENDER";
        if (IAPsCount > 6)
            return "SUPER SPENDER";
        return null;
    }

    public void UpdateMetadata()
    {
#if !UNITY_EDITOR
        Dictionary<string, object> configMap = new Dictionary<string, object>();
        configMap.Add("User Cognito", CognitoEntry.UserID);
        configMap.Add("Save ID", CognitoEntry.SaveID);
        configMap.Add("Application version", Application.version);
        configMap.Add("Level", Game.Instance.gameState().GetHospitalLevel().ToString());
        configMap.Add("Coin Count", Game.Instance.gameState().GetCoinAmount().ToString());
        configMap.Add("Hearts Count", Game.Instance.gameState().GetDiamondAmount().ToString());
        configMap.Add("Positive Energy Count", Game.Instance.gameState().PositiveEnergyAmount.ToString());
        configMap.Add("Facebook ID", AccountManager.Instance.IsFacebookConnected ? AccountManager.FacebookID : "NULL");
        configMap.Add("GameCenter ID", AccountManager.Instance.IsGameCenterConnected ? Social.localUser.id : "NULL");
        configMap.Add("SupportCode", LoadingGame.GenerateSupportCode(CognitoEntry.UserID));
        int IAPsCount = Game.Instance.gameState().GetIAPEasterCount() + Game.Instance.gameState().GetIAPValentineCount() + Game.Instance.gameState().GetIAPPurchasesCount();
        configMap.Add("IAPs Count", IAPsCount);
        configMap.Add("Coins from IAP", Game.Instance.gameState().CoinAmountFromIAP);
        configMap.Add("Hearts from IAP", Game.Instance.gameState().DiamondAmountFromIAP);
        configMap.Add("Last Successful Purchase", string.IsNullOrEmpty(Game.Instance.gameState().GetLastSuccessfulPurchase()) ? "NULL" : Game.Instance.gameState().GetLastSuccessfulPurchase());
        configMap.Add("FirstSession", string.IsNullOrEmpty(CacheManager.GetFirstLaunchDate()) ? "NULL" : CacheManager.GetFirstLaunchDate());
        configMap.Add("MemorySize", SystemInfo.systemMemorySize);
        if (IAPsCount > 0)
            configMap.Add(HelpshiftSdk.HSTAGSKEY, new string[] { GetSpenderTag(IAPsCount) });

        if (help != null)
            help.updateMetaData(configMap);
        else
            Debug.LogWarning("HelpShift not inited");
#endif
    }

    public void ShowFAQ()
    {
#if UNITY_EDITOR
        Debug.Log("<color=yellow>Helpshift not available in Editor mode</color>");
#else 
        //help.setUserIdentifier(CognitoEntry.UserID);
        HelpshiftUser userID = new HelpshiftUser.Builder(CognitoEntry.UserID, null).build();
        Debug.LogError("ShowFAQ for userID: " + userID.name + " || " + userID.identifier);
        Debug.LogError("HELP: " + help.ToString());
        help.login(userID);

#if UNITY_IPHONE
        help.showFAQs();
#elif UNITY_ANDROID
        help.showFAQs();
        //if (UniAndroidPermission.IsPermitted(AndroidPermission.WRITE_EXTERNAL_STORAGE))
        //{
        //    Debug.Log("is permited for HelpShift");
        //    help.showFAQs();    
        //}
        //else
        //{
        //    Debug.Log("isn't permited for HelpShift");
        //    UniAndroidPermission.RequestPermission(AndroidPermission.WRITE_EXTERNAL_STORAGE, 
        //        () => { help.showFAQs();
        //            Debug.Log("is permited for HelpShift");
        //        },
        //        () => { OnAndroidDenyPermission(); },
        //        () => { OnAndroidDenyAndNeverAskPermission(); });
        //}
#else
        help.showFAQs();
#endif
#endif
    }

    public static void OnAndroidDenyPermission()
    {
        //   if (UniAndroidPermission.IsPermitted(AndroidPermission.WRITE_EXTERNAL_STORAGE) == false)
        // {
        //   string popupTitleText = ScriptLocalization.Get(PERMISION_REQUIRED_TITLE_TERM);
        //   string popupBodyText = ScriptLocalization.Get(PERMISION_REQUIRED_BODY_TERM);
        //    string popupSettingsButtonText = ScriptLocalization.Get(GO_TO_SETTINGS_TERM);
        //    string popupCancelButtonText = ScriptLocalization.Get(CANCEL_TERM);
        //    string popupWebText = ScriptLocalization.Get(WEB_TERM);
        //
        //  MobileNativePopups.OpenAlertDialog(
        // popupTitleText, popupBodyText, popupSettingsButtonText, popupCancelButtonText, popupWebText,
        // () => { GoToSettings(); },
        //() => {/*basically no operation here * / },
        // () => { Application.OpenURL("https://kuuhubb.helpshift.com/a/my-hospital/?p=web"); }
        // );
        //}
    }

    public static void OnAndroidDenyAndNeverAskPermission()
    {
#if UNITY_ANDROID
        if(!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            string popupBodyText = "test tresć";//I2.Loc.ScriptLocalization.Get(PERMISION_REQUIRED_BODY_TERM); 
            string popupSettingsButtonText = "settings";//I2.Loc.ScriptLocalization.Get(GO_TO_SETTINGS_TERM);
            string popupWebText = "web";// I2.Loc.ScriptLocalization.Get(WEB_TERM);

            NativeAlerts.ShowNativeAlert(popupBodyText, popupSettingsButtonText, popupWebText,
                () => { GoToSettings(); },
                () => { Application.OpenURL("https://kuuhubb.helpshift.com/a/my-hospital/?p=web"); });
        }
#endif
    }

    public static void GoToSettings()
    {
        try
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string packageName = currentActivityObject.Call<string>("getPackageName");

                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Application.OpenURL("https://kuuhubb.helpshift.com/a/my-hospital/?p=web");
        }
    }

}
*/