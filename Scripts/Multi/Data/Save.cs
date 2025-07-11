using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System;

namespace Hospital
{
    [Serializable]
    [DynamoDBTable("Saves")]
    public class Save
    {
        [DynamoDBHashKey]
        public string ID;

        [DynamoDBProperty]
        public string FacebookID;

        /// <summary>
        /// current Application version set in unity/xcode i.e 4.3.4
        /// </summary>
        [DynamoDBProperty]
        public string gameVersion;

        /// <summary>
        /// Made as a check for cheaters.
        /// the biggest seen Application version set in unity/xcode i.e 4.3.4
        /// </summary>
        [DynamoDBProperty]
        public string maxGameVersion;//

        /// <summary>
        /// Save Version
        /// </summary>
        [DynamoDBProperty]
        public string version;  //save version

        [DynamoDBProperty]
        public long lastBuyByWise = 0;

        /// <summary>
        /// Deprecated- because of multiple issues with saving parsing data into string
        /// </summary>
#pragma warning disable 0649
        [DynamoDBProperty]
        private string saveTime;
#pragma warning restore 0649
        /// <summary>
        /// Date in Unix time, as of 09 August 2017 is in use.
        /// Current Time of Save.
        /// </summary>
        [DynamoDBProperty]
        public long saveDateTime;

        [DynamoDBProperty]
        public long lastGameEventChestSpawnTime = -1;

        [DynamoDBProperty]
        public int gameEventChestsCount = 0;

        /// <summary>
        /// Time when last chest was spawned. -1=there is not such a time.
        /// </summary>

        /*[DynamoDBProperty]
        public long lastGlobalEventChestSpawnTime = -1;

        /// <summary>
        /// How many chests do we have spawned now on scene.
        /// </summary>
        [DynamoDBProperty]
        public int globalEventChestsCount = 0;

        /// <summary>
        /// ID of the Last Event that used chest mechanism.
        /// Defaults to "" if none was seen (yet) by the user.
        /// </summary>
        [DynamoDBProperty]
        public string lastGlobalEventChestsEventID = "";
         */

        [DynamoDBProperty]
        public int globalOffersSlotsCount = -1;

        [DynamoDBProperty]
        public int friendsOffersSlotsCount = -1;

        [DynamoDBProperty]
        public long lastPromotionOfferAdd = -1;

        [DynamoDBProperty]
        public long lastStandardOfferAdd = -1;

        /// <summary>
        /// Deprecated
        /// </summary>
        public DateTime GetSaveTime()
        {
            DateTime saveDateTime = DateTime.Now;
            try
            {
                saveDateTime = DateTime.Parse(saveTime);
            }
            catch (Exception) { }
            return saveDateTime;
        }

        [DynamoDBProperty]
        public string notificationData;

        [DynamoDBProperty]
        public int KidsToSpawn;

        [DynamoDBProperty]
        public long timedOfferEndDate;

        [DynamoDBProperty]
        public long timedOfferPurchaseDate;

        [DynamoDBProperty]
        public string HospitalName;

        [DynamoDBProperty]
        public int Level = 1;

        [DynamoDBProperty]
        public int Experience = 0;

        [DynamoDBProperty]
        public int CoinAmount = 0;

        [DynamoDBProperty]
        public int DiamondAmount = 0;

        [DynamoDBProperty]
        public int PositiveEnergyAmount = 0;

        //[DynamoDBProperty]
        //public int ActionsCounterForExtraGift = 0;

        [DynamoDBProperty]
        public int ActionsCounterForExtraGiftStorage = 0;

        [DynamoDBProperty]
        public int ActionsCounterForExtraGiftTank = 0;

        [DynamoDBProperty]
        public int AchievementsDone = 0;

        [DynamoDBProperty]
        public int ReceptionLevel = 0;

        [DynamoDBProperty]
        public bool ReceptionIsBusy = false;

        //[DynamoDBProperty]
        //public int EmergencyLevel = 0;

        [DynamoDBProperty]
        public int PlaygroundLevel = 0;

        [DynamoDBProperty]
        public int TutorialStep = 0;

        [DynamoDBProperty]
        public bool IsArrowAnimationNeededForWhiteElixir = false;

        [DynamoDBProperty]
        public string TutorialStepTag;

        [DynamoDBProperty]
        public List<string> ClinicObjectsData;

        [DynamoDBProperty]
        public bool ShowEmergencyIndicator = false;

        [DynamoDBProperty]
        public bool ShowSignIndicator = false;

        [DynamoDBProperty]
        public bool ShowPaintBadgeClinic = false;

