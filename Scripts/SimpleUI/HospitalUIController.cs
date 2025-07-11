using UnityEngine;
using System.Collections.Generic;
using Hospital;
using Hospital.LootBox;


public class HospitalUIController : BaseUIController
{
    public static HospitalUIController get;

    [Header("PopUps")]
    public ChildrenPopUpController ChildrenPopUp;
    public PharmacyPopUpController PharmacyPopUp;
    public GlobalOffersPopUp GlobalOffersPopUp;
    public CreateOfferPopUp CreateOfferPopUp;
    public ModifyOfferPopUp ModifyOfferPopUp;
    public QueuePopUpController QueueDiagnosisPopUp;
    public StoragePopup StoragePopUp;
    public PanaceaPopUp PanaceaPopUp;
    public PanaceaUpgradePopUp PanaceaUpgradePopUp;
    public PatientCardController PatientCard;
    public MainPatientCardPopUpController MainPatientCardPopUpController;
    public AchievementPopUpController AchievementsPopUp;
    public AchievementInfoPopUpController AchievementsInfoPopUp;

    public EpidemyOffPopUpController EpidemyOffPopUp;
    public EpidemyOnPopUpController EpidemyOnPopUp;
    public PatientZeroPopUp PatientZeroPopUp;
    public LockedFeatureArtPopUpController LockedFeatureArtPopUpController;
    public BoosterInfoPopUpController BoosterInfoPopUp;
    public BoosterMenuPopUpController BoosterMenuPopUp;
    public BoosterActivatedPopupController BoosterStartsPopup;
    public CasesPopUpController casesPopUpController;
    public VIPPopUp vIPPopUp;
    public ConnectFBPopupController connectFBPopup;
    public WelcomePopupController welcomePopupController;
    public BubbleBoyEntryOverlayUI bubbleBoyEntryOverlayUI;
    public BubbleBoyMinigameUI bubbleBoyMinigameUI;
    public DailyQuestPopUpUI DailyQuestPopUpUI;
    public DailyQuestWeeklyUI DailyQuestWeeklyUI;
    public ReplaceDailyTaskPopup ReplaceDailyTaskPopup;
    public EventPopUp EventPopUp;
    public HospitalSignPopupController hospitalSignPopup;
    public HospitalNameTab hospitalNameTab;
    public DailyDealConfirmationUI dailyDealConfirmationPopup;
    public EventGoalReached EventGoalReached;
    public TreatmentDonateUI treatmentDonatePopup;
    public TreatmentHelpSummaryUI treatmentHelpSummaryPopup;
    public TreatmentSendPushesUI treatmentSendPushesPopup;
    public BuyLootBoxPopup buyLootBoxPopup;
    public ObjectivesPanelUI ObjectivesPanelUI;
    public NextLevelPopUp NextLevelPopUp;
    public StorageFullPopUp StorageFullPopUp;
    public StorageUpgradePopUp StorageUpgradePopUp;

    public MailboxPopupController mailboxPopup;
    public FriendManagementView friendManagementView;
    public VitaminsMakerUpgradeConfirmPopUp vitaminsMakerUpgradeConfirmPopup;
    public VitamineCollectorInfoPopup vitaminesCollectorInfoPopup;
    public VitaminsMakerUpgradePopup vitaminsMakerUpgradePopup;
    public InfoPopUp HospitalInfoPopUp;
    public AddFriendsPopupController addFriendsPopupController;
    public PassFriendCodeController passFriendCodeController;
    public FriendAddingResultController friendAddingResult;
    public GiftsReceivePopupUI giftUI;
    public MaternityStatusPopup MaternityStatusPopup;
    public UIElementTabController DailyQuestAndDailyRewardUITabController;
    public UpgradeVIPPopupController UpgradeVIPPopup;
    public EventCenterPopupController EventCenterPopup;
    public EventEndedPopupController EventEndedPopup;
    public TimedOffersScreenController TimerOffersScreen;

    [Header("Hovers")]      //hovers are instantiated from prefabs.
    public GameObject doctorHoverPrefab;
    public GameObject diagnosticHoverPrefab;
    public GameObject probeTableHoverPrefab;
    public GameObject plantationPatchHoverPrefab;

    public AchievementIndicatorController achievementIndicator;
    public GameObject EpidemyObjectHoverPrefab;
    public GameObject MaternityWardObjectHoverPrefab;

