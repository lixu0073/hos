using UnityEngine;
using Unity.Services.RemoteConfig;
using System.Collections;
using System;

//public class RemoteSettingsManager : MonoBehaviour
//{
//    #region Remote Config Params
//    public static string versionURL = "";
//    public static string supportEmail = "";
//    public static string TermsOfServiceURL = "";
//    public static string PrivacyPolicyURL = "";
//    public static string FacebookFanpageURL = "";
//    public static string InstagramURL = "";
//    public static string YouTubeURL = "";
//    private float TimeOut;
//    #endregion

//    public struct userAttributes { /* Optionally declare variables for any custom user attributes; if none keep an empty struct */ }
//    public struct appAttributes { /* Optionally declare variables for any custom app attributes; if none keep an empty struct */ }

//    private bool IsReady = false;    

//    public IEnumerator Init()
//	{
//        RemoteConfigService.Instance.SetEnvironmentID("079c87c5-570c-48ec-81ea-af93440d9d64");
//        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
//        RemoteConfigService.Instance.FetchConfigs<userAttributes, appAttributes>(new userAttributes() { }, new appAttributes() { });

//        float startingTime = Time.time;
//        while (!IsReady && (Time.time - startingTime) < TimeOut)
//            yield return null;
//    }

//    public static T GetJSONSetting<T>(string key)
//    {
//        try
//        {
//            var json = RemoteConfigService.Instance.appConfig.GetJson(key,"");
//            if (!string.IsNullOrEmpty(json))
//            {
//                var jsonAsObject = JsonUtility.FromJson<T>(json);
//                return jsonAsObject;
//            }
//        }
//        catch(Exception ex)
//        {
//            Debug.LogError("Failed to get " + key + " from Remote Config as " + typeof(T) + " Reason:" + ex);
//        }
//        return default(T);
//    }

//    public void UpdateSettings()
//    {
//        versionURL = RemoteConfigService.Instance.appConfig.GetString("BaseVersionCheckPath");
//        supportEmail = RemoteConfigService.Instance.appConfig.GetString("SupportEmail");
//        TermsOfServiceURL = RemoteConfigService.Instance.appConfig.GetString("TermsOfServiceURL");
//        PrivacyPolicyURL = RemoteConfigService.Instance.appConfig.GetString("PrivacyPolicyURL");
//        FacebookFanpageURL = RemoteConfigService.Instance.appConfig.GetString("FacebookFanpageURL");
//        InstagramURL = RemoteConfigService.Instance.appConfig.GetString("InstagramURL");
//        YouTubeURL = RemoteConfigService.Instance.appConfig.GetString("YouTubeURL");
//        TimeOut = RemoteConfigService.Instance.appConfig.GetFloat("RemoteConfigConnectionTimeOut");
//    }

//    public void ApplyRemoteSettings(ConfigResponse configResponse)
//    {
//        switch (configResponse.requestOrigin)
//        {
//            case ConfigOrigin.Default:
//                Debug.Log("No settings loaded this session; using default values.");
//                break;
//            case ConfigOrigin.Cached:
//                Debug.Log("No settings loaded this session; using cached values from a previous session.");
//                break;
//            case ConfigOrigin.Remote:
//                Debug.Log("New settings loaded this session; update values accordingly.");
//                UpdateSettings();
//                break;
//        }
//        IsReady = true;
//    }

//}
