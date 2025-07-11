using Amazon.Runtime.Internal.Transform;
using Hospital;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Unity.Services.RemoteConfig;
using UnityEditor;
using UnityEngine;

public class DefaultConfigurationProvider
{
    public static GeneralCData GetGeneralConfig()
    {
        return new GeneralCData()
        {
            versionURL = "https://versionsupport.queserve.net/api/1/mh/",
            supportEmail = "myhospital@kuuhubb.com",
            TermsOfServiceURL = "http://myhospital.games/terms-of-service/",
            PrivacyPolicyURL = "http://myhospital.games/privacy-policy/",
            FacebookFanpageURL = "https://m.facebook.com/myhospitalgame/?fref=ts",
            InstagramURL = "http://instagram.com/myhospitalgame",
            YouTubeURL = "https://www.youtube.com/channel/UC8P2gdbzzalaH0-sBquI0dg/featured"
        };
    }

    private static GlobalEventsCData _cachedGlobalEventsCData = null;
    private static StandardEventsCData _cachedStandardEventsCData = null;
    private static ConfigCData _cachedConfigCData = null;
    private static DailyRewardsCData _cachedDailyRewardsCData = null;
    private static List<AdPointCData> _cachedAdPointCDatas = null;
    private static int _lastAdPointLevel = 0;
    public static List<AdPointCData> GetAdPointCDatas(int playerLevel)
    {
        if (_cachedAdPointCDatas != null && _lastAdPointLevel == playerLevel) return _cachedAdPointCDatas;
        _lastAdPointLevel = playerLevel;
        _cachedAdPointCDatas = new List<AdPointCData>();
        _cachedAdPointCDatas.Add(new AdPointCData()
        {
            AdPoint = "rewarded_ad_billboard",
            AdCooldownSeconds = 10,
            AdDailyLimit = 15,
            AdRewardAmount = 1,
            AdRewardMultiplier = 0,
            AdSessionLimit = 0
        });
        _cachedAdPointCDatas.Add(new AdPointCData()
        {
            AdPoint = "rewarded_ad_vitamin_collector",
            AdCooldownSeconds = 180,
            AdDailyLimit = 999,
            AdRewardAmount = 1,
            AdRewardMultiplier = 0,
            AdSessionLimit = 0
        });
        //AdsController doesnt use AdRewardAmount for rewarded_ad_coins. Only AdRewardMultiplier if its != 0. So this has only set CD and limit.
        if (playerLevel < 11)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 25,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }
        else if (playerLevel >= 11 && playerLevel < 21)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 75,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }
        else if (playerLevel >= 21 && playerLevel < 31)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 150,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }
        else if (playerLevel >= 31 && playerLevel < 41)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 300,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }
        else if (playerLevel >= 41 && playerLevel < 51)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 400,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }
        else if (playerLevel >= 51 && playerLevel < 621)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 500,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }
        else if (playerLevel >= 61)
        {
            _cachedAdPointCDatas.Add(new AdPointCData()
            {
                AdPoint = "rewarded_ad_coins",
                AdCooldownSeconds = 10,
                AdDailyLimit = 15,
                AdRewardAmount = 600,
                AdRewardMultiplier = 0,
                AdSessionLimit = 0
            });
        }

        return _cachedAdPointCDatas;
    }

    public static DailyRewardsCData GetDailyRewardsCData()
    {
        if (_cachedDailyRewardsCData != null) return _cachedDailyRewardsCData;
        _cachedDailyRewardsCData = new DailyRewardsCData()
        {
            defaultIncrementValueDailyRewardConfig = "0.065!0.085!0.105!0.145!0.185!0.225!0.265",
            defaultWeekDailyRewardConfig = "coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!bundle;1|1||1@specialBox"
        };
        return _cachedDailyRewardsCData;
    }
    public static ConfigCData GetConfigCData()
    {
        if (_cachedConfigCData != null) return _cachedConfigCData;
        _cachedConfigCData = new ConfigCData()
        {
            Key = "version",
            Value = "205",
            ValueAndroid = "205",
            Maintenance = false,
            MaintenanceAndroid = false,
            EnableResetHospitalFix = false,
            AdsAlwaysReward = true,
            bacteriaRewards = new List<int>(),
            AdsIntervalOnStand = 180,
            bacteriaInfectionTime = new List<int>(),
            EventsAndBalance_v2 = new Dictionary<string, string>()
            {
                {"rewardForTODOSCoins","500#100###10"},
                {"shovelDrawChance","80#20#2018-08-10 12:00:00#2018-08-13 12:00:00#15"},
                {"nextVIPPenalty","1#7200#2018-03-02 12:00:00#2018-03-05 12:00:00#10"},
                {"treasureChestDiamondsAfterIAP","5#5###1"},
                {"nextVIPArrive","20#3600#2018-07-27 12:00:00#2018-07-30 12:00:00#10"},
                {"moreKids","30#30#2017-08-26 12:00:00#2017-08-27 12:00:00#12"},
                {"expForGardenHelp","15#15###1"},
                {"nextVipPenalty","1#7200#2018-03-02 12:00:00#2018-03-05 12:00:00#10"},
                {"tankToolsMinRange","90#90###1"},
                {"adsIntervalIAPAndroid","180#180###1"},
                {"panaceaCollectorFillRate","100#100###1"},
                {"positiveEnergyRewardForHelpInTreatmentRoomX","2#0#2017-11-23 12:00:00#2017-11-27 12:00:00#12"},
                {"rewardForTODOSDiamonds","1000#500###10"},
                {"positiveEnergyRewardForHelpInTreatmentRoomY","3#2#2017-11-23 12:00:00#2017-11-27 12:00:00#12"},
                {"tankToolsMaxRange","110#110###1"},
                {"expRewardForHelpInTreatmentRoom","100#100###1"},
                {"nextEpidemy","60#14400#2017-11-23 12:00:00#2017-11-27 12:00:00#17"},
                {"patientWithBacteria","7#7###21"},
                {"shovelDrawChanceInGarden","100#20#2018-04-20 12:00:00#2018-04-23 12:00:00#15"},
                {"xpForCurePatientInHospital","100#100###1"},
                {"patientVitaminRequired","50#50###16"},
                {"treasureChestInterval","60#300#2018-07-12 12:00:00#2018-07-16 12:00:00#10"},
                {"nextFreeBubbleBoy","600#-1#2018-08-24 12:00:00#2018-08-27 12:00:00#10"},
                {"positiveEnergyFromKids","100#100###12"},
                {"bacteriaReward0","10#10###8"},
                {"patientToDiagnose","50#50###13"},
                {"bacteriaReward1","20#20###8"},
                {"bacteriaReward2","30#30###8"},
                {"bacteriaSpreedDuration1","64800#64800###19"},
                {"bacteriaSpreedDuration0","86400#86400###19"},
                {"bacteriaSpreedDuration2","57600#57600###19"},
                {"adsCoinsRewardIAPAndroid","25#25###1"}
            },
            SuperBundles = new Dictionary<string, string>(),
            GameAssetBundles = new Dictionary<string, string>()
            {
                {"Lvl11","Lvl11#CacheOnlyAssetBundle#HospitalSign/Lvl11#1"},
                {"Lvl12","Lvl12#CacheOnlyAssetBundle#HospitalSign/Lvl12#1"},
                {"hospitalSign","hospital_sign#HospitalSignAssetBundle#HospitalSign#1"},
                {"Lvl10","Lvl10#CacheOnlyAssetBundle#HospitalSign/Lvl10#1"},
                {"Balloon","Balloon#CacheOnlyAssetBundle#HospitalSign/Balloon#1"},
                {"Lvl1","Lvl1#CacheOnlyAssetBundle#HospitalSign/Lvl1#1"},
                {"Modern","Modern#CacheOnlyAssetBundle#HospitalSign/Modern#1"},
                {"Lvl6","Lvl6#CacheOnlyAssetBundle#HospitalSign/Lvl6#1"},
                {"Lvl7","Lvl7#CacheOnlyAssetBundle#HospitalSign/Lvl7#1"},
                {"Lvl8","Lvl8#CacheOnlyAssetBundle#HospitalSign/Lvl8#1"},
                {"Lvl9","Lvl9#CacheOnlyAssetBundle#HospitalSign/Lvl9#1"},
                {"Lvl2","Lvl2#CacheOnlyAssetBundle#HospitalSign/Lvl2#1"},
                {"Lvl3","Lvl3#CacheOnlyAssetBundle#HospitalSign/Lvl3#1"},
                {"Lvl4","Lvl4#CacheOnlyAssetBundle#HospitalSign/Lvl4#1"},
                {"Lvl5","Lvl5#CacheOnlyAssetBundle#HospitalSign/Lvl5#1"},
                {"Arizona","Arizona#CacheOnlyAssetBundle#HospitalSign/Arizona#1"},
                {"Classic1","Classic1#CacheOnlyAssetBundle#HospitalSign/Classic1#1"},
                {"Classic2","Classic2#CacheOnlyAssetBundle#HospitalSign/Classic2#1"},
                {"Chocolate","Chocolate#CacheOnlyAssetBundle#HospitalSign/Chocolate#1"},
                {"Tropical","Tropical#CacheOnlyAssetBundle#HospitalSign/Tropical#1"},
                {"Acient","Acient#CacheOnlyAssetBundle#HospitalSign/Acient#1"},
                {"Floral","Floral#CacheOnlyAssetBundle#HospitalSign/Floral#1"},
                {"Country","Country#CacheOnlyAssetBundle#HospitalSign/Country#1"},
                {"global_event_asset_bundle","global_event_asset_bundle#CacheOnlyAssetBundle#global_event_asset_bundle#7"}
            },
            ObjectivesDocParam = new Dictionary<string, int>()
            {
                {"GreenDoc",10},
                {"WhiteDoc",11},
                {"SunnyDoc",15},
                {"YellowDoc",13},
                {"BlueDoc",25},
                {"SkyDoc",23},
                {"RedDoc",19},
                {"PinkDoc",21},
                {"PurpleDoc",25}
            },
            BoostersPrice = new Dictionary<string, int>(),
            PackageIntervals = new List<int>()
            {
                300,
                1800,
                3600,
                7200,
                21600
            },
            DailyDealConfig = "15(00)%3%0.5^15(01)%3%0.5^15(02)%3%0.5^15(03)%5%0.8^15(04)%3%0.5^15(05)%3%0.5^15(06)%3%0.5^16(00)%40%0.6!0.8%0%2%86400!3%4%0!2!1.2",
            Campaigns = new Dictionary<string, string>()
            {
                {"HintSystem","LevelGoals#"},
                {"Objectives","Objectives#CurePatientHospitalRoomObjective;3;Coin^1^false;2;AnyPatient?CurePatientDoctorRoomObjective;3;Coin^1^false;1;SingleRoom;BlueDoc?LevelUpObjective;3;Coin^1^false;1!CurePatientDoctorRoomObjective;4;Coin^1^false;1;SingleRoom;BlueDoc?CurePatientHospitalRoomObjective;4;Coin^1^false;2;AnyPatient?BuildRotatableObjective;4;Coin^1^false;3;ProbTab?LevelUpObjective;4;Coin^1^false;1!CurePatientDoctorRoomObjective;5;Coin^2^false;1;SingleRoom;YellowDoc?CurePatientHospitalRoomObjective;5;Coin^2^false;2;AnyPatient?CurePatientDoctorRoomObjective;5;Coin^2^false;2;SingleRoom;BlueDoc?LevelUpObjective;5;Coin^2^false;1!CurePatientHospitalRoomObjective;6;Coin^2^false;3;AnyPatient?CurePatientDoctorRoomObjective;6;Coin^2^False;1;SingleRoom;GreenDoc?BuildRotatableObjective;6;Coin^2^false;1;EyeDropsLab?BuildRotatableObjective;6;Coin^2^false;1;2xBedsRoom?LevelUpObjective;6;Coin^2^false;1!CurePatientDoctorRoomObjective;7;Coin^2^false;8;AnyRoom?CurePatientHospitalRoomObjective;7;Coin^3^false;4;AnyPatient?BuildRotatableObjective;7;Coin^3^false;1;NoseLab?BuildRotatableObjective;7;Coin^3^false;1;2xBedsRoom?LevelUpObjective;7;Coin^3^false;1!TreatmentHelpRequestObjective;8;Diamonds^1^false;3?CurePatientHospitalRoomObjective;8;Coin^4^false;5;AnyPatient?CurePatientDoctorRoomObjective;8;Coin^4^false;1;SingleRoom;WhiteDoc?BuildRotatableObjective;8;Coin^4^false;1;CapsuleLab?LevelUpObjective;8;Coin^4^false;1!CurePatientHospitalRoomObjective;9;Coin^5^false;7;AnyPatient?CurePatientDoctorRoomObjective;9;Coin^5^false;15;AnyRoom?RenovateSpecialObjective;9;Coin^6^false;1;VIP_ROOM?BuildRotatableObjective;9;Coin^5^false;1;PillLab?LevelUpObjective;9;Coin^5^false;1"},{"LevelGoals","LevelGoals#CurePatientLevelGoal;3;1;2;2xBedsRoom;HospitalRoomPatient?CurePatientLevelGoal;3;1;1;BlueDoc;DoctorRoomPatient!CurePatientLevelGoal;4;1;1;BlueDoc;DoctorRoomPatient?CurePatientLevelGoal;4;1;2;2xBedsRoom;HospitalRoomPatient?BuildRotatableObjectLevelGoal;4;1;3;ProbTab!CurePatientLevelGoal;5;2;1;YellowDoc;DoctorRoomPatient?CurePatientLevelGoal;5;2;2;2xBedsRoom;HospitalRoomPatient?CurePatientLevelGoal;5;2;2;BlueDoc;DoctorRoomPatient!CurePatientLevelGoal;6;2;3;2xBedsRoom;HospitalRoomPatient?CurePatientLevelGoal;6;2;1;GreenDoc;DoctorRoomPatient?BuildRotatableObjectLevelGoal;6;2;1;EyeDropsLab?BuildRotatableObjectLevelGoal;6;2;1;2xBedsRoom!CurePatientLevelGoal;7;2;8;AnyDoc;DoctorRoomPatient?CurePatientLevelGoal;7;3;4;2xBedsRoom;HospitalRoomPatient?BuildRotatableObjectLevelGoal;7;3;1;NoseLab?BuildRotatableObjectLevelGoal;7;3;1;2xBedsRoom!TreatmentHelpRequestObjective;8;Diamonds^1^false;3?CurePatientLevelGoal;8;4;5;2xBedsRoom;HospitalRoomPatient?CurePatientLevelGoal;8;3;1;WhiteDoc;DoctorRoomPatient?BuildRotatableObjectLevelGoal;8;4;1;CapsuleLab!CurePatientLevelGoal;9;5;10;2xBedsRoom;HospitalRoomPatient?CurePatientLevelGoal;9;5;15;AnyDoc;DoctorRoomPatient?RenovateSpecialObjectLevelGoal;9;5;1;VIP_ROOM?BuildRotatableObjectLevelGoal;9;5;1;PillLab"},
                {"ObjectivesLVL10","Objectives#False!CurePatientHospitalRoomObjective;3;Diamonds^1^false;2;AnyPatient?CurePatientDoctorRoomObjective;3;Diamonds^1^false;1;SingleRoom;BlueDoc!CurePatientDoctorRoomObjective;4;Diamonds^1^false;1;SingleRoom;BlueDoc?CurePatientHospitalRoomObjective;4;Diamonds^1^false;2;AnyPatient?BuildRotatableObjective;4;Diamonds^1^false;3;ProbTab!CurePatientDoctorRoomObjective;5;Diamonds^1^false;1;SingleRoom;YellowDoc?CurePatientHospitalRoomObjective;5;Diamonds^1^false;2;AnyPatient?CurePatientDoctorRoomObjective;5;Diamonds^1^false;2;SingleRoom;BlueDoc!CurePatientHospitalRoomObjective;6;Diamonds^1^false;3;AnyPatient?CurePatientDoctorRoomObjective;6;Diamonds^1^False;1;SingleRoom;GreenDoc?BuildRotatableObjective;6;Diamonds^1^false;1;EyeDropsLab?BuildRotatableObjective;6;Diamonds^1^false;1;2xBedsRoom!CurePatientDoctorRoomObjective;7;Diamonds^1^false;8;AnyRoom?CurePatientHospitalRoomObjective;7;Diamonds^1^false;4;AnyPatient?BuildRotatableObjective;7;Diamonds^1^false;1;NoseLab?BuildRotatableObjective;7;Diamonds^1^false;1;2xBedsRoom!TreatmentHelpRequestObjective;8;Diamonds^1^false;3?CurePatientHospitalRoomObjective;8;Diamonds^1^false;5;AnyPatient?CurePatientDoctorRoomObjective;8;Diamonds^1^false;1;SingleRoom;WhiteDoc?BuildRotatableObjective;8;Diamonds^1^false;1;CapsuleLab!CurePatientHospitalRoomObjective;9;Diamonds^1^false;7;AnyPatient?CurePatientDoctorRoomObjective;9;Diamonds^1^false;15;AnyRoom?RenovateSpecialObjective;9;Diamonds^1^false;1;VIP_ROOM?BuildRotatableObjective;9;Diamonds^1^false;1;PillLab"},
                {"ObjectivesDynamic","Objectives#True!CurePatientHospitalRoomObjective;3;Diamonds^1^false;2;AnyPatient?CurePatientDoctorRoomObjective;3;Diamonds^1^false;1;SingleRoom;BlueDoc!CurePatientDoctorRoomObjective;4;Diamonds^1^false;1;SingleRoom;BlueDoc?CurePatientHospitalRoomObjective;4;Diamonds^1^false;2;AnyPatient?BuildRotatableObjective;4;Diamonds^1^false;3;ProbTab!CurePatientDoctorRoomObjective;5;Diamonds^1^false;1;SingleRoom;YellowDoc?CurePatientHospitalRoomObjective;5;Diamonds^1^false;2;AnyPatient?CurePatientDoctorRoomObjective;5;Diamonds^1^false;2;SingleRoom;BlueDoc!CurePatientHospitalRoomObjective;6;Diamonds^1^false;3;AnyPatient?CurePatientDoctorRoomObjective;6;Diamonds^1^False;1;SingleRoom;GreenDoc?BuildRotatableObjective;6;Diamonds^1^false;1;EyeDropsLab?BuildRotatableObjective;6;Diamonds^1^false;1;2xBedsRoom!CurePatientDoctorRoomObjective;7;Diamonds^1^false;8;AnyRoom?CurePatientHospitalRoomObjective;7;Diamonds^1^false;4;AnyPatient?BuildRotatableObjective;7;Diamonds^1^false;1;NoseLab?BuildRotatableObjective;7;Diamonds^1^false;1;2xBedsRoom!TreatmentHelpRequestObjective;8;Diamonds^1^false;3?CurePatientHospitalRoomObjective;8;Diamonds^1^false;5;AnyPatient?CurePatientDoctorRoomObjective;8;Diamonds^1^false;1;SingleRoom;WhiteDoc?BuildRotatableObjective;8;Diamonds^1^false;1;CapsuleLab!CurePatientHospitalRoomObjective;9;Diamonds^1^false;7;AnyPatient?CurePatientDoctorRoomObjective;9;Diamonds^1^false;15;AnyRoom?RenovateSpecialObjective;9;Diamonds^1^false;1;VIP_ROOM?BuildRotatableObjective;9;Diamonds^1^false;1;PillLab"},
                {"ObjectivesHint","Objectives#True"}
            },            
            GlobalEvents = new Dictionary<string, string>()
            {
                {"GE4_XMAS","CollectOnMapActivityGlobalEvent;true;10000000;10001688;65;25?65?140?280?500;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;LootBox?1?false?xmas;1514203200;1514548800;EVENTS/EVENT_DESCRIPTION_XMAS_TITLE;EVENTS/EVENT_DESCRIPTION_XMAS;ChristmasGift;5;2;1;90"},
                {"GE7_REAL_EASTER","CollectOnMapActivityGlobalEvent;true;10000000;10011690;65;25?65?140?280?500;LootBox?1?false?xmas%GoodieBox?1?false%LootBox?1?false?xmas%LootBox?1?false?xmas%LootBox?1?false?xmas;LootBox?1?false?xmas;1522411200;1522756800;EVENTS/EVENT_DESCRIPTION_EASTER_TITLE;EVENTS/EVENT_DESCRIPTION_EASTER;Pumpkin;5;2;1;90"},
                {"GE1","MedicineContributeGlobalEvent;true;1000000;1104025;5;2?5?10?20?50;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;1501761600;1502107200;EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE;EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1;AdvancedElixir(1)"},
                {"GE3","CollectPumpkinsActivityGlobalEvent;true;10000000;10204000;65;25?65?140?280?500;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Decoration?1?false?scarecrow_halloween;1666177200;1667307600;EVENTS/EVENT_DESCRIPTION_HALLOWEEN_1_TITLE;EVENTS/EVENT_DESCRIPTION_HALLOWEEN_1;1;5;2;1;180"},
                {"GE5_VALENT","MedicineContributeGlobalEvent;true;1000000;1001169;2;4?10?20?40?80;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;1535716800;1536062400;EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE;EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1;AdvancedElixir(4)"},
                {"GE2","CurePatientActivityGlobalEvent;true;3000000;3241875;6;3?6?10?15?20;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;1506686400;1507032000;EVENTS/EVENT_DESCRIPTION_CURE_TITLE;EVENTS/EVENT_DESCRIPTION_CURE;2xBedsRoom"},
                {"GE6_EASTER","CurePatientActivityGlobalEvent;true;3000000;3241875;6;3?6?10?15?20;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;1519300800;1519646400;EVENTS/EVENT_DESCRIPTION_CURE_TITLE;EVENTS/EVENT_DESCRIPTION_CURE;2xBedsRoom"},
                {"GE9","CurePatientActivityGlobalEvent;true;1000000;1017245;10;3?10?15?20?25;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;1530705600;1531137600;EVENTS/EVENT_DESCRIPTION_CURE_TITLE;EVENTS/EVENT_DESCRIPTION_CURE;2xBedsRoom"},
                {"GE8_HEALTH","CurePatientActivityGlobalEvent;true;1000000;1022895;10;3?10?15?20?25;Luring?1?false?%GoodieBox?1?false%Luring?2?false?%Booster?1?false?1%SpecialBox?1?false;Diamond?15?false;1523016000;1523361600;EVENTS/EVENT_DESCRIPTION_CURE_TITLE;EVENTS/EVENT_DESCRIPTION_CURE;2xBedsRoom"}
            },            
            GameEvents = new Dictionary<string, string>()
            {
                {"CancerDay" , "2017-10-20 12:00:00#2017-10-23 12:00:00"}
            },
            GlobalOffersInitSlotsCount = 8,
            GlobalOffersMaxSlotsCount = 16,
            GlobalOffersCheckingOfferState = false,
            FriendsOffersInitSlotsCount = 8,
            FriendsOffersMaxSlotsCount = 16,
            GlobalOffersParamA = 17,
            GlobalOffersParamC = 2,
            GlobalOffersParamX = 0.12358f,
            TapjoyEnabled = true,
            GiftsRefreshIntervalInSeconds = -1,
            GiftsToSendRefreshIntervalInSeconds = -1,
            GiftsMax = -1,
            GiftsFeatureMinLevel = 6,
            GiftRewardGeneratorData = "Coin^60;Diamond^3;Mixture^12;StorageUpgrader^3;Shovel^9;PositiveEnergy^13?AdvancedElixir(0)^25;AdvancedElixir(1)^25;AdvancedElixir(2)^10;AdvancedElixir(3)^15;AdvancedElixir(4)^25?0.57^21.0?Diamond^5;Mixture^1;StorageUpgrader^1;Shovel^1;PositiveEnergy^1",
            AddWiseGiftIntervalInHours = 48,
            CooldownToSendGiftToSpecificFriendInMinutes = 1440,
            ObjectiveRewardFactor1 = new Dictionary<string, float>()
            {
                {"expParameterB",0.2972f},
                {"logAdder",9f},
                {"expParameterA",0.201f},
                {"expAdder",9f},
                {"logParameterA",350.36f},
                {"logParameterB",863.45f}
            },
            ObjectiveProgressForCurePatientInDoctorFactor1 = new Dictionary<string, float>()
            {
                {"anyDoctorFactor",2.5f},
                {"logBase",1.181f},
                {"adder",0f},
                {"power",2f}
            },
             ObjectiveProgressForCurePatientInHospitalRoomFactor1 = new Dictionary<string, float>
            {
                {"bacteriaFactor",0.1f},
                 {"logBase",1.3f},
                 {"deseaseFactor",0.15f},
                 {"vipFactor",0.16f},
                 {"bloodFactor",0.2f},
                 {"diagnosisFactor",0.2f},
                 {"adder",-3f},
                 {"power",2f},
                 {"sexFactor",0.5f},
                 {"plantFactor",0.2f}
            },
            ObjectiveProgressForCureKidstInDoctorFactor1 = new Dictionary<string, float>()
            {
                {"logBase",1.3f},
                {"adder",-11f},
                {"power",2f}
            },
            ObjectiveProgressForDiagnosePatientFactor1 = new Dictionary<string, float>()
            {
                {"logBase",2.862f},
                {"adder",0f},
                {"power",2f},
                {"diagnoseAnyFactor",2.5f}
            },
            MasterySystemConfig = new Dictionary<string, string>()
            {
                {"EyeDropsLab","140;510;1940?600;2680;5600?1.1;1.1;0.9"},
                {"GreenDoc","110;330;540;1020?600;1460;2340;4340?1.1;1.1;1.1;0.9"},
                {"BloodPressure","50;230;1110?600;2040;5600?0.95;0.9;0.85"},
                {"Mri","40;180;870?600;2360;5600?0.95;0.9;0.85"},
                {"SyrupLab","200;650;2430?600;1780;5600?1.1;1.1;0.9"},
                {"BalmLab","240;690;2470?600;1260;5600?1.1;1.1;0.9"},
                {"NoseLab","150;520;2030?620;3000;5600?1.1;1.1;0.9"},
                {"SkyDoc","50;200;460;950?640;2680;6040;9600?1.1;1.1;1.1;0.9"},
                {"FizzyTabLab","70;240;850?680;3180;5600?1.1;1.1;0.9"},
                {"DripsLab","60;190;690?680;3000;5600?1.1;1.1;0.9"},
                {"JellyLab","30;80;280?880;3880;5600?1.1;1.1;0.9"},
                {"ElixirLab","400;1940;9740?600;2000;5600?0.95;0.9;0.85"},
                {"BlueDoc","1000;2600;3680;6600?760;1780;2460;4340?1.1;1.1;1.1;0.9"},
                {"PinkDoc","50;190;380;760?620;2100;4120;5600?1.1;1.1;1.1;0.9"},
                {"CapsuleLab","110;410;1620?600;2800;5600?1.1;1.1;0.9"},
                {"WhiteDoc","70;240;420;800?600;1640;2760;5240?1.1;1.1;1.1;0.9"},
                {"YellowDoc","280;760;1120;2040?600;1320;1900;3380?1.1;1.1;1.1;0.9"},
                {"RedDoc","90;280;490;940?600;1640;2800;5300?1.1;1.1;1.1;0.9"},
                {"PurpleDoc","50;290;880;1910?680;4040;12160;20000?1.1;1.1;1.1;0.9"},
                {"Inhaler Maker","70;210;740?680;2920;5600?1.1;1.1;0.9"},
                {"ExtractLab","30;110;420?820;4080;5600?1.1;1.1;0.9"},
                {"SunnyDoc","60;210;390;750?600;1840;3360;5600?1.1;1.1;1.1;0.9"},
                {"Laser","30;110;530?680;2860;5600?0.95;0.9;0.85"},
                {"MicroscopeLab","80;260;920?620;2680;5600?1.1;1.1;0.9"},
                {"PillLab","110;390;1440?620;3000;5600?1.1;1.1;0.9"},
                {"Xray","50;270;1340?600;1840;5600?0.95;0.9;0.85"},
                {"ShotLab","60;190;670?680;3060;5600?1.1;1.1;0.9"},
                {"UltraSound","30;130;650?620;2600;5600?0.95;0.9;0.85"},
                {"VitaminMaker", "320;990;6680?820;3560;5600?1.1;1.1;0.9" },
                {"vipWard", "5;20;50;100;250?100;160;320;780;1500?1.1667;1.3333;1.5;1.6667;1.8333?Special(4)+2/Special(5)+2/Special(6)+2;Special(4)+5/Special(5)+5/Special(6)+5;Special(4)+10/Special(5)+10/Special(6)+10;Special(4)+25/Special(5)+25/Special(6)+25;Special(4)+50/Special(5)+50/Special(6)+50" },
                {"vipHelipad","5;20;50;100;250?100;160;320;780;1500?0.8333;0.6667;0.5;0.3333;0.1667?Special(0)+2/Special(1)+2/Special(2)+2;Special(0)+5/Special(1)+5/Special(2)+5;Special(0)+10/Special(1)+10/Special(2)+10;Special(0)+25/Special(1)+25/Special(2)+25;Special(0)+50/Special(1)+50/Special(2)+50" }
            },
            MastershipMinLevel = 15,
            AddAdsRewardOnAdOpen = true,
            AddAdsRewardFromSaveLogic = false,
            ShowelChanceFromGoodieBox1 = 0.5f,
            ShowelChanceFromGoodieBox2 = 0.5f,
            ShowelChanceFromGoodieBox3 = 0.25f,
            ShowelChanceFromVIP = 0.5f,
            ShowelChanceFromEpidemyBox = 0.25f,
            ShowelChanceFromTreasureChest = 0.5f,
            DiamondAmountPerLevelUpAfter50 = 3,
            GoldFactorForLevelUpRewardAfter50 = 5f,
            FriendsDrawerItemsLimit = 50,
            FBAdsEnabled = true,
            PreloaderBeforeAdsEnabled = true,
            BreastCancerFoundationUrl = "http://nbcf.org.au/",
            HelpInTreatmentRoomRequestCooldown = 86400,
            HelpInTreatmentRoomPushCooldown = 86400,
            HelpInTreatmentRoomRequestMaxCounter = 3,
            HelpInTreatmentRoomFeatureMinLevel = 8,
            TreatmentHelpSummaryPopupEnabled = true,
            ParticlesInGameEnabled = true,
            ParticlesInGameEnabledAndroid = false,
            CheaterScoreConditions = new Dictionary<string, int>()
            {
                {"HighLevel",200},
                {"PharmacyItems",1000000},
                {"Progress",10000},
                {"AppId",2000000},
                {"Language",1},
                {"Platform",2},
                {"Income",1000},
                {"GEcontribution",100},
                {"QuickBuck",20000},
                {"Rich",100000},
                {"LowLevel",20},
                {"Week",10}
            },
            VitaminsProductionTime = new Dictionary<string, int>()
            {
                {"Vitamins(2)",1800},
                {"Vitamins(1)",1200},
                {"Vitamins(3)",3600},
                {"Vitamins(0)",900}
            },
            OpenShopAlwaysOnTop = false,
            VitaminsCollectStep = new Dictionary<string, int>()
            {
                {"Vitamins(2)",1},
                {"Vitamins(1)",1},
                {"Vitamins(3)",1},
                {"Vitamins(0)",1}
            },
            VitaminsCollectorCapacity = new Dictionary<string, int>()
            {
                {"Vitamins(2)",3},
                {"Vitamins(1)",4},
                {"Vitamins(3)",2},
                {"Vitamins(0)",5}
            },
            VitaminsDeltaCapacityUpgrade = new Dictionary<string, int>()
            {
                {"Vitamins(2)",1},
                {"Vitamins(1)",1},
                {"Vitamins(3)",1},
                {"Vitamins(0)",1}
            },
            VitaminsDeltaFillRatioUpgrade = new Dictionary<string, int>()
            {
                {"Vitamins(2)",1},
                {"Vitamins(1)",1},
                {"Vitamins(3)",1},
                {"Vitamins(0)",1}
            },
            PositiveEnergyUpgradeEquationParameters = new Dictionary<string, List<float>>()
            {
                {"Vitamins(2)",new List<float>() {13.95f,0.6579f,0f} },
                {"Vitamins(1)",new List<float>() {8.0481f,0.6579f,0f} },
                {"Vitamins(3)",new List<float>() {20.387f,0.6579f,0f} },
                {"Vitamins(0)",new List<float>() {7.5115f,0.6579f,0f} }
            },
            ToolsUpgradeEquationParameters = new Dictionary<string, List<float>>()
            {
                {"Vitamins(2)",new List<float>() {1.0359f,0.3289f,1f}},
                {"Vitamins(1)",new List<float>() {1.0359f,0.3289f,1f}},
                {"Vitamins(3)",new List<float>() {1.0359f,0.3289f,1f}},
                {"Vitamins(0)",new List<float>() {1.0359f,0.3289f,1f}}
            },
            VipUnlockCostType = "Coin",
            VipUnlockCost = 210,
            VipUnlockTime = 7200,
            minVitaminesToCure = 3,
            maxVitaminesToCure = 10,
            ktPlayButtonEnabled = true,
            ktPlayLoggingEnabled = true,
            ktPlayButtonUnlockLevel = 1,
            SocialRequestTimeoutEPOCH = 259200,
            SocialFriendsPoolinSeconds = 60,
            SocialFriendsLimit = 100,
            SocialRandomFriendsReloadTimeEPOCH = 180,
            SocialRandomFriendsDisplayLimit = 8,
            SocialServerRandomFriendsLevelSpreed = 2,
            SocialServerRandomFriendsQuerryLimit = 100,
            SocialRandomFriendsGetURL = "https://0481m99i0b.execute-api.eu-west-1.amazonaws.com/prod/random-friends-proposal?level=",
            SocialPersonalFriendCodeURL = "https://vc6q0smhn6.execute-api.eu-west-1.amazonaws.com/prod/get-friend-code-by-save-id?saveId=",
            AssetBundleSizes = new Dictionary<string, float>()
            {
                {"fonts_ab",6.5f},
                {"maternity_scene_ab",28f},
                {"main_scene_ab",32f}
            },
            DiamondLimitToShowSpendConfirmation = 1,
            ItemsRequiredToUnlockMaternity = new Dictionary<string, int>()
            {
                { "Special(3)", 5},
                { "Special(6)", 2}
            },
            MaternityMinTimeToNextMotherSpawn = 0, //ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityTimeToSpawnNextMother_MIN), //BundleManager.MaternityMinTimeToNextMotherSpawnDefault,
            OneMaternityExpCostInSec = 0,// ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityExpToRealtimeSeconds_CONVERSION), //BundleManager.OneMaternityExpCostInSecDefault,
            MaternityMaxTimeToNextMotherSpawn = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityTimeToSpawnNextMother_MAX), //BundleManager.MaternityMaxTimeToNextMotherSpawnDefault,
            MaternityBloodTestCoinsCost = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBloodTestCoinsCost_VALUE), //BundleManager.MaternityBloodTestCoinsCostDefault,
            MaternityBloodTestTime = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBloodTestDuration_TIME_INTERVAL), //BundleManager.MaternityBloodTestTimeDefault,
            MaternityLabourStageHardChance = 0, // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.maternityLaborStageHard_CHANCE), //BundleManager.MaternityLaborStageHardChance,
            MaternityLevelStageDifficultyLevelChange = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLevelStageDifficultyChange_VALUE), //BundleManager.MaternityLevelStageDifficultyLevelChangeDefault,
            MaternityMinLaborStageEasy = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageEasyDuration_TIME_INTERVAL_MIN), //BundleManager.MaternityMinLaborStageTimeEasyDefault,
            MaternityMaxLaborStageEasy = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageEasyDuration_TIME_INTERVAL_MAX), //BundleManager.MaternityMaxLaborStageTimeEasyDefault,
            MaternityMinLaborStageHard = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageHardDuration_TIME_INTERVAL_MIN), //BundleManager.MaternityMinLaborStageTimeHardDefault,
            MaternityMaxLaborStageHard = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityLaborStageHardDuration_TIME_INTERVAL_MAX), //BundleManager.MaternityMaxLaborStageTimeHardDefault,
            MaternityMinBondingTime = 0,// ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBondingDuration_TIME_INTERVAL_MIN), //BundleManager.MaternityMinBondingTimeDefault,
            MaternityMaxBondingTime = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.maternityBondingDuration_TIME_INTERVAL_MAX), //BundleManager.MaternityMaxBondingTimeDefault,
            VitaminRandomizationLevelThreshold = 0, // ResourcesHolder.Get().GetDefaultValueInt(BalancableKeys.vitaminRandomizationLevelThreshold_VALUE), //BundleManager.VitaminRandomizationLevelThresholdDefault,
            EndlessVitaminRandomization = false,
            minExponentialParameterA = 0, // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterA_VALUE_MIN), //BundleManager.minExponentialParameterADefault,
            maxExponentialParameterA = 0, // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterA_VALUE_MAX), //BundleManager.maxExponentialParameterADefault,
            minExponentialParameterB = 0, // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterB_VALUE_MIN),  //BundleManager.minExponentialParameterBDefault,
            maxExponentialParameterB = 0, // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.exponentialParameterB_VALUE_MAX), //BundleManager.maxExponentialParameterBDefault,
            MaternityWardFeatureEnabledAndroid = true,
            MaternityWardFeatureEnabledIOS = true,
            HospitalExpToMaternityExpConverterFactor = 0, // ResourcesHolder.Get().GetDefaultValueFloat(BalancableKeys.hospitalExpToMaternityExp_CONVERSION), //BundleManager.HospitalExpToMaternityExpConverterFactor,
            ShowIntro = true //Used to be 50% of players true and 50% false
        };
        return _cachedConfigCData;
    }

    //Standard Event config was SE_Config_4102019 in DDNA
    public static StandardEventsCData GetStandardEventsCData()
    {
        if (_cachedStandardEventsCData != null) return _cachedStandardEventsCData;
        _cachedStandardEventsCData = new StandardEventsCData()
        {
            Key = "SE_Config_4102019",
            MinVersion = "1.1.85",
            MaxVersion = "-",
            LenghtInWeeks = 7,
            StartTime = "0 0 1 10 * 2019",
            Events = new List<SingleStandardEventCData>()
            {
                new SingleStandardEventCData()
                {
                    EventUniqueID = "SpeedUpProduction",
                    EventDuration = 259200,
                    WeekNumber = 0,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_1",
                    Effects = new List<EventEffectCData> 
                    { 
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "medicineProductionTime_FACTOR",
                            Parameter = "0.5"
                        } 
                    }
                },
                new SingleStandardEventCData()
                {
                    EventUniqueID = "FreeDiagnosticStations",
                    EventDuration = 259200,
                    WeekNumber = 1,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_2",
                    Effects = new List<EventEffectCData>
                    {
                        new EventEffectCData()
                        {
                            MinLevel = 14,
                            Type = "costOfDiagnosis_FACTOR",
                            Parameter = "0"
                        }
                    }
                },
                new SingleStandardEventCData()
                {
                    EventUniqueID = "DoubleCoinsWeekend",
                    EventDuration = 259200,
                    WeekNumber = 2,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_3",
                    Effects = new List<EventEffectCData>
                    {
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "coinsForTreatmentRooms_FACTOR",
                            Parameter = "2"
                        },
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "coinsForDoctorRooms_FACTOR",
                            Parameter = "2"
                        }
                    }
                },
                new SingleStandardEventCData()
                {
                    EventUniqueID = "DoubleExpWeekend",
                    EventDuration = 259200,
                    WeekNumber = 3,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_4",
                    Effects = new List<EventEffectCData>
                    {
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "expForDoctors_FACTOR",
                            Parameter = "2"
                        },
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "expForTreatmentRooms_FACTOR",
                            Parameter = "2"
                        }
                    }
                },
                new SingleStandardEventCData()
                {
                    EventUniqueID = "BBEvent",
                    EventDuration = 259200,
                    WeekNumber = 4,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_5",
                    Effects = new List<EventEffectCData>
                    {
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "nextFreeBubbleBoy_TIME_INTERVAL",
                            Parameter = "0.01875"
                        }
                    }
                },
                new SingleStandardEventCData()
                {
                    EventUniqueID = "TreasureHunt",
                    EventDuration = 259200,
                    WeekNumber = 5,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_6",
                    Effects = new List<EventEffectCData>
                    {
                        new EventEffectCData()
                        {
                            MinLevel = 10,
                            Type = "treasureChestInterval_FACTOR",
                            Parameter = "0.01875"
                        }
                    }
                },
                new SingleStandardEventCData()
                {
                    EventUniqueID = "ShovelUp",
                    EventDuration = 259200,
                    WeekNumber = 6,
                    StartTimeCron = "0 12 * * 5",
                    ArtDecPoint = "custom_event_image_7",
                    Effects = new List<EventEffectCData>
                    {
                        new EventEffectCData()
                        {
                            MinLevel = 15,
                            Type = "shovelDrawChanceForHelpInGarden_FACTOR",
                            Parameter = "1"
                        }
                    }
                }
            }
        };
        return _cachedStandardEventsCData;
    }

    //Global Event config was GE_21GE in DDNA
    public static GlobalEventsCData GetGlobalEventsCData()
    {
        if (_cachedGlobalEventsCData != null) return _cachedGlobalEventsCData;
        _cachedGlobalEventsCData = new GlobalEventsCData()
        {
            Key = "GE_21GE",
            LenghtInWeeks = 3,
            StartTime = "0 0 1 1 * 2019",
            RewardsRanges = { 10, 15, 25, 50 },
            LeaderboardLength = 120,
            Events = new List<SingleGlobalEventCData>
            {
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "BaseElixir(1)",
                        SpawnInterval = 0,
                        MapSprite = "-",
                        MainActivitySprite = "-",
                        MaxObjectsOnMap = 0,
                        ParticleSprite = "-",
                        IconActivitySprite = "-"
                    },
                    StartTimeCron = "0 0 * * 1",
                    LastReward = "coin;25|25||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 0,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;1|1||1#coin"},
                            Goal = 20,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;1|1||1#coin"},
                            Goal = 50,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;1|1||1#coin"},
                            Goal = 100,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;1|1||1#coin"},
                            Goal = 250,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;1|1||1#coin"},
                            Goal = 500,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "Syrop(5)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 2",
                    LastReward = "coin;25|25||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 0,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;29|29||1#coin"},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;29|29||1#coin"},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"}
                        ,
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;29|29||1#coin"},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;29|29||1#coin"},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;29|29||1#coin"},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CURE_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "2xBedsRoom", 
                        SpawnInterval = 0, 
                        MapSprite = "-", 
                        MainActivitySprite = "-", 
                        MaxObjectsOnMap = 0, 
                        ParticleSprite = "-", 
                        IconActivitySprite = "-"
                    }, 
                    StartTimeCron = "0 0 * * 3", 
                    LastReward = "coin;25|25||1#coin", 
                    MinLevel = 10, 
                    Type = "CurePatientActivityGlobalEvent", 
                    EventDuration = 86000, 
                    SecondPlaceReward = "bundle;1|1||1@goodieBox", 
                    FirstPlaceReward = "bundle;1|1||1@specialBox", 
                    WeekNumber = 0, 
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CURE"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "AdvancedElixir(0)", 
                        SpawnInterval = 0, 
                        MapSprite =  "-", 
                        MainActivitySprite =  "-", 
                        MaxObjectsOnMap =  0, 
                        ParticleSprite =  "-", 
                        IconActivitySprite =  "-"
                    }, 
                    StartTimeCron = "0 0 * * 4", 
                    LastReward = "coin;25|25||1#coin", 
                    MinLevel = 10, 
                    Type = "MedicineContributeGlobalEvent", 
                    EventDuration = 86000, 
                    SecondPlaceReward = "bundle;1|1||1@goodieBox", 
                    FirstPlaceReward = "bundle;1|1||1@specialBox", 
                    WeekNumber = 0, 
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 20,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 50,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 100,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 250,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 500,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {                    
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "BlueDoc", 
                        SpawnInterval = 0, 
                        MapSprite =  "-", 
                        MainActivitySprite =  "-", 
                        MaxObjectsOnMap =  0, 
                        ParticleSprite =  "-", 
                        IconActivitySprite =  "-"
                    }, 
                    StartTimeCron = "0 0 * * 5", 
                    LastReward = "coin;35|35||1#coin", 
                    MinLevel = 10, 
                    Type = "CurePatientActivityGlobalEvent", 
                    EventDuration = 86000, 
                    SecondPlaceReward = "bundle;1|1||1@goodieBox", 
                    FirstPlaceReward = "bundle;1|1||1@specialBox", 
                    WeekNumber = 0, 
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },  
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "BaseElixir(11)", 
                        SpawnInterval = 0, 
                        MapSprite =  "-", 
                        MainActivitySprite =  "-", 
                        MaxObjectsOnMap =  0, 
                        ParticleSprite =  "-", 
                        IconActivitySprite =  "-"
                    }, 
                    StartTimeCron = "0 0 * * 6", 
                    LastReward = "coin;35|35||1#coin", 
                    MinLevel = 10, 
                    Type = "MedicineContributeGlobalEvent", 
                    EventDuration = 86000, 
                    SecondPlaceReward = "bundle;1|1||1@goodieBox", 
                    FirstPlaceReward = "bundle;1|1||1@specialBox", 
                    WeekNumber = 0, 
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 20,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 50,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 100,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 250,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 500,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "YellowDoc", 
                        SpawnInterval = 0, 
                        MapSprite =  "-", 
                        MainActivitySprite =  "-", 
                        MaxObjectsOnMap =  0, 
                        ParticleSprite =  "-", 
                        IconActivitySprite =  "-"
                    }, 
                    StartTimeCron = "0 0 * * 7", 
                    LastReward = "coin;35|35||1#coin", 
                    MinLevel = 10, 
                    Type = "CurePatientActivityGlobalEvent", 
                    EventDuration = 86000, 
                    SecondPlaceReward = "bundle;1|1||1@goodieBox", 
                    FirstPlaceReward = "bundle;1|1||1@specialBox", 
                    WeekNumber = 0, 
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"},
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                         RotatableTag = "RedDoc",
                         SpawnInterval = 0,
                         MapSprite =  "-",
                         MainActivitySprite =  "-",
                         MaxObjectsOnMap =  0,
                         ParticleSprite =  "-",
                         IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 1",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "BaseElixir(2)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 2",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;8|8||1#coin"},
                            Goal = 10,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;8|8||1#coin"},
                            Goal = 25,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;8|8||1#coin"},
                            Goal = 50,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;8|8||1#coin"},
                            Goal = 100,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;8|8||1#coin"},
                            Goal = 250,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "WhiteDoc",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 3",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "GreenDoc",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 4",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "BaseElixir(10)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 5",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;3|3||1#coin"},
                            Goal = 20,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;3|3||1#coin"},
                            Goal = 50,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;3|3||1#coin"},
                            Goal = 100,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {"coin;3|3||1#coin"},
                            Goal = 250,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;3|3||1#coin"},
                            Goal = 500,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "SunnyDoc",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 6",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic" 
                        }
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "Syrop(1)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 7",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 1,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;6|6||1#coin"},
                            Goal = 10,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;6|6||1#coin"},
                            Goal = 25,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;6|6||1#coin"},
                            Goal = 50,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;6|6||1#coin"},
                            Goal = 100,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;6|6||1#coin"},
                            Goal = 250,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        } 
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "Pill(2)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 1",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;28|28||1#coin"},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;28|28||1#coin"},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;28|28||1#coin"},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;28|28||1#coin"},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;28|28||1#coin"},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        } 
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "PinkDoc",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 2",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        } 
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "BaseElixir(7)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 3",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 20,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 50,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 100,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 250,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;2|2||1#coin"},
                            Goal = 500,
                            Reward = "medicine;3|3||1#RS#dynamic" 
                        }
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "SkyDoc",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 4",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic" 
                        } 
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "Syrop(3)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 5",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;20|20||1#coin"},
                            Goal = 5,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;20|20||1#coin"},
                            Goal = 10,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {"coin;20|20||1#coin"},
                            Goal = 20,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;20|20||1#coin"},
                            Goal = 50,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;20|20||1#coin"},
                            Goal = 100,
                            Reward = "medicine;3|3||1#RS#dynamic" 
                        } 
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1_TITLE",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "Pill(3)",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 6",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 11,
                    Type = "MedicineContributeGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;56|56||1#coin"},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {"coin;56|56||1#coin"},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;56|56||1#coin"},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {"coin;56|56||1#coin"},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {"coin;56|56||1#coin"},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        } 
                    },

                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CONTRIBUTE_1"
                },
                new SingleGlobalEventCData()
                {
                    TitleTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_TITLE_1",
                    OtherParams = new GlobalEventOtherParamsCData()
                    {
                        Medicine = "-",
                        RotatableTag = "PurpleDoc",
                        SpawnInterval = 0,
                        MapSprite =  "-",
                        MainActivitySprite =  "-",
                        MaxObjectsOnMap =  0,
                        ParticleSprite =  "-",
                        IconActivitySprite =  "-"
                    },
                    StartTimeCron = "0 0 * * 7",
                    LastReward = "coin;35|35||1#coin",
                    MinLevel = 10,
                    Type = "CurePatientActivityGlobalEvent",
                    EventDuration = 86000,
                    SecondPlaceReward = "bundle;1|1||1@goodieBox",
                    FirstPlaceReward = "bundle;1|1||1@specialBox",
                    WeekNumber = 2,
                    ThirdPlaceReward = "medicine;3|3||1#RS#dynamic",
                    Goals = new List<GlobalEventGoalCData>()
                    {
                        new GlobalEventGoalCData() 
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 2,
                            Reward = "exp;20|20||1#exp"
                        },
                        new GlobalEventGoalCData()
                        { 
                            ContributionRewards = new string[] {},
                            Goal = 5,
                            Reward = "exp;50|50||1#exp"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 10,
                            Reward = "medicine;1|1||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData()
                        {
                            ContributionRewards = new string[] {},
                            Goal = 25,
                            Reward = "medicine;2|2||1#RS#dynamic"
                        },
                        new GlobalEventGoalCData() 
                        {
                            ContributionRewards = new string[] {},
                            Goal = 50,
                            Reward = "medicine;3|3||1#RS#dynamic"
                        }
                    },
                    DescriptionTerm = "EVENTS/EVENT_DESCRIPTION_CUREDOCTOR_1"
                }
            }
        };
          
        return _cachedGlobalEventsCData;
    }

    public static void LoadConfigurations()
    {
        //TODO check if developer mocks requested  if (!DeveloperParametersController.Instance().parameters.UseMockedDailyRewards), etc
        
        CampaignController.Instance.ResetCampaignController();



        BalancableConfig.InitializeBalancable(new BalanceCData()
        {
            positiveEnergyRewardForHelpInTreatment_MIN = 0,
            positiveEnergyRewardForHelpInTreatment_MAX = 2,
            expForGardenHelp_VALUE = 15,
            shovelDraw_CHANCE = 0.2f,
            shovelRewardForHelping_CHANCE = 0.2f,
            patientToDiagnose_CHANCE = 0.5f,
            patientWithBacteria_CHANCE = 0.07f,
            //patientRequiresVitamin_CHANCE = 0.8f,
            patientVitaminRequired_CHANCE = 0.8f,
            spawnKid_CHANCE = 0.3f,
            nextEpidemy_TIME_INTERVAL = 14400,
            nextVIPArrive_TIME_INTERVAL = 10800,
            vipCure_TIME = 10800,
            nextFreeBubbleBoy_TIME_INTERVAL = 28800,
            treasureChestSpawn_TIME_INTERVAL = 300,
            bacteriaSpread_0_TIME_INTERVAL = 86400,
            bacteriaSpread_1_TIME_INTERVAL = 64800,
            bacteriaSpread_2_TIME_INTERVAL = 57600,
            bacteriaReward_0_VALUE = 10,
            bacteriaReward_1_VALUE = 20,
            bacteriaReward_2_VALUE = 30,
            diamondsInTreasureChestAfterIAP_VALUE = 5,
            storageToolsRange_MIN = 90,
            storageToolsRange_MAX = 110,
            tankToolsRange_MIN = 90,
            tankToolsRange_MAX = 110,
            maxGiftsPerDay_VALUE = 5,
            //rewardForTODOsCoinsMultiplikator_VALUE = 1,
            rewardForTODOSCoinsMultiplikator_VALUE = 1,
            rewardForTODOsDiamonds_VALUE = 5,
            maternityTimeToSpawnNextMother_MIN = 60,
            maternityTimeToSpawnNextMother_MAX = 360,
            maternityBloodTestCoinsCost_VALUE = 450,
            maternityLevelStageDifficultyChange_VALUE = 5,
            maternityBloodTestDuration_TIME_INTERVAL = 3600,
            maternityLaborStageEasyDuration_TIME_INTERVAL_MIN = 14400,
            maternityLaborStageEasyDuration_TIME_INTERVAL_MAX = 21600,
            maternityLaborStageHardDuration_TIME_INTERVAL_MIN = 21600,
            maternityLaborStageHardDuration_TIME_INTERVAL_MAX = 36000,
            maternityBondingDuration_TIME_INTERVAL_MIN = 79200,
            maternityBondingDuration_TIME_INTERVAL_MAX = 93600,
            vitaminRandomizationLevelThreshold_VALUE = 15,
            diamondSpendLimitToShowConfirmation_VALUE = 0,
            helpInTreatmentRoomCounter_VALUE_MAX = 3,
            helpInTreatmentRoomRequestCooldown_TIME_INTERVAL = 86400,
            helpInTreatmentRoomPushCooldown_TIME_INTERVAL = 86400,
            helpInTreatmentRoomLevelToUnlock_MIN = 8,
            friendsDrawerItems_VALUE_MAX = 50,
            ktPlayUnlockLevel_MIN = 1,
            maternityExpToRealtimeSeconds_CONVERSION = 900,
            hospitalExpToMaternityExp_CONVERSION = 0.65f,
            maternityLaborStageHard_CHANCE = 0.07f,
            exponentialParameterA_VALUE_MIN = 1476.6f,
            exponentialParameterA_VALUE_MAX = 3014.5f,
            exponentialParameterB_VALUE_MIN = 0.198f,
            exponentialParameterB_VALUE_MAX = 0.1775f,
            shovelDrawFromGoodieBox1_CHANCE = 0.5f,
            shovelDrawFromGoodieBox2_CHANCE = 0.5f,
            shovelDrawFromGoodieBox3_CHANCE = 0.25f,
            shovelDrawFromVIP_CHANCE = 0.5f,
            shovelDrawFromEpidemyChest_CHANCE = 0.25f,
            shovelDrawFromTreasureChest_CHANCE = 0.5f,
            bonusMedicineProductionForWatchingAd_PERCENT = 0.2f, //Is value correct?
            rewardAdMedicineProductionButtonCooldown_TIME_INTERVAL = 30 //Is value correct?
        });

        List<BundleRewardCData> bundleRewardList = new List<BundleRewardCData>();
 
        bundleRewardList.Add(new BundleRewardCData()
        {
            BundledRewardType = Hospital.BundledRewardTypes.superAwesomeBox,
            Value = "SUPER_AWESOME_BOX@pinkBox@diamond;50|100||1#diamond1@coin;300|500||1#coin1@booster;1|1||0.05#RPB#dynamic@booster;1|3||0.08#RSB#dynamic@positiveEnergy;5|15||0.5#positiveEnergy1@medicine;2|2||0.5#Special(3)#dynamic@medicine;1|3||0.20#RST#dynamic@medicine;1|3||0.20#RTT#dynamic"
        });
        bundleRewardList.Add(new BundleRewardCData()
        {
            BundledRewardType = Hospital.BundledRewardTypes.goodieBox,
            Value = "GIFT_BOXES_NAME_1@goodieBox@coin;10|40||1#coin1@medicine;1|1||1#RS#dynamic"
        });
        bundleRewardList.Add(new BundleRewardCData()
        {
            BundledRewardType = Hospital.BundledRewardTypes.awesomeBox,
            Value = "AWESOME_BOX@dailyRewardBoxWeek1@diamond;30|60||1#diamond1@coin;50|150||1#coin1@booster;1|3||0.08#RSB#dynamic@booster;1|1||0.05#RPB#dynamic@positiveEnergy;5|15||0.5#positiveEnergy1@medicine;2|2||0.5#Special(3)#dynamic@medicine;1|3||0.2#RST#dynamic@medicine;1|3||0.2#RTT#dynamic"
        });
        bundleRewardList.Add(new BundleRewardCData()
        {
            BundledRewardType = Hospital.BundledRewardTypes.premiumBox,
            Value = "GIFT_BOXES_NAME_3@premiumBox@coin;750|1250||1#coin1@medicine;1|1||1#RS#dynamic@medicine;1|1||1#RS#dynamic@medicine;1|1||1#RS#dynamic@medicine;1|1||1#RS#dynamic@booster;1|1||1#RSB#dynamic@decoration;1|1||1#R#dynamic"
        });
        bundleRewardList.Add(new BundleRewardCData()
        {
            BundledRewardType = Hospital.BundledRewardTypes.specialBox,
            Value = "GIFT_BOXES_NAME_2@specialBox@coin;100|250||1#coin1@medicine;1|1||1#RS#dynamic@medicine;1|1||1#RS#dynamic@booster;1|1||0.1#RSB#dynamic@decoration;1|1||1#R#dynamic"
        });
        Hospital.BundledRewardDefinitionConfig.InstantiateConfig(bundleRewardList);

        List<CaseTierRewardCData> caseTierRewardCDatas = new List<CaseTierRewardCData>();
        caseTierRewardCDatas.Add(new CaseTierRewardCData()
        {
            Tier = CasePrizeDeltaConfig.Tiers.tier1,
            Value = "bundle;1|1||1@goodieBox"
        });
        caseTierRewardCDatas.Add(new CaseTierRewardCData()
        {
            Tier = CasePrizeDeltaConfig.Tiers.tier2,
            Value = "bundle;1|1||1@specialBox"
        });
        caseTierRewardCDatas.Add(new CaseTierRewardCData()
        {
            Tier = CasePrizeDeltaConfig.Tiers.tier3,
            Value = "bundle;1|1||1@premiumBox"
        });
        CasePrizeDeltaConfig.Initialize(caseTierRewardCDatas);

        TresureChestDeltaConfig.SetupDiamondFromTreasureChest(false);
 
        //Standard Event config was SE_Config_4102019 in DDNA
        StandardEventConfig.InitializeEvent(GetStandardEventsCData());

        //FakedContributionConfig.InstantiateConfig(parameters); nothing was in DDNA

        LevelUpGiftsConfig.InstantiateConfig(new GiftsForLevelCData()
        {
            parameters = new Dictionary<string, object>()
            {
                {"def2","30%9999%bundle;1|1||1@awesomeBox"},
                {"def0","35%P5%bundle;1|1||1@superAwesomeBox"},
                {"def1","20%29%bundle;1|1||1@specialBox"}
            }
        });

        IAPShopConfig.InitializeBundles(new IAPShopBundlesCData()
        {
            parameters = new Dictionary<string, string>()
            {
                {"bundleSpecialPack","1;-1;65;green" },
                {"bundleTapjoy","-1;2;0;yellow" },
                {"bundleSuperBundle4","-1;3;380" },
                {"bundlePositiveEnergy50","-1;-1;60" },
                {"bundleShovels9","2;-1;48" },
                {"bundleBreastCancerDeal","-1;-1;0" },
            }
        });
        IAPShopConfig.InitializeCoinsPackages(new IAPShopCoinsCData()
        {
            parameters = new Dictionary<string, string>()
            {
                {"packOfCoins2","3;100;3.16667;coins2"},
                {"packOfCoins5","6;900;41;coins5"},
                {"packOfCoins4","5;650;19.5;coins4"},
                {"packOfCoins1","2;70;1;coins1"},
                {"packOfCoins3","4;200;8;coins3"},
                {"packOfCoins6","7;2300;132.33334;coins6"},
                {"packOfCoinsForVideo","1;0;0"}
            }
        });
        IAPShopConfig.InitializeSections(new IAPShopOrderCData()
        {
            parameters = new Dictionary<string, int>()
            {
                {"sectionFeatures",1},
                {"sectionSpecialOffers",4},
                {"sectionCoins",3},
                {"sectionDiamonds",2}
            }
        });

        //TODO create timed offers 
        //Key: offer_1 Value: timedOffer_dec1;1;1669507199;1    
        //TimedOffersDeltaConfig.Initialize(respons, parameters);

        //TODO why this was in DDNA controller?
        CampaignController.Instance.AddCampaignId("ObjectivesDynamic");

        //TODO
        //Was only TNT crosspromo bool
        //CrossPromotionController.instance.InitializeCampaign(parameters);
    }
}