    [Header("Other")]
    public GameObject BoosterButton;

    public DailyQuestMainButtonUI DailyQuestMainButtonUI;
    public ParticleSystem DailyTaskUpdatedParticle;

    [TutorialTriggerable]
    public void UnlockBoosterButton()
    {
        SetBoosterButton();
    }
    [TutorialTriggerable]
    public void UnlockBoxButton()
    {
        SetBoxButton();
    }
    [TutorialTriggerable]
    public void UnlockDailyQuest()
    {
        SetDailyQuestsButton();
        DailyQuestMainButtonUI.Refresh();
    }
    [TutorialTriggerable] public void BlockFade(bool isBlocked) { FadeBlocked = isBlocked; }

    void Awake()
    {
        get = this;
        AchievementsPopUp.StartAchievements();
        PoPUpArtsFromResources = new Dictionary<string, GameObject>();
    }
    public void Start()
    {
        InitializeButtons();
    }
    public void InitializeButtons()
    {
        SetBoosterAndBoxButtons();
        SetDailyQuestsButton();
        BaseGameState.OnLevelUp -= SetDailyQuestsButton;
        BaseGameState.OnLevelUp += SetDailyQuestsButton;
    }

    private void SetBoosterButton()
    {
        if (TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.NL_lvl_10_wise_2, true))
        {
            BoosterButton.SetActive(true);
            BaseGameState.OnLevelUp -= SetBoosterButton;
        }

        else if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            BaseGameState.OnLevelUp -= SetBoosterButton;
            BaseGameState.OnLevelUp += SetBoosterButton;
        }

    }
    private void SetBoxButton()
    {
        if (TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.package_collected, true))
        {
            BoxButton.SetActive(true);
            BaseGameState.OnLevelUp -= SetBoxButton;
        }
        else if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            BaseGameState.OnLevelUp -= SetBoxButton;
            BaseGameState.OnLevelUp += SetBoxButton;
        }
    }
    public override void SetBoosterAndBoxButtons()
    {
        BoosterButton.SetActive(false);
        BoxButton.SetActive(false);
    }

    public override void SetStandardUI()
    {
        base.SetStandardUI();
        SetObjectivesPanelUI();
    }

    public override void SetVisitingUI(string name, int lvl, string userID)
    {
        base.SetVisitingUI(name, lvl, userID);
        SetObjectivesPanelUI();
    }

    public override void SetDailyQuestsButton()
    {
        if (TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.daily_quests_unlocked, true))
        {
            DailyQuestMainButtonUI.gameObject.SetActive(true);
            Debug.Log("Show DailyQuest button!");
            BaseGameState.OnLevelUp -= SetDailyQuestsButton;
        }
        else
        {
            DailyQuestMainButtonUI.gameObject.SetActive(false);
        }
    }

    public bool FadeBlocked { get; private set; }

    public override void FadeClicked()
    {
        if (FadeBlocked)
            return;

        int count = ActivePopUps.Count;
        if (count == 0)
            return;
        if (ActivePopUps.Contains(alertPopUp))
            return;
        if (ActivePopUps.Contains(RatePopUp))
            return;
        if (ActivePopUps.Contains(EventEndedPopup.GetPopup()))
            return;
        if (ActivePopUps.Contains(TimerOffersScreen.GetPopup()))
            return;

        //AnalyticsController.instance.ReportButtonClick("Fade", ActivePopUps[count - 1].name);
        ActivePopUps[count - 1].Exit();
        Fade.UpdateFadePosition(this.transform.GetSiblingIndex());

    }

    public void ButtonXPBar()
    {
        if (!NextLevelPopUp.gameObject.activeSelf)
            StartCoroutine(NextLevelPopUp.Open());
        else
            NextLevelPopUp.Exit();
    }

    public void SetObjectivesPanelUI()
    {
        if (AreaMapController.Map.VisitingMode || Game.Instance.gameState().GetHospitalLevel() <= 2 || CampaignConfig.hintSystemEnabled || !ReferenceHolder.Get().objectiveController.ObjectivesSet)
            ObjectivesPanelUI.Hide();
        else
            ObjectivesPanelUI.Show();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BaseGameState.OnLevelUp -= SetDailyQuestsButton;
        BaseGameState.OnLevelUp -= SetBoxButton;
        BaseGameState.OnLevelUp -= SetBoosterButton;
        AchievementsPopUp.Destroy();
    }

}
