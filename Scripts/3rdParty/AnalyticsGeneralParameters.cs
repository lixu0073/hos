using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Services.Analytics;

/// <summary>
/// 分析通用参数管理器，负责管理和维护游戏分析所需的通用参数。
/// 包括用户货币、等级、社交连接状态等核心游戏数据的收集和上报。
/// </summary>
public class AnalyticsGeneralParameters : MonoBehaviour {

    public static int softCurrency = -1;
    public static int hardCurrency = -1;
    public static int positiveEnergy = -1;
    public static int userLevel = -1;
    public static int userXP = -1;
    public static int lastSaveDate = -1;
    public static string hospitalName = "NotSet";
    public static string cognitoId = "NotSet";
    public static string gamecenterId = "NotSet";
    public static string facebookId = "NotSet";
    public static string supportCode = "NotSet";
    public static string facebookEmail = "NotSet";
    public static bool facebookConnected = false;
    public static int maternityUserLevel = -1;
    public static int maternityUserXP = -1;

    void Awake()
    {
        LoadGeneralParameters();
    }

    public void LoadGeneralParameters()
    {
        softCurrency = ObscuredPrefs.GetInt("softCurrency");
        hardCurrency = ObscuredPrefs.GetInt("hardCurrency");
        positiveEnergy = ObscuredPrefs.GetInt("positiveEnergy");

        userLevel = ObscuredPrefs.GetInt("userLevel");
        userXP = ObscuredPrefs.GetInt("userXP");
        maternityUserLevel = ObscuredPrefs.GetInt("maternityUserLevel", -1);
        maternityUserXP = ObscuredPrefs.GetInt("maternityUserXP", -1);
        lastSaveDate = ObscuredPrefs.GetInt("lastSaveDate");

        if (!string.IsNullOrEmpty(ObscuredPrefs.GetString("hospitalName")))
            hospitalName = ObscuredPrefs.GetString("hospitalName");
        if (!string.IsNullOrEmpty(ObscuredPrefs.GetString("cognitoId")))
            cognitoId = ObscuredPrefs.GetString("cognitoId");
        if (!string.IsNullOrEmpty(ObscuredPrefs.GetString("gamecenterId")))
            gamecenterId = ObscuredPrefs.GetString("gamecenterId");
        if (!string.IsNullOrEmpty(ObscuredPrefs.GetString("supportCode")))
            supportCode = ObscuredPrefs.GetString("supportCode");
        if (!string.IsNullOrEmpty(ObscuredPrefs.GetString("facebookId")))
            facebookId = ObscuredPrefs.GetString("facebookId");
        if (!string.IsNullOrEmpty(ObscuredPrefs.GetString("facebookEmail")))
            facebookEmail = ObscuredPrefs.GetString("facebookEmail");

        facebookConnected = ObscuredPrefs.GetBool("facebookConnected");
    }

    public void SaveGeneralParameters()
    {
        ObscuredPrefs.SetInt("softCurrency", softCurrency);
        ObscuredPrefs.SetInt("hardCurrency", hardCurrency);

        ObscuredPrefs.SetInt("positiveEnergy", positiveEnergy);

        ObscuredPrefs.SetInt("userLevel", userLevel);
        ObscuredPrefs.SetInt("userXP", userXP);
        ObscuredPrefs.SetInt("maternityUserLevel", maternityUserLevel);
        ObscuredPrefs.SetInt("maternityUserXP", maternityUserXP);
        ObscuredPrefs.SetBool("facebookConnected", facebookConnected);
        ObscuredPrefs.SetInt("lastSaveDate", lastSaveDate);
        if(hospitalName.Length > 0)
            ObscuredPrefs.SetString("hospitalName", hospitalName);
        if (cognitoId.Length > 0)
            ObscuredPrefs.SetString("cognitoId", cognitoId);
        if (gamecenterId.Length > 0)
            ObscuredPrefs.SetString("gamecenterId", gamecenterId);
        if (supportCode.Length > 0)
            ObscuredPrefs.SetString("supportCode", supportCode);
        if (facebookId.Length > 0)
            ObscuredPrefs.SetString("facebookId", facebookId);
        if (facebookEmail.Length > 0)
            ObscuredPrefs.SetString("facebookEmail", facebookEmail);

        ObscuredPrefs.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveGeneralParameters();
    }

