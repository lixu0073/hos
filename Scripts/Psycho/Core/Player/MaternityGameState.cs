using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IsoEngine;
using System.Text;
using System.Linq;
using Facebook.Unity;
using SimpleUI;

public class MaternityGameState : BaseGameState, IGameState
{
    public MaternityBoosters maternityBoosters = new MaternityBoosters();
    public CMaternityPatientsCount maternityPatientsCount = new CMaternityPatientsCount();
    public int ExpansionMaternityClinic;
    private static MaternityGameState value;
    private int HospitalExperience;
    #region Static variables
    public static int levelToSetRandomRewards = 15;
    public static int coinMultiplierInRandomisedRewards = 10;
    #endregion

    void Awake()
    {
        expForLevel = new int[] { 0, 140, 296, 512, 783, 1284, 1812, 2628, 3356, 4271,
            5589, 6892, 8215,9857,11798};
        if (value != null && value != this)
        {
            Debug.LogWarning("Multiple instances of GameState were found!");
            Destroy(gameObject);
        }
        else
            value = this;
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public static MaternityGameState Get()
    {
        if (value == null)
            throw new IsoException("Fatal Failure of GameState.");
        else return value;
    }

    public string SampleVariable
    {
        get;
        set;
    }

    public bool IsMaternityFirstLoopCompleted
    {
        get;
        set;
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
        base.maternityLevel = save.MaternityLevel;
        base.hospitalLevel = save.Level;
        ExperienceAmount = save.MaternityExperience;
        HospitalExperience = save.Experience;
        HospitalName = save.HospitalName;
        Coins = save.CoinAmount;
        Diamonds = save.DiamondAmount;
        PositiveEnergyAmount = save.PositiveEnergyAmount;
        SampleVariable = save.SampleVariable;
        Version = save.version;
        // ExpansionMaternityClinic = save.ExpansionsMaternityClinic;
        IsMaternityFirstLoopCompleted = save.MaternityIsFirstLoopCompleted;
        UIController.get.drawer.UnlockItems(ResourcesHolder.Get().GetUnlockedMachines());
        if (BuildedObjects == null)
            BuildedObjects = new Dictionary<string, int>();
        else
            BuildedObjects.Clear();
        LoadStoredItems(save.StoredItems);

        UIController.get.coinCounter.SetValue(Coins);
        UIController.get.diamondCounter.SetValue(Diamonds);

        LocalNotificationController.Instance.notificationGroups.LoadFromString(save.NotificationSettings);

        OtherMapBadgesToShow = save.BadgesToShow;
        LoadBadgedMachines(save.BadgesToShowMaternity);
        LoadCommunityRewards(save.CommunityRewards);

        AnalyticsGeneralParameters.maternityUserLevel = maternityLevel;
        AnalyticsGeneralParameters.maternityUserXP = ExperienceAmount;
        AnalyticsGeneralParameters.softCurrency = Coins;
        AnalyticsGeneralParameters.hardCurrency = Diamonds;
        AnalyticsGeneralParameters.positiveEnergy = PositiveEnergyAmount;

        EverOpenedCrossPromotionPopup = save.EverOpenedCrossPromotionPopup;
        CrossPromotionCompleted = save.CrossPromotionCompleted;

        MaternityUIController.get.SetLevelText(maternityLevel);

        OnLoaded();
    }

    public override void RefreshXPBar()
    {
        UIController.get.XPBar.SetValue(ExperienceAmount);
        UIController.get.XPBar.SetMaxValue(GetExpForLevel(maternityLevel));
    }

    //matward zrobic poprawnego sejva
    public override void SaveData(Save save, bool isVisiting = false)
    {
        //save.version = LoadingGame.version;
        //save.gameVersion = Application.version;
        save.Elixirs = new List<string>(resources.Count);

        for (int i = 0; i < resources.Count; i++)
        {
            save.Elixirs.Add(resources.ElementAt(i).Key + "+" + Checkers.CheckedAmount(resources.ElementAt(i).Value, 0, int.MaxValue, "ElixirStorage " + resources.ElementAt(i).Key + " amount: "));
        }
        save.MaternityExperience = ExperienceAmount;
        save.Experience = HospitalExperience;
        save.version = LoadingGame.version;
        save.HospitalName = HospitalName;
        save.CoinAmount = Coins;
        save.DiamondAmount = Diamonds;
        save.PositiveEnergyAmount = PositiveEnergyAmount;
        save.MaternityLevel = maternityLevel;
        save.Level = hospitalLevel;
        save.SampleVariable = SampleVariable;
        // save.ExpansionsMaternityClinic = Checkers.CheckedAmount(ExpansionMaternityClinic, 0, AreaMapController.Map.mapConfig.MaternityWardClinic.areas.Count, "Maternity Clinic expansions");
        save.MaternityIsFirstLoopCompleted = IsMaternityFirstLoopCompleted;
        save.BadgesToShowMaternity = SaveBadgedMachines();
        save.BadgesToShow = OtherMapBadgesToShow;
        save.StoredItems = SaveStoredItems();
        save.NotificationSettings = LocalNotificationController.Instance.notificationGroups.SaveToString();
        save.EverOpenedCrossPromotionPopup = EverOpenedCrossPromotionPopup;
        save.CrossPromotionCompleted = CrossPromotionCompleted;
    }


    protected override void LoadBadgedMachines(string rotationsStr)
    {
        BadgedMachines = RotationsStrToRotations(rotationsStr);
        if (BadgedMachines != null)
        {
            UIController.get.drawer.AddBadgeForItems(BadgedMachines);
            //UIController.getHospital.drawer.AddBadgeForItems(BadgedMachines);//

            int unlockedHospitalMachines = 0;

            for (int i = 0; i < BadgedMachines.Count; i++)
            {
                BaseRoomInfo infos = BadgedMachines[i].infos;
                if (BadgedMachines[i].infos.DrawerArea == HospitalAreaInDrawer.MaternityClinic)
                    unlockedHospitalMachines++;
            }

            UIController.get.drawer.HideAllBadges();
            UIController.get.drawer.AddBadgeForItems(BadgedMachines);
            //UIController.getHospital.drawer.AddBadgeForItems(BadgedMachines);
            UIController.get.drawer.AddTabButtonBadges(unlockedHospitalMachines, 0, 0);
            UIController.get.drawer.ShowMainButtonBadge(unlockedHospitalMachines + 0 + 0);
        }
    }

    public override bool CheckIfLevelUP()
    {
        bool isLevelUP = false;

        if (HospitalAreasMapController.Map.VisitingMode)
        {
            return isLevelUP;
        }

        while (ExperienceAmount >= GetExpForLevel(maternityLevel))
        {

            ExperienceAmount -= GetExpForLevel(maternityLevel);
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

    public override void LoadDrawerData(Save save)
    {
        UIController.get.drawer.UnlockItems(ResourcesHolder.Get().GetUnlockedMachines());
    }

    public void AddHospitalExperience(int amount, EconomySource source, string buildingTag = null)
    {
        if (amount <= 0)
            return;

        HospitalExperience += amount;
        AnalyticsGeneralParameters.userXP = ExperienceAmount;
    }
    /// <summary>
    /// Get the gifts present on this level
    /// </summary>
    public LevelUpGifts.LevelUpGift GetLevelUpGifts()
    {
        LevelUpGifts.LevelUpGift giftsThisLevel = null;
        if (Game.Instance.gameState().GetMaternityLevel() < MaternityGameState.levelToSetRandomRewards)
        {
            giftsThisLevel = ResourcesHolder.Get().levelUpGifts.Gifts[Mathf.Clamp(Game.Instance.gameState().GetMaternityLevel(), 0, 49)]; //The max 49 a bit useless since we have a check for level above.
        }
        else
        {
            giftsThisLevel = new LevelUpGifts.LevelUpGift();
            giftsThisLevel.resources = new LevelUpGifts.GiftedResources[2] { new LevelUpGifts.GiftedResources() { type = ResourceType.Coin, amount = 0 }, new LevelUpGifts.GiftedResources() { type = ResourceType.Diamonds, amount = 0 } };
            if (Game.Instance.gameState().GetMaternityLevel() % 2 == 0)
            {
                //Debug.Log("Maternity level: " + Game.Instance.gameState().GetMaternityLevel() + " Random seed: " + Game.Instance.gameState().GetMaternityLevel());
                UnityEngine.Random.InitState(Game.Instance.gameState().GetMaternityLevel());

                //Debug.Log("Seed set. Random is: " + UnityEngine.Random.Range(BundleManager.GetDiamondAmountPerLevelUpAfter50(), BundleManager.GetDiamondAmountPerLevelUpAfter50() * 2 + 1) + "Random value" + UnityEngine.Random.value);
                giftsThisLevel.resources[1].amount = (int)UnityEngine.Random.Range(DefaultConfigurationProvider.GetConfigCData().DiamondAmountPerLevelUpAfter50, DefaultConfigurationProvider.GetConfigCData().DiamondAmountPerLevelUpAfter50 * 2 + 1); //+1 is to set inclusivity
            }
            else
            {
                giftsThisLevel.resources[0].amount = Mathf.CeilToInt(Game.Instance.gameState().GetMaternityLevel() * DefaultConfigurationProvider.GetConfigCData().GoldFactorForLevelUpRewardAfter50 * coinMultiplierInRandomisedRewards);
            }
        }
        return giftsThisLevel;
    }
    /// <summary>
    /// Here we give level up gifts to the player.
    /// </summary>
    public override void GiveLevelUpGifts()
    {
        LevelUpGifts.LevelUpGift giftsThisLevel = GetLevelUpGifts();

        int giftNumber = 0;
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

                    int currentResourceAmount = Game.Instance.gameState().GetCurrencyAmount(type);
                    AddResource(type, amount, EconomySource.LevelUpGift, false);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(giftType, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), amount, .5f + (giftNumber * .5f), 1.75f, Vector3.one, new Vector3(0.75f, 0.75f, 1), null, null, () =>
                    {
                        UpdateCounter(type, amount, currentResourceAmount);
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
                        //HintsController.Get().RemoveHint(new CollectHint(medicineGift.medRef));

                        AddResource(medicineGift.medRef, medicineGift.amount, true, EconomySource.LevelUpGift);
                        giftNumber++;
                    }
                }
            }
        }

        if (giftsThisLevel.decorations != null && giftsThisLevel.decorations.Length > 0)
        {
            List<DecorationInfo> decorationsList = new List<DecorationInfo>();
            foreach (var decorationGift in giftsThisLevel.decorations)
            {
                if (decorationGift.amount > 0)
                {
                    int amount = decorationGift.amount;
                    var decInfo = decorationGift.medRef;

                    if (decInfo != null)
                    {
                        for (int i = 0; i < amount; i++)
                            decorationsList.Add(decInfo);
                        giftNumber++;
                    }
                }
            }
            MaternityUIController.get.maternityDrawer.GivePlayerSelectedDecorations(decorationsList.ToArray(), true);

            //TODO: add support for maternity refactored Drawer. INFO this would be a huge change //MaternityUIController.get.refactoredDrawer.GivePlayerSelectedDecorations(decorationsList.ToArray());
        }
    }
    //TODO we use the same exp system for both hospitals.
    //We need to make a new exp system for maternity or change the amount of experience per giving birth.
    public override int GetExpForLevel(int level)
    {
        if (level <= 14)
        {
            return expForLevel[level];
        }
        else
        {
            float last = expForLevel[14];
            float previous = expForLevel[13];
            int expDifference = (int)((last - previous) * 1.02f);
            int ExpForLevel = expDifference + (int)last;
            for (int i = 0; i < maternityLevel - 15; i++) //-15 couse first iteration takes place outside of loop
            {
                previous = last;
                last = ExpForLevel;
                expDifference = (int)((last - previous) * 1.02f);
                ExpForLevel = expDifference + (int)last;
            }
            //Debug.LogError("Exp for level: " + (expForLevel[50] + ((level - 50) * 11000)).ToString());
            return ExpForLevel;
        }
    }