public class StandardEventsCData
{
    public string Key { get; set; }
    public string MinVersion { get; set; }
    public string MaxVersion { get; set; }
    public int LenghtInWeeks { get; set; }
    public string StartTime { get; set; } //cron format
    public List<SingleStandardEventCData> Events { get; set; }
}
public class SingleStandardEventCData
{
    public string EventUniqueID { get; set; }
    public int EventDuration { get; set; }
    public int WeekNumber { get; set; }
    public string StartTimeCron { get; set; }
    public string ArtDecPoint { get; set; }
    public List<EventEffectCData> Effects { get; set; }


    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class EventEffectCData
{
    public int MinLevel { get; set; }
    public string Type { get; set; }
    public string Parameter { get; set; }
}

public class GlobalEventsCData
{
    public string Key { get; set; }
    public int LenghtInWeeks { get; set; }
    public string StartTime { get; set; } //cron format
    public List<int> RewardsRanges { get; set; }
    public int LeaderboardLength { get; set; }
    public List<SingleGlobalEventCData> Events { get; set; }
}

public class SingleGlobalEventCData
{
    public string Type { get; set; }
    public int EventDuration { get; set; }
    public int WeekNumber { get; set; }
    public string StartTimeCron { get; set; }
    public int MinLevel { get; set; }
    public string FirstPlaceReward { get; set; }
    public string SecondPlaceReward { get; set; }
    public string ThirdPlaceReward { get; set; }
    public string LastReward { get; set; }
    public string TitleTerm { get; set; }
    public string DescriptionTerm { get; set; }
    public List<GlobalEventGoalCData> Goals { get; set; }
    public GlobalEventOtherParamsCData OtherParams { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
public class GlobalEventGoalCData
{
    public int Goal { get; set; }
    public string Reward { get; set; }
    public string[] ContributionRewards { get; set; }
}

public class GlobalEventOtherParamsCData
{
    public int MaxObjectsOnMap { get; set; }
    public int SpawnInterval { get; set; }
    public string MainActivitySprite { get; set; }
    public string MapSprite { get; set; }
    public string ParticleSprite { get; set; }
    public string IconActivitySprite { get; set; }
    public string Medicine { get; set; }
    public string RotatableTag { get; set; }
}

public class GeneralCData
{
    public string versionURL = "";
    public string supportEmail = "";
    public string TermsOfServiceURL = "";
    public string PrivacyPolicyURL = "";
    public string FacebookFanpageURL = "";
    public string InstagramURL = "";
    public string YouTubeURL = "";
}

public class AdPointCData
{
    public long AdCooldownSeconds; 
    public long AdDailyLimit;
    //Supported adpoints:
    //rewarded_ad_billboard
    //rewarded_ad_bubbleboy
    //rewarded_ad_dailyquest
    //rewarded_ad_coins
    //rewarded_ad_vitamin_collector
    //rewarded_ad_medicine_production
    //rewarded_ad_missed_daily_reward
    public string AdPoint; 
    public long AdRewardAmount;
    public long AdSessionLimit;
    public int AdRewardMultiplier;
}
public class BundleRewardCData
{
    public Hospital.BundledRewardTypes BundledRewardType;
    //TODO refactor
    public string Value;
}

public class CaseTierRewardCData
{
    public CasePrizeDeltaConfig.Tiers Tier;
    //TODO refactor
    public string Value;
}

public class DailyRewardsCData
{
    //TODO refactor
    public string defaultWeekDailyRewardConfig;
    //TODO refactor
    public string defaultIncrementValueDailyRewardConfig;
}

public class GiftsForLevelCData
{
    //TODO refactor
    public Dictionary<string, object> parameters;
}

public class IAPShopCoinsCData
{
    //TODO refactor
    public Dictionary<string, string> parameters;
}
public class IAPShopBundlesCData
{
    //TODO refactor
    public Dictionary<string, string> parameters;
}
public class IAPShopOrderCData
{
    //TODO refactor
    public Dictionary<string, int> parameters;
}

public class BalanceCData
{
    public int rewardAdMedicineProductionButtonCooldown_TIME_INTERVAL; //Was missing Typo?
    public float bonusMedicineProductionForWatchingAd_PERCENT; //Was missing Typo?
    public int positiveEnergyRewardForHelpInTreatment_MIN;
    public int positiveEnergyRewardForHelpInTreatment_MAX;
    public int expForGardenHelp_VALUE;
    public float shovelDraw_CHANCE;
    public float shovelRewardForHelping_CHANCE;
    public float patientToDiagnose_CHANCE;
    public float patientWithBacteria_CHANCE;
    //public float patientRequiresVitamin_CHANCE; Typo or other config?
    public float patientVitaminRequired_CHANCE;
    public float spawnKid_CHANCE;
    public int nextEpidemy_TIME_INTERVAL;
    public int nextVIPArrive_TIME_INTERVAL;
    public int vipCure_TIME;
    public int nextFreeBubbleBoy_TIME_INTERVAL;
    public int treasureChestSpawn_TIME_INTERVAL;
    public int bacteriaSpread_0_TIME_INTERVAL;
    public int bacteriaSpread_1_TIME_INTERVAL;
    public int bacteriaSpread_2_TIME_INTERVAL;
    public int bacteriaReward_0_VALUE;
    public int bacteriaReward_1_VALUE;
    public int bacteriaReward_2_VALUE;
    public int diamondsInTreasureChestAfterIAP_VALUE;
    public int storageToolsRange_MIN;
    public int storageToolsRange_MAX;
    public int tankToolsRange_MIN;
    public int tankToolsRange_MAX;
    public int maxGiftsPerDay_VALUE;
    public float rewardForTODOSCoinsMultiplikator_VALUE;
    //public float rewardForTODOsCoinsMultiplikator_VALUE; // TYPO
    public float rewardForTODOsDiamonds_VALUE;
    public int maternityTimeToSpawnNextMother_MIN;
    public int maternityTimeToSpawnNextMother_MAX;
    public int maternityBloodTestCoinsCost_VALUE;
    public int maternityLevelStageDifficultyChange_VALUE;
    public int maternityBloodTestDuration_TIME_INTERVAL;
    public int maternityLaborStageEasyDuration_TIME_INTERVAL_MIN;
    public int maternityLaborStageEasyDuration_TIME_INTERVAL_MAX;
    public int maternityLaborStageHardDuration_TIME_INTERVAL_MIN;
    public int maternityLaborStageHardDuration_TIME_INTERVAL_MAX;
    public int maternityBondingDuration_TIME_INTERVAL_MIN;
    public int maternityBondingDuration_TIME_INTERVAL_MAX;
    public int vitaminRandomizationLevelThreshold_VALUE;
    public int diamondSpendLimitToShowConfirmation_VALUE;
    public int helpInTreatmentRoomCounter_VALUE_MAX;
    public int helpInTreatmentRoomRequestCooldown_TIME_INTERVAL;
    public int helpInTreatmentRoomPushCooldown_TIME_INTERVAL;
    public int helpInTreatmentRoomLevelToUnlock_MIN;
    public int friendsDrawerItems_VALUE_MAX;
    public int ktPlayUnlockLevel_MIN;
    public float maternityExpToRealtimeSeconds_CONVERSION;
    public float hospitalExpToMaternityExp_CONVERSION;
    public float maternityLaborStageHard_CHANCE;
    public float exponentialParameterA_VALUE_MIN;
    public float exponentialParameterA_VALUE_MAX;
    public float exponentialParameterB_VALUE_MIN;
    public float exponentialParameterB_VALUE_MAX;
    public float shovelDrawFromGoodieBox1_CHANCE;
    public float shovelDrawFromGoodieBox2_CHANCE;
    public float shovelDrawFromGoodieBox3_CHANCE;
    public float shovelDrawFromVIP_CHANCE;
    public float shovelDrawFromEpidemyChest_CHANCE;
    public float shovelDrawFromTreasureChest_CHANCE;
}
public class ConfigCData
{
    public string Key;    
    public string Value;    //Used
    public string ValueAndroid;   //Used 
    public bool Maintenance;    //Used
    public bool MaintenanceAndroid;     //Used
    public bool EnableResetHospitalFix;    //Used
    public bool ShowIntro; //Used
    public bool AdsAlwaysReward;    
    public List<int> bacteriaRewards;    
    public int AdsIntervalOnStand;    
    public List<int> bacteriaInfectionTime;    
    public Dictionary<string, string> EventsAndBalance_v2;    
    public Dictionary<string, string> SuperBundles;    
    public Dictionary<string, string> GameAssetBundles;    //Used
    public Dictionary<string, int> ObjectivesDocParam;    //Used
    public Dictionary<string, int> BoostersPrice;    //Used
    public List<int> PackageIntervals;    //Used
    public string DailyDealConfig;    //Used
    public Dictionary<string, string> Campaigns;    //Used
    public Dictionary<string, string> GlobalEvents;    
    public Dictionary<string, string> GameEvents;   //Used
    public int GlobalOffersInitSlotsCount;    //Used
    public int GlobalOffersMaxSlotsCount;    //Used
    public bool GlobalOffersCheckingOfferState;    //Used
    public int FriendsOffersInitSlotsCount;    //Used
    public int FriendsOffersMaxSlotsCount;    //Used
    public int GlobalOffersParamA;    //Used
    public int GlobalOffersParamC;    //Used
    public float GlobalOffersParamX;    //Used
    public bool TapjoyEnabled;    
    public int GiftsRefreshIntervalInSeconds;    //Used
    public int GiftsToSendRefreshIntervalInSeconds;    //Used
    public int GiftsMax;    //Used
    public int GiftsFeatureMinLevel;    //Used
    public string GiftRewardGeneratorData;    //Used
    public int AddWiseGiftIntervalInHours;    //Used
    public int CooldownToSendGiftToSpecificFriendInMinutes; //Used
    public Dictionary<string, float> ObjectiveRewardFactor1;    //Used
    public Dictionary<string, float> ObjectiveProgressForCurePatientInDoctorFactor1;    //Used
    public Dictionary<string, float> ObjectiveProgressForCurePatientInHospitalRoomFactor1;   //Used 
    public Dictionary<string, float> ObjectiveProgressForCureKidstInDoctorFactor1;    //Used
    public Dictionary<string, float> ObjectiveProgressForDiagnosePatientFactor1;    //Used
    public Dictionary<string, string> MasterySystemConfig;    //Used
    public int MastershipMinLevel;    //Used
    public bool AddAdsRewardOnAdOpen;    
    public bool AddAdsRewardFromSaveLogic;    //Used
    public float ShowelChanceFromGoodieBox1;    
    public float ShowelChanceFromGoodieBox2;    
    public float ShowelChanceFromGoodieBox3;    
    public float ShowelChanceFromVIP;    
    public float ShowelChanceFromEpidemyBox;    
    public float ShowelChanceFromTreasureChest;    
    public int DiamondAmountPerLevelUpAfter50;    //Used
    public float GoldFactorForLevelUpRewardAfter50;    //Used
    public int FriendsDrawerItemsLimit;        
    public bool FBAdsEnabled;    
    public bool PreloaderBeforeAdsEnabled;    //Used
    public string BreastCancerFoundationUrl;    //Used
    public int HelpInTreatmentRoomRequestCooldown;    
    public int HelpInTreatmentRoomPushCooldown;    
    public int HelpInTreatmentRoomRequestMaxCounter;    
    public int HelpInTreatmentRoomFeatureMinLevel;    
    public bool TreatmentHelpSummaryPopupEnabled;    //Used
    public bool ParticlesInGameEnabled;     //Used
    public bool ParticlesInGameEnabledAndroid;     //Used
    public Dictionary<string, int> CheaterScoreConditions;    
    public Dictionary<string, int> VitaminsProductionTime;    //Used
    public bool OpenShopAlwaysOnTop;    //Used
    public Dictionary<string, int> VitaminsCollectStep;    //Used
    public Dictionary<string, int> VitaminsCollectorCapacity;    //Used
    public Dictionary<string, int> VitaminsDeltaCapacityUpgrade;    //Used
    public Dictionary<string, int> VitaminsDeltaFillRatioUpgrade;    //Used
    public Dictionary<string, List<float>> PositiveEnergyUpgradeEquationParameters;    //Used
    public Dictionary<string, List<float>> ToolsUpgradeEquationParameters;    //Used
    public int VipUnlockCost; //Used
    public string VipUnlockCostType; //Used    
    public int VipUnlockTime;    //Used
    public int minVitaminesToCure;    //Used
    public int maxVitaminesToCure;    //Used
    public bool ktPlayButtonEnabled;    
    public bool ktPlayLoggingEnabled;    //Used
    public int ktPlayButtonUnlockLevel;    
    public int SocialRequestTimeoutEPOCH;    //Used
    public int SocialFriendsPoolinSeconds;    //Used
    public int SocialFriendsLimit;    
    public int SocialRandomFriendsReloadTimeEPOCH;    //Used
    public int SocialRandomFriendsDisplayLimit;    //Used
    public int SocialServerRandomFriendsLevelSpreed;    
    public int SocialServerRandomFriendsQuerryLimit;    
    public string SocialRandomFriendsGetURL;    //Used
    public string SocialPersonalFriendCodeURL;    //Used
    public Dictionary<string, float> AssetBundleSizes;    //Used
    public int DiamondLimitToShowSpendConfirmation;
#region MaternityWard    
    public Dictionary<string, int> ItemsRequiredToUnlockMaternity; //Used
    public int MaternityMinTimeToNextMotherSpawn;
    public int OneMaternityExpCostInSec;
    public int MaternityMaxTimeToNextMotherSpawn;
    public int MaternityBloodTestCoinsCost;
    public int MaternityBloodTestTime;
    public float MaternityLabourStageHardChance;
    public int MaternityLevelStageDifficultyLevelChange;
    public int MaternityMinLaborStageEasy;
    public int MaternityMaxLaborStageEasy;
    public int MaternityMinLaborStageHard;
    public int MaternityMaxLaborStageHard;
    public int MaternityMinBondingTime;
    public int MaternityMaxBondingTime;
    public int VitaminRandomizationLevelThreshold;
    public bool EndlessVitaminRandomization; //Used
    public float minExponentialParameterA;
    public float maxExponentialParameterA;
    public float minExponentialParameterB;
    public float maxExponentialParameterB;
    public bool MaternityWardFeatureEnabledAndroid; //Used
    public bool MaternityWardFeatureEnabledIOS; //Used
    public float HospitalExpToMaternityExpConverterFactor;
    #endregion

    //Original always returned default
    public int GetValueModifiedByParam(BalancedParam.Key key, int toModify)
    {
        string dictionaryKey = Enum.GetName(typeof(BalancedParam.Key), key);
        int defaultPercent = 100;
        if (EventsAndBalance_v2.ContainsKey(dictionaryKey))
        {
            Debug.LogError("Not in correct format");
            if (!int.TryParse(EventsAndBalance_v2[dictionaryKey], out var valuePercent))
            {
                Debug.LogError("EventsAndBalance_v2 KEY: " + dictionaryKey + " Not in correct format");
            }
            return valuePercent * toModify / defaultPercent;
        }
        else
        {
            return defaultPercent * toModify / defaultPercent;
        }
    }

    //Original always returned default
    public float GetValueModifiedByParam(BalancedParam.Key key, float toModify) //Used
    {
        string dictionaryKey = Enum.GetName(typeof(BalancedParam.Key), key);
        int defaultPercent = 100;
        if (EventsAndBalance_v2.ContainsKey(dictionaryKey))
        {
            Debug.LogError("Not in correct format");
            if (!int.TryParse(EventsAndBalance_v2[dictionaryKey], out var valuePercent))
            {
                Debug.LogError("EventsAndBalance_v2 KEY: " + dictionaryKey + " Not in correct format");
            }
            return valuePercent * toModify / defaultPercent;
        }
        else
        {
            return defaultPercent * toModify / defaultPercent;
        }
    }

    public AlgorithmHolder.ObjectiveRewardFactorHolder GetObjectiveRewardFactor()
    {
        Dictionary<string, float> dic = ObjectiveRewardFactor1;
        AlgorithmHolder.ObjectiveRewardFactorHolder holder = new AlgorithmHolder.ObjectiveRewardFactorHolder();
        List<string> parameters = new List<string>()
        {
            "expParameterA",
            "expParameterB",
            "expAdder",
            "logParameterA",
            "logParameterB",
            "logAdder"
        };

        holder.expParameterA = dic.ContainsKey(parameters[0]) ? dic[parameters[0]] : holder.expParameterA;
        holder.expParameterB = dic.ContainsKey(parameters[1]) ? dic[parameters[1]] : holder.expParameterB;
        holder.expAdder = dic.ContainsKey(parameters[2]) ? dic[parameters[2]] : holder.expAdder;
        holder.logParameterA = dic.ContainsKey(parameters[3]) ? dic[parameters[3]] : holder.logParameterA;
        holder.logParameterB = dic.ContainsKey(parameters[4]) ? dic[parameters[4]] : holder.logParameterB;
        holder.logAdder = dic.ContainsKey(parameters[5]) ? dic[parameters[5]] : holder.logAdder;
        return holder;
    }
    public AlgorithmHolder.ObjectiveProgressForCurePatientInDoctorFactorHolder GetObjectiveProgressForCurePatientInDoctorFactor()
    {
        Dictionary<string, float> dic = ObjectiveProgressForCurePatientInDoctorFactor1;
        AlgorithmHolder.ObjectiveProgressForCurePatientInDoctorFactorHolder holder = new AlgorithmHolder.ObjectiveProgressForCurePatientInDoctorFactorHolder();
        List<string> parameters = new List<string>()
        {
            "power",
            "logBase",
            "adder",
            "anyDoctorFactor"
        };
        holder.power = dic.ContainsKey(parameters[0]) ? dic[parameters[0]] : holder.power;
        holder.logBase = dic.ContainsKey(parameters[1]) ? dic[parameters[1]] : holder.logBase;
        holder.adder = dic.ContainsKey(parameters[2]) ? dic[parameters[2]] : holder.adder;
        holder.anyDoctorFactor = dic.ContainsKey(parameters[3]) ? dic[parameters[3]] : holder.anyDoctorFactor;
        return holder;
    }
    public AlgorithmHolder.ObjectiveProgressForCurePatientInHospitalRoomFactorHolder GetObjectiveProgressForCurePatientInHospitalRoomFactor()
    {
        Dictionary<string, float> dic = ObjectiveProgressForCurePatientInHospitalRoomFactor1;
        AlgorithmHolder.ObjectiveProgressForCurePatientInHospitalRoomFactorHolder holder = new AlgorithmHolder.ObjectiveProgressForCurePatientInHospitalRoomFactorHolder();
        List<string> parameters = new List<string>()
        {
            "power",
            "logBase",
            "adder",
            "sexFactor",
            "deseaseFactor",
            "plantFactor",
            "bacteriaFactor",
            "vipFactor",
            "diagnosisFactor",
            "bloodFactor"
        };

        holder.power = dic.ContainsKey(parameters[0]) ? dic[parameters[0]] : holder.power;
        holder.logBase = dic.ContainsKey(parameters[1]) ? dic[parameters[1]] : holder.logBase;
        holder.adder = dic.ContainsKey(parameters[2]) ? dic[parameters[2]] : holder.adder;
        holder.sexFactor = dic.ContainsKey(parameters[3]) ? dic[parameters[3]] : holder.sexFactor;
        holder.deseaseFactor = dic.ContainsKey(parameters[4]) ? dic[parameters[4]] : holder.deseaseFactor;
        holder.plantFactor = dic.ContainsKey(parameters[5]) ? dic[parameters[5]] : holder.plantFactor;
        holder.bacteriaFactor = dic.ContainsKey(parameters[6]) ? dic[parameters[6]] : holder.bacteriaFactor;
        holder.vipFactor = dic.ContainsKey(parameters[7]) ? dic[parameters[7]] : holder.vipFactor;
        holder.diagnosisFactor = dic.ContainsKey(parameters[8]) ? dic[parameters[8]] : holder.diagnosisFactor;
        holder.bloodFactor = dic.ContainsKey(parameters[9]) ? dic[parameters[9]] : holder.bloodFactor;

        return holder;
    }
    public AlgorithmHolder.ObjectiveProgressForCureKidstInDoctorFactorHolder GetObjectiveProgressForCureKidstInDoctorFactor()
    {
        Dictionary<string, float> dic = ObjectiveProgressForCureKidstInDoctorFactor1;
        AlgorithmHolder.ObjectiveProgressForCureKidstInDoctorFactorHolder holder = new AlgorithmHolder.ObjectiveProgressForCureKidstInDoctorFactorHolder();
        List<string> parameters = new List<string>()
        {
            "power",
            "logBase",
            "adder"
        };

        holder.power = dic.ContainsKey(parameters[0]) ? dic[parameters[0]] : holder.power;
        holder.logBase = dic.ContainsKey(parameters[1]) ? dic[parameters[1]] : holder.logBase;
        holder.adder = dic.ContainsKey(parameters[2]) ? dic[parameters[2]] : holder.adder;
        return holder;
    }
    public AlgorithmHolder.ObjectiveProgressForDiagnosePatientFactorHolder GetObjectiveProgressForDiagnosePatientFactor()
    {
        Dictionary<string, float> dic = ObjectiveProgressForDiagnosePatientFactor1;
        AlgorithmHolder.ObjectiveProgressForDiagnosePatientFactorHolder holder = new AlgorithmHolder.ObjectiveProgressForDiagnosePatientFactorHolder();
        List<string> parameters = new List<string>()
        {
            "power",
            "logBase",
            "adder",
            "diagnoseAnyFactor"
        };

        holder.power = dic.ContainsKey(parameters[0]) ? dic[parameters[0]] : holder.power;
        holder.logBase = dic.ContainsKey(parameters[1]) ? dic[parameters[1]] : holder.logBase;
        holder.adder = dic.ContainsKey(parameters[2]) ? dic[parameters[2]] : holder.adder;
        holder.diagnoseAnyFactor = dic.ContainsKey(parameters[3]) ? dic[parameters[3]] : holder.diagnoseAnyFactor;
        return holder;
    }

    public string GetVersion()
    {
#if UNITY_ANDROID
        return ValueAndroid;
#else
        return Value;
#endif
    }
    public bool IsInMaintenanceMode() //Used
    {
#if UNITY_ANDROID
        return MaintenanceAndroid;
#else
        return Maintenance;
#endif
    }

    public bool IsMaternityWardFeatureEnabled() //Used
    {
#if UNITY_IOS
        return MaternityWardFeatureEnabledIOS;
#else
        return MaternityWardFeatureEnabledAndroid;
#endif
    }

    public bool IsParticlesInGameEnabled() //Used
    {
#if UNITY_IOS
        return ParticlesInGameEnabled;
#else
        return ParticlesInGameEnabledAndroid;
#endif
    }
}
