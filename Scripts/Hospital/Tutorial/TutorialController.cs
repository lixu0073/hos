using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleUI;
using Hospital;
using System.Text;
using System;

public class TutorialController : MonoBehaviour
{
    public delegate void TutorialStepStartedCallback(HospitalTutorialStep stepInfo);
    public event TutorialStepStartedCallback TutorialStepStarted;

    private static TutorialController instance = null;
    public static TutorialController Instance { get { return instance; } }

    public bool tutorialEnabled = true;
    //ASK: why not reference the currentHostpialTutorialstep here?(Translated from Polish)
    public int CurrentTutorialStepIndex;
    public StepTag CurrentTutorialStepTag;
    public bool ConditionFulified;
    public List<HospitalTutorialStep> TutorialStepsList = new List<HospitalTutorialStep>();
    public List<HospitalTutorialStep> NonLinearStepsList = new List<HospitalTutorialStep>();
    public Dictionary<StepTag, bool> NonLinearCompletion = new Dictionary<StepTag, bool>();

    private Dictionary<StepTag, int> stepTagIndexDict;
    [SerializeField]
    protected NotificationListener instanceNL = null;
    protected HospitalTutorialStep currentTutorialStep;
    private IGameState gameState;
    //private bool isStepsVisibleInEdit = false;
    [SerializeField]
    protected HospitalTutorialStep defaultStep = null;

    public bool ram = false;
    public void ToggleRAM()
    {
        ram = !ram;
    }

    [HideInInspector]
    public bool WiseVisitedThisSession = false;
    public bool IsArrowAnimationNeededForWhiteElixir = false;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //Debug.LogError("SERVER TIME: " + ServerTime.GetServerTimeDirectlyFromServer().ToString() + " , TIMESTAMP: " + ServerTime.DateTimeToUnixTimestamp(ServerTime.serverTime));

