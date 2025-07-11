using Hospital;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using MovementEffects;
using SimpleUI;
using UnityEngine.UI;

public class HospitalCasesManager : CasesManager
{
    public int deliveryIntervalSeconds = 21600;
    //private List<int> deliveryIntervalsSeconds = null;
    [SerializeField] int[] collectXP = new int[2];
    [SerializeField] private GameObject DeliveryCar = null;
    public int[] getNowPrices = new int[3];

    public int countingStartTime = 0;
    //[HideInInspector] public int secondsLeft = 0;

    [HideInInspector] public int[] casesStorage = new int[3];
    [HideInInspector] public bool[] canCollect = new bool[3];
    [HideInInspector] public bool[] canDeliver = new bool[3];

    delegate void ButtonDelegate(int i);
    ButtonDelegate[] collectDelegates = new ButtonDelegate[3];
    ButtonDelegate[] deliverDelegates = new ButtonDelegate[3];

    public ParticleSystem[] dqParticles;

    [SerializeField] private Transform casesDepot = null;
    [HideInInspector] public List<int> casesStack = new List<int>();
    [HideInInspector] public List<RewardPackage> dailyQuestRewardStack = new List<RewardPackage>();
    [HideInInspector] public List<int> globalEventStack = new List<int>();
    [HideInInspector] public BundleGiftableResource bundledGift;
    [HideInInspector] public BaseGiftableResource singleGift;

    [SerializeField] private CasePrizesParams casePrizesParams = null;
    public CasePrizesParams GetCasePrizesParams { get { return casePrizesParams; } }

    private CasePrize tempCasePrize = null;

    [HideInInspector] public bool GiftFromVIP = false;
    [HideInInspector] public bool GiftFromFacebook = false;
    [HideInInspector] public bool GiftFromEpidemy = false;
    [HideInInspector] public bool GiftFromTreasure = false;
    [HideInInspector] public bool GiftFromDailyQuest = false;
    [HideInInspector] public bool GiftFromGlobalEvent = false;
    [HideInInspector] public bool BundledGiftFromGlobalEvent = false;
    [HideInInspector] public bool SingleGiftFromGlobalEvent = false;
    [HideInInspector] public bool DailyRewardBundledGift = false;
    [HideInInspector] public bool deliveryCarBusy = false;
    [HideInInspector] public bool casesDelivered = true;

    private int deliveredPackages = 0;

    private IEnumerator<float> deliveryWaitingCorountine;

    private int lastCaseStackID = -1;

    public GiftReward wiseGiftToShow = null;
    public List<Giver> giftsToShow = new List<Giver>();

    private void Awake()
    {
        casePrizeGenerator = new CasePrizeGenerator(casePrizesParams, decorationsToDraw, lootBoxRewards);
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(deliveredPackages.ToString());
        builder.Append("?");
        builder.Append(countingStartTime.ToString());
        builder.Append("?");
        for (int i = 0; i < casesStorage.Length; ++i)
        {
            builder.Append(Checkers.CheckedAmount(casesStorage[i], 0, int.MaxValue, "Cases storage " + i + ": ").ToString());
            if (i < casesStorage.Length - 1)
                builder.Append("!");
        }
        if (casesStack.Count > 0 || DeliveryCar.GetComponent<DeliveryCarController>().GetCargo().Count > 0)
        {
            builder.Append("?");

            for (int i = 0; i < casesStack.Count; ++i)
            {
                builder.Append(Checkers.CheckedAmount(casesStack[i], 0, 2, "Case " + i + ": ").ToString());
                if (i < casesStack.Count - 1)
                    builder.Append("!");
            }
            if (DeliveryCar != null)
            {
                if (TutorialController.Instance.CurrentTutorialStepTag > StepTag.package_arrive)
                {
                    if (DeliveryCar.GetComponent<DeliveryCarController>().GetCargo().Count > 0)
                        builder.Append("!");
                    for (int i = 0; i < DeliveryCar.GetComponent<DeliveryCarController>().GetCargo().Count; ++i)
                    {
                        builder.Append(Checkers.CheckedAmount(DeliveryCar.GetComponent<DeliveryCarController>().GetCargo()[i], 0, 2, "Cargo " + i + ": ").ToString());
                        if (i < DeliveryCar.GetComponent<DeliveryCarController>().GetCargo().Count - 1)
                            builder.Append("!");
                    }
                }
            }

            if (casePrize != null && tempCasePrize == null && !GiftFromVIP && !GiftFromFacebook)
            {
                builder.Append("?");
                builder.Append(casePrize.ToString());
            }
            else if (tempCasePrize != null)
            {
                builder.Append("?");
                builder.Append(tempCasePrize.ToString());
            }
        }

        return builder.ToString();
    }

