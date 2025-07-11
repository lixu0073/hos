using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hospital;
using SimpleUI;
using System.Text;
using System;
using System.Threading;
using Facebook.Unity;
using UnityEngine.Events;
using ScriptableEventSystem;
using UnityEngine.SceneManagement;

public abstract class BaseGameState : MonoBehaviour
{
    #region types

    public delegate void onLvl();

    #endregion

    #region SCENE LOADED
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static GameObject testObjects = null; // Reference to TestObjects or TestObjectsMaternity to show debug options on screen

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
#if !MH_RELEASE || MH_QA || MH_DEVELOP        
        string objName = "";
        switch (scene.name)
        {
            case "MainScene":
                objName = "TestObjects";
                break;

            case "MaternityScene":
                objName = "TestObjectsMaternity";
                break;
        }
        if (objName != "")
        {
            GameObject obj = GameObject.Find(objName);
            if (obj)
            {
                testObjects = obj.gameObject;
                testObjects.SetActive(false); // By default is not shown on screen
            }
        }
#endif
    }
    #endregion

    #region FieldsAndProperties
    public static bool isHoverOn = false;
    public static event Action Loaded;
    public static event onLvl OnLevelUp;
    private static int seed = Environment.TickCount;
    public static readonly System.Random rand = new System.Random(Interlocked.Increment(ref seed));
    private static readonly object syncLock = new object();
#pragma warning disable 0649
    [SerializeField] GameEvent expAdded;
