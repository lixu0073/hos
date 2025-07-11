using UnityEngine;

namespace Hospital
{
    public class PublicSaveManager : MonoBehaviour
    {

        private IPublicSaveManager manager = null;
        private BubbleBoyReward bestWonReward = null;
        private PublicSaveModel PublicSaveModel = null;

        #region Static

        private static PublicSaveManager instance;

        public static PublicSaveManager Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("There is no PublicSaveManager instance on scene!");

                return instance;
            }
        }

        #endregion

        #region Mono

        void Awake()
        {
            if (instance != null)
                Debug.LogWarning("There are possibly multiple instances of PublicSaveManager on scene!");

            instance = this;
        }

        void Start()
        {
            manager = ObjectFactory.GetPublicSaveManager();
            if (manager != null)
                manager.Start();

            BaseGameState.OnLevelUp += UpdatePublicSaveForEvent;
            AccountManager.OnFacebookConnected += AccountManager_OnFacebookConnected;

#if UNITY_ANDROID && !UNITY_EDITOR
            GPGSController.Instance.SignInAfterIntro();
#endif
        }

        void OnDestroy()
        {
            if (manager != null)
                manager.OnDestroy();

            BaseGameState.OnLevelUp -= UpdatePublicSaveForEvent;
            AccountManager.OnFacebookConnected -= AccountManager_OnFacebookConnected;
        }

        #endregion

        #region All Scenes

        private void AccountManager_OnFacebookConnected()
        {
            if (AccountManager.Instance.IsFacebookConnected)
            {
                UpdatePublicSave();
            }
        }

        #endregion

        #region MainScene

        public void TryToSaveBestWonReward(BubbleBoyReward reward)
        {
            if (reward is BubbleBoyRewardDecoration)
                return;

            bestWonReward = reward;
            if (PublicSaveModel != null)
            {
                if (!UpdateBestWonItem())
                {
                    return;
                }
                UpdatePublicSave();
            }
            else
            {
                UpdatePublicSave(true);
            }
        }


        private bool UpdateBestWonItem()
        {
            if (PublicSaveModel.BestWonItem == null)
            {
                PublicSaveModel.BestWonItem = bestWonReward;
                return true;
            }
            if (PublicSaveModel.BestWonItem.IsExpired() || bestWonReward > PublicSaveModel.BestWonItem)
            {
                PublicSaveModel.BestWonItem = bestWonReward;
                return true;
            }
            return false;
        }

        #endregion

        private void DownloadPublicSave(bool CheckReward = false)
        {
            if (manager == null)
                return;
            if (string.IsNullOrEmpty(manager.GetSaveID()))
                return;
            if (!manager.CanSave())
                return;

            AccountManager.Instance.GetPublicSave(manager.GetSaveID(), (save) =>
            {
                if (save != null)
                {
                    PublicSaveModel = save;
                    if (CheckReward && !UpdateBestWonItem())
                        return;
                }
                SyncSave();
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
            });
        }

        public void UpdatePublicSaveForEvent()
        {
            UpdatePublicSave();
        }

        public void UpdatePublicSave(bool CheckReward = false)
        {
            if (manager == null)
                return;
            if (string.IsNullOrEmpty(manager.GetSaveID()))
                return;
            if (!manager.CanSave())
                return;

            if (PublicSaveModel == null)
                DownloadPublicSave(CheckReward);
            else
                SyncSave();
        }

        private string GetPlatformName()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#else
            return "unsupported";
#endif
        }

        private string GetPushRegistrationID()
        {
            Debug.LogError("GetPushRegistrationID");
            return "";
            //#if UNITY_IOS
            //            return DDNA.Instance.PushNotificationToken;
            //#elif UNITY_ANDROID
            //            return DDNA.Instance.AndroidRegistrationID;
            //#else
            //            return "";
            //#endif
        }

        private PublicSaveModel GetDefaultSave()
        {
            PublicSaveModel publicSaveModel = null;
            if (PublicSaveModel != null)
                publicSaveModel = PublicSaveModel.Copy();
            else
                publicSaveModel = new PublicSaveModel();

            publicSaveModel.SaveID = manager.GetSaveID();
            return publicSaveModel;
        }

        private void SyncSave()
        {
            PublicSaveModel publicSaveModel = GetDefaultSave();

            publicSaveModel.Level = Game.Instance.gameState().GetHospitalLevel();
            publicSaveModel.Name = Game.Instance.gameState().GetHospitalName();
            publicSaveModel.Platform = GetPlatformName();
            publicSaveModel.Language = I2.Loc.LocalizationManager.CurrentLanguageCode;
            publicSaveModel.NotificationApiKey = GetPushRegistrationID();
            publicSaveModel.lastActivity = (long)ServerTime.getTime();
            publicSaveModel.PushOn = LocalNotificationController.Instance.notificationGroups.IsSocialNotificationsEnabled();
            publicSaveModel.ReputationScores = ReputationSystem.ReputationController.Instance.SaveToString();

            if (manager == null)
                return;

            manager.Save(publicSaveModel, PublicSaveModel);

            if (string.IsNullOrEmpty(publicSaveModel.FacebookID))
            {
                UserModel userModel = AccountManager.Instance.UserModel;
                if (userModel != null && !string.IsNullOrEmpty(userModel.FacebookID))
                    publicSaveModel.FacebookID = userModel.FacebookID;
            }

            // CV: 2.2.0 FB support removed
            string dbg = (PlayerPrefs.GetInt("FB_PREV_BLOCKED") == 0) ? " NO" : "YES";
            Debug.Log("<color=#00f010ff> SyncSave -> FBId: </color>" + publicSaveModel.FacebookID +  "     <color=#f00000ff>Prev shown?: </color>" + dbg);
            if (PlayerPrefs.GetInt("FB_PREV_BLOCKED") == 0 && !string.IsNullOrEmpty(publicSaveModel.FacebookID))
            {
                if (UIController.get.WarningNoFBPopup != null)
                    StartCoroutine(UIController.get.WarningNoFBPopup.Open());

                PlayerPrefs.SetInt("FB_PREV_BLOCKED", 1);
                AccountManager.Instance.IsFacebookConnected = false;
            }

            PublicSaveModel = publicSaveModel;
            AccountManager.Instance.PostPublicSave(publicSaveModel, () =>
            {
                Debug.Log("[PUBLIC SAVE] - success save post");
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
            });
        }

    }
}
