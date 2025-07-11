using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;

namespace SimpleUI
{
    public abstract class UIElement : AnimatorMonitor, IDiamondTransactionMaker
    {
        private Guid PopupOpenID;

        [HideInInspector]
        public float pastDrawerPosition = 1;

        [SerializeField] private UIElementType uiElementType = UIElementType.defaultType;

        [TutorialTrigger]
        public event EventHandler uiElementOpened;
        [TutorialTrigger]
        public event EventHandler uiElementClosed;

        public static UnityAction<UIElementType> onUIElementOpen = null;
        public static UnityAction<UIElementType> onUIElementExit = null;

        public bool IsVisible
        {
            get;
            /*private*/ set;
        }

        public bool keepHeirarchyPosition = false;
        public bool disableOnEnd = true;

        protected virtual void Start()
        {
            Initialize();
        }

        public UIElement()
        {
            IsVisible = false;
        }

        private bool isInitialized = false;
        public bool isBlockedGame = false;
        public bool isFadeClickable = true;

        protected void Initialize(bool setVisible = true)
        {
            //Debug.LogError("Initialize " + setVisible);
            if (isInitialized)
                return;

            isInitialized = true;
            IsVisible = setVisible;

            if (disableOnEnd)
                OnFinishedAnimating += DisableOnEnd;
        }

        void UIElementOpenedInvoke(UIElement sender, EventArgs e)
        {
            uiElementOpened?.Invoke(sender, e);
        }

        void UIElementClosedInvoke(UIElement sender, EventArgs e)
        {
            uiElementClosed?.Invoke(sender, e);
        }

        [TutorialCondition]
        public bool IsOpen() { return IsVisible; }
        [TutorialCondition]
        public bool IsClosed() { return !IsVisible; }

        public virtual void SetVisible(bool visible)
        {
            if (visible == false && SoundsController.Instance != null)
                SoundsController.Instance.PlayButtonClick(false);

            if (IsVisible != visible)
            {
                if (visible)
                    gameObject.SetActive(true);

                IsVisible = visible;
                try
                {
                    if (TryGetComponent<Animator>(out var animator))                    
                        animator.SetBool("IsVisible", IsVisible);

                    CheckForAnimation();
                }
                catch (Exception)
                {
                    Debug.LogWarning("UIElement animation is doing weird stuff");
                }

                try
                {
                    if (visible)
                    {
                        InvokeOnUIElementOpen(uiElementType);
                        UIElementOpenedInvoke(this, null);
                    }
                    else
                    {
                        InvokeOnUIElementExit(uiElementType);
                        UIElementClosedInvoke(this, null);
                    }
                }
                catch (Exception)
                {
                    Debug.LogWarning("UIElement invoke is doing weird stuff");
                }
            }
        }

        protected virtual void DisableOnEnd()
        {
            //Debug.LogError("DisableOnEnd !IsVisible && !IsAnimating = " + !IsVisible + !IsAnimating);
            if (!IsVisible && !IsAnimating)
            {
                gameObject.SetActive(false);
                if (!keepHeirarchyPosition)
                    transform.SetAsFirstSibling();
            }
        }

        public bool isStopVisibleAnim()
        {
            return IsVisible && !IsAnimating;
        }

        public virtual void ToggleVisible()
        {
            SoundsController.Instance.PlayButtonClick(false);
            SetVisible(!IsVisible);
        }

        //public virtual void Open(bool isFadeIn = true, bool preservesHovers = false)
        //{
        //    Debug.LogError("> > > UIELEMENT - Open Pre Coroutine");
        //    gameObject.SetActive(true);
        //    StartCoroutine(OpenCoroutine(isFadeIn, preservesHovers));
        //}

        //private IEnumerator OpenCoroutine(bool isFadeIn, bool preservesHovers)
        //{
        //    //yield return null;
        //    yield return new WaitForSeconds(0.5f);
        //    OpenN(isFadeIn, preservesHovers);
        //}        

        //private void OpenN(bool isFadeIn = true, bool preservesHovers = false)
        public virtual IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return null; // CV: Always delay 1 frame to avoid TextMesh Pro crashing

            InitializeID();

            if (UIController.get != null && UIController.get.drawer.IsVisible)
                UIController.get.drawer.ToggleVisible();

            if (UIController.get != null && UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
                UIController.get.FriendsDrawer.HideFriendsDrawer();

            if (!CampaignConfig.hintSystemEnabled && UIController.getHospital != null && UIController.getHospital.ObjectivesPanelUI != null && UIController.getHospital.ObjectivesPanelUI.isSlidIn)
            {
                UIController.getHospital.ObjectivesPanelUI.SlideOut();
                UIController.getHospital.ObjectivesPanelUI.HideTemporary();
            }

            transform.SetAsLastSibling();
            //used by all popups
            try
            {
                SetVisible(true);
            }
            catch (Exception)
            {
                Debug.LogWarning("UI element - something went wrong when setinng it visible");
            }

            if (UIController.get != null)
                UIController.get.AddActivePopUp(this, isFadeIn, preservesHovers);

            if (Hospital.AreaMapController.Map != null)
                Hospital.AreaMapController.Map.HideTransformBorder();
            
            whenDone?.Invoke();
        }

