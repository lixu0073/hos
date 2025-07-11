using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Hospital;
using SimpleUI;
using TMPro;
using IsoEngine;
using MovementEffects;


public class PatientCardController : UIElement
{
    [SerializeField] private CanvasGroup canvasGroup = null;

    public Animator SwitchCardAnimator;
    public Image ExitImage;
    public Image VIP;
    public GameObject VIPCounterBadge;
    public TextMeshProUGUI VIPCounter;
    public Image AvatarHead;
    public Image AvatarBody;
    public Image AvatarVIPBackground;
    public Image AvatarRegularBackground;

    public TextMeshProUGUI Name;
    public TextMeshProUGUI CoinsForCure;
    public TextMeshProUGUI EXPForCure;

    public List<Sprite> HealthyPerson;
    public GameObject SilhouetteArea;
    public GameObject CuresArea;
    public GameObject PatientInfoArea;
    public GameObject WaitInfoArea;
    public TextMeshProUGUI SpawnTimer;

    public GameObject ClockBadge;
    public GameObject OnTheWayBadge;
    public Button SpeedButton;

    public List<Image> Silhouette;
    public List<Sprite> DiagnosisSmall;
    public GameObject DiagnosisArea;
    public GameObject NoDiagnosisArea;
    public GameObject RequiresDiagnosis;
    public GameObject DiagnosisInProgress;
    public GameObject InQueue;

    public GameObject TreatmentContent;
    public GameObject TreatmentPrefab;

    public Button SendButton;
    public GameObject OtherPatientsContent;
    public GameObject OtherPatientsPrefab;
    public Animator CureBlinkAnim;
    public Button CureButton;
    public Button DischargeButton;
    public Button ExitButton;
    public GameObject VipBonus;
    public ScrollRect otherPatientsScroll;
    public TextMeshProUGUI onTheWayText;

    public bool vipGiftPending;

    public Animator InfoHoverAnim;
    public TextMeshProUGUI InfoTextSex;
    public TextMeshProUGUI InfoTextAge;
    public TextMeshProUGUI InfoTextBlood;
    public TextMeshProUGUI InfoTextOther;
    public Animator InfoHoverVIPAnim;
    public TextMeshProUGUI InfoTextVIP;

    public Animator InfoButtonAnim;
    bool infoHoverDataSet = false;

    private string diagnosisMachineName;
    private bool InDiagnosisState = false;
    //private bool InDiagnosisQueueState = false;

    private bool dischargeFirstTapButtonBlock = true;//

    private int currentWaitTimer = 0;
    [HideInInspector]
    public int selectedBedId = 0;
    private IEnumerator<float> updateWaitTimeCorountine;
    [SerializeField] private Color defaultCoinRewardColor = Color.white;
    [SerializeField] private Color defaultXPRewardColor = Color.white;
#pragma warning disable 0649
    [SerializeField] private MicroscopeSection microscopeSection;
    [SerializeField] BacteriaAvatarBackground bacteriaAvatarBackground;
    [SerializeField] PatientCardRoomGrid roomGrid;
    [SerializeField] HelpRequestPanelView helpRequestPanelView;
#pragma warning restore 0649
    Transform FanfareSpawnT;
    Coroutine scrollCoroutine;
    CharacterCreator characterCreator;

    [SerializeField] float VIPDiamondsModifier = 1;

    float lastPointerPos = -99999;
    float swipeDetectionThreshold = 0.1f;     //percent of screen width

    private Vector2 treatmentContentPos = Vector2.zero;

    public List<MedicineDatabaseEntry> showedMedicineDatabase;
    public static List<HospitalCharacterInfo> localInfo = new List<HospitalCharacterInfo>();
    private List<OtherPatient> otherList = new List<OtherPatient>();

    private HospitalCharacterInfo currentCharacter;
    public HospitalCharacterInfo CurrentCharacter
    {
        get { return currentCharacter; }
    }

    private BalanceableInt ExpForTreatmentRoom;
    private BalanceableInt GoldForTreatmentRoom;

    public enum Mode { CanDoEverything, CantCureOrDischarge, CanCureCantDischarge, VipMode, BacteriaMode, CantCureOrDischargeButCanExit }
    Mode currentMode = Mode.CanDoEverything;

    [TutorialTriggerable]
    public void SetMode(Mode mode)
    {
        Mode pastMode = currentMode;
        if (!TutorialSystem.TutorialController.ShowTutorials)
            currentMode = Mode.CanDoEverything;
        else
            currentMode = mode;

        isExitBlocked = !((currentMode == Mode.CanDoEverything) || (currentMode == Mode.CantCureOrDischargeButCanExit));
        ExitButton.interactable = (currentMode == Mode.CanDoEverything) || (currentMode == Mode.CantCureOrDischargeButCanExit);

        if (this.IsVisible)
        {
            if (currentMode == Mode.CanDoEverything)
            {
                isExitBlocked = false;
                ExitButton.interactable = true;
            }
            if (pastMode != currentMode)
            {
                if (currentCharacter != null)
                    ApplyModeSettings(currentCharacter);
                UpdateSecondPatientForTutorial();
            }
        }
    }

    [TutorialTriggerable]
    public void SetCanExitCard(bool canExit)
    {
        isExitBlocked = !canExit;
        ExitButton.interactable = canExit;
    }

    #region MainFunctions

    void Awake()
    {
        treatmentContentPos = TreatmentContent.GetComponent<RectTransform>().anchoredPosition;
    }

    /*protected override*/
    protected override void Start()
    {
        base.Start();
        showedMedicineDatabase = new List<MedicineDatabaseEntry>();
        characterCreator = ReferenceHolder.GetHospital().PersonCreator;
        ResetShowedListForDailyQuest();
        dischargeFirstTapButtonBlock = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Open(HospitalCharacterInfo info, int bedId)
    {
        if (info != null)
        {
            ExpForTreatmentRoom = BalanceableFactory.CreateXPForTreatmentRoomsBalanceable(info.EXPForCure);
            GoldForTreatmentRoom = BalanceableFactory.CreateGoldForTreatmentRoomsBalanceable(info.CoinsForCure);
        }

        SubscribeToEvents();

        gameObject.SetActive(true);
        StartCoroutine(base.Open(true, false, OnPostOpen(info, bedId)));
    }

    private Action OnPostOpen(HospitalCharacterInfo info, int bedId)
    {
        TreatmentContent.GetComponent<RectTransform>().anchoredPosition = treatmentContentPos;

        Initialize();
        Debug.LogWarning("patient card open bed id: " + bedId);

        if (UIController.get.drawer.GameObject.activeSelf)
            UIController.get.drawer.SetVisible(false);

        if (UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);

        /*
        if (HintsController.Get().isHintArrowVisible)
        {
            if (TutorialUIController.Instance.IsAnyOfTutorialScreenClosedAndItsFreePlayStep())
            {
                TutorialUIController.Instance.HideIndicator();
                HintsController.Get().isHintArrowVisible = false;
            }
        }
        */

        SoundsController.Instance.PlayPatientCardOpen();

        ReferenceHolder.Get().engine.MainCamera.BlockUserInput = true;
        Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
        dischargeFirstTapButtonBlock = true;
        //if (info != null) {
        currentCharacter = info;
        //}
        selectedBedId = bedId;
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[selectedBedId].Bed.gameObject.transform.position, 1f, false);
        SetData(info, bedId, true);

        if (info)
            NotificationCenter.Instance.PatientCardIsOpen.Invoke(new PatientCardIsOpenEventArgs(info.HasBacteria));
        else
            NotificationCenter.Instance.PatientCardIsOpen.Invoke(new PatientCardIsOpenEventArgs(true));

        ApplyModeSettings(info);

        InfoButtonUp();
        canvasGroup.interactable = true;

        this.IsVisible = false; // CV: to ensure that SetVisible checks IsVisible != visible and UIElementOpenedInvoke is called

        return null;
    }

    private void ApplyModeSettings(HospitalCharacterInfo info)
    {
        //Debug.Log("Apply Mode: " + currentMode.ToString() + " for " + info.name);
        switch (currentMode)
        {
            case Mode.CantCureOrDischarge:
                CureButton.gameObject.SetActive(false);
                DischargeButton.gameObject.SetActive(false);
                ExitButton.interactable = false;
                break;
            case Mode.CantCureOrDischargeButCanExit:
                CureButton.gameObject.SetActive(false);
                DischargeButton.gameObject.SetActive(false);
                ExitButton.interactable = true;
                isExitBlocked = false;
                break;
            case Mode.CanCureCantDischarge:
                CureButton.gameObject.SetActive(true);
                DischargeButton.gameObject.SetActive(false);
                ExitButton.interactable = false;
                break;
            case Mode.VipMode:
                if (info != null && info.IsVIP)
                {
                    TutorialUIController.Instance.BlinkImage(VIPCounterBadge.GetComponent<Image>());
                    bottomTransform.gameObject.SetActive(false);
                    CureButton.gameObject.SetActive(true);
                    DischargeButton.gameObject.SetActive(true);
                    ExitButton.interactable = false;
                    isExitBlocked = true;
                    UIController.getHospital.BlockFade(true);
                }
                else
                {
                    bottomTransform.gameObject.SetActive(true);
                    UIController.getHospital.BlockFade(false);
                    CureButton.gameObject.SetActive(true);
                    DischargeButton.gameObject.SetActive(true);
                    ExitButton.interactable = true;
                    isExitBlocked = false;
                }
                break;
            case Mode.BacteriaMode:
                if (info != null && info.HasBacteria && TutorialSystem.TutorialController.ShowTutorials)
                {
                    UIController.getHospital.BlockFade(true);
                    CureButton.gameObject.SetActive(false);
                    DischargeButton.gameObject.SetActive(false);
                    ExitButton.interactable = false;
                    isExitBlocked = true;
                }
                else
                {
                    UIController.getHospital.BlockFade(false);
                    CureButton.gameObject.SetActive(true);
                    DischargeButton.gameObject.SetActive(true);
                    ExitButton.interactable = true;
                    isExitBlocked = false;
                }
                break;
            case Mode.CanDoEverything:
            default:
                CureButton.gameObject.SetActive(true);
                DischargeButton.gameObject.SetActive(true);
                if (info != null)
                {
                    if (info.IsVIP)
                        DischargeButton.gameObject.SetActive(false);
                }
                ExitButton.interactable = true;
                isExitBlocked = false;
                bottomTransform.gameObject.SetActive(true);
                UIController.getHospital.BlockFade(false);
                break;
        }
    }

    private void SubscribeToEvents()
    {
        UnsubscribeToEvents();
        TreatmentRoomHelpController.onRefresh += RefreshHelpRequest;
        microscopeSection.BacteriaStatusChanged += MicroscopeSection_BacteriaStatusChanged;
    }

    private void MicroscopeSection_BacteriaStatusChanged(bool hasBacteria, float frequency, bool IsVip)
    {
        bacteriaAvatarBackground.SetBlinking(hasBacteria, frequency, IsVip);
    }

    private void UnsubscribeToEvents()
    {
        TreatmentRoomHelpController.onRefresh -= RefreshHelpRequest;
        microscopeSection.BacteriaStatusChanged -= MicroscopeSection_BacteriaStatusChanged;
    }

    public void SetCureButtonColor(bool isHelpProvided)
    {
        UIController.SetImageSpriteSecure(CureButton.image, isHelpProvided ? ResourcesHolder.Get().pinkOvalButton : ResourcesHolder.Get().greenOvalButton);
    }

    public void ButtonExit()
    {
        if (TutorialController.Instance.CurrentTutorialStepTag.ToString().Contains("patient_card") &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.diagnose_open_patient_card)
            return;
        Exit();
    }

    public bool isExitBlocked = false;

    [TutorialTriggerable]
    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        if (isExitBlocked)
            return;

        UnsubscribeToEvents();
        if (updateWaitTimeCorountine != null)
        {
            // Debug.LogWarning("kill corountine");
            Timing.KillCoroutine(updateWaitTimeCorountine);
            updateWaitTimeCorountine = null;
        }