        [DynamoDBProperty]
        public bool ShowPaintBadgeLab = false;

        [DynamoDBProperty]
        public List<string> LaboratoryObjectsData;

        [DynamoDBProperty]
        public List<string> PatioObjectsData;

        [DynamoDBProperty("UnlockedLaboratory")]
        public List<int> UnlockedLaboratoryAreas;

        [DynamoDBProperty]
        public List<string> LastHelpers;

        [DynamoDBProperty("UnlockedClinic")]
        public List<int> UnlockedClinicAreas;

        [DynamoDBProperty]
        public List<string> Elixirs;

        [DynamoDBProperty]
        public List<string> Plantation;

        [DynamoDBProperty]
        public string HospitalCustomizations;

        [DynamoDBProperty]
        public string MedicinePermutations;

        [DynamoDBProperty]
        public string LastMedicineRndPool;

        [DynamoDBProperty]
        public List<string> PendingPharmacyAddOrderTransactions = new List<string>();

        [DynamoDBProperty]
        public List<string> dailyRewardSave = new List<string>();

        [DynamoDBProperty]
        public List<string> PendingPharmacyDeleteOrderTransactions = new List<string>();

        [DynamoDBProperty]
        public string PendingPharmacyClaimedOrderTransactions = "";

        public List<string> NotificationSettings;

        /*[DynamoDBProperty]
		public List<string> VariousSaves;*/

        [DynamoDBProperty]
        public string StoredItems;

        [DynamoDBProperty]
        public float PharmacyTime;

        [DynamoDBProperty]
        public float PharmacyPageRefreshTime;

        [DynamoDBProperty]
        public long PharmacyLastRefreshBoardTime;

        [DynamoDBProperty]
        public List<string> PharmacyOffers;

        [DynamoDBProperty]
        public List<string> WiseOffers;

        [DynamoDBProperty]
        public long LastRefreshWiseOffers;

        /*[DynamoDBProperty]
		public int PharmacyScanRandSeed;*/

        [DynamoDBProperty]
        public string VipHouse;

        [DynamoDBProperty]
        public string PlayHouse;

        [DynamoDBProperty]
        public string BubbleBoy;

        [DynamoDBProperty]
        public List<int> BedsToUnlock;

        [DynamoDBProperty]
        public int PatientsHealedEver;

        [DynamoDBProperty]
        public int LastSpawnedPatientLevel;

        [DynamoDBProperty]
        public int lastRandomizedCureCounter;

        [DynamoDBProperty]
        public int IAPPurchasesCount;

        [DynamoDBProperty]
        public int IAPValentineCount;

        [DynamoDBProperty]
        public int IAPEasterCount;

        [DynamoDBProperty]
        public int IAPLabourDayCount;

        [DynamoDBProperty]
        public int IAPCancerDayCount;

        [DynamoDBProperty]
        public int PharmacySlots = 1;

        [DynamoDBProperty]
        public bool IAPBoughtLately = false;

        [DynamoDBProperty]
        public bool DiamondUsedLately = false;

        [DynamoDBProperty]
        public List<string> Following;

        [DynamoDBProperty]
        public List<string> Achievements;

        [DynamoDBProperty]
        public string Booster;

        [DynamoDBProperty]
        public string Cases;

        [DynamoDBProperty]
        public string VIPSystem;

        [DynamoDBProperty]
        public bool NoMoreRemindFBConnection;

        [DynamoDBProperty]
        public bool FBConnectionRewardEnabled;

        [DynamoDBProperty]
        public bool FBRewardConnectionClaimed;

        [DynamoDBProperty]
        public bool HomePharmacyVisited;

        [DynamoDBProperty]
        public bool EverLoggedInFB;

        [DynamoDBProperty]
        public string NonLinearCompletion;    //Tutorial non linear steps

        [DynamoDBProperty]
        public List<string> OtherPatients;

        [DynamoDBProperty]
        public List<string> EpidemyData;

        [DynamoDBProperty]
        public List<string> MaternityWardData;

        [DynamoDBProperty]
        public bool StarterPackUsed;

        /// <summary>
        /// Current Session Timer - starts at the begging of current session
        /// </summary>
        [DynamoDBProperty]
        public float GameplayTimer;

        [DynamoDBProperty]
        public string Treasure;

        [DynamoDBProperty]
        public string DailyQuest;

        [DynamoDBProperty]
        public string DailyDeal;

        [DynamoDBProperty]
        public string LevelGoals;

        [DynamoDBProperty]
        public string GlobalEvent;

