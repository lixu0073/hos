using AssetBundles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameAssetBundleManager : MonoBehaviour
{

    #region Modules

    public HospitalSignAssetBundleModule hospitalSign;
    public HospitalFlagAssetBundleModule hospitalFlag;
    public GlobalEventAssetBundleModule globalEvents;

    #endregion

    #region Static

    public static GameAssetBundleManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    #endregion

    private Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, OnSuccess> successCallbacks = new Dictionary<string, OnSuccess>();
    private Dictionary<string, OnFailure> failureCallbacks = new Dictionary<string, OnFailure>();
    private Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

    private static string GetUrl(string name, string route, int version)
    {
		return Hospital.LoadingGame.GetRootURL() + AssetBundles.Utility.GetPlatformName() + "/" + AssetBundles.Utility.GetPlatformName() + "/" + route + "/" + version + "/" + name.ToLower();
    }

    public delegate void OnSuccess(AssetBundle assetBundle);
    public delegate void OnFailure(Exception exception);

    #region API

    public static void UnloadAssetBundle(string name)
    {
        GameAssetBundleManager gameAssetBundleManager = GameAssetBundleManager.instance;
        if (gameAssetBundleManager == null)
        {
            throw new Exception("GameAssetBundleManager not Initialized");
        }
        if (gameAssetBundleManager.assetBundles.ContainsKey(name))
        {
            gameAssetBundleManager.assetBundles[name].Unload(true);
            gameAssetBundleManager.assetBundles.Remove(name);
        }
        UnbindCallbacks(name);
    }

    public static void UnloadAllAssetBundles()
    {
        GameAssetBundleManager gameAssetBundleManager = GameAssetBundleManager.instance;
        if (gameAssetBundleManager == null)
        {
            throw new Exception("GameAssetBundleManager not Initialized");
        }
        foreach(KeyValuePair<string, AssetBundle> pair in gameAssetBundleManager.assetBundles)
        {
            pair.Value.Unload(true);
        }
        gameAssetBundleManager.assetBundles.Clear();
        gameAssetBundleManager.successCallbacks.Clear();
        gameAssetBundleManager.failureCallbacks.Clear();
    }

    public static void UnbindCallbacks(string name)
    {
        GameAssetBundleManager gameAssetBundleManager = GameAssetBundleManager.instance;
        if (gameAssetBundleManager == null)
        {
            throw new Exception("GameAssetBundleManager not Initialized");
        }
        gameAssetBundleManager.successCallbacks.Remove(name);
        gameAssetBundleManager.failureCallbacks.Remove(name);
    }

    public static void GetAssetBundle(string name, string route, int version, OnSuccess onSuccess, OnFailure onFailure)
    {
        GameAssetBundleManager gameAssetBundleManager = GameAssetBundleManager.instance;
        if(gameAssetBundleManager == null)
        {
            onFailure?.Invoke(new Exception("GameAssetBundleManager not Initialized"));
            return;
        }
        gameAssetBundleManager.GetAssetBundleAPI(name, route, version, onSuccess, onFailure);
    }

    #endregion

    #region Methods

    public void GetAssetBundleAPI(string name, string route, int version, OnSuccess onSuccess, OnFailure onFailure)
    {
        successCallbacks[name] = onSuccess;
        failureCallbacks[name] = onFailure;
        if(assetBundles.ContainsKey(name))
        {
            onSuccess?.Invoke(assetBundles[name]);
            return;
        }
        if(!coroutines.ContainsKey(name))
        {
            coroutines.Add(name, StartCoroutine(DownloadOrLoadFromCache(name, route, version)));
        }
    }

    #endregion

    #region Core

    IEnumerator DownloadOrLoadFromCache(string name, string route, int version)
    {
        using (WWW www = WWW.LoadFromCacheOrDownload(GetUrl(name, route, version), version))
        {
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                SuccessGetTextureCallback(name, www.assetBundle);
            }
            else
            {
                FailureCallback(name, new Exception(www.error));
            }
        }
        
        UnbindCallbacks(name);
        coroutines.Remove(name);
    }

    private void SuccessGetTextureCallback(string name, AssetBundle assetBundle)
    {
        assetBundles[name] = assetBundle;
        if(successCallbacks.ContainsKey(name))
        {
            successCallbacks[name].Invoke(assetBundle);
        }
    }

    private void FailureCallback(string name, Exception exception)
    {
        if(failureCallbacks.ContainsKey(name))
        {
            failureCallbacks[name].Invoke(exception);
        }
    }

    #endregion

}
