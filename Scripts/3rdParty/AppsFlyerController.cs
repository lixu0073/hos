using AppsFlyerSDK;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
//using Unity.Notifications.iOS;
using NCrontab.Advanced.Extensions;

/// <summary>
/// AppsFlyer第三方归因平台控制器，负责移动应用归因分析和数据追踪。
/// 提供用户获取来源追踪、深度链接处理、应用内事件上报等功能。
/// </summary>
public class AppsFlyerController : MonoBehaviour, IAppsFlyerConversionData
{
    private string _userID;
    public static AppsFlyerController instance;
//#if !UNITY_WEBGL
    //public Firebase.FirebaseApp FirebaseInstance;
//#endif

#if UNITY_IOS
        private bool _iOSNotificationTokenSent;
#endif

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
#if !UNITY_IOS
        // On iOS it will be initialised after the user pass the AppTrackingTransparency question or where was asked for the first time
        //AppsFlyerSetCustomerUserId(DeltaDNA.DDNA.Instance.UserID);
        AppsFlyerSetCustomerUserId(AnalyticsController.GetUserID());
        InitAppsFlyer();
#endif
    }

    // Initializes AppsFlyer SDK
    public void InitAppsFlyer()
    {
        AppsFlyer.initSDK("gGBMyJQtDEcdkDVSzbYohJ", "951697341", this);
        AppsFlyer.startSDK();
#if UNITY_IOS && !UNITY_EDITOR && (MH_QA || MH_DEVELOP)
        AppsFlyeriOS.setUseReceiptValidationSandbox(true);
#endif
#if UNITY_IOS
        //UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
#endif

        Debug.LogFormat("<color=yellow> DDNA UserID: {0} </color><color=green> AF UserID: {1}</color>", AnalyticsController.GetUserID(), _userID);

        AppsFlyer.AFLog("AppsFlyer SDK initialised", AppsFlyer.getSdkVersion());
        Debug.LogFormat("<color=magenta>[APPSFLYER] SDK: {0}</color>", AppsFlyer.kAppsFlyerPluginVersion);
    }

    // Initializes Firebase SDK - Will be used to register the uninstalls
    public void InitFireBase()
    {
//#if !UNITY_WEBGL
//        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
//        {
//            var dependencyStatus = task.Result;
//            if (dependencyStatus == Firebase.DependencyStatus.Available)
//            {
//                FirebaseInstance = Firebase.FirebaseApp.DefaultInstance;
//                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnFirebaseTokenReceived;
//                UnityEngine.Debug.Log("Firebase initialized successfully");
//            }
//            else
//            {
//                UnityEngine.Debug.LogError(String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
//            }
//        });
//#endif
    }