    public bool IsStarterPackUsed()
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
        ReferenceHolder.Get().giftSystem.SetUIElementsPos(); //This is to set coin and diamond positions and lock them so that UI does not move them while hiding UI.
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
        //matward TODO
        //NotificationCenter.Instance.LevelUp.Invoke(new LevelUpEventArgs(actualLevel + 1));
        //ObjectiveNotificationCenter.Instance.LevelUpObjectiveUpdate.Invoke(new ObjectiveEventArgs(1));

        base.maternityLevel++;
        UIController.get.ExitAllPopUps();
        UIController.get.SetLevelText(base.maternityLevel);
        //matward TODO
        //List<MedicineRef> unlockedMedicines = ResourcesHolder.Get().medicines.cures.SelectMany((x) => { return x.medicines.Where((y) => { return (y.minimumLevel == actualLevel && x.type != MedicineType.Fake); }); }).Select<MedicineDatabaseEntry, MedicineRef>((z) => { return z.GetMedicineRef(); }).ToList();

        List<MedicineRef> unlockedMedicines = new List<MedicineRef>();
        List<Rotations> unlockedMachines = ResourcesHolder.Get().GetMachinesForLevel(base.maternityLevel);
        BadgedMachines = unlockedMachines;
        List<Rotations> additionalMachines = ResourcesHolder.Get().GetAdditionalMachines(base.maternityLevel);