        public virtual void Exit(bool hidePopupWithShowMainUI = true)
        {
            //used by all popups
            EraseID();
            if (!this.isBlockedGame)
            {
                if (hidePopupWithShowMainUI)
                {
                    if (HospitalUIPrefabController.Instance != null)
                        HospitalUIPrefabController.Instance.ShowMainUI();
                }

                SetVisible(false);

                if (UIController.get != null)
                    UIController.get.RemoveActivePopUp(this);
                if (UIController.getHospital != null)
                {
                    if (!CampaignConfig.hintSystemEnabled && UIController.getHospital.ObjectivesPanelUI != null && !UIController.getHospital.ObjectivesPanelUI.isSlidIn && UIController.getHospital.ObjectivesPanelUI.IsHiddenTemporary())
                        UIController.getHospital.ObjectivesPanelUI.SlideIn();
                }
            }
        }

        public void InitializeID()
        {
            PopupOpenID = Guid.NewGuid();
        }

        public Guid GetID()
        {
            return PopupOpenID;
        }

        public void EraseID()
        {
            PopupOpenID = Guid.Empty;
        }

        private void InvokeOnUIElementOpen(UIElementType uiElementType)
        {
            if (Enum.IsDefined(typeof(UIElementType), uiElementType))
            {
                if (onUIElementOpen != null && uiElementType != UIElementType.ignore)
                {
                    try
                    {
                        onUIElementOpen.Invoke(uiElementType);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("Invalid uiElementType: " + (int)uiElementType);
                    }
                }
            }
            else 
                return;
        }

        private void InvokeOnUIElementExit(UIElementType uiElementType)
        {
            if (Enum.IsDefined(typeof(UIElementType), uiElementType))
            {
                if (onUIElementExit != null && uiElementType != UIElementType.ignore)
                {
                    try
                    {
                        onUIElementExit.Invoke(uiElementType);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("Invalid uiElementType: " + (int)uiElementType);
                    }
                }
            }
            else
                return;
        }

        public enum UIElementType
        {
            defaultType = 0,
            addFriends = 1,
            enterCode = 2,
            friendAddingResult = 3,
            hostPopup = 4,
            mainPatientCard = 5,
            patientCard = 6,
            epidemyOn = 7,
            epidemyOff = 8,
            chooseAccount = 9,
            expand = 10,
            storage = 11,
            storageUpgrade = 12,
            panaceaCollector = 13,
            panaceaCollectorUpgrade = 14,
            children = 15,
            achievement = 16,
            patientZero = 17,
            info = 18,
            storageFull = 19,
            nextLevel = 20,
            diagnosisQueue = 21,
            renovate = 22,
            pharmacy = 23,
            modifyOffer = 24,
            globalOffers = 25,
            lockedFeature = 26,
            boosterInfo = 27,
            bosterMenu = 28,
            casesMenu = 29,
            settings = 30,
            caseOpening = 31,
            bilboardAd = 32,
            vip = 33,
            connectFb = 34,
            buyBundle = 35,
            buyResources = 36,
            welcome = 37,
            bubbleBoy = 38,
            updateReward = 39,
            dailyQuestDaily = 40,
            eventPopup = 41,
            advancedSettings = 42,
            exit = 43,
            report = 44,
            languageSettings = 45,
            rate = 46,
            replaceDailyTask = 47,
            credits = 48,
            gameEvent = 49,
            createOffer = 50,
            signCustomization = 51,
            dailyDealConfirmation = 52,
            boosterStarts = 53,
            achivementUnlocked = 54,
            eventGoalReached = 55,
            objectivesInfo = 57,
            breastCancerFundation = 58,
            treatmentHelpDonate = 59,
            treatmentSendPusches = 60,
            treatmentHelpSummary = 61,
            buyLootbox = 62,
            bundlePurchaseConfirmation = 63,
            vitaminMakerUpgrade = 64,
            vitaminMakerUpgradeConfirmation = 65,
            acceptReject = 66,
            vitamineCollectorInfo = 67,
            vitaminsMakerRefilment = 68,
            mailbox = 69,
            coinPurchaseConfirmation = 70,
            maternityStatus = 71,
            dailyQuestAndReward = 72,
            iapShop = 73,
            upgradeVip = 74,
            levelUp = 75,
            notificationsSettings = 76,
            maternityInfo = 77,
            patientCardMaternity = 78,
            baby = 79,
            nurseRoomMaternity = 80,
            vitaminCollector = 81,
            boxOpening = 82,
            shopDrawer = 83,
            friendsDrawer = 84,
            ignore = 85,
            loseConnection = 86,
            globalEventCenter = 87,
            eventEnded = 88,
            nextEventIn = 89,
            crossPromotion = 90,
            fb_noSupport = 91,
        }
    }
}