//#if !UNITY_WEBGL
//    public void OnFirebaseTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
//    {
//#if UNITY_ANDROID
//        AppsFlyerAndroid.updateServerUninstallToken(token.Token);
//        Helpshift.HelpshiftSdk.getInstance().registerDeviceToken(token.Token);
//#endif
//    }
//#endif

    public void TrySetIOSNotificationToken()
    {
#if UNITY_IOS
        //if (!_iOSNotificationTokenSent)
        //{ 
        //    byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
        //    if (token != null)
        //    {
        //        AppsFlyeriOS.registerUninstall(token);
        //        _iOSNotificationTokenSent = true;
        //    }
        //}
#endif
    }
    //TODO 2022.3 way of getting unistall token
    //IEnumerator RequestUninstallToken()
    //{
    //    const AuthorizationOption AUTHORIZATION_OPTION = AuthorizationOption.Alert | AuthorizationOption.Badge;
    //    using (var req = new AuthorizationRequest(AUTHORIZATION_OPTION, true))
    //    {
    //        while (!req.IsFinished)
    //        {
    //            yield return null;
    //        }

    //        if (req.Granted && !req.DeviceToken.IsNullOrWhiteSpace())
    //        {
    //            AppsFlyeriOS.registerUninstall(Encoding.UTF8.GetBytes(req.DeviceToken));
    //        }
    //    }
    //}

    // To assign the Customer ID
    public void AppsFlyerSetCustomerUserId(string userId)
    {
        _userID = userId;
        AppsFlyer.setCustomerUserId(userId);
    }

    #region IAppsFlyerConversionData implementation
    // Used by IAppsFlyerConversionData interface to report conversion data to AppsFlyer
    public void onConversionDataSuccess(string conversionData)
    {
        Debug.Log("Appsflyer Deferred deeplink Data: " + conversionData);
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        foreach (string attrName in conversionDataDictionary.Keys)
        {
            Debug.Log("Deferred Deeplink attribute: " + attrName + " = " + conversionDataDictionary[attrName]);
        }
        if (conversionDataDictionary.ContainsKey("af_status") && (string)conversionDataDictionary["af_status"] == "Non - organic")
        {
            if (conversionDataDictionary.ContainsKey("is_first_launch") && (bool)conversionDataDictionary["is_first_launch"])
            {
                Debug.Log("Conversion: First Launch");
                if (conversionDataDictionary.ContainsKey("af_sub1"))
                {
                    Debug.Log("onConversionDataSuccess: Deferred Deep linking into " + conversionDataDictionary["af_sub1"]);
                    //AppsFlyerController.SetDeepLinkedItemId((string)conversionDataDictionary["af_sub1"]);
                }
                else
                {
                    Debug.LogWarning("onConversionDataSuccess: Does not contain parameter af_sub1");
                }
            }
            else
            {
                Debug.Log("Conversion: Not First Launch");
            }
        }
        else
        {
            Debug.Log("Conversion: This is an organic install.");
        }
    }

    // For IAppsFlyerConversionData - DataFail
    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    // For IAppsFlyerConversionData - Open Attribution
    public void onAppOpenAttribution(string attributionData)
    {
        Debug.Log("Appsflyer deeplink Data: " + attributionData);
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        if (!attributionDataDictionary.ContainsKey("is_first_launch"))
            Debug.Log("onAppOpenAttribution: This is NOT deferred deep linking");
        foreach (string attrName in attributionDataDictionary.Keys)
        {
            Debug.Log("Deeplink attribute: " + attrName + " = " + attributionDataDictionary[attrName]);
        }
        if (attributionDataDictionary.ContainsKey("af_sub1"))
        {
            Debug.Log("onAppOpenAttribution: Deep linking into " + attributionDataDictionary["af_sub1"]);
            //AppsFlyerController.SetDeepLinkedItemId((string)attributionDataDictionary["af_sub1"]);
        }
        else
        {
            Debug.LogWarning("onAppOpenAttribution: Does not contain parameter af_sub1");
        }
    }

    /// For IAppsFlyerConversionData - Open attribution failed
    public void onAppOpenAttributionFailure(string error)
    {
        Debug.LogError("Appsflyer fail: " + error);
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }
    #endregion

    
    public void ReportTransactionIAP(IAPResult result, UnityEngine.Purchasing.Product product)
    {
        Dictionary<string, object> par = new Dictionary<string, object>();

        switch (result)
        {
            case IAPResult.FRAUD:
            case IAPResult.CANCEL:
                par.Add("Status", result.ToString());
                par.Add("localName", product.definition.id);
                break;
            case IAPResult.PURCHASE:
            default:
                par.Add("transID", product.transactionID);
                par.Add("Status", result.ToString());
                par.Add("localName", product.metadata.localizedTitle);
                par.Add("localPrize", product.metadata.localizedPrice.ToString());
                break;
        }

        ReportEvent(EconomySource.IAP.ToString(), par);
    }

    #region Send Events
    public void ReportEvent(string eventName)
    {
        string afEventName = "af_" + eventName;
        AppsFlyer.sendEvent(afEventName, null);
        AppsFlyer.AFLog("Event sent: ", afEventName);
    }

    public void ReportEvent(string eventName, Dictionary<string, object> par)
    {
        string afEventName = "af_" + eventName;
        Dictionary<string, string> dict = new Dictionary<string, string>();
        bool isValidValue = false;
        foreach (KeyValuePair<string, object> pair in par)
        {
            isValidValue = par.TryGetValue(pair.Key, out object val);
            dict.Add(pair.Key, isValidValue ? val.ToString() : "");
        }
        AppsFlyer.sendEvent(afEventName, dict);
        AppsFlyer.AFLog("Event sent: ", afEventName);
    }
    #endregion
}