        for (int i = 0; i < additionalMachines.Count; i++)
        {
            if (unlockedMachines.Contains(additionalMachines[i]))
            {
                Debug.Log("Removing machine from additional machines: " + additionalMachines[i].infos.name);
                additionalMachines.RemoveAt(i);
            }
        }

        UIController.get.drawer.UnlockItems(unlockedMachines);
        ShowPopupCoroutine = StartCoroutine(ShowLevelPopup(unlockedMedicines, unlockedMachines, additionalMachines));

        NotificationCenter.Instance.DrawerUpdate.Invoke(new DrawerUpdateEventArgs());

        AnalyticsGeneralParameters.maternityUserLevel = maternityLevel;
        AnalyticsGeneralParameters.maternityUserXP = ExperienceAmount;

        //matward TODO
        AnalyticsController.instance.ReportLevelUp(false);
        //AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Level.ToString(), actualLevel, actualLevel.ToString());
        // TODO
        //TenjinController.instance.ReportLevelUp(actualLevel);

        try
        {
            if (FB.IsInitialized)
            {
                LogAchievedLevelEvent(base.maternityLevel.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        //matward TODO
        SendOnLevelUpEvent();
        RefreshXPBar();
        //SaveDynamoConnector.Instance.InstantSave();
    }

    public int GetExperienceAmount()
    {
        return ExperienceAmount;
    }

    public int GetExpansionClinicAmount()
    {
        return AreaMapController.Map.CountBoughtAreas(HospitalArea.MaternityWardClinic);
    }

    public void SetExpansionClinicAmount(int value)
    {
        /// ExpansionMaternityClinic += value;
    }

    public int GetCoinAmount()
    {
        return Coins;
    }

    public int GetPositiveEnergyAmount()
    {
        return PositiveEnergyAmount;
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
                Debug.LogError("returining zero");
                return 0;
        }
    }


    public int GetDiamondAmount()
    {
        return Diamonds;
    }

    public float GetGameplayTimer()
    {
        return GameplayTimer;
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

    public MasterableProperties GetMasterableProductionMachine(MedicineDatabaseEntry cure)
    {
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

    public class CMaternityPatientsCount
    {
        public CMaternityPatientsCount(int patientsCount = 0, int patientsCuredCount = 0, int patientsCuredCountBed = 0)
        {
            this.patientsCount = patientsCount;
            this.patientsCuredCount = patientsCuredCount;
            this.patientsCuredCountBed = patientsCuredCountBed;
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

        //add
        public void AddPatientsCuredBed(int count = 1)
        {
            PatientsCuredCountBed += count;
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

        //save/load
        public string Save()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Checkers.CheckedAmount(PatientsCuredCountBed, 0, int.MaxValue, "PatientsCuredCountBed"));
            return builder.ToString();
        }

        public void Load(string save)
        {
            SetPatients(0);
            SetPatientsCured(0);
            patientsCuredCountBed = 0;
            if (!string.IsNullOrEmpty(save))
            {
                var saveData = save.Split(';');
                SetPatientsCuredBed(int.Parse(saveData[0], System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {

            }
        }

        private int GetDiagnosedPatients()
        {
            return HospitalDataHolder.Instance.GetDiagnosedPatients();
        }
    }

}
