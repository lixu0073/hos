using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Text;
using UnityEngine.SceneManagement;

namespace Hospital
{

    public class CacheManager : MonoBehaviour
    {

        public interface IGetPublicSave
        {
            string GetSaveID();
        }

        #region Static

        private static CacheManager instance;

        public static CacheManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("There is no CacheManager instance on scene!");
                }
                return instance;
            }
        }


        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of CacheManager on scene!");
            }
            instance = this;
        }

        #endregion

        public delegate void OnSuccess(string login, Sprite avatar);
        public delegate void OnSuccessCallback();
        public delegate void OnSuccessListCallback(List<PublicSaveModel> saves);
        public delegate void OnSuccessSaveGet(PublicSaveModel save);

        public static Dictionary<string, KeyValuePair<string, Sprite>> users = new Dictionary<string, KeyValuePair<string, Sprite>>();
        public static Dictionary<string, PublicSaveModel> publicSaves = new Dictionary<string, PublicSaveModel>();

        private const string TREATMENT_ROOM_HELPERS_TAG = "treatment_room_helpers";
        private const string SOCIAL_NOTIFICATIONS_ON_TAG = "social_notifications_on";
        private const string IAP_TRANSACTIONS_TAG = "iap_transactions";
        private const string VITAMIN_REFILL_POPUP = "vitamin_refill_popup";

        #region Player Prefs

        public static void SaveIAPTransactionData(string TransactionID, string data)
        {
            PlayerPrefs.SetString(GetIAPTransactionKey(TransactionID), data);
            PlayerPrefs.Save();
        }

        public static string GetIAPTransactionData(string TransactionID)
        {
            return PlayerPrefs.GetString(GetIAPTransactionKey(TransactionID), null);
        }

        public static void RemoveIAPTransactionData(string TransactionID)
        {
            PlayerPrefs.DeleteKey(TransactionID);
            PlayerPrefs.Save();
        }

        private static string GetIAPTransactionKey(string TransactionID)
        {
            return IAP_TRANSACTIONS_TAG + CognitoEntry.SaveID + TransactionID;
        }

        public static void SetSocialNotifications(bool on)
        {
            PlayerPrefs.SetInt(SOCIAL_NOTIFICATIONS_ON_TAG + CognitoEntry.SaveID, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool IsSocialNotificationsOn()
        {
            return PlayerPrefs.GetInt(SOCIAL_NOTIFICATIONS_ON_TAG + CognitoEntry.SaveID, 1) == 1;
        }

        public static void AddToHelpers(string key)
        {
            PlayerPrefs.SetInt(TREATMENT_ROOM_HELPERS_TAG + CognitoEntry.SaveID + key, 1);
            PlayerPrefs.Save();
        }

        public static bool WasHelped(string key)
        {
            return PlayerPrefs.GetInt(TREATMENT_ROOM_HELPERS_TAG + CognitoEntry.SaveID + key, 0) == 1;
        }

        public static bool HasRefillVitaminPopupSeen(string vitamin)
        {
            return PlayerPrefs.GetInt(VITAMIN_REFILL_POPUP + CognitoEntry.SaveID + vitamin, 0) == 1;
        }

        public static void PlayerHasSeenRefillVitaminPopup(string vitamin)
        {
            PlayerPrefs.SetInt(VITAMIN_REFILL_POPUP + CognitoEntry.SaveID + vitamin, 1);
        }

        public static void TryToSaveFirstLaunchDate(string cognito)
        {
            if (string.IsNullOrEmpty(GetFirstLaunchDate(cognito)))
            {
                PlayerPrefs.SetString("first_launch_date" + cognito, DateTime.UtcNow.ToString());
                PlayerPrefs.Save();
            }
        }

        public static string GetFirstLaunchDate(string cognito = null)
        {
            if (string.IsNullOrEmpty(cognito))
            {
                cognito = CognitoEntry.UserID;
            }
            return PlayerPrefs.GetString("first_launch_date" + cognito, null);
        }

        public static bool IsOfferBought(PharmacyOrder order)
        {
            int bought = PlayerPrefs.GetInt(GetOfferKey(order));
            return bought == 1;
        }

        public static void SetOfferBought(PharmacyOrder order)
        {
            PlayerPrefs.SetInt(GetOfferKey(order), 1);
            PlayerPrefs.Save();
        }

        public static bool IsOffersMigrationComplete()
        {
            return PlayerPrefs.GetInt("offers_migration_complete" + CognitoEntry.SaveID, 0) == 1;
        }

        public static void SetOffersMigrationComplete()
        {
            PlayerPrefs.SetInt("offers_migration_complete" + CognitoEntry.SaveID, 1);
            PlayerPrefs.Save();
        }

        public static void SetLastGlobalEventContribution(int contrib = 0) //Complete amount of contribution.
        {
            //PlayerPrefs.SetInt("last_global_event_contribution" + CognitoEntry.saveID, contrib);
            //PlayerPrefs.Save();

            ObscuredPrefs.SetInt("last_global_event_contribution" + CognitoEntry.SaveID, contrib);
            ObscuredPrefs.Save();
        }

        public static int GetLastGlobalEventContribution() //Complete amount of contribution.
        {
            return ObscuredPrefs.GetInt("last_global_event_contribution" + CognitoEntry.SaveID, 0);
            //return PlayerPrefs.GetInt("last_global_event_contribution" + CognitoEntry.saveID, 0);
        }

        public static void SetPrevGlobalEventId(string globalEventId)
        {
            //PlayerPrefs.SetString("global_event_prev_id" + CognitoEntry.saveID, globalEventId);
            //PlayerPrefs.Save();

            ObscuredPrefs.SetString("global_event_prev_id" + CognitoEntry.SaveID, globalEventId);
            ObscuredPrefs.Save();
        }

        /// <summary>
        /// Get Previous Global Event ID. 
        /// </summary>
        /// <returns>Previous GE ID</returns>
        public static string GetPrevGlobalEventId()
        {
            //return PlayerPrefs.GetString("global_event_prev_id" + CognitoEntry.saveID, "");
            return ObscuredPrefs.GetString("global_event_prev_id" + CognitoEntry.SaveID, "");
        }

        private static string GetOfferKey(PharmacyOrder order)
        {
            return (order.ID + order.UserID + CognitoEntry.SaveID).Trim();
        }

        public static void ClearEventDisplayedPref(StandardEventGeneralData gameEvent)
        {
            PlayerPrefs.DeleteKey(GetGameEventKey(gameEvent));
        }

        public static void SetEventDisplayed(StandardEventGeneralData gameEvent, bool isDisplayed)
        {
            PlayerPrefs.SetInt(GetGameEventKey(gameEvent), isDisplayed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool IsEventDisplayed(StandardEventGeneralData gameEvent)
        {
            return PlayerPrefs.GetInt(GetGameEventKey(gameEvent)) == 1;
        }

        public static string GetGameEventKey(StandardEventGeneralData gameEvent)
        {
            return gameEvent.EventID + gameEvent.startTime.ToString();
        }

        public static bool IsGameEventDisplayedCached(StandardEventGeneralData gameEvent)
        {
            return PlayerPrefs.HasKey(GetGameEventKey(gameEvent));
        }

        #endregion

        #region Update Help Requests

        public static void UpdatePlantationHelpRequest(string SaveID, bool HasHelpRequest)
        {
            if (publicSaves.ContainsKey(SaveID))
            {
                publicSaves[SaveID].PlantationHelp = HasHelpRequest;
            }
            UpdatePersonHelpRequestStatus(SaveID, HasHelpRequest, HelpSource.plantation);
        }

        public static void UpdateEpidemyHelpRequest(string SaveID, bool HasHelpRequest)
        {
            if (publicSaves.ContainsKey(SaveID))
            {
                publicSaves[SaveID].EpidemyHelp = HasHelpRequest;
            }
            UpdatePersonHelpRequestStatus(SaveID, HasHelpRequest, HelpSource.epidemy);
        }

        public static void UpdateTreatmentHelpRequest(string SaveID, bool HasHelpRequest)
        {
            if (publicSaves.ContainsKey(SaveID))
            {
                publicSaves[SaveID].TreatmentHelp = HasHelpRequest;
            }
            UpdatePersonHelpRequestStatus(SaveID, HasHelpRequest, HelpSource.treatment);
        }

        private static void UpdatePersonHelpRequestStatus(string SaveID, bool HasHelpRequest, HelpSource source)
        {
            if (FriendsController.Instance.followersList != null)
            {
                foreach (IFollower user in FriendsController.Instance.followersList)
                {
                    if (user.GetSaveID().Equals(SaveID))
                    {
                        switch (source)
                        {
                            case HelpSource.plantation:
                                user.HasPlantationHelpRequest = HasHelpRequest;
                                break;
                            case HelpSource.epidemy:
                                user.HasEpidemyHelpRequest = HasHelpRequest;
                                break;
                            case HelpSource.treatment:
                                user.HasTreatmentHelpRequest = HasHelpRequest;
                                break;
                            default:
                                break;
                        }

                        if (user is BaseFollower)
                        {
                            ((BaseFollower)user).NotifyOnHelpRequestStatusChanged(user.HasPlantationHelpRequest, user.HasEpidemyHelpRequest, user.HasTreatmentHelpRequest);
                        }
                    }
                }
            }

            foreach (BaseUserModel userModel in LastHelpersSynchronizer.Instance.data)
            {
                if (userModel.GetSaveID().Equals(SaveID))
                {
                    switch (source)
                    {
                        case HelpSource.plantation:
                            userModel.HasPlantationHelpRequest = HasHelpRequest;
                            break;
                        case HelpSource.epidemy:
                            userModel.HasEpidemyHelpRequest = HasHelpRequest;
                            break;
                        case HelpSource.treatment:
                            userModel.HasTreatmentHelpRequest = HasHelpRequest;
                            break;
                        default:
                            break;
                    }

                    if (userModel is BaseUserModel)
                    {
                        ((BaseUserModel)userModel).NotifyOnHelpRequestStatusChanged(userModel.IsPlantationHelpRequested(), userModel.IsEpidemyHelpRequested(), userModel.IsTreatmentHelpRequested());
                    }
                }
            }

            if (FriendsController.Instance.likedFollowers != null)
            {
                foreach (IFollower user in FriendsController.Instance.likedFollowers)
                {
                    if (user.GetSaveID().Equals(SaveID))
                    {
                        switch (source)
                        {
                            case HelpSource.plantation:
                                user.HasPlantationHelpRequest = HasHelpRequest;
                                break;
                            case HelpSource.epidemy:
                                user.HasEpidemyHelpRequest = HasHelpRequest;
                                break;
                            case HelpSource.treatment:
                                user.HasTreatmentHelpRequest = HasHelpRequest;
                                break;
                            default:
                                break;
                        }
                        if (user is BaseFollower)
                        {
                            ((BaseFollower)user).NotifyOnHelpRequestStatusChanged(user.HasPlantationHelpRequest, user.HasEpidemyHelpRequest, user.HasTreatmentHelpRequest);
                        }
                    }
                }
            }

            if (AccountManager.Instance.FbFriends != null)
            {
                foreach (AccountManager.FacebookFriend user in AccountManager.Instance.FbFriends)
                {
                    if (user.GetSaveID().Equals(SaveID))
                    {
                        switch (source)
                        {
                            case HelpSource.plantation:
                                user.HasPlantationHelpRequest = HasHelpRequest;
                                break;
                            case HelpSource.epidemy:
                                user.HasEpidemyHelpRequest = HasHelpRequest;
                                break;
                            case HelpSource.treatment:
                                user.HasTreatmentHelpRequest = HasHelpRequest;
                                break;
                            default:
                                break;
                        }
                        user.NotifyOnHelpRequestStatusChanged(user.IsPlantationHelpRequested(), user.IsEpidemyHelpRequested(), user.IsTreatmentHelpRequested());
                    }
                }
            }
        }

        #endregion

        #region Facebook Avatars Cache Manager

        public delegate void OnFBDataUpdate(string facebookID, string login, Sprite avatar);
        public static event OnFBDataUpdate onFBDataUpdate;

        public static Sprite GetAvatar(string facebookID)
        {
            if (users.ContainsKey(facebookID))
            {
                return users[facebookID].Value;
            }
            return null;
        }

        #endregion

        #region PublicSave API

        public delegate void OnPublicSaveUpdate(PublicSaveModel model);
        public static event OnPublicSaveUpdate onPublicSaveUpdate;

        public static void GetPublicSaveById(string SaveID, OnSuccessSaveGet onSuccess, OnFailure onFailure, bool forceGetFromServer = false)
        {
            if (!forceGetFromServer)
            {
                if (publicSaves.ContainsKey(SaveID))
                {
                    if (onSuccess != null)
                    {
                        onSuccess.Invoke(publicSaves[SaveID]);
                        return;
                    }
                }
            }
            AccountManager.Instance.GetPublicSave(SaveID, (save) =>
            {
                if (save == null)
                {
                    onFailure?.Invoke(new Exception("public_save_not_found"));
                    return;
                }

                if (!publicSaves.ContainsKey(save.SaveID))
                    publicSaves.Add(save.SaveID, save);
                onSuccess?.Invoke(save);
                onPublicSaveUpdate?.Invoke(save);
            }, onFailure);
        }

        public static void BatchPublicSavesWithResults(List<IGetPublicSave> list, OnSuccessListCallback onSuccess, OnFailure onFailure, bool forceRequestServer = false)
        {
            BatchPublicSaves(list, () =>
            {
                List<PublicSaveModel> saves = new List<PublicSaveModel>();
                foreach (IGetPublicSave iSave in list)
                {
                    if (publicSaves.ContainsKey(iSave.GetSaveID()))
                    {
                        saves.Add(publicSaves[iSave.GetSaveID()]);
                    }
                }
                onSuccess?.Invoke(saves);
            }, onFailure, forceRequestServer);
        }

        public static void BatchPublicSaves(List<IGetPublicSave> list, OnSuccessCallback onSuccess, OnFailure onFailure, bool forceRequestServer = false)
        {
            List<string> idsToGet = new List<string>();
            foreach (IGetPublicSave obj in list)
            {
                if (!idsToGet.Contains(obj.GetSaveID()) && (!publicSaves.ContainsKey(obj.GetSaveID()) || forceRequestServer) && !string.IsNullOrEmpty(obj.GetSaveID()))
                {
                    idsToGet.Add(obj.GetSaveID());
                }
            }
            if (idsToGet.Count == 0)
            {
                onSuccess?.Invoke();
                return;
            }
            AccountManager.Instance.GetPublicSaves(idsToGet, (saves) =>
            {
                foreach (PublicSaveModel save in saves)
                {
                    if (!publicSaves.ContainsKey(save.SaveID))
                    {
                        publicSaves.Add(save.SaveID, save);
                    }
                    else
                    {
                        publicSaves[save.SaveID] = save;
                    }
                }
                onSuccess?.Invoke();
                foreach (PublicSaveModel save in saves)
                {
                    onPublicSaveUpdate.Invoke(save);
                }
            }, onFailure);
        }

        public static void ForceBatchPublicSaves(List<IGetPublicSave> list, OnSuccessCallback onSuccess, OnFailure onFailure)
        {

        }

        public static void GetUserDataByFacebookID(string facebookID, OnSuccess onSuccess, OnFailure onFailure)
        {
            if (users.ContainsKey(facebookID))
            {
                onSuccess?.Invoke(users[facebookID].Key, users[facebookID].Value);
                return;
            }
            GetFacebookUserDataUseCase getFacebookUserDataUseCase = new GetFacebookUserDataUseCase();
            getFacebookUserDataUseCase.Execute((user) =>
            {
                if (!users.ContainsKey(facebookID))
                {
                    users.Add(facebookID, new KeyValuePair<string, Sprite>(user.Name, user.Avatar));
                }
                onSuccess?.Invoke(user.Name, user.Avatar);
                onFBDataUpdate?.Invoke(facebookID, user.Name, user.Avatar);
            }, onFailure, facebookID);
        }

        #endregion

        #region Save Cache

        private string RESET_LEVEL_TAG = "reset_lvl";

        public bool HasUserUpFirstLevel(string hash)
        {
            return PlayerPrefs.GetInt(RESET_LEVEL_TAG + hash, 0) == 1;
        }

        public void SetUserHasUpFirstLevel(string hash)
        {
            PlayerPrefs.SetInt(RESET_LEVEL_TAG + hash, 1);
            PlayerPrefs.Save();
        }

        public static void CacheSave(bool fromPause)
        {
            if (AreaMapController.Map.IsMapEmpty())
            {
                return;
            }

            var Id = CognitoEntry.SaveID;

            if (AreaMapController.HomeMapLoaded)
            {
                // WARNING: Previous version created a copy of the save and merged data into it.
                // This version fetches the cached save DIRECTLY.
                var p = SaveCacheSingleton.GetCachedSave();
                AreaMapController.Map.SaveGame(p);
                if (BaseGameState.saveFilePrepared)
                {
                    if (fromPause && p.Level == 0)
                    {
                        AnalyticsController.instance.ReportBug("from_pause_level_zero_1");
                        return;
                    }

                    if (fromPause && string.IsNullOrEmpty(p.version))
                    {
                        AnalyticsController.instance.ReportBug("empty_game_version_pause_error");
                        return;
                    }

                    SaveLoadController.SaveGame(p, Id);
                    GlobalData.Instance().HomeSave = p;
                }
            }
            else if (VisitingController.Instance.IsVisiting)
            {
                var locSave = VisitingController.Instance.LocalSave;
                locSave.ID = Id;

                AreaMapController.Map.SaveGameInVisigingMode(locSave);
                if (BaseGameState.saveFilePrepared)
                {
                    if (fromPause && locSave.Level == 0)
                    {
                        AnalyticsController.instance.ReportBug("from_pause_level_zero_2");
                        return;
                    }

                    if (fromPause && string.IsNullOrEmpty(locSave.version))
                    {
                        AnalyticsController.instance.ReportBug("empty_game_version_pause_error");
                        return;
                    }

                    SaveLoadController.SaveGame(locSave, Id);
                    GlobalData.Instance().HomeSave = locSave;
                }
            }
        }

        public static Save GetSaveFromCache()
        {
            return SaveLoadController.Get().LoadSaveFromFile(CognitoEntry.SaveID);
        }

        #endregion

        public static void SaveConfigDataInCache(string key, string data)
        {
            ObscuredString tmp = ObscuredString.EncryptDecrypt(data, key);
            string encrypted = tmp.GetEncrypted();
            ObscuredPrefs.SetString(key, encrypted);
            ObscuredPrefs.Save();
        }

        public static string GetConfigDataFromCache(string key)
        {
            ObscuredString cryptedSave = ObscuredString.EncryptDecrypt(ObscuredPrefs.GetString(key), key);

            if (string.IsNullOrEmpty(cryptedSave))
                return null;

            return cryptedSave.GetEncrypted();

        }

        public enum HelpSource
        {
            plantation,
            epidemy,
            treatment
        }
    }
}