    static bool emailSet = false;
    public static void UpdateFacebookEmailAttribute()
    {
        if (emailSet)   //already updated email this session, we only want to do this once per session so to not request facebook server all the time
            return;

        try
        {
            string query = "/me?fields=email";
            Facebook.Unity.FB.API(query, Facebook.Unity.HttpMethod.GET, (result) =>
            {
                if (result.Error == null)
                {
                    string email = result.ResultDictionary["email"].ToString();
                    facebookEmail = email;
                    emailSet = true;
                }
                else
                {
                }
            });
        }
        catch
        {
        }
    }
    
    public static Dictionary<string, object> GetGeneralParams(bool withCheaterScore = true)
    {
        Dictionary<string, object> paramiters = new Dictionary<string, object>();
        if (string.IsNullOrEmpty(hospitalName))
            hospitalName = "NotSet";

        if (string.IsNullOrEmpty(supportCode))
#if UNITY_EDITOR
            supportCode = "UnityEditor";
#else
            supportCode = "NotSet";
#endif

        paramiters.Add("softCurrency", softCurrency);
        paramiters.Add("hardCurrency", hardCurrency);

        paramiters.Add("positiveEnergy", positiveEnergy);

        paramiters.Add("userLevel", userLevel);
        paramiters.Add("userXP", userXP);
        paramiters.Add("maternityUserLevel", maternityUserLevel);
        paramiters.Add("maternityUserXP", maternityUserXP);
        paramiters.Add("hospitalName", hospitalName);
        paramiters.Add("cognitoId", cognitoId);
        paramiters.Add("gamecenterId", gamecenterId);
        paramiters.Add("supportCode", supportCode);
        paramiters.Add("facebookConnected", facebookConnected);
        paramiters.Add("facebookId", facebookId);
        paramiters.Add("facebookEmail", facebookEmail);
        paramiters.Add("clientVersion", Application.version);
        paramiters.Add("lastSaveDate", lastSaveDate);
        paramiters.Add("ram", SystemInfo.systemMemorySize);

        try
        {
            paramiters.Add("sceneName", GetSceneName());
        }
        catch (Exception e)
        {
            paramiters.Add("sceneName", "None");
            Debug.LogError(e.Message);
        }

        if (withCheaterScore)
        {
            try
            {
                if (GetSceneName() == "MainScene")
                {
                    paramiters.Add("cheaterScore", CheaterCalculator.GetScore());
                }
                else
                {
                    paramiters.Add("cheaterScore", -1);
                }
            }
            catch (Exception e)
            {
                paramiters.Add("cheaterScore", -1);
                Debug.LogError(e.Message);
            }
        }
        paramiters.Add("platformUnity", Application.platform);       //Android, IPhonePlayer, etc

        return paramiters;
    }

    public static void AddGeneralParameters(Dictionary<string, object> parameters)
    {
        Dictionary<string, object> _parms = GetGeneralParams();
        foreach (var p in _parms.Keys)
        {
            parameters.Add(p, _parms[p]);
        }
    }
    public static void AddGeneralParameters(CustomEvent customEvent)
    {
        Dictionary<string, object> _parms = GetGeneralParams();
        foreach (var p in _parms.Keys)
        {
            customEvent.Add(p, _parms[p]);
        }
    }

    //public static void AddGeneralParameters(GameEvent gameEvent)
    //{
    //    Dictionary<string, object> _parms = GetGeneralParams();
    //    foreach (var p in _parms.Keys)
    //    {
    //        gameEvent.AddParam(p, _parms[p]);
    //    }

    //}

    //public static void AddGeneralParameters(Params gameEvent)
    //{
    //    Dictionary<string, object> _parms = GetGeneralParams();
    //    foreach (var p in _parms.Keys)
    //    {
    //        gameEvent.AddParam(p, _parms[p]);
    //    }
    //}


    private static string GetSceneName()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.name;
    }

    //public static void AddGeneralParameters(Transaction gameEvent)
    //{
    //    Dictionary<string, object> _parms = GetGeneralParams(withCheaterScore: false);
    //    foreach (var p in _parms.Keys)
    //    {
    //        gameEvent.AddParam(p, _parms[p]);
    //    }

    //}

    //public static void AddGeneralParameters(Engagement gameEvent)
    //{
    //    Dictionary<string, object> _parms = GetGeneralParams(false);
    //    foreach (var p in _parms.Keys)
    //    {
    //        gameEvent.AddParam(p, _parms[p]);
    //    }
    //}
}