#pragma warning restore 0649
    public UnityAction<MedicineRef> onMedicineAmountChanged;

    public void OnDestroy()
    {
        if (OnLevelUp != null)
        {
            foreach (onLvl onLevelUpcall in OnLevelUp.GetInvocationList())
            {
                OnLevelUp -= onLevelUpcall;
            }
        }
    }

    /// <summary>
    /// z minimalną, bez maxymalnej
    /// </summary>
    /// <param name="min">INCLUSIVE</param>
    /// <param name="max">EXCLUSIVE</param>
    /// <returns></returns>
    public static int RandomNumber(int min, int max)
    {
        lock (syncLock)
        {
            return rand.Next(min, max);
        }
    }

    public static int RandomNumber(int max)
    {
        lock (syncLock)
        {
            return rand.Next(0, max);
        }
    }

    public static float RandomFloat(float min, float max)
    {
        lock (syncLock)
        {
            return rand.Next((int)(min * 100), (int)(max * 100)) / 100f;
        }
    }

    private static Dictionary<string, int> buildedObjects;
    public static Dictionary<string, int> BuildedObjects
    {
        get
        {
            if (buildedObjects == null)
                buildedObjects = new Dictionary<string, int>();
            return buildedObjects;
        }
        protected set
        {
            buildedObjects = value;
        }
    }

    private static Dictionary<string, int> storedObjects;
    public static Dictionary<string, int> StoredObjects
    {
        get
        {
            if (storedObjects == null)
                storedObjects = new Dictionary<string, int>();
            return storedObjects;
        }
        private set
        {
            if (storedObjects == null)
                storedObjects = new Dictionary<string, int>();
            storedObjects = value;
        }
    }

    protected int[] expForLevel = new int[]{0,6,7,35,70,80,210,275,440,810,990,1200,1610,1810,2290,2870,3190,4150,4750,5400,6150,
        7800,9100,10640,12250,13690,15440,17890,20110,23410,26090,29020,32380,35410,38720,42190,45210,48950,54850,61830,67900,74990,
        81500,94450,100770,110940,119550,129450,138100,148440,157100,170000};

    public virtual int GetExpForLevel(int level)
    {
        if (level <= 50)
        {
            return expForLevel[level];
        }
        else
        {
            return expForLevel[50] + ((level - 50) * 11000);
        }
    }

    // contains medicines to random until player achieve lvl 10
    public List<MedicinePermutations> MedicinePermutationsList = new List<MedicinePermutations>();
    public int MedicinePermutationsCounter = 0;
    public List<MedicineRef> LastMedicineRndPool = new List<MedicineRef>();

    public hospitalBoosters HospitalBoosters = new hospitalBoosters();
    public int AdsWatched;
    public void IncrementAdsWatched()
    {
        ++AdsWatched;
        if (AdsWatched % 10 == 0)
        {
            TenjinController.instance.ReportAdsWatched(AdsWatched);
        }
    }

    public int CoinAmountFromIAP { get; set; }
    public int DiamondAmountFromIAP { get; set; }

    public int Diamonds { get; set; }
    public int Coins { get; set; }

    protected bool AddAdsReward = false;
    protected string AdType;

    public bool IsAdRewardActive()
    {
        return AddAdsReward;
    }

    public void SetIsAdRewardActive(bool AddAdsReward)
    {
        this.AddAdsReward = AddAdsReward;
    }

    public string GetAdType()
    {
        return AdType;
    }

    public void SetAdType(string AdType)
    {
        this.AdType = AdType;
    }

    public string LastSuccessfulPurchase;

    public string GetLastSuccessfulPurchase()
    {
        return LastSuccessfulPurchase;
    }

    public void SetLastSuccessfulPurchase(string lastSuccessfulPurchase)
    {
        LastSuccessfulPurchase = lastSuccessfulPurchase;
    }

    protected int ExperienceAmount;

    public string OtherMapBadgesToShow { get; protected set; }

    public int PositiveEnergyAmount { get; set; }

    public string PersonalFriendCode { get; set; }

    public List<string> RandomFriendsIds { get; set; }

    public int RandomFriendsTimestamp { get; set; }

    public string Version { get; protected set; }

    public int ActionsCounterForExtraGift { get; protected set; }

    public int ActionsCounterForExtraGiftStorage { get; protected set; }

    public int ActionsCounterForExtraGiftTank { get; protected set; }

    public float GameplayTimer = 0;

    public bool NoMoreRemindFBConnection = false;
    public bool FBConnectionRewardEnabled = false;
    public bool FBRewardConnectionClaimed = false;

    public bool HomePharmacyVisited = false;
    public bool EverLoggedInFB = false;

    private int[] ExtraGiftRange = new int[] { 60, 70 };

    private BalanceableInt storageToolsMinRangeBalanceable;
    private BalanceableInt storageToolsMaxRangeBalanceable;

    private int StorageToolsMinRange
    {
        get
        {
            if (storageToolsMinRangeBalanceable == null)
            {
                storageToolsMinRangeBalanceable = BalanceableFactory.CreateStorageToolsMinRangeBalanceable();
            }

            return storageToolsMinRangeBalanceable.GetBalancedValue();
        }
    }

    private int StorageToolsMaxRange
    {
        get
        {
            if (storageToolsMaxRangeBalanceable == null)
            {
                storageToolsMaxRangeBalanceable = BalanceableFactory.CreateStorageToolsMaxRangeBalanceable();
            }

            return storageToolsMaxRangeBalanceable.GetBalancedValue();
        }
    }

    private int[] ExtraGiftRangeStorage
    {
        get
        {
            return new int[] { StorageToolsMinRange, StorageToolsMaxRange };
        }
    }

    private BalanceableInt tankToolsMinRangeBalanceable;
    private BalanceableInt tankToolsMaxRangeBalanceable;

    private int tankToolsMinRange
    {
        get
        {
            if (tankToolsMinRangeBalanceable == null)
            {
                tankToolsMinRangeBalanceable = BalanceableFactory.CreateTankToolsMinRangeBalanceable();
            }

            return tankToolsMinRangeBalanceable.GetBalancedValue();
        }
    }

    private int tankToolsMaxRange
    {
        get
        {
            if (tankToolsMaxRangeBalanceable == null)
            {
                tankToolsMaxRangeBalanceable = BalanceableFactory.CreateTankToolsMaxRangeBalanceable();
            }

            return tankToolsMaxRangeBalanceable.GetBalancedValue();
        }
    }

    private int[] ExtraGiftRangeTank
    {
        get
        {
            return new int[] { tankToolsMinRange, tankToolsMaxRange };
        }
    }

    //private int ExtraGiftThresholdValue = 0;
    private int ExtraGiftThresholdValueStorage = 0;
    private int ExtraGiftThresholdValueTank = 0;

    public string HospitalName { get; set; }

    public string GetHospitalName()
    {
        return HospitalName;
    }

    public int elixirStorageAmount
    {
        get
        {
            return ElixirStore.actualAmount;
        }
        set
        {
            ElixirStore.SetActualAmount(value);
        }
    }

    public int maximimElixirStorageAmount
    {
        get
        {
            return ElixirStore.maximumAmount;
        }
    }

    public int elixirTankAmount
    {
        get
        {
            return ElixirTank.actualAmount;
        }
        set
        {
            ElixirTank.SetActualAmount(value);
        }
    }

    public int maximimElixirTankAmount
    {
        get
        {
            return ElixirTank.maximumAmount;
        }
    }

    public int hospitalLevel { get; protected set; }

    public int maternityLevel { get; protected set; }


    public int GetHospitalLevel()
    {
        return hospitalLevel;
    }

    public int GetMaternityLevel()
    {
        return maternityLevel;
    }

    public int IAPPurchasesCount;

    public void IncrementIAPPurchasesCount()
    {
        ++IAPPurchasesCount;
    }

    public int GetIAPPurchasesCount()
    {
        return IAPPurchasesCount;
    }

    protected bool HasSomeFriends = false;
    public bool HasAnyFriends()
    {
        return HasSomeFriends;
    }

    public void SetHasAnyFriends(bool hasAny)
    {
        HasSomeFriends = hasAny;
    }

    protected bool HasSomeFollowers = false;
    public bool HasAnyFollowers()
    {
        return HasSomeFollowers;
    }

    public void SetHasAnyFollowers(bool hasAny)
    {
        HasSomeFollowers = hasAny;
    }

    protected bool HasSomeFollowings = false;
    public bool HasAnyFollowings()
    {
        return HasSomeFollowings;
    }

    public void SetHasAnyFollowings(bool hasAny)
    {
        HasSomeFollowings = hasAny;
    }

    public bool IAPBoughtLately;

    public void SetIAPBoughtLately(bool buyLately)
    {
        IAPBoughtLately = buyLately;
    }

    public bool DiamondUsedLately;

    public bool StarterPackUsed;

    public string LootBoxTransaction { get; set; }

    public bool AddLootBoxReward { get; set; }

    public int IAPValentineCount;

    public void IncrementIAPValentineCount()
    {
        ++IAPValentineCount;
    }

    public int GetIAPValentineCount()
    {
        return IAPValentineCount;
    }

    public int IAPEasterCount;

    public void IncrementIAPEasterCount()
    {
        ++IAPEasterCount;
    }

    public int GetIAPEasterCount()
    {
        return IAPEasterCount;
    }

    public int IAPLabourDayCount;
    public int IAPCancerDayCount;

    private LocalizationType localizationLang = LocalizationType.en;

    public LocalizationType LocalizationLang
    {
        get { return localizationLang; }
        set { localizationLang = value; }
    }

    public long timedOfferEndDate;
    public long timedOfferPurchaseDate;

    public bool EverOpenedCrossPromotionPopup { get; set; }
    public bool CrossPromotionCompleted { get; set; }

    [SerializeField]
    protected Dictionary<int, int> resources = new Dictionary<int, int>();

    protected Coroutine ShowPopupCoroutine;

    protected List<Rotations> BadgedMachines = new List<Rotations>();

    bool[] communityRewards = new bool[4] { true, true, true, true };

    [HideInInspector]
    public static bool saveFilePrepared = false;
    #endregion

    #region ResourceManipulation

    public int CanAddResource(MedicineRef type, int amount, bool canExceedLimit)
    {
        if (type.IsMedicineForTankElixir())
        {
            if (!CanAddAmountForTankStorage(amount) && !canExceedLimit)
            {
                return 0;
            }
        }
        else
        {
            if (!CanAddAmountForElixirStorage(amount) && !canExceedLimit)
            {
                return 0;
            }
        }

        return 1;
    }

    public int GetTankSorageLeftCapacity()
    {
        return Math.Max(maximimElixirTankAmount - elixirTankAmount, 0);
    }

    public int AddResource(MedicineRef type, int amount, bool canExceedLimit, EconomySource source)
    {
        if (type.IsMedicineForTankElixir())
        {
            if (!CanAddAmountForTankStorage(amount) && !canExceedLimit)
            {
                amount = maximimElixirTankAmount - elixirTankAmount;
                print("Elixr Storage Full");
                return 0;
            }
            elixirTankAmount += amount;
        }
        else
        {
            if (!CanAddAmountForElixirStorage(amount) && !canExceedLimit)
            {
                amount = maximimElixirStorageAmount - elixirStorageAmount;
                print("Elixr Storage Full");
                return 0;
            }
            elixirStorageAmount += amount;
        }

        if (!resources.ContainsKey(100 * (int)type.type + type.id))
        {
            resources.Add(100 * (int)type.type + type.id, 0);
        }

        resources[100 * (int)type.type + type.id] += amount;

        NotificationCenter.Instance.ResourceAmountChanged.Invoke(new ResourceAmountChangedEventArgs(type, amount, resources[100 * (int)type.type + type.id], source));

        onMedicineAmountChanged?.Invoke(type);

        if (ReferenceHolder.GetHospital() != null && ReferenceHolder.GetHospital().Epidemy.Outbreak)
        {
            if (UIController.getHospital != null)
            {
                UIController.getHospital.EpidemyOnPopUp.GetComponent<EpidemyOnPopUpController>().RefreshMedicinesStatus();
            }
        }

        SaveSynchronizer.Instance.MarkToSave(SavePriorities.MedicineManipulation * amount);

        return amount;
    }

    public int GetLevelSceneDependent()
    {
        if (this is GameState)
        {
            return hospitalLevel;
        }
        else
        {
            return maternityLevel;
        }
    }

    public bool CanAddAmountForElixirStorage(int amount)
    {
        return maximimElixirStorageAmount >= amount + elixirStorageAmount;
    }

    public bool CanAddAmountForTankStorage(int amount)
    {
        return maximimElixirTankAmount >= amount + elixirTankAmount;
    }

    public int GetCureCount(MedicineRef type)
    {
        if (!resources.ContainsKey(100 * (int)type.type + type.id))
            return 0;
        return resources[100 * (int)type.type + type.id];
    }

    public int CheckCureCount(MedicineRef type)
    {
        if (!resources.ContainsKey(100 * (int)type.type + type.id))
            return -1;
        return resources[100 * (int)type.type + type.id];
    }

    public bool GetCure(MedicineRef type, int amount, EconomySource source)
    {
        if (!resources.ContainsKey(100 * (int)type.type + type.id))
            return false;
        if (resources[100 * (int)type.type + type.id] < amount)
            return false;

        if (type.IsMedicineForTankElixir())
            elixirTankAmount -= amount;
        else
            elixirStorageAmount -= amount;

        NotificationCenter.Instance.ResourceAmountChanged.Invoke(new ResourceAmountChangedEventArgs(type, amount, resources[100 * (int)type.type + type.id], source));
        resources[100 * (int)type.type + type.id] -= amount;
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.MedicineManipulation * amount);
        NotificationCenter.Instance.ElixirDelivered.Invoke(new ElixirDeliveredEventArgs());
        onMedicineAmountChanged?.Invoke(type);
        return true;
    }

    public bool GetPrerequisitesForMedicine(MedicineRef medicine, MedicineProductionMachine machine = null)
    {
        var z = ResourcesHolder.Get().medicines.cures[(int)medicine.type].medicines[medicine.id];
        var medicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();
        int amount = 0;
        foreach (var p in z.Prerequisities)
        {
            if ((amount = GetCureCount(p.medicine.GetMedicineRef())) < p.amount)
                medicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(p.amount - amount, p.medicine));
        }
        if (medicines.Count > 0)
        {
            UIController.get.BuyResourcesPopUp.Open(medicines, false, false, false, () =>
            {
                if (machine)
                {
                    machine.AddMedicineToQueue(medicine);
                    machine.ShowMachineHoover();
                }
            }, () =>
            {
                if (machine)
                {
                    machine.ShowMachineHoover();
                }
            }, null);
            return false;
        }
        int counter = 0;
        foreach (var p in z.Prerequisities)
        {
            GetCure(p.medicine.GetMedicineRef(), p.amount, EconomySource.ProductionMachine);
            if (machine)
            {
                ReferenceHolder.Get().giftSystem.CreateItemUsed(machine.transform.position + new Vector3(1f, 1, 1f), p.amount, counter * .75f, p.medicine.image);
                counter++;
            }
        }
        return true;
    }


    private List<KeyValuePair<int, int>> EnumerateResources()
    {
        return resources.Where((x) => { return x.Value != 0; }).ToList();
    }

    public Dictionary<int, int> GetStorageResources()
    {
        return resources;
    }

    public int ResourceCount()
    {
        return EnumerateResources().Count;
    }

    public List<KeyValuePair<MedicineRef, int>> EnumerateResourcesMedRef()
    {
        return EnumerateResources().Select((x) => { return new KeyValuePair<MedicineRef, int>(new MedicineRef((MedicineType)(x.Key / 100), x.Key % 100), x.Value); }).ToList();
    }

    public void AddResource(ResourceType type, int amount, EconomySource source, bool updateCounter, string buildingTag = null)
    {
        switch (type)
        {
            case ResourceType.Coin:
                AddCoins(amount, source, updateCounter, buildingTag);
                break;
            case ResourceType.Exp:
                AddExperience(amount, source, updateCounter, buildingTag);
                break;
            case ResourceType.Diamonds:
                AddDiamonds(amount, source, updateCounter, false, buildingTag);
                break;
            case ResourceType.PositiveEnergy:
                AddPositiveEnergy(amount, source, buildingTag);
                break;
            default:
                break;
        }
    }

    public void UpdateCounter(ResourceType type, int amount, int amountBeforeOperation)
    {
        //Debug.LogWarning("UpdateCounter " + type + " " + amount);
        switch (type)
        {
            case ResourceType.Coin:
                UIController.get.coinCounter.AddToCounter(amount, amountBeforeOperation);
                if (amount > 0)
                    SoundsController.Instance.PlayCoinIncrease();
                break;
            case ResourceType.Exp:
                if (amount > 0)
                    SoundsController.Instance.PlayXPIncrease();
                RefreshXPBar();
                break;
            case ResourceType.Diamonds:
                UIController.get.diamondCounter.AddToCounter(amount, amountBeforeOperation);
                if (amount > 0)
                    SoundsController.Instance.PlayDiamondSpend();
                break;
            case ResourceType.PositiveEnergy:
                if (amount > 0)
                    SoundsController.Instance.PlayPositiveEnergyIncrease();
                break;
            default:
                break;
        }
    }

    public void AddCoins(int amount, EconomySource source, bool updateCounter = true, string buildingTag = null)
    {
        Coins += amount;
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.CoinManipulation);
        AnalyticsGeneralParameters.softCurrency = Coins;
        AnalyticsController.instance.ReportInGameItem(EconomyAction.Earn, ResourceType.Coin, source, amount, MedicineType.BaseElixir, -1, -1, buildingTag);

        if (source == EconomySource.IAP)
        {
            CoinAmountFromIAP += amount;
        }

        if (updateCounter)
        {
            UIController.get.coinCounter.AddToCounter(amount, Coins - amount);
            if (amount > 0)
                SoundsController.Instance.PlayCoinIncrease();
        }
    }

    public void RemoveCoins(int amount, EconomySource source, bool updateCounter = true, string buildingTag = null)
    {
        Coins -= amount;
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.CoinManipulation);

        if (updateCounter)
        {
            UIController.get.coinCounter.SubstractFromCounter(amount, Coins + amount);
        }
        AnalyticsGeneralParameters.softCurrency = Coins;
        AnalyticsController.instance.ReportInGameItem(EconomyAction.Spend, ResourceType.Coin, source, amount, MedicineType.BaseElixir, -1, -1, buildingTag);

        if (amount > 0)
            SoundsController.Instance.PlayCashing();
    }

    public void AddDiamonds(int amount, EconomySource source, bool updateCounter = true, bool fromIAP = false, string buildingTag = null)
    {
        Diamonds += amount;

        if (source == EconomySource.IAP)
        {
            DiamondAmountFromIAP += amount;
        }

        if (!fromIAP)
            SaveSynchronizer.Instance.InstantSave();

        AnalyticsGeneralParameters.hardCurrency = Diamonds;
        AnalyticsController.instance.ReportInGameItem(EconomyAction.Earn, ResourceType.Diamonds, source, amount, MedicineType.BaseElixir, -1, -1, buildingTag);
        if (updateCounter)
        {
            SoundsController.Instance.PlayDiamondSpend();
            //if (amount > 0)
            UIController.get.diamondCounter.AddToCounter(amount, Diamonds - amount);
        }
    }

    public void RemoveDiamonds(int amount, EconomySource source, string buildingTag = null)
    {
        Diamonds -= amount;
        //SaveDynamoConnector.Instance.InstantSave();
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.DiamondManipulation);
        UIController.get.diamondCounter.SubstractFromCounter(amount, Diamonds + amount);
        AnalyticsGeneralParameters.hardCurrency = Diamonds;
        AnalyticsController.instance.ReportInGameItem(EconomyAction.Spend, ResourceType.Diamonds, source, amount, MedicineType.BaseElixir, -1, -1, buildingTag);

        if (amount > 0)
            SoundsController.Instance.PlayDiamondSpend();

        // Only check coins level so should be light ops

        DiamondUsedLately = true;
        //  HintsController.Get().UpdateHintsWithType(HintType.BuildingRotatableObject);
    }

    public void AddPositiveEnergy(int amount, EconomySource source, string buildingTag = null)
    {
        SoundsController.Instance.PlayPositiveEnergyIncrease();
        PositiveEnergyAmount += amount;
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.PositiveEnergyManipulation);
        AnalyticsController.instance.ReportInGameItem(EconomyAction.Earn, ResourceType.PositiveEnergy, source, amount, MedicineType.BaseElixir, -1, -1, buildingTag);
    }

    public void RemovePositiveEnergy(int amount, EconomySource source, string buildingTag = null)
    {
        PositiveEnergyAmount -= amount;
        SaveSynchronizer.Instance.MarkToSave(SavePriorities.PositiveEnergyManipulation);
        AnalyticsController.instance.ReportInGameItem(EconomyAction.Spend, ResourceType.PositiveEnergy, source, amount, MedicineType.BaseElixir, -1, -1, buildingTag);
    }

    [TutorialTriggerable]
    public void AddExperienceWithAnimationFromTutorial(int amount)
    {
        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
        AddResource(ResourceType.Exp, amount, EconomySource.Tutorial, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, Vector3.zero, amount, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
        {
            Game.Instance.gameState().UpdateCounter(ResourceType.Exp, amount, currentExpAmount);
        });
    }

    public void AddExperience(int amount, EconomySource source, bool updateCounter, string buildingTag = null)
    {
        if (amount <= 0)
            return;

        ExperienceAmount += amount;
        expAdded?.Invoke(this, new ExpAddedEventArgs(ExperienceAmount, amount, source));

        if (updateCounter)
        {
            SoundsController.Instance.PlayXPIncrease();
            RefreshXPBar();
        }

        NotificationCenter.Instance.ExpAmountChanged.Invoke(new ExpAmountChangedEventArgs(hospitalLevel, ExperienceAmount));

        if (!HospitalAreasMapController.Map.VisitingMode)
        {
            if (CheckIfLevelUP())
            {
                NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Notification -= CheckExpDependantTutorial;
                NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Notification += CheckExpDependantTutorial;
            }
            else
            {
                CheckExpDependantTutorial(new BaseNotificationEventArgs());
            }
        }
        if (this is MaternityGameState)
        {
            AnalyticsGeneralParameters.maternityUserXP = ExperienceAmount;
        }
        else
        {
            AnalyticsGeneralParameters.userXP = ExperienceAmount;
        }
    }

    public virtual bool CheckIfLevelUP()
    {
        bool isLevelUP = false;

        if (HospitalAreasMapController.Map.VisitingMode)
        {
            return isLevelUP;
        }

        while (ExperienceAmount >= GetExpForLevel(hospitalLevel))
        {
            ExperienceAmount -= GetExpForLevel(hospitalLevel);
            LevelUp();

            isLevelUP = true;

            if (hospitalLevel == 1)
            {
                AnalyticsController.instance.ReportBug("level_zero");
                isLevelUP = false;
            }
        }

        return isLevelUP;
    }

    public void CheckExpDependantTutorial(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Notification -= CheckExpDependantTutorial;

        TutorialProgressChecker.GetInstance().CheckHardSkipOnExp(hospitalLevel, ExperienceAmount);

        HospitalTutorialStep currentStepData = TutorialController.Instance.GetCurrentStepData();
        if (currentStepData.NecessaryCondition == Condition.ObjectDoesNotExistOnLevel)
        {
            NotificationCenter.Instance.ObjectExistOnLevel.Invoke(new ObjectExistOnLevelEventArgs());
        }
        else if (currentStepData.NecessaryCondition == Condition.ExpAmountChanged)
        {
            NotificationCenter.Instance.ExpAmountChanged.Invoke(new ExpAmountChangedEventArgs(hospitalLevel, ExperienceAmount));
        }

        if (hospitalLevel == 9 && ExperienceAmount >= 400 || hospitalLevel > 9)
        {
            NotificationCenter.Instance.ExpAmountChangedNonLinear.Invoke(new ExpAmountChangedEventArgs(hospitalLevel, ExperienceAmount));
        }

        if (hospitalLevel >= 8 && ExperienceAmount >= 300)
        {
            if (hospitalLevel > 8 && !TutorialController.Instance.IsNonLinearStepCompleted(StepTag.NL_push_disabled))
                TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_push_disabled);
            else
            {
                if (LocalNotificationController.Instance.IsAllNotificationsOff())
                    NotificationCenter.Instance.PushNotificationsDisabled.Invoke(new BaseNotificationEventArgs());
                else
                    TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_push_disabled);
            }
        }

    }

    public virtual void RefreshXPBar()
    {
        UIController.get.XPBar.SetValue(ExperienceAmount);
        UIController.get.XPBar.SetMaxValue(GetExpForLevel(hospitalLevel));
    }

    public void UpdateExtraGift(Vector3 position, bool isDoctor, SpecialItemTarget target = SpecialItemTarget.All)
    {
        if (hospitalLevel < 3)    //no extra gifts on early levels
            return;

        switch (target)
        {
            case SpecialItemTarget.All:
                ++ActionsCounterForExtraGiftStorage;
                ++ActionsCounterForExtraGiftTank;
                break;
            case SpecialItemTarget.Storage:
                ++ActionsCounterForExtraGiftStorage;
                break;
            case SpecialItemTarget.Tank:
                ++ActionsCounterForExtraGiftTank;
                break;
            default:
                break;
        }

        MedicineRef screwdriver = new MedicineRef(MedicineType.Special, 0);
        MedicineRef hammer = new MedicineRef(MedicineType.Special, 1);
        MedicineRef spanner = new MedicineRef(MedicineType.Special, 2);
        MedicineRef gum = new MedicineRef(MedicineType.Special, 4);
        MedicineRef metal = new MedicineRef(MedicineType.Special, 5);
        MedicineRef pipe = new MedicineRef(MedicineType.Special, 6);

        if (ExtraGiftThresholdValueStorage == 0)
        {
            if (ElixirStore.actualLevel == 1 && (GetCureCount(screwdriver) == 0 || GetCureCount(hammer) == 0 || GetCureCount(spanner) == 0))
                ExtraGiftThresholdValueStorage = RandomNumber(ExtraGiftRangeStorage[0] / 8, ExtraGiftRangeStorage[1] / 8);      //4 times bigger chance for the first upgrade when the first 3 items are not given yet
            else
                ExtraGiftThresholdValueStorage = RandomNumber(ExtraGiftRangeStorage[0], ExtraGiftRangeStorage[1]);
        }
        if (ExtraGiftThresholdValueTank == 0)
        {
            if (ElixirTank.actualLevel == 1 && (GetCureCount(gum) == 0 || GetCureCount(metal) == 0 || GetCureCount(pipe) == 0))
                ExtraGiftThresholdValueTank = RandomNumber(ExtraGiftRangeTank[0] / 8, ExtraGiftRangeTank[1] / 8);      //4 times bigger chance for the first upgrade when the first 3 items are not given yet
            else
                ExtraGiftThresholdValueTank = RandomNumber(ExtraGiftRangeTank[0], ExtraGiftRangeTank[1]);
        }

        MedicineRef extraMedicine = null;

        if (ExtraGiftThresholdValueStorage <= ActionsCounterForExtraGiftStorage && ExtraGiftThresholdValueTank <= ActionsCounterForExtraGiftTank)
        {
            extraMedicine = GetRandomSpecial();
        }
        else if (ExtraGiftThresholdValueStorage <= ActionsCounterForExtraGiftStorage)
        {
            extraMedicine = GetRandomSpecial(SpecialItemTarget.Storage);
        }
        else if (ExtraGiftThresholdValueTank <= ActionsCounterForExtraGiftTank)
        {
            extraMedicine = GetRandomSpecial(SpecialItemTarget.Tank);
        }
        else
        {
            return;
        }

        SpecialItemTarget itemTarget = extraMedicine.GetSpecialItemTarget();

        AddExtraGift(extraMedicine, position, isDoctor);

        if (itemTarget == SpecialItemTarget.Tank)
        {

            ActionsCounterForExtraGiftTank = 0;

            if (ElixirTank.actualLevel == 1 && (GetCureCount(gum) == 0 || GetCureCount(metal) == 0 || GetCureCount(pipe) == 0))
                ExtraGiftThresholdValueTank = RandomNumber(ExtraGiftRangeTank[0] / 8, ExtraGiftRangeTank[1] / 8);      //4 times bigger chance for the first upgrade when the first 3 items are not given yet
            else
                ExtraGiftThresholdValueTank = RandomNumber(ExtraGiftRangeTank[0], ExtraGiftRangeTank[1]);

            if (target == SpecialItemTarget.All)
                --ActionsCounterForExtraGiftStorage;
        }
        else if (itemTarget == SpecialItemTarget.Storage)
        {

            ActionsCounterForExtraGiftStorage = 0;
            if (ElixirStore.actualLevel == 1 && (GetCureCount(screwdriver) == 0 || GetCureCount(hammer) == 0 || GetCureCount(spanner) == 0))
                ExtraGiftThresholdValueStorage = RandomNumber(ExtraGiftRangeStorage[0] / 8, ExtraGiftRangeStorage[1] / 8);      //4 times bigger chance for the first upgrade when the first 3 items are not given yet
            else
                ExtraGiftThresholdValueStorage = RandomNumber(ExtraGiftRangeStorage[0], ExtraGiftRangeStorage[1]);

            if (target == SpecialItemTarget.All)
                --ActionsCounterForExtraGiftTank;
        }
        else
        {
            ActionsCounterForExtraGiftTank = 0;

            if (ElixirTank.actualLevel == 1 && (GetCureCount(gum) == 0 || GetCureCount(metal) == 0 || GetCureCount(pipe) == 0))
                ExtraGiftThresholdValueTank = RandomNumber(ExtraGiftRangeTank[0] / 8, ExtraGiftRangeTank[1] / 8);      //4 times bigger chance for the first upgrade when the first 3 items are not given yet
            else
                ExtraGiftThresholdValueTank = RandomNumber(ExtraGiftRangeTank[0], ExtraGiftRangeTank[1]);

            ActionsCounterForExtraGiftStorage = 0;
            if (ElixirStore.actualLevel == 1 && (GetCureCount(screwdriver) == 0 || GetCureCount(hammer) == 0 || GetCureCount(spanner) == 0))
                ExtraGiftThresholdValueStorage = RandomNumber(ExtraGiftRangeStorage[0] / 8, ExtraGiftRangeStorage[1] / 8);      //4 times bigger chance for the first upgrade when the first 3 items are not given yet
            else
                ExtraGiftThresholdValueStorage = RandomNumber(ExtraGiftRangeStorage[0], ExtraGiftRangeStorage[1]);
        }
    }

    public void AddExtraGift(MedicineRef extraMedicine, Vector3 position, bool isDoctor = false)
    {
        UIController.get.storageCounter.Add(false, isDoctor);
        AddResource(extraMedicine, 1, true, EconomySource.BonusItem);
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, position, 1, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(extraMedicine), null, () =>
        {
            UIController.get.storageCounter.Remove(1, false, isDoctor);
        });
    }

    void Update()
    {
        GameplayTimer += Time.deltaTime;
    }

    protected void SendOnLevelUpEvent()
    {
        OnLevelUp?.Invoke();
    }

    public MedicineRef GetRandomSpecial(DrawShovelSource drawShovelSource = DrawShovelSource.Standard)
    {
        MedicineRef screwdriver = new MedicineRef(MedicineType.Special, 0);
        MedicineRef hammer = new MedicineRef(MedicineType.Special, 1);
        MedicineRef spanner = new MedicineRef(MedicineType.Special, 2);
        MedicineRef gum = new MedicineRef(MedicineType.Special, 4);
        MedicineRef metal = new MedicineRef(MedicineType.Special, 5);
        MedicineRef pipe = new MedicineRef(MedicineType.Special, 6);

        List<KeyValuePair<MedicineRef, int>> specialInventoryAmounts = new List<KeyValuePair<MedicineRef, int>>();
        specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(screwdriver, GetCureCount(screwdriver)));
        specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(hammer, GetCureCount(hammer)));
        specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(spanner, GetCureCount(spanner)));
        specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(gum, GetCureCount(gum)));
        specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(metal, GetCureCount(metal)));
        specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(pipe, GetCureCount(pipe)));

        specialInventoryAmounts = specialInventoryAmounts.OrderBy(i => i.Value).ToList();

        //CASE 1:
        //if elixir storage has never been upgraded, we make sure that first 3 special items will be the ones needed for upgrade.
        if (ElixirStore.actualLevel == 1 && ElixirTank.actualLevel == 1)
        {
            //Debug.LogError("Elixir store is on level 1, giving not random special");
            if (specialInventoryAmounts[0].Value == 0)
                return specialInventoryAmounts[0].Key;
            else if (specialInventoryAmounts[1].Value == 0)
                return specialInventoryAmounts[1].Key;
            else if (specialInventoryAmounts[2].Value == 0)
                return specialInventoryAmounts[2].Key;
            else if (specialInventoryAmounts[3].Value == 0)
                return specialInventoryAmounts[3].Key;
            else if (specialInventoryAmounts[4].Value == 0)
                return specialInventoryAmounts[4].Key;
            else if (specialInventoryAmounts[5].Value == 0)
                return specialInventoryAmounts[5].Key;
            else
                return specialInventoryAmounts[UnityEngine.Random.Range(0, 6)].Key;
        }

        //CASE 2:
        //if player is above level on which plantation unlocks(15?) there's a 20% chance to get a SHOVEL.        

        MedicineRef med = TryToDrawShovel(drawShovelSource);
        if (med != null)
        {
            return med;
        }

        //CASE 3:
        //if item amounts are equal, get a random one (no chance favor for first on the list)
        if (specialInventoryAmounts[0].Value == specialInventoryAmounts[1].Value
            && specialInventoryAmounts[0].Value == specialInventoryAmounts[2].Value
            && specialInventoryAmounts[0].Value == specialInventoryAmounts[3].Value
            && specialInventoryAmounts[0].Value == specialInventoryAmounts[4].Value
            && specialInventoryAmounts[0].Value == specialInventoryAmounts[5].Value)
            return specialInventoryAmounts[UnityEngine.Random.Range(0, 6)].Key;

        //CASE 4:
        //if previous cases are not met, randomize special item with the following chances: 36%, 34%, 30%. The more item you have, the more chance you will get more of it.
        //new chances after introducing 2nd storage: 20/18/17/16/15/14%

        float rand = RandomFloat(0, 1);
        if (rand < .20f)        //20% chance to get the item player has the MOST of
            return specialInventoryAmounts[5].Key;
        else if (rand < .38f)    //18% chance to get the second item
            return specialInventoryAmounts[4].Key;
        else if (rand < .55f)    //17%    
            return specialInventoryAmounts[3].Key;
        else if (rand < .71f)    //16%    
            return specialInventoryAmounts[2].Key;
        else if (rand < .86f)    //15%    
            return specialInventoryAmounts[1].Key;
        else                     //14% chance to get the item player has the LEAST of
            return specialInventoryAmounts[0].Key;
    }

    public enum DrawShovelSource
    {
        Standard,
        GoodieBox1,
        GoodieBox2,
        GoodieBox3,
        VIPBox,
        EpidemyBox,
        TreasureChest,
        DailyQuest
    }

    private BalanceableFloat drawShovelChanceBalanceable;
    private float DrawShovelChance
    {
        get
        {
            if (drawShovelChanceBalanceable == null)
            {
                drawShovelChanceBalanceable = BalanceableFactory.CreateShovelDrawChanceBalanceable();
            }

            return drawShovelChanceBalanceable.GetBalancedValue();
        }
    }

    private BalanceableFloat shovelChanceFromGoodieBox1Balanceable;
    private BalanceableFloat shovelChanceFromGoodieBox2Balanceable;
    private BalanceableFloat shovelChanceFromGoodieBox3Balanceable;
    private BalanceableFloat shovelChanceFromVIPBalanceable;
    private BalanceableFloat shovelChanceFromEpidemyBoxBalanceable;
    private BalanceableFloat shovelChanceFromTreasureChestBalanceable;

    private float ShovelChanceFromGoodieBox1
    {
        get
        {
            if (shovelChanceFromGoodieBox1Balanceable == null) { shovelChanceFromGoodieBox1Balanceable = BalanceableFactory.CreateShovelDrawFromGoodieBox1Balanceable(); }
            return shovelChanceFromGoodieBox1Balanceable.GetBalancedValue();
        }
    }

    private float ShovelChanceFromGoodieBox2
    {
        get
        {
            if (shovelChanceFromGoodieBox2Balanceable == null) { shovelChanceFromGoodieBox2Balanceable = BalanceableFactory.CreateShovelDrawFromGoodieBox2Balanceable(); }
            return shovelChanceFromGoodieBox2Balanceable.GetBalancedValue();
        }
    }

    private float ShovelChanceFromGoodieBox3
    {
        get
        {
            if (shovelChanceFromGoodieBox3Balanceable == null) { shovelChanceFromGoodieBox3Balanceable = BalanceableFactory.CreateShovelDrawFromGoodieBox3Balanceable(); }
            return shovelChanceFromGoodieBox3Balanceable.GetBalancedValue();
        }
    }

    private float ShovelChanceFromVIP
    {
        get
        {
            if (shovelChanceFromVIPBalanceable == null) { shovelChanceFromVIPBalanceable = BalanceableFactory.CreateShovelDrawFromVIPBalanceable(); }
            return shovelChanceFromVIPBalanceable.GetBalancedValue();
        }
    }

    private float ShovelChanceFromEpidemyBox
    {
        get
        {
            if (shovelChanceFromEpidemyBoxBalanceable == null) { shovelChanceFromEpidemyBoxBalanceable = BalanceableFactory.CreateShovelDrawFromEpidemyBoxBalanceable(); }
            return shovelChanceFromEpidemyBoxBalanceable.GetBalancedValue();
        }
    }

    private float ShovelChanceFromTreasureChest
    {
        get
        {
            if (shovelChanceFromTreasureChestBalanceable == null) { shovelChanceFromTreasureChestBalanceable = BalanceableFactory.CreateShovelDrawFromTreasureChestBalanceable(); }
            return shovelChanceFromTreasureChestBalanceable.GetBalancedValue();
        }
    }

    private MedicineRef TryToDrawShovel(DrawShovelSource drawShovelSource = DrawShovelSource.Standard)
    {
        if (hospitalLevel < 15)
            return null;
        float shovelDrawChance = 0;

        switch (drawShovelSource)
        {
            case DrawShovelSource.Standard:
            default:
                shovelDrawChance = DrawShovelChance;
                break;
            case DrawShovelSource.GoodieBox1:
                shovelDrawChance = ShovelChanceFromGoodieBox1; //BundleManager.GetShowelChanceFromGoodieBox1();
                break;
            case DrawShovelSource.GoodieBox2:
                shovelDrawChance = ShovelChanceFromGoodieBox2; //BundleManager.GetShowelChanceFromGoodieBox2();
                break;
            case DrawShovelSource.GoodieBox3:
                shovelDrawChance = ShovelChanceFromGoodieBox3; //BundleManager.GetShowelChanceFromGoodieBox3();
                break;
            case DrawShovelSource.VIPBox:
                shovelDrawChance = ShovelChanceFromVIP; //BundleManager.GetShowelChanceFromVIP();
                break;
            case DrawShovelSource.EpidemyBox:
                shovelDrawChance = ShovelChanceFromEpidemyBox; //BundleManager.GetShowelChanceFromEpidemyBox();
                break;
            case DrawShovelSource.TreasureChest:
                shovelDrawChance = ShovelChanceFromTreasureChest; //BundleManager.GetShowelChanceFromTreasureChest();
                break;
            case DrawShovelSource.DailyQuest:
                shovelDrawChance = 1f;
                break;
        }

        if (UnityEngine.Random.value <= shovelDrawChance)
            return new MedicineRef(MedicineType.Special, 3);
        return null;
    }

    public MedicineRef GetRandomSpecial(SpecialItemTarget target)
    {
        MedicineRef screwdriver = new MedicineRef(MedicineType.Special, 0);
        MedicineRef hammer = new MedicineRef(MedicineType.Special, 1);
        MedicineRef spanner = new MedicineRef(MedicineType.Special, 2);
        MedicineRef gum = new MedicineRef(MedicineType.Special, 4);
        MedicineRef metal = new MedicineRef(MedicineType.Special, 5);
        MedicineRef pipe = new MedicineRef(MedicineType.Special, 6);

        List<KeyValuePair<MedicineRef, int>> specialInventoryAmounts = new List<KeyValuePair<MedicineRef, int>>();

        if (target == SpecialItemTarget.Storage || target == SpecialItemTarget.GlobalEvent || target == SpecialItemTarget.All)
        {
            specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(screwdriver, GetCureCount(screwdriver)));
            specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(hammer, GetCureCount(hammer)));
            specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(spanner, GetCureCount(spanner)));
        }
        if (target == SpecialItemTarget.Tank || target == SpecialItemTarget.GlobalEvent || target == SpecialItemTarget.All)
        {
            specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(gum, GetCureCount(gum)));
            specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(metal, GetCureCount(metal)));
            specialInventoryAmounts.Add(new KeyValuePair<MedicineRef, int>(pipe, GetCureCount(pipe)));
        }
        specialInventoryAmounts = specialInventoryAmounts.OrderBy(i => i.Value).ToList();

        //CASE 1: 
        //if elixir storage has never been upgraded, we make sure that first 3 special items will be the ones needed for upgrade.
        if (target != SpecialItemTarget.GlobalEvent)
        {
            if (((ElixirStore.actualLevel == 1 && target == SpecialItemTarget.Storage)
            || (ElixirTank.actualLevel == 1 && target == SpecialItemTarget.Tank))) //TutorialSystem.TutorialController.ShowTutorials &&
            {
                if (specialInventoryAmounts[0].Value == 0)
                    return specialInventoryAmounts[0].Key;
                else if (specialInventoryAmounts[1].Value == 0)
                    return specialInventoryAmounts[1].Key;
                else if (specialInventoryAmounts[2].Value == 0)
                    return specialInventoryAmounts[2].Key;
                else
                    return specialInventoryAmounts[UnityEngine.Random.Range(0, 3)].Key;
            }

            //CASE 2:
            //if player is above level on which plantation unlocks(15?) there's a 20% chance to get a SHOVEL.
            MedicineRef med = TryToDrawShovel();
            if (med != null)
            {
                return med;
            }
        }
        else
        {
            if (Game.Instance.gameState().GetHospitalLevel() > 15)
                return new MedicineRef(MedicineType.Special, 3);
        }

        //CASE 3:
        //if item amounts are equal, get a random one (no chance favor for first on the list)
        if (specialInventoryAmounts[0].Value == specialInventoryAmounts[1].Value
            && specialInventoryAmounts[0].Value == specialInventoryAmounts[2].Value)
            return specialInventoryAmounts[UnityEngine.Random.Range(0, 3)].Key;

        //CASE 4:
        //if previous cases are not met, randomize special item with the following chances: 36%, 34%, 30%. The more item you have, the more chance you will get more of it.
        float rand = RandomFloat(0, 1);
        if (rand < .36f)    //36%    
            return specialInventoryAmounts[2].Key;
        else if (rand < .7f)    //34%    
            return specialInventoryAmounts[1].Key;
        else                     //30% chance to get the item player has the LEAST of
            return specialInventoryAmounts[0].Key;
    }

    public void AddToObjectStored(ShopRoomInfo info, int amount = 1)
    {
        var oData = BaseGameState.StoredObjects;
        if (oData.ContainsKey(info.Tag))
        {
            oData[info.Tag] = oData[info.Tag] + amount;
        }
        else
            oData.Add(info.Tag, amount);

        if (info.unlockLVL > hospitalLevel)
        {
            //this is to unlock decorations given through IAP or other sources where you can get item of higher level than player
            //UIController.get.drawer.UpdateAllItems();
            Debug.LogWarningFormat("<color=red>Trying to get item of higher level than player: {0} lvl {1} </color>", info.Tag, info.unlockLVL);
        }
        UIController.get.drawer.UpdatePrices(); // deco and cans
    }

    public int GetResourceAmount(ResourceType resourceType)
    {
        int amount = 0;
        switch (resourceType)
        {
            case ResourceType.Coin:
                amount = Coins;
                break;
            case ResourceType.Diamonds:
                amount = Diamonds;
                break;
            case ResourceType.Exp:
                amount = ExperienceAmount;
                break;
            case ResourceType.PositiveEnergy:
                amount = PositiveEnergyAmount;
                break;
            default:
                break;
        }
        return amount;
    }

    public void RemoveResources(ResourceType resourceType, int amount, EconomySource source, string buildingTag = null)
    {
        switch (resourceType)
        {
            case ResourceType.Coin:
                RemoveCoins(amount, source, true, buildingTag);
                break;
            case ResourceType.Diamonds:
                RemoveDiamonds(amount, source, buildingTag);
                break;
            case ResourceType.Exp:
                Debug.LogError("Can't remove Exp");
                break;
            case ResourceType.PositiveEnergy:
                RemovePositiveEnergy(amount, source, buildingTag);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Localization

    public void ChangeLang(string lang)
    {
        LocalizationLang = (LocalizationType)Enum.Parse(typeof(LocalizationType), lang);
        //Debug.Log("Lang swithed to: " + lang.ToString());
    }

    #endregion

    public abstract void LevelUp();

    public void LogAchievedLevelEvent(string level)
    {
        var parameters = new Dictionary<string, object>();
        parameters[AppEventParameterName.Level] = level;
        FB.LogAppEvent(
            AppEventName.AchievedLevel,
            null,
            parameters
        );
    }

    public virtual void GiveLevelUpGifts()
    {
        if (hospitalLevel > 2)
            PanaceaCollector.SetPanaceaFull(true);

        LevelUpGifts.LevelUpGift giftsThisLevel = null;
        int giftNumber = 0;
        if (Game.Instance.gameState().GetHospitalLevel() < 50)
        {
            giftsThisLevel = ResourcesHolder.Get().levelUpGifts.Gifts[Mathf.Clamp(Game.Instance.gameState().GetHospitalLevel(), 0, 49)];

            if (giftsThisLevel.resources != null && giftsThisLevel.resources.Length > 0)
            {
                foreach (var resourceGift in giftsThisLevel.resources)
                {
                    if (resourceGift.amount > 0)
                    {
                        ResourceType type = resourceGift.type;
                        int amount = resourceGift.amount;
                        GiftType giftType = (GiftType)type;
                        if (type == ResourceType.PositiveEnergy)
                            giftType = GiftType.PositiveEnergy;

                        int currentAmount = Game.Instance.gameState().GetCurrencyAmount(type);
                        AddResource(type, amount, EconomySource.LevelUpGift, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(giftType, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), amount, .5f + (giftNumber * .5f), 1.75f, new Vector3(1f, 1f, 1), new Vector3(0.75f, 0.75f, 1), null, null, () =>
                        {
                            UpdateCounter(type, amount, currentAmount);
                        });
                        giftNumber++;
                    }
                }
            }

            if (giftsThisLevel.medicines != null && giftsThisLevel.medicines.Length > 0)
            {
                foreach (var medicineGift in giftsThisLevel.medicines)
                {
                    if (medicineGift.amount > 0)
                    {
                        int amount = medicineGift.amount;
                        var medDE = ResourcesHolder.Get().GetMedicineInfos(medicineGift.medRef);


                        if (medDE != null)
                        {
                            bool isTank = medicineGift.medRef.IsMedicineForTankElixir();

                            UIController.get.storageCounter.AddLater(amount, isTank, true);
                            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), medicineGift.amount, .5f + (giftNumber * .5f), 1.75f, Vector3.one, new Vector3(0.75f, 0.75f, 1), medDE.image, null, () =>
                            {
                                UIController.get.storageCounter.Remove(amount, isTank, true);
                            });

                            AddResource(medicineGift.medRef, medicineGift.amount, true, EconomySource.LevelUpGift);

                            giftNumber++;
                        }
                    }
                }
            }
        }
        BaseGiftableResource gift = LevelUpGiftsConfig.GetLevelUpGift(Game.Instance.gameState().GetHospitalLevel());

        if (gift != null)
        {
            gift.Collect(false);
        }
    }

    protected IEnumerator ShowLevelPopup(List<MedicineRef> unlockedMedicines, List<Rotations> unlockedMachines, List<Rotations> additionalMachines)
    {
        if (UIController.get.LevelUpPopUp.isActiveAndEnabled)
        {
            UIController.get.LevelUpPopUp.ButtonContinue();
        }
        UIController.get.reportPopup.canBeOpen = false;

        if (UIController.getMaternity != null)
        {
            while (UIController.getMaternity.boxOpeningPopupUI.gameObject.activeSelf || UIController.getMaternity.boxOpeningPopupUI.IsBoxOpeningProcess())
                yield return null;
            while (TutorialController.Instance.tutorialEnabled && (
                TutorialUIController.Instance.IsFullscreenActive() ||
                TutorialController.Instance.GetCurrentTutorialStep().StepTag == StepTag.maternity_waiting_for_labor_info_b) ||
                (TutorialController.Instance.GetCurrentTutorialStep().StepTag == StepTag.maternity_waiting_for_labor_info_a && TutorialController.Instance.ConditionFulified))
                yield return null;
        }

        UIController.get.ExitAllPopUps();
        UIController.get.CloseActiveHover();
        if (UIController.getHospital != null)
        {
            UIController.getHospital.ObjectivesPanelUI.SlideOut();
        }

        if (ReferenceHolder.Get().objectiveController.ObjectivesSet && !ReferenceHolder.Get().objectiveController.IsDynamicObjective())
        {
            if (UIController.getHospital != null)
            {
                UIController.getHospital.ObjectivesPanelUI.ClaimUnclaimedRewards();

                while (UIController.getHospital.ObjectivesPanelUI.isSlidIn)
                    yield return null;
            }
        }

        while (TutorialController.Instance.tutorialEnabled && TutorialUIController.Instance.IsFullscreenActive())
            yield return null;
        if (UIController.getHospital != null)
        {
            while (UIController.getHospital.PatientCard.vipGiftPending || UnboxingPoUpController.unboxingPending || UIController.getHospital.unboxingPopUp.isActiveAndEnabled)   //when VIP got cured and player levels up we delay popup for after unboxing
                yield return null;
        }

        yield return null;

        UIController.get.LevelUpPopUp.Open(unlockedMedicines, unlockedMachines, additionalMachines);
        NotificationCenter.Instance.LevelUp.Invoke(new LevelUpEventArgs(hospitalLevel));
        ShowPopupCoroutine = null;

        if (!ReferenceHolder.Get().objectiveController.ObjectivesSet || !ReferenceHolder.Get().objectiveController.IsDynamicObjective())
            ReferenceHolder.Get().objectiveController.UpdateObjectives();
    }

    #region SaveLoad

    public void RecountElixirs()
    {
        // reset
        elixirStorageAmount = 0;
        elixirTankAmount = 0;

        // iterate via resources and increment amount again
        foreach (var p in EnumerateResourcesMedRef())
        {
            if (ResourcesHolder.Get().GetIsTankStorageCure(p.Key) == true)
                elixirTankAmount = elixirTankAmount + p.Value;
            else elixirStorageAmount = elixirStorageAmount + p.Value;
        }
    }

    public abstract void LoadDrawerData(Save save);

    public abstract void LoadData(Save save);

    protected void OnLoaded()
    {
        Loaded?.Invoke();
    }

    public List<string> CastDictionary(Dictionary<string, string> input)
    {
        return input.Select(x => x.Key + "#" + x.Value).ToList();
    }

    public Dictionary<string, string> CastList(List<string> list)
    {
        Dictionary<string, string> dictioanry = new Dictionary<string, string>();
        for (int i = 0; i < list.Count; ++i)
        {
            string[] pair = list[i].Split('#');
            if (pair.Length == 2)
            {
                dictioanry.Add(pair[0], pair[1]);
            }
        }
        return dictioanry;
    }

    public abstract void SaveData(Save save, bool isVisiting = false);

    protected string SaveStoredItems()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < StoredObjects.Count; i++)
        {
            string Key = StoredObjects.Keys.ElementAt(i);
            builder.Append(Key);
            builder.Append('/');
            builder.Append(Checkers.CheckedAmount(StoredObjects[Key], 0, int.MaxValue, "Stored objects " + Key + "amount: ").ToString());
            if (i < StoredObjects.Count - 1)
            {
                builder.Append(';');
            }
        }
        return builder.ToString();
    }

    protected void LoadStoredItems(string saveData)
    {
        if (string.IsNullOrEmpty(saveData))
        {
            return;
        }
        var items = saveData.Split(';');
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i].Split('/');
            //var oData = GameState.BuildedObjects;
            if (StoredObjects.ContainsKey(item[0]))
            {
                StoredObjects[item[0]] = int.Parse(item[1], System.Globalization.CultureInfo.InvariantCulture);
            }
            else
                StoredObjects.Add(item[0], int.Parse(item[1], System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    protected string SaveMedicinePermutationsList()
    {
        if (MedicinePermutationsList != null && MedicinePermutationsList != null)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(MedicinePermutationsCounter.ToString());
            builder.Append("!");

            for (int i = 0; i < MedicinePermutationsList.Count; i++)
            {
                builder.Append(MedicinePermutationsList[i].ToString());
                if (i < MedicinePermutationsList.Count - 1)
                    builder.Append("?");
            }

            return builder.ToString();
        }
        else
        {
            return "";
        }
    }

    protected void LoadMedicinePermutationsList(string rndStr)
    {
        if (MedicinePermutationsList != null)
            MedicinePermutationsList.Clear();

        if (!string.IsNullOrEmpty(rndStr))
        {
            var rnds = rndStr.Split('!');

            MedicinePermutationsCounter = int.Parse(rnds[0], System.Globalization.CultureInfo.InvariantCulture);

            var perms = rnds[1].Split('?');

            if (perms != null)
            {
                for (int i = 0; i < perms.Length; ++i)
                    MedicinePermutationsList.Add(MedicinePermutations.Parse(perms[i]));
            }
        }
    }

    protected string SaveLastMedicineRndPool()
    {
        if (LastMedicineRndPool != null && LastMedicineRndPool != null)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < LastMedicineRndPool.Count; i++)
            {
                builder.Append(LastMedicineRndPool[i].ToString());
                if (i < LastMedicineRndPool.Count - 1)
                    builder.Append(";");
            }

            return builder.ToString();
        }
        else
        {
            return "";
        }
    }

    protected void LoadLastMedicineRndPool(string rndStr)
    {
        if (LastMedicineRndPool != null)
            LastMedicineRndPool.Clear();

        if (!string.IsNullOrEmpty(rndStr))
        {
            var rnds = rndStr.Split(';');

            if (rnds != null)
            {
                for (int i = 0; i < rnds.Length; ++i)
                    LastMedicineRndPool.Add(MedicineRef.Parse(rnds[i]));
            }
        }
    }

    protected string SaveBadgedMachines()
    {
        if (BadgedMachines != null)
        {
            return RotationsListToString(BadgedMachines);
        }
        else
        {
            return "";
        }
    }

    protected virtual void LoadBadgedMachines(string rotationsStr)
    {
        BadgedMachines = RotationsStrToRotations(rotationsStr);
        if (BadgedMachines != null)
        {
            UIController.get.drawer.AddBadgeForItems(BadgedMachines);
            UIController.getHospital.drawer.AddBadgeForItems(BadgedMachines);

            int unlockedHospitalMachines = 0;
            int unlockedLaboratoryMachines = 0;
            int unlockedPatioMachines = 0;

            for (int i = 0; i < BadgedMachines.Count; i++)
            {
                BaseRoomInfo infos = BadgedMachines[i].infos;
                if (infos.dummyType == BuildDummyType.Decoration && ((ShopRoomInfo)(infos)).unlockLVL < 8)
                {
                    continue;
                }
                if (BadgedMachines[i].infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                    unlockedHospitalMachines++;
                else if (BadgedMachines[i].infos.DrawerArea == HospitalAreaInDrawer.Laboratory)
                    unlockedLaboratoryMachines++;
                else if (BadgedMachines[i].infos.DrawerArea == HospitalAreaInDrawer.Patio)
                    unlockedPatioMachines++;
            }

            UIController.get.drawer.HideAllBadges();
            UIController.get.drawer.AddBadgeForItems(BadgedMachines);
            UIController.getHospital.drawer.AddBadgeForItems(BadgedMachines);
            UIController.get.drawer.AddTabButtonBadges(unlockedHospitalMachines, unlockedLaboratoryMachines, unlockedPatioMachines);
            UIController.get.drawer.ShowMainButtonBadge(unlockedHospitalMachines + unlockedLaboratoryMachines + unlockedPatioMachines);
        }
    }

    protected string SaveCommunityRewards()
    {
        string rewards = "";
        for (int i = 0; i < communityRewards.Length; ++i)
        {
            rewards += communityRewards[i].ToString();
            if (i < communityRewards.Length - 1)
            {
                rewards += "?";
            }
        }
        return rewards;
    }

    protected void LoadCommunityRewards(string rewards)
    {
        if (string.IsNullOrEmpty(rewards))
        {
            return;
        }
        var rewardsSplit = rewards.Split('?');
        for (int i = 0; i < rewardsSplit.Length; ++i)
        {
            communityRewards[i] = bool.Parse(rewardsSplit[i]);
        }
    }

    #endregion

    #region Community Rewards
    public void SetCommunityRewardState(int ID, bool state)
    {
        communityRewards[ID] = state;
    }

    public bool CheckCommunityRewardState(int ID)
    {
        return communityRewards[ID];
    }
    #endregion

    #region Badges Management
    private string RotationsListToString(List<Rotations> rotations)
    {
        string rotationsStr = "";
        for (int i = 0; i < rotations.Count; ++i)
        {
            rotationsStr += HospitalAreasMapController.Map.rotationsIDs.FindIndex(a => a.infos == rotations[i].infos).ToString();
            if (i < rotations.Count - 1)
            {
                rotationsStr += "?";
            }
        }

        return rotationsStr;
    }

    protected List<Rotations> RotationsStrToRotations(string rotationsStr)
    {
        List<Rotations> rotations = new List<Rotations>();
        if (!string.IsNullOrEmpty(rotationsStr))
        {
            var rotationsSplit = rotationsStr.Split('?');
            for (int i = 0; i < rotationsSplit.Length; ++i)
            {
                try
                {
                    rotations.Add(HospitalAreasMapController.Map.rotationsIDs[int.Parse(rotationsSplit[i], System.Globalization.CultureInfo.InvariantCulture)]);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            return rotations;
        }
        return null;
    }

    public void RemoveBadge(Rotations rotations)
    {
        if (BadgedMachines != null)
        {
            BadgedMachines.Remove(rotations);
        }
    }
    #endregion

    public class hospitalBoosters
    {
        public float coinsPatientCard;
        public float coinsDoctorPatient;
        public float xpPatientCard;
        public float xpDoctorPatient;

        public hospitalBoosters(float coinsPC = 1f, float coinsDP = 1f, float xpPC = 1f, float xpDP = 1f)
        {
            coinsPatientCard = coinsPC;
            coinsDoctorPatient = coinsDP; ;
            xpPatientCard = xpPC;
            xpDoctorPatient = xpDP;
        }
    }

    public class MaternityBoosters
    {
        public float coinsMaternity;
        public float expMaternity;

        public MaternityBoosters(float coins = 1f, float expmaternity = 1f)
        {
            coinsMaternity = coins;
            expMaternity = expmaternity;
        }
    }

    #region AccessReferences

    public PanaceaCollector PanaceaCollector { get; set; }

    public ElixirStorage ElixirStore { get; set; }

    public ElixirTank ElixirTank { get; set; }

    public int ElixirStoreToUpgrade = 0;
    public int ElixirTankToUpgrade = 0;

    #endregion

    private void EnableGiftsFeature()
    {
        UIController.get.FriendsDrawer.SetAvailableGiftsAmountActive(true);

    }

    protected int GetGiftsFeatureMinLevel()
    {
        return GiftsAPI.Instance.GiftsFeatureMinLevel;
    }

    public MedicineProductionMachine GetMedicineProductionMachine(MedicineDatabaseEntry med)
    {
        IProductables productable = HospitalDataHolder.Instance.BuiltProductionMachines.Find(a => string.Compare(a.GetTag(), med.producedIn.Tag) == 0);
        if (productable is MedicineProductionMachine)
        {
            return productable as MedicineProductionMachine;
        }
        return null;
    }

    public List<MedicineProductionMachine> GetAllMedicineProductionMachines(string machineTag)
    {
        return HospitalDataHolder.Instance.BuiltProductionMachines.FindAll(a => string.Compare(a.GetTag(), machineTag) == 0 && a is MedicineProductionMachine).ToList().Cast<MedicineProductionMachine>().ToList();
    }

    public void SetCoinAmount(int amount)
    {
        Coins = amount;
        UIController.get.coinCounter.SetValue(amount);
    }

    public void SetDiamondAmount(int amount)
    {
        Diamonds = amount;
        UIController.get.diamondCounter.SetValue(amount);
    }
}

#region enums

public enum ResourceType
{
    Coin,
    Diamonds,
    Exp,
    PositiveEnergy,
    Booster,    //used for analytics only!
    Medicine,    //used for analytics only!
    Decoration  //recently used for halloween pumpkin event
}

public enum DiseaseType
{
    Head, Brain, Nose, Lips, Throat, Skin, Hand, Heart, Tummy, Bone, Foot, Ear, Eye, Lungs, Kidneys, None, Empty, Bacteria, VitaminDeficiency,
}

public enum DoctorMachineType
{
    Blue,
    Green,
    Pink,
    Purple,
    Red,
    SkyBlue,
    SunnyYellow,
    White,
    Yellow
}

public enum MedicineType
{
    BaseElixir,
    Syrop,
    NoseDrops,
    EyeDrops,
    Capsule,
    Pill,
    FizzyTab,
    InhaleMist,
    Shot,
    Extract,
    Drips,
    Jelly,
    Balm,
    AdvancedElixir,
    BasePlant,
    Special,
    Fake,
    Bacteria,
    Vitamins
}

public enum ExternalRoomType
{
    VIPRoom,
    PlayRoom,
    Epidemy,
    BubbleBoy,
    NurseRoom,
    MaternityWard
}

public enum LocalizationType
{
    en,
    pl
}

public enum BoosterType
{
    Coin,
    Exp,
    Action,
    CoinAndExp
}

public enum BoosterTarget
{
    PatientCard,
    DoctorPatient,
    AllPatients,
    Lab,
    MaternityPatients
}

public enum CasePrizeType
{
    Coins,
    Diamonds,
    SpecialItem,
    Decoration,
    Booster,
    PositiveEnergy
}

public enum ExpansionType
{
    Clinic,
    Lab,
    Other,
    MaternityClinic
}

public enum SpecialItemTarget
{
    None,
    Storage,
    Tank,
    All,
    GlobalEvent
}

#endregion