        base.Exit(hidePopupWithShowMainUI);

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.positive_energy_text)
            TutorialUIController.Instance.ShowIndictator(AreaMapController.Map.FindRotatableObject(TutorialController.Instance.GetCurrentStepData().TargetMachineTag));

        HospitalAreasMapController.HospitalMap.ResetOnPressAction();
        NotificationCenter.Instance.PatientCardClosed.Invoke(new PatientCardClosedEventArgs());
        NotificationCenter.Instance.PatientCardIsClosed.Invoke(new PatientCardIsClosedEventArgs());

        ReferenceHolder.Get().engine.MainCamera.StopCameraMoveAnywayAndUnblockPlayerInteraction();
        canvasGroup.interactable = false;
        lastPointerPos = -99999;
    }

    public void Discharge(HospitalCharacterInfo info, bool isCured, float delay = 0f)
    {
        for (int i = 0; i < info.requiredMedicines.Length; ++i)
        {
            int cureAmount = info.requiredMedicines[i].Value;
            if (info.requiredMedicines[i].Key.IsDiagnosisMedicine())
            {
                if (!info.RequiresDiagnosis)
                {
                    MedicineBadgeHintsController.Get().RemoveMedicineNeededToHeal(info.requiredMedicines[i].Key, cureAmount);
                    MedicineBadgeHintsController.Get().RemoveOnlyNeededMedicine(info.requiredMedicines[i].Key.GetMedicineRef(), cureAmount);
                }
            }
            else
            {
                MedicineBadgeHintsController.Get().RemoveMedicineNeededToHeal(info.requiredMedicines[i].Key, cureAmount);
                MedicineBadgeHintsController.Get().RemoveOnlyNeededMedicine(info.requiredMedicines[i].Key.GetMedicineRef(), cureAmount);
            }
        }

        ReferenceHolder.GetHospital().treatmentRoomHelpController.CancelHelpRequest(info);
        RemovePatient(info);

        if (info.IsVIP)
        {
            //tutaj co się stanie po uleczeniu (here what will happen after healing)
            if (HospitalDataHolder.Instance.QueueContainsPatient(info.GetComponent<VIPPersonController>()))
            {
                info.GetComponent<VIPPersonController>().CancelDiagnose(HospitalDataHolder.Instance.ReturnPatientDiagnosisRoom(info.GetComponent<VIPPersonController>()));
                HospitalDataHolder.Instance.ReturnPatientQueue(info.GetComponent<VIPPersonController>()).Remove(info.GetComponent<VIPPersonController>());
            }

            Timing.RunCoroutine(DelayedGoHome(info, isCured, delay));

            info.GetComponent<Hospital.VIPPersonController>().goHome = true;

            // for future dailyQuest ? dissmis VIP or normalPatient?

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.VIPHealed);
        }
        else
        {
            if (HospitalDataHolder.Instance.QueueContainsPatient(info.GetComponent<Hospital.HospitalPatientAI>()))
            {
                info.GetComponent<Hospital.HospitalPatientAI>().CancelDiagnose(HospitalDataHolder.Instance.ReturnPatientDiagnosisRoom(info.GetComponent<Hospital.HospitalPatientAI>()));
                HospitalDataHolder.Instance.ReturnPatientQueue(info.GetComponent<Hospital.HospitalPatientAI>()).Remove(info.GetComponent<Hospital.HospitalPatientAI>());
            }

            Timing.RunCoroutine(DelayedGoHome(info, isCured, delay));

            info.GetComponent<Hospital.HospitalPatientAI>().goHome = true;

            // for future dailyQuest ? dissmis VIP or normalPatient?

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.BedPatientHealed);
        }

        //if (bed!=null)
        //    bed.Patient = null;

        //RefreshView(null);
        //info.GetComponent<Hospital.HospitalPatientAI> ().GoHomeSweetHome (delay);

        Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
        dischargeFirstTapButtonBlock = true;

        info.RequiresDiagnosis = false;
        HospitalDataHolder.Instance.CheckAllDiagRooms();

        Exit();
    }

    private void CurePatient(HospitalCharacterInfo info, bool afterMissing)
    {
        if (info != null)
        {
            int check = 0;
            int amount1, amount2;
            List<KeyValuePair<int, MedicineDatabaseEntry>> missing = new List<KeyValuePair<int, MedicineDatabaseEntry>>();

            Dictionary<MedicineRef, int> requestedMeds = ReferenceHolder.GetHospital().treatmentRoomHelpController.GetHelpedMedicinesForPatient(info);

            var helpers = ReferenceHolder.GetHospital().treatmentRoomHelpController.GetHelpersInfoForPatient(info);

            for (int i = 0; i < info.requiredMedicines.Length; ++i)
            {
                amount1 = info.requiredMedicines[i].Value;
                amount2 = GameState.Get().GetCureCount(info.requiredMedicines[i].Key.GetMedicineRef()) + GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef());
                DiseaseType dt = DiseaseType.None;
                if (info.requiredMedicines[i].Key.Disease != null)
                    dt = info.requiredMedicines[i].Key.Disease.DiseaseType;

                if ((info.RequiresDiagnosis || InDiagnosisState) && info.requiredMedicines[i].Key.IsDiagnosisMedicine())
                {
                    missing.Add(new KeyValuePair<int, MedicineDatabaseEntry>(info.requiredMedicines[i].Value, info.requiredMedicines[i].Key));
                }
                else if (amount1 <= amount2)
                {
                    ++check;
                }
                else
                {
                    missing.Add(new KeyValuePair<int, MedicineDatabaseEntry>(amount1 - amount2, info.requiredMedicines[i].Key));
                }
            }

            //Debug.LogError("info.RequiresDiagnosis = " + info.RequiresDiagnosis);
            //Debug.LogError("InDiagnosisState = " + InDiagnosisState);
            if (check == info.requiredMedicines.Length)
            {
                float delay = 0;

                for (int i = 0; i < info.requiredMedicines.Length; ++i)
                {
                    int cureAmount = info.requiredMedicines[i].Value;
                    int usedAmount = info.requiredMedicines[i].Value - GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef());

                    if (usedAmount > 0)
                        GameState.Get().GetCure(info.requiredMedicines[i].Key.GetMedicineRef(), usedAmount, EconomySource.BedPatient);

                    delay = i * 0.4f;

                    if (currentCharacter == null)
                        Debug.LogError("currentCharacter is NULL");
                    if (info.requiredMedicines[i].Key.GetMedicineRef() == null)
                        Debug.LogError("medicine is NULL");

                    ReferenceHolder.Get().giftSystem.CreateItemUsed(currentCharacter.transform.position, cureAmount, delay, ResourcesHolder.Get().GetSpriteForCure(info.requiredMedicines[i].Key.GetMedicineRef()));
                }

                if (info.IsVIP)
                {
                    Timing.RunCoroutine(VIPDelayedGift());
                    if (afterMissing)
                        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.VIP.ToString(), (int)FunnelStepVip.VipCuredWithDiamonds, FunnelStepVip.VipCuredWithDiamonds.ToString());
                    else
                        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.VIP.ToString(), (int)FunnelStepVip.VipCured, FunnelStepVip.VipCured.ToString());

                    if (currentMode == Mode.VipMode)
                    {
                        isExitBlocked = false;
                        bottomTransform.gameObject.SetActive(true);
                        TutorialUIController.Instance.InGameCloud.Hide();
                    }

                    /*FanfareSpawnT = HospitalAreasMapController.Map.vipRoom.FloatingInfoPosition;
                    GameObject FanfareParticles = Instantiate(ResourcesHolder.Get().ParticleBedPatientCured, FanfareSpawnT.position, Quaternion.Euler(45, 45, 0)) as GameObject;
                    Destroy(FanfareParticles, 2f);
                    GameObject floatingInfo = FanfareSpawnT.GetChild(0).gameObject;
                    if (!info.Name.Contains("_"))
                    {
                        floatingInfo.GetComponent<TextMeshPro>().text = info.Name + " " + info.Surname + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    }
                    else {
                        floatingInfo.GetComponent<TextMeshPro>().text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + info.Surname) + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    }*/
                }
                else
                {
                    FanfareSpawnT = HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[selectedBedId].Bed.transform.GetChild(7);

                    if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                    {
                        GameObject FanfareParticles = Instantiate(ResourcesHolder.GetHospital().ParticleBedPatientCured, FanfareSpawnT.position, Quaternion.Euler(45, 45, 0)) as GameObject;
                        Destroy(FanfareParticles, 2f);
                    }

                    GameObject floatingInfo = FanfareSpawnT.GetChild(0).gameObject;

                    if (!info.Name.Contains("_"))
                        floatingInfo.GetComponent<TextMeshPro>().text = info.Name + " " + info.Surname + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";
                    else
                        floatingInfo.GetComponent<TextMeshPro>().text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + info.Surname) + " " + I2.Loc.ScriptLocalization.Get("CURED") + "!";

                    floatingInfo.gameObject.SetActive(false);
                    floatingInfo.gameObject.SetActive(true);
                    floatingInfo.GetComponent<Animator>().SetTrigger("PlayFloat");
                }

                if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                    Timing.RunCoroutine(PatientHealedFanfare(info.transform, FanfareSpawnT, info.IsVIP));

                info.patientCardStatus = HospitalCharacterInfo.PatientCardInfoStatus.Cured;
                if (info.DisaseDiagnoseType != DiseaseType.None && (TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.diagnose_spawn) &&
                                                                    TutorialController.Instance.CurrentTutorialStepIndex <= TutorialController.Instance.GetStepId(StepTag.positive_energy_text)))
                {
                    TutorialController.Instance.SetStep(TutorialController.Instance.TutorialStepsList[TutorialController.Instance.GetStepId(StepTag.positive_energy_text) + 1].StepTag);
                    TutorialUIController.Instance.StopShowCoroutines();
                    TutorialUIController.Instance.InGameCloud.Hide();
                }
                Discharge(info, isCured: true, delay: 1.5f);

                //Achievements
                AchievementNotificationCenter.Instance.TreatmentCenterPatientCured.Invoke(new TimedAchievementProgressEventArgs(1, Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds)));

                if (info.HasBacteria)
                {
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.PreventBacteriaSpread));
                    AchievementNotificationCenter.Instance.NoYouDont.Invoke(new AchievementProgressEventArgs(1));
                    SoundsController.Instance.PlayPatientDecontaminated();
                    HashSet<StepTag> stepTagsToSkip = new HashSet<StepTag>() { StepTag.bacteria_george_1, StepTag.bacteria_george_2, StepTag.bacteria_george_3, StepTag.bacteria_george_4, StepTag.bacteria_george_5 };
                    if (stepTagsToSkip.Contains(TutorialSystem.TutorialController.CurrentStep.StepTag) || !TutorialSystem.TutorialController.ShowTutorials)
                    {
                        HideBacteriaTutorialArrow();
                        HideBacteriaTutorialMask();
                        HideBacteriaTutorialInfo();
                        TutorialUIController.Instance.HideTapToContinue();
                        //NotificationListener.Instance.CancelInvoke("EnableTapAnywhere");
                        TutorialSystem.TutorialController.SkipTo(StepTag.level_20);
                    }
                }
                else
                    SoundsController.Instance.PlayTreatmentRoomCure();

                //DailyQuests, GlobalEvents and Level Goals magic stuff
                if (!VisitingController.Instance.IsVisiting)
                {
                    if (info.IsVIP)
                    {
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CureVips));
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TreatmentRoomPatients));
                    }
                    else
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TreatmentRoomPatients));

                    // Level Goals
                    ObjectiveNotificationCenter.Instance.HospitalPatientWithInfoObjectiveUpdate.Invoke(new ObjectiveHospitalPatientWithInfoEventArgs(1, info.BloodType, info.Sex, info.IsVIP));
                    List<MedicineDatabaseEntry> filtredOutMedicine = ResourcesHolder.Get().FilterOutRepeatingDiseases(info.requiredMedicines);
                    bool hasPatientWithPlantBeenUsed = false;
                    foreach (MedicineDatabaseEntry medicine in filtredOutMedicine)
                    {
                        if (medicine.Disease != null)
                        {
                            bool isPlantNeeded = false;
                            if (!hasPatientWithPlantBeenUsed)
                            {
                                isPlantNeeded = medicine.GetMedicineRef().type == MedicineType.BasePlant ? true : false;
                                if (isPlantNeeded)
                                    hasPatientWithPlantBeenUsed = true;
                            }
                            // if(medicine.Disease.DiseaseType != info.DisaseDiagnoseType)
                            ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveHospitalPatientWithDiseaseEventArgs(1, medicine.Disease.DiseaseType, isPlantNeeded));
                        }
                    }

                    // Global Event
                    GlobalEventNotificationCenter.Instance.CurePatientGlobalEvent.Invoke(new GlobalEventCurePatientProgressEventArgs("2xBedsRoom"));
                    //if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.cure_bed_patient)
                    //TutorialUIController.Instance.tutorialArrowUI.Hide();
                }

                int expReward = ExpForTreatmentRoom.GetBalancedValue(); //TimedEventsCalculator.GetParamValueAfterEventAndBooster( BalancedParam.Key.increaseXPForTreatmentRooms, info.EXPForCure);  //multiply by boosters
                int currentEXPAmount = Game.Instance.gameState().GetExperienceAmount();
                GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.BedPatientCured, false);

                int coinReward = GoldForTreatmentRoom.GetBalancedValue(); //TimedEventsCalculator.GetParamValueAfterEventAndBooster(BalancedParam.Key.increaseGoldForTreatmentRooms, info.CoinsForCure);  //multiply by boosters
                int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                GameState.Get().AddResource(ResourceType.Coin, coinReward, EconomySource.BedPatientCured, false);

                if (helpers.Count == 0 || !DefaultConfigurationProvider.GetConfigCData().TreatmentHelpSummaryPopupEnabled)
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, currentCharacter.transform.position, expReward, delay + .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                    {
                        GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentEXPAmount);
                    });

                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, currentCharacter.transform.position, coinReward, delay + 1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                    {
                        GameState.Get().UpdateCounter(ResourceType.Coin, coinReward, currentCoinAmount);
                    });
                }

                HospitalCharacterInfo otherInfo;
                int infectionTime = info.GetTimeTillInfection(out otherInfo);

                if (otherInfo == null && infectionTime > 0)
                {
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.PositiveEnergy, currentCharacter.transform.position, info.PositiveEnergyForCure, delay + .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[4], null, null, false);
                    GameState.Get().AddResource(ResourceType.PositiveEnergy, info.PositiveEnergyForCure, EconomySource.BacteriaCured, true);
                }

                AnalyticsController.instance.ReportBedPatient(AnalyticsPatientAction.BedCured, expReward, coinReward, afterMissing, info);

                //AdvancedPatientCounter
                if (!VisitingController.Instance.IsVisiting)
                {
                    if (info.IsVIP)
                        GameState.Get().PatientsCount.AddPatientsCuredVIP();
                    else
                        GameState.Get().PatientsCount.AddPatientsCuredBed();

                    ++GameState.Get().patientsHealedEver;
                }

                Exit();

                if (helpers.Count > 0 && DefaultConfigurationProvider.GetConfigCData().TreatmentHelpSummaryPopupEnabled)
                {
                    Vector3 patientPos = currentCharacter.transform.position;
                    gameObject.SetActive(true);
                    UIController.getHospital.treatmentHelpSummaryPopup.Open(helpers, () =>
                    {
                        currentEXPAmount = Game.Instance.gameState().GetExperienceAmount();
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, patientPos, expReward, delay + .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentEXPAmount);
                        });

                        currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, patientPos, coinReward, delay + 1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Coin, coinReward, currentCoinAmount);
                        });
                    });
                }
            }
            else
            {
                bool canBepatientRequested = !info.IsVIP && !ReferenceHolder.GetHospital().treatmentRoomHelpController.IsHelpRequestForPatient(info);

                if (!TreatmentRoomHelpController.HasTreatmentRoomHelpFeatureMinLevel)
                    canBepatientRequested = false;

                //Debug.LogError("info.RequiresDiagnosis = " + info.RequiresDiagnosis + "  InDiagnosisState = " + InDiagnosisState);
                float modifier = (info.IsVIP) ? VIPDiamondsModifier : 1;

                gameObject.SetActive(true);
                UIController.get.BuyResourcesPopUp.Open(missing, info.RequiresDiagnosis, InDiagnosisState, true, () =>
                {
                    info.RequiresDiagnosis = false;
                    InDiagnosisState = false;
                    CurePatient(info, true);
                    RefreshView(info);
                    UpdateOtherPatients();

                }, null, () =>
                {
                    if (ReferenceHolder.GetHospital().treatmentRoomHelpController.CheckIfHelpRequestPossible())
                    {
                        RequestHelp(info);
                        UIController.get.BuyResourcesPopUp.Exit();
                    }
                    else
                    {
                        string message = string.Format(I2.Loc.ScriptLocalization.Get("NEXT_REQUEST_IN"), UIController.GetFormattedShortTime(ReferenceHolder.GetHospital().treatmentRoomHelpController.NextRequestTime));
                        MessageController.instance.ShowMessage(message);
                    }

                }, modifier, canBepatientRequested);
            }
        }
    }
    #endregion

    #region Counters
    void UpdateVipCounter()
    {
        if (currentCharacter != null && currentCharacter.IsVIP)
        {
            if (VIPCounterBadge.activeSelf)
                VIPCounter.text = UIController.GetFormattedShortTime((int)currentCharacter.VIPTime);

            if ((int)currentCharacter.VIPTime <= 0)
            {
                VIP.enabled = false;
                VIPCounterBadge.SetActive(false);

                if (currentCharacter != null)
                    currentCharacter.patientCardStatus = HospitalCharacterInfo.PatientCardInfoStatus.Discharged;

                Discharge(currentCharacter, isCured: false, delay: 0.1f);
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.VIP.ToString(), (int)FunnelStepVip.VipNotCured, FunnelStepVip.VipNotCured.ToString());
            }
        }
    }
    #endregion

    #region SetData
    void SetData(HospitalCharacterInfo info, int bedID, bool instant, bool animRight = false)
    {
        //Debug.LogError("SetData");
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[bedID].Bed.gameObject.transform.position, 1f, false);
        Timing.RunCoroutine(SwitchCardsCoroutine(info, bedID, instant, animRight));

        if (info != null)
        {
            NotificationCenter.Instance.PatientCardOpened.Invoke(new PatientCardOpenedEventArgs(info));
            if (info.HasBacteria) SoundsController.Instance.PlayPatientContaminated();
        }
    }

    void SetSilhouetteImageDelegate(Image img, HospitalCharacterInfo info, int cureIndex)
    {
        img.GetComponent<PointerDownListener>().SetDelegate(() =>
        {
            if (!UIController.getHospital.PatientCard.showedMedicineDatabase.Contains(info.requiredMedicines[cureIndex].Key))
            {
                UIController.getHospital.PatientCard.showedMedicineDatabase.Add(info.requiredMedicines[cureIndex].Key);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TapOnAPatient));
            }

            TextTooltip.Open(info.requiredMedicines[cureIndex].Key.Disease, info.requiredMedicines[cureIndex].Key.GetMedicineRef());
        });
    }

    void SetRandomOnTheWayText()
    {
        onTheWayText.text = I2.Loc.ScriptLocalization.Get("ON_THE_WAY/ON_THE_WAY_" + UnityEngine.Random.Range(0, 30));
    }

    void SetHealthy()
    {
        Silhouette[0].enabled = false;
        for (int i = 1; i < Silhouette.Count; ++i)
        {
            if (i < 19)
            {
                Silhouette[i].enabled = false;
                Silhouette[i].color = new Color(255, 255, 255, 0.7f);
            }
            else
            {
                Silhouette[i].enabled = false;
                Silhouette[i].color = new Color(255, 255, 255, 0.7f);
            }
        }
    }

    void SetStateArea(HospitalCharacterInfo info, bool diagnosticRoomExists)
    {
        UIController.SetButtonClickSoundInactiveSecure(SendButton.gameObject, false);
        SendButton.image.material = null;
        if (info.IsVIP)
        {
            VIPPersonController infoAI = info.GetComponent<VIPPersonController>();

            if (info.RequiresDiagnosis)
            {
                SendButton.onClick.RemoveAllListeners();

                bool roomExist = HospitalDataHolder.Instance.isRoomExistForPatient(infoAI);
                //Debug.LogError("roomExist = " + roomExist);
                if (roomExist != false)
                {
                    SendButton.onClick.RemoveAllListeners();
                    SendButton.onClick.AddListener(delegate ()
                    {
                        UIController.PlayClickSoundSecure(SendButton.gameObject);
                        if ((infoAI.state == VIPPersonController.CharacterStatus.Diagnose) || (infoAI.state == VIPPersonController.CharacterStatus.InQueue))
                            ShowPatientDiagnosticRoom(infoAI);
                        else
                        {
                            if (HospitalDataHolder.Instance.ReturnDiseaseQueue((int)info.DisaseDiagnoseType).Count < HospitalDataHolder.Instance.MaxQueueSize)
                            {
                                MessageController.instance.ShowMessage(40);
                                SetPatientToDiagnose(infoAI);
                            }
                            else
                            {
                                MessageController.instance.ShowMessage(40);
                                Debug.LogError("that");
                            }
                        }

                        //SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisInProgreess, info);
                    });
                }
                else
                {
                    //Debug.LogError("diagnose button grayscale");
                    UIController.SetButtonClickSoundInactiveSecure(SendButton.gameObject, true);
                    SendButton.image.material = ResourcesHolder.Get().GrayscaleMaterial;
                    SendButton.onClick.RemoveAllListeners();
                    SendButton.onClick.AddListener(delegate ()
                    {
                        UIController.PlayClickSoundSecure(SendButton.gameObject);
                        HospitalDataHolder.Instance.ShowMessageRoomNeededForPatient(infoAI);
                    });
                }
            }
            else
            {
                switch (infoAI.state)
                {
                    case VIPPersonController.CharacterStatus.Diagnose:
                        SendButton.gameObject.SetActive(false);
                        CureButton.gameObject.SetActive(true);
                        DischargeButton.enabled = true;

                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisInProgreess, info);

                        SendButton.onClick.RemoveAllListeners();
                        SendButton.onClick.AddListener(delegate ()
                        {
                            UIController.PlayClickSoundSecure(SendButton.gameObject);
                            ShowPatientDiagnosticRoom(infoAI);
                        });
                        break;
                    case VIPPersonController.CharacterStatus.InQueue:
                        CureButton.gameObject.SetActive(true);
                        SendButton.gameObject.SetActive(false);
                        DischargeButton.enabled = true;

                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnoseInQueue, info);

                        SendButton.onClick.RemoveAllListeners();
                        SendButton.onClick.AddListener(delegate ()
                        {
                            UIController.PlayClickSoundSecure(SendButton.gameObject);
                            ShowPatientDiagnosticRoom(infoAI);
                        });
                        break;
                    case VIPPersonController.CharacterStatus.Healed:
                        SendButton.onClick.RemoveAllListeners();
                        SendButton.gameObject.SetActive(false);
                        DischargeButton.enabled = true;

                        SetDiagnosisPanel(DiagnosisPanelMode.NoDiagnosisRequire, info);

                        CureButton.gameObject.SetActive(true);                        
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            HospitalPatientAI infoAI = info.GetComponent<HospitalPatientAI>();

            if (info.RequiresDiagnosis)
            {
                bool roomExist = HospitalDataHolder.Instance.isRoomExistForPatient(infoAI);
                //Debug.LogError("roomExist = " + roomExist);
                if (roomExist != false)
                {
                    SendButton.onClick.RemoveAllListeners();
                    SendButton.onClick.AddListener(delegate ()
                    {
                        UIController.PlayClickSoundSecure(SendButton.gameObject);
                        if ((infoAI.state == HospitalPatientAI.CharacterStatus.Diagnose) || (infoAI.state == HospitalPatientAI.CharacterStatus.InQueue))
                            ShowPatientDiagnosticRoom(infoAI);
                        else
                        {
                            if (HospitalDataHolder.Instance.ReturnDiseaseQueue((int)info.DisaseDiagnoseType).Count < HospitalDataHolder.Instance.MaxQueueSize)
                            {
                                MessageController.instance.ShowMessage(40);
                                SetPatientToDiagnose(infoAI);
                            }
                            else
                            {
                                MessageController.instance.ShowMessage(40);
                                Debug.LogError("this");
                            }
                        }
                    });
                }
                else
                {
                    //Debug.LogError("diagnose button grayscale");
                    UIController.SetButtonClickSoundInactiveSecure(SendButton.gameObject, true);
                    SendButton.image.material = ResourcesHolder.Get().GrayscaleMaterial;
                    SendButton.onClick.RemoveAllListeners();
                    SendButton.onClick.AddListener(delegate ()
                    {
                        UIController.PlayClickSoundSecure(SendButton.gameObject);
                        HospitalDataHolder.Instance.ShowMessageRoomNeededForPatient(infoAI);
                    });
                }
            }
            else
            {
                switch (infoAI.state)
                {
                    case HospitalPatientAI.CharacterStatus.Diagnose:
                        SendButton.gameObject.SetActive(false);
                        CureButton.gameObject.SetActive(true);
                        DischargeButton.enabled = true;

                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisInProgreess, info);

                        SendButton.onClick.RemoveAllListeners();
                        SendButton.onClick.AddListener(delegate ()
                        {
                            UIController.PlayClickSoundSecure(SendButton.gameObject);
                            ShowPatientDiagnosticRoom(infoAI);
                        });
                        break;
                    case HospitalPatientAI.CharacterStatus.InQueue:
                        CureButton.gameObject.SetActive(true);
                        SendButton.gameObject.SetActive(false);
                        DischargeButton.enabled = true;

                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnoseInQueue, info);

                        SendButton.onClick.RemoveAllListeners();
                        SendButton.onClick.AddListener(delegate ()
                        {
                            UIController.PlayClickSoundSecure(SendButton.gameObject);
                            ShowPatientDiagnosticRoom(infoAI);
                        });
                        break;
                    case HospitalPatientAI.CharacterStatus.Healed:
                        SendButton.onClick.RemoveAllListeners();
                        SendButton.gameObject.SetActive(false);
                        DischargeButton.enabled = true;

                        SetDiagnosisPanel(DiagnosisPanelMode.NoDiagnosisRequire, info);

                        CureButton.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    void SetDiagnosisPanel(DiagnosisPanelMode mode, HospitalCharacterInfo info, bool allowWhenDiagnosisInQueue = true)
    {
        if (!allowWhenDiagnosisInQueue)
            return;

        //Debug.LogError("panel:" + mode);
        NoDiagnosisArea.SetActive(false);
        DiagnosisArea.SetActive(false);
        RequiresDiagnosis.SetActive(false);
        InQueue.SetActive(false);
        DiagnosisInProgress.SetActive(false);
        helpRequestPanelView.gameObject.SetActive(false);

        if (!TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.cure_bed_patient))
        {
            //all diagnosis panels disabled on tutorial level 3.
        }
        else if (mode == DiagnosisPanelMode.DiagnosisRequire)
        {
            DiagnosisArea.SetActive(true);
            RequiresDiagnosis.SetActive(true);

            if (GameState.Get().PositiveEnergyAmount >= HospitalDataHolder.Instance.NeededPositiveEnergy((int)info.DisaseDiagnoseType))
                RequiresDiagnosis.transform.GetChild(2).gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(.157f, .165f, .165f);
            else
                RequiresDiagnosis.transform.GetChild(2).gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.red;

            RequiresDiagnosis.transform.GetChild(2).gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                (
                    HospitalDataHolder.Instance.NeededPositiveEnergy((int)info.DisaseDiagnoseType) > 0 ?
                    GameState.Get().PositiveEnergyAmount.ToString() + "/" + HospitalDataHolder.Instance.NeededPositiveEnergy((int)info.DisaseDiagnoseType).ToString() :
                    "FREE"
                );
            SendButton.gameObject.SetActive(true);
            SendButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get("SEND");
        }
        else if (mode == DiagnosisPanelMode.DiagnosisInProgreess)
        {
            DiagnosisArea.SetActive(true);
            DiagnosisInProgress.SetActive(true);
            SendButton.gameObject.SetActive(true);
            SendButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get("CHECK");
        }
        else if (mode == DiagnosisPanelMode.DiagnoseInQueue)
        {
            DiagnosisArea.SetActive(true);
            InQueue.SetActive(true);
            SendButton.gameObject.SetActive(true);
            SendButton.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get("CHECK");
        }
        else if (mode == DiagnosisPanelMode.NoDiagnosisRequire)
        {
            //NoDiagnosisArea.SetActive(true);
            helpRequestPanelView.gameObject.SetActive(false);

            if (TreatmentRoomHelpController.HasTreatmentRoomHelpFeatureMinLevel)
            {
                if (!info.IsVIP)
                {
                    helpRequestPanelView.gameObject.SetActive(true);

                    if (ReferenceHolder.GetHospital().treatmentRoomHelpController.IsHelpRequestForPatient(info))
                    {
                        helpRequestPanelView.ShowActiveHelpRequestPanel();
                    }
                    else if (ReferenceHolder.GetHospital().treatmentRoomHelpController.CheckIfHelpRequestPossible())
                    {
                        helpRequestPanelView.ShowAskForHelpButton();
                        helpRequestPanelView.BlinkAskForHelpButton(!GameState.Get().wasTreatmentHelpRequested && !info.CheckCurePosible(out bool cureWithHelp));
                        helpRequestPanelView.SetAvailableHelpRequestsButton(ReferenceHolder.GetHospital().treatmentRoomHelpController.AvailableRequests, ReferenceHolder.GetHospital().treatmentRoomHelpController.MaxRequests, delegate ()
                        {
                            RequestHelp(info);
                            helpRequestPanelView.ScaleUpActiveHelpRequestPanel(true);
                        });
                    }
                    else
                    {
                        helpRequestPanelView.ShowAskForHelpButton();
                        helpRequestPanelView.BlinkAskForHelpButton(false);
                        helpRequestPanelView.SetAvailableHelpRequestsButton(ReferenceHolder.GetHospital().treatmentRoomHelpController.AvailableRequests, ReferenceHolder.GetHospital().treatmentRoomHelpController.MaxRequests, delegate ()
                        {
                            string message = string.Format(I2.Loc.ScriptLocalization.Get("NEXT_REQUEST_IN"), UIController.GetFormattedShortTime(ReferenceHolder.GetHospital().treatmentRoomHelpController.NextRequestTime));
                            MessageController.instance.ShowMessage(message);
                        });
                    }
                }
            }
        }
    }

    void RequestHelp(HospitalCharacterInfo info)
    {
        if (info != null)
        {
            int index = selectedBedId;

            ReferenceHolder.GetHospital().treatmentRoomHelpController.RequestHelp(info);

            SaveSynchronizer.Instance.InstantSave();

            List<IFollower> friends = ReferenceHolder.GetHospital().treatmentRoomHelpController.GetFriendsForPushPopup();
            if (friends.Count > 0)
            {
                Exit();

                List<string> friendsPushList = new List<string>();
                UIController.getHospital.treatmentSendPushesPopup.Open(friends,
                    (friend, friendsView) =>
                    {
                        string id = friend.GetSaveID();
                        if (friendsView.selectFriendToggle.isOn)
                        {
                            if (!friendsPushList.Contains(id))
                                friendsPushList.Add(id);
                        }
                        else
                        {
                            if (friendsPushList.Contains(id))
                                friendsPushList.Remove(id);

                            UIController.getHospital.treatmentSendPushesPopup.SetAllTogle(false);
                        }
                    },
                    (selected) =>
                    {
                        UIController.getHospital.treatmentSendPushesPopup.SetAllSelected(selected);
                    },
                    () =>
                    {
                        if (friendsPushList.Count > 0)
                            ReferenceHolder.GetHospital().treatmentRoomHelpController.RequestHelp(info, friendsPushList);
                        UIController.getHospital.treatmentSendPushesPopup.Exit();
                    },
                    () =>
                    {

                    },
                    () =>
                    {
                        if (info != null && index >= 0)
                            Open(info, index);
                    }
                    );
            }
        }
    }

    void AddTreatment(HospitalCharacterInfo info, MedicineDatabaseEntry data, int cureAmount, int helpedCureAmount)
    {
        GameObject temp = Instantiate(TreatmentPrefab);
        temp.transform.SetParent(TreatmentContent.transform);
        temp.transform.localScale = Vector3.one;
        temp.transform.localRotation = Quaternion.identity;
        TreatmentPanel tp = temp.GetComponent<TreatmentPanel>();
        tp.Initialize(data, cureAmount, helpedCureAmount);
    }

    void AddDiagnosis(DiseaseType disaseType)
    {
        GameObject temp = Instantiate(TreatmentPrefab);
        temp.transform.SetParent(TreatmentContent.transform);
        temp.transform.localScale = Vector3.one;
        temp.transform.localRotation = Quaternion.identity;
        TreatmentPanel tp = temp.GetComponent<TreatmentPanel>();
        tp.Initialize(disaseType);
    }

    void AddBedInfo(int id)
    {
        GameObject otherPatient = Instantiate(OtherPatientsPrefab);
        otherPatient.transform.SetParent(OtherPatientsContent.transform);
        otherPatient.transform.localScale = Vector3.one;
        OtherPatient op = otherPatient.GetComponent<OtherPatient>();
        HospitalBedController hbc = HospitalAreasMapController.HospitalMap.hospitalBedController;
        //Debug.LogError("SetInfo from AddBedInfo null id: " + id);
        op.SetInfo(null, hbc.GetBedStatusForID(id), hbc.GetWaitTimerForBed(id));
        op.SetButton(delegate ()
        {
            Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
            dischargeFirstTapButtonBlock = true;
            //Debug.LogError("OnClick id: " + id);
            if (selectedBedId == id)
                return;
            selectedBedId = id;
            currentCharacter = null;
            SetData(null, id, false);
        });

        otherList.Add(op);
        localInfo.Add(null);
    }
    #endregion

    #region Refresh & Updates
    void ResetBedInfoAtId(int which)
    {
        //Debug.LogWarning("Reset pos in PatientCard at pos: " + which);
        if (which < OtherPatientsContent.transform.childCount)
        {
            OtherPatient op = OtherPatientsContent.transform.GetChild(which).GetComponent<OtherPatient>();
            //Debug.LogError("SetInfo from ResetBedInfoAtId null id: " + which);
            op.SetInfo(null, -1);
            op.SetButton(delegate ()
            {
                //Debug.LogError("OnClick which: " + which);
                SetData(null, which, false);
            });
            localInfo[which] = null;
        }
    }

    public void ResetShowedListForDailyQuest()
    {
        showedMedicineDatabase.Clear();
    }

    public void ResetPatientCard()
    {
        Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
        dischargeFirstTapButtonBlock = true;
        otherList.Clear();
        localInfo.Clear();
        ResetShowedListForDailyQuest();

        if (OtherPatientsContent != null && OtherPatientsContent.transform.childCount > 0)
        {
            for (int i = 0; i < OtherPatientsContent.transform.childCount; i++)
            {
                Destroy(OtherPatientsContent.transform.GetChild(i).gameObject);
            }
        }
    }

    private void RefreshHelpRequest()
    {
        if (UIController.getHospital.PatientCard.gameObject.activeSelf)
        {
            if (UIController.getHospital.PatientCard.CurrentCharacter != null)
                UIController.getHospital.PatientCard.RefreshView(UIController.getHospital.PatientCard.CurrentCharacter);

            UIController.getHospital.PatientCard.UpdateOtherPatients();
        }
    }

    public void RefreshViewOnBed(int bedID)
    {
        if (gameObject.activeInHierarchy)
        {
            if (bedID != selectedBedId)
                return;

            HospitalCharacterInfo info = null;
            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count > bedID)
            {
                if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[bedID].Patient != null)
                {
                    if (((BasePatientAI)HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[bedID].Patient).GetComponent<HospitalCharacterInfo>() != null)
                        info = ((BasePatientAI)HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[bedID].Patient).GetComponent<HospitalCharacterInfo>();
                }
            }

            if (/*info != null &&*/ info != currentCharacter)
            {
                currentCharacter = info;
                SetData(info, bedID, false);
            }
            else
                RefreshView(currentCharacter);

            UpdateOtherPatients();
        }

        UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();
    }

    public void RefreshView(HospitalCharacterInfo info)
    {
        infoHoverDataSet = false;
        microscopeSection.SetupBacteriaInfo(info);
        microscopeSection.SetupVitaminInfo(info);

        //Debug.Log("Refresh view. Patientinfo: " + info.ID + " coins: " + info.CoinsForCure + " exp: " + info.EXPForCure);
        bool goHome = (info != null) && info.GetComponentInParent<IDiagnosePatient>().GetGoHome();
        bool statusDefault = (info != null) && (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Default);

        if (info != null && !goHome && statusDefault)
        {
            SilhouetteArea.SetActive(true);
            CuresArea.SetActive(true);
            PatientInfoArea.SetActive(true);
            WaitInfoArea.SetActive(false);
            ClockBadge.SetActive(false);
            OnTheWayBadge.SetActive(false);

            if (info == currentCharacter)
            {
                SendButton.gameObject.SetActive(false);

                //if (info != null)
                currentCharacter = info;

                if (info.IsVIP)
                {
                    DischargeButton.gameObject.SetActive(false);
                    VIP.enabled = true;
                    VipBonus.SetActive(true);
                    VIPCounterBadge.SetActive(true);
                    AvatarVIPBackground.gameObject.SetActive(true);
                    AvatarRegularBackground.gameObject.SetActive(false);
                }
                else
                {
                    DischargeButton.gameObject.SetActive(true);
                    VIP.enabled = false;
                    VIPCounterBadge.SetActive(false);
                    VipBonus.SetActive(false);
                    AvatarVIPBackground.gameObject.SetActive(false);
                    AvatarRegularBackground.gameObject.SetActive(true);
                }

                InDiagnosisState = false;
                if (info.IsVIP)
                {
                    //tu co w else tylko odpowiednio dla vipa
                    VIPPersonController infoAI = info.GetComponent<VIPPersonController>();

                    if (infoAI.state == VIPPersonController.CharacterStatus.None)
                    {
                        if (InDiagnosisState)
                        {
                            SetDiagnosisPanel(DiagnosisPanelMode.DiagnoseInQueue, info);
                        }
                        else if (info.RequiresDiagnosis)
                        {
                            SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);
                        }
                        else
                        {
                            SetDiagnosisPanel(DiagnosisPanelMode.NoDiagnosisRequire, info);
                            CureButton.gameObject.SetActive(true);
                        }
                    }
                    else if (infoAI.state == VIPPersonController.CharacterStatus.InQueue)
                    {
                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnoseInQueue, info);
                    }
                    else if (infoAI.state == VIPPersonController.CharacterStatus.Diagnose)
                    {
                        InDiagnosisState = true;
                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisInProgreess, info);
                    }
                    else if (infoAI.state == VIPPersonController.CharacterStatus.Healed)
                    {
                        SetDiagnosisPanel(DiagnosisPanelMode.NoDiagnosisRequire, info);
                        CureButton.gameObject.SetActive(true);
                    }
                }
                else
                {
                    HospitalPatientAI infoAI = info.GetComponent<HospitalPatientAI>();

                    if (infoAI.state == HospitalPatientAI.CharacterStatus.None)
                    {
                        if (info.RequiresDiagnosis)
                        {
                            SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);
                        }
                        else
                        {
                            SetDiagnosisPanel(DiagnosisPanelMode.NoDiagnosisRequire, info);
                            CureButton.gameObject.SetActive(true);
                        }
                    }
                    else if (infoAI.state == HospitalPatientAI.CharacterStatus.InQueue)
                    {
                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnoseInQueue, info);
                    }
                    else if (infoAI.state == HospitalPatientAI.CharacterStatus.Diagnose)
                    {
                        InDiagnosisState = true;
                        SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisInProgreess, info);
                    }
                    else if (infoAI.state == HospitalPatientAI.CharacterStatus.Healed)
                    {
                        SetDiagnosisPanel(DiagnosisPanelMode.NoDiagnosisRequire, info);
                        CureButton.gameObject.SetActive(true);
                    }
                }
                for (int i = 0; i < TreatmentContent.transform.childCount; ++i)
                {
                    Destroy(TreatmentContent.transform.GetChild(i).gameObject);
                }
                SetHealthy();
                SendButton.onClick.RemoveAllListeners();
                CureButton.onClick.RemoveAllListeners();
                DischargeButton.enabled = true;
                bool cureWithHelp;

                CureBlinkAnim.SetBool("IsBlinking", info.CheckCurePosible(out cureWithHelp));
                SetCureButtonColor(cureWithHelp);

                helpRequestPanelView.SetHelpRequestedText(cureWithHelp);

                CureButton.onClick.AddListener(delegate ()
                {
                    UIController.PlayClickSoundSecure(CureButton.gameObject);
                    CurePatient(info, false);
                    RefreshView(info);
                    UpdateOtherPatients();
                });

                AvatarHead.sprite = info.AvatarHead;
                AvatarBody.sprite = info.AvatarBody;

                if (!info.Name.Contains("_"))
                    Name.text = info.Name + " " + info.Surname;
                else
                    Name.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + info.Surname);

                CoinsForCure.enableVertexGradient = false;
                EXPForCure.enableVertexGradient = false;

                CoinsForCure.color = defaultCoinRewardColor;
                CoinsForCure.outlineColor = Color.black;
                EXPForCure.color = defaultXPRewardColor;
                EXPForCure.outlineColor = Color.black;

                GoldForTreatmentRoom = BalanceableFactory.CreateGoldForTreatmentRoomsBalanceable(info.CoinsForCure);
                ExpForTreatmentRoom = BalanceableFactory.CreateXPForTreatmentRoomsBalanceable(info.EXPForCure);

                CoinsForCure.text = GoldForTreatmentRoom.GetBalancedValue().ToString();
                EXPForCure.text = ExpForTreatmentRoom.GetBalancedValue().ToString();

                //zmiana kolorów (color change)
                bool boosterActive = HospitalAreasMapController.HospitalMap.boosterManager.boosterActive;
                BoosterType boosterType;
                BoosterTarget boosterTarget;

                bool increaseXPForTreatmentRoomsEventActive = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.expForTreatmentRooms_FACTOR);
                bool increaseGoldForTreatmentRoomsEventActive = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.coinsForTreatmentRooms_FACTOR);

                if (boosterActive)
                {
                    boosterType = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterType;
                    boosterTarget = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterTarget;

                    if (boosterTarget == BoosterTarget.PatientCard || boosterTarget == BoosterTarget.AllPatients)
                    {
                        if (boosterType == BoosterType.Coin || boosterType == BoosterType.CoinAndExp || increaseGoldForTreatmentRoomsEventActive)
                            BoosterManager.TintWithBoosterGradient(CoinsForCure);

                        if (boosterType == BoosterType.Exp || boosterType == BoosterType.CoinAndExp || increaseXPForTreatmentRoomsEventActive)
                            BoosterManager.TintWithBoosterGradient(EXPForCure);
                    }
                }
                else
                {
                    if (increaseXPForTreatmentRoomsEventActive)
                        BoosterManager.TintWithBoosterGradient(EXPForCure);

                    if (increaseGoldForTreatmentRoomsEventActive)
                        BoosterManager.TintWithBoosterGradient(CoinsForCure);
                }

                DischargeButton.onClick.RemoveAllListeners();
                DischargeButton.onClick.AddListener(delegate
                {
                    if (!dischargeFirstTapButtonBlock)
                    {
                        if (!VisitingController.Instance.IsVisiting)
                        {
                            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.DiscardPatients));
                            GameState.Get().PatientsCount.AddPatientsDischarged();

                            AnalyticsController.instance.ReportBedPatient(AnalyticsPatientAction.BedDismissed, (int)(info.EXPForCure * GameState.Get().HospitalBoosters.xpPatientCard), (int)(info.CoinsForCure * GameState.Get().HospitalBoosters.coinsPatientCard), false, info);
                        }
                        if (info != null)
                            info.patientCardStatus = HospitalCharacterInfo.PatientCardInfoStatus.Discharged;

                        Discharge(info, isCured: false, delay: 0.1f);
                        RefreshView(info);
                        UpdateOtherPatients();
                    }
                    else
                    {
                        MessageController.instance.ShowMessage(44);
                        dischargeFirstTapButtonBlock = false;
                        Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
                        Timing.RunCoroutine(ResetDischargeFirstTapButtonBlockAfterTime());
                    }
                });

                int whichToBeDiagnosed = -1;
                int diagnoseType = -1;

                /*if (info.IsVIP) {
                    //VIPPersonController infoAI
                } else {
                    HospitalPatientAI infoAI = info.GetComponent<HospitalPatientAI> ();
                    if (infoAI.state == HospitalPatientAI.CharacterStatus.Diagnose) {
                        InDiagnosisState = true;
                    }
                }*/
                Dictionary<MedicineRef, int> requestedMeds = ReferenceHolder.GetHospital().treatmentRoomHelpController.GetHelpedMedicinesForPatient(info);

                //Debug.Log("Required Medicines: " + info.requiredMedicines.Length + " requestedMeds: " + requestedMeds.Count + ". " + info.GetMissingMedicines().Count + " missing meds.");
                for (int i = 0; i < info.requiredMedicines.Length; ++i)
                {
                    switch ((int)info.requiredMedicines[i].Key.Disease.DiseaseType)
                    {
                        case (int)DiseaseType.Head:
                            Silhouette[0].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                            Silhouette[0].enabled = true;

                            SetSilhouetteImageDelegate(Silhouette[0], info, i);
                            AddTreatment(info, info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                            break;
                        case (int)DiseaseType.None:
                            AddTreatment(info, info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                            break;
                        case (int)DiseaseType.Brain:
                            whichToBeDiagnosed = i;
                            if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            {
                                Silhouette[19].color = new Color(255, 255, 255, 1);
                                Silhouette[19].enabled = true;

                                //DiagnosisMachine.sprite = DiagnosticMachines[2];                                
                                SetStateArea(info, HospitalDataHolder.Instance.MRIRoomList.Count > 0);

                                if (info.RequiresDiagnosis && !InDiagnosisState && !InQueue.activeSelf)
                                    SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);

                                diagnoseType = (int)DiseaseType.Brain;

                                Silhouette[19].GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/BRAIN"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                                });
                            }
                            else
                            {
                                Silhouette[1].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[1].color = new Color(255, 255, 255, 1);
                                Silhouette[1].enabled = true;

                                SetSilhouetteImageDelegate(Silhouette[1], info, i);
                            }
                            break;
                        case (int)DiseaseType.Bone:
                            whichToBeDiagnosed = i;
                            if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            {
                                Silhouette[20].color = new Color(255, 255, 255, 1);
                                Silhouette[20].enabled = true;

                                //DiagnosisMachine.sprite = DiagnosticMachines[1];                                
                                SetStateArea(info, HospitalDataHolder.Instance.XRayRoomList.Count > 0);

                                if (info.RequiresDiagnosis && !InDiagnosisState && !InQueue.activeSelf)
                                    SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);

                                diagnoseType = (int)DiseaseType.Bone;

                                Silhouette[20].GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/BONES"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                                });
                            }
                            else
                            {
                                Silhouette[9].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[9].color = new Color(255, 255, 255, 1);
                                Silhouette[9].enabled = true;

                                SetSilhouetteImageDelegate(Silhouette[9], info, i);
                            }
                            break;
                        case (int)DiseaseType.Ear:
                            whichToBeDiagnosed = i;
                            if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            {
                                // need graphics
                                Silhouette[23].color = new Color(255, 255, 255, 1);
                                Silhouette[24].color = new Color(255, 255, 255, 1);
                                Silhouette[23].enabled = true;
                                Silhouette[24].enabled = true;

                                //.sprite = DiagnosticMachines[3];                                
                                SetStateArea(info, HospitalDataHolder.Instance.UltrasoundRoomList.Count > 0);

                                if (info.RequiresDiagnosis && !InDiagnosisState && !InQueue.activeSelf)
                                    SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);

                                diagnoseType = (int)DiseaseType.Ear;

                                Silhouette[23].GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/EARS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                                });
                                Silhouette[24].GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/EARS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                                });
                            }
                            else
                            {
                                Silhouette[11].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[12].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[11].color = new Color(255, 255, 255, 1);
                                Silhouette[12].color = new Color(255, 255, 255, 1);
                                Silhouette[11].enabled = true;
                                Silhouette[12].enabled = true;

                                SetSilhouetteImageDelegate(Silhouette[11], info, i);
                                SetSilhouetteImageDelegate(Silhouette[12], info, i);
                            }
                            break;
                        case (int)DiseaseType.Eye:

                            Silhouette[13].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                            Silhouette[14].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                            Silhouette[13].color = new Color(255, 255, 255, 1);
                            Silhouette[14].color = new Color(255, 255, 255, 1);
                            Silhouette[13].enabled = true;
                            Silhouette[14].enabled = true;

                            SetSilhouetteImageDelegate(Silhouette[13], info, i);
                            SetSilhouetteImageDelegate(Silhouette[14], info, i);

                            AddTreatment(info, info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                            break;
                        case (int)DiseaseType.Lungs:
                            whichToBeDiagnosed = i;
                            if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            {
                                Silhouette[21].color = new Color(255, 255, 255, 1);
                                Silhouette[21].enabled = true;
                                //DiagnosisMachine.sprite = DiagnosticMachines[0];

                                SetStateArea(info, HospitalDataHolder.Instance.LungTestingRoomList.Count > 0);

                                if (info.RequiresDiagnosis && !InDiagnosisState && !InQueue.activeSelf)
                                    SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);

                                diagnoseType = (int)DiseaseType.Lungs;

                                Silhouette[21].GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/LUNGS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                                });
                            }
                            else
                            {
                                Silhouette[15].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[16].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[15].color = new Color(255, 255, 255, 1);
                                Silhouette[16].color = new Color(255, 255, 255, 1);
                                Silhouette[15].enabled = true;
                                Silhouette[16].enabled = true;

                                SetSilhouetteImageDelegate(Silhouette[15], info, i);
                                SetSilhouetteImageDelegate(Silhouette[16], info, i);
                            }
                            break;
                        case (int)DiseaseType.Kidneys:
                            whichToBeDiagnosed = i;
                            if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            {
                                Silhouette[22].color = new Color(255, 255, 255, 1);
                                Silhouette[22].enabled = true;
                                //DiagnosisMachine.sprite = DiagnosticMachines[4];

                                SetStateArea(info, HospitalDataHolder.Instance.LaserRoomList.Count > 0);

                                if (info.RequiresDiagnosis && !InDiagnosisState && !InQueue.activeSelf)
                                    SetDiagnosisPanel(DiagnosisPanelMode.DiagnosisRequire, info);

                                diagnoseType = (int)DiseaseType.Kidneys;

                                Silhouette[22].GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SICKNESS/KIDNEYS"), I2.Loc.ScriptLocalization.Get("TOOLTIP_REQUIRES_DIAGNOSIS"));
                                });
                            }
                            else
                            {
                                Silhouette[17].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[18].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                                Silhouette[17].color = new Color(255, 255, 255, 1);
                                Silhouette[18].color = new Color(255, 255, 255, 1);
                                Silhouette[17].enabled = true;
                                Silhouette[18].enabled = true;

                                SetSilhouetteImageDelegate(Silhouette[17], info, i);
                                SetSilhouetteImageDelegate(Silhouette[18], info, i);
                            }
                            break;
                        case (int)DiseaseType.Bacteria:

                            BacteriaModule.SetUpBacteriaData bacteriaData = new BacteriaModule.SetUpBacteriaData(false, info.requiredMedicines[i].Key.GetMedicineRef().id, info.requiredMedicines[i].Key, info.requiredMedicines[i].Key.Disease, info.requiredMedicines[i].Key.GetMedicineRef());
                            microscopeSection.SetupBacteria(bacteriaData);
                            AddTreatment(info, info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                            break;
                        case (int)DiseaseType.VitaminDeficiency:
                            microscopeSection.SetupVitamin();
                            AddTreatment(info, info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                            break;
                        default:
                            Silhouette[(int)info.requiredMedicines[i].Key.Disease.DiseaseType].sprite = info.requiredMedicines[i].Key.Disease.DiseasePic;
                            //Debug.LogWarning("Silhouette[(int)info.RequiredCures[i].Disease.DiseaseType].sprite " + Silhouette[(int)info.RequiredCures[i].Disease.DiseaseType].sprite);

                            SetSilhouetteImageDelegate(Silhouette[(int)info.requiredMedicines[i].Key.Disease.DiseaseType], info, i);

                            Silhouette[(int)info.requiredMedicines[i].Key.Disease.DiseaseType].color = new Color(255, 255, 255, 1);
                            Silhouette[(int)info.requiredMedicines[i].Key.Disease.DiseaseType].enabled = true;
                            AddTreatment(info, info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                            break;
                    }
                }

                // SET REQUIRED DIAGNOSE AS LAST IF IT'S EXIST
                if ((whichToBeDiagnosed != -1))
                {
                    if (diagnoseType != -1)
                        AddDiagnosis((DiseaseType)diagnoseType);
                    else
                        AddTreatment(info, info.requiredMedicines[whichToBeDiagnosed].Key, info.requiredMedicines[whichToBeDiagnosed].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[whichToBeDiagnosed].Key.GetMedicineRef()));
                    //Debug.Log("whichToBeDiagnosed: " + whichToBeDiagnosed + " diagnoseType: " + diagnoseType);
                }

                if (info.RequiresDiagnosis)
                    UpdateDiagnosisMachineName(info);
            }

            if (updateWaitTimeCorountine != null)
            {
                Timing.KillCoroutine(updateWaitTimeCorountine);
                updateWaitTimeCorountine = null;
            }
        }
        else
        {
            SilhouetteArea.SetActive(false);
            CuresArea.SetActive(false);
            PatientInfoArea.SetActive(false);
            WaitInfoArea.SetActive(true);

            if (HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedStatusForID(selectedBedId) == 0 || (info != null && info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged))
            {
                ClockBadge.SetActive(true);
                OnTheWayBadge.SetActive(false);

                WaitInfoArea.transform.GetChild(1).gameObject.SetActive(true);
                WaitInfoArea.transform.GetChild(2).gameObject.SetActive(false);
                //int secs = HospitalAreasMapController.Map.hospitalBedController.GetWaitTimerForBed(bedId);

                if (updateWaitTimeCorountine != null)
                {
                    Timing.KillCoroutine(updateWaitTimeCorountine);
                    updateWaitTimeCorountine = null;
                }
                updateWaitTimeCorountine = Timing.RunCoroutine(CheckBedTimeCorountine(selectedBedId));

                SpeedButton.onClick.RemoveAllListeners();
                SpeedButton.onClick.AddListener(delegate ()
                {
                    int diamondSpeedCost = 3;

                    if (Game.Instance.gameState().GetDiamondAmount() >= diamondSpeedCost)
                    {
                        DiamondTransactionController.Instance.AddDiamondTransaction(diamondSpeedCost, delegate
                        {
                            if (updateWaitTimeCorountine != null)
                            {
                                Timing.KillCoroutine(updateWaitTimeCorountine);
                                updateWaitTimeCorountine = null;
                            }

                            ClockBadge.SetActive(false);
                            OnTheWayBadge.SetActive(true);
                            SetRandomOnTheWayText();

                            WaitInfoArea.transform.GetChild(1).gameObject.SetActive(false);
                            WaitInfoArea.transform.GetChild(2).gameObject.SetActive(true);

                            HospitalAreasMapController.HospitalMap.hospitalBedController.SpeedBedWaitingForID(selectedBedId);

                            //otherList[selectedBedId].SetBadge(HospitalAreasMapController.Map.hospitalBedController.GetBedStatusForID(selectedBedId));
                            otherList[selectedBedId].SetSelected(true);
                            //Debug.LogError("Inform Mikko if you see this. selectedBedId = " + selectedBedId);
                            UpdateOtherPatients();

                            otherPatientsScroll.horizontalNormalizedPosition = 0f;
                            GameState.Get().RemoveDiamonds(diamondSpeedCost, EconomySource.SpeedUpBed);
                        }, this);
                    }
                    else
                    {
                        AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                        UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                    }
                });
            }
            else
            {
                if (updateWaitTimeCorountine != null)
                {
                    Timing.KillCoroutine(updateWaitTimeCorountine);
                    updateWaitTimeCorountine = null;
                }

                currentWaitTimer = 0;
                ClockBadge.SetActive(false);
                OnTheWayBadge.SetActive(true);
                SetRandomOnTheWayText();

                WaitInfoArea.transform.GetChild(1).gameObject.SetActive(false);
                WaitInfoArea.transform.GetChild(2).gameObject.SetActive(true);

                //else otherList[selectedBedId].SetBadge(HospitalAreasMapController.Map.hospitalBedController.GetBedStatusForID(selectedBedId));
            }
        }
        if (currentCharacter != null)
            ApplyModeSettings(currentCharacter);
        UpdateSelectedInPatientList(false);
    }

    public void RemovePatient(HospitalCharacterInfo info, bool isoDestroy = false)
    {
        if ((!info.IsVIP) || isoDestroy)
        {
            int currentInfoIndex = localInfo.IndexOf(info);
            if (currentInfoIndex >= 0)
                localInfo[currentInfoIndex] = null;

            //localInfo.Remove(info);
            OtherPatient temp = otherList.Find(op => op.info == info);
            if (temp == null)
                return;

            ResetBedInfoAtId(otherList.IndexOf(temp));    //mikko: this caused avatar to disapear when closing patient card after curing/dismissing
        }
    }

    public void UpdateOtherPatients()
    {
        if (!gameObject.activeSelf)
            return;

        // full all other list if empty
        if (localInfo.Count < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count)
        {
            for (int i = localInfo.Count; i < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count; i++)
            {
                AddBedInfo(i);
            }
        }

        if ((HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count <= 1) || !TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.cure_bed_patient))
        {
            OtherPatientsContent.SetActive(false);
            //return;
        }
        else
            OtherPatientsContent.SetActive(true);

        if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds != null && HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count > 0)
        {
            for (int i = 0; i < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count; i++)
            {
                if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[i].Patient != null)
                {
                    //Debug.Log(((BasePatientAI)HospitalAreasMapController.Map.hospitalBedController.Beds[i].Patient).GetComponent<HospitalCharacterInfo>().Name);
                    HospitalCharacterInfo tmp = ((BasePatientAI)HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[i].Patient).GetComponent<HospitalCharacterInfo>();
                    UpdateBedInfo(tmp, i);
                }
                else
                    UpdateBedInfo(null, i);
            }
        }

        roomGrid.UpdateGrid();
    }

    void UpdateSelectedInPatientList(bool scrollOtherPatients)
    {
        //Debug.LogError("UpdateSelectedInPatientList: " + selectedBedId);
        if (otherList != null && otherList.Count > 0 && HospitalAreasMapController.HospitalMap.hospitalBedController.Beds != null && HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count > 0)
        {
            int id = 0;
            foreach (var otherPatient in otherList)
            {
                if (id == selectedBedId)
                {
                    if (otherPatient.info != null)
                        otherPatient.info.WasPatientCardSeen = true;
                    otherPatient.SetSelected(true);

                    if (scrollCoroutine != null && scrollOtherPatients)
                    {
                        try
                        {
                            StopCoroutine(scrollCoroutine);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                        }
                    }
                    if (otherList.Count > 8 && gameObject.activeSelf && scrollOtherPatients)
                        scrollCoroutine = StartCoroutine(UpdateOtherPatientsScrollPosition((float)id / (float)(otherList.Count - 1)));

                    otherPatient.SetIndicator();
                }
                else
                {
                    otherPatient.SetSelected(false);
                    otherPatient.SetIndicator();
                }

                if (id >= HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count)
                {
                    otherList.RemoveAt(otherList.Count - 1);
                    localInfo.RemoveAt(otherList.Count - 1);
                    Destroy(OtherPatientsContent.transform.GetChild(OtherPatientsContent.transform.childCount - 1).gameObject);
                }
                else
                {
                    if (otherPatient.info != null)
                    {
                        if (otherPatient.info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured)
                            otherPatient.SetBadge(3);
                        else if (otherPatient.info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
                            otherPatient.SetBadge(0);
                        else
                            otherPatient.SetBadge(HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedStatusForID(id));
                    }
                    else
                        otherPatient.SetBadge(HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedStatusForID(id));
                }
                ++id;
            }
        }
    }

    void UpdateBedInfo(HospitalCharacterInfo info, int id)
    {
        if (id < OtherPatientsContent.transform.childCount)
        {
            if (info != null)
            {
                localInfo[id] = info;

                //Debug.LogError("SetInfo id: " + id);
                otherList[id].SetInfo(info, -1);
                otherList[id].SetButton(delegate ()
                {
                    //Debug.LogError("OnClick id: " + id);
                    if (info == currentCharacter && (info != null && currentCharacter != null))     //second condition is for situations where you have dismissed 2 or more patients and want to switch between their cards. we dont want to return then.
                    {
                        //Debug.LogError("this " + info + " " + currentCharacter);
                        return;
                    }
                    if (id == selectedBedId)
                    {
                        //Debug.LogError("same id returning");
                        return;
                    }
                    Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
                    dischargeFirstTapButtonBlock = true;
                    currentCharacter = info;
                    SetData(info, id, false);
                });
            }
            else
            {
                HospitalBedController hbc = HospitalAreasMapController.HospitalMap.hospitalBedController;
                //Debug.LogError("SetInfo null id: " + id);
                otherList[id].SetInfo(null, hbc.GetBedStatusForID(id), hbc.GetWaitTimerForBed(id));
            }

            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[id].room == null)
                OtherPatientsContent.transform.GetChild(id).gameObject.SetActive(HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState == ExternalRoom.EExternalHouseState.enabled);
        }
    }

    void UpdateDiagnosisMachineName(HospitalCharacterInfo patient)
    {
        string diagnoseMachineName = HospitalDataHolder.Instance.GetDiagnosisMachineName(patient);
        //Debug.LogWarning("UPDATE: " + diagnoseMachineName);

        RequiresDiagnosis.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = diagnoseMachineName;
        InQueue.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = diagnoseMachineName;
        DiagnosisInProgress.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = diagnoseMachineName;
    }

    void Update()
    {
        if (IsVisible)
        {
            HandleSwipe();
            UpdateVipCounter();
            microscopeSection.UpdateBacteriaCounter(currentCharacter);
        }
    }
    #endregion

    #region SetPatientToDiagnose
    public void SetPatientToDiagnose(IDiagnosePatient infoAI)
    {
        HospitalCharacterInfo info = infoAI.GetAI().GetComponent<HospitalCharacterInfo>();

        if (HospitalDataHolder.Instance.ReturnDiseaseQueue((int)info.DisaseDiagnoseType).Count == HospitalDataHolder.Instance.GetQueueSize((int)info.DisaseDiagnoseType))
        {
            UIController.getHospital.QueueDiagnosisPopUp.Open(infoAI, HospitalDataHolder.Instance.GetDiagnosisMachineName(info), DiamondCostCalculator.GetQueueSlotCost(HospitalDataHolder.Instance.GetQueueSize((int)info.DisaseDiagnoseType)));
            return;
        }

        int positiveNeeded = (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.positive_energy_text) ? 0 : HospitalDataHolder.Instance.NeededPositiveEnergy((int)info.DisaseDiagnoseType);
        if (GameState.Get().PositiveEnergyAmount >= positiveNeeded)
            SendHim(infoAI);
        else
        {
            UIController.get.BuyResourcesPopUp.Open(positiveNeeded - GameState.Get().PositiveEnergyAmount, () =>
            {
                GameState.Get().RemoveResources(ResourceType.PositiveEnergy, HospitalDataHolder.Instance.NeededPositiveEnergy((int)info.DisaseDiagnoseType), EconomySource.Diagnose);
                SendHim(infoAI);
            }, null, BuyResourcesPopUp.missingResourceType.positive);
            return;
        }
        GameState.Get().RemoveResources(ResourceType.PositiveEnergy, positiveNeeded, EconomySource.Diagnose);
    }

    public void SetCurrentPatientToDiagnose()
    {
        IDiagnosePatient info;
        if (currentCharacter.IsVIP)
        {
            info = currentCharacter.GetComponent<VIPPersonController>();
            Debug.LogWarning("Set current to diagnose: " + ((VIPPersonController)info).name);
        }
        else
        {
            info = currentCharacter.GetComponent<HospitalPatientAI>();
            Debug.LogWarning("Set current to diagnose: " + ((HospitalPatientAI)info).name);
        }

        SetPatientToDiagnose(info);
    }

    void SendHim(IDiagnosePatient infoAI)
    {
        HospitalCharacterInfo info = infoAI.GetAI().GetComponent<HospitalCharacterInfo>();

        HospitalDataHolder.Instance.AddLastToDiagnosticQueue(infoAI);
        if ((HospitalDataHolder.Instance.ReturnPatientQueue(infoAI) != null))
        {
            //infoAI.SetStateDiagnose ();
            infoAI.SetStateInQueue();
            SetDiagnosisPanel(DiagnosisPanelMode.DiagnoseInQueue, info);
            //info.RequiresDiagnosis = false;
            // Level Goals
            NotificationCenter.Instance.PatientSentToXRay.Invoke(new BaseNotificationEventArgs());
            SaveSynchronizer.Instance.MarkToSave(SavePriorities.DiagnoseQueued);
            ShowPatientDiagnosticRoom(infoAI, true);
        }
    }
    #endregion

    #region ShowPatientDiagnosticRoom
    public void ShowCurrentPatientDiagnosticRoom()
    {
        if (currentCharacter)
        {
            IDiagnosePatient currentPatient = currentCharacter.GetComponent<IDiagnosePatient>();

            Exit();

            var room = HospitalDataHolder.Instance.ReturnPatientDiagnosisRoom(currentPatient);
            if (room != null)
            {
                Vector2i pos = room.position;// room.GetEntrancePosition();
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(pos.x + 3, 0, pos.y + 3), 1f, TutorialController.Instance.GetCurrentStepData().CameraLocked);
                //room.ShowDiagnosisHoover();
            }
        }
        else
            Debug.LogError("else");
    }

    void ShowPatientDiagnosticRoom(IDiagnosePatient patient, bool newPatient = false)
    {
        if (patient != null)
        {
            Exit();

            var room = HospitalDataHolder.Instance.ReturnPatientDiagnosisRoom(patient);
            if (room != null)
            {
                Vector2i pos = room.position;// room.GetEntrancePosition();
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(pos.x + 3, 0, pos.y + 3), 1f, TutorialController.Instance.GetCurrentStepData().CameraLocked);
                room.ShowDiagnosisHoover(newPatient);
                //room.ShowPositiveEnergyUsed ();
            }
        }
    }
    #endregion

    #region Tutorial
    void UpdateSecondPatientForTutorial()
    {
        if (currentMode != Mode.CanDoEverything)
        {
            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count > 1)
            {
                bottomTransform.gameObject.SetActive(false);
                OtherPatientsContent.transform.GetChild(0).gameObject.SetActive(false);
                OtherPatientsContent.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        else
        {
            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count > 1)
            {
                bottomTransform.gameObject.SetActive(true);
                OtherPatientsContent.transform.GetChild(0).gameObject.SetActive(true);
                OtherPatientsContent.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
    #endregion

    #region Coroutines
    IEnumerator<float> DelayedGoHome(HospitalCharacterInfo info, bool isCured, float delay)
    {
        float patientDelay = isCured ? 0f : delay;  // This is passed to the patient AI
        if (info.IsVIP)
        {
            yield return Timing.WaitForSeconds(delay);
            info.GetComponent<Hospital.VIPPersonController>().DepartVIP(patientDelay);
        }
        else
        {
            var patientAI = info.GetComponent<Hospital.HospitalPatientAI>();
            NotificationCenter.Instance.PatientCuredInPatientCard.Invoke(new PatientCuredInPatientCardEventArgs(patientAI));
            yield return Timing.WaitForSeconds(delay);
            info.GetComponent<Hospital.HospitalPatientAI>().GoChange(patientDelay);
        }
    }

    IEnumerator<float> PatientHealedFanfare(Transform target, Transform fanfareT, bool isVIP = false)
    {
        yield return Timing.WaitForSeconds(3);
        if (fanfareT != null && fanfareT.GetChild(0) != null)
            fanfareT.GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator<float> VIPDelayedGift()
    {
        VisitingController.Instance.canVisit = false;
        vipGiftPending = true;
        yield return Timing.WaitForSeconds(3);
        vipGiftPending = false;
        ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromVIP = true;
        UIController.getHospital.unboxingPopUp.OpenVIPCasePopup();
    }

    IEnumerator<float> ResetDischargeFirstTapButtonBlockAfterTime()
    {
        yield return Timing.WaitForSeconds(2f);
        dischargeFirstTapButtonBlock = true;
    }

    IEnumerator<float> SwitchCardsCoroutine(HospitalCharacterInfo info, int bedID, bool instant, bool animRight = false)
    {
        //if (info != null && info == currentCharacter)
        //    yield break;

        SoundsController.Instance.PlayPatientCardOpen();
        if (currentCharacter != null && !currentCharacter.WasPatientCardSeen)
            currentCharacter.WasPatientCardSeen = true;

        BlinkInfoButton(true);

        selectedBedId = bedID;

        if (!instant)
        {
            if (animRight)
            {
                SwitchCardAnimator.ResetTrigger("SwitchLeft");
                SwitchCardAnimator.SetTrigger("SwitchRight");
            }
            else
            {
                SwitchCardAnimator.ResetTrigger("SwitchRight");
                SwitchCardAnimator.SetTrigger("SwitchLeft");
            }

            yield return Timing.WaitForSeconds(1f / 3f);
        }

        UpdateOtherPatients();
        UpdateSelectedInPatientList(true);

        RefreshView(info);
        if (TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepIndex < 54)
            UpdateSecondPatientForTutorial();
    }

    IEnumerator UpdateOtherPatientsScrollPosition(float pos)
    {
        //Debug.LogError("Setting norm pos to = " + pos);

        float timeToAdjust = .4f;
        float timer = 0f;
        float startPos = otherPatientsScroll.horizontalNormalizedPosition;
        //yield return new WaitForSeconds(1f);
        while (timer < timeToAdjust)
        {
            timer += Time.deltaTime;
            otherPatientsScroll.horizontalNormalizedPosition = Mathf.Lerp(startPos, pos, timer / timeToAdjust);
            yield return null;
        }
        //Debug.LogError("horizontalNormalizedPosition = " + otherPatientsScroll.horizontalNormalizedPosition);
        scrollCoroutine = null;
    }

    IEnumerator<float> CheckBedTimeCorountine(int bedId)
    {
        while (true)
        {
            try
            {
                currentWaitTimer = HospitalAreasMapController.HospitalMap.hospitalBedController.GetWaitTimerForBed(bedId);

                if (currentWaitTimer >= 1)
                    SpawnTimer.text = UIController.GetFormattedShortTime(currentWaitTimer);
                else
                {
                    //Debug.LogError("UPDATE BED CURRENT TIME");
                    ClockBadge.SetActive(false);
                    OnTheWayBadge.SetActive(true);
                    SetRandomOnTheWayText();

                    WaitInfoArea.transform.GetChild(1).gameObject.SetActive(false);
                    WaitInfoArea.transform.GetChild(2).gameObject.SetActive(true);

                    otherList[bedId].SetBadge(HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedStatusForID(bedId));
                    break;
                }
            }
            catch (System.Exception) { }
            yield return Timing.WaitForSeconds(1);
        }
    }
    #endregion

    #region InfoArea
    public void SetInfoHoverData()
    {
        InfoTextSex.text = CharacterCreator.GetSexString(currentCharacter.Sex);
        InfoTextAge.text = currentCharacter.Age.ToString();
        InfoTextBlood.text = CharacterCreator.GetBloodTypeString(currentCharacter.BloodType);
        InfoTextOther.text = string.Format(I2.Loc.ScriptLocalization.Get("LIKES_DISLIKES"), currentCharacter.GetLikesString(), currentCharacter.GetDislikesString());
        if (currentCharacter.IsVIP)
        {
            //Debug.Log("Setting VIP description: " + I2.Loc.ScriptLocalization.Get("VIP_BIOS/VIP_" + currentCharacter.VIPDescription));
            InfoTextVIP.text = I2.Loc.ScriptLocalization.Get("VIP_BIOS/VIP_" + currentCharacter.VIPDescription);
        }

        infoHoverDataSet = true;
    }

    public void InfoButtonDown()
    {
        if (currentCharacter != null)
        {
            if (!infoHoverDataSet)
                SetInfoHoverData();

            InfoHoverAnim.SetBool("Show", true);
            if (currentCharacter != null && currentCharacter.IsVIP)
                InfoHoverVIPAnim.SetBool("Show", true);
            BlinkInfoButton(false);
            SoundsController.Instance.PlayInfoButton();

            if (currentCharacter.IsVIP)
            {
                if (currentCharacter.WasPatientInfoSeen == false && ReferenceHolder.GetHospital().dailyQuestController.CurrentDailyHasTaskTypeWithoutCompletion(DailyTask.DailyTaskType.WhoAreTheVips))
                {
                    currentCharacter.WasPatientInfoSeen = true;
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.WhoAreTheVips));
                }
            }
            else
            {
                if (currentCharacter.WasPatientInfoSeen == false && ReferenceHolder.GetHospital().dailyQuestController.CurrentDailyHasTaskTypeWithoutCompletion(DailyTask.DailyTaskType.PatientLikesAndDislikes))
                {
                    currentCharacter.WasPatientInfoSeen = true;
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.PatientLikesAndDislikes));
                }
            }
        }
        else InfoHoverVIPAnim.SetBool("Show", false);
    }

    public void InfoButtonUp()
    {
        InfoHoverAnim.SetBool("Show", false);
        if (currentCharacter != null && currentCharacter.IsVIP)
            InfoHoverVIPAnim.SetBool("Show", false);
    }

    void BlinkInfoButton(bool isBlinking)
    {
        InfoButtonAnim.SetBool("IsBlinking", isBlinking);
    }
    #endregion

    #region Bacteria
    public CanvasGroup bacteriaTutorialMask;
    public GameObject bacteriaTutorialButton;
    public GameObject bacteriaTutorialArrow;
    public Transform bottomTransform;
    public CanvasGroup bacteriaTutorialMaskGroup;

    [TutorialTriggerable]
    public void ShowBacteriaTutorialMask()
    {

        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        bacteriaTutorialMask.gameObject.SetActive(true);
        bacteriaTutorialButton.SetActive(true);
        bottomTransform.SetAsFirstSibling();
        StartCoroutine(BacteriaMaskFadeIn());
    }

    [TutorialTriggerable]
    public void HideBacteriaTutorialMask()
    {
        Timing.RunCoroutine(BacteriaMaskFadeOut());
        bacteriaTutorialButton.SetActive(false);
        microscopeSection.InfoButtonUp();
    }

    [TutorialTriggerable]
    public void ShowBacteriaTutorialArrow()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        bacteriaTutorialArrow.SetActive(true);
    }

    [TutorialTriggerable]
    public void HideBacteriaTutorialArrow()
    {
        bacteriaTutorialArrow.SetActive(false);
    }

    [TutorialTriggerable]
    public void ShowBacteriaTutorialInfo()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;
        microscopeSection.ShowBacterialTutorialInfo();
    }

    [TutorialTriggerable]
    public void HideBacteriaTutorialInfo()
    {
        microscopeSection.HideBacteriaTutorialInfo();
    }

    IEnumerator BacteriaMaskFadeIn()
    {
        bacteriaTutorialMask.gameObject.SetActive(true);

        float fadeTime = 2f;
        float fadeTimer = 0f;

        bacteriaTutorialMaskGroup.blocksRaycasts = true;
        bacteriaTutorialMaskGroup.interactable = true;
        while (fadeTimer <= fadeTime)
        {
            fadeTimer += Time.deltaTime;
            bacteriaTutorialMaskGroup.alpha = Mathf.Lerp(0, .8f, fadeTime / fadeTimer);
            yield return null;
        }
    }

    IEnumerator<float> BacteriaMaskFadeOut()
    {
        float fadeTime = .5f;
        float fadeTimer = 0f;

        bacteriaTutorialMaskGroup.blocksRaycasts = false;
        bacteriaTutorialMaskGroup.interactable = false;
        while (fadeTimer <= fadeTime)
        {
            fadeTimer += Time.deltaTime;
            bacteriaTutorialMaskGroup.alpha = Mathf.Lerp(.8f, 0, fadeTime / fadeTimer);
            yield return 0.02f;
        }
        bacteriaTutorialMask.gameObject.SetActive(false);
        bottomTransform.SetAsLastSibling();
    }
    #endregion

    #region Other
    /// <summary>
    /// To implement!!!
    /// </summary>
    /// <returns>Amount of cures provided by helpers</returns>
    int GetHelpedCureAmount(Dictionary<MedicineRef, int> requestedMeds, MedicineRef medToFind)
    {
        if (requestedMeds != null && requestedMeds.Count > 0)
        {
            if (requestedMeds.TryGetValue(medToFind, out int val))
                return val;
        }

        return 0;
    }

    public Sprite GetDiagnosisSprite(DiseaseType diseaseType)
    {
        switch (diseaseType)
        {
            case DiseaseType.Brain:
                return DiagnosisSmall[1];
            case DiseaseType.Bone:
                return DiagnosisSmall[0];
            case DiseaseType.Ear:
                return DiagnosisSmall[2];
            case DiseaseType.Lungs:
                return DiagnosisSmall[3];
            case DiseaseType.Kidneys:
                return DiagnosisSmall[4];
            default:
                Debug.LogError("Incorrect diagnosis type");
                return null;
        }
    }

    public Sprite GetDiagnosisSprite(HospitalDataHolder.DiagRoomType TypeOfDiagRoom)
    {
        switch (TypeOfDiagRoom)
        {
            case HospitalDataHolder.DiagRoomType.MRI:
                return DiagnosisSmall[1];
            case HospitalDataHolder.DiagRoomType.XRay:
                return DiagnosisSmall[0];
            case HospitalDataHolder.DiagRoomType.UltraSound:
                return DiagnosisSmall[2];
            case HospitalDataHolder.DiagRoomType.LungTesting:
                return DiagnosisSmall[3];
            case HospitalDataHolder.DiagRoomType.Laser:
                return DiagnosisSmall[4];
            default:
                return null;
        }
    }

    void HandleSwipe()
    {
        //do not start swiping if finger is on the bottom of the screen (other patients)
        if (Input.mousePosition.y < Screen.height * .28f)
            return;

        //do not swipe when other pop up is active
        if (UIController.get.BuyResourcesPopUp.isActiveAndEnabled
            || UIController.getHospital.QueueDiagnosisPopUp.isActiveAndEnabled
            || UIController.get.IAPShopUI.isActiveAndEnabled
            || bacteriaTutorialMask.alpha > 0f)
            return;

        //set last mouse/touch position when starting touch or when patient card is open
        if (Input.GetMouseButtonDown(0) || lastPointerPos == -99999)
            lastPointerPos = Input.mousePosition.x;

        if (Input.GetMouseButtonUp(0))
        {
            //on before level 4 there's only one patient. Do not swipe.
            //if (Game.Instance.gameState().GetHospitalLevel() < 4)
            //    return;

            //1 or 0 patients, no reason to swipe.
            if (OtherPatientsContent.transform.childCount < 2)
                return;

            //detect swipe only if Olivia was healed
            if (TutorialSystem.TutorialController.CurrentStep.StepTag >= StepTag.level_4)
            {
                //detect swipe right
                if (Input.mousePosition.x - (Screen.width * swipeDetectionThreshold) > lastPointerPos)
                {
                    //Debug.LogError("Swipe right detected. SelectedBedID: " + selectedBedId);
                    --selectedBedId;
                    if (selectedBedId < 0)
                        selectedBedId = OtherPatientsContent.transform.childCount - 1;

                    //Debug.Log("otherList[selectedBedId].IsVIP() " + otherList[selectedBedId].IsVIP() + "!otherList[selectedBedId].gameObject.activeSelf " + !otherList[selectedBedId].gameObject.activeSelf);
                    if (!otherList[selectedBedId].gameObject.activeSelf)
                    { //vip portrait not shown, skip this card;
                        --selectedBedId;
                        if (selectedBedId < 0)
                            selectedBedId = OtherPatientsContent.transform.childCount - 1;
                    }
                    currentCharacter = otherList[selectedBedId].info;
                    SetData(otherList[selectedBedId].info, selectedBedId, false, true);

                    if (ReferenceHolder.GetHospital().dailyQuestController.CurrentDailyHasTaskTypeWithoutCompletion(DailyTask.DailyTaskType.PatientCardSwoosh))
                    {
                        // if (currentCharacter.WasPatientCardSwipe == false)
                        // {
                        //     currentCharacter.WasPatientCardSwipe = true;
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.PatientCardSwoosh));
                        // }
                    }

                    Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
                    dischargeFirstTapButtonBlock = true;
                }

                //detect swipe left
                else if (Input.mousePosition.x + (Screen.width * swipeDetectionThreshold) < lastPointerPos)
                {
                    //Debug.LogError("Swipe left detected. SelectedBedID: " + selectedBedId);

                    ++selectedBedId;
                    if (selectedBedId >= OtherPatientsContent.transform.childCount)
                        selectedBedId = 0;

                    //Debug.Log("otherList[selectedBedId].IsVIP() " + otherList[selectedBedId].IsVIP() + "!otherList[selectedBedId].gameObject.activeSelf " + !otherList[selectedBedId].gameObject.activeSelf);
                    if (!otherList[selectedBedId].gameObject.activeSelf) //vip portrait not shown, skip this card;
                    {
                        ++selectedBedId;
                        if (selectedBedId >= OtherPatientsContent.transform.childCount)
                            selectedBedId = 0;
                    }
                    currentCharacter = otherList[selectedBedId].info;
                    SetData(otherList[selectedBedId].info, selectedBedId, false, false);

                    if (ReferenceHolder.GetHospital().dailyQuestController.CurrentDailyHasTaskTypeWithoutCompletion(DailyTask.DailyTaskType.PatientCardSwoosh))
                    {
                        // if (currentCharacter.WasPatientCardSwipe == false)
                        // {
                        //     currentCharacter.WasPatientCardSwipe = true;
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.PatientCardSwoosh));
                        // }
                    }

                    Timing.KillCoroutine(ResetDischargeFirstTapButtonBlockAfterTime().GetType());
                    dischargeFirstTapButtonBlock = true;
                }
            }
        }
    }
    #endregion
}

public enum DiagnosisPanelMode
{
    DiagnosisRequire,
    DiagnosisInProgreess,
    NoDiagnosisRequire,
    DiagnoseInQueue
}
