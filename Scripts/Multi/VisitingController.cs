using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Hospital
{
    public class VisitingController : MonoBehaviour
    {
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                HospitalAreasMapController.HospitalMap.DestroyMap();
                Debug.LogError("Loading Scene form 0. This was called by pressing f6 lol");
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LoadingScene");
            }
        }

        #region static
        private static VisitingController instance;

        public static VisitingController Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("No instance of PharmacyDynamoconnector was found on scene!");
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("Multiple instances of Visiting Controller were found!");
            }
            else
                instance = this;
        }

        #endregion

        public bool canVisit = true;

        private bool Visiting = false;
        public bool IsVisiting
        {
            get { return Visiting; }
        }

        private bool VisitingScheduled = false;
        public bool RestoringScheduled = false;

        private Save localSave;

        public Save LocalSave
        {
            get
            {
                if (localSave == null)
                    throw new IsoEngine.IsoException("Fatal Failure of LocalSave. Can't find!");
                return localSave;
            }
            set
            {
                localSave = value;
            }
        }

        public void RunVisiting(Save save)
        {
            if (!Visiting)
            {
                PrepareVisitingSave();
                if (!GameState.saveFilePrepared || CognitoEntry.SaveID != SaveLoadController.SaveState.ID)
                {
                    AnalyticsController.instance.ReportException("visitng_reset", new Exception());
                    Visiting = false;
                    VisitingScheduled = false;
                    localSave = null;
                    HospitalAreasMapController.HospitalMap.DestroyMap();
                    Debug.LogError("Loading game from 0. This is called from VisitingController.RunVisiting()");
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LoadingScene");
                    return;
                }
            }

            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.VisitOtherHospitals));
            HospitalAreasMapController.HospitalMap.ReloadGame(save, true, false);
            VisitingScheduled = false;
        }

        public void RedirectToMaternityMap(bool sourceFromButton = true)
        {
            SaveSynchronizer.Instance.InstantSave();
            LocalNotificationController.Instance.CacheNotifications();
            ShutDownSystems();
            AnalyticsController.instance.ReportChangeScene(sourceFromButton, "MaternityScene");
            UIController.get.LoadingPopupController.Open(0, 0, 0);
            AreaMapController.Map.IsoDestroy();
            SceneManager.LoadScene("RedirectToMaternityScene");
        }

        public void RedirectToDeveloperScene(bool sourceFromButton = true)
        {
            SaveSynchronizer.Instance.InstantSave();
            LocalNotificationController.Instance.CacheNotifications();
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene != null && activeScene.name == "MainScene") // CV: Check if we came from MainScene (normal Hospital)
                ShutDownSystems();        
            AnalyticsController.instance.ReportChangeScene(sourceFromButton, "DevelopScene");
            UIController.get.LoadingPopupController.Open(0, 0, 0);
            AreaMapController.Map.IsoDestroy();
            SceneManager.LoadScene("DevelopScene");
        }

        private void ShutDownSystems()
        {
            ReferenceHolder.GetHospital().treatmentHelpAPI.StopRefreshingRequests();
        }

        public void Visit(string UserId, bool OpenPharmacy = false)
        {
            if (!canVisit || RestoringScheduled || VisitingScheduled)
            {          
                return;
            }

            SoundsController.Instance.StopAllSounds();

            VisitingScheduled = true;
            UIController.get.CloseActiveHover();
            UIController.get.ExitAllPopUps();
            SaveSynchronizer.Instance.InstantSave();
            if (AccountManager.HasInternetConnection())
            {
                UIController.get.LoadingPopupController.Open(0, 100, 3);
                TutorialUIController.Instance.InGameCloud.Hide();
                SaveSynchronizer.Instance.GetUserSave(UserId, (x) =>
                {
                    ReferenceHolder.Get().engine.AddTask(() =>
                    {
                        if (x == null)
                        {
                            VisitingScheduled = false;
                            UIController.get.LoadingPopupController.Exit();
                            return;
                        }
                        OnSuccessGetVisitingSave(x, OpenPharmacy);
                    });
                }, (ex) =>
                {
                    VisitingScheduled = false;
                    BaseUIController.ShowServerOrInternetConnectionProblem(ex);
                });
            }
            else
            {
                VisitingScheduled = false;
                BaseUIController.ShowInternetConnectionProblemPopup(this);
            }
        }

        private void OnSuccessGetVisitingSave(Save save, bool OpenPharmacy)
        {
            var version = DefaultConfigurationProvider.GetConfigCData().GetVersion();
            //AccountManager.Instance.GetVersion((version) =>
            //{
                ReferenceHolder.Get().engine.AddTask(() =>
                {
                    UIController.get.LoadingPopupController.Exit();
                    if (int.Parse(version, System.Globalization.CultureInfo.InvariantCulture) > int.Parse(GameState.Get().Version, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        VisitingScheduled = false;
                        UIController.get.LoadingPopupController.Exit();
                        BaseUIController.ShowNewVersionPopup(this);
                    }
                    else
                    {
                        save = VersionManager.Instance.UpgradeSave(save, true);
                        RunVisiting(save);
                        UIController.get.LoadingPopupController.Exit();
                        if (OpenPharmacy)
                        {
                            UIController.getHospital.PharmacyPopUp.Open(true, SaveLoadController.SaveState.ID, SaveLoadController.SaveState.HospitalName);
                        }
                    }
                });
            //}, (ex) =>
            //{
            //    ReferenceHolder.Get().engine.AddTask(() =>
            //    {
            //        VisitingScheduled = false;
            //        UIController.get.LoadingPopupController.Exit();
            //        BaseUIController.ShowServerOrInternetConnectionProblem(ex);
            //    });
            //});
        }

        private bool HaveHaigherVersions(Save save)
        {
            int.TryParse(GameState.Get().Version, out int MySaveVersion);
            int.TryParse(save.version, out int VisitedSaveVersion);
            return MySaveVersion > VisitedSaveVersion;
        }

        public void Restore()
        {
            if (!IsVisiting || VisitingScheduled)
                return;

            SoundsController.Instance.StopAllSounds();

            UIController.get.FriendsDrawer.HideFriendsDrawer();
            RestoringScheduled = true;
            //if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.wise_thank_you || TutorialController.Instance.CurrentTutorialStepTag == StepTag.emma_about_wise)
            //    TutorialController.Instance.WiseVisitedThisSession = true;
            Visiting = false;
            UIController.get.LoadingPopupController.Open(0, 100, 1);
            SaveSynchronizer.Instance.LoadGame(true);
        }

        public void ShowICP()
        {
            //LocalNotificationController.Instance.Test();
            //return;
            //UIController.ShowNonActivePopup();

            //UIController.ShowCriticalProblemPopup();

            //CacheManager.CacheSave();
            //return;

            SaveSynchronizer.Instance.InstantSave();

            // SAVE WISE
            /*var Id = "SuperWise";
            var p = new Save();
            p.ID = Id;

            HospitalAreasMapController.Map.SaveGame(p);
            p.Level = 100;
            p.HospitalName = "Doctor Wise";
            SaveLoadController.Get().SaveGame(p);*/
        }

        public void VisitWiseHospital()
        {
            if (Game.Instance.gameState().GetHospitalLevel() < 4 || RestoringScheduled ||
                !SaveLoadController.Get().HasSaveInResources("wise") || VisitingScheduled)
            {
                return;
            }            

            SoundsController.Instance.StopAllSounds();

            VisitingScheduled = true;

            UIController.get.LoadingPopupController.Open(0, 100, 1);

            TutorialUIController.Instance.InGameCloud.Hide();
            TutorialUIController.Instance.tutorialArrowOnMap.Hide();

            Invoke("LoadWiseHospital", 1);
        }

        private void LoadWiseHospital()
        {
            Save save = SaveLoadController.Get().LoadGameFromResource("wise");
            if (save != null)
            {
                if (!Visiting)
                {
                    PrepareVisitingSave();
                    if (!GameState.saveFilePrepared || (!string.IsNullOrEmpty(SaveLoadController.SaveState.ID) && CognitoEntry.SaveID != SaveLoadController.SaveState.ID))
                    {
                        AnalyticsController.instance.ReportException("visitng_reset", new Exception());
                        Visiting = false;
                        VisitingScheduled = false;
                        localSave = null;
                        HospitalAreasMapController.HospitalMap.DestroyMap();
                        Debug.LogError("Loading game from 0. This is called from VisitingController.LoadWiseHospital()" +
                            "saveFilePrepared : " + GameState.saveFilePrepared +
                            "SaveLoadController.SaveState.ID: " + SaveLoadController.SaveState.ID +
                            "CognitoEntry.SaveID : " + CognitoEntry.SaveID +
                            "SaveLoadController.SaveState.ID" + SaveLoadController.SaveState.ID);
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LoadingScene");
                        return;
                    }
                }
                HospitalAreasMapController.HospitalMap.ReloadGame(save, true, false);
                UIController.get.LoadingPopupController.Exit();
                NotificationCenter.Instance.WiseHospitalLoaded.Invoke(new BaseNotificationEventArgs());
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.VisitOtherHospitals));
                TutorialController.Instance.ShowCurrentStepPopupInformation();
                VisitingScheduled = false;
            }
            else
            {
                VisitingScheduled = false;
            }
        }

        private void PrepareVisitingSave()
        {
            localSave = SaveCacheSingleton.GetCachedSave();
            Visiting = true;
            HospitalAreasMapController.HospitalMap.SaveGame(localSave);
        }
    }
}
