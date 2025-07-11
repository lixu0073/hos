using UnityEngine;
using System.Collections.Generic;
using Hospital;
using MovementEffects;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    private bool canLoad = false;
    public static bool timeEmulated = false;

    public void StartSaving()
    {
        StopSaving();
        Timing.RunCoroutine(SaveInterval());
    }

    public void StopSaving()
    {
        Timing.KillCoroutine(SaveInterval().GetType());
    }

    void Awake()
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        timeEmulated = false;
    }

    void OnApplicationFocus(bool focusState)
    {
        UIController.get.preloaderView.Exit();
    }

    private TimePassedObject GetTimePassedObject(Save save)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "MainScene")
            return new HospitalTimePassedObject(save.saveDateTime, save.MaternitySaveDateTime);
        return new MaternityTimePassedObject(save.saveDateTime, save.MaternitySaveDateTime);
    }

    void OnApplicationPause(bool focusState)
    {
        try
        {
            if (UIController.get != null && UIController.get.preloaderView != null)
                UIController.get.preloaderView.Exit();
            else
                Debug.LogError("SaveLoadManager::OnApplicationPause - something is wrong: focusState: " + focusState);

            if (focusState)
            {
                StopSaving();
                canLoad = true;
                LocalNotificationController.Instance.RemoveNotifications();
                if (timeEmulated)
                {
                    LocalNotificationController.Instance.SetAllNotifications();
                    CacheManager.CacheSave(true);
                }
                UIController.get.CloseActiveHover();
                timeEmulated = false;
            }
            else
            {
                if (canLoad)
                {
                    NoActivityController.Instance.UpdateLastActivityTime();
                    ServerTime.Get().StopUpdateServerTime();
                    SoundsController.Instance.CheckSoundSettings();

                    Save save = CacheManager.GetSaveFromCache();
                    if (save != null)
                    {
                        TimePassedObject timePass = GetTimePassedObject(save);
                        Debug.Log("GET SAVED PAUSE TIME AT START : " + timePass.GetSaveTime());
                        Debug.Log("SERVER TIME AT START : " + ServerTime.Get().GetServerTime().ToString());

                        long time = timePass.GetTimePassed();

                        Debug.Log(time);

                        double timeOut = 360;//240;
//#if UNITY_ANDROID
//                    timeOut = 240;
//#endif

                        if (IAPController.isPauseBlocked)
                            timeOut = int.MaxValue;

                        if (time > timeOut || time < 0)
                        {
                            Debug.LogError("Loading game from 0. This is called from SaveLoadManager.OnApplicationPause() " +
                                "time > timeOut:" + (time > timeOut).ToString() + " ::: " + "time < 0:" + (time < 0).ToString());
                            Debug.LogError("RELOAD GAME AFTER TIME : " + time);
                            UIController.get.LoadingPopupController.Open(0, 100, 5);
                            // experimental
                            AreaMapController.Map.DestroyMap();
                            SceneManager.LoadSceneAsync("LoadingScene");
                        }
                        else
                        {
                            Debug.LogError("PAUSE TIME EMULATED : " + time);
                            AreaMapController.Map.EmulateMapObjectTime(timePass);
                            ReferenceHolder.Get().engine.AddTask(() =>
                            {
                                SaveLoadManager.timeEmulated = true;
                                UIController.get.preloaderView.Exit();
                            });
                            if (DefaultConfigurationProvider.GetConfigCData().AddAdsRewardFromSaveLogic && Game.Instance.gameState().IsAdRewardActive())
                            {
                                try
                                {
                                    AdsController.AdType adType = (AdsController.AdType)Enum.Parse(typeof(AdsController.AdType), Game.Instance.gameState().GetAdType());
                                    AdsController.instance.RewardPlayer(adType);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex.Message);
                                }
                            }
                            StartSaving();
                        }
                    }
                    else
                    {
                        Debug.LogError("RELOAD GAME BECAUSE CAN'T GET PAUSE TIME PROPERTY");
                        UIController.get.LoadingPopupController.Open(0, 100, 5);
                        AreaMapController.Map.DestroyMap();
                        SceneManager.LoadSceneAsync("LoadingScene");
                    }
                    canLoad = false;
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogError("SaveLoadManager::OnApplicationPause: " + ex.Message + "//n" + ex.StackTrace);
        }
    }

    void OnApplicationQuit()
    {
        ServerTime.Get().StopUpdateServerTime();
    }

    IEnumerator<float> SaveInterval()
    {
        for (; ; )
        {
            yield return Timing.WaitForSeconds(1);
            if (!VisitingController.Instance.IsVisiting)
            {
                SaveSynchronizer.Instance.MarkToSave(1);
            }
        }
    }
}
