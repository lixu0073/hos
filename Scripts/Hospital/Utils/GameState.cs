using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hospital;
using IsoEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using System.Text;
using System;
using System.Threading;
using MovementEffects;
using Transactions;
using Facebook.Unity;
using TutorialSystem;

public class GameState : BaseGameState, IGameState
{

    #region FieldsAndProperties

    public List<BaseCharacterInfo> childrenList;

    public List<string> PendingPharmacyAddOrderTransactions = new List<string>();
    public List<string> PendingPharmacyDeleteOrderTransactions = new List<string>();
    public Dictionary<string, int> PendingPharmacyClaimOrderTransactions = new Dictionary<string, int>();
    public CCuresCount CuresCount = new CCuresCount();
    public int HelpsCounter;
    public void IncrementHelpsCounter()
    {
        ++HelpsCounter;
        if (HelpsCounter % 10 == 0)
        {
            TenjinController.instance.ReportHelpsCounter(HelpsCounter);
        }
    }

    private int globalOffersSlotsCount;
    public int GlobalOffersSlotsCount
    {
        get
        {
            return globalOffersSlotsCount;
        }
        set
        {
            if (value <= 0)
            {
                globalOffersSlotsCount = DefaultConfigurationProvider.GetConfigCData().GlobalOffersInitSlotsCount;
                return;
            }
            globalOffersSlotsCount = value;
        }
    }
    public int GetGlobalOffersSlotsCount()
    {
        return Mathf.Min(GlobalOffersSlotsCount, DefaultConfigurationProvider.GetConfigCData().GlobalOffersMaxSlotsCount);
    }

    public long LastPromotionOfferAdd = -1;
    public long LastStandardOfferAdd = -1;



    private int friendsOffersSlotsCount;
    public int FriendsOffersSlotsCount
    {
        get
        {
            return friendsOffersSlotsCount;
        }
        set
        {
            if (value <= 0)
            {
                friendsOffersSlotsCount = DefaultConfigurationProvider.GetConfigCData().FriendsOffersInitSlotsCount;
                return;
            }
            friendsOffersSlotsCount = value;
        }
    }
    public int GetFriendsOffersSlotsCount()
    {
        return Mathf.Min(FriendsOffersSlotsCount, DefaultConfigurationProvider.GetConfigCData().FriendsOffersMaxSlotsCount);
    }


    private int globalEventChestCount;
    public int GlobalEventChestsCount
    {
        get { return globalEventChestCount; }
        set { globalEventChestCount = value; }
    }

    private long lastGlobalEventChestSpawnTime;
    public long LastGlobalEventChestSpawnTime
    {
        get { return lastGlobalEventChestSpawnTime; }
        set { lastGlobalEventChestSpawnTime = value; }
    }
    private string lastGlobalEventChestsEventID;
    public string LastGlobalEventChestsEventID
    {
        get { return lastGlobalEventChestsEventID; }
        set { lastGlobalEventChestsEventID = value; }
    }

    public int KidsToSpawn;

    public long LastBuyByWise
    { get; set; }

    public int patientsHealedEver;
    public int lastSpawnedPatientLevel;
    public int lastRandomizedCureCounter;

    public int ExpansionsLab
    {
        get
        {
            return AreaMapController.Map.CountBoughtAreas(HospitalArea.Laboratory);
        }
        set { }
    }

    public bool HasAnyTreatmentRoomHelpRequests = false;

    public bool wasTreatmentHelpRequested = false;

    public CPatientsCount PatientsCount = new CPatientsCount();

    public bool canSpawnKids = false;
    /// <summary>
    /// returns existing copy of GameState Controller.
    /// </summary>
    /// <returns></returns>
    public static GameState Get()
    {
        if (value == null)
            throw new IsoException("Fatal Failure of GameState.");
        else return value;
    }

    private static GameState value;

    void Awake()
    {
        if (value != null && value != this)
        {
            Debug.LogWarning("Multiple instances of GameState were found!");
            Destroy(gameObject);
        }
        else
            value = this;
    }

    #endregion

    #region TutorialConditions

    [TutorialCondition]
    public bool LevelReached(int level) { return Get().hospitalLevel >= level; }

    [TutorialCondition]
    public bool AmountOfMedicineGreaterOrEqual(MedicineType type, int id, int amount)
    {
        if (!resources.ContainsKey(100 * (int)type + id))
            return 0 >= amount;
        return resources[100 * (int)type + id] >= amount;
    }
    [TutorialCondition]
    public bool ExpAmountReached(int exp) { return Get().ExperienceAmount >= exp; }
    [TutorialCondition]
    public bool PatientReachedFirstEverBed()
    {
        return HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[0]._BedStatus ==
                       HospitalBedController.HospitalBed.BedStatus.OccupiedBed;
    }
    [TutorialCondition]
    public bool NumberOfHealedPatientsEverIsGreaterThanOrEqual(int num) { return Get().patientsHealedEver >= num; }