    public override void LoadFromString(string saveString, TimePassedObject timeFromSave, bool VisitingMode)
    {
        casesStack.Clear();
        RefreshDepot();

        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split('?');

            if (deliveryWaitingCorountine != null)
            {
                Timing.KillCoroutine(deliveryWaitingCorountine);
                deliveryWaitingCorountine = null;
            }

            if (!VisitingMode)
            {
                if (TutorialController.Instance.IsTutorialStepCompleted(StepTag.package_collected))
                {
                    deliveredPackages = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
                    countingStartTime = int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture);
                    if (countingStartTime == 0)
                        countingStartTime = Convert.ToInt32((long)ServerTime.getTime());

                    int secondsPassed = Convert.ToInt32((long)ServerTime.getTime()) - countingStartTime;
                    SetDeliveryIntervalSeconds();
                    if (secondsPassed < deliveryIntervalSeconds)
                        deliveryWaitingCorountine = Timing.RunCoroutine(DeliveryWaiting());
                    else
                        UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOffCounting();
                }
            }
            else
                UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOffCounting();

            var storage = save[2].Split('!');
            for (int i = 0; i < storage.Length; ++i)
            {
                casesStorage[i] = int.Parse(storage[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            if (save.Length > 3)
            {
                var savedStack = save[3].Split('!');
                casesStack.Clear();
                if (DeliveryCar.activeInHierarchy)
                    DeliveryCar.SetActive(false);

                for (int i = 0; i < casesDepot.childCount; ++i)
                {
                    casesDepot.GetChild(i).gameObject.SetActive(false);
                }

                for (int i = 0; i < savedStack.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(savedStack[i]))
                        DeployCase(int.Parse(savedStack[i], System.Globalization.CultureInfo.InvariantCulture));
                }

                if (save.Length > 4)
                    casePrize = CasePrize.Parse(save[4]);
            }
        }
        RefreshCases();
        CheckAlert();
    }

    protected override void RefreshCases(bool refreshStorage = true)
    {
        //FIRST
        int secondsPassed = Convert.ToInt32((long)ServerTime.getTime()) - countingStartTime;

        if (secondsPassed >= deliveryIntervalSeconds && TutorialController.Instance.IsTutorialStepCompleted(StepTag.package_click))
        {
            canDeliver[0] = true;
            deliverDelegates[0] = DeliverCase;
            if (casesStorage[0] < 4)
            {
                canCollect[0] = true;
                collectDelegates[0] = CollectCase;
            }
            else
            {
                canCollect[0] = false;
                collectDelegates[0] = null;
            }
        }
        else
        {
            canDeliver[0] = false;
            deliverDelegates[0] = null;
        }
        //SECOND
        if (casesStorage[0] > 3)
        {
            canDeliver[1] = true;
            deliverDelegates[1] = DeliverCase;
            if (casesStorage[1] < 4)
            {
                canCollect[1] = true;
                collectDelegates[1] = CollectCase;
            }
            else
            {
                canCollect[1] = false;
                collectDelegates[1] = null;
            }
        }
        else
        {
            canDeliver[1] = false;
            deliverDelegates[1] = null;

        }
        //THIRD
        if (casesStorage[1] > 3)
        {
            canDeliver[2] = true;
            deliverDelegates[2] = DeliverCase;
        }
        else
        {
            canDeliver[2] = false;
            deliverDelegates[2] = null;
        }
        canCollect[2] = false;
        collectDelegates[2] = null;

        ActualizePrices();
        UIController.getHospital.casesPopUpController.RefreshPopUP(refreshStorage);
    }

    [TutorialTriggerable]
    public void StartCounting()
    {
        SetDeliveryIntervalSeconds();
        countingStartTime = Convert.ToInt32((long)ServerTime.getTime());

        if (deliveryWaitingCorountine != null)
        {
            Timing.KillCoroutine(deliveryWaitingCorountine);
            deliveryWaitingCorountine = null;
        }
        deliveryWaitingCorountine = Timing.RunCoroutine(DeliveryWaiting());
    }

    public override void CardClicked()
    {
        UIController.getHospital.unboxingPopUp.cardContent.SetActive(false);
        UIController.getHospital.unboxingPopUp.cardAnimator.enabled = false;
        //GivePrize ();

        if (!UIController.getHospital.unboxingPopUp.isFromFriendGift)
            ShowPrizeAnimation(UIController.getHospital.unboxingPopUp.isFromGlobalEvent);
        else
            ShowGiftRewardAnimation();

        SoundsController.Instance.PlayPrizeCollect();
    }

    [TutorialTriggerable]
    public void StartDeliverCarTutorial()
    {
        if (DeliveryCar.activeSelf)
            return;

        DeliveryCar.GetComponent<DeliveryCarController>().EmptyCargo();
        DeliveryCar.GetComponent<DeliveryCarController>().AddCargo(0);
        StartDeliveryCar();
    }

    public void AddDailyQuestRewardToStack(RewardPackage dailyQuestRewardPackage)
    {
        dailyQuestRewardStack.Add(dailyQuestRewardPackage);
    }

    public void AddBundleGift(BundleGiftableResource bundledReward, EconomySource economySource)
    {
        bundledGift = (BundleGiftableResource)BaseGiftableResourceFactory.CreateBundleGiftableResource(bundledReward.GetBundledGifts(), bundledReward.GetGiftAmount(), economySource, bundledReward.GetLocalizationKey(), bundledReward.GetGiftBoxType(), bundledReward.GetBoxImageCoverType());
    }

