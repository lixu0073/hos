using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;

namespace Hospital
{
    public class UnboxingPoUpController : UIElement
    {
        [SerializeField] private GameObject boxContent = null;
        [SerializeField] private TextMeshProUGUI topText = null;
        [SerializeField] private TextMeshProUGUI bottomText = null;
        [SerializeField] private TextMeshProUGUI dayText = null;

        public GameObject cardContent = null;

        [SerializeField] private Image boxImage = null;
        [SerializeField] private Image coverImage = null;
        [SerializeField] private Image coverImage2 = null;

        [SerializeField] private TextMeshProUGUI prizeName = null;
        [SerializeField] private TextMeshProUGUI amountText = null;
        public GameObject prizeImage = null;

        public Animator cardAnimator = null;

        [SerializeField] private Animator boxAnimator = null;

        delegate void OpenDelegate();
        OpenDelegate openDelegate = null;

        public bool isFromGlobalEvent = false;
        public bool isFromFriendGift = false;
        private bool instantOpen = false;

        void Awake()
        {
            boxAnimator = boxContent.GetComponent<Animator>();
        }

        public static bool unboxingPending;

        public void OpenCasesPopUp()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                if (HospitalAreasMapController.HospitalMap != null)
                {
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromVIP = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromFacebook = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromEpidemy = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromTreasure = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromDailyQuest = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromGlobalEvent = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).BundledGiftFromGlobalEvent = false;
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).SingleGiftFromGlobalEvent = false;