        [DynamoDBProperty]
        public string TreatmentHelp;

        [DynamoDBProperty]
        public string AdvancedPatientCounter;

        [DynamoDBProperty]
        public string AdvancedCuresCounter;

        [DynamoDBProperty]
        public int ElixirStoreToUpgrade;

        [DynamoDBProperty]
        public int ElixirTankToUpgrade;

        /// <summary>
        /// IAP transactions which are already completed and saved
        /// </summary>
        [DynamoDBProperty]
        public string CompletedIAP;

        [DynamoDBProperty]
        public string BadgesToShow;

        [DynamoDBProperty]
        public string CommunityRewards;

        [DynamoDBProperty]
        public int CoinAmountFromIAP = 0;

        [DynamoDBProperty]
        public int DiamondAmountFromIAP = 0;

        [DynamoDBProperty]
        public string LastSuccessfulPurchase;

        [DynamoDBProperty]
        public string CampaignConfigs;

        [DynamoDBProperty]
        public List<long> GiftsCooldownTimers;

        [DynamoDBProperty]
        public List<string> SendedGifts;

        [DynamoDBProperty]
        public long LastBuyOrLastRefreshWithSomeGiftsTime = -1;

        [DynamoDBProperty]
        public int AdsWatched = 0;

        [DynamoDBProperty]
        public int HelpsCounter = 0;

        [DynamoDBProperty]
        public bool AddAdsReward = false;

        [DynamoDBProperty]
        public string AdType;

        [DynamoDBProperty]
        public bool HasSomeFriends = false;

        [DynamoDBProperty]
        public bool HasSomeFollowers = false;

        [DynamoDBProperty]
        public bool HasSomeFollowings = false;

        [DynamoDBProperty]
        public List<string> AddRequestInTreatmentRoomTransactions;

        [DynamoDBProperty]
        public List<string> RemoveRequestInTreatmentRoomTransaction;

        [DynamoDBProperty]
        public List<string> AddDonationInTreatmentRoomTransactions;

        [DynamoDBProperty]
        public bool wasTreatmentHelpRequested = false;

        [DynamoDBProperty]
        public string LootBoxTransaction = null;

        [DynamoDBProperty]
        public bool AddLootBoxReward = false;

        [DynamoDBProperty]
        public bool HasAnyTreatmentRoomHelpRequests = false;

        [DynamoDBProperty]
        public List<string> RandomFriendsIds;

        [DynamoDBProperty]
        public int RandomFriendsTimestamp;

        [DynamoDBProperty]
        public string PersonalFriendCode;

        [DynamoDBProperty]
        public string ReputationAmounts;

        [DynamoDBProperty]
        public bool EverOpenedCrossPromotionPopup = false;

        // Whether the player has completed the TnT cross-promotion campaign
        // (either by getting the reward or by having TnT already installed)
        [DynamoDBProperty]
        public bool CrossPromotionCompleted = false;

        #region Maternity

        [DynamoDBProperty]
        public string MaternityTutorialStepTag = "maternity_welcome";

        [DynamoDBProperty]
        public bool MaternityIsFirstLoopCompleted = false;

        [DynamoDBProperty("UnlockedMaternityWard")]
        public List<int> UnlockedMaternityWardClinicAreas;

        [DynamoDBProperty]
        public List<string> MaternityClinicObjectsData;

        [DynamoDBProperty]
        public List<string> MaternityPatioObjectsData;

        [DynamoDBProperty]
        public string MaternityPatientCounter;

        [DynamoDBProperty]
        public string MaternityCustomization;

        [DynamoDBProperty]
        public List<string> MaternityPatients = new List<string>();

        [DynamoDBProperty]
        public bool ShowPaintBadgeMaternityClinic = false;

        [DynamoDBProperty]
        public int MaternityExperience;

        [DynamoDBProperty]
        public int MaternityLevel = 1;

        [DynamoDBProperty]
        public long MaternitySaveDateTime;

        [DynamoDBProperty]
        public string SampleVariable;

        [DynamoDBProperty]
        public string NurseRoom;

        [DynamoDBProperty]
        public string BadgesToShowMaternity;
        #endregion

        public Save()
        {
            UnlockedLaboratoryAreas = new List<int>();
            UnlockedClinicAreas = new List<int>();
            //VariousSaves = new List<string>();
            Elixirs = new List<string>();
        }

        public string VariousContains(string tag)
        {
            return null;//zaslepka - do jakiegos sensownego szukania (for some meaningful search)
        }

        public void AddVarious(string tag, string value)
        {

        }
    }
}