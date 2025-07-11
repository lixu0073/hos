using UnityEngine;
using TMPro;
using SimpleUI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace Hospital
{
    public enum AlertType
    {
        NO_INTERNET_CONNECTION = 1,
        SERVER_CONNECTION_PROBLEM = 2,
        MAINTENANCE = 3,
        CRITICAL_LOCAL = 4,
        NEW_VERSION = 5,
        NON_ACTIVE = 6,
        RECOMPENSATION = 7,
        SAVE_HAS_MORE_RECENT_SAVE_THAN_CLIENT = 8,
        OPTIONAL_RESTART = 9
    };

    public class AlertPopupController : UIElement
    {
        public GameObject normalInfo;
        public GameObject newVersionInfo;
        public TextMeshProUGUI message;
        public TextMeshProUGUI title;
        public TextMeshProUGUI buttonText;
        public Button button;
        public GameObject ExitButton;

#pragma warning disable 0649
        [SerializeField] private GameObject HelpAndSupportButton;
#pragma warning restore 0649
        public AlertType Type { get { return currentType; } }
        private AlertType currentType = 0;

        public IEnumerator Open(AlertType type, string customMessage = null, Exception ex = null, Action whenDone = null)
        {
            AnalyticsController.instance.ReportLoadingProcess(AnalyticsLoadingStep.ErrorAlert, (int)Time.time);

            if (UIController.get != null)
            {
                UIController.get.CloseActiveHover();
                UIController.get.ExitAllPopUps();
            }

            bool switchPopup = type == AlertType.NON_ACTIVE && type != currentType && gameObject.activeSelf;
            if (gameObject.activeSelf && !switchPopup)
                yield break; //return;

            currentType = type;
            if (!switchPopup)
                yield return base.Open(true, true);
            else
                UIController.get.AddPopupFade(this);

            yield return null;

            buttonText.text = I2.Loc.ScriptLocalization.Get("RELOAD");
            button.RemoveAllOnClickListeners();
            button.onClick.AddListener(() => { OnConfirmButtonClick(); });

            normalInfo.SetActive(true);
            newVersionInfo.SetActive(false);
            if (HelpAndSupportButton != null)
            {
                HelpAndSupportButton.SetActive(false);
            }
            if (ExitButton != null)
            {
                ExitButton.SetActive(false);
            }
            switch (type)
            {
                case AlertType.NO_INTERNET_CONNECTION:
                    message.text = I2.Loc.ScriptLocalization.Get("NO_INTERNET_CONNECTION");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_NO_INTERNET_CONNECTION");
                    HandleAmazonRuntimeException(ex);
                    break;
                case AlertType.SERVER_CONNECTION_PROBLEM:
                    message.text = I2.Loc.ScriptLocalization.Get("SERVER_CONNECTION_PROBLEM");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_SERVER_CONNECTION_PROBLEM");
                    HandleAmazonRuntimeException(ex);
                    break;
                case AlertType.MAINTENANCE:
                    message.text = I2.Loc.ScriptLocalization.Get("MAINTENANCE");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_MAINTENANCE");
                    break;
                case AlertType.CRITICAL_LOCAL:
                    message.text = I2.Loc.ScriptLocalization.Get("CRITICAL_ERROR_LOCAL");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_CRITICAL_ERROR_LOCAL");
                    FreezeGame();
                    Debug.LogError("Critical error: " + customMessage);
                    if (ex != null)
                    {
                        Debug.LogError("Exception: " + ex.Message + " \n" + ex.StackTrace);
                    }
                    SetError();
                    if (HelpAndSupportButton != null)
                        HelpAndSupportButton.SetActive(true);
                    break;
                case AlertType.NEW_VERSION:
                    buttonText.text = I2.Loc.ScriptLocalization.Get("UPDATE_NOW");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_NEW_VERSION");
                    normalInfo.SetActive(false);
                    newVersionInfo.SetActive(true);
                    break;
                case AlertType.NON_ACTIVE:
                    buttonText.text = I2.Loc.ScriptLocalization.Get("REFRESH");
                    message.text = I2.Loc.ScriptLocalization.Get("NON_ACTIVE");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_NON_ACTIVE");
                    FreezeGame();
                    break;
                case AlertType.RECOMPENSATION:
                    buttonText.text = I2.Loc.ScriptLocalization.Get("CINEMA_CLAIM");
                    ApplyTextForRecompensation(ReferenceHolder.GetHospital().RecomensationGiftController.GetLatestRecompensationType());
                    button.RemoveAllOnClickListeners();
                    button.onClick.AddListener(() => { OnGetRecompensationButtonCLick(); });
                    break;
                case AlertType.SAVE_HAS_MORE_RECENT_SAVE_THAN_CLIENT:
                    buttonText.text = I2.Loc.ScriptLocalization.Get("UPDATE_NOW");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_NEW_VERSION");
                    //updateMessageText.text = I2.Loc.ScriptLocalization.Get("NEW_VERSION_3");
                    button.onClick.AddListener(() => { OpenShop(); });
                    normalInfo.SetActive(false);
                    newVersionInfo.SetActive(true);
                    break;
                case AlertType.OPTIONAL_RESTART:
                    //TODO ADD MESSAGE LOCALIZATION. Do before release.
                    if (I2.Loc.ScriptLocalization.Get("OPTIONAL_RESTART") != null)
                        message.text = I2.Loc.ScriptLocalization.Get("OPTIONAL_RESTART");
                    else
                        message.text = customMessage;
                    title.text = I2.Loc.ScriptLocalization.Get("SELECTED");
                    if (ExitButton != null)
                    {
                        ExitButton.SetActive(true);
                    }
                    break;
            }

            whenDone?.Invoke();
        }

        private void ApplyTextForRecompensation(RecompensationType type)
        {
            switch (type)
            {
                case RecompensationType.recompensation:
                    message.text = I2.Loc.ScriptLocalization.Get("RECOMPENSATION");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_RECOMPENSATION");
                    break;
                case RecompensationType.reward:
                    message.text = I2.Loc.ScriptLocalization.Get("REWARD_DES");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_REWARD");
                    break;
                case RecompensationType.support:
                    message.text = I2.Loc.ScriptLocalization.Get("SUPPORT_DES");
                    title.text = I2.Loc.ScriptLocalization.Get("TITLE_SUPPORT");
                    break;
                default:
                    message.text = String.Empty;
                    title.text = String.Empty;
                    break;
            }
        }

        private void HandleAmazonRuntimeException(Exception ex)
        {
            if (ex != null && ex is Amazon.Runtime.AmazonClientException)
            {
                title.text = I2.Loc.ScriptLocalization.Get("WRONG_TIME_POPUP_TITLE");
                message.text = I2.Loc.ScriptLocalization.Get("WRONG_TIME_POPUP_BODY");
                if (HelpAndSupportButton != null)
                    HelpAndSupportButton.SetActive(true);
            }
        }

        private void OpenShop()
        {
#if UNITY_IOS
            Application.OpenURL("https://itunes.apple.com/us/app/id951697341");
#else
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.cherrypickgames.myhospital");
#endif
        }

        private void FreezeGame()
        {
            try
            {
                ReferenceHolder.GetHospital().saveLoadManager.StopSaving();
            }
            catch (Exception) { }
        }

        private void SetError()
        {
            GlobalDataHolder gdh = GlobalDataHolder.instance;
            if (gdh != null)
                gdh.IsCriticalErrorOccured = true;
        }

        public void OnConfirmButtonClick()
        {
            if (currentType == AlertType.NO_INTERNET_CONNECTION && AccountManager.HasInternetConnection() || currentType == AlertType.RECOMPENSATION)
            {
                Exit();
                return;
            }
            if (AreaMapController.Map != null)
                AreaMapController.Map.DestroyMap();
            SceneManager.LoadSceneAsync("LoadingScene");
        }

        public void OnGetRecompensationButtonCLick()
        {
            if (AccountManager.HasInternetConnection())
            {
                ReferenceHolder.GetHospital().RecomensationGiftController.ClaimGifts();
                NoSmoothExit();
            }
            else
            {
                Exit();
                StartCoroutine(Open(AlertType.NO_INTERNET_CONNECTION));
            }
        }

        public void ButtonHelpAndSupport()
        {
            switch (currentType)
            {
                case AlertType.CRITICAL_LOCAL:
                    HelpShiftManager.Instance.ShowFAQs();
                    break;
                case AlertType.OPTIONAL_RESTART:
                    Exit();
                    break;
                default:
                    HelpShiftManager.Instance.ShowFAQs();
                    break;
            }

        }

        public void NoSmoothExit()
        {
            base.Exit();
        }

        public void Exit()
        {
            void DeactivateOnAnimationFinish()
            {
                OnFinishedAnimating -= DeactivateOnAnimationFinish;
                gameObject.SetActive(false);
            }

            base.Exit();
            OnFinishedAnimating += DeactivateOnAnimationFinish;
        }
    }
}
