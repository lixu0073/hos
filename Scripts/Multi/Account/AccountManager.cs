using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Hospital.Connectors;
using UnityEditor;

namespace Hospital
{
    public class AccountManager : MonoBehaviour
    {
        #region Static
        private static AccountManager instance;

        public static AccountManager Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("There is no AccountManager instance on scene!");

                return instance;
            }
        }

        void Awake()
        {
            if (instance != null)
                Debug.LogWarning("There are possibly multiple instances of AccountManager on scene!");

            instance = this;
        }
        #endregion

        public GetCurrentSaveUseCase getCurrentSaveUseCase;

        public delegate void OnUpdate();

        #region API
        public static event OnUpdate OnFacebookStateUpdate;
        public static event OnUpdate OnGameCenterStateUpdate;
        public static event OnUpdate OnFacebookFriendsUpdate;
        public static event OnUpdate OnFacebookUserDataUpdate;
        public static event OnUpdate OnFacebookConnected;
        public static event OnUpdate OnGPGStateUpdate;

        public delegate void OnDataChanged();
        public static event OnDataChanged updateGiftsBadge;

        public delegate void OnSuccessUserModel(UserModel userModel);
        public delegate void OnSuccessFriends(List<ProviderModel> list);
        public delegate void OnSuccessSave(Save saveModel);
        public delegate void OnSuccessProvider(ProviderModel providerModel);
        public delegate void OnSuccessPublicSave(PublicSaveModel publicSave = null);
        public delegate void OnSuccessPublicSaves(List<PublicSaveModel> publicSaves);
        public delegate void OnSuccessConfig(string value);

        public FacebookUser FbUser = null;
        public List<IFollower> FbFriends = new List<IFollower>();

        public static bool IsFacebookConnectedGlobal = false;
        public static bool IsGameCenterConnectedGlobal = false;

        private bool isFacebookConnected = false;
        public bool IsFacebookConnected
        {
            get { return isFacebookConnected; }
            set
            {
                isFacebookConnected = value;
                IsFacebookConnectedGlobal = value;
            }
        }

        private bool isGameCenterConnected = false;
        public bool IsGameCenterConnected
        {
            get { return isGameCenterConnected; }
            set
            {
                isGameCenterConnected = value;
                IsGameCenterConnectedGlobal = value;
            }
        }
        #endregion

        public bool DEBUG;
        public bool HasInternetDebug;

        #region Models
        private bool pendingConnection = false;
        public bool PendingConnection
        {
            get { return pendingConnection; }
            private set { pendingConnection = value; }
        }

        public class UserSaveItem
        {
            public string ProviderID;
            public int Level;
            public string SaveID;
            public UserModel.Providers Provider;
        }

        public class FacebookUser : BaseFollower
        {
            public override PublicSaveModel GetSave()
            {
                return new PublicSaveModel();
            }

            public virtual string Preety()
            {
                return "Name: " + name + ", Avatar: " + avatar + ", Avatar URL: " + avatarURL;
            }
        }

        public class FacebookFriend : FacebookUser
        {
            public override PublicSaveModel GetSave()
            {
                return save ?? new PublicSaveModel();
            }

            public override string Preety()
            {
                return base.Preety() + ", FacebookID: " + FacebookID + ", SaveID: " + SaveID + ", Level: " + Level;
            }

            public override Sprite GetFrame()
            {
                return ResourcesHolder.Get().frameData.facebokFrame;
            }
        }
        #endregion

        public UserModel UserModel = null;
        public Save SaveModel = null;
        public ProviderModel ProviderModel = null;
        public static string FacebookID;

        #region Facebook
        public void AutoConnectToFacebook(string facebookID = null)
        {
            AutoConnectFacebookUseCase autoConnectFacebookUseCase = new AutoConnectFacebookUseCase();
            autoConnectFacebookUseCase.Execute(facebookID, (user) =>
            {
                NotifyFacebookConnectionState(true, SocialEntryPoint.Auto);
            }, (friends) =>
            {
                UpdateFacebookFriends(friends);
            }, (ex) =>
            {
                NotifyFacebookConnectionState(false, SocialEntryPoint.Auto);
                UpdateFacebookFriends(new List<IFollower>());
                Debug.Log(ex.Message);
            });
        }

        public void ToggleFacebookStatus()
        {
            if (IsFacebookConnected)
                DisconnectFromFacebook(SocialEntryPoint.Settings);
            else
                ConnectToFacebook(false, SocialEntryPoint.Settings);
        }

        public void ForceConnectToFacebook(UserSaveItem Item)
        {
            if (IsFacebookConnected)
                return;

            UIController.get.LoadingPopupController.Open(0, 100, 5);
            ConnectFacebookUseCase connectFacebookUseCase = new ConnectFacebookUseCase();
            connectFacebookUseCase.Execute((save) =>
            {
                NotifyFacebookConnectionState(true, SocialEntryPoint.ChooseHospital);
                FirstConnectToFacebook();
            }, (saveProviderItem) =>
            {
            }, (userData) =>
            {
                UpdateFacebookUserData(userData);
            }, (friends) =>
            {
                UpdateFacebookFriends(friends);
            }, (ex) =>
            {
                NotifyFacebookConnectionState(false, SocialEntryPoint.ChooseHospital);
                Debug.LogError(ex.Message);
            }, true, Item);
        }

        public void ConnectToFacebook(bool openAddFriendsDialog = false, SocialEntryPoint entryPoint = 0)
        {
            if (ReferenceHolder.GetHospital().dailyQuestController.CurrentDailyHasTaskType(DailyTask.DailyTaskType.ConnectToFacebook) && ReferenceHolder.GetHospital().dailyQuestController.CurrentDailyProgressGoalForTaskType(DailyTask.DailyTaskType.ConnectToFacebook) == 1)
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ConnectToFacebook));

            SetPendingConnection(true);
            if (IsFacebookConnected)
            {
                SetPendingConnection(false);
                return;
            }
            ConnectFacebookUseCase connectFacebookUseCase = new ConnectFacebookUseCase();
            connectFacebookUseCase.Execute((save) =>
            {
                SetPendingConnection(false);
                NotifyFacebookConnectionState(true, entryPoint);
                FirstConnectToFacebook();
                if (openAddFriendsDialog)
                {
                    ConnectToFacebook(true, SocialEntryPoint.AddFriends);
                }
            }, (saveProviderItem) =>
            {
                SetPendingConnection(false);
                NotifyFacebookConnectionState(false, entryPoint);
                ShowAccountChoose(saveProviderItem);
            }, (userData) =>
            {
                SetPendingConnection(false);
                UpdateFacebookUserData(userData);
            }, (friends) =>
            {
                SetPendingConnection(false);
                UpdateFacebookFriends(friends);
            }, (ex) =>
            {
                SetPendingConnection(false);
                NotifyFacebookConnectionState(false, entryPoint);
                Debug.LogError(ex.Message);
            });
        }

        private void ShowAccountChoose(UserSaveItem saveProviderItem)
        {
            StartCoroutine(UIController.get.chooseAccountPopUp.Open(saveProviderItem));
        }

        private void UpdateFacebookFriends(List<IFollower> friends)
        {
            FbFriends = friends;
            FriendsDataZipper.ZipFbInGameFriends();
            Game.Instance.gameState().SetHasAnyFriends(friends.Count > 0);
            updateGiftsBadge?.Invoke();
            if (friends.Count > 0)
                NotifyFacebookFriendsChanged();
        }

        private void UpdateFacebookUserData(FacebookUser user)
        {
            NotifyFacebookUserDataChanged();
        }

        public void DisconnectFromFacebook(SocialEntryPoint entryPoint)
        {
            if (!IsFacebookConnected)
                return;

            DisconnectFromFacebookUseCase disconnectFromFacebookUseCase = new DisconnectFromFacebookUseCase();
            disconnectFromFacebookUseCase.Execute(() =>
            {
                NotifyFacebookConnectionState(false, entryPoint);
            }, (ex) =>
            {
                NotifyFacebookConnectionState(true, entryPoint);
                Debug.LogError(ex.Message);
            });
        }

        public void SetPendingConnection(bool connecting)
        {
            PendingConnection = connecting;
        }

        private void NotifyFacebookConnectionState(bool IsConnected, SocialEntryPoint entryPoint)
        {
            IsFacebookConnected = IsConnected;
            Debug.Log("NotifyFacebookConnectionState: " + (IsConnected ? "connected" : "dissconneced"));
            OnFacebookStateUpdate?.Invoke();

            if (IsConnected)
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Connect, entryPoint, SocialServiceType.Facebook);
            else
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Disconnect, entryPoint, SocialServiceType.Facebook);
        }

        private void NotifyFacebookFriendsChanged()
        {
            OnFacebookFriendsUpdate?.Invoke();
        }

        private void NotifyFacebookUserDataChanged()
        {
            Debug.Log("NotifyFacebookUserDataChanged");
            OnFacebookUserDataUpdate?.Invoke();
        }

        private void FirstConnectToFacebook()
        {
            OnFacebookConnected?.Invoke();
            if (UIController.getHospital != null)
                UIController.getHospital.connectFBPopup.OnFacebookConnected();
        }
        #endregion

        #region GameCenter
        public void AutoConnectToGameCenter(string GameCenterID)
        {
            AutoConnectGameCenterUseCase autoConnectGameCenterUseCase = new AutoConnectGameCenterUseCase();
            autoConnectGameCenterUseCase.Execute((save) =>
            {
                NotifyGameCenterConnectionState(true, SocialEntryPoint.Auto);
                Debug.LogError("success auto gamecenter connect");
            }, (item) =>
            {
            }, (ex) =>
            {
                NotifyGameCenterConnectionState(false, SocialEntryPoint.Auto);
                Debug.LogError(ex.Message);
            }, false, null, GameCenterID);
            // TODO
            Debug.LogError("Auto Logging To GameCenter Functionality");
        }

        public void ToggleGameCenterStatus()
        {
            if (IsGameCenterConnected)
                DisconnectFromGameCenter(SocialEntryPoint.Settings);
            else
                ConnectToGameCenter(SocialEntryPoint.Settings);
        }

        public void ForceConnectToGameCenter(UserSaveItem Item)
        {
            if (IsGameCenterConnected)
                return;

            UIController.get.LoadingPopupController.Open(0, 100, 5);
            ConnectGameCenterUseCaseImpl connectGameCenterUseCase = new ConnectGameCenterUseCaseImpl();
            connectGameCenterUseCase.Execute((save) =>
            {
                NotifyGameCenterConnectionState(true, SocialEntryPoint.ChooseHospital);
                Debug.LogError("success gamecenter connect");
            }, (saveProviderItem) =>
            {
            }, (ex) =>
            {
                NotifyGameCenterConnectionState(false, SocialEntryPoint.ChooseHospital);
                Debug.LogError(ex.Message);
            }, true, Item);
        }

        public void ConnectToGameCenter(SocialEntryPoint entryPoint)
        {
            if (IsGameCenterConnected)
                return;

            ConnectGameCenterUseCaseImpl connectGameCenterUseCase = new ConnectGameCenterUseCaseImpl();
            connectGameCenterUseCase.Execute((save) =>
            {
                NotifyGameCenterConnectionState(true, entryPoint);
                Debug.LogError("success gamecenter connect");
            }, (saveProviderItem) =>
            {
                Debug.LogError("<color=red> > > > OnChooseAccount: </color> " + saveProviderItem.SaveID + " > > " + saveProviderItem.Provider + " > > " + saveProviderItem.Level);
                Debug.LogError("UserID: " + AccountManager.instance.UserModel.ID);
                NotifyGameCenterConnectionState(false, entryPoint);
                ShowAccountChoose(saveProviderItem);
            }, (ex) =>
            {
                NotifyGameCenterConnectionState(false, entryPoint);
                Debug.LogError(ex.Message);
            });
        }

        public void DisconnectFromGameCenter(SocialEntryPoint entryPoint)
        {
            if (!IsGameCenterConnected)
                return;

            DisconnectFromGameCenterUseCase disconnectFromGameCenterUseCase = new DisconnectFromGameCenterUseCase();
            disconnectFromGameCenterUseCase.Execute(() =>
            {
                NotifyGameCenterConnectionState(false, entryPoint);
            }, (ex) =>
            {
                NotifyGameCenterConnectionState(true, entryPoint);
                Debug.LogError(ex.Message);
            });
        }

        private void NotifyGameCenterConnectionState(bool IsConnected, SocialEntryPoint entryPoint)
        {
            IsGameCenterConnected = IsConnected;
            OnGameCenterStateUpdate?.Invoke();

            if (IsConnected)
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Connect, entryPoint, SocialServiceType.GameCenter);
            else
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Disconnect, entryPoint, SocialServiceType.GameCenter);
        }

        public void ShowAchievements()
        {
            if (Social.localUser.authenticated)
                Social.ShowAchievementsUI();
            else
                ConnectToGameCenter(SocialEntryPoint.Achievements);
        }

        public void UpdateAchievements(List<Achievement> AchievementList)
        {
            if (AreaMapController.Map.VisitingMode)
                return;

            if (Social.localUser.authenticated)
            {
                Debug.Log("user authenticated, updating achievements!");
                int count = AchievementList.Count;

                for (int i = 0; i < count; i++)
                {
                    if (AchievementList[i].stage == 0)
                    {
                        UpdateAchievement(AchievementList[i].achievementInfo, 0, AchievementList[i].progress / AchievementList[i].achievementInfo.requiredValues[0]);
                    }
                    else if (AchievementList[i].stage == 1)
                    {
                        UpdateAchievement(AchievementList[i].achievementInfo, 0, 100.0);
                        UpdateAchievement(AchievementList[i].achievementInfo, 1, AchievementList[i].progress / AchievementList[i].achievementInfo.requiredValues[1]);
                    }
                    else if (AchievementList[i].stage == 2)
                    {
                        UpdateAchievement(AchievementList[i].achievementInfo, 0, 100.0);
                        UpdateAchievement(AchievementList[i].achievementInfo, 1, 100.0);
                        UpdateAchievement(AchievementList[i].achievementInfo, 2, AchievementList[i].progress / AchievementList[i].achievementInfo.requiredValues[2]);
                    }
                }
            }
        }

        private void UpdateAchievement(AchievementInfo achievementInfo, int stage, double progress)
        {
            string achievementID = "";
#if UNITY_IPHONE
            achievementID = achievementInfo.gameCenterIds[stage];
#else
            achievementID = achievementInfo.googlePlayIds[stage];
#endif
            if (progress < 0.0)
                progress = 0.0;
            else if (progress > 100.0)
                progress = 100.0;

            Social.ReportProgress(achievementID, progress, result =>
            {
                if (result)
                    Debug.Log("Successfully reported achievement progress " + achievementID + ", " + progress);
                else
                    Debug.Log("Failed to report achievement " + achievementID);
            });
        }
        #endregion


        #region Google Play Games
        public void AutoConnectToGPG(string GameCenterID)
        {
            AutoConnectGPGUseCase autoConnectGPGUseCase = new AutoConnectGPGUseCase();
            autoConnectGPGUseCase.Execute((save) =>
            {
                NotifyGPGConnectionState(true, SocialEntryPoint.Auto);
                Debug.LogError("Successfull Auto Google Play Games connection");
            }, (item) =>
            {
            }, (ex) =>
            {
                NotifyGPGConnectionState(false, SocialEntryPoint.Auto);
                Debug.LogError(ex.Message);
            }, false, null, GameCenterID);
            // TODO
            Debug.LogError("Auto Logging To Google Play Games Functionality");
        }

        public void ToggleGPGStatus()
        {
            if (IsGameCenterConnected)
                DisconnectFromGPG(SocialEntryPoint.Settings);
            else
                ConnectToGPG(SocialEntryPoint.Settings);
        }

        public void ForceConnectToGPG(UserSaveItem Item)
        {
            if (IsGameCenterConnected)
                return;

            UIController.get.LoadingPopupController.Open(0, 100, 5);
            ConnectGPGUseCase connectGPGUseCase = new ConnectGPGUseCase();
            connectGPGUseCase.SkipCheck = true;
            connectGPGUseCase.Execute((save) =>
            {
                NotifyGPGConnectionState(true, SocialEntryPoint.ChooseHospital);
                Debug.LogError("Successfull Auto Google Play Games connection");
            }, (saveProviderItem) =>
            {
            }, (ex) =>
            {
                NotifyGPGConnectionState(false, SocialEntryPoint.ChooseHospital);
                Debug.LogError(ex.Message);
            }, true, Item);
        }

        public void ConnectToGPG(SocialEntryPoint entryPoint)
        {
            if (IsGameCenterConnected)
                return;

            ConnectGPGUseCase connectGPGUseCase = new ConnectGPGUseCase();
            connectGPGUseCase.Execute((save) =>
            {
                NotifyGPGConnectionState(true, entryPoint);
                Debug.LogError("Success Google Play Games connect");
            }, (saveProviderItem) =>
            {
                NotifyGPGConnectionState(false, entryPoint);
                ShowAccountChoose(saveProviderItem);
            }, (ex) =>
            {
                NotifyGPGConnectionState(false, entryPoint);
                Debug.LogError(ex.Message);
            });
        }

        public void DisconnectFromGPG(SocialEntryPoint entryPoint)
        {
            if (!IsGameCenterConnected)
                return;

            DisconnectFromGPGUseCase disconnectFromGPGUseCase = new DisconnectFromGPGUseCase();
            disconnectFromGPGUseCase.Execute(() =>
            {
                NotifyGPGConnectionState(false, entryPoint);
                GPGSController.Instance.SignOut();
            }, (ex) =>
            {
                NotifyGPGConnectionState(true, entryPoint);
                Debug.LogError(ex.Message);
            });
        }

        private void NotifyGPGConnectionState(bool IsConnected, SocialEntryPoint entryPoint)
        {
            IsGameCenterConnected = IsConnected;
            OnGPGStateUpdate?.Invoke();

            if (IsConnected)
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Connect, entryPoint, SocialServiceType.GooglePlayGames);
            else
                AnalyticsController.instance.ReportSocialConnect(SocialServiceAction.Disconnect, entryPoint, SocialServiceType.GooglePlayGames);
        }

        public void OverwriteGPGProgress(AccountManager.UserSaveItem localSaveItem, AccountManager.UserSaveItem cloudSaveItem)
        {
            Debug.LogError("> > > OverwriteGPGProgress: -----:          SaveID          |   LEVEL   |   ProviderID");
            Debug.LogError("> > > OverwriteGPGProgress: LOCAL: " + localSaveItem.SaveID + " | " + localSaveItem.Level + " | " + localSaveItem.ProviderID);
            Debug.LogError("> > > OverwriteGPGProgress: CLOUD: " + cloudSaveItem.SaveID + " | " + cloudSaveItem.Level + " | " + cloudSaveItem.ProviderID);

            AccountManager.UserSaveItem newMixedItem = new AccountManager.UserSaveItem()
            {
                Provider = UserModel.Providers.GAMECENTER,
                ProviderID = cloudSaveItem.ProviderID,
                SaveID = AccountManager.instance.UserModel.ID,
                Level = localSaveItem.Level
            };

            ProviderModel newProviderModel = new ProviderModel()
            {
                ProviderID = newMixedItem.ProviderID,
                Level = newMixedItem.Level,
                SaveID = newMixedItem.SaveID
            };
            AccountManager.Instance.ProviderModel = newProviderModel;

            Debug.LogError("> > > OverwriteGPGProgress: newMixedItem: " + newMixedItem.SaveID + " | " + newMixedItem.Level + " | " + newMixedItem.ProviderID);

            ForceConnectToGPG(newMixedItem);
        }

        #endregion Google Play Games


        #region Save & Load

        public void GetCurrentSaveByUserId(string userId, GetCurrentSaveUseCase.OnSuccess onSuccess = null, GetCurrentSaveUseCase.OnChooseAccount onChooseAccount = null, OnFailure onFailure = null)
        {
            getCurrentSaveUseCase = new GetCurrentSaveUseCase();
            getCurrentSaveUseCase.Execute(userId, (save) =>
            {
                onSuccess?.Invoke(save);
            }, (saveProviderItem) =>
            {
                onChooseAccount?.Invoke(saveProviderItem);
                Debug.LogError("TODO: Save Picker Popup");

            }, (ex) =>
            {
                onFailure?.Invoke(ex);
                Debug.LogError(ex.GetType() + " : " + ex.InnerException.GetType() + ":" + ex.Message);
            });
        }
        #endregion

        #region User Model Queries
        public async void GetUserModel(string hash, OnSuccessUserModel onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                var result = await UserConnector.LoadAsync(hash);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void PostUserModel(UserModel userModel, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            Debug.Log(userModel.Preety());
            try
            {
                await UserConnector.SaveAsync(userModel);
                Debug.Log("Saved user model");
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Error saving user model: " + e.Message);
                onFailure?.Invoke(e);
            }
        }
        #endregion

        #region Public Save Queries
        public async void GetPublicSaves(List<string> list, OnSuccessPublicSaves onSuccess, OnFailure onFailure)
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < list.Count; ++i)
            {
                if (!ids.Contains(list[i]))
                    ids.Add(list[i]);
            }
            if (ids.Count == 0)
            {
                onSuccess?.Invoke(new List<PublicSaveModel>());
                return;
            }

            try
            {
                var result = await PublicSaveConnector.BatchGetAsync(ids);
                if (result == null)
                    onSuccess?.Invoke(new List<PublicSaveModel>());
                else
                    onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void PostPublicSave(PublicSaveModel publicSaveModel, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                await PublicSaveConnector.SaveAsync(publicSaveModel);
                Debug.Log("Saved public save model");
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Error saving public save model: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void GetPublicSave(string hash, OnSuccessPublicSave onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                var result = await PublicSaveConnector.LoadAsync(hash);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }
        #endregion

        #region Save Queries
        public async void GetSave(string hash, OnSuccessSave onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
#if UNITY_EDITOR
                if (PlayerPrefs.GetInt("UseCloud") != -1)
                {
                    var result = await SaveConnector.LoadAsync(hash);
                    onSuccess?.Invoke(result);
                }
                else
                {
                    Save save = CacheManager.GetSaveFromCache();
                    onSuccess?.Invoke(save);
                }
#else
                    var result = await SaveConnector.LoadAsync(hash);
                    onSuccess?.Invoke(result);
#endif
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }
        #endregion

        #region Provider Queries
        public async void GetProvider(string hash, OnSuccessProvider onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                var result = await ProviderConnector.LoadAsync(hash);
                if (result != null)
                {
                    GetPublicSave(result.SaveID, (save) =>
                    {
                        result.Level = save == null ? 1 : save.Level;
                        onSuccess?.Invoke(result);
                    }, onFailure);
                }
                else
                {
                    onSuccess?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void PostProvider(ProviderModel providerModel, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                await ProviderConnector.SaveAsync(providerModel);
                Debug.Log("Saved provider model");
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Error saving provider model: " + e.Message);
                onFailure?.Invoke(e);
            }
        }
        #endregion

        #region Facebook Queries
        public async void FindFriendsSaves(List<IFollower> friends, OnSuccessFriends onSuccess, OnFailure onFailure)
        {
            try
            {
                var result = await ProviderConnector.BatchGetAsync(friends);
                if (result == null)
                    onSuccess?.Invoke(new List<ProviderModel>());
                else
                    onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
                Debug.LogError("Could not get provider batch:" + e.Message);
            }
        }
        #endregion

        #region Option Queries
        //        public async void GetVersion(OnSuccessConfig onSuccess = null, OnFailure onFailure = null)
        //        {
        //            try
        //            {
        //                var result = await ConfigConnector.LoadAsync("version");
        //#if UNITY_ANDROID
        //                onSuccess?.Invoke(result == null ? null : result.ValueAndroid);
        //#else
        //                onSuccess?.Invoke(result == null ? null : result.Value);
        //#endif
        //            }
        //            catch (Exception e)
        //            {
        //                onFailure?.Invoke(e);
        //            }
        //        }
        #endregion

        #region FBUtils
        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public void DownloadFacebookAvatar(IFollower friend, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            Sprite avatar = CacheManager.GetAvatar(friend.FacebookID);
            if (avatar == null)
                StartCoroutine(TryDownloadAvatar(friend, onSuccess, onFailure));
            else
            {
                friend.Avatar = avatar;
                friend.SetIsFacebookAvatarDownloaded(true);
                onSuccess?.Invoke();
            }
        }

        private IEnumerator TryDownloadAvatar(IFollower friend, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            Texture2D texture = new Texture2D(100, 100, TextureFormat.DXT1, false);
            using (UnityWebRequest request = UnityWebRequest.Get(friend.AvatarURL))
            {
                yield return request.SendWebRequest();
                texture.LoadImage(request.downloadHandler.data);
            }
            friend.Avatar = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
            friend.SetIsFacebookAvatarDownloaded(true);
            onSuccess?.Invoke();
        }
        #endregion

        #region InternetConnection
        public static bool HasInternetConnection()
        {
            //if(AccountManager.Instance.DEBUG)
            //{
            //  return AccountManager.Instance.HasInternetDebug;
            //}
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public delegate void ConnectionState(bool isConnected);
        public delegate void InternetConnected();

        public static void HasInternetConnection(ConnectionState connectionStateCallback)
        {
            ReferenceHolder.Get().engine.AddTask(() =>
            {
                connectionStateCallback?.Invoke(HasInternetConnection());
            });
        }

        public static void ProcessIfInternetConnected(InternetConnected internetConnectedCallback)
        {
            HasInternetConnection((isConnected) =>
            {
                if (isConnected)
                    internetConnectedCallback?.Invoke();
                else
                    BaseUIController.ShowInternetConnectionProblemPopup(UIController.get);
            });
        }
        #endregion

        public static bool IsSocialConnected()
        {
            //mod 2025-7-11-15:16
            return false;

            // #if UNITY_IOS
            // 	        return AccountManager.Instance.IsGameCenterConnected;
            // #elif UNITY_ANDROID
            //             return Social.localUser.authenticated;
            // #endif
        }

    }

}
