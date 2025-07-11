using UnityEngine;
using UnityEngine.EventSystems;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Hospital
{
    public class SettingsPopup : UIElement
    {
        public TextMeshProUGUI musicText;
        public TextMeshProUGUI soundText;
        public Image musicIcon;
        public Image soundIcon;
        public Sprite[] musicSprites;
        public Sprite[] soundSprites;

        public TextMeshProUGUI facebookText;
        public TextMeshProUGUI gamecenterText;
        public TextMeshProUGUI gpgText;
        public Image facebookButtonImage;
        public Image gamecenterButtonImage;
        public Image gpgButtonImage;
        public Sprite[] socialButtonSprites;
        public GameObject debugButton;  // Toggles the TestObjects in MainScene and MaternityScene
        public GameObject devSceneButton; // Develop scene launcher
        public GameObject[] iosObjects;
        public GameObject[] androidObjects;
#pragma warning disable 0649
        [SerializeField] private GameObject[] communityButtons;
#pragma warning restore 0649
        public GameObject fbReward;

        SoundsController soundsController;
        private bool permissionRequested = false;
        private int permissionMaxChecks = 10, permissionChecksCount = 0;

        void Awake()
        {
            SetPlatformSpecific();
        }

        void SetPlatformSpecific()
        {
#if UNITY_ANDROID
            for (int i = 0; i < iosObjects.Length; ++i)
                iosObjects[i].SetActive(false);

            for (int i = 0; i < androidObjects.Length; ++i)
                androidObjects[i].SetActive(true);
            //facebookButtonImage.rectTransform.anchoredPosition = new Vector2(0, facebookButtonImage.rectTransform.anchoredPosition.y);
#elif UNITY_IPHONE
            for (int i = 0; i < iosObjects.Length; ++i)
                iosObjects[i].SetActive(true);

            for (int i = 0; i < androidObjects.Length; ++i)
                androidObjects[i].SetActive(false);
#endif
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers, null);
            HospitalUIPrefabController.Instance.HideMainUI();
            soundsController = SoundsController.Instance;

            AccountManager_OnFacebookStateUpdate();
            AccountManager_OnGameCenterStateUpdate();
            BindListeners();
            HandleDebugButton();
            SetSoundButton(soundsController.IsSoundEnabled());
            SetMusicButton(soundsController.IsMusicEnabled());            
            OnGPGStateUpdate();
            UpdateCommunityButtons();
            SetFacebookRewardIcon();

            whenDone?.Invoke();
        }

        public void UpdateTranslation()
        {
            AccountManager_OnFacebookStateUpdate();
            AccountManager_OnGameCenterStateUpdate();
            OnGPGStateUpdate();
            SetSoundButton(soundsController.IsSoundEnabled());
            SetMusicButton(soundsController.IsMusicEnabled());
        }

        public void OnFbLikeButtonClick()
        {
            //#if UNITY_IOS
            //Application.OpenURL("fb://page/902544849789567");
            //#elif UNITY_ANDROID
            Application.OpenURL("https://m.facebook.com/My-Hospital-902544849789567");
            //#endif
        }

        void SetSoundButton(bool soundEnabled)
        {
            if (soundEnabled)
            {
                soundText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_SOUND_ON");
                soundIcon.sprite = soundSprites[0];
            }
            else
            {
                soundText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_SOUND_OFF"); ;
                soundIcon.sprite = soundSprites[1];
            }
        }

        public void OnNotificationsClick()
        {
            ButtonExit(false);
            StartCoroutine(UIController.get.NotificationsSettingsPopup.Open());
        }

        public void ButtonLanguages()
        {
            ButtonExit(false);
            StartCoroutine(UIController.get.LanguageSettingsPopUp.Open());
        }

        public void ButtonCredits()
        {
            ButtonExit(false);
            StartCoroutine(UIController.get.CreditsPopup.Open());
        }

        void SetMusicButton(bool musicEnabled)
        {
            if (musicEnabled)
            {
                musicText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_MUSIC_ON");
                musicIcon.sprite = musicSprites[0];
            }
            else
            {
                musicText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_MUSIC_OFF");
                musicIcon.sprite = musicSprites[1];
            }
        }

        public void ButtonMusic()
        {
            bool newState = !soundsController.IsMusicEnabled();

            if (newState)
                soundsController.UnmuteMusic();
            else
                soundsController.MuteMusic();

            SetMusicButton(newState);
        }

        public void ButtonSound()
        {
            bool newState = !soundsController.IsSoundEnabled();

            if (newState)
                soundsController.UnmuteSound();
            else
                soundsController.MuteSound();

            SetSoundButton(newState);
        }

        public void ButtonHelpAndSupport()
        {
            Debug.LogError("ButtonHelpAndSupport Pressed");
            HelpShiftManager.Instance.ShowFAQs();
        }

        public static void SendLogByEmail(string message)
        {
            if (DeveloperParametersController.Instance().parameters.sendExceptionViaEmail)
            {
                string email = DefaultConfigurationProvider.GetGeneralConfig().supportEmail;
                string subject = MyEscapeURL("MH Maternity Log: " + SystemInfo.deviceModel);
                StringBuilder builder = new StringBuilder();

                builder.Append("Device Model: ");
                builder.Append(SystemInfo.deviceModel);
                builder.Append(" \n\n");

                builder.Append("Time: ");
                builder.Append(DateTime.Now.ToLocalTime());
                builder.Append(" \n\n");

                builder.Append("Message: ");
                builder.Append(message);

                string body = MyEscapeURL(builder.ToString());
                Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
            }
        }

        public static void SendLogByEmail(Exception ex)
        {
            if (DeveloperParametersController.Instance().parameters.sendExceptionViaEmail)
            {
                string email = DefaultConfigurationProvider.GetGeneralConfig().supportEmail;
                string subject = MyEscapeURL("MH Maternity Log: " + SystemInfo.deviceModel);
                StringBuilder builder = new StringBuilder();

                builder.Append("Device Model: ");
                builder.Append(SystemInfo.deviceModel);
                builder.Append(" \n\n");

                builder.Append("Time: ");
                builder.Append(DateTime.Now.ToLocalTime());
                builder.Append(" \n\n");

                builder.Append("Message: ");
                builder.Append(ex.Message);
                builder.Append(" \n\n");

                builder.Append("StackTrace: ");
                builder.Append(ex.StackTrace);
                builder.Append(" \n\n");

                builder.Append("Source: ");
                builder.Append(ex.Source);

                string body = MyEscapeURL(builder.ToString());
                Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
            }
        }

        public void SendDeltaKey()
        {
            Debug.LogError("DDNA has been removed");
            //test stuff
            //string email = DefaultConfigurationProvider.GetGeneralConfig().supportEmail;
            //string subject = MyEscapeURL("MH Test support");
            //string body = MyEscapeURL(DeltaDNAController.pushReceiveLog );
            //Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        public void ButtonTermsOfServices()
        {
            Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().TermsOfServiceURL);
        }

        public void ButtonPrivacyPolicy()
        {
            Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().PrivacyPolicyURL);
        }

        public void ButtonFacebookFanpage()
        {
            AddCommunityReward();
            Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().FacebookFanpageURL);
        }

        public void ButtonInstagram()
        {
            AddCommunityReward();
            Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().InstagramURL);
        }

        public void ButtonYouTube()
        {
            AddCommunityReward();
            Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().YouTubeURL);
        }

        public void ButtonKTPlay()
        {
#if UNITY_IPHONE
            AddCommunityReward();
            Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().FacebookFanpageURL);