                    AreaMapController.Map.casesManager.GiftFromIAP = false;
                    openDelegate += OpenCases;
                }
            }
            else
            {
                OpenCases();
            }
        }

        void OpenCases()
        {
            openDelegate -= OpenCases;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenCases;
            }
            else
            {
                int caseID = ((HospitalCasesManager)AreaMapController.Map.casesManager).casesStack[((HospitalCasesManager)AreaMapController.Map.casesManager).casesStack.Count - 1];
                UIController.get.ExitAllPopUps();
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () => { 
                    HospitalUIPrefabController.Instance.HideMainUI();
                    ShowCase(caseID);
                }));
            }
        }

        [TutorialTriggerable]
        public void OpenVIPCasePopup()
        {
            if (!TutorialSystem.TutorialController.ShowTutorials)
                return;

            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromVIP = false;
                openDelegate += OpenVIPCase;
            }
            else
            {
                OpenVIPCase();
            }
        }

        public void OpenVIPCase()
        {
            openDelegate -= OpenVIPCase;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenVIPCase;
            }
            else
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromVIP = true;
                UIController.get.ExitAllPopUps();
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    ShowVIPCase();
                }));
            }
        }

        public void OpenEpidemyCasePopup()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromEpidemy = false;
                openDelegate += OpenEpidemyCase;
            }
            else
            {
                OpenEpidemyCase();
            }
        }

        public void OpenEpidemyCase()
        {
            openDelegate -= OpenEpidemyCase;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenEpidemyCase;
            }
            else
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromEpidemy = true;
                UIController.get.ExitAllPopUps();
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    ShowEpidemyCase();
                }));
            }
        }

        public void OpenFacebookRewardCasePopup()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromFacebook = false;
                openDelegate += OpenFacebookRewardCase;
            }
            else
            {
                OpenFacebookRewardCase();
            }
        }

        public void OpenFacebookRewardCase()
        {
            openDelegate -= OpenFacebookRewardCase;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenFacebookRewardCase;
            }
            else
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromFacebook = true;
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    ShowFacebookRewardCase();
                }));
            }
        }

        public void OpenTreasureCasePopup()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromTreasure = false;
                openDelegate += OpenTreasureCase;
            }
            else
            {
                OpenTreasureCase();
            }
        }

        public void OpenTreasureCase()
        {
            openDelegate -= OpenTreasureCase;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenTreasureCase;
            }
            else
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromTreasure = true;
                UIController.get.ExitAllPopUps();
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    ShowTreasureCase();
                }));
            }
        }

        public void OpenDailyQuestRewardPopup()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromDailyQuest = false;
                openDelegate += OpenDailyQuestReward;
            }
            else
            {
                OpenDailyQuestReward();
            }
        }

        public void OpenDailyQuestReward()
        {
            openDelegate -= OpenDailyQuestReward;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenDailyQuestReward;
            }
            else
            {
                //Debug.LogError("OpenDailyQuestReward");
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromDailyQuest = true;
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    AreaMapController.Map.casesManager.ChooseCardType();
                }));
            }
        }

        public void OpenDailyRewardPopup()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).DailyRewardBundledGift = false;
                openDelegate += OpenDailyReward;
            }
            else
            {
                OpenDailyReward();
            }
        }

        public void OpenDailyReward()
        {
            openDelegate -= OpenDailyReward;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenDailyReward;
            }
            else
            {
                //Debug.LogError("OpenDailyQuestReward");
                ((HospitalCasesManager)AreaMapController.Map.casesManager).DailyRewardBundledGift = true;
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    AreaMapController.Map.casesManager.ChooseCardType();
                }));
            }
        }

        public void OpenBundledGlobalEventRewardPopup()
        {
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                ((HospitalCasesManager)AreaMapController.Map.casesManager).BundledGiftFromGlobalEvent = false;
                openDelegate += OpenBundledGlobalEventReward;
            }
            else
            {
                OpenBundledGlobalEventReward();
            }
        }

        public void OpenBundledGlobalEventReward()
        {
            openDelegate -= OpenBundledGlobalEventReward;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenBundledGlobalEventReward;
            }
            else
            {
                //Debug.LogError("OpenDailyQuestReward");
                ((HospitalCasesManager)AreaMapController.Map.casesManager).BundledGiftFromGlobalEvent = true;
                UIController.get.ExitAllPopUps();
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    ShowGlobalEventBundleRewardCase(((HospitalCasesManager)Hospital.AreaMapController.Map.casesManager).bundledGift);
                }));
            }
        }

        public void OpenSingleGlobalEventReward()
        {
            openDelegate -= OpenSingleGlobalEventReward;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenSingleGlobalEventReward;
            }
            else
            {
                //Debug.LogError("OpenDailyQuestReward");
                ((HospitalCasesManager)AreaMapController.Map.casesManager).SingleGiftFromGlobalEvent = true;
                gameObject.SetActive(true);
                StartCoroutine(Open(true, false, () =>
                {
                    HospitalUIPrefabController.Instance.HideMainUI();
                    AreaMapController.Map.casesManager.ChooseCardType();
                }));
            }
        }

        public void OpenGlobalEventRewardPopup(int caseType)
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromGlobalEvent = true;
            ((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack.Add(caseType);
            ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromGlobalEvent = false;
            openDelegate += OpenGlobalEventsReward;
            unboxingPending = true;
            Invoke("DelayedOpenGlobalEventRewardPopUp", 2.25f);
        }

        public void OpenLootBox(LootBox.Box lootBoxType, List<GiftReward> rewards, bool isGlobalEvent = false, bool instantOpen = false, string tag = "")
        {
            int caseGfxIndex = 0;
            this.instantOpen = instantOpen;

            switch (lootBoxType)
            {
                case LootBox.Box.blue:
                    caseGfxIndex = 11;
                    break;
                case LootBox.Box.pink:
                    caseGfxIndex = 12;
                    break;
                case LootBox.Box.xmas:
                    caseGfxIndex = 13;
                    break;
                default:
                    caseGfxIndex = 0;
                    break;
            }

            if (isGlobalEvent)
                ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromGlobalEvent = true;
            else
                AreaMapController.Map.casesManager.GiftFromIAP = true;

            bool openRoutinePath = false;

            if (rewards != null && rewards.Count > 0)
            {
                if (AreaMapController.Map.casesManager.lootBoxRewards != null && AreaMapController.Map.casesManager.lootBoxRewards.Count > 0)
                    AreaMapController.Map.casesManager.lootBoxRewards.Clear();

                for (int i = 0; i < rewards.Count; i++)
                {
                    AreaMapController.Map.casesManager.lootBoxRewards.Add(rewards[i]);
                }

                if (isGlobalEvent)
                {
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack.Add(caseGfxIndex);
                    if (instantOpen)
                    {
                        if (((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack.Count > 0)
                        {
                            int lastglobalEventStack = ((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack[0];
                            ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromGlobalEvent = true;
                            openRoutinePath = true;
                            gameObject.SetActive(true);
                            StartCoroutine(Open(true, false, () =>
                            {
                                HospitalUIPrefabController.Instance.HideMainUI();
                                ShowCaseGE(lastglobalEventStack, true);
                                ((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack.Remove(lastglobalEventStack);
                                unboxingPending = false;
                                return;
                            }));
                        }
                        else
                        {
                            unboxingPending = false;
                            return;
                        }
                    }
                    else
                    {
                        openDelegate += OpenGlobalEventsReward;
                    }
                }
                else
                {
                    AreaMapController.Map.casesManager.iapStack.Add(new IAPCaseData(caseGfxIndex, tag));
                    openDelegate += OpenIAPReward;
                }
                if (!openRoutinePath)
                {
                    unboxingPending = true;
                    Invoke("DelayedOpenGlobalEventRewardPopUp", 2.25f);
                }
            }
        }

        void DelayedOpenGlobalEventRewardPopUp()
        {
            openDelegate.Invoke();
            unboxingPending = false;
        }

        public void OpenGlobalEventsReward()
        {
            openDelegate -= OpenGlobalEventsReward;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenGlobalEventsReward;
            }
            else
            {
                if (((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack.Count > 0)
                {
                    int lastglobalEventStack = ((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack[0];

                    Debug.LogError("OpenGlobalEventsReward");
                    ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromGlobalEvent = true;
                    gameObject.SetActive(true);
                    StartCoroutine(Open(true, false, () =>
                    {
                        HospitalUIPrefabController.Instance.HideMainUI();
                        ShowCaseGE(lastglobalEventStack);

                        ((HospitalCasesManager)AreaMapController.Map.casesManager).globalEventStack.Remove(lastglobalEventStack);
                    }));
                }
                //HospitalAreasMapController.Map.casesManager.ChooseCardType();
            }
        }

        public void OpenIAPReward()
        {
            openDelegate -= OpenIAPReward;
            if (UIController.getHospital.unboxingPopUp.gameObject.activeSelf)
            {
                openDelegate += OpenIAPReward;
            }
            else
            {
                if (AreaMapController.Map.casesManager.iapStack.Count > 0)
                {
                    var lastIAPStackObject = AreaMapController.Map.casesManager.iapStack[0];

                    if (lastIAPStackObject != null)
                    {
                        int iapID = lastIAPStackObject.case_id;
                        string iapTAG = lastIAPStackObject.case_tag;

                        Debug.LogError("OpenIAPsEventsReward");
                        AreaMapController.Map.casesManager.GiftFromIAP = true;
                        gameObject.SetActive(true);
                        StartCoroutine(Open(true, false, () =>
                        {
                            HospitalUIPrefabController.Instance.HideMainUI();
                            ShowCaseIAP(iapID, iapTAG);
                            AreaMapController.Map.casesManager.iapStack.Remove(lastIAPStackObject);
                        }));
                    }
                    else
                        AreaMapController.Map.casesManager.iapStack.Remove(lastIAPStackObject);
                }
                //HospitalAreasMapController.Map.casesManager.ChooseCardType();
            }
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            VisitingController.Instance.canVisit = false;
            yield return base.Open(isFadeIn, preservesHovers);

            //Debug.LogError("Open unboxing");
            dayText.text = "";
            isFromGlobalEvent = false;

            if (!AreaMapController.Map.casesManager.GiftFromIAP)
                AreaMapController.Map.casesManager.GivePrizesAndSave();

            whenDone?.Invoke();
        }

        public void OpenGlobalEventPrize(bool isFadeIn = true, bool preservesHovers = false)
        {
            int rewardsCount = ReferenceHolder.GetHospital().globalEventController.GetGlobalEventRewardForReloadSpawn().Count;

            if (rewardsCount > 0)
            {
                GlobalEventRewardModel rewardModel = ReferenceHolder.GetHospital().globalEventController.GetGlobalEventRewardForReloadSpawn()[rewardsCount - 1].Value;
                HospitalCasesManager CM = (HospitalCasesManager)Hospital.AreaMapController.Map.casesManager;
                if (rewardModel.GetGlobalEventGift().rewardType == BaseGiftableResourceFactory.BaseResroucesType.bundle)
                {
                    CM.BundledGiftFromGlobalEvent = true;
                    CM.AddBundleGift((BundleGiftableResource)rewardModel.GetGlobalEventGift(), EconomySource.GlobalEventReward);
                    UIController.getHospital.unboxingPopUp.OpenBundledGlobalEventRewardPopup();
                }
                else
                {
                    CM.SingleGiftFromGlobalEvent = true;
                    CM.AddSingleGift(rewardModel.GetGlobalEventGift());
                    UIController.getHospital.unboxingPopUp.OpenSingleGlobalEventReward();
                }
            }
            ReferenceHolder.GetHospital().globalEventController.GetGlobalEventRewardForReloadSpawn().RemoveAt(rewardsCount - 1);
        }

        public void OpenGiftFromFriend(List<Giver> giversList, GiftReward wiseGift, bool isFadeIn = true, bool preservesHovers = false)
        {
            HospitalUIPrefabController.Instance.HideMainUI();
            ((HospitalCasesManager)AreaMapController.Map.casesManager).SetGiftsToShow(giversList, wiseGift);
            VisitingController.Instance.canVisit = false;
            gameObject.SetActive(true);
            StartCoroutine(base.Open(isFadeIn, preservesHovers, () =>
            {
                dayText.text = "";
                topText.text = "";

                boxContent.SetActive(false);
                cardContent.SetActive(true);

                if (((HospitalCasesManager)AreaMapController.Map.casesManager).giftsToShow.Count + (((HospitalCasesManager)AreaMapController.Map.casesManager).giftsToShow != null ? 1 : 0) > 0)
                {
                    boxContent.SetActive(true);
                    boxAnimator.SetTrigger("Idle");
                    cardContent.SetActive(true);
                    ShowFriendGift();
                }
                else
                {
                    boxContent.SetActive(false);
                    cardContent.SetActive(false);
                    ExitAfterGiftFromFriend();
                }
            }));
        }

        public void ExitAfterLast()
        {
            base.Exit();

            if (((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromTreasure)
                NotificationCenter.Instance.TreasureCollected.Invoke(new BaseNotificationEventArgs());
            else if (((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromEpidemy)
                NotificationCenter.Instance.EpidemyCompleted.Invoke(new BaseNotificationEventArgs());
            else
                NotificationCenter.Instance.PackageCollected.Invoke(new BaseNotificationEventArgs());

            Timing.RunCoroutine(checkOtherCases());
        }

        public void ExitAfterGlobalEventReward()
        {
            isFromGlobalEvent = false;
            VisitingController.Instance.canVisit = true;
            base.Exit();
        }

        public void ExitAfterGiftFromFriend()
        {
            isFromFriendGift = false;
            VisitingController.Instance.canVisit = true;
            base.Exit();
        }

        public void OpenBoxAnim()
        {
            boxAnimator.SetTrigger(((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromTreasure ? "OpenChest" : "OpenBox");            
        }

        public void ShowCase(int caseID)
        {
            //Debug.LogError("ShowCase id: " + caseID);
            switch (caseID)
            {
                case 0:
                    topText.text = I2.Loc.ScriptLocalization.Get("GIFT_BOXES_NAME_1");
                    break;
                case 1:
                    topText.text = I2.Loc.ScriptLocalization.Get("GIFT_BOXES_NAME_2");
                    break;
                case 2:
                    topText.text = I2.Loc.ScriptLocalization.Get("GIFT_BOXES_NAME_3");
                    break;

                default:
                    break;
            }
            dayText.text = "";

            boxContent.SetActive(true);
            cardContent.SetActive(false);
            RefreshCoverImage(caseID);
            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowCaseGE(int caseID, bool globalEventReward = false)
        {
            //Debug.LogError("ShowCase id: " + caseID);

            dayText.text = "";

            if (globalEventReward)
                topText.text = I2.Loc.ScriptLocalization.Get("REWARD");
            else
                topText.text = I2.Loc.ScriptLocalization.Get("EVENTS/PERSONAL_EVENT").ToUpper() + " " + I2.Loc.ScriptLocalization.Get("REWARD");
            //topText.text = I2.Loc.ScriptLocalization.Get("REWARD");

            boxContent.SetActive(true);
            cardContent.SetActive(false);
            RefreshCoverImage(caseID);
            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowCaseIAP(int caseID, string tag)
        {
            //Debug.LogError("ShowCase id: " + caseID);
            dayText.text = "";
            topText.text = I2.Loc.ScriptLocalization.Get(tag + "_BOX").ToUpper();
            //topText.text = I2.Loc.ScriptLocalization.Get("REWARD");

            boxContent.SetActive(true);
            cardContent.SetActive(false);
            RefreshCoverImage(caseID);
            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowVIPCase()
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            RefreshCaseOfID(3);
            topText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_VIP_GIFT");
            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowEpidemyCase()
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            RefreshCaseOfID(4);
            topText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_THANK_YOU_REWARD");
            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowFacebookRewardCase()
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            RefreshCaseOfID(3);
            topText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_FB_REWARD");
            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowTreasureCase()
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            RefreshCaseOfID(5, true);
            topText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TREASURE");
            boxAnimator.SetTrigger("ShowChest");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowDailyQuestCase(RewardPackage rewardPackage)
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            //daily quest cases have ids: 6,7,8,9,10 (1star, 2star, 3star, super, super duper)
            switch (rewardPackage.PackageRewardQuality)
            {
                case RewardPackage.RewardQuality.Starx1:
                    RefreshCaseOfID(6);
                    topText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/REWARD_1_STAR");
                    dayText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_X"), rewardPackage.DayCorespondingToRewardPackage);
                    Timing.RunCoroutine(ShowDqParticles(0));
                    break;
                case RewardPackage.RewardQuality.Starx2:
                    RefreshCaseOfID(7);
                    topText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/REWARD_2_STAR");
                    dayText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_X"), rewardPackage.DayCorespondingToRewardPackage);
                    Timing.RunCoroutine(ShowDqParticles(1));
                    break;
                case RewardPackage.RewardQuality.Starx3:
                    RefreshCaseOfID(8);
                    topText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/REWARD_3_STAR");
                    dayText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_X"), rewardPackage.DayCorespondingToRewardPackage);
                    Timing.RunCoroutine(ShowDqParticles(2));
                    break;
                case RewardPackage.RewardQuality.Super:
                    RefreshCaseOfID(9);
                    topText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/SUPER_REWARD");
                    dayText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/BONUS");
                    Timing.RunCoroutine(ShowDqParticles(3));
                    break;
                case RewardPackage.RewardQuality.SuperGrand:
                    RefreshCaseOfID(10);
                    topText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/SUPER_GRAND_REWARD");
                    dayText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/BONUS");
                    Timing.RunCoroutine(ShowDqParticles(4));
                    break;
                default:
                    Debug.LogError("This DQ reward quality is not handled for unboxing! CALL MIKKO, BLAME PIETA!");
                    break;
            }

            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        public void ShowDailyRewardCase(BundleGiftableResource rewardPackage)
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            //daily quest cases have ids: 6,7,8,9,10 (1star, 2star, 3star, super, super duper)
            coverImage.gameObject.SetActive(true);

            boxImage.sprite = ResourcesHolder.Get().bundledPackagesReferences.GetBottomBoxIconForBox(rewardPackage.GetBoxImageCoverType());
            coverImage.sprite = ResourcesHolder.Get().bundledPackagesReferences.GetTopBoxIconForBox(rewardPackage.GetBoxImageCoverType());
            topText.text = I2.Loc.ScriptLocalization.Get(rewardPackage.GetLocalizationKey());
            Timing.RunCoroutine(ShowDqParticles(0));

            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        IEnumerator<float> ShowDqParticles(int index)
        {
            yield return Timing.WaitForSeconds(0.1f);
            ((HospitalCasesManager)AreaMapController.Map.casesManager).dqParticles[index].Play();
            Debug.Log("PLAY PARTICLES");
        }

        public void ShowItemCard(CasePrizeType prizeType)
        {
            boxContent.SetActive(true);
            cardContent.SetActive(true);

            switch (prizeType)
            {
                case CasePrizeType.Coins:
                    prizeName.SetText(I2.Loc.ScriptLocalization.Get("COINS"));
                    amountText.SetText("x" + AreaMapController.Map.casesManager.casePrize.coinsAmount.ToString());
                    prizeImage.GetComponent<Image>().sprite = AreaMapController.Map.casesManager.goldStack;
                    break;

                case CasePrizeType.Diamonds:
                    prizeName.SetText(I2.Loc.ScriptLocalization.Get("DIAMONDS"));
                    amountText.SetText("x" + AreaMapController.Map.casesManager.casePrize.diamondsAmount.ToString());
                    prizeImage.GetComponent<Image>().sprite = AreaMapController.Map.casesManager.diamondsChest;
                    break;

                case CasePrizeType.SpecialItem:
                    prizeName.SetText(ResourcesHolder.Get().GetNameForCure(AreaMapController.Map.casesManager.casePrize.items[AreaMapController.Map.casesManager.casePrize.items.Count - 1].item));
                    amountText.SetText("x" + AreaMapController.Map.casesManager.casePrize.items[AreaMapController.Map.casesManager.casePrize.items.Count - 1].amount.ToString());
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(AreaMapController.Map.casesManager.casePrize.items[AreaMapController.Map.casesManager.casePrize.items.Count - 1].item);
                    break;

                case CasePrizeType.Decoration:
                    prizeName.SetText(I2.Loc.ScriptLocalization.Get(AreaMapController.Map.casesManager.casePrize.decorations[AreaMapController.Map.casesManager.casePrize.decorations.Count - 1].decoration.ShopTitle));
                    amountText.SetText("x" + AreaMapController.Map.casesManager.casePrize.decorations[AreaMapController.Map.casesManager.casePrize.decorations.Count - 1].amount.ToString());
                    prizeImage.GetComponent<Image>().sprite = AreaMapController.Map.casesManager.casePrize.decorations[AreaMapController.Map.casesManager.casePrize.decorations.Count - 1].decoration.ShopImage;
                    break;

                case CasePrizeType.Booster:
                    prizeName.SetText(I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[AreaMapController.Map.casesManager.casePrize.boosters[AreaMapController.Map.casesManager.casePrize.boosters.Count - 1].boosterID].shortInfo));
                    amountText.SetText("x" + AreaMapController.Map.casesManager.casePrize.boosters[AreaMapController.Map.casesManager.casePrize.boosters.Count - 1].amount.ToString());
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().boosterDatabase.boosters[AreaMapController.Map.casesManager.casePrize.boosters[AreaMapController.Map.casesManager.casePrize.boosters.Count - 1].boosterID].icon;
                    break;

                case CasePrizeType.PositiveEnergy:
                    MedicineRef positiveRef = MedicineRef.Parse("16(00)");
                    prizeName.SetText(ResourcesHolder.Get().GetNameForCure(positiveRef));
                    amountText.SetText("x" + AreaMapController.Map.casesManager.casePrize.positiveEnergyAmount.ToString());
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(positiveRef);
                    break;

                default:
                    break;
            }
            cardAnimator.enabled = true;
            cardAnimator.SetTrigger("Bounce");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_COLLECT");
        }

        public void ShowGlobalEventReward(GlobalEventRewardModel reward)
        {
            BaseGiftableResource gift = reward.GetGlobalEventGift();

            //if (reward.RewardType == GlobalEventRewardPackage.GlobalEventRewardType.LootBox)
            if (gift.rewardType == BaseGiftableResourceFactory.BaseResroucesType.bundle)
            {
                boxContent.SetActive(false);
                cardContent.SetActive(false);
                //reward.Collect(Vector2.zero, true);
                //reward.CollectReward(true, false);
            }
            else
            {
                boxContent.SetActive(true);
                cardContent.SetActive(true);

                prizeName.SetText(/*reward.GetName()*/ I2.Loc.ScriptLocalization.Get(gift.GetLocalizationKey()));
                amountText.SetText("x" + gift.GetGiftAmount());
                prizeImage.GetComponent<Image>().sprite = gift.GetIconForGift(); //reward.GetSprite();

                //switch (gift.rewardType)
                //{
                //    case BaseGiftableResourceFactory.BaseResroucesType.coin: //GlobalEventRewardPackage.GlobalEventRewardType.Coin:
                //        Debug.LogError("To Implement!");
                //        break;
                //    case BaseGiftableResourceFactory.BaseResroucesType.diamond:  // GlobalEventRewardPackage.GlobalEventRewardType.Diamond:
                //        prizeName.SetText(I2.Loc.ScriptLocalization.Get("DIAMONDS"));
                //        amountText.SetText("x" + gift.GetGiftAmount());
                //        prizeImage.GetComponent<Image>().sprite = AreaMapController.Map.casesManager.diamondsChest;
                //        break;
                //    case BaseGiftableResourceFactory.BaseResroucesType.medicine: //GlobalEventRewardPackage.GlobalEventRewardType.Medicine:
                //        Debug.LogError("To Implement!");
                //        break;
                //    case BaseGiftableResourceFactory.BaseResroucesType.decoration: //GlobalEventRewardPackage.GlobalEventRewardType.Decoration:
                //        //TODO_Duobix
                //        prizeName.SetText(/*reward.GetName()*/ I2.Loc.ScriptLocalization.Get(gift.GetLocalizationKey()) );
                //        amountText.SetText("x" + gift.GetGiftAmount());
                //        prizeImage.GetComponent<Image>().sprite = gift.GetIconForGift(); //reward.GetSprite();
                //        break;
                //    /*
                //    case GlobalEventRewardPackage.GlobalEventRewardType.Default:
                //        Debug.LogError("To Implement!");
                //        break;
                //        */
                //    default:
                //        Debug.LogError("Case not found!");
                //        break;
                //}

                cardAnimator.enabled = true;
                cardAnimator.SetTrigger("Bounce");
                bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_COLLECT");
                topText.text = I2.Loc.ScriptLocalization.Get("REWARD");
            }
        }

        public void ShowFriendGift()
        {
            isFromFriendGift = true;
            GiftReward reward = null;
            if (((HospitalCasesManager)AreaMapController.Map.casesManager).wiseGiftToShow != null)
                reward = ((HospitalCasesManager)AreaMapController.Map.casesManager).wiseGiftToShow;
            else
                reward = ((HospitalCasesManager)AreaMapController.Map.casesManager).giftsToShow[0].reward;

            boxContent.SetActive(true);
            cardContent.SetActive(true);

            switch (reward.rewardType)
            {
                case GiftRewardType.Coin:
                    prizeName.SetText(I2.Loc.ScriptLocalization.Get("COINS"));
                    amountText.SetText("x" + reward.amount);
                    prizeImage.GetComponent<Image>().sprite = AreaMapController.Map.casesManager.goldStack;
                    break;
                case GiftRewardType.Diamond:
                    prizeName.SetText(I2.Loc.ScriptLocalization.Get("DIAMONDS"));
                    amountText.SetText("x" + reward.amount);
                    prizeImage.GetComponent<Image>().sprite = AreaMapController.Map.casesManager.diamondsChest;
                    break;
                case GiftRewardType.Mixture:
                    prizeName.SetText(ResourcesHolder.Get().GetNameForCure(((GiftRewardMixture)reward).GetRewardMedicineRef()));
                    amountText.SetText("x" + reward.amount);
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(((GiftRewardMixture)reward).GetRewardMedicineRef());
                    break;
                case GiftRewardType.StorageUpgrader:
                    prizeName.SetText(ResourcesHolder.Get().GetNameForCure(((GiftRewardStorageUpgrader)reward).GetRewardMedicineRef()));
                    amountText.SetText("x" + reward.amount);
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(((GiftRewardStorageUpgrader)reward).GetRewardMedicineRef());
                    break;
                case GiftRewardType.Shovel:
                    prizeName.SetText(ResourcesHolder.Get().GetNameForCure(((GiftRewardShovel)reward).GetRewardMedicineRef()));
                    amountText.SetText("x" + reward.amount);
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(((GiftRewardShovel)reward).GetRewardMedicineRef());
                    break;
                case GiftRewardType.PositiveEnergy:
                    MedicineRef positiveRef = MedicineRef.Parse("16(00)");
                    prizeName.SetText(ResourcesHolder.Get().GetNameForCure(positiveRef));
                    amountText.SetText("x" + reward.amount);
                    prizeImage.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(positiveRef);
                    break;
                default:
                    break;
            }

            cardAnimator.enabled = true;
            cardAnimator.SetTrigger("Bounce");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_COLLECT");

            if (((HospitalCasesManager)AreaMapController.Map.casesManager).wiseGiftToShow != null)
                topText.text = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_RECEIVE").Replace("{0}", I2.Loc.ScriptLocalization.Get("FRIENDS_DR_WISE").ToUpper());
            else
                topText.text = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_RECEIVE").Replace("{0}", GetFriendName(((HospitalCasesManager)AreaMapController.Map.casesManager).giftsToShow[0], AccountManager.Instance.IsFacebookConnected).ToUpper());
        }

        public void ShowGlobalEventBundleRewardCase(BundleGiftableResource rewardPackage)
        {
            boxContent.SetActive(true);
            cardContent.SetActive(false);

            coverImage.gameObject.SetActive(true);

            boxImage.sprite = ResourcesHolder.Get().bundledPackagesReferences.GetBottomBoxIconForBox(rewardPackage.GetBoxImageCoverType());
            coverImage.sprite = ResourcesHolder.Get().bundledPackagesReferences.GetTopBoxIconForBox(rewardPackage.GetBoxImageCoverType());
            topText.text = I2.Loc.ScriptLocalization.Get(rewardPackage.GetLocalizationKey());

            boxAnimator.SetTrigger("ShowBox");
            bottomText.text = bottomText.text = I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }

        private string GetFriendName(IFollower follower, bool FromFb = false)
        {
            string name = "";

            if (DisplayInFacebookMode(follower, FromFb))
            {
                if (follower.IsFacebookDataDownloaded())
                {
                    name = follower.Name;
                }
                else
                {
                    name = follower.Name;
                }
            }
            else
            {
                name = follower.Name;
            }

            return name;
        }

        private bool DisplayInFacebookMode(IFollower person, bool FromFb = false)
        {
            return (!string.IsNullOrEmpty(person.FacebookID) || FromFb) && AccountManager.Instance.IsFacebookConnected;
        }

        private void RefreshCoverImage(int caseID, bool altCover = false)
        {
            coverImage2.gameObject.SetActive(altCover);
            coverImage.gameObject.SetActive(!altCover);

            boxImage.sprite = AreaMapController.Map.casesManager.openBoxSprites[caseID];
            if (altCover)
                coverImage2.sprite = AreaMapController.Map.casesManager.openCoverSprites[caseID];
            else
                coverImage.sprite = AreaMapController.Map.casesManager.openCoverSprites[caseID];
                Debug.Log("CASE ID IS " + caseID);
        }

        private void RefreshCaseOfID(int ID, bool altCover = false)
        {
            coverImage2.gameObject.SetActive(altCover);
            coverImage.gameObject.SetActive(!altCover);

            boxImage.sprite = AreaMapController.Map.casesManager.openBoxSprites[ID];
            if (altCover)
                coverImage2.sprite = ((HospitalCasesManager)AreaMapController.Map.casesManager).openCoverSprites[ID];
            else
                coverImage.sprite = ((HospitalCasesManager)AreaMapController.Map.casesManager).openCoverSprites[ID];
        }

        IEnumerator<float> checkOtherCases()
        {
            yield return Timing.WaitForSeconds(1);
            openDelegate?.Invoke();
        }
    }
}
