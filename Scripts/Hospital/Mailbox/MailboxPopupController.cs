using SimpleUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Hospital
{

    [RequireComponent(typeof(TabChangerView))]
    public class MailboxPopupController : UIElement
    {
        private const string SOCIAL_MAILBOX_GIFT_UNAVAILABLE = "SOCIAL_MAILBOX_GIFT_UNAVAILABLE";
        private const string ANIMATION_TRIGGER = "Bounce";
        private const string LEVEL_INSUFICCIENT = "GIFT_SYSTEM/GIFT_FLOAT_RECEIVE";
        private const string REPLACE_VALUE = "{0}";
#pragma warning disable 0649
        [SerializeField] private TabChangerView tabChanger;
        [SerializeField] private MailboxPopUpView mailboxPopupView;
#pragma warning restore 0649

        private readonly Dictionary<Tab, string> tabTranslationKey = new Dictionary<Tab, string>
        {
            {Tab.Gifts,"SOCIAL_GIFTBOX"},
            {Tab.Inbox,"SOCIAL_MAILBOX_INBOX"},
            {Tab.Outbox,"SOCIAL_MAILBOX_OUTBOX"}
        };

        //public event System.EventHandler popUpClosed;

        private enum Tab
        {
            Gifts,
            Inbox,
            Outbox
        }

        private void OnEnable()
        {
            UnityAction[] actions = { OnNambackGiftsClick, OnNambackInboxClick, OnNAmbackOutboxActive };
            tabChanger.Initialize(actions);
            tabChanger.Exit = () => Exit();
            gameObject.transform.SetAsLastSibling();
            SetAddFriendsButton();
            SelectTabOnOpen();
        }

        private void OnEnableNambackGiftsClick()
        {
            tabChanger.SetNambackActive((int)Tab.Gifts, ANIMATION_TRIGGER);
            mailboxPopupView.AddFriendsButtonActive = false;
            mailboxPopupView.AddFriendsLabelActive = false;
            mailboxPopupView.SetLocalization(tabTranslationKey[Tab.Gifts]);
        }

        public void OnNambackGiftsClick()
        {
            if (!VisitingController.Instance.IsVisiting)
            {
                if (IsLevelToLow())
                {
                    MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(LEVEL_INSUFICCIENT)
                        .Replace(REPLACE_VALUE, GiftsAPI.Instance.GiftsFeatureMinLevel.ToString()));
                }
                else
                {
                    OnEnableNambackGiftsClick();
                }
            }
            else
            {
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(SOCIAL_MAILBOX_GIFT_UNAVAILABLE));
                mailboxPopupView.SetGrayscale(true);
            }
        }

        public void OnNambackInboxClick()
        {
            tabChanger.SetNambackActive((int)Tab.Inbox, ANIMATION_TRIGGER);
            mailboxPopupView.AddFriendsButtonActive = true;
            mailboxPopupView.AddFriendsLabelActive = true;
            mailboxPopupView.SetLocalization(tabTranslationKey[Tab.Inbox]);
        }

        public void OnNAmbackOutboxActive()
        {
            tabChanger.SetNambackActive((int)Tab.Outbox, ANIMATION_TRIGGER);
            mailboxPopupView.AddFriendsButtonActive = true;
            mailboxPopupView.AddFriendsLabelActive = true;
            mailboxPopupView.SetLocalization(tabTranslationKey[Tab.Outbox]);
        }

        private void SetAddFriendsButton()
        {
            mailboxPopupView.SetAddFriendsButton(() =>
            {
                Exit();
                StartCoroutine(UIController.getHospital.addFriendsPopupController.Open());
            });
        }

        private bool IsLevelToLow()
        {
            return !GiftsAPI.Instance.IsFeatureUnlocked();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            NotificationCenter.Instance.MailBoxClosed.Invoke(new BaseNotificationEventArgs());
        }

        private void SelectTabOnOpen()
        {
            if (IsLevelToLow() || VisitingController.Instance.IsVisiting)
            {
                mailboxPopupView.SetGrayscale(true);
                OnNambackInboxClick();
            }
            else //if (!TutorialController.Instance.tutorialEnabled ||TutorialController.Instance.CurrentTutorialStepIndex > TutorialController.Instance.GetStepId(StepTag.wise_thank_you))
            {
                mailboxPopupView.SetGrayscale(false);
                OnNambackGiftsClick();
            }
        }
    }

}