#elif UNITY_ANDROID

            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) && Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                AddCommunityReward();
                Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().FacebookFanpageURL);
            }
            else
            {
                permissionRequested = true;
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
#else
           Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().FacebookFanpageURL);
#endif
        }

#if UNITY_ANDROID
        private void OnGUI()
        {
            if(permissionRequested)
            {
                if (permissionChecksCount++ < permissionMaxChecks)
                {
                    if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) && Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                    {
                        AddCommunityReward();
                        Application.OpenURL(DefaultConfigurationProvider.GetGeneralConfig().FacebookFanpageURL);
                    }
                }
                else
                {
                    permissionRequested = false;
                }
            }
        }
#endif

        private void AddCommunityReward()
        {
            GameObject button = EventSystem.current.currentSelectedGameObject;
            int ID = button.transform.parent.transform.GetSiblingIndex();
            if (Game.Instance.gameState().CheckCommunityRewardState(ID))
            {
                Vector3 startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ReferenceHolder.Get().engine.MainCamera.GetCamera().nearClipPlane));
                Game.Instance.gameState().SetCommunityRewardState(ID, false);
                button.GetComponent<CommunityButton>().SetRewardImageActive(false);
                int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
                Game.Instance.gameState().AddResource(ResourceType.Diamonds, 3, EconomySource.CommunityReward, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, startPoint, 3, 0f, 2f, new Vector3(1.2f, 1.2f, 1), new Vector3(0.9f, 0.9f, 1), ReferenceHolder.Get().giftSystem.particleSprites[1], null, () =>
                {
                    Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, 3, currentDiamondAmount);
                });
            }
        }

        private void UpdateCommunityButtons()
        {
            for (int i = 0; i < communityButtons.Length; ++i)
            {
                communityButtons[i].GetComponent<CommunityButton>().SetRewardImageActive(Game.Instance.gameState().CheckCommunityRewardState(i));
            }
        }

        static string MyEscapeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }

        void SetFacebookRewardIcon()
        {
            fbReward.SetActive(!Game.Instance.gameState().IsFBRewardConnectionClaimed());
        }

        public void ButtonFacebookToggle()
        {
            switch (Tools.SceneManagingTool.GetScene())
            {
                case Tools.SceneManagingTool.Scene.Main:
                    AccountManager.Instance.ToggleFacebookStatus();
                    break;
                case Tools.SceneManagingTool.Scene.Maternity:
                    ShowUnsupportedFeaturefloat();
                    break;
            }
        }

        public void ButtonGameCenterToggle()
        {
            switch (Tools.SceneManagingTool.GetScene())
            {
                case Tools.SceneManagingTool.Scene.Main:
                    AccountManager.Instance.ToggleGameCenterStatus();
                    break;
                case Tools.SceneManagingTool.Scene.Maternity:
                    ShowUnsupportedFeaturefloat();
                    break;
            }
        }

        public void ButtonGPGToggle()
        {
            switch (Tools.SceneManagingTool.GetScene())
            {
                case Tools.SceneManagingTool.Scene.Main:
                    AccountManager.Instance.ToggleGPGStatus();
                    break;
                case Tools.SceneManagingTool.Scene.Maternity:
                    ShowUnsupportedFeaturefloat();
                    break;
            }
        }

        private void ShowUnsupportedFeaturefloat()
        {
            MessageController.instance.ShowMessage(68, true);
        }

        void SetFacebookButton(bool isSignedIn)
        {
            if (isSignedIn)
            {
                facebookText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_CONNECTED");
                facebookButtonImage.sprite = socialButtonSprites[0];
            }
            else
            {
                facebookText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_DISCONNECTED");
                facebookButtonImage.sprite = socialButtonSprites[1];
            }

            UpdateCommunityButtons();
            SetFacebookRewardIcon();
        }

        void SetGameCenterButton(bool isSignedIn)
        {
            if (isSignedIn)
            {
                gamecenterText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_CONNECTED");
                gamecenterButtonImage.sprite = socialButtonSprites[0];
            }
            else
            {
                gamecenterText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_DISCONNECTED");
                gamecenterButtonImage.sprite = socialButtonSprites[1];
            }

            UpdateCommunityButtons();
        }

        void SetGPGButton(bool isSignedIn)
        {
            if (isSignedIn)
            {
                gpgText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_CONNECTED");
                gpgButtonImage.sprite = socialButtonSprites[0];
            }
            else
            {
                gpgText.text = I2.Loc.ScriptLocalization.Get("SETTINGS_DISCONNECTED");
                gpgButtonImage.sprite = socialButtonSprites[1];
            }
        }

        public void ButtonExit(bool showMainUI = true)
        {
            Exit(showMainUI);
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            UnbindListeners();
        }

        void OnDestory()
        {
            UnbindListeners();
        }

        private void BindListeners()
        {
            AccountManager.OnFacebookStateUpdate += AccountManager_OnFacebookStateUpdate;
            AccountManager.OnGameCenterStateUpdate += AccountManager_OnGameCenterStateUpdate;
            AccountManager.OnGPGStateUpdate += OnGPGStateUpdate;
        }

        private void UnbindListeners()
        {
            AccountManager.OnFacebookStateUpdate -= AccountManager_OnFacebookStateUpdate;
            AccountManager.OnGameCenterStateUpdate -= AccountManager_OnGameCenterStateUpdate;
            AccountManager.OnGPGStateUpdate -= OnGPGStateUpdate;
        }

        private void AccountManager_OnGameCenterStateUpdate()
        {
            SetGameCenterButton(AccountManager.IsGameCenterConnectedGlobal);
        }

        private void AccountManager_OnFacebookStateUpdate()
        {
            SetFacebookButton(AccountManager.IsFacebookConnectedGlobal);
        }

        private void OnGPGStateUpdate()
        {
            SetGPGButton(AccountManager.IsGameCenterConnectedGlobal); // Using IsGameCenterConnectedGlobal intendedly even for Google Play Games
        }

        public void ButtonDebug()
        {
            togglable.ToggleStatic();
        }

        void HandleDebugButton()
        {
#if MH_RELEASE && !MH_QA
            debugButton.gameObject.SetActive(false);
            devSceneButton.gameObject.SetActive(false);
#else            
            debugButton.gameObject.SetActive(DeveloperParametersController.Instance().parameters.devTestButtonVisible);
            devSceneButton.gameObject.SetActive(DeveloperParametersController.Instance().parameters.devTestButtonVisible);
#endif
        }
    }
}