    public void AddSingleGift(BaseGiftableResource singleReward)
    {
        singleGift = singleReward;
    }

    public override void ShowPrizeAnimation(bool fromGlobalEvent = false)
    {
        SoundsController.Instance.PlayReward();
        Canvas canvas = UIController.get.canvas;
        RectTransform targetTransform = UIController.getHospital.unboxingPopUp.prizeImage.GetComponent<RectTransform>();

        float centreX = targetTransform.anchoredPosition.x * canvas.scaleFactor + (Screen.width * 0.5f);
        float centreY = targetTransform.anchoredPosition.y * canvas.scaleFactor + (Screen.height * 0.5f);

        Vector3 startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(centreX, centreY, ReferenceHolder.Get().engine.MainCamera.GetCamera().nearClipPlane));
        
        if (casePrize.coinsAmount > 0)
        {
            int coinReward = casePrize.coinsAmount;
            casePrize.coinsAmount = 0;
            ChooseCardType();
            int currentCoinAmount = Game.Instance.gameState().GetCoinAmount() - coinReward;
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, startPoint, coinReward, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), goldStack, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Coin, coinReward, currentCoinAmount);
            });
        }
        else if (casePrize.diamondsAmount > 0)
        {
            int diamondReward = casePrize.diamondsAmount;
            casePrize.diamondsAmount = 0;
            ChooseCardType();
            int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount() - diamondReward;
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, startPoint, diamondReward, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), diamondsChest, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Diamonds, diamondReward, currentDiamondAmount);
            });
        }
        else if (casePrize.positiveEnergyAmount > 0)
        {
            int positiveEnergyReward = casePrize.positiveEnergyAmount;
            casePrize.positiveEnergyAmount = 0;

            ChooseCardType();
            MedicineRef positiveRef = MedicineRef.Parse("16(00)");
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.PositiveEnergy, startPoint, positiveEnergyReward, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(positiveRef), null, null);
        }
        else if (casePrize.items != null && casePrize.items.Count > 0)
        {
            MedicineRef tempSpecial = casePrize.items[casePrize.items.Count - 1].item;
            int tempAmount = casePrize.items[casePrize.items.Count - 1].amount;
            bool isTankElixir = tempSpecial.IsMedicineForTankElixir();

            casePrize.items.RemoveAt(casePrize.items.Count - 1);
            ChooseCardType();
            canAddSpecial = true;

            UIController.get.storageCounter.AddManyLater(tempAmount, isTankElixir, true);

            GiftType giftType = GiftType.Special;
            if (tempSpecial.type != MedicineType.Special)
            {
                giftType = GiftType.Medicine;

                if (HospitalAreasMapController.HospitalMap != null)                
                    HospitalAreasMapController.HospitalMap.hospitalBedController.UpdateAllBedsIndicators(true);

                if (HospitalBedController.isNewCureAvailable)
                {
                    SoundsController.Instance.PlayAlert();
                    UIController.get.CureReadyParticles.Play();
                    Debug.LogError("DING DING DING DING");
                }

                HospitalBedController.isNewCureAvailable = false;
            }

            ReferenceHolder.Get().giftSystem.CreateGiftParticle(giftType, startPoint, tempAmount, 0, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(tempSpecial), () =>
            {
            }, () =>
            {
                UIController.get.storageCounter.Remove(tempAmount, isTankElixir, true);
            });

        }
        else if (casePrize.decorations != null && casePrize.decorations.Count > 0)
        {
            ShopRoomInfo tempInfo = casePrize.decorations[casePrize.decorations.Count - 1].decoration;
            int tempAmount = casePrize.decorations[casePrize.decorations.Count - 1].amount;

            casePrize.decorations.RemoveAt(casePrize.decorations.Count - 1);
            ChooseCardType();

            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Drawer, startPoint, tempAmount, 0, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), tempInfo.ShopImage, null, null);
        }
        else if (casePrize.boosters != null && casePrize.boosters.Count > 0)
        {
            int tempBoosterID = casePrize.boosters[casePrize.boosters.Count - 1].boosterID;
            int tempAmount = casePrize.boosters[casePrize.boosters.Count - 1].amount;
            casePrize.boosters.RemoveAt(casePrize.boosters.Count - 1);
            ChooseCardType();

            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Booster, startPoint, tempAmount, 0, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1.3f, 1.3f, 1), ResourcesHolder.Get().boosterDatabase.boosters[tempBoosterID].icon, () =>
            {
            }, () =>
            {
            });
        }        
    }

    public void CreateDailyQuestCasePrize()
    {
        casePrize = casePrizeGenerator.GetCasePrize(6, CaseType.DAILY_QUEST, EconomySource.DailyQuestReward, 0, dailyQuestRewardStack);
    }

    [TutorialTriggerable] public void SetGiftFromVIP(bool isFromVIP) { GiftFromVIP = isFromVIP; }

    public override void ChooseCardType()
    {
        if (casePrize != null && casePrize.coinsAmount > 0)
            UIController.get.unboxingPopUp.ShowItemCard(CasePrizeType.Coins);
        else if (casePrize != null && casePrize.diamondsAmount > 0)        
            UIController.get.unboxingPopUp.ShowItemCard(CasePrizeType.Diamonds);
        else if (casePrize != null && casePrize.positiveEnergyAmount > 0)
            UIController.get.unboxingPopUp.ShowItemCard(CasePrizeType.PositiveEnergy);
        else if (casePrize != null && casePrize.items != null && casePrize.items.Count > 0)
            UIController.get.unboxingPopUp.ShowItemCard(CasePrizeType.SpecialItem);
        else if (casePrize != null && casePrize.decorations != null && casePrize.decorations.Count > 0)
            UIController.get.unboxingPopUp.ShowItemCard(CasePrizeType.Decoration);
        else if (casePrize != null && casePrize.boosters != null && casePrize.boosters.Count > 0)
            UIController.get.unboxingPopUp.ShowItemCard(CasePrizeType.Booster);
        else
        {
            casePrize = null;
            VisitingController.Instance.canVisit = true;
            if (GiftFromVIP)
            {
                GiftFromVIP = false;
                UIController.get.unboxingPopUp.ExitAfterLast();
                NotificationCenter.Instance.TreasurePopUpClosed.Invoke(new TreasurePopUpClosedArgs());

                VIPSystemManager vs = ReferenceHolder.GetHospital().vipSystemManager;
                if (vs.TotalVIPCured == vs.vipMastership.MasterableConfigData.MasteryGoals[0] && vs.vipMastership.MasteryLevel < 1 && vs.heliMastership.MasteryLevel < 1)
                    NotificationCenter.Instance.CuredVipCountIsEnough.Invoke(new BaseNotificationEventArgs());

                SaveSynchronizer.Instance.InstantSave();
                return;
            }

            if (GiftFromFacebook)
            {
                GiftFromFacebook = false;
                UIController.get.unboxingPopUp.ExitAfterLast();
                Game.Instance.gameState().SetFBRewardConnectionClaimed(true);
                SaveSynchronizer.Instance.InstantSave();
                return;
            }

            if (GiftFromEpidemy)
            {
                UIController.get.unboxingPopUp.ExitAfterLast();
                GiftFromEpidemy = false;
                EpidemyHelicopter.Instance.FlyOutCargo();
                SaveSynchronizer.Instance.InstantSave();
                return;
            }

            if (GiftFromTreasure)
            {
                UIController.getHospital.unboxingPopUp.ExitAfterLast();
                GiftFromTreasure = false;
                SaveSynchronizer.Instance.InstantSave();
                return;
            }

            if (GiftFromDailyQuest)
            {
                //if (dailyQuestRewardStack.Count > 0)
                //{
                //    Debug.Log("Removing from stack quality: " + dailyQuestRewardStack[0].PackageRewardQuality);
                //    for (int i = 0; i < dailyQuestRewardStack[0].RewardListInPackage.Count; i++)
                //    {
                //        Debug.Log("Reward " + i + "type: " + dailyQuestRewardStack[0].RewardListInPackage[i].rewardType + " amount: " + dailyQuestRewardStack[0].RewardListInPackage[i].amount);
                //    }
                //}

                if (dailyQuestRewardStack.Count > 0)
                {
                    //if there are more rewards show them instead of closing, prepare the next case to show
                    UIController.get.storageCounter.SetCounterStartAmount();
                    CreateDailyQuestCasePrize();
                    UIController.getHospital.unboxingPopUp.ShowDailyQuestCase(dailyQuestRewardStack[0]);
                    dailyQuestRewardStack.RemoveAt(0);
                }
                else
                {
                    UIController.getHospital.unboxingPopUp.ExitAfterLast();
                    GiftFromDailyQuest = false;
                    UIController.getHospital.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyQuest);
                    //UIController.getHospital.DailyQuestPopUpUI.Open();
                    SaveSynchronizer.Instance.InstantSave();
                }
                return;
            }

            if (DailyRewardBundledGift)
            {
                if (bundledGift != null)
                {
                    UIController.get.storageCounter.SetCounterStartAmount();
                    CreateDailyRewardCasePrize();
                    UIController.getHospital.unboxingPopUp.ShowDailyRewardCase(bundledGift);
                    bundledGift = null;
                }
                else
                {
                    UIController.getHospital.unboxingPopUp.ExitAfterLast();
                    DailyRewardBundledGift = false;
                    SaveSynchronizer.Instance.InstantSave();
                }
                return;
            }

            if (BundledGiftFromGlobalEvent)
            {
                    UIController.getHospital.unboxingPopUp.ExitAfterLast();
                    BundledGiftFromGlobalEvent = false;
                    SaveSynchronizer.Instance.InstantSave();
                return;
            }

            if (SingleGiftFromGlobalEvent)
            {
                UIController.getHospital.unboxingPopUp.ExitAfterLast();
                SingleGiftFromGlobalEvent = false;
                SaveSynchronizer.Instance.InstantSave();

                return;
            }

            if (GiftFromIAP)
            {
                UIController.getHospital.unboxingPopUp.ExitAfterLast();
                GiftFromIAP = false;
                SaveSynchronizer.Instance.InstantSave();
                return;
            }

            //casesStack.RemoveAt (lastCaseStackID);

            if (casesStack.Count > 0 && canAddSpecial)
            {
                lastCaseStackID = casesStack.Count - 1;
                int caseID = casesStack[casesStack.Count - 1];
                GivePrizesAndSave();
                UIController.get.unboxingPopUp.ShowCase(caseID);
            }
            else
            {
                RefreshDepot();
                UIController.get.unboxingPopUp.ExitAfterLast();
                SaveSynchronizer.Instance.InstantSave();
            }
        }
    }

    private void CreateDailyRewardCasePrize()
    {
        casePrize = casePrizeGenerator.GetCasePrize(6, CaseType.DAILY_REWARD, EconomySource.DailyRewards, 0, null, bundledGift);
    }

    private void CreateBundledGlobalEventCasePrize()
    {
        casePrize = casePrizeGenerator.GetCasePrize(0, CaseType.ordinary, EconomySource.GlobalEventReward, 0, null, bundledGift);
    }

    private void CreateSingleGlobalEventCasePrize()
    {
        casePrize = casePrizeGenerator.GetCasePrizeFromSingleGift(singleGift, EconomySource.GlobalEventReward);
    }

    public override void GivePrizesAndSave()
    {
        if (GiftFromFacebook)
        {
            // tempCasePrize = casePrize;
            casePrize = casePrizeGenerator.GetCasePrize(0, CaseType.FACEBOOK, EconomySource.GiftBoxPrize, 0);
        }
        else if (GiftFromVIP)
        {
            //tempCasePrize = casePrize;
            casePrize = casePrizeGenerator.GetCasePrize(3, CaseType.VIP, EconomySource.GiftBoxPrize, 0);
        }
        else if (GiftFromEpidemy)
        {
            casePrize = casePrizeGenerator.GetCasePrize(4, CaseType.EPIDEMY, EconomySource.GiftBoxPrize, 0);
        }
        else if (GiftFromTreasure)
        {
            casePrize = casePrizeGenerator.GetCasePrize(5, CaseType.TREASURE, EconomySource.GiftBoxPrize, 0);
        }
        else if (GiftFromDailyQuest)
        {
            //daily quest reward are given instantly in DailyQuestController.cs:ClaimRewardForQuest
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CourierPackage));
            return;
        }
        else if (DailyRewardBundledGift)
        {
            return;
        }
        else if (BundledGiftFromGlobalEvent)
        {
            if (bundledGift == null)
            {
                return;
            }
            CreateBundledGlobalEventCasePrize();
            //ReferenceHolder.GetHospital().globalEventController.ClaimGlobalEventRewardForReloadSpawn();
        }
        else if (SingleGiftFromGlobalEvent)
        {
            if (singleGift == null)
            {
                return;
            }
            CreateSingleGlobalEventCasePrize();

            //ReferenceHolder.GetHospital().globalEventController.ClaimGlobalEventRewardForReloadSpawn();
        }
        else if (GiftFromGlobalEvent)
        {
            int globalEventCaseTier = globalEventStack[0];

            if (lootBoxRewards != null && lootBoxRewards.Count > 0)
                casePrize = casePrizeGenerator.GetPrizeFromGifts(lootBoxRewards, EconomySource.GlobalEventReward);
            else
                casePrize = casePrizeGenerator.GetGiftFromGlobalEvent(globalEventCaseTier);
        }
        else if (GiftFromIAP)
        {
            casePrize = casePrizeGenerator.GetPrizeFromGifts(lootBoxRewards, EconomySource.LootBox);
        }
        else
        {
            BaseGiftableResource baseGiftable = BaseResourceParser.CreateGiftableFromString(CasePrizeDeltaConfig.GetCaseTierConfigForGivenTier(casesStack[lastCaseStackID]), EconomySource.GiftBoxPrize);
            if (baseGiftable is BundleGiftableResource)
            {
                BundleGiftableResource bundleGiftable = baseGiftable as BundleGiftableResource;
                casePrize = casePrizeGenerator.GetCasePrize(casesStack[lastCaseStackID], CaseType.ordinary, EconomySource.GiftBoxPrize, 0, null, bundleGiftable);
                casesStack.RemoveAt(lastCaseStackID);
            }
        }

        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CourierPackage));
        GivePrizeInstantly(casePrize);
    }

    public override void GivePrizeInstantly(CasePrize caseprize)
    {
        EconomySource ecoSource = casePrize.economySource;

        int coinReward = casePrize.coinsAmount;
        Game.Instance.gameState().AddResource(ResourceType.Coin, coinReward, ecoSource, false);

        int diamondReward = casePrize.diamondsAmount;
        Game.Instance.gameState().AddResource(ResourceType.Diamonds, diamondReward, ecoSource, false);

        int positiveEnergyReward = casePrize.positiveEnergyAmount;
        Game.Instance.gameState().AddResource(ResourceType.PositiveEnergy, positiveEnergyReward, ecoSource, false);

        UIController.get.storageCounter.SetCounterStartAmount();

        if (casePrize.items != null && casePrize.items.Count > 0)
        {
            for (int i = 0; i < casePrize.items.Count; ++i)
            {
                MedicineRef tempSpecial = casePrize.items[i].item;
                int tempAmount = casePrize.items[i].amount;
                Game.Instance.gameState().AddResource(tempSpecial, tempAmount, true, ecoSource);
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(tempSpecial, GameState.Get().GetCureCount(tempSpecial)));
            }
        }

        if (casePrize.decorations != null && casePrize.decorations.Count > 0)
        {
            for (int i = 0; i < casePrize.decorations.Count; ++i)
            {
                ShopRoomInfo tempInfo = casePrize.decorations[i].decoration;
                int tempAmount = casePrize.decorations[i].amount;
                Game.Instance.gameState().AddToObjectStored(tempInfo, tempAmount);
            }
        }

        if (casePrize.boosters != null && casePrize.boosters.Count > 0)
        {
            for (int i = 0; i < casePrize.boosters.Count; ++i)
            {
                int tempBoosterID = casePrize.boosters[i].boosterID;
                int tempAmount = casePrize.boosters[i].amount;
                for (int j = 0; j < tempAmount; j++)
                {
                    AreaMapController.Map.boosterManager.AddBooster(tempBoosterID, ecoSource);
                }
            }
        }
    }

    public void DeployCase(int caseID, bool isNew = true)
    {
        casesStack.Add(caseID);
        if (casesStack.Count <= casesDepot.childCount)
        {
            casesDepot.GetChild(casesStack.Count - 1).gameObject.SetActive(true);
            casesDepot.GetChild(casesStack.Count - 1).GetComponent<SpriteRenderer>().sprite = caseSprites[caseID];
        }
        SoundsController.Instance.PlayHit();
    }

    public void RefreshDepot()
    {
        for (int i = 0; i < casesDepot.childCount; ++i)
        {
            casesDepot.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < casesDepot.childCount && i < casesStack.Count; ++i)
        {

            casesDepot.GetChild(i).gameObject.SetActive(true);
            casesDepot.GetChild(i).GetComponent<SpriteRenderer>().sprite = caseSprites[casesStack[i]];
        }
    }
    
    public override void OpenUnboxingPopUp()
    {
        lastCaseStackID = casesStack.Count - 1;
        UIController.getHospital.unboxingPopUp.OpenCasesPopUp();
        SoundsController.Instance.PlayButtonClick2();
    }

    public void CollectButton(int caseID)
    {
        ButtonDelegate buttonDelegate = collectDelegates[caseID];
        if (buttonDelegate != null)
        {
            buttonDelegate.Invoke(caseID);
            RefreshCases(false);
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CollectGifts));
            SaveSynchronizer.Instance.InstantSave();
        }
    }

    public void GetNowButton(int caseID, IDiamondTransactionMaker diamondTransactionMaker)
    {
        int diaCost = getNowPrices[caseID];
        if (Game.Instance.gameState().GetDiamondAmount() >= diaCost)
        {
            DiamondTransactionController.Instance.AddDiamondTransaction(diaCost, delegate
            {
                GameState.Get().RemoveDiamonds(diaCost, EconomySource.GetGiftBox);

                if (caseID == 0)
                    countingStartTime = Convert.ToInt32((long)ServerTime.getTime()) - deliveryIntervalSeconds;

                if (caseID > 0)
                    casesStorage[caseID - 1] = 4;

                RefreshCases();

                NotificationCenter.Instance.GiftReady.Invoke(new BaseNotificationEventArgs());
            }, diamondTransactionMaker);
        }
        else
        {
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
            UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
        }
    }

    private void ActualizePrices()
    {
        getNowPrices[0] = Mathf.Clamp(DiamondCostCalculator.GetCostForCase(GetCurrentIntervalSeconds() - (Convert.ToInt32((long)ServerTime.getTime()) - countingStartTime), GetCurrentIntervalSeconds()), 1, int.MaxValue);
        getNowPrices[1] = Mathf.Clamp(DiamondCostCalculator.GetCostForCase(GetMaxIntervalSeconds(), GetMaxIntervalSeconds()) * (4 - casesStorage[0]), 1, int.MaxValue);
        getNowPrices[2] = Mathf.Clamp(4 * DiamondCostCalculator.GetCostForCase(GetMaxIntervalSeconds(), GetMaxIntervalSeconds()) * (4 - casesStorage[1]), 1, int.MaxValue);
        UIController.getHospital.casesPopUpController.RefreshPrices();
    }

    private void SetDeliveryIntervalSeconds()
    {
        deliveryIntervalSeconds = GetCurrentIntervalSeconds();
    }

    private int GetCurrentIntervalSeconds()
    {
        if (DefaultConfigurationProvider.GetConfigCData().PackageIntervals == null || DefaultConfigurationProvider.GetConfigCData().PackageIntervals.Count == 0) return 21600;
        int IntervalsCount = DefaultConfigurationProvider.GetConfigCData().PackageIntervals.Count;

        if (deliveredPackages < IntervalsCount - 1)
            return DefaultConfigurationProvider.GetConfigCData().PackageIntervals[deliveredPackages];

        return DefaultConfigurationProvider.GetConfigCData().PackageIntervals[IntervalsCount - 1];
    }

    private int GetMaxIntervalSeconds()
    {
        if (DefaultConfigurationProvider.GetConfigCData().PackageIntervals == null || DefaultConfigurationProvider.GetConfigCData().PackageIntervals.Count == 0) return 21600;

        int IntervalsCount = DefaultConfigurationProvider.GetConfigCData().PackageIntervals.Count;
        return DefaultConfigurationProvider.GetConfigCData().PackageIntervals[IntervalsCount - 1];
    }

    private void StartDeliveryCar()
    {
        //DeliveryCar = Instantiate(DeliveryCarPrefab, new Vector3(0,0,0), DeliveryCarPrefab.transform.rotation) as GameObject;
        DeliveryCar.SetActive(true);
        DeliveryCar.GetComponent<DeliveryCarController>().StartRunAt();
        SoundsController.Instance.PlayDeliveryCarIn();
        //ReferenceHolder.Get ().engine.MainCamera.FollowGameObject (casesDepot);
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(casesDepot.position, 1, true, true);
    }

    private void DeliverCase(int caseID)
    {
        if (canDeliver[caseID])
        {
            if (caseID == 0)
            {
                ++deliveredPackages;
                StartCounting();
            }
            if (caseID > 0)            
                casesStorage[caseID - 1] -= 4;

            UIController.getHospital.casesPopUpController.RefreshPopUP();
            if (casesDelivered)
            {
                Timing.KillCoroutine(BusWaiting().GetType());
                Timing.RunCoroutine(BusWaiting());
            }

            DeliveryCar.GetComponent<DeliveryCarController>().AddCargo(caseID);
            SaveSynchronizer.Instance.InstantSave();
        }
        else
        {
            Debug.Log("You Shall not pass");
        }

    }

    public void DeliverButton(int caseID)
    {
        ButtonDelegate buttonDelegate = deliverDelegates[caseID];
        if (buttonDelegate != null)
        {
            buttonDelegate.Invoke(caseID);
            RefreshCases();
            UIController.getHospital.casesPopUpController.Exit();
            // SaveDynamoConnector.Instance.InstantSave();
        }
    }

    public void SetGiftsToShow(List<Giver> giversList, GiftReward wiseGift)
    {
        if (wiseGift != null)
            wiseGiftToShow = wiseGift;
        else
            wiseGiftToShow = null;

        if (giftsToShow == null)
            giftsToShow = new List<Giver>();

        int giftsAmount = (int)Mathf.Clamp(giversList.Count, 0, GiftsAPI.Instance.MaxGifts - (wiseGift != null ? 1 : 0));

        giftsToShow.Clear();

        for (int i = 0; i < giftsAmount; ++i)
        {
            giftsToShow.Add(giversList[i]);
        }
    }

    private void CollectCase(int caseID)
    {
        if (canCollect[caseID])
        {
            canCollect[caseID] = false;

            Canvas canvas = UIController.get.canvas;

            Vector2 startPoint = new Vector2((UIController.getHospital.casesPopUpController.CaseImages[caseID].transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (UIController.getHospital.casesPopUpController.CaseImages[caseID].transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);
            Vector2 finishPoint = new Vector2((UIController.getHospital.casesPopUpController.Storages[caseID].transform.GetChild(casesStorage[caseID]).position.x - Screen.width / 2) / canvas.transform.localScale.x, (UIController.getHospital.casesPopUpController.Storages[caseID].transform.GetChild(casesStorage[caseID]).position.y - Screen.height / 2) / canvas.transform.localScale.y);
            if (caseID == 0)
            {
                ++deliveredPackages;
                StartCounting();
            }
            if (caseID > 0)
            {
                casesStorage[caseID - 1] -= 4;
                UIController.getHospital.casesPopUpController.RefreshStorage(caseID - 1);
            }

            int expRecieved = collectXP[caseID];
            Vector2 tapPos = new Vector2((UIController.getHospital.casesPopUpController.CollectCaseButtons[caseID].transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (UIController.getHospital.casesPopUpController.CollectCaseButtons[caseID].transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);

            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expRecieved, EconomySource.CaseCollected, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, tapPos, expRecieved, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, expRecieved, currentExpAmount);
            });

            casesStorage[caseID]++;

            ReferenceHolder.Get().giftSystem.CreateItemMoveParticle(GiftType.Case, startPoint, finishPoint, 0, 0f, new Vector3(2.8f, 2.8f, 1), new Vector3(1, 1, 1), UIController.getHospital.casesPopUpController.CaseImages[caseID].GetComponent<Image>().sprite, () =>
            {
                UIController.getHospital.casesPopUpController.CaseImages[caseID].GetComponent<Animator>().SetTrigger("Pop");

                //tutaj button wyłączę od collectowania
                //	UIController.get.casesPopUpController.RefreshPopUP ();

                UIController.getHospital.casesPopUpController.CollectCaseButtons[caseID].transform.GetChild(0).GetComponent<Button>().interactable = false;
                UIController.getHospital.casesPopUpController.GetNowButtons[caseID + 1].transform.GetChild(0).GetComponent<Button>().interactable = false;

            }, () =>
            {
                RefreshCases();

                //UIController.get.casesPopUpController.RefreshPopUP ();
                UIController.getHospital.casesPopUpController.CollectCaseButtons[caseID].transform.GetChild(0).GetComponent<Button>().interactable = true;
                UIController.getHospital.casesPopUpController.GetNowButtons[caseID + 1].transform.GetChild(0).GetComponent<Button>().interactable = true;
            });
        }
        else
            Debug.Log("You Shall not pass");
    }

    public void CheckAlert()
    {
        bool showAlert = false;
        for (int i = 0; i < canDeliver.Length; ++i)
        {
            if (canDeliver[i] == true)
            {
                showAlert = true;
                break;
            }
        }
        if (showAlert)
            UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOnAlert();
        else
            UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOffAlert();
    }

    IEnumerator<float> DeliveryWaiting()
    {
        UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOffCounting();
        UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOnCounting();

        while (Convert.ToInt32((long)ServerTime.getTime()) - countingStartTime < deliveryIntervalSeconds)
        {
            yield return Timing.WaitForSeconds(1);
            ActualizePrices();
        }
        UIController.get.BoxButton.GetComponent<BoxButtonController>().turnOffCounting();
        RefreshCases();
    }

    IEnumerator<float> BusWaiting()
    {
        casesDelivered = false;
        while (deliveryCarBusy)
        {
            yield return Timing.WaitForSeconds(1f);
        }
        /*if (HospitalAreasMapController.Map.carsManager.carBusy) {
            yield return Timing.WaitForSeconds (5f);
        }*/
        deliveryCarBusy = true;
        StartDeliveryCar();
        yield return 0f;
    }

    public override void ShowGiftRewardAnimation()
    {
        GiftReward reward = null;
        if (wiseGiftToShow != null)
            reward = wiseGiftToShow;
        else
            reward = giftsToShow[0].reward;

        SoundsController.Instance.PlayReward();
        Canvas canvas = UIController.get.canvas;
        RectTransform targetTransform = UIController.getHospital.unboxingPopUp.prizeImage.GetComponent<RectTransform>();

        float centreX = targetTransform.anchoredPosition.x * canvas.scaleFactor + (Screen.width * 0.5f);
        float centreY = targetTransform.anchoredPosition.y * canvas.scaleFactor + (Screen.height * 0.5f);

        Vector3 startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(centreX, centreY, ReferenceHolder.Get().engine.MainCamera.GetCamera().nearClipPlane));

        int amount = reward.amount;

        switch (reward.rewardType)
        {
            case GiftRewardType.Coin:
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, startPoint, amount, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), goldStack, null, null);
                break;
            case GiftRewardType.Diamond:
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, startPoint, amount, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), diamondsChest, null, null);
                break;
            case GiftRewardType.PositiveEnergy:
                MedicineRef positiveRef = MedicineRef.Parse("16(00)");
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.PositiveEnergy, startPoint, amount, 0f, prizeMoveDuration, new Vector3(3.2f, 3.2f, 1), new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(positiveRef), null, null);
                break;
            case GiftRewardType.Mixture:
                UIController.get.storageCounter.Add(false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, startPoint, amount, 0, prizeMoveDuration, new Vector3(3f, 3f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(((GiftRewardMixture)reward).GetRewardMedicineRef()), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(amount, false);
                });
                break;
            case GiftRewardType.StorageUpgrader:
                UIController.get.storageCounter.Add(false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, amount, 0, prizeMoveDuration, new Vector3(3f, 3f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(((GiftRewardStorageUpgrader)reward).GetRewardMedicineRef()), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(amount, false);
                });
                break;
            case GiftRewardType.Shovel:
                UIController.get.storageCounter.Add(false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, amount, 0, prizeMoveDuration, new Vector3(3f, 3f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(((GiftRewardShovel)reward).GetRewardMedicineRef()), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(amount, false);
                });
                break;
            default:
                break;
        }

        if (wiseGiftToShow != null)
            wiseGiftToShow = null;
        else
            giftsToShow.RemoveAt(0);

        if (giftsToShow.Count > 0)
            UIController.getHospital.unboxingPopUp.ShowFriendGift();
        else
        {
            UIController.getHospital.unboxingPopUp.ExitAfterGiftFromFriend();
            SaveSynchronizer.Instance.InstantSave();
            HospitalUIPrefabController.Instance.ShowMainUI();
        }
    }

    public void TestCollectCaseI()
    {
        canCollect[0] = false;
        ++deliveredPackages;
        countingStartTime = Convert.ToInt32((long)ServerTime.getTime());

        if (deliveryWaitingCorountine != null)
        {
            Timing.KillCoroutine(deliveryWaitingCorountine);
            deliveryWaitingCorountine = null;
        }

        StartCounting();

        ++casesStorage[0];
        RefreshCases();

        UIController.getHospital.casesPopUpController.RefreshPopUP();
    }
}
