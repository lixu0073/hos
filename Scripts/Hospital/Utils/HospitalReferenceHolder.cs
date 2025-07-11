using UnityEngine;
using Hospital;
using Hospital.TreatmentRoomHelpRequest;

public class HospitalReferenceHolder : Base_ReferenceHolder
{

    #region systems
    [Header("Systems")]

    #endregion

    #region HospitalSystems
    [Header("Hospital Systems")]
    public ClinicAISpawner ClinicAI;
    public HospitalAISpawner HospitalSpawner;
    public CharacterCreator PersonCreator;
    public VIPSpawner vipSpawner;
    public HospitalNameSign HospitalNameSign;
    public HospitalAmbulance Ambulance;
    public MaternityStatusController MaternityStatusController;
    public DailyRewardController DailyRewardController;
    public RecompensationGiftsRecieverController RecomensationGiftController;
    public VIPSystemManager vipSystemManager;
    #endregion

    #region SpecialObjects
    [Header("SpecialObjects and Other")]

    public Epidemy Epidemy;
    public Pharmacy Pharmacy;
    public Reception Reception;
    public Plantation plantation;
    public BillboardAd BillboardAd;
    public GameObject plusOnePanacea;
    public BubbleBoyEntryOverlayController bubbleBoyEntryOverlayController;
    public BubbleBoyCharacterAI bubbleBoyCharacterAI;
    public DailyQuestController dailyQuestController;
    public DailyDealController dailyDealController;
    public GlobalEventController globalEventController;
    public AnimalController animalsController;
    public Camping camping;
    public GiftRewardGeneratorParser giftRewardGeneratorParser;
    public TreatmentRoomHelpController treatmentRoomHelpController;
    public TreatmentHelpAPI treatmentHelpAPI;
    public TreatmentHelpNotificationCenter treatmentHelpNotificationCenter;
    public TreatmentRoomHelpProviderController treatmentRoomHelpProviderController;
    public BubbleBoyMinigameController bubbleBoyMinigameController;
    public IFlagControllable flagControllable;
    public ISignControllable signControllable;
    public DrWiseCardController drWiseCardController;


    public override void Initialize()
    {
        base.Initialize();
        flagControllable = customizationControllerHolder.GetComponent<IFlagControllable>();
        signControllable = customizationControllerHolder.GetComponent<ISignControllable>();
    }
    #endregion

}
