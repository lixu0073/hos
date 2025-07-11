using System;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;

namespace Hospital
{
    public class CrossPromotionController : MonoBehaviour
    {
        public static CrossPromotionController instance;

        [SerializeField]
        private int diamondReward;

        public event Action CrossPromotionStateChanged;

        // Is the player part of the campaign in DDNA and is the campaign active?
        private bool isInCampaign = false;

        private GetAppInfo appCheck;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                appCheck = new GetAppInfo();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeCampaign(Dictionary<string, object> parameters)
        {
            parameters.TryGetValue("show_cross_promotion", out var parameter);
            isInCampaign = parameter != null ? (bool)parameter : false;

            // If the game state isn't loaded, wait until it's loaded to check if the campaign is completed
            if (Game.Instance == null)
                BaseGameState.Loaded += CheckCompletion;
            else
                CheckCompletion();
        }

        public bool ShouldShowCrossPromotion()
        {
            return isInCampaign && IsLanguageCorrect() && !IsTNTInstalled();
        }

        // Return true if either the system language or the game language is English
        private static bool IsLanguageCorrect()
        {
            if (Application.systemLanguage == SystemLanguage.English)
                return true;
            if (LocalizationManager.CurrentLanguage == "English")
                return true;
            return false;
        }

        private bool IsTNTInstalled()
        {
#if UNITY_IOS
            // For iOS, this is the URL specified in Info.plist
            const string appID = "tilesandtales";
#else
            const string appID = "com.kuuhubb.tilesandtales";
#endif

            return appCheck.CheckInstalledApp(appID);
        }

        private void CheckCompletion()
        {
            BaseGameState.Loaded -= CheckCompletion;

            // DDNA might still think the player is in the campaign even if
            // they've completed it, so we need to check the save
            IGameState gameState = Game.Instance.gameState();
            if (gameState.CrossPromotionCompleted)
                isInCampaign = false;

            if (isInCampaign && IsTNTInstalled())
                Complete();
        }

        private void Complete()
        {
            IGameState gameState = Game.Instance.gameState();

            isInCampaign = false;

            // If player has ever opened the cross-promotion popup, get the diamond reward
            if (gameState.EverOpenedCrossPromotionPopup)
                gameState.AddResource(ResourceType.Diamonds, diamondReward, EconomySource.TNTCrossPromotion, true);

            // This event is set up as the campaign's conversion event, so it automatically ends the campaign.
            // However, it takes some time for the campaign to update, so we also need to save it in the save data.
            AnalyticsController.instance.ReportCrossPromotionConversion();
            gameState.CrossPromotionCompleted = true;
            SaveSynchronizer.Instance.InstantSave();

            CrossPromotionStateChanged?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && Game.Instance != null)
                CheckCompletion();
        }
    }
}