        instance = this;
    }

    protected virtual void Start()
    {
        try
        {
            gameState = Game.Instance.gameState();
        }
        catch (Exception e)
        {
            Debug.LogError("Error with Game.Instance.gameState(): " + e.Message);
        }

        defaultStep.NotificationType = NotificationType.Error;

        if (tutorialEnabled)
        {
            CreateStepsDict();
        }
        else
        { }//NotificationCenter.UnsuscribeAllNotification(instanceNL);
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    /// <summary>
    /// Called only at Start of program.
    /// </summary>

    public HospitalTutorialStep GetCurrentTutorialStep()
    {
        return currentTutorialStep;
    }

    protected void InvokeNewStepEvent(HospitalTutorialStep stepInfo)
    {
        if (TutorialStepStarted != null)
            TutorialStepStarted.Invoke(stepInfo);
    }

    void CreateStepsDict()
    {
        /*var list = SearchSteps();
        TutorialStepsList.Clear();
        TutorialStepsList.AddRange(list);
        stepTagIndexDict = new Dictionary<StepTag, int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                Debug.LogError("null step");
                continue;
            }
            try
            {
                stepTagIndexDict.Add(TutorialStepsList[i].StepTag, TutorialStepsList[i].StepID);
            }
            catch
            {
                Debug.LogError("Coudn't add tutorial step to dict.");
            }
        }*/
        try
        {
            stepTagIndexDict = new Dictionary<StepTag, int>();
            for (int i = 0; i < TutorialStepsList.Count; i++)
            {
                if (TutorialStepsList[i] == null)
                {
                    Debug.LogError("null step");
                    continue;
                }
                //Debug.LogWarning("Adding step number: " + i + " tag: " + TutorialStepsList[i].StepTag);
                //if (TutorialStepsList[i].StepTag == StepTag.level_goals_ended)
                //    Debug.LogError("Step no. " + TutorialStepsList[i].StepID);
                //if (stepTagIndexDict.ContainsKey(TutorialStepsList[i].StepTag))
                //    Debug.LogError("This key is already in the dictionary: " + TutorialStepsList[i].StepTag + " " + TutorialStepsList[i].StepID);
                try
                {
                    stepTagIndexDict.Add(TutorialStepsList[i].StepTag, TutorialStepsList[i].StepID);
                }
                catch
                {
                    Debug.LogError("Error while creating stepTagIndexDict, while trying to add " + TutorialStepsList[i].StepTag + ", " + TutorialStepsList[i].StepID);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Source);
            Debug.LogError("Error in the CreateStepsDict(). " + e.ToString());
        }
        Debug.LogError("Total steps: " + stepTagIndexDict.Count);
    }

    /// <summary>
    /// Sets CurrentStep. 
    /// Stops Coroutines in TutorialUI controller.
    /// Stops camera.
    /// Activates DisableAllStepButtonColor () method which probably does not work. This metods uses gameObject.Find("TestObjects") in hierarchy but this object is diactivated 
    /// through entire game.
    /// 
    /// </summary>
    public void SetCurrentStep()
    {
        ResetTutorialSpecialAnimsPos();
        TutorialUIController.Instance.StopShowCoroutines();
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();

        SetTutorialStep();

        InvokeOnSetStep();
    }

    public virtual void ResetTutorialSpecialAnimsPos()
    {

    }

    /// <summary>
    /// This Method returns current HospitalTutorialStep. Method checks what is current index of tutorialstep. 
    /// If it is greater than total number of tutorial step list, it returns an error TutorialStep. 
    /// </summary>
    /// <returns> HospitalTutorialStep </returns>
    public HospitalTutorialStep GetCurrentStepData()
    {
        if (CurrentTutorialStepIndex >= TutorialStepsList.Count)
            return defaultStep;
        return TutorialStepsList[CurrentTutorialStepIndex];
    }

    /// <summary>
    /// Searching for some objects by tag.
    /// </summary>
    /// <returns></returns>
    public RotatableObject FindObjectForStep()
    {
        //Debug.Log("FindObjectForStep(): object tag = " + GetCurrentStepData().TargetMachineTag);
        return AreaMapController.Map.FindRotatableObject(GetCurrentStepData().TargetMachineTag);
    }

    public void ShowCurrentStepPopupInformation()
    {
        if (VisitingController.Instance.IsVisiting && !(StepIsInWiseHospital(CurrentTutorialStepTag)))
            return;
        if (CurrentTutorialStepTag == StepTag.level_19)
            return;
        if (CurrentTutorialStepTag == StepTag.bacteria_emma_micro_1 && !ConditionFulified)
            return;
        if (CurrentTutorialStepTag == StepTag.vip_flyby_emma_1 && !ConditionFulified)
            return;
        if (CurrentTutorialStepTag == StepTag.bubble_boy_arrow && !ConditionFulified)
            return;

        if ((CurrentTutorialStepTag == StepTag.first_yellow_patient_popup_open || CurrentTutorialStepTag == StepTag.first_green_patient_popup_open) && !ConditionFulified)
        {
            return;
        }

        // if (currentTutorialStep.ForceClosePopups)
        //    UIController.get.ExitAllPopUps(true);

        TutorialUIController.Instance.ShowGuidesInformation(currentTutorialStep);
    }

    /// <summary>
    /// A method that checks the current HostpialTutorial step and checks its completion conditions. It then sets up a listener for that event that will trigger the fulfillment
    /// tutorial completion conditions. (Translated from polish)
    /// </summary>
    protected virtual void SetTutorialStep()
    {
    }

    public virtual void CheckSubscriptionMoment()
    {
    }

    public virtual void SubscribeToStepCondition(Condition condition)
    {
        switch (condition)
        {
            case Condition.NoCondition:
                break;
            case Condition.HospitalNamed:
                instanceNL.SubscribeHospitalNamedNotification();
                break;
            case Condition.SetCurrentlyPointedMachine:
                instanceNL.SubscribeSetCurrentlyPointedMachineNotification();
                break;
            case Condition.SetTutorialArrow:
                UIController.get.reportPopup.canBeOpen = false;
                instanceNL.SubscribeTutorialArrowSetNotification();
                break;
            case Condition.DrawerOpened:
                instanceNL.SubscribeDrawerOpenedNotification(true);
                break;
            case Condition.FinishedBuilding:
                instanceNL.SubscribeFinishedBuildingNotification();
                break;
            case Condition.Level1ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(1);
                break;
            case Condition.Level2ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(2);
                break;
            case Condition.Level3ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(3);
                break;
            case Condition.Level4ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(4);
                break;
            case Condition.Level5ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(5);
                break;
            case Condition.Level6ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(6);
                break;
            case Condition.Level7ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(7);
                break;
            case Condition.Level8ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(8);
                break;
            case Condition.Level9ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(9);
                break;
            case Condition.Level10ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(10);
                break;
            case Condition.Level12ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(12);
                break;
            case Condition.Level14ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(14);
                break;
            case Condition.Level15ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(15);
                break;
            case Condition.Level16ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(16);
                break;
            case Condition.Level17ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(17);
                break;
            case Condition.Level18ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(18);
                break;
            case Condition.Level19ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(19);
                break;
            case Condition.Level20ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(20);
                break;
            case Condition.Level23ReachedAndClosed:
                instanceNL.SubscribeLevelReachedAndClosedNotification(23);
                break;
            case Condition.FirstPatientArriving:
                instanceNL.SubscribeFirstPatientArrivingNotification();
                break;
            case Condition.PatientCardIsOpen:
                instanceNL.SubscribePatientCardIsOpenNotification();
                break;
            case Condition.PatientCardIsClosed:
                instanceNL.SubscribePatientCardIsClosedNotification();
                break;
            case Condition.RuntimeStepInfoChanged:
                instanceNL.SubscribeRunTimeStepInfoChangedNotification();
                break;
            case Condition.MoveRotateRoomEnd:
                instanceNL.SubscribeMoveRotateRoomEndNotification();
                break;
            case Condition.ObjectDoesNotExistOnLevel:
                instanceNL.SubscribeObjectExistOnLeveldNotification();
                break;
            case Condition.KidsRoomUnlocked:
                instanceNL.SubscribeKidsRoomUnlockedNotification();
                break;
            case Condition.KidPatientSpawned:
                instanceNL.SubscribeKidPatientSpawnedNotification();
                break;
            case Condition.FirstEmergencyPatientSpawned:
                instanceNL.SubscribeFirstEmergencyPatientSpawnedNotification();
                break;
            case Condition.SpawnFirstPatient:
                instanceNL.SubscribeSpawnFirstPatientNotification();
                break;
            case Condition.FirstPatientNearSignOfHospital:
                instanceNL.SubscribeFirstPatientNearSignOfHospitalNotification();
                break;
            case Condition.ExpandConditions:
                instanceNL.SubscribeExpandConditionsNotification();
                break;
            case Condition.ExpAmountChanged:
                instanceNL.SubscribeExpAmountChangedNotification();
                break;
            case Condition.NotEnoughPanacea:
                instanceNL.SubscribeNotEnoughPanaceaNotification();
                break;
            case Condition.FollowBob:
                instanceNL.SubscribeFollowBobNotification();
                break;
            case Condition.MedicopterTookOff:
                instanceNL.SubscribeMedicopterTookOffNotification();
                break;
            case Condition.VIPLeoReachedPosition:
                instanceNL.SubscribeCharacterReachedDestinationNotification();
                break;
            case Condition.TreasurePopUpOpened:
                instanceNL.SubscribeTreasurePopUpOpened();
                break;
            case Condition.VIPMedicopterStarted:
                if (currentTutorialStep.ForceClosePopups)
                    UIController.get.ExitAllPopUps(true);
                instanceNL.SubscribeVIPMedicopterStartedNotification();
                break;
            case Condition.VIPReachedBed:
                instanceNL.SubscribeVIPReachedBedNotification();
                break;
            case Condition.VIPNotCured:
                instanceNL.SubscribeVIPNotCuredNotification();
                break;
            case Condition.HomeHospitalLoaded:
                instanceNL.SubscribeHomeHospitalLoaded();
                break;
            case Condition.PackageCollected:
                instanceNL.SubscribePackageCollected();
                break;
            case Condition.WisePharmacyVisited:
                instanceNL.SubscribeWisePharmacyVisited();
                break;
            case Condition.BoostersPopUpOpen:
                instanceNL.SubscribeBoostersPopUpOpen();
                break;
            case Condition.GiftReady:
                instanceNL.SubscribeGiftReady();
                break;
            case Condition.OpenWelcomePopUp:
                instanceNL.SubscribeOpenWelcomePopUp();
                break;
            case Condition.EpidemyStarting:
                instanceNL.SubscribeEpidemyStarting();
                break;
            case Condition.EpidemyCompleted:
                instanceNL.SubscribeEpidemyCompleted();
                break;
            case Condition.TwentyProbeTables:
                instanceNL.SubscribeTwentyProbeTables();
                break;
            case Condition.SecondDiagnosticMachineOpen:
                instanceNL.SubscribeSecondDiagnosticMachineOpen();
                break;
            case Condition.TenPatioDecorations:
                instanceNL.SubscribeTenPatioDecorations();
                break;
            case Condition.ExpAmountChangedNonLinear:
                instanceNL.SubscribeExpAmountChangedNonLinear();
                break;
            case Condition.Level11ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(11);
                break;
            case Condition.Level13ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(13);
                break;
            case Condition.Level16ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(16);
                break;
            case Condition.Level22ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(22);
                break;
            case Condition.Level20ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(20);
                break;
            case Condition.Level30ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(30);
                break;
            case Condition.Level42ReachedAndClosedNonLinear:
                instanceNL.SubscribeLevelReachedAndClosedNonLinear(42);
                break;
            case Condition.Level10NewspaperClosed:
                instanceNL.SubscribeLevel10NewspaperClosedCond();
                break;
            case Condition.Level10Wise1Closed:
                instanceNL.SubscribeLevel10WiseClosedCond();
                break;
            case Condition.VitaminesMakerEmma1Closed:
                instanceNL.SubscribeVitaminesMakerEmma1ClosedCond();
                break;
            case Condition.TreasureCollected:
                instanceNL.SubscribeTreasureCollected();
                break;
            case Condition.BubbleBoyAvailable:
                instanceNL.SubscribeBubbleBoyAvailable();
                break;
            case Condition.VipFlyByStart:
                instanceNL.SubscribeVipFlyByStart();
                break;
            case Condition.PushNotificationsDisabled:
                instanceNL.SubscribePushNotificationsDisabled();
                break;
            case Condition.ShowDailyQuestAnimation:
                instanceNL.SubscribeShowDailyQuestAnimation();
                break;
            case Condition.MicroscopeShow:
                instanceNL.SubscribeMicroscopeShow();
                break;
            case Condition.LevelGoalsActive:
                instanceNL.SubscribeLevelGoalsActive();
                break;
            case Condition.OnFirstVitaminesMakerPopupOpened:
                instanceNL.SubscribeFirstVitaminesMakerPopupOpened();
                break;
            case Condition.CuredVipCountIsEnough:
                instanceNL.SubscribeCuredVipCountIsEnough();
                break;
            default:
                break;
        }
    }

    public void ConfirmConditionExecution()
    {
        //Debug.Log("Condition validation");
        ConditionFulified = true;
    }

    HospitalTutorialStep GetNonLinearStepData(StepTag tag)
    {
        return NonLinearStepsList.Where((x) => x.StepTag == tag).SingleOrDefault();
    }

    public void SubscribeNotificationNonLinear(StepTag stepTag)
    {
        NotificationType notif = NotificationType.None;
        HospitalTutorialStep step = GetNonLinearStepData(stepTag);
        if (step != null)
            notif = step.NotificationType;

        /*switch (notif)
        {
            //case NotificationType.NewspaperRewardExp:
            //    NotificationListener.Instance.SubscribeNewspaperRewardExp();
            //    break;
            //case NotificationType.NewspaperRewardDiamond:
            //    NotificationListener.Instance.SubscribeNewspaperRewardDiamond();
            //    break;
            //case NotificationType.Level10Wise1Closed:
            //    instanceNL.SubscribeLevel10WiseClosedNotif();
            //    break;
            //case NotificationType.Level10NewspaperClosed:
            //    instanceNL.SubscribeLevel10NewspaperClosedNotif();
            //    break;
            //case NotificationType.Level10WiseGiveBooster:
            //    instanceNL.SubscribeLevel10WiseGiveBooster();
            //    break;
            //case NotificationType.VitaminesMakerEmma1Closed:
            //    instanceNL.SubscribeVitaminesMakerEmma1ClosedNotif();
            //    break;
            //case NotificationType.BoosterPopupClosed:
            //    instanceNL.SubscribeBoosterPopupClosed();
            //    break;
            //case NotificationType.VipUpgradeTutorial1Closed:
            //    instanceNL.SubscribeVipUpgradeTutorial1Closed();
            //    break;
            //case NotificationType.VipUpgradeTutorial2Closed:
            //    instanceNL.SubscribeVipUpgradeTutorial2Closed();
            //    break;
            //case NotificationType.VipUpgradeTutorial3Closed:
            //    instanceNL.SubscribeVipUpgradeTutorial3Closed();
            //    break;
            //case NotificationType.VipUpgradeTutorial4Closed:
            //    instanceNL.SubscribeVipUpgradeTutorial4Closed();
            //    break;
            default:
                break;
        }*/
    }

    void GiveCashAndExp(int coinReward, int expReward)
    {
        if (coinReward > 0)
        {
            int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
            gameState.AddResource(ResourceType.Coin, coinReward, EconomySource.Tutorial, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, Vector3.zero, coinReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Coin, coinReward, currentCoinAmount);
            });
        }
        if (expReward > 0)
        {
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            gameState.AddResource(ResourceType.Exp, expReward, EconomySource.Tutorial, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, Vector3.zero, expReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });
        }
    }

    public virtual void StopTutorialCameraFollowingForSpecialObjects()
    {
    }

    public void IncrementCurrentStep()
    {
        //Debug.LogError("Incrementing tutorial step");        
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.TutorialStepCompleted);
        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Tutorial.ToString(), CurrentTutorialStepIndex, CurrentTutorialStepTag.ToString());
        TutorialUIController.Instance.HideTapToContinue();

        if (currentTutorialStep != null && currentTutorialStep.UseCameraTargeting && currentTutorialStep.CameraLocked)
        {
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = true;
            //Debug.Log("Blocking user input");
        }
        else
        {
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
            //Debug.Log("NOT Blocking user input");
        }

        if (currentTutorialStep.NextStep != null)
        {
            int coinReward = currentTutorialStep.CoinsReward;
            int expReward = currentTutorialStep.ExperienceReward;
            ConditionFulified = false;
            TutorialUIController.Instance.HideIndicator();
            //Debug.Log("Inkrementacja stepu " + currentTutorialStep);
            CurrentTutorialStepIndex++;
            /*
            if (!currentTutorialStep.NextStep.isFreePlayStep)
            {
                HintsController.Get().TryToHideIndicator();
                HintsController.Get().HideButton();
            }
            */

            currentTutorialStep = currentTutorialStep.NextStep;
            CurrentTutorialStepTag = currentTutorialStep.StepTag;
            TutorialUIController.Instance.currentStep = currentTutorialStep;

            //Tutorial step before this step is shown only when level goals are enabled. 
            //This is a fix for a correct cloud open animation
            if (currentTutorialStep.StepTag == StepTag.daily_quests_unlocked && CampaignConfig.hintSystemEnabled)
                currentTutorialStep.NeedsOpenAnimation = true;

            SetCurrentStep();
            GiveCashAndExp(coinReward, expReward);

        }
        else
        {
            Debug.Log("Next Step null");
        }
    }

    #region Non-Linear Steps
    public void SubscribeNonLinearSteps()
    {
        //Debug.LogError("SubscribeNonLinearSteps");
        for (int i = 0; i < NonLinearStepsList.Count; i++)
        {
            if (NonLinearCompletion.ContainsKey(NonLinearStepsList[i].StepTag))
            {
                if (NonLinearCompletion[NonLinearStepsList[i].StepTag] == false)
                {
                    //Debug.LogError("SubscribeNonLinearSteps");
                    SubscribeToStepCondition(NonLinearStepsList[i].NecessaryCondition);
                }
                else
                {
                    //Debug.LogError("SubscribeNonLinearSteps");
                }
            }
        }
    }

    public void LoadNonLinearStepsCompletion(string parsedDict)
    {
        //Debug.LogError("LoadNonLinearStepsCompletion from string: " + parsedDict);
        Dictionary<StepTag, bool> savedDict = ParseNonLinearCompletionDictionary(parsedDict);
        NonLinearCompletion.Clear();
        //Debug.LogError("NonLinearStepsList Count: " + NonLinearStepsList.Count);

        //rebuild the dictionary in case there are some new steps which were not in the previous save
        for (int i = 0; i < NonLinearStepsList.Count; i++)
        {
            if (savedDict != null && savedDict.ContainsKey(NonLinearStepsList[i].StepTag))
                NonLinearCompletion.Add(NonLinearStepsList[i].StepTag, savedDict[NonLinearStepsList[i].StepTag]);
            else
            {
                //Debug.LogError("savedDict is null or does not containt key: " + NonLinearStepsList[i].StepTag);
                NonLinearCompletion.Add(NonLinearStepsList[i].StepTag, false);
            }
        }
    }

    public string GetParsedNonLinearCompletion()
    {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<StepTag, bool> pair in NonLinearCompletion)
        {
            sb.Append(pair.Key);
            sb.Append("*");
            sb.Append(pair.Value);
            sb.Append("*");
        }
        //Debug.LogError("Parsed Dictionary = " + sb.ToString());
        return sb.ToString();
    }

    Dictionary<StepTag, bool> ParseNonLinearCompletionDictionary(string parsedDict)
    {
        //Debug.LogError("Parsing dictionary from string: " + parsedDict);
        if (parsedDict == null)
            return null;

        Dictionary<StepTag, bool> tempDict = new Dictionary<StepTag, bool>();
        string[] temp = parsedDict.Split('*');
        for (int i = 0; i < temp.Length - 1; i += 2)
        {
            //Debug.LogError("Ading to tempDict key: " + temp[i] + " value: " + temp[i + 1]);
            try
            {
                StepTag stepTagFromString = (StepTag)Enum.Parse(typeof(StepTag), temp[i], false);
                tempDict.Add(stepTagFromString, bool.Parse(temp[i + 1]));
            }
            catch
            {
                Debug.LogError("PARSED STEP TAG DOES NOT EXIST! " + temp[i]);
            }
        }

        //
        //Debug.LogError("Finished parsing dictionary. Count: " + tempDict.Count);
        return tempDict;
    }

    public void MarkNonLinearStepAsCompleted(StepTag stepTag)
    {
        //Debug.Log("MarkNonLinearStepAsCompleted: " + stepTag);
        NonLinearCompletion[stepTag] = true;
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.TutorialStepCompleted);
    }

    public void ShowNonLinearStep(StepTag stepTag)
    {
        TutorialUIController.Instance.StopShowCoroutines();
        HospitalTutorialStep nonLinearStep = NonLinearStepsList[GetStepId(stepTag, true)];
        TutorialUIController.Instance.TargetCamera(nonLinearStep);
        if (nonLinearStep.ForceCloseHovers)
            UIController.get.CloseActiveHover();

        TutorialUIController.Instance.ShowGuidesInformation(nonLinearStep);
        if (nonLinearStep.NotificationType != NotificationType.None)
            SubscribeNotificationNonLinear(stepTag);
    }

    private UnityAction onSetStep = null;

    private void InvokeOnSetStep()
    {
        if (onSetStep != null)
        {
            onSetStep.Invoke();
            onSetStep = null;
        }
    }

    private Coroutine setStepCoroutine = null;

    public void SetNonLinearStep(StepTag stepTag)
    {
        onSetStep = null;
        try
        {
            if (setStepCoroutine != null)
            {
                StopCoroutine(setStepCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        setStepCoroutine = StartCoroutine(TryShowNonLinearStepCoroutine(stepTag));
    }

    private IEnumerator TryShowNonLinearStepCoroutine(StepTag stepTag)
    {
        while (TutorialUIController.Instance.ShouldWait())
        {
            yield return new WaitForSeconds(1f);
        }
        yield return null; // wait one additional frame

        if ((currentTutorialStep.NecessaryCondition != Condition.NoCondition && !ConditionFulified) || (currentTutorialStep.NecessaryCondition == Condition.NoCondition && currentTutorialStep.StepInfoType == StepInfoType.None))
        {
            ShowNonLinearStep(stepTag);
            onSetStep = null;
        }
        else
        {
            onSetStep = () =>
            {
                SetNonLinearStep(stepTag);
            };
        }
    }
    #endregion

    // SAVE TUTORIAL STEP FUNCTIONS
    public void SetStep(StepTag tutorialStepName)
    {
        //Debug.LogError("SetStep " + tutorialStepName);

        TutorialUIController instanceTUI = TutorialUIController.Instance;

        instanceTUI.BlinkDrawerButton(false);
        instanceTUI.HideIndicator();
        instanceTUI.InGameCloud.Hide();
        instanceTUI.HideTapToContinue();
        instanceTUI.StopShowCoroutines();

        ConditionFulified = false;
        if (!TutorialSystem.TutorialController.ShowTutorials)
            ConditionFulified = true;
        ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;

        if (stepTagIndexDict.ContainsKey(tutorialStepName))
        {
            instance.CurrentTutorialStepTag = tutorialStepName;
            instance.CurrentTutorialStepIndex = stepTagIndexDict[instance.CurrentTutorialStepTag];

            /*
            if (!TutorialStepsList[stepTagIndexDict[CurrentTutorialStepTag]].isFreePlayStep)
            {
                HintsController.Get().TryToHideIndicator();
                HintsController.Get().HideButton();
            }
            */

            currentTutorialStep = TutorialStepsList[stepTagIndexDict[CurrentTutorialStepTag]];
            TutorialUIController.Instance.currentStep = currentTutorialStep;
        }
        else
        {
            /*
            if (!defaultStep.isFreePlayStep)
            {
                HintsController.Get().TryToHideIndicator();
                HintsController.Get().HideButton();
            }
            */

            currentTutorialStep = defaultStep;
            TutorialUIController.Instance.currentStep = currentTutorialStep;
        }
        //DontDestroyOnLoad(transform.root.gameObject);
        //NotificationCenter.UnsubscribeAllNotification();
        SubscribeNonLinearSteps();
        SetCurrentStep();
    }

    public void DeleteStepSave(int id)
    {
        SaveLoadController.Get().ClearSaveStepTutorial(id);
    }

    /// <summary>
    /// Returs HospitalTutorialStep field StepID.
    /// </summary>
    /// <param name="stepTag"></param>
    /// <param name="isNonLinear"></param>
    /// <returns></returns>
    public int GetStepId(StepTag stepTag, bool isNonLinear = false)
    {
        if (isNonLinear)
        {
            for (int i = 0; i < NonLinearStepsList.Count; i++)
            {
                if (NonLinearStepsList[i].StepTag == stepTag)
                    return i;
            }
            Debug.LogError("NonLinear Tutorial Step NOT FOUND! Tag: " + stepTag);
            return 0;
        }

        if (stepTagIndexDict != null && stepTagIndexDict.ContainsKey(stepTag))
        {
            return stepTagIndexDict[stepTag];
        }
        return -1;
    }

    public bool IsTutorialStepCompleted(StepTag stepTag)
    {
        if (CurrentTutorialStepIndex > GetStepId(stepTag))
            return true;

        return false;
    }

    public bool IsTutorialStepEqual(StepTag stepTag)
    {
        if (CurrentTutorialStepIndex == GetStepId(stepTag))
            return true;

        return false;
    }

    public List<ProbeTable> GetAllEmptyProbeTables()
    {
        List<ProbeTable> emptyProbeTables = new List<ProbeTable>();
        ProbeTable[] allTables = FindObjectsOfType<ProbeTable>();
        //Debug.Log("Found { " + allTables.Length + " } probe tables");

        for (int i = 0; i < allTables.Length; i++)
        {
            if (allTables[i].producedElixir == null)
            {
                emptyProbeTables.Add(allTables[i]);
            }
        }
        //Debug.Log("Found { " + emptyProbeTables.Count + " } empty probe tables");
        return emptyProbeTables;
    }

    public List<ProbeTable> GetAllFullProbeTables()
    {
        List<ProbeTable> fullProbeTables = new List<ProbeTable>();
        ProbeTable[] allTables = FindObjectsOfType<ProbeTable>();
        //Debug.Log("Found { " + allTables.Length + " } probe tables");

        for (int i = 0; i < allTables.Length; i++)
        {
            if (allTables[i].producedElixir != null)
            {
                fullProbeTables.Add(allTables[i]);
            }
        }
        //Debug.Log("Found { " + fullProbeTables.Count + " } full probe tables");
        return fullProbeTables;
    }

    public int GetAllFullProbeTablesCount()
    {
        ProbeTable[] allTables = FindObjectsOfType<ProbeTable>();

        return allTables.Length;
    }

    public bool IsNonLinearStepCompleted(StepTag stepTag)
    {
        //If we don't want to show tutorials, we can skip all non-linear steps.
        //These shouldn't be saved if player wants to reactivate tutorials.
        if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            return true;
        }
        if (!NonLinearCompletion.ContainsKey(stepTag))
        {
            return true;
        }

        return NonLinearCompletion[stepTag];
    }

    public bool IsPatientCardExitLocked(HospitalCharacterInfo patient)
    {
        if (CurrentTutorialStepIndex < GetStepId(StepTag.patient_card_text_2))
            return true;
        if ((currentTutorialStep.StepTag == StepTag.bacteria_george_3 || currentTutorialStep.StepTag == StepTag.bacteria_george_4) && patient.HasBacteria)
            return true;

        return false;
    }

    readonly HashSet<Condition> stepsThatRequireDrawerScroll = new HashSet<Condition>()
    {
        Condition.DrawerOpened, Condition.ObjectDoesNotExistOnLevel
    };

    public bool CurrentStepTagRequiresDrawerScroll()
    {
        return stepsThatRequireDrawerScroll.Contains(currentTutorialStep.NecessaryCondition);
    }

    readonly HashSet<StepTag> stepsThatBlockDrawerUI = new HashSet<StepTag>()
    {
        StepTag.build_doctor_text, StepTag.yellow_doc_add, StepTag.syrup_lab_add, StepTag.elixir_mixer_add,
        StepTag.green_doc_add_text, StepTag.green_doc_add, StepTag.new_cures_2, StepTag.emma_on_Xray
    };

    public bool CurrentStepBlocksDrawerUI()
    {
        return stepsThatBlockDrawerUI.Contains(CurrentTutorialStepTag);
    }

    public bool IsPatioStep()
    {
        return CurrentTutorialStepTag == StepTag.patio_tidy_5 && ConditionFulified;
    }

    private readonly HashSet<StepTag> wiseVisitStepTags = new HashSet<StepTag>() { StepTag.wise_pharmacy, StepTag.wise_thank_you };

    public bool StepIsInWiseHospital(StepTag stepTag)
    {
        return wiseVisitStepTags.Contains(stepTag);
    }

    public int helperStartingIndex = 999;
    public int decrementOrIncrementAmount = 1;

#if UNITY_EDITOR
    [ContextMenu("Log non linear completion")]    //option for soring tutorial steps by ID
    void LogNonLinearCompletion()
    {
        foreach (KeyValuePair<StepTag, bool> k in NonLinearCompletion)
        {
            Debug.Log(k.Key + ", " + k.Value);
        }
    }

    [ContextMenu("Sort Steps by ID")]    //option for soring tutorial steps by ID
    void SortTutorialSteps()
    {
        TutorialStepsList = TutorialStepsList.OrderBy(step => step.StepID).ToList();
        //Debug.Log("***Tutorial steps have been sorted by ID!***");
    }

    [ContextMenu("Increment stepIDs(change helperStartingIndex in inspector)")]    //option for incrementing stepIDs after creating a step inbetween. Please change startingIndex to your needs.
    void IncrementStepIDs()
    {
        for (int i = helperStartingIndex; i < TutorialStepsList.Count; i++)
        {
            TutorialStepsList[i].StepID++;
        }
        //Debug.Log("***Steps " + helperStartingIndex + "+ incremented!***");
        RenameSteps();
    }

    [ContextMenu("Decrement stepIDs(change helperStartingIndex in inspector)")]    //option for decrementing stepIDs after creating a step inbetween. Please change startingIndex to your needs.
    void DecrementStepIDs()
    {
        for (int i = helperStartingIndex; i < TutorialStepsList.Count; i++)
        {
            TutorialStepsList[i].StepID--;
        }
        //Debug.Log("***Steps " + helperStartingIndex + "+ decremented!***");
        RenameSteps();
    }

    [ContextMenu("Rename step files (to match ID)")]    //option for renaming all steps by ID. WARNING! When there are 2 steps with the same ID the renaming will be wrong!
    void RenameSteps()
    {
        SortTutorialSteps();
        //change all names to placeholders, so when they are set to correct ones there's no conflicts
        for (int i = helperStartingIndex; i < TutorialStepsList.Count; i++)
        {
            //Debug.Log("Renaming file: " + TutorialStepsList[i].name + " to: " + TutorialStepsList[i].StepID.ToString());
            AssetDatabase.RenameAsset("Assets/_MyHospital/Tutorial/Steps/Linear/" + TutorialStepsList[i].name + ".asset", TutorialStepsList[i].StepID.ToString());
        }

        //now set the correct names
        for (int i = helperStartingIndex; i < TutorialStepsList.Count; i++)
        {
            AssetDatabase.RenameAsset("Assets/_MyHospital/Tutorial/Steps/Linear/" + TutorialStepsList[i].name + ".asset", "TutorialStep_" + TutorialStepsList[i].StepID.ToString());
        }
        //Debug.Log("***Steps renamed to match ID! Start index = " + helperStartingIndex + " ***");
    }

    [ContextMenu("Add decrementOrIncrementAmount to stepIDs of steps from helperStartingIndex and up")]
    void DecrementStepIDsByValue()
    {
        for (int i = helperStartingIndex; i < TutorialStepsList.Count; i++)
        {
            SerializedObject serializedObject = new SerializedObject(TutorialStepsList[i]);
            int temp = serializedObject.FindProperty("StepID").intValue;
            temp += decrementOrIncrementAmount;
            serializedObject.FindProperty("StepID").intValue = temp;
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }

    [ContextMenu("Update Next Step references")]    //option for assigning next step references
    void UpdateNextStep()
    {
        SortTutorialSteps();

        for (int i = 0; i < TutorialStepsList.Count - 1; i++)
        {
            TutorialStepsList[i].NextStep = TutorialStepsList[i + 1];//AssetDatabase.LoadAssetAtPath<HospitalTutorialStep>(AssetDatabase.GetAssetPath(TutorialStepsList[i+1]));  //
            UnityEditor.EditorUtility.SetDirty(TutorialStepsList[i]);
        }

        //Debug.Log("***Updated Next Step references***");
    }

    [ContextMenu("LogStepIDs")]
    void LogStepIDs()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < TutorialStepsList.Count; i++)
        {
            sb.Append(i);
            sb.Append(",");
            sb.Append(TutorialStepsList[i].StepTag);
            sb.Append(",");
            sb.Append(TutorialStepsList[i].NecessaryCondition);
            sb.Append(",");
            sb.Append(TutorialStepsList[i].NotificationType);
            sb.Append("\n");
        }

        Debug.Log(sb.ToString());
    }

    [ContextMenu("LogStepsThatGiveExp")]
    void LogStepsThatGiveExp()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < TutorialStepsList.Count; i++)
        {
            if (TutorialStepsList[i].ExperienceReward > 0)
            {
                sb.Append(i);
                sb.Append(",");
                sb.Append(TutorialStepsList[i].StepTag);
                sb.Append(",");
                sb.Append(TutorialStepsList[i].ExperienceReward);
                sb.Append("\n");
            }
        }

        Debug.Log(sb.ToString());
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        FixIssues();
#endif
    }

    public bool disableFixIssues = false;

    [ContextMenu("FIX ISSUES")]    //clears steps list, finds, assigns and sorts all steps from project
    void FixIssues()
    {
        if (disableFixIssues)
            return;
        Debug.Log("*** [TUTORIAL EDITOR] Referencing tutorial steps***");
        SetupLinearTutorialSteps();
        SetupNonLinearTutorialSteps();
    }

    [ContextMenu("Search Steps")]
    List<HospitalTutorialStep> SearchSteps()
    {
        List<HospitalTutorialStep> steps = new List<HospitalTutorialStep>();
        string tutorialGUIDName = GetGUIDName();
        string[] stepPaths = AssetDatabase.FindAssets(tutorialGUIDName);
        int localStepsCount = stepPaths.Length;
        //Debug.LogError("Found " + stepsCount + " NON linear steps.");
        for (int i = 0; i < localStepsCount; i++)
        {
            HospitalTutorialStep step = (HospitalTutorialStep)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(stepPaths[i]), typeof(HospitalTutorialStep));
            if (!(step.name.ToUpper().Contains("OLD")))
                steps.Add(step);
        }
        List<HospitalTutorialStep> temp = Quicksort(steps);
        return temp;
    }

    List<HospitalTutorialStep> Quicksort(List<HospitalTutorialStep> list)
    {
        if (list.Count <= 1) return list;
        int pivotPosition = list.Count / 2;
        HospitalTutorialStep pivotValue = list[pivotPosition];
        list.RemoveAt(pivotPosition);
        List<HospitalTutorialStep> smaller = new List<HospitalTutorialStep>();
        List<HospitalTutorialStep> greater = new List<HospitalTutorialStep>();
        foreach (HospitalTutorialStep item in list)
        {
            if (item.StepID < pivotValue.StepID)
            {
                smaller.Add(item);
            }
            else
            {
                greater.Add(item);
            }
        }
        List<HospitalTutorialStep> sorted = Quicksort(smaller);
        sorted.Add(pivotValue);
        sorted.AddRange(Quicksort(greater));
        return sorted;
    }

    private void SetupNonLinearTutorialSteps()
    {
        NonLinearStepsList.Clear();
        int stepsCount;
        string[] stepPaths = AssetDatabase.FindAssets("NonLinearStep_");
        stepsCount = stepPaths.Length;
        //Debug.LogError("Found " + stepsCount + " NON linear steps.");
        for (int i = 0; i < stepsCount; i++)
        {
            HospitalTutorialStep step = (HospitalTutorialStep)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(stepPaths[i]), typeof(HospitalTutorialStep));
            NonLinearStepsList.Add(step);
        }
    }

    private void SetupLinearTutorialSteps()
    {
        TutorialStepsList.Clear();
        string tutorialGUIDName = GetGUIDName();
        string[] stepPaths = AssetDatabase.FindAssets(tutorialGUIDName);
        int stepsCount = stepPaths.Length;
        //Debug.LogError("Found " + stepsCount + " linear steps.");

        int lastStepId = -1;
        for (int i = 0; i < stepsCount; i++)
        {
            HospitalTutorialStep step = (HospitalTutorialStep)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(stepPaths[i]), typeof(HospitalTutorialStep));
            if (step.StepID - 1 != lastStepId)
            {
                Debug.LogError("CRITICAL ERROR! Tutorial step missing");
                Debug.LogError("lastStepId = " + lastStepId + " this step id = " + step.StepID);
            }
            lastStepId = step.StepID;
            TutorialStepsList.Add(step);
        }
        UpdateNextStep();
    }

    protected virtual string GetGUIDName()
    {
        return "";
    }
#endif
}
