using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using System;

namespace Hospital
{
    public class FriendsDrawerController : UIElement
    {
        private const float ANIMATION_SPEED_ON_POPUP_SHOW = 1000;
        private const float ANIMATION_SPEED_DEFAULT = 1;

        private enum CurrentTab
        {
            LastHelpers,
            Friends,
            Liked,
            HelpRequests
        }

        #region Fields
        private CurrentTab currentTab = CurrentTab.Friends;
#pragma warning disable 0649
        [SerializeField] private Transform helpRequestorsContent;
        [SerializeField] private Transform likedContent;
        [SerializeField] private Transform friendsContent;
        [SerializeField] private Transform helpersContent;
        [SerializeField] private Transform helpRequestorsTab;
        [SerializeField] private Transform likedTab;
        [SerializeField] private Transform friendsTab;
        [SerializeField] private Transform helpersTab;

        [SerializeField] private Transform noHelpersText;
        [SerializeField] private Transform noHelpRequestorsText;
        [SerializeField] private Transform noLikedText;
        [SerializeField] public Animator animator;
#pragma warning restore 0649

        private Dictionary<CurrentTab, string> animationsDict = new Dictionary<CurrentTab, string>()
        {
            {CurrentTab.LastHelpers, "Helpers_tab"}, {CurrentTab.Friends, "Friends_tab"} ,
            {CurrentTab.Liked, "Following_tab"}, {CurrentTab.HelpRequests,"Followers_tab" }
        };
#pragma warning disable 0649
        [SerializeField] private Sprite TabButtonGraphicsActive;
        [SerializeField] private Sprite TabButtonGraphicsInActive;
        [SerializeField] private Sprite[] TabIconsActive;
        [SerializeField] private Sprite[] TabIconsInactive;
        [SerializeField] private Image[] TabBackgroundsGO;
        [SerializeField] private Image[] TabIconsGO;
        [SerializeField] private RecycledViewManager recycledFriends;
        [SerializeField] private RecycledViewManager recycledLiked;
        [SerializeField] private RecycledViewManager recycledHelpRequests;
        [SerializeField] private RecycledViewManager recycledHelpers;
        [SerializeField] private GameObject loaderSpinner;
        [SerializeField] private GameObject GiftBadgeGameObject;
        [SerializeField] private RectTransform[] ScrollingPanels;
        [SerializeField] private RectTransform SoftMaskRight;
#pragma warning restore 0649
        #endregion

        private List<IFollower> lastHelpers = new List<IFollower>();
        [Header("Device specific layout refs")]
        public RectTransform friendsDrawerRect;
        public RectTransform exitButtonRect;

        [SerializeField] GameObject availableGiftAmountWraper = null;

        [SerializeField] private TextMeshProUGUI availableGiftsAmount = null;
#pragma warning disable 0649
        [SerializeField] private GameObject newMail;
#pragma warning restore 0649

        //private bool refreshTabs = true;

        void Awake()
        {
            SetDeviceLayout();
            Initialize(false);
            this.InvokeDelayed(() => { this.gameObject.SetActive(false); }, .5f);
        }

        protected override void Start()
        {
            animator = GetComponent<Animator>();

            InGameFriendsProvider.OnInGameFriendsChange += OnFriendsUpdate;
            AccountManager.OnFacebookFriendsUpdate += OnFriendsUpdate;

            FriendsController.OnLikedRefresh += FriendsController_OnLikedRefresh;
            GiftsSendController.onUpdate += GiftsSendController_onUpdate;

            if (ExtendedCanvasScaler.HasNotch())
                SetupPanelForIphoneX();
        }

        #region EventCallbacks
        private void OnFriendsUpdate()
        {
            FillTabContent(recycledLiked, FriendsDataZipper.RemoveInGameFriendsFbfromLikes());
            FillTabContent(recycledFriends, FriendsDataZipper.GetFbAndInGameFriends());
        }

        private void FriendsController_OnLikedRefresh()
        {
            noLikedText.gameObject.SetActive(false);
            FillTabContent(recycledLiked, FriendsDataZipper.RemoveInGameFriendsFbfromLikes());
            if (FriendsController.Instance.likedFollowers.Count == 0)
                noLikedText.gameObject.SetActive(true);
        }

        private void GiftsSendController_onUpdate(string saveID = null)
        {
            SetAvailableGiftsAmount(GiftsSendController.Instance.GetAvailableGifts());
        }

        public void SetBadgeNewNotification(bool value)
        {
            newMail.SetActive(value);
        }
        #endregion

