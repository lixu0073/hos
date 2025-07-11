using System;
using System.Collections.Generic;
using UnityEngine;
using MovementEffects;
using UnityEngine.SceneManagement;
using Hospital.Connectors;

namespace Hospital
{
    public class SaveSynchronizer : MonoBehaviour
    {
        #region static
        private static SaveSynchronizer instance;

        public static SaveSynchronizer Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("No instance of SaveSynchronizer was found on scene!");
                return instance;
            }
        }
        #endregion

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("Multiple instances of Save entrypoint were found!");
            }
            else
                instance = this;

        }

        void Start()
        {
            LoadGame();
            Timing.RunCoroutine(Synchronizing());
        }

        #region types
        public delegate void OnGameLoad(Save loadedSave);
        public delegate void OnFailure(Exception ex);
        #endregion

        #region fields
        int savePriority;
        bool saveByButton = false;

        #endregion

        #region synchronizing
        public void MarkToSave(int priority)
        {
            savePriority += priority;
            //Debug.Log("added to save: " + priority + " overall: " + savePriority + " needed to save: " + SavePriorities.SaveThreshold);
        }

        private IEnumerator<float> Synchronizing()
        {
            while (true)
            {
                //Debug.Log("Synchronizing " + savePriority + "/" + SavePriorities.SaveThreshold);
                if (savePriority >= SavePriorities.SaveThreshold)
                {
                    SaveGameState();
                }
                yield return Timing.WaitForSeconds(SavePriorities.MinimumSaveInterval);
            }
        }
        #endregion

        private void SaveGame(string hash)
        {
            //return; //otututu
#if UNITY_EDITOR
            if (DeveloperParametersController.Instance().parameters.blockSaving)
            {
                return;
            }
#endif

            if (AreaMapController.Map == null || AreaMapController.Map.GetAreas() == null || AreaMapController.Map.GetAreas().Count == 0)
            {
                Debug.LogError("Trying save game before loaded");
                return;
            }

            if (hash == null)
                return;

            if (VisitingController.Instance.IsVisiting) // SAVE IN VISITING MODE
            {
                var locSave = VisitingController.Instance.LocalSave;
                locSave.ID = hash;

                AreaMapController.Map.SaveGameInVisigingMode(locSave);

                if (locSave.Level == 0)
                {
                    BaseUIController.ShowCriticalProblemPopup(this);
                    AnalyticsController.instance.ReportBug("level_zero_error_1");
                    return;
                }
                if (string.IsNullOrEmpty(locSave.version))
                {
                    AnalyticsController.instance.ReportBug("empty_game_version_error");
                    RedirectToLoadingScene();
                    return;
                }
                CacheManager.CacheSave(false);

                SendSave(locSave, hash);
                return;
            }

            if (!AreaMapController.HomeMapLoaded)
            {
                return;
            }

            if (UIController.get.drawer.IsDraggingAnything())
            {
                Debug.LogError("Don't save 'cuz you are in Edit Mode");
                return;
            }

            // WARNING: Previous version created a copy of the save and merged data into it.
            // This version fetches the cached save DIRECTLY.
            var p = SaveCacheSingleton.GetCachedSave();
            AreaMapController.Map.SaveGame(p);

            /* if(TimedOffersController.Instance != null)
             {
                 TimedOffersController.Instance.SaveToPlayerPrefs();
             }*/

            if (p.Level == 0)
            {
                BaseUIController.ShowCriticalProblemPopup(this);
                AnalyticsController.instance.ReportBug("level_zero_error_2");
                return;
            }
            if (string.IsNullOrEmpty(p.version))
            {
                AnalyticsController.instance.ReportBug("empty_game_version_error");
                RedirectToLoadingScene();
                return;
            }
            CacheManager.CacheSave(false);

            SendSave(p, hash);
        }


        private async void SendSave(Save p, string hash)
        {
            if (BaseGameState.saveFilePrepared)
            {
                if (AreaMapController.Map.IsMapEmpty())
                {
                    return;
                }
                //local save cash
                //SaveLoadController.Get().SaveGame(p, hash);

                if (AccountManager.HasInternetConnection())
                {
                    try
                    {
                        await SaveConnector.SaveAsync(p);
                        Debug.Log("[SYNCHRONIZER] save successfull");
                        IAPController.instance.ConfirmPendingPurchase();
                        AnalyticsController.instance.ReportSave(false);

                        if (saveByButton)
                        {
                            saveByButton = false;
                            MessageController.instance.ShowMessage(33);
                        }
                    }
                    catch (Exception e)
                    {
                        if (saveByButton)
                        {
                            saveByButton = false;
                            MessageController.instance.ShowMessage(34);
                        }
                        BaseUIController.ShowServerOrInternetConnectionProblem(e);
                        SaveSynchronizer.LogError(e);
                    }
                }
                else
                {
                    BaseUIController.ShowInternetConnectionProblemPopup(this);
                    SaveSynchronizer.LogError("There is no internet connection");
                }
            }
            else
            {
                Debug.LogError("Save validation failure");
            }
        }
        public void InstantSave()
        {
            SaveGameState();
        }

        private void SaveGameState()
        {
            savePriority = 0;
            if (string.IsNullOrEmpty(CognitoEntry.SaveID))
            {
                CognitoEntry.OnSaveIDRetrieval -= SaveGame;
                CognitoEntry.OnSaveIDRetrieval += SaveGame;
            }
            else
            {
                SaveGame(CognitoEntry.SaveID);
            }
        }


        #region Save&Load
        public void GetUserSave(string UserID, OnGameLoad onSuccess, OnFailure onFailure)
        {
            AccountManager.ProcessIfInternetConnected(async () =>
            {
                try
                {
#if UNITY_EDITOR
                    if (PlayerPrefs.GetInt("UseCloud") != -1)
                    {
                        var result = await SaveConnector.LoadAsync(UserID);
                        onSuccess?.Invoke(result);
                    }
                    else
                    {
                        Save save = CacheManager.GetSaveFromCache();
                        onSuccess?.Invoke(save);
                    }
#else
                    var result = await SaveConnector.LoadAsync(UserID);
                    onSuccess?.Invoke(result);
#endif
                }
                catch (Exception e)
                {
                    onFailure?.Invoke(e);
                }
            });
        }

        private void TryToLoadSaveFromCloud(Save save, bool restoreFromVisiting)
        {
            try
            {
                save = VersionManager.Instance.UpgradeSave(save);
                TimeFix(save);
                AreaMapController.Map.ReloadGame(save, false, !restoreFromVisiting);
                SuccessGameLoad(restoreFromVisiting);
                OnUserDataLoad();
                PublicSaveManager.Instance.UpdatePublicSave();
                IAPController.instance.Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError("Error from cloud save: " + e.Message);
                AnalyticsController.instance.ReportException("bug_loading_save_cloud", e);
                BaseUIController.ShowCriticalProblemPopup(this);
                throw e;
            }
        }

        private static int RetryCounter = 3;

        private void LoadGame(string hash, bool restoreFromVisiting = false)
        {
            if (AccountManager.HasInternetConnection())
            {
                AccountManager.Instance.GetCurrentSaveByUserId(hash, (save) =>
                {
                    ReferenceHolder.Get().engine.AddTask(() =>
                    {
                        if (save == null)
                        {
                            AnalyticsController.instance.ReportBug("savedynamo.save_is_null");
                            Debug.Log("Save is null");
                            Save cachedSave = CacheManager.GetSaveFromCache();

                            if (DevelopSaveController.IsEnabled && DevelopSaveHolder.Instance != null && !string.IsNullOrEmpty(DevelopSaveHolder.Instance.ForcedCognito))
                            {
                                Save saveFromResources = SaveLoadController.Get().LoadGameFromResource("Saves/" + DevelopSaveHolder.Instance.SaveName);
                                if (saveFromResources != null)
                                {
                                    saveFromResources.ID = hash;
                                    saveFromResources = VersionManager.Instance.UpgradeSave(saveFromResources);
                                    TimeFix(saveFromResources);
                                    if (saveFromResources.Level > 1)
                                    {
                                        CacheManager.Instance.SetUserHasUpFirstLevel(saveFromResources.ID);
                                    }
                                }

                                AreaMapController.Map.ReloadGame(saveFromResources == null ? SaveLoadController.GenerateDefaultSave() : saveFromResources);

                                SuccessGameLoad(restoreFromVisiting);
                                OnUserDataLoad();
                                PublicSaveManager.Instance.UpdatePublicSave();
                                IAPController.instance.Initialize();
                                VisitingController.Instance.RestoringScheduled = false;
                            }
                            else if (DefaultConfigurationProvider.GetConfigCData().EnableResetHospitalFix && CacheManager.Instance.HasUserUpFirstLevel(CognitoEntry.SaveID))
                            {
                                Debug.Log("Hospital reset attempt");
                                RetryCounter--;
                                if (RetryCounter <= 0)
                                {
                                    ///TODO possible fix if reset attempts fail<see href= "https://discussions.unity.com/t/how-to-restart-application/180865/5"/>
                                    AnalyticsController.instance.ReportBug("savedynamo.fix_failed");
                                    BaseUIController.ShowCriticalProblemPopup(this, "savedynamo.fix_failed");
                                    VisitingController.Instance.RestoringScheduled = false;
                                    return;
                                }
                                LoadGame(hash, restoreFromVisiting);
                            }
                            else if (cachedSave != null)
                            {
                                AreaMapController.Map.ReloadGame(cachedSave);
                                SuccessGameLoad(restoreFromVisiting);
                                OnUserDataLoad();
                                IAPController.instance.Initialize();
                                PublicSaveManager.Instance.UpdatePublicSave();
                                VisitingController.Instance.RestoringScheduled = false;
                            }
                            else
                            {
                                AnalyticsController.instance.ReportBug("savedynamo.generate_default");
                                Debug.Log("Generate default save");
                                AreaMapController.Map.ReloadGame(SaveLoadController.GenerateDefaultSave());
                                SuccessGameLoad(restoreFromVisiting);
                                OnUserDataLoad();
                                IAPController.instance.Initialize();
                                VisitingController.Instance.RestoringScheduled = false;
                            }
                        }
                        else
                        {
                            Save cloudSave = save;
                            try
                            {
                                Save newSave = GetSaveFromCacheOrServer(save);
                                newSave = VersionManager.Instance.UpgradeSave(newSave);
                                TimeFix(newSave);
                                if (newSave.Level > 1)
                                {
                                    CacheManager.Instance.SetUserHasUpFirstLevel(newSave.ID);
                                }

                                if (string.IsNullOrEmpty(save.maxGameVersion)) save.maxGameVersion = save.gameVersion;
                                if (SaveLoadController.SaveVersionIsNewerThanGameVerion(save.maxGameVersion))
                                {
                                    StartCoroutine(UIController.get.alertPopUp.Open(AlertType.SAVE_HAS_MORE_RECENT_SAVE_THAN_CLIENT));
                                    return;
                                }

                                AreaMapController.Map.ReloadGame(newSave, false, !restoreFromVisiting);
                                SuccessGameLoad(restoreFromVisiting);
                                OnUserDataLoad();
                                PublicSaveManager.Instance.UpdatePublicSave();
                                IAPController.instance.Initialize();
                                VisitingController.Instance.RestoringScheduled = false;
                                AnalyticsController.instance.ReportLoad(IsSaveGetFromCache(cloudSave));
                            }
                            catch (VersionManagerException exception)
                            {
                                LogError(exception);
                                Debug.LogError(exception.Message);
                                CriticalErrorOccuredAndRedirectToLoadingScene();
                                AnalyticsController.instance.ReportException("bug_loading_version", exception);
                                VisitingController.Instance.RestoringScheduled = false;
                            }
                            catch (Exception e)
                            {
                                LogError(e);
                                if (IsSaveGetFromCache(cloudSave))
                                {
                                    Debug.LogError("Error from cache save: " + e.StackTrace);
                                    VisitingController.Instance.RestoringScheduled = false;
                                    TryToLoadSaveFromCloud(cloudSave, restoreFromVisiting);
                                    AnalyticsController.instance.ReportException("bug_loading_save_cached", e);
                                }
                                else
                                {
                                    Debug.LogError("Error from cloud save: " + e.Message + e.StackTrace);
                                    VisitingController.Instance.RestoringScheduled = false;
                                    AnalyticsController.instance.ReportException("bug_loading_save_cloud", e);
                                    BaseUIController.ShowCriticalProblemPopup(this);
                                    throw e;
                                }
                            }
                        }
                    });
                }, (saveProviderItem) =>
                {
                    VisitingController.Instance.RestoringScheduled = false;
                    OnUserDataLoad();
                    CriticalErrorOccuredAndRedirectToLoadingScene();
                }, (ex) =>
                {
                    VisitingController.Instance.RestoringScheduled = false;
                    Debug.LogError(ex.Message);
                    LogError(ex);
                    RedirectToLoadingScene();
                });
            }
            else
            {
                VisitingController.Instance.RestoringScheduled = false;
                RedirectToLoadingScene();
            }
        }

        private void LoadGameNotFromVisiting(string hash)
        {
            LoadGame(hash, restoreFromVisiting: false);
        }

        public static void LogError(Exception ex)
        {
            //#if !UNITY_EDITOR
            //if(Debug.isDebugBuild)
            SettingsPopup.SendLogByEmail(ex);
            //#endif
            //#endif
        }

        public static void LogError(string devErrorMessage)
        {
            SettingsPopup.SendLogByEmail(devErrorMessage);
        }

        public static bool IsSaveGetFromCache(Save save)
        {
            Save cachedSave = CacheManager.GetSaveFromCache();
            if (cachedSave == null)
            {
                return false;
            }
            if (cachedSave.saveDateTime == save.saveDateTime)
            {
                return cachedSave.MaternitySaveDateTime >= save.MaternitySaveDateTime;
            }
            return cachedSave.saveDateTime >= save.saveDateTime;
        }

        public static Save GetSaveFromCacheOrServer(Save save)
        {
            Save cachedSave = CacheManager.GetSaveFromCache();
            if (cachedSave == null)
            {
                Debug.Log("Use online save");
                return save;
            }
            if (cachedSave.saveDateTime == save.saveDateTime)
            {
                Debug.Log("Use cached save: " + (cachedSave.MaternitySaveDateTime >= save.MaternitySaveDateTime));
                return cachedSave.MaternitySaveDateTime >= save.MaternitySaveDateTime ? cachedSave : save;
            }
            return cachedSave.saveDateTime >= save.saveDateTime ? cachedSave : save;
        }

        private void TimeFix(Save save)
        {
            if (save.saveDateTime <= 0)
            {
                save.saveDateTime = ServerTime.UnixTime(save.GetSaveTime());
            }
        }

        private void CriticalErrorOccuredAndRedirectToLoadingScene()
        {
            GlobalDataHolder globalDataHolder = GlobalDataHolder.instance;
            if (globalDataHolder != null)
            {
                globalDataHolder.IsCriticalErrorOccured = true;
            }
            RedirectToLoadingScene();
        }

        private void RedirectToLoadingScene()
        {
            AreaMapController.Map.DestroyMap();

            Debug.LogError("Loading game from 0. This is called from SaveSynchronizer.RedirectToLoadingScene().");

            SceneManager.LoadSceneAsync("LoadingScene");
        }

        public void LoadGame(bool restoreFromVisiting = false)
        {
            if (string.IsNullOrEmpty(CognitoEntry.UserID))
            {
                CognitoEntry.OnUserIDRetrieval -= LoadGameNotFromVisiting;
                CognitoEntry.OnUserIDRetrieval += LoadGameNotFromVisiting;
            }
            else
            {
                LoadGame(CognitoEntry.UserID, restoreFromVisiting);
            }
        }

        void OnDestroy()
        {
            CognitoEntry.OnUserIDRetrieval -= LoadGameNotFromVisiting;
        }
        #endregion

        #region Actions

        //private void RequestGameLoadedImage()
        //{
        //    DecisionPointCalss.RequestImageMessage(DecisionPoint.game_loaded, null);
        //    IAPController.instance.onIapInitialized -= RequestGameLoadedImage;
        //}

        private void SuccessGameLoad(bool restoreFromVisiting = false)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;

            //Debug.LogError("SuccessGameLoad");
            if (!AnalyticsController.GameLoadedShown)
            {
                //TenjinController.instance.SendGameLoadedEvent();
                //IAPController.instance.onIapInitialized -= RequestGameLoadedImage;
                //IAPController.instance.onIapInitialized += RequestGameLoadedImage;
                //AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.game_loaded, 2.5f);
                AnalyticsController.GameLoadedShown = true;
            }
            else
            {
                AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.game_reloaded, 2.5f);
            }

            if (restoreFromVisiting)
            {
                if (HospitalAreasMapController.HospitalMap != null)
                {
                    HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);
                }
                NotificationCenter.Instance.HomeHospitalLoaded.Invoke(new BaseNotificationEventArgs());
                InstantSave();
            }
            else
            {
                AnalyticsManager.Instance.OnLaunched();
                BundleManager.Instance.LoadLocalBundleDatabase();
                DailyDealParser.Instance.LoadDailyDealData(DefaultConfigurationProvider.GetConfigCData().DailyDealConfig);
            }

            AnalyticsController.instance.starterPack.CheckForStarterPack();
            ConnectFBPopupController.TryToShowFBConnectReward();
            ReferenceHolder.Get().inGameFriendsProvider.FetchFriends();
            ReferenceHolder.Get().engine.AddTask(() =>
            {
                SaveLoadManager.timeEmulated = true;
            });

            if (!restoreFromVisiting)
            {
                //HelpShiftManager.Instance.UpdateMetadata();
            }
        }

        private void OnUserDataLoad()
        {
            HideClouds();
            UIController.get.LoadingPopupController.Exit();
            HospitalUIPrefabController.Instance.ShowMainUI(1.25f);
            SoundsController.Instance.CheckSoundSettings();
        }

        public void HideClouds()
        {
            if (UIController.get.CloudIntroAnim != null)
            {
                Animator animator = UIController.get.CloudIntroAnim.GetComponent<Animator>();
                animator.SetTrigger("Start");
            }
        }
        #endregion

        #region etceteras
        public void SaveButtonClicked()
        {
            saveByButton = true;
        }
        #endregion

        public void ResetProgress()
        {
            Save defLevel = SaveLoadController.GenerateDefaultSave();
            ReferenceHolder.Get().objectiveController.UpdateObjectives();
            defLevel.CampaignConfigs = "Objectives?ObjectivesDynamic";
            CampaignConfig.hintSystemEnabled = false;
            HospitalAreasMapController.HospitalMap.hospitalBedController.ClearAllBedController(); // CV;
            AreaMapController.Map.ReloadGame(defLevel);
            HospitalAreasMapController.HospitalMap.IsoDestroy(); // CV
            SuccessGameLoad(false);
            //OnUserDataLoad();
            PublicSaveManager.Instance.UpdatePublicSave();
            IAPController.instance.Initialize();
            VisitingController.Instance.RestoringScheduled = false;
            HospitalAreasMapController.HospitalMap.StartGame();  // CV
        }
    }
}