    [TutorialCondition]
    public bool NotAllTutorialTablesAreFull()
    {
        ProbeTable[] allTables = FindObjectsOfType<ProbeTable>();
        if (allTables.Length < 6)
        {
            if (TutorialSystem.TutorialController.ShowTutorials) //Notify only if tutorials are shown.
                Debug.LogError("Couldn't find 6 probe tables! Found only " + allTables.Length);

            return true;
        }
        for (int i = 0; i < 6; i++)
        {
            if (allTables[i].producedElixir == null)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region TutorialTriggerables

    [TutorialTriggerable]
    public void GiveMedicine(MedicineType type, int id, int amountThatThePlayerShouldHave)
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;
        MedicineRef medicine = new MedicineRef(type, id);
        int count = GetCureCount(medicine);
        if (count < amountThatThePlayerShouldHave)
            AddResource(medicine, amountThatThePlayerShouldHave - count, true, EconomySource.LevelUpGift);
    }

    [TutorialTriggerable]
    public void GivePlayerFreeVIPMeds()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        if (UIController.getHospital.PatientCard.CurrentCharacter == null)
            return;
        var meds = UIController.getHospital.PatientCard.CurrentCharacter.requiredMedicines;
        Vector3 startPoint = Vector3.zero;
        float particleDelay = 0f;
        foreach (var item in meds)
        {
            Get().AddResource(item.Key.GetMedicineRef(), item.Value, true, EconomySource.Tutorial);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, startPoint, 1, particleDelay, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), item.Key.image, null, null);
            particleDelay += .35f;
        }

        this.InvokeDelayed(() => { UIController.getHospital.PatientCard.RefreshView(UIController.getHospital.PatientCard.CurrentCharacter); }, 1.75f);
    }

    [TutorialTriggerable]
    public void GiveMedicineWithAnimation(MedicineType type, int id, int amountToAdd)
    {

        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;
        MedicineRef medicine = new MedicineRef(type, id);
        bool isTank = medicine.IsMedicineForTankElixir();

        UIController.get.storageCounter.Add(isTank);

        AddResource(medicine, amountToAdd, true, EconomySource.Tutorial);

        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, Vector3.zero, amountToAdd, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(medicine), null, () =>
        {
            UIController.get.storageCounter.Remove(1, isTank);
        });
    }

    [TutorialTriggerable]
    public void GiveMedicineWithAnimationUpToAmount(MedicineType type, int id, int amountThatThePlayerShouldHave)
    {

        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;
        MedicineRef medicine = new MedicineRef(type, id);
        bool isTank = medicine.IsMedicineForTankElixir();

        UIController.get.storageCounter.Add(isTank);
        int count = GetCureCount(medicine);

        AddResource(medicine, amountThatThePlayerShouldHave - count, true, EconomySource.Tutorial);

        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, Vector3.zero, amountThatThePlayerShouldHave - count, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(medicine), null, () =>
        {
            UIController.get.storageCounter.Remove(1, isTank);
        });
    }

    [TutorialTriggerable] public void UnblockPanacea() { PanaceaCollector.Unblock(); }
    [TutorialTriggerable]
    public void UnblockTreatmentRoomSpawn()
    {
        BaseGameState.OnLevelUp -= UnblockTreatmentRoomSpawn;
        if (!TutorialSystem.TutorialController.ShowTutorials && !TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.cure_bed_patient, true))
        {
            HospitalBedController.blockedByTutorial = true;
            BaseGameState.OnLevelUp += UnblockTreatmentRoomSpawn;
            return;
        }

        if (TutorialSystem.TutorialController.ShowTutorials && !TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.cure_bed_patient))
            return;

        HospitalBedController.blockedByTutorial = false;
    }
    [TutorialTriggerable]
    public void SetTreatmentRoomTutorialMode(bool isOn) { HospitalRoom.tutorialMode = isOn; }
    [TutorialTriggerable]
    public void UnlockKidsSpawn()
    {
        BaseGameState.OnLevelUp -= UnlockKidsSpawn;
        if (TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.kids_open, true))
        {
            canSpawnKids = true;
        }
        else
        {
            BaseGameState.OnLevelUp += UnlockKidsSpawn;
        }
    }

    [TutorialTriggerable]
    public void GivePlayerDiamonds(int amount)
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, amount, 0, 2f, new Vector3(3.2f, 3.2f, 1), new Vector3(1.3f, 1.3f, 1), ResourcesHolder.Get().diamondSprite, null, () =>
        {
            AddDiamonds(amount, EconomySource.Tutorial);
        });
    }

    [TutorialTriggerable]
    public void GivePlayerABooster()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Booster, Vector3.zero, 1, 0, 2f, new Vector3(3.2f, 3.2f, 1), new Vector3(1.3f, 1.3f, 1), ResourcesHolder.Get().boosterDatabase.boosters[1].icon, null, () =>
        {
            HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(1, EconomySource.Tutorial);
            HospitalUIController.get.BoosterButton.GetComponent<Button>().interactable = true;
        });
    }

    [TutorialTriggerable]
    public void AllowSpawningFirstPatient()
    {
        BaseGameState.OnLevelUp -= AllowSpawningFirstPatient;
        if (TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.follow_ambulance, true))
            HospitalBedController.canSpawnFirstEverPatient = true;
        else
        {
            BaseGameState.OnLevelUp += AllowSpawningFirstPatient;
        }
    }

    void GiveMedicine(MedicineRef medicine, int amountThatThePlayerShouldHave)
    {
        int count = GetCureCount(medicine);
        if (count < amountThatThePlayerShouldHave)
            AddResource(medicine, amountThatThePlayerShouldHave - count, true, EconomySource.LevelUpGift);
    }
    #endregion

    #region ResourceManipulation

    public void UpdateMedicinesNeededListWithPatient(HospitalCharacterInfo patient)
    {
        if (patient != null)
        {
            if (patient.requiredMedicines != null)
            {
                for (int i = 0; i < patient.requiredMedicines.Length; ++i)
                {
                    if (patient.requiredMedicines[i].Key.IsDiagnosisMedicine()) // HAS DIAGNOSTIC MEDICINE AND NOT REQUIRED DIAGNOSIS THEN ADD ELSE ADD IN OTHER LAY IN BED IF NOT REQUIRED DIAG
                    {
                        if (!patient.RequiresDiagnosis && patient.isUpdatedDiagnosableMedicineNeeded == false)
                        {
                            MedicineBadgeHintsController.Get().AddMedNeededToHeal(patient.requiredMedicines[i].Key, patient.requiredMedicines[i].Value);
                            MedicineBadgeHintsController.Get().AddOnlyNeededMedicine(patient.requiredMedicines[i].Key.GetMedicineRef(), patient.requiredMedicines[i].Value);

                            patient.isUpdatedDiagnosableMedicineNeeded = true;
                        }
                    }
                    else if (patient.isUpdatedNonDiagnosableMedicineNeeded == false)
                    {
                        MedicineBadgeHintsController.Get().AddMedNeededToHeal(patient.requiredMedicines[i].Key, patient.requiredMedicines[i].Value);
                        MedicineBadgeHintsController.Get().AddOnlyNeededMedicine(patient.requiredMedicines[i].Key.GetMedicineRef(), patient.requiredMedicines[i].Value);
                    }
                }
            }
        }

        patient.isUpdatedNonDiagnosableMedicineNeeded = true;
    }

    public bool GetPanacea(int amount)
    {
        if (PanaceaCollector != null)
            return PanaceaCollector.GetPanacea(amount);
        return false;
    }

    public int CheckPanaceaAmount()
    {
        if (PanaceaCollector != null)
            return PanaceaCollector.actualAmount;
        return 0;
    }

    public int achievementsDone = 0;
    public int AchievementsDone
    {
        get { return achievementsDone; }
        set
        {
            achievementsDone = value;
            HospitalAreasMapController.HospitalMap.reception.CheckLevel(value);
        }
    }

    public bool IsMaternityFirstLoopCompleted
    {
        get
        {
            return false;
        }
        set { }
    }

    #endregion

    #region SaveLoad

    public override void LoadDrawerData(Save save)
    {
        UIController.get.drawer.UnlockItems(ResourcesHolder.Get().GetUnlockedMachines());
    }

    #region Pharmacy Transactions

    public bool AddToPendingClaimedOrderTransactionsList(PharmacyOrder order)
    {
        if (PendingPharmacyClaimOrderTransactions.ContainsKey(order.ID))
        {
            return false;
        }
        PendingPharmacyClaimOrderTransactions.Add(order.ID, order.pricePerUnit);
        return true;
    }

    public bool RemoveFromPendingClaimedOrderTransactionsList(PharmacyOrder order)
    {
        return PendingPharmacyClaimOrderTransactions.Remove(order.ID);
    }

    public bool RemoveFromPendingClaimedOrderTransactionsList(string ID)
    {
        return PendingPharmacyClaimOrderTransactions.Remove(ID);
    }

    public string SavePendingClaimedOrderTransactionsList()
    {
        return string.Join("#", PendingPharmacyClaimOrderTransactions.Select(x => x.Key + "=" + x.Value).ToArray());
    }

    public Dictionary<string, int> LoadPendingClaimedOrderTransactionsList(string str)
    {
        Dictionary<string, int> TempPendingPharmacyClaimOrderTransactions = new Dictionary<string, int>();
        string[] transactions = str.Split('#');
        for (int i = 0; i < transactions.Length; ++i)
        {
            string[] pair = transactions[i].Split('=');
            if (pair.Length == 2)
            {
                TempPendingPharmacyClaimOrderTransactions.Add(pair[0], int.Parse(pair[1], System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        return TempPendingPharmacyClaimOrderTransactions;
    }

    public bool AddOrderToPendingDeleteOrderTransactionsList(PharmacyOrder order)
    {
        for (int i = 0; i < PendingPharmacyDeleteOrderTransactions.Count; ++i)
        {
            if (order.ID == PendingPharmacyDeleteOrderTransactions[i])
            {
                return false;
            }
        }
        PendingPharmacyDeleteOrderTransactions.Add(order.ID);
        return true;
    }

    public bool RemoveOrderFromPendingDeleteOrderTransactionsList(PharmacyOrder order)
    {
        return PendingPharmacyDeleteOrderTransactions.Remove(order.ID);
    }

    public bool AddNewOrderToPendingAddOrderTransactionsList(PharmacyOrder order)
    {
        for (int i = 0; i < PendingPharmacyAddOrderTransactions.Count; ++i)
        {
            if (order.ShortSaveString() == PendingPharmacyAddOrderTransactions[i])
            {
                return false;
            }
        }
        PendingPharmacyAddOrderTransactions.Add(order.ShortSaveString());
        return true;
    }

    public bool RemoveOrderFromPendingAddOrderTransactionsList(PharmacyOrder order)
    {
        return PendingPharmacyAddOrderTransactions.Remove(order.ShortSaveString());
    }

    #endregion

    public string maxGameVersion;

    public virtual int GetLevelHospital()
    {
        return hospitalLevel;
    }

    public override void LoadData(Save save)
    {
        string[] p;
        resources = new Dictionary<int, int>(save.Elixirs.Count);
        save.Elixirs.ForEach(x =>
        {
            p = x.Split('+');
            resources.Add(int.Parse(p[0]), int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture));
        });

        HasAnyTreatmentRoomHelpRequests = save.HasAnyTreatmentRoomHelpRequests;

        hospitalLevel = save.Level;
        ReferenceHolder.GetHospital().Epidemy.Level = hospitalLevel;
        ExperienceAmount = save.Experience;

        HasSomeFriends = save.HasSomeFriends;
        HasSomeFollowers = save.HasSomeFollowers;
        HasSomeFollowings = save.HasSomeFollowings;

        AdsWatched = save.AdsWatched;
        HelpsCounter = save.HelpsCounter;

        wasTreatmentHelpRequested = save.wasTreatmentHelpRequested;

        LocalNotificationController.Instance.notificationGroups.LoadFromString(save.NotificationSettings);

        ActionsCounterForExtraGift = 0;
        ActionsCounterForExtraGiftStorage = 0;
        ActionsCounterForExtraGiftTank = 0;
        HospitalName = save.HospitalName;
        if (!string.IsNullOrEmpty(HospitalName))
            AnalyticsGeneralParameters.hospitalName = HospitalName;
        KidsToSpawn = save.KidsToSpawn;
        Coins = save.CoinAmount;
        UIController.get.coinCounter.SetValue(Coins);
        CoinAmountFromIAP = save.CoinAmountFromIAP;
        //UIController.get.PharmacyPopUp.salesUnlocked = save.PharmacySlots;
        Diamonds = save.DiamondAmount;
        DiamondAmountFromIAP = save.DiamondAmountFromIAP;
        PositiveEnergyAmount = save.PositiveEnergyAmount;
        //ActionsCounterForExtraGift = save.ActionsCounterForExtraGift;
        ActionsCounterForExtraGiftStorage = save.ActionsCounterForExtraGiftStorage;
        ActionsCounterForExtraGiftTank = save.ActionsCounterForExtraGiftTank;
        UIController.get.diamondCounter.SetValue(Diamonds);
        patientsHealedEver = save.PatientsHealedEver;
        lastSpawnedPatientLevel = save.LastSpawnedPatientLevel;
        lastRandomizedCureCounter = save.lastRandomizedCureCounter;

        PersonalFriendCode = save.PersonalFriendCode;
        RandomFriendsIds = save.RandomFriendsIds;
        RandomFriendsTimestamp = save.RandomFriendsTimestamp;
        AchievementsDone = save.AchievementsDone;
        Version = save.version;
        LastBuyByWise = save.lastBuyByWise;
        IAPPurchasesCount = save.IAPPurchasesCount;
        IAPValentineCount = save.IAPValentineCount;
        IAPEasterCount = save.IAPEasterCount;
        IAPLabourDayCount = save.IAPLabourDayCount;
        IAPCancerDayCount = save.IAPCancerDayCount;
        LastSuccessfulPurchase = save.LastSuccessfulPurchase;

        IAPBoughtLately = save.IAPBoughtLately;
        DiamondUsedLately = save.DiamondUsedLately;

        timedOfferEndDate = save.timedOfferEndDate;
        timedOfferPurchaseDate = save.timedOfferPurchaseDate;
        ElixirStoreToUpgrade = save.ElixirStoreToUpgrade;
        ElixirTankToUpgrade = save.ElixirTankToUpgrade;

        LoadMedicinePermutationsList(save.MedicinePermutations);
        LoadLastMedicineRndPool(save.LastMedicineRndPool);

        PendingPharmacyAddOrderTransactions = save.PendingPharmacyAddOrderTransactions;
        PendingPharmacyDeleteOrderTransactions = save.PendingPharmacyDeleteOrderTransactions;
        PendingPharmacyClaimOrderTransactions = LoadPendingClaimedOrderTransactionsList(save.PendingPharmacyClaimedOrderTransactions);


        if (SaveLoadController.CheckIfGameUpgraded(Application.version, save.maxGameVersion))
        {
            Debug.LogError("Detected different game version! Rewarding player! old: " + save.gameVersion + " new: " + Application.version);
            UIController.get.UpdateRewardPopUp.Open();
        }
        else
        {
            maxGameVersion = save.maxGameVersion;
        }

        UIController.getHospital.PharmacyPopUp.wisePharmacyManager.Initialize(save);

        NoMoreRemindFBConnection = save.NoMoreRemindFBConnection;
        FBConnectionRewardEnabled = save.FBConnectionRewardEnabled;
        FBRewardConnectionClaimed = save.FBRewardConnectionClaimed;
        HomePharmacyVisited = save.HomePharmacyVisited;
        EverLoggedInFB = save.EverLoggedInFB;
        StarterPackUsed = save.StarterPackUsed;
        GameplayTimer = save.GameplayTimer;
        LocalNotificationController.Instance.LoadFromString(save.notificationData);

        UIController.get.SetLevelText(hospitalLevel);
        UIController.get.drawer.UnlockItems(ResourcesHolder.Get().GetUnlockedMachines());

        RefreshXPBar();
        if (BuildedObjects == null)
            BuildedObjects = new Dictionary<string, int>();
        else
            BuildedObjects.Clear();
        LoadStoredItems(save.StoredItems);

        OtherMapBadgesToShow = save.BadgesToShowMaternity;
        LoadBadgedMachines(save.BadgesToShow);
        LoadCommunityRewards(save.CommunityRewards);

        GlobalOffersSlotsCount = save.globalOffersSlotsCount;
        FriendsOffersSlotsCount = save.friendsOffersSlotsCount;
        LastPromotionOfferAdd = save.lastPromotionOfferAdd;
        LastStandardOfferAdd = save.lastStandardOfferAdd;
        AddAdsReward = save.AddAdsReward;
        AdType = save.AdType;

        GiftsSynchronizer.Instance.Load(save.GiftsCooldownTimers, save.SendedGifts, save.LastBuyOrLastRefreshWithSomeGiftsTime, VisitingController.Instance.IsVisiting);

        if (!VisitingController.Instance.IsVisiting)
        {
            LastHelpersSynchronizer.Instance.LoadFromList(save.LastHelpers);
        }

        if (!VisitingController.Instance.IsVisiting)
        {
            AnalyticsGeneralParameters.userLevel = hospitalLevel;
            AnalyticsGeneralParameters.userXP = ExperienceAmount;
            AnalyticsGeneralParameters.softCurrency = Coins;
            AnalyticsGeneralParameters.hardCurrency = Diamonds;
            AnalyticsGeneralParameters.positiveEnergy = PositiveEnergyAmount;
        }
        TransactionManager.Instance.Load(save);
        LootBoxTransaction = save.LootBoxTransaction;
        AddLootBoxReward = save.AddLootBoxReward;
        ReferenceHolder.Get().lootBoxManager.TryToCompletePurchase();

        EverOpenedCrossPromotionPopup = save.EverOpenedCrossPromotionPopup;
        CrossPromotionCompleted = save.CrossPromotionCompleted;

        OnLoaded();
    }
    //matward this save should go to abstraction, override etc etc.(Translated from Polish)
    public override void SaveData(Save save, bool isVisiting = false)
    {
        save.Elixirs = new List<string>(resources.Count);

        for (int i = 0; i < resources.Count; i++)
        {
            save.Elixirs.Add(resources.ElementAt(i).Key + "+" + Checkers.CheckedAmount(resources.ElementAt(i).Value, 0, int.MaxValue, "ElixirStorage " + resources.ElementAt(i).Key + " amount: "));
        }

        save.version = LoadingGame.version;
        save.gameVersion = Application.version;
        if (SaveLoadController.CheckIfGameUpgraded(Application.version, maxGameVersion, true))
        {
            save.maxGameVersion = Application.version;
        }
        else
        {
            save.maxGameVersion = maxGameVersion;
        }
        save.HasAnyTreatmentRoomHelpRequests = HasAnyTreatmentRoomHelpRequests;
        save.HasSomeFriends = HasSomeFriends;
        save.HasSomeFollowers = HasSomeFollowers;
        save.HasSomeFollowings = HasSomeFollowings;
        save.NoMoreRemindFBConnection = NoMoreRemindFBConnection;
        save.FBConnectionRewardEnabled = FBConnectionRewardEnabled;
        save.FBRewardConnectionClaimed = FBRewardConnectionClaimed;

        save.AdsWatched = AdsWatched;
        save.HelpsCounter = HelpsCounter;

        save.wasTreatmentHelpRequested = wasTreatmentHelpRequested;

        save.HomePharmacyVisited = HomePharmacyVisited;

        save.EverLoggedInFB = EverLoggedInFB;
        save.StarterPackUsed = StarterPackUsed;
        save.GameplayTimer = GameplayTimer;

        save.KidsToSpawn = Checkers.CheckedAmount(KidsToSpawn, 0, int.MaxValue, "KidsToSpawn");

        save.Level = Checkers.CheckedAmount(hospitalLevel, 1, int.MaxValue, "Actual level");
        save.CoinAmount = Checkers.CheckedAmount(Coins, 0, int.MaxValue, "Coins");
        save.DiamondAmount = Checkers.CheckedAmount(Diamonds, 0, int.MaxValue, "Diamonds");
        save.PositiveEnergyAmount = Checkers.CheckedAmount(PositiveEnergyAmount, 0, int.MaxValue, "Positive energy");
        save.ActionsCounterForExtraGiftStorage = Checkers.CheckedAmount(ActionsCounterForExtraGiftStorage, 0, int.MaxValue, "Actions for extra gifts"); //to check
        save.ActionsCounterForExtraGiftTank = Checkers.CheckedAmount(ActionsCounterForExtraGiftTank, 0, int.MaxValue, "Actions for extra gifts"); //to check
        save.Experience = Checkers.CheckedExperience(ExperienceAmount, hospitalLevel);
        save.StoredItems = SaveStoredItems();
        save.HospitalName = Checkers.CheckedHospitalName(HospitalName);
        save.CoinAmountFromIAP = Checkers.CheckedAmount(CoinAmountFromIAP, 0, int.MaxValue, "Coins from IAP");
        save.DiamondAmountFromIAP = Checkers.CheckedAmount(DiamondAmountFromIAP, 0, int.MaxValue, "Diamonds from IAP");
        save.LastSuccessfulPurchase = LastSuccessfulPurchase;

        //save.BedsToUnlock = Checkers.CheckedBedsToUnlock(bedsToUnlock).ToList();
        save.PatientsHealedEver = Checkers.CheckedAmount(patientsHealedEver, 0, int.MaxValue, "Patients healed ever");
        save.LastSpawnedPatientLevel = Checkers.CheckedAmount(lastSpawnedPatientLevel, 0, int.MaxValue, "Last spawned patient level");
        save.AchievementsDone = Checkers.CheckedAmount(AchievementsDone, 0, UIController.getHospital.AchievementsPopUp.achievementDatabase.AchievementItem.Count * 3, "Achievements");

        if (!isVisiting)
        {
            save.PharmacySlots = Checkers.CheckedAmount(UIController.getHospital.PharmacyPopUp.salesUnlocked, 0, int.MaxValue, "PharmacySlots: ");
        }

        save.IAPPurchasesCount = Checkers.CheckedAmount(IAPPurchasesCount, 0, int.MaxValue, "IAP purchases count");
        save.IAPValentineCount = Checkers.CheckedAmount(IAPValentineCount, 0, int.MaxValue, "IAP valentine count");
        save.IAPEasterCount = Checkers.CheckedAmount(IAPEasterCount, 0, int.MaxValue, "IAP easter count");
        save.IAPLabourDayCount = Checkers.CheckedAmount(IAPLabourDayCount, 0, int.MaxValue, "IAP labour count");
        save.IAPCancerDayCount = Checkers.CheckedAmount(IAPCancerDayCount, 0, int.MaxValue, "IAP cancer count");

        save.IAPBoughtLately = Checkers.CheckedBool(IAPBoughtLately);
        save.DiamondUsedLately = Checkers.CheckedBool(DiamondUsedLately);

        save.lastBuyByWise = LastBuyByWise;
        save.timedOfferEndDate = timedOfferEndDate;
        save.timedOfferPurchaseDate = timedOfferPurchaseDate;

        save.notificationData = LocalNotificationController.Instance.SaveToString();
        save.NotificationSettings = LocalNotificationController.Instance.notificationGroups.SaveToString();
        save.AdvancedPatientCounter = GameState.Get().PatientsCount.Save();
        save.AdvancedCuresCounter = GameState.Get().CuresCount.Save();
        save.ElixirStoreToUpgrade = Checkers.CheckedAmount(ElixirStoreToUpgrade, 0, int.MaxValue, "ElixirStoreToUpgrade");
        save.ElixirTankToUpgrade = Checkers.CheckedAmount(ElixirTankToUpgrade, 0, int.MaxValue, "ElixirTankToUpgrade");

        save.RandomFriendsTimestamp = RandomFriendsTimestamp;
        save.RandomFriendsIds = RandomFriendsIds;
        save.PersonalFriendCode = PersonalFriendCode;

        save.MedicinePermutations = SaveMedicinePermutationsList();
        save.LastMedicineRndPool = SaveLastMedicineRndPool();

        save.PendingPharmacyDeleteOrderTransactions = PendingPharmacyDeleteOrderTransactions;
        save.PendingPharmacyClaimedOrderTransactions = SavePendingClaimedOrderTransactionsList();

        save.BadgesToShowMaternity = OtherMapBadgesToShow;
        save.BadgesToShow = SaveBadgedMachines();
        save.CommunityRewards = SaveCommunityRewards();

        save.LastHelpers = LastHelpersSynchronizer.Instance.SaveToString();
        save.CampaignConfigs = CampaignController.Instance.SaveToString();

        GiftsSynchronizer.Data giftsData = GiftsSynchronizer.Instance.GetData();
        save.GiftsCooldownTimers = giftsData.giftsCooldownTimers;
        save.SendedGifts = giftsData.SendedGiftsToList();
        save.LastBuyOrLastRefreshWithSomeGiftsTime = giftsData.LastBuyOrLastRefreshWithSomeGiftsTime;

        save.globalOffersSlotsCount = GlobalOffersSlotsCount;
        save.friendsOffersSlotsCount = FriendsOffersSlotsCount;
        save.lastPromotionOfferAdd = LastPromotionOfferAdd;
        save.lastStandardOfferAdd = LastStandardOfferAdd;
        save.AddAdsReward = AddAdsReward;
        save.AdType = AdType;

        save.ReputationAmounts = ReputationSystem.ReputationController.Instance.SaveToString();

        TransactionManager.Instance.Save(save);

        save.LootBoxTransaction = LootBoxTransaction;
        save.AddLootBoxReward = AddLootBoxReward;


        UIController.getHospital.PharmacyPopUp.wisePharmacyManager.SaveState(save);

        save.EverOpenedCrossPromotionPopup = EverOpenedCrossPromotionPopup;
        save.CrossPromotionCompleted = CrossPromotionCompleted;
    }

    #endregion

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void testAddAchievementDone()
    {
        AchievementsDone++;
    }

    bool IGameState.IsStarterPackUsed()
    {
        return StarterPackUsed;
    }

    public void SetStarterPackUsed(bool value)
    {
        StarterPackUsed = value;
    }

    public bool IsNoMoreRemindFBConnection()
    {
        return NoMoreRemindFBConnection;
    }
    public bool IsFBConnectionRewardEnabled()
    {
        return FBConnectionRewardEnabled;
    }

    public bool IsFBRewardConnectionClaimed()
    {
        return FBRewardConnectionClaimed;
    }

    public bool IsEverLoggedInFB()
    {
        return EverLoggedInFB;
    }

    public bool IsHomePharmacyVisited()
    {
        return HomePharmacyVisited;
    }

    public void SetNoMoreRemindFBConnection(bool value)
    {
        NoMoreRemindFBConnection = value;
    }

    public void SetFBConnectionRewardEnabled(bool value)
    {
        FBConnectionRewardEnabled = value;
    }

    public void SetEverLoggedInFB(bool value)
    {
        EverLoggedInFB = value;
    }

    public void SetFBRewardConnectionClaimed(bool value)
    {
        FBRewardConnectionClaimed = value;
    }

    public void SetSaveFilePrepared(bool value)
    {
        saveFilePrepared = value;
    }

    public override void LevelUp()
    {
        if (CampaignController.Instance.CheckIfCampainTypeIsActive(CampaignData.CampaingType.Objectives))
        {
            if (!ReferenceHolder.Get().objectiveController.IsDynamicObjective() && ReferenceHolder.Get().objectiveController.GetAllObjectives() != null)
            {
                for (int i = 0; i < ReferenceHolder.Get().objectiveController.GetAllObjectives().Count; i++)
                {
                    string levelGoalName = ReferenceHolder.Get().objectiveController.GetAllObjectives()[i].GetObjectiveNameForAnalitics();
                    int progress = ReferenceHolder.Get().objectiveController.GetAllObjectives()[i].Progress;
                    int maxProgress = ReferenceHolder.Get().objectiveController.GetAllObjectives()[i].ProgressObjective;
                    bool completed = ReferenceHolder.Get().objectiveController.GetAllObjectives()[i].IsCompleted;
                    AnalyticsController.instance.ReportObjectivesStatus(levelGoalName, progress, maxProgress, completed);
                }
            }
        }

        try
        {
            if (ShowPopupCoroutine != null)
            {
                StopCoroutine(ShowPopupCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }

        Debug.Log("LVL UP!");

        ObjectiveNotificationCenter.Instance.LevelUpObjectiveUpdate.Invoke(new ObjectiveEventArgs(1));

        hospitalLevel++;

        if (hospitalLevel > 1)
        {
            CacheManager.Instance.SetUserHasUpFirstLevel(CognitoEntry.SaveID);
        }

        ReferenceHolder.GetHospital().Epidemy.Level = hospitalLevel;
        ReferenceHolder.GetHospital().Epidemy.EpidemyObject.OnLvlUp();
        HospitalAreasMapController.HospitalMap.maternityWard.OnLvlUp();
        ReferenceHolder.GetHospital().plantation.enableOnLevelUp();

        UIController.get.SetLevelText(hospitalLevel);
        List<MedicineRef> unlockedMedicines = ResourcesHolder.Get().medicines.cures.SelectMany((x) => { return x.medicines.Where((y) => { return (y.minimumLevel == hospitalLevel && x.type != MedicineType.Fake); }); }).Select<MedicineDatabaseEntry, MedicineRef>((z) => { return z.GetMedicineRef(); }).ToList();
        List<Rotations> unlockedMachines = ResourcesHolder.Get().GetMachinesForLevel(hospitalLevel);
        BadgedMachines = unlockedMachines;

        List<Rotations> additionalMachines = ResourcesHolder.Get().GetAdditionalMachines(hospitalLevel);

        for (int i = 0; i < additionalMachines.Count; i++)
        {
            if (unlockedMachines.Contains(additionalMachines[i]))
            {
                additionalMachines.RemoveAt(i);
            }
        }

        UIController.get.drawer.UnlockItems(unlockedMachines);

        ShowPopupCoroutine = StartCoroutine(ShowLevelPopup(unlockedMedicines, unlockedMachines, additionalMachines));

        // ADD HINTS FOR NEW MACHINES
        /*
        if (unlockedMachines != null && unlockedMachines.Count > 0)
        {
            for (int i = 0; i < unlockedMachines.Count; i++)
            {
                if (unlockedMachines[i].infos.Tag == "EyeDropsLab")
                {
                    HintsController.Get().AddHint(new BuildingHint((ShopRoomInfo)unlockedMachines[i].infos));
                }
            }
        }
        */

        // NotificationCenter.Instance.LevelUp.Invoke(new LevelUpEventArgs(actualLevel));
        NotificationCenter.Instance.DrawerUpdate.Invoke(new DrawerUpdateEventArgs());

        //here adding unlocks from reference (Translated from Polish)
        ClinicPatientAI.SpawnPatientOnLevelUp();
        AnalyticsGeneralParameters.userLevel = hospitalLevel;
        AnalyticsGeneralParameters.userXP = ExperienceAmount;
        AnalyticsController.instance.ReportLevelUp();
        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Level.ToString(), hospitalLevel, hospitalLevel.ToString());
        TenjinController.instance.ReportLevelUp(hospitalLevel);

        if (hospitalLevel == 8)
        {
            if (UIController.get.EventButton != null)
                UIController.get.EventButton.Setup();
        }
        if (hospitalLevel == 9)
        {
            UIController.get.drawer.SetPaintBadgeClinicVisible(true);
            UIController.get.drawer.SetPaintBadgeLabVisible(true);
        }

        try
        {
            if (FB.IsInitialized)
            {
                LogAchievedLevelEvent(hospitalLevel.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        SendOnLevelUpEvent();

        SaveSynchronizer.Instance.InstantSave();
    }

    public int GetExperienceAmount()
    {
        return ExperienceAmount;
    }

    public int GetExpansionClinicAmount()
    {
        return AreaMapController.Map.CountBoughtAreas(HospitalArea.Clinic);
    }

    public void SetExpansionClinicAmount(int value)
    {
        // ExpansionsClinic += value;
    }

    public int GetCoinAmount()
    {
        return Coins;
    }

    public int GetDiamondAmount()
    {
        return Diamonds;
    }

    public int GetCurrencyAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Coin:
                return Coins;
            case ResourceType.Diamonds:
                return Diamonds;
            case ResourceType.Exp:
                return ExperienceAmount;
            case ResourceType.PositiveEnergy:
                return PositiveEnergyAmount;
            default:
                Debug.LogError("returning zero");
                return 0;
        }
    }

    public float GetGameplayTimer()
    {
        return GameplayTimer;
    }

    public int GetPositiveEnergyAmount()
    {
        return PositiveEnergyAmount;
    }

    public long GetTimedOfferEndDate()
    {
        return timedOfferEndDate;
    }

    public long GetTimedOfferPurchaseDate()
    {
        return timedOfferPurchaseDate;
    }

    public hospitalBoosters GetHospitalBoosterSystem()
    {
        return HospitalBoosters;
    }

    public void UpdateExtraGift(Vector3 position, bool isDoctor)
    {
        throw new NotImplementedException();
    }

    public MasterableProperties GetMasterableProductionMachine(MedicineDatabaseEntry cure)
    {
        IProductables productable = HospitalDataHolder.Instance.BuiltProductionMachines.Find(a => string.Compare(a.GetTag(), cure.producedIn.Tag) == 0);
        if (productable is MedicineProductionMachine)
        {
            return ((MedicineProductionMachine)productable).masterableProperties;
        }
        return null;
    }

    public void SetTimedOfferEndDate(long time)
    {
        timedOfferEndDate = time;
    }

    public void SetTimedOfferPurchaseDate(long time)
    {
        timedOfferPurchaseDate = time;
    }

    #region DebugCheats
#if MH_QA || !MH_RELEASE
    public void GivePlayerDiamondsDebug(int amount, int currentDiamondAmount, bool updateCounter = true)
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, amount, 0, 2f, new Vector3(3.2f, 3.2f, 1), new Vector3(1.3f, 1.3f, 1), ResourcesHolder.Get().diamondSprite, null, () =>
        {
            AddDiamonds(amount, EconomySource.DebugCheats, updateCounter);
            Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, amount, currentDiamondAmount);
        });
    }

    public void GivePlayerCoinsDebug(int amount, int currentCoinsAmount, bool updateCounter = true)
    {
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, amount, 0, 2f, new Vector3(3.2f, 3.2f, 1), new Vector3(1.3f, 1.3f, 1), ResourcesHolder.Get().coinSprite, null, () =>
        {
            AddCoins(amount, EconomySource.DebugCheats, updateCounter);
            Game.Instance.gameState().UpdateCounter(ResourceType.Coin, amount, currentCoinsAmount);
        });
    }
#endif

    public static void IncreaseGameSpeed()
    {
        Time.timeScale = (Time.timeScale != 0.0001f) ? Time.timeScale * 2f : 1f;
    }

    public static void DecreaseGameSpeed()
    {
        Time.timeScale = (Time.timeScale != 0.0001f) ? Time.timeScale / 2f : 1f;
    }

    public static void ResetGameSpeed()
    {
        Time.timeScale = 1f;
    }

    #endregion // DebugCheats

    public class CPatientsCount
    {
        public CPatientsCount(int patientsCount = 0, int patientsCuredCount = 0, int patientsCuredCountBed = 0, int patientsCuredCountDoctor = 0, int patientsCuredCountVIP = 0, int patientsCuredCountKids = 0, int patientsCountDischarged = 0)
        {
            this.patientsCount = patientsCount;
            this.patientsCuredCount = patientsCuredCount;
            this.patientsCuredCountBed = patientsCuredCountBed;
            this.patientsCuredCountDoctor = patientsCuredCountDoctor;
            this.patientsCuredCountVIP = patientsCuredCountVIP;
            this.patientsCuredCountKids = patientsCuredCountKids;
            this.patientsCountDischarged = patientsCountDischarged;
        }
        //all patients
        private int patientsCount;
        public int PatientsCount
        {
            get { return patientsCount; }
            private set { patientsCount = value; }
        }
        //all patients
        private int patientsCuredCount;
        public int PatientsCuredCount
        {
            get { return patientsCuredCount; }
            private set
            {
                patientsCuredCount = value;
            }
        }
        //treatment room patients
        private int patientsCuredCountBed;
        public int PatientsCuredCountBed
        {
            get { return patientsCuredCountBed; }
            set
            {
                PatientsCount += value - patientsCuredCountBed;
                PatientsCuredCount += value - patientsCuredCountBed;
                patientsCuredCountBed = value;
            }
        }
        //doctor room patients
        private int patientsCuredCountDoctor;
        public int PatientsCuredCountDoctor
        {
            get { return patientsCuredCountDoctor; }
            set
            {
                PatientsCount += value - patientsCuredCountDoctor;
                PatientsCuredCount += value - patientsCuredCountDoctor;
                patientsCuredCountDoctor = value;
            }
        }
        //VIP patients
        private int patientsCuredCountVIP;
        public int PatientsCuredCountVIP
        {
            get { return patientsCuredCountVIP; }
            set
            {
                PatientsCount += value - patientsCuredCountVIP;
                PatientsCuredCount += value - patientsCuredCountVIP;
                patientsCuredCountVIP = value;
            }
        }
        //Kid patients
        private int patientsCuredCountKids;
        public int PatientsCuredCountKids
        {
            get { return patientsCuredCountKids; }
            set
            {
                patientsCuredCountKids = value;
            }
        }
        //Kid patients
        private int patientsCountDischarged;
        public int PatientsCountDischarged
        {
            get { return patientsCountDischarged; }
            set
            {
                patientsCount += value - patientsCountDischarged;
                patientsCountDischarged = value;
            }
        }

        private int patientsDiagnosedCount;
        public int PatientsDiagnosedCount
        {
            get { return patientsDiagnosedCount; }
            set
            {
                patientsCount += value - patientsDiagnosedCount;
                patientsDiagnosedCount = value;
            }
        }

        //add
        public void AddPatientsCuredBed(int count = 1)
        {
            PatientsCuredCountBed += count;
        }
        public void AddPatientsCuredDoctor(int count = 1)
        {
            PatientsCuredCountDoctor += count;
        }
        public void AddPatientsCuredVIP(int count = 1)
        {
            PatientsCuredCountVIP += count;
        }
        public void AddPatientsCuredKids(int count = 1)
        {
            PatientsCuredCountKids += count;
        }
        public void AddPatientsDiagnosed(int count = 1)
        {
            PatientsDiagnosedCount += count;
        }
        public void AddPatientsDischarged(int count = 1)
        {
            PatientsCountDischarged += count;
        }

        //set
        private void SetPatients(int count)
        {
            PatientsCount = count;
        }

        private void SetPatientsCured(int count)
        {
            PatientsCuredCount = count;
        }

        public void SetPatientsCuredBed(int count)
        {
            PatientsCuredCountBed = count;
        }
        public void SetPatientsCuredDoctor(int count)
        {
            PatientsCuredCountDoctor = count;
        }
        public void SetPatientsCuredVIP(int count)
        {
            PatientsCuredCountVIP = count;
        }
        public void SetPatientsCuredKids(int count)
        {
            PatientsCuredCountKids = count;
        }
        public void SetPatientsDiagnosed(int count)
        {
            PatientsDiagnosedCount = count;
        }
        public void SetPatientsDischarged(int count)
        {
            PatientsCountDischarged = count;
        }

        //save/load
        public string Save()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Checkers.CheckedAmount(PatientsCuredCountBed, 0, int.MaxValue, "PatientsCuredCountBed"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(PatientsCuredCountDoctor, 0, int.MaxValue, "PatientsCuredCountDoctor"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(PatientsCuredCountVIP, 0, int.MaxValue, "PatientsCuredCountVIP"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(PatientsCuredCountKids, 0, int.MaxValue, "PatientsCuredCountKids"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(PatientsCountDischarged, 0, int.MaxValue, "PatientsCountDischarged"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(PatientsDiagnosedCount, 0, int.MaxValue, "PatientsCountDiagnosed"));
            return builder.ToString();
        }

        public void Load(string save)
        {
            SetPatients(0);
            SetPatientsCured(0);
            patientsCuredCountBed = 0;
            patientsCuredCountDoctor = 0;
            patientsCuredCountVIP = 0;
            patientsCuredCountKids = 0;
            patientsDiagnosedCount = 0;
            patientsCountDischarged = 0;
            if (!string.IsNullOrEmpty(save))
            {
                var saveData = save.Split(';');
                SetPatientsCuredBed(int.Parse(saveData[0], System.Globalization.CultureInfo.InvariantCulture));
                SetPatientsCuredDoctor(int.Parse(saveData[1], System.Globalization.CultureInfo.InvariantCulture));
                SetPatientsCuredVIP(int.Parse(saveData[2], System.Globalization.CultureInfo.InvariantCulture));
                SetPatientsCuredKids(int.Parse(saveData[3], System.Globalization.CultureInfo.InvariantCulture));
                SetPatientsDischarged(int.Parse(saveData[4], System.Globalization.CultureInfo.InvariantCulture));
                if (saveData.Length > 5)
                {
                    SetPatientsDiagnosed(int.Parse(saveData[5], System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    SetPatientsDiagnosed(GetDiagnosedPatients());
                }
            }
            else
            {
                //get missing data from achievements

                //SetPatientsCuredBed
                //int tempCounter = 0;
                /* int stage = UIController.get.AchievementsPopUp.ac.AchievementList[7].stage;
                 for (int i = 0; i < stage; i++)
                 {
                     tempCounter += UIController.get.AchievementsPopUp.achievementDatabase.AchievementItem[7].requiredValues[i];
                 }
                 if (stage < 2)
                 {
                     tempCounter += UIController.get.AchievementsPopUp.ac.AchievementList[7].progress;
                 }
                 SetPatientsCuredBed((int)(tempCounter/2));//fake count of bed patients cured
                 */
                //SetPatientsCuredDoctor
                int tempCounter = 0;
                int stage = UIController.getHospital.AchievementsPopUp.ac.AchievementList[7].stage;
                for (int i = 0; i < stage; i++)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.achievementDatabase.AchievementItem[7].requiredValues[i];
                }
                if (stage < 2)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.ac.AchievementList[7].progress;
                }
                SetPatientsCuredDoctor(tempCounter);
                SetPatientsCuredBed((int)(tempCounter / 2));//fake count of bed patients cured

                //SetPatientsCuredKids
                tempCounter = 0;
                stage = UIController.getHospital.AchievementsPopUp.ac.AchievementList[11].stage;
                for (int i = 0; i < stage; i++)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.achievementDatabase.AchievementItem[11].requiredValues[i];
                }
                if (stage < 2)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.ac.AchievementList[11].progress;
                }
                SetPatientsCuredKids(tempCounter);


                //SetPatientsDiagnosed
                SetPatientsDiagnosed(GetDiagnosedPatients());
            }
        }

        private int GetDiagnosedPatients()
        {
            return HospitalDataHolder.Instance.GetDiagnosedPatients();
        }
    }

    public class CCuresCount
    {
        public CCuresCount(int producedMedicinesCount = 0, int producedElixirsCount = 0)
        {
            this.producedMedicinesCount = producedMedicinesCount;
            this.producedElixirsCount = producedElixirsCount;
        }

        private int producedMedicinesCount;
        public int ProducedMedicinesCount
        {
            get { return producedMedicinesCount; }
            private set { producedMedicinesCount = value; }
        }

        private int producedElixirsCount;
        public int ProducedElixirsCount
        {
            get { return producedElixirsCount; }
            private set { producedElixirsCount = value; }
        }

        private int producedPlantsCount;
        public int ProducedPlantsCount
        {
            get { return producedPlantsCount; }
            private set { producedPlantsCount = value; }
        }

        private int producedFungiCount;
        public int ProducedFungiCount
        {
            get { return producedFungiCount; }
            private set { producedFungiCount = value; }
        }

        public void AddProducedMedicines(int count = 1)
        {
            ProducedMedicinesCount += count;
        }

        public void AddProducedElixirs(int count = 1)
        {
            ProducedElixirsCount += count;
        }

        public void AddProducedPlants(int count = 1)
        {
            ProducedPlantsCount += count;
        }

        public void AddProducedFungi(int count = 1)
        {
            ProducedFungiCount += count;
        }

        private void SetProducedMedicines(int count)
        {
            ProducedMedicinesCount = count;
        }

        private void SetProducedElixirs(int count)
        {
            ProducedElixirsCount = count;
        }

        public void SetProducedPlants(int count)
        {
            ProducedPlantsCount = count;
        }

        public void SetProducedFungi(int count)
        {
            ProducedFungiCount = count;
        }

        public string Save()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Checkers.CheckedAmount(ProducedMedicinesCount, 0, int.MaxValue, "ProducedMedicinesCount"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(ProducedElixirsCount, 0, int.MaxValue, "ProducedElixirsCount"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(ProducedPlantsCount, 0, int.MaxValue, "ProducedPlantsCount"));
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(ProducedFungiCount, 0, int.MaxValue, "ProducedFungiCount"));
            return builder.ToString();
        }

        public void Load(string save)
        {
            producedMedicinesCount = 0;
            producedElixirsCount = 0;
            producedPlantsCount = 0;
            producedFungiCount = 0;

            if (!string.IsNullOrEmpty(save))
            {
                var saveData = save.Split(';');
                SetProducedMedicines(int.Parse(saveData[0], System.Globalization.CultureInfo.InvariantCulture));
                SetProducedElixirs(int.Parse(saveData[1], System.Globalization.CultureInfo.InvariantCulture));
                SetProducedPlants(int.Parse(saveData[2], System.Globalization.CultureInfo.InvariantCulture));
                SetProducedFungi(int.Parse(saveData[3], System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                //get missing data from achievements

                int tempCounter = 0;
                int stage = UIController.getHospital.AchievementsPopUp.ac.AchievementList[9].stage;
                for (int i = 0; i < stage; i++)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.achievementDatabase.AchievementItem[9].requiredValues[i];
                }
                if (stage < 2)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.ac.AchievementList[9].progress;
                }
                SetProducedMedicines(tempCounter);
                SetProducedElixirs(tempCounter * 4); //fake produced elixir count

                tempCounter = 0;
                stage = UIController.getHospital.AchievementsPopUp.ac.AchievementList[3].stage;
                for (int i = 0; i < stage; i++)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.achievementDatabase.AchievementItem[3].requiredValues[i];
                }
                if (stage < 2)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.ac.AchievementList[3].progress;
                }
                SetProducedPlants(tempCounter);

                tempCounter = 0;
                stage = UIController.getHospital.AchievementsPopUp.ac.AchievementList[14].stage;
                for (int i = 0; i < stage; i++)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.achievementDatabase.AchievementItem[14].requiredValues[i];
                }
                if (stage < 2)
                {
                    tempCounter += UIController.getHospital.AchievementsPopUp.ac.AchievementList[14].progress;
                }
                SetProducedFungi(tempCounter);


            }
        }
    }
}
