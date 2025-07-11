using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using System;

namespace Hospital
{
    public class WelcomePopupController : UIElement
    {
        public static bool isTermsAccepted = true; //Set to true because the popup is shown only once.
        public Button OkButton;
        public Image OkButtonImage;

#pragma warning disable 0067
        [TutorialTrigger]
        public event EventHandler popupOpenedEvent;
        [TutorialTrigger]
        public event EventHandler okButtonClicked;
#pragma warning restore 0067

        [TutorialTriggerable]
        public void OpenPopup(bool isFadeIn, bool preservesHovers)
        {
            try
            {
                CoroutineInvoker.Instance.StartCoroutine(base.Open(isFadeIn, preservesHovers));
                isTermsAccepted = false;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n::::\n" + e.StackTrace);
            }
        }

        [TutorialTriggerable]
        public void Test(float floatArg, bool boolArg, int intArg, string stringArg)
        {
            Debug.LogError(floatArg + ", " + boolArg + ", " + intArg + ", " + stringArg);
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            NotificationCenter.Instance.CloseWelcomePopUp.Invoke(new BaseNotificationEventArgs());
        }

        public void ButtonTermsOfServices()
        {
            Application.OpenURL("http://myhospital.games/terms-of-service/");
        }

        public void ButtonPrivacyPolicy()
        {
            Application.OpenURL("http://myhospital.games/privacy-policy/");
        }

        public void ToggleTermsOfServiceAcceptance(bool isAccepted)
        {
            if (isAccepted)
            {
                OkButtonImage.material = null;
                OkButton.RemoveAllOnClickListeners();
                OkButton.onClick.AddListener(ButtonOK);
            }
            else
            {
                OkButtonImage.material = ResourcesHolder.Get().GrayscaleMaterial;
                OkButton.RemoveAllOnClickListeners();
            }
        }

        private void ButtonOK()
        {
            isTermsAccepted = true;
            Exit();
        }
    }
}