        void SetDeviceLayout()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
                friendsDrawerRect.localScale = new Vector3(1.3f, 1.3f, 1f);
            else
                friendsDrawerRect.localScale = Vector3.one;
        }

        private VisitingEntryPoint GetVisitingEntryPoint(Transform content)
        {
            if (content == helpRequestorsContent)
                return VisitingEntryPoint.Followers;
            else if (content == likedContent)
                return VisitingEntryPoint.Following;
            else if (content == helpersContent)
                return VisitingEntryPoint.LastHelpers;
            else
                return VisitingEntryPoint.Friends;
        }

        private void SetupPanelForIphoneX()
        {
            gameObject.GetComponent<RectTransform>().localPosition += new Vector3(-95.0f, 30, 0);
            exitButtonRect.localPosition += new Vector3(43, 0, 0);
            for (int i = 0; i < ScrollingPanels.Length; i++)
            {
                ScrollingPanels[i].offsetMax += new Vector2(47.72f, 0);
            }
            SoftMaskRight.localPosition += new Vector3(47.72f, 0, 0);
        }

        public void OnMailboxButtonClick()
        {
            StartCoroutine(UIController.getHospital.mailboxPopup.Open(true, false, () =>
            {
                ToggleVisible();
                animator.speed = ANIMATION_SPEED_ON_POPUP_SHOW;
            }));
        }

        public void OnAddFriendsButtonClick()
        {
            StartCoroutine(UIController.getHospital.addFriendsPopupController.Open(true, false, () =>
            {
                ToggleVisible();
                animator.speed = ANIMATION_SPEED_ON_POPUP_SHOW;
            }));
        }

        void OnDestroy()
        {
            AccountManager.OnFacebookFriendsUpdate -= OnFriendsUpdate;
            InGameFriendsProvider.OnInGameFriendsChange -= OnFriendsUpdate;
            FriendsController.OnLikedRefresh -= FriendsController_OnLikedRefresh;
            GiftsSendController.onUpdate -= GiftsSendController_onUpdate;
        }

        private void FillTabContent(RecycledViewManager recycledView, List<IFollower> list)
        {
            recycledView.SetData(list);
        }

        public override void SetVisible(bool visible)
        {
            if (UIController.getHospital.isAnyPopupActive())
                return;

            if (visible)
            {
                UIController.getHospital.CloseActiveHover();
                try
                {
                    ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ChangeOnPressType(x => { SetVisible(false); });
                    NotificationCenter.Instance.FriendsDrawerOpened.Invoke(new BaseNotificationEventArgs());
                }
                catch (Exception e)
                {
                    Debug.LogError("FIX THIS!!!: " + e.Message);
                }
            }
            else
            {
                NotificationCenter.Instance.FriendsDrawerClosed.Invoke(new BaseNotificationEventArgs());
                HospitalAreasMapController.Map.ResetOnPressAction();
            }

            UIController.getHospital.SetBoxesAndBoostersButtonsVisible(!visible);
            base.SetVisible(visible);
        }

        public void HideFriendsDrawer()
        {
            UIController.getHospital.SetBoxesAndBoostersButtonsVisible(true);
            base.SetVisible(false);
        }

        public void RefeshLastHelpersList()
        {
            lastHelpers.Clear();
            foreach (BaseUserModel userModel in LastHelpersSynchronizer.Instance.data)
            {
                lastHelpers.Add(userModel);
            }
            if (lastHelpers.Count == 0)
            {
                if (noHelpersText != null && noHelpersText.gameObject != null)
                    noHelpersText.gameObject.SetActive(true);
            }
            else
            {
                if (noHelpersText != null && noHelpersText.gameObject != null)
                    noHelpersText.gameObject.SetActive(false);
                FillTabContent(recycledHelpers, FriendsDataZipper.ReplaceStandardWithFacebookAndInGameFriends(lastHelpers));
            }
        }

        public override void ToggleVisible()
        {
            if (!IsVisible)
            {
                animator.speed = ANIMATION_SPEED_DEFAULT;
            }
            base.ToggleVisible();
            try
            {
                if (IsVisible && UIController.get.drawer.IsVisible)
                    UIController.get.drawer.ToggleVisible();
            }
            catch (Exception e)
            {
                Debug.LogError("FIX THIS!!!: " + e.Message);
            }

            if (IsVisible)
                ShowFriends(false);

            UIController.get.SetBoxesAndBoostersButtonsVisible(!IsVisible);
        }

        public void SetAvailableGiftsAmountActive(bool setActive)
        {
            if (availableGiftAmountWraper == null)
            {
                Debug.LogError("availableGiftsAmountWrapper is null");
                return;
            }
            else
                availableGiftAmountWraper.SetActive(setActive);
        }

        public void SetAvailableGiftsAmount(int amount)
        {
            SetAvailableGiftsAmountActive(GiftsAPI.Instance.IsFeatureUnlocked());
            if (availableGiftsAmount != null)
                availableGiftsAmount.text = amount.ToString();
            else
                Debug.LogError("availableGiftsAmount is null");
        }

        public void AvailableGiftsButton()
        {
            if (Game.Instance.gameState().GetHospitalLevel() < GiftsAPI.Instance.GiftsFeatureMinLevel)
            {
                string message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_FLOAT_SEND").Replace("{0}", GiftsAPI.Instance.GiftsFeatureMinLevel.ToString());
                MessageController.instance.ShowMessage(message);
                Debug.LogError("Za maly level");
            }
            else if (GiftsSendController.Instance.GetAvailableGifts() > 0)
            {
                string message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_FLOAT_AMOUNT").Replace("{0}", GetAvailableGiftsAmount().ToString());
                MessageController.instance.ShowMessage(message);
            }
            else
            {
                string message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFTS_WAIT_TO_SEND").Replace("{0}", GetFormatedTimeToNextGift());
                MessageController.instance.ShowMessage(message);
                Debug.LogError("Nie ma dostępnych prezentów");
            }
        }

        private string GetFormatedTimeToNextGift()
        {
            int time = (int)Mathf.Max(GiftsSendController.Instance.GetDurationInSecondsToNextGift(), 0);
            return UIController.GetFormattedTime(time);
        }

        private int GetAvailableGiftsAmount()
        {
            return GiftsSendController.Instance.GetAvailableGifts();
        }

        #region TabChanges
        public void ShowLastHelpers()
        {
            if (TryToChangeTab(CurrentTab.LastHelpers, helpersTab))
                LastHelpersProvider.Instance.OnDataLoad();
        }

        public void ShowHelpRequests(bool tabAnim = true)
        {
            if (TryToChangeTab(CurrentTab.HelpRequests, helpRequestorsTab))
            {
                FillTabContent(recycledHelpRequests, FriendsDataZipper.ZipFbAndIgfWithHelpRequests());
                noHelpRequestorsText.gameObject.SetActive(recycledHelpRequests.Count == 0);
            }
        }

        public void ShowLiked(bool tabAnim = true)
        {
            if (TryToChangeTab(CurrentTab.Liked, likedTab))
            {
                loaderSpinner.SetActive(false);
                RefreshLikedAndHelpRequestors();
            }
        }

        private void RefreshLikedAndHelpRequestors()
        {
            noLikedText.gameObject.SetActive(false);
            loaderSpinner.gameObject.SetActive(true);
            FriendsController.Instance.GetLiked(
            () =>
            {
                FillTabContent(recycledLiked, FriendsDataZipper.RemoveInGameFriendsFbfromLikes());
                loaderSpinner.gameObject.SetActive(false);
                noLikedText.gameObject.SetActive(recycledLiked.Count == 0);
            });
        }

        public void ShowFriends(bool tabAnim = true)
        {
            TryToChangeTab(CurrentTab.Friends, friendsTab, tabAnim);
        }

        private bool TryToChangeTab(CurrentTab targetTab, Transform tabToSet, bool tabAnim = true)
        {
            if (currentTab == targetTab)
                return false;

            for (int i = 0; i < TabBackgroundsGO.Length; i++)
            {
                TabBackgroundsGO[i].sprite = TabButtonGraphicsInActive;
                TabIconsGO[i].sprite = TabIconsInactive[i];
            }

            currentTab = targetTab;

            RestoreTabOrder();
            tabToSet.SetAsLastSibling();

            int activeIndex = (int)currentTab;
            TabBackgroundsGO[activeIndex].sprite = TabButtonGraphicsActive;
            TabIconsGO[activeIndex].sprite = TabIconsActive[activeIndex];

            if (tabAnim)
                animator.SetTrigger(animationsDict[currentTab]);

            //ReferenceHolder.GetHospital().drWiseCardController.WiseShowHideTutorialArrow(IsVisible, currentTab != CurrentTab.Friends);

            return true;
        }

        private void RestoreTabOrder()
        {
            likedTab.SetAsLastSibling();
            helpersTab.SetAsLastSibling();
            friendsTab.SetAsLastSibling();
            helpRequestorsTab.SetAsLastSibling();
        }
        #endregion

    }
}
