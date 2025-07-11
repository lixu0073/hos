/// <summary>
/// 分析字符串常量和枚举定义，包含游戏分析系统使用的各种常量和枚举类型。
/// 定义了奖励类型、分析漏斗、经济行为、社交行为等分析相关的数据结构。
/// </summary>
public static class AnalyticsStrings
{

    const string GCM = "665990624524";


    public static readonly string RewardCoin = "coin";
    public static readonly string RewardDiamond = "diamond";
    public static readonly string RewardAdBillboard = "reward_ad_billboard";
    public static readonly string RewardAdBillboardDiamond = "reward_ad_billboard_diamond";
    public static readonly string RewardAdBubbleBoy = "reward_ad_bubbleboy";
    public static readonly string RewardAdDailyQuest = "reward_ad_dailyquest";
    public static readonly string RewardAdCoins = "reward_ad_coins";

}


public enum AnalyticsFunnel
{
    IAPMissingResources,
    IAPMissingDiamonds,
    IAPPopUp,
    IAPVGP,
    Booster,
    VIP,
    BillboardAd,
    Tutorial,
    Level
}

public enum EconomyAction
{
    Earn,
    Spend
}

public enum AnalyticsBuildingAction
{
    Build,
    Stored,
    Restored,
    Upgrade,
}

public enum AnalyticsBubbleBoyAction
{
    Open,
    GameStarted,
    PopMore
}

public enum AnalyticsDailyQuestAction
{
    TaskCompleted,
    TaskFailed,
    QuestFinished,
    WeekFinished,
    RewardOpened
}

public enum AnalyticsPatientAction
{
    DoctorCured,
    BedCured,
    BedDismissed
}

public enum VisitingEntryPoint
{
    Friends,
    Followers,
    Following,
    FriendsPopup,
    Car,
    Pharmacy,
    LastHelpers,
    GlobalEventTopContributors,
    GiftInbox,
    TreatmentHelpSummary,
    TreatmentSendPush,
    InGameFriendRequest
}

public enum SocialHelpAction
{
    GiveHelpPlantation,
    RequestHelpPlantation,
    GiveHelpEpidemy,
    RequestHelpEpidemy
}

public enum SocialServiceAction
{
    Connect,
    Disconnect,
    Enter,
    Invite
}

public enum SocialEntryPoint
{
    None,
    Auto,
    ChooseHospital,
    Achievements,
    Settings,
    FriendsDrawer,
    AddFriends,
    Car,
    FBPopUp,
    TreatmentSendPushesPopup
}

public enum SocialServiceType
{
    Facebook,
    GameCenter,
    GooglePlayGames,
    KTPlay
}

public enum AnalyticsLoadingStep
{
    None,
    GameLoaded,
    GamePaused,
    GameResumed,
    ErrorAlert,
}

public enum IAPResult
{
    PURCHASE,
    CANCEL,
    FRAUD
}

public enum CurrentIAPFunnel
{
    PopUp,
    MissingResources,
    VGP,
    MissingDiamonds
}

public enum FunnelStepIAPVGP
{
    VGPOpen,
    StartPurchase,
    PurchaseComplete
}

public enum FunnelStepIAPPopUp
{
    OpenIAPPopUp,
    StartPurchase,
    PurchaseComplete
}

public enum FunnelStepIAPMissingResources
{
    MissingResourcesPopUp,
    OpenIAPPopUp,
    StartPurchase,
    PurchaseComplete
}

public enum FunnelStepIAPMissingDiamonds
{
    OpenIAPPopUp,
    StartPurchase,
    PurchaseComplete
}

public enum FunnelStepBillboardAd
{
    PopUpOpen,
    WatchVideoClicked,
    RewardAwarded
}

public enum FunnelStepBoosters
{
    PopUpOpen,
    GetBooster,
    Purchased,
    Activated
}

public enum FunnelStepVip
{
    VipSpeedUpSpawn,
    VipSpawned,
    VipCuredWithDiamonds,
    VipCured,
    VipNotCured
}

public enum EconomySource
{
    //common sources (used for multiple different resources)
    DrawerPurchase,
    DrawerPurchaseAfterMissing,
    FloorColor,
    FloorColorAfterMissing,
    BubbleBoy,
    IAP,
    LevelUpGift,
    RewardedVideo,
    CampaignReward,
    GiftBoxPrize,
    MissingCoins,
    MissingPositive,
    TestModeIAP,
    Tutorial,
    Diagnose,
    ComicCloud,
    DailyQuestReward,
    DailyQuestTaskReplace,
    Epidemy,
    LevelGoalReward,
    GlobalEventReward,
    TapjoyReward,   // Obsolete.
    GiftReward,
    LootBox,
    MaternityHealingRewardBox,
    RecompensationGift,

