using UnityEngine;
using System.Collections;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System;

namespace Hospital
{

//    [DynamoDBTable("Configs")]
//    public class ConfigModel
//    {
//        [DynamoDBHashKey]
//        public string Key;

//        [DynamoDBProperty]
//        public string Value;

//        [DynamoDBProperty]
//        public string ValueAndroid;

//        [DynamoDBProperty]
//        public bool Maintenance;

//        [DynamoDBProperty]
//        public bool MaintenanceAndroid;

//        [DynamoDBProperty]
//        public bool EnableResetHospitalFix = false;

//        [DynamoDBProperty]
//        public bool AdsAlwaysReward = false;

//        [DynamoDBProperty]
//        public List<int> bacteriaRewards;

//        [DynamoDBProperty]
//        public int AdsIntervalOnStand = 180;

//        [DynamoDBProperty]
//        public List<int> bacteriaInfectionTime;

//        public bool IsInMaintenanceMode()
//        {
//#if UNITY_IOS
//            return Maintenance;
//#elif UNITY_ANDROID
//                return MaintenanceAndroid;
//#else
//                return Maintenance;
//#endif
//        }

//        [DynamoDBProperty]
//        public Dictionary<string, string> EventsAndBalance_v2;

//        [DynamoDBProperty]
//        public Dictionary<string, string> SuperBundles;

//        [DynamoDBProperty]
//        public Dictionary<string, string> GameAssetBundles;

//        [DynamoDBProperty]
//        public Dictionary<string, int> ObjectivesDocParam;

//        [DynamoDBProperty]
//        public Dictionary<string, int> BoostersPrice;

//        [DynamoDBProperty]
//        public List<int> PackageIntervals;

//        [DynamoDBProperty]
//        public string DailyDealConfig;

//        [DynamoDBProperty]
//        public Dictionary<string, string> Campaigns;

//        [DynamoDBProperty]
//        public Dictionary<string, string> GlobalEvents;

//        [DynamoDBProperty]
//        public Dictionary<string, string> GameEvents;

//        [DynamoDBProperty]
//        public int GlobalOffersInitSlotsCount = 8;

//        [DynamoDBProperty]
//        public int GlobalOffersMaxSlotsCount = 16;

//        [DynamoDBProperty]
//        public bool GlobalOffersCheckingOfferState = false;

//        [DynamoDBProperty]
//        public int FriendsOffersInitSlotsCount = 8;

//        [DynamoDBProperty]
//        public int FriendsOffersMaxSlotsCount = 16;

//        [DynamoDBProperty]
//        public int GlobalOffersParamA = -1;

//        [DynamoDBProperty]
//        public int GlobalOffersParamC = -1;

//        [DynamoDBProperty]
//        public float GlobalOffersParamX = -1;

//        [DynamoDBProperty]
//        public bool TapjoyEnabled = false;

//        [DynamoDBProperty]
//        public int GiftsRefreshIntervalInSeconds = -1;

//        [DynamoDBProperty]
//        public int GiftsToSendRefreshIntervalInSeconds = -1;

//        [DynamoDBProperty]
//        public int GiftsMax = -1;

//        [DynamoDBProperty]
//        public int GiftsFeatureMinLevel = -1;

//        [DynamoDBProperty]
//        public string GiftRewardGeneratorData;

//        [DynamoDBProperty]
//        public int AddWiseGiftIntervalInHours = 48;

//        [DynamoDBProperty]
//        public int CooldownToSendGiftToSpecificFriendInMinutes = 1440;

//        [DynamoDBProperty]
//        public Dictionary<string, float> ObjectiveRewardFactor1;

//        [DynamoDBProperty]
//        public Dictionary<string, float> ObjectiveProgressForCurePatientInDoctorFactor1;

//        [DynamoDBProperty]
//        public Dictionary<string, float> ObjectiveProgressForCurePatientInHospitalRoomFactor1;

//        [DynamoDBProperty]
//        public Dictionary<string, float> ObjectiveProgressForCureKidstInDoctorFactor1;

//        [DynamoDBProperty]
//        public Dictionary<string, float> ObjectiveProgressForDiagnosePatientFactor1;

//        [DynamoDBProperty]
//        public Dictionary<string, string> MasterySystemConfig;

//        [DynamoDBProperty]
//        public int MastershipMinLevel = -1;

//        [DynamoDBProperty]
//        public bool AddAdsRewardOnAdOpen = true;

//        [DynamoDBProperty]
//        public bool AddAdsRewardFromSaveLogic = false;

//        [DynamoDBProperty]
//        public float ShowelChanceFromGoodieBox1 = 0.5f;

//        [DynamoDBProperty]
//        public float ShowelChanceFromGoodieBox2 = 0.5f;

//        [DynamoDBProperty]
//        public float ShowelChanceFromGoodieBox3 = 0.25f;

//        [DynamoDBProperty]
//        public float ShowelChanceFromVIP = 0.5f;

//        [DynamoDBProperty]
//        public float ShowelChanceFromEpidemyBox = 0.25f;

//        [DynamoDBProperty]
//        public float ShowelChanceFromTreasureChest = 0.5f;

//        [DynamoDBProperty]
//        public int DiamondAmountPerLevelUpAfter50 = 3;

//        [DynamoDBProperty]
//        public float GoldFactorForLevelUpRewardAfter50 = 5f;

//        [DynamoDBProperty]
//        public int FriendsDrawerItemsLimit = 50;

//        [DynamoDBProperty]
//        public bool FBAdsEnabled = true;

//        [DynamoDBProperty]
//        public bool PreloaderBeforeAdsEnabled = true;

//        [DynamoDBProperty]
//        public string BreastCancerFoundationUrl;

//        [DynamoDBProperty]
//        public int HelpInTreatmentRoomRequestCooldown = 30;

//        [DynamoDBProperty]
//        public int HelpInTreatmentRoomPushCooldown = 120;

//        [DynamoDBProperty]
//        public int HelpInTreatmentRoomRequestMaxCounter = 3;

//        [DynamoDBProperty]
//        public int HelpInTreatmentRoomFeatureMinLevel = -1;

//        [DynamoDBProperty]
//        public bool TreatmentHelpSummaryPopupEnabled = false;

//        [DynamoDBProperty]
//        public bool ParticlesInGameEnabled = true;

//        [DynamoDBProperty]
//        public bool ParticlesInGameEnabledAndroid = true;

//        [DynamoDBProperty]
//        public Dictionary<string, int> CheaterScoreConditions;

//        [DynamoDBProperty]
//        public Dictionary<string, int> VitaminsProductionTime;

//        [DynamoDBProperty]
//        public bool OpenShopAlwaysOnTop = false;

//        [DynamoDBProperty]
//        public Dictionary<string, int> VitaminsCollectStep;

//        [DynamoDBProperty]
//        public Dictionary<string, int> VitaminsCollectorCapacity;

//        [DynamoDBProperty]
//        public Dictionary<string, int> VitaminsDeltaCapacityUpgrade;

//        [DynamoDBProperty]
//        public Dictionary<string, int> VitaminsDeltaFillRatioUpgrade;

//        [DynamoDBProperty]
//        public Dictionary<string, List<float>> PositiveEnergyUpgradeEquationParameters;

//        [DynamoDBProperty]
//        public Dictionary<string, List<float>> ToolsUpgradeEquationParameters;

//        [DynamoDBProperty]
//        public string VitaminMakerMastershipConfig;

//        [DynamoDBProperty]
//        public string VipRoomMastershipConfig;

//        [DynamoDBProperty]
//        public string VipHelipadMastershipConfig;

//        [DynamoDBProperty]
//        public int minVitaminesToCure = 15;

//        [DynamoDBProperty]
//        public int maxVitaminesToCure = 20;

//        [DynamoDBProperty]
//        public bool ktPlayButtonEnabled = true;

//        [DynamoDBProperty]
//        public bool ktPlayLoggingEnabled = true;

//        [DynamoDBProperty]
//        public int ktPlayButtonUnlockLevel = 1;

//        [DynamoDBProperty]
//        public int SocialRequestTimeoutEPOCH;

//        [DynamoDBProperty]
//        public int SocialFriendsPoolinSeconds;

//        [DynamoDBProperty]
//        public int SocialFriendsLimit;

//        [DynamoDBProperty]
//        public int SocialRandomFriendsReloadTimeEPOCH;

//        [DynamoDBProperty]
//        public int SocialRandomFriendsDisplayLimit;

//        [DynamoDBProperty]
//        public int SocialServerRandomFriendsLevelSpreed;

//        [DynamoDBProperty]
//        public int SocialServerRandomFriendsQuerryLimit;

//        [DynamoDBProperty]
//        public string SocialRandomFriendsGetURL;

//        [DynamoDBProperty]
//        public string SocialPersonalFriendCodeURL;

//        [DynamoDBProperty]
//        public Dictionary<string,float> AssetBundleSizes;

//        [DynamoDBProperty]
//        public int DiamondLimitToShowSpendConfirmation = BundleManager.DiamondSpendLimitToShowConfirmationDefault; 

//        #region MaternityWard

//        [DynamoDBProperty]
//        public Dictionary<string, int> ItemsRequiredToUnlockMaternity = new Dictionary<string, int>();

//        [DynamoDBProperty]
//        public int MaternityMinTimeToNextMotherSpawn = 0; //ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityTimeToSpawnNextMother_MIN); //BundleManager.MaternityMinTimeToNextMotherSpawnDefault;

//        [DynamoDBProperty]
//        public int OneMaternityExpCostInSec = 0;// ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityExpToRealtimeSeconds_CONVERSION); //BundleManager.OneMaternityExpCostInSecDefault;

//        [DynamoDBProperty]
//        public int MaternityMaxTimeToNextMotherSpawn = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityTimeToSpawnNextMother_MAX); //BundleManager.MaternityMaxTimeToNextMotherSpawnDefault;

//        [DynamoDBProperty]
//        public int MaternityBloodTestCoinsCost = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBloodTestCoinsCost_VALUE); //BundleManager.MaternityBloodTestCoinsCostDefault;

//        [DynamoDBProperty]
//        public int MaternityBloodTestTime = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBloodTestDuration_TIME_INTERVAL); //BundleManager.MaternityBloodTestTimeDefault;

//        [DynamoDBProperty]
//        public float MaternityLabourStageHardChance = 0; // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.maternityLaborStageHard_CHANCE); //BundleManager.MaternityLaborStageHardChance;

//        [DynamoDBProperty]
//        public int MaternityLevelStageDifficultyLevelChange = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLevelStageDifficultyChange_VALUE); //BundleManager.MaternityLevelStageDifficultyLevelChangeDefault;

//        [DynamoDBProperty]
//        public int MaternityMinLaborStageEasy = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageEasyDuration_TIME_INTERVAL_MIN); //BundleManager.MaternityMinLaborStageTimeEasyDefault;

//        [DynamoDBProperty]
//        public int MaternityMaxLaborStageEasy = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageEasyDuration_TIME_INTERVAL_MAX); //BundleManager.MaternityMaxLaborStageTimeEasyDefault;

//        [DynamoDBProperty]
//        public int MaternityMinLaborStageHard = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageHardDuration_TIME_INTERVAL_MIN); //BundleManager.MaternityMinLaborStageTimeHardDefault;

//        [DynamoDBProperty]
//        public int MaternityMaxLaborStageHard = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageHardDuration_TIME_INTERVAL_MAX); //BundleManager.MaternityMaxLaborStageTimeHardDefault;

//        [DynamoDBProperty]
//        public int MaternityMinBondingTime = 0;// ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBondingDuration_TIME_INTERVAL_MIN); //BundleManager.MaternityMinBondingTimeDefault;

//        [DynamoDBProperty]
//        public int MaternityMaxBondingTime = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBondingDuration_TIME_INTERVAL_MAX); //BundleManager.MaternityMaxBondingTimeDefault;

//        [DynamoDBProperty]
//        public int VitaminRandomizationLevelThreshold = 0; // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.vitaminRandomizationLevelThreshold_VALUE); //BundleManager.VitaminRandomizationLevelThresholdDefault;

//        [DynamoDBProperty]
//        public bool EndlessVitaminRandomization = BundleManager.EndlessVitaminRandomizationDefault;

//        [DynamoDBProperty]
//        public float minExponentialParameterA = 0; // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterA_VALUE_MIN); //BundleManager.minExponentialParameterADefault;

//        [DynamoDBProperty]
//        public float maxExponentialParameterA = 0; // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterA_VALUE_MAX); //BundleManager.maxExponentialParameterADefault;

//        [DynamoDBProperty]
//        public float minExponentialParameterB = 0; // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterB_VALUE_MIN);  //BundleManager.minExponentialParameterBDefault;

//        [DynamoDBProperty]
//        public float maxExponentialParameterB = 0; // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterB_VALUE_MAX); //BundleManager.maxExponentialParameterBDefault;

//        [DynamoDBProperty]
//        public bool MaternityWardFeatureEnabledAndroid = true;

//        [DynamoDBProperty]
//        public bool MaternityWardFeatureEnabledIOS = true;

//        [DynamoDBProperty]
//        public float HospitalExpToMaternityExpConverterFactor = 0; // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.hospitalExpToMaternityExp_CONVERSION); //BundleManager.HospitalExpToMaternityExpConverterFactor;

//        #endregion

//        public bool IsMaternityWardFeatureEnabled()
//        {
//#if UNITY_IOS
//            return MaternityWardFeatureEnabledIOS;
//#elif UNITY_ANDROID
//                       return MaternityWardFeatureEnabledAndroid;
//#else
//                       return MaternityWardFeatureEnabledAndroid;
//#endif
//        }

//        public bool IsParticlesInGameEnabled()
//        {
//#if UNITY_IOS
//            return ParticlesInGameEnabled;
//#elif UNITY_ANDROID
//                       return ParticlesInGameEnabledAndroid;
//#else
//                       return ParticlesInGameEnabledAndroid;
//#endif
//        }
//    }
}