    //diamond spend sources
    SpeedUpBuilding,
    SpeedUpDoctor,
    SpeedUpProduction,
    SpeedUpProbeTable,
    SpeedUpDiagnose,
    SpeedUpBed,
    SpeedUpPlantation,
    SpeedUpEpidemy,
    SpeedUpVIP,
    EnlargeQueue,
    GetBooster,
    GetGiftBox,
    PharmacyAdvert,
    PharmacyRefresh,
    PharmacyExtendSlots,
    PharmacyExtendSlotsGlobalOffers,
    PharmacyExtendSlotsFriendsOffers,
    MissingResources,
    MissingResourcesStorage,
    MissingResourcesVitaminsMaker,
    MissingResourcesShovel,
    GetPremiumSign,
    GetPremiumFlag,
    GetDailyDeal,
    MastershipUpgrade,
    IAPShopBundle,
    FullfillVitaminCollector,
    PlusOneVitamineCollector,
    SpeedUpBloodTest,
    SpeedUpBondingAndHealing,
    SpeedUpMotherSpawn,
    SpeedUpLabor,
    SpeedUpWaitingForLabor,
    VIPWardUpgrade,
    VIPHelipadUpgrade,

    //diamond earn sources
    Achievement,
    FacebookConnected,
    CommunityReward,
    UpdateReward,
    KTPlay,
    TNTCrossPromotion,

    //coin spend sources
    ExpandHospital,
    ExpandMaternityClinic,
    ExpandLab,
    PlantationRegrow,
    RenewBuilding,
    PharmacyItemPurchased,
    PanaceaUpgrade,
    ElixirStorageUpgrade,
    RollbackPharmacyCollect,
    CureMaternityPatient,
    SendToBloodTest,

    //coin earn sources
    DoctorPatientCured,
    PharmacyRefund,
    PharmacyItemSold,

    //positive energy
    KidCured,
    BacteriaCured,

    //exp
    BedPatientCured,
    BuildingBuilt,
    MedicineProduced,
    PlantProduced,
    CaseCollected,
    PlantationHelp,
    SuperBundle,
    ProductionMachine,
    Plantation,
    BedPatientDonated,
    BloodTestCollected,

    //medicine earn sources
    ProbeTable,
    PharmacyBuy,
    GiftBox,
    BonusItem,
    MissingResourcesBuy,

    //medicine spend sources
    DoctorRoom,
    BedPatient,
    PharmacySell,
    StorageUpgrade,
    VitaminsMakerUpgrade,

    //MaternityPatientCard
    MaternityReadyForLabor,
    MaternityLabourEnded,
    MaternityMotherVitaminized,
    MaternityBloodTest,
    MaternityHealingAndBonding,

    //other
    UseBooster,
    GlobalEventContribution,
    TreatmentRoomHelp,
    DailyRewards,
    EpidemyHelp,

    // Cheat Debug menu
    DebugCheats
}

public enum DecisionPoint
{
    test,
    game_loaded,
    game_reloaded,
    iap_shop_open,
    missing_resources_closed,
    timed_offer,
    starter_pack,
    rewarded_ad_billboard,
    rewarded_ad_bubbleboy,
    rewarded_ad_dailyquest,
    rewarded_ad_coins,
    hints_config,
    tutorial_config,
    kt_config,
    ad_billboard_config,
    ad_coins_config,
    ad_bubbleboy_config,
    ad_dailyquest_config,
    cancer_day,
    loot_box_config,
    loot_box_open,
    iap_shop_sections_order,
    iap_shop_bundles,
    iap_shop_coins,
    ad_vitamin_collector_config,
    image,
    maternity_box_config,
    eventParameters,
    balanceParameters,
    dailyRewardNew,
    global_Events_Parametres,
    bundledRewardDefinition,
    fakedContributionConfig,
    giftsForLevel,
    ad_speedUpMedicine_config,
    caseTierRewards,
    diamondsFromTresures,
    miscellaneousConfigs,
    vip_ward_config,
    custom_offer_1,
    standardEventConfig,
    globalEventConfig,
    timed_config,
    tnt_cross_promotion_config,
}
