using Hospital;
using MovementEffects;
using SimpleUI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MaternityWard : SuperObjectWithVisiting
{
    #region Indicators
#pragma warning disable 0649
    [SerializeField] private Animator MaternityNotificationIndicators;
#pragma warning restore 0649
    #endregion

    #region MapObject
#pragma warning disable 0649
    [SerializeField] private GameObject maternityWardLocked;
    [SerializeField] private GameObject maternityWardRenovating;
    [SerializeField] private GameObject maternityWardGift;
    [SerializeField] private GameObject maternityWardWorking;
    [SerializeField] private Transform particleTransform;
    [SerializeField] private GameObject objectBorder;
#pragma warning restore 0649
    #endregion

    [SerializeField] public ExternalRoomInfo Info;
    [SerializeField] private Transform hoverPoint = null;
    public GameObject ObjectBorder { get { return objectBorder; } }
    private ExternalRoomState maternityWardObjectState = ExternalRoomState.Disabled;
    private MaternityWardBuildingHover hover;
    //private ProgressBarController progressBar = null;
    delegate void DClick();
    DClick dOnClick;
    IEnumerator<float> buildingCoroutine;
    public Dictionary<MedicineDatabaseEntry, int> RequiredItems = new Dictionary<MedicineDatabaseEntry, int>();

    private float timeSinceRenovationStarted = 0;
    public float TimeSinceRenovationStarted { get { return timeSinceRenovationStarted; } }

    #region Mono
    private void Start()
    {
        SetUpIndicator();
    }

    private void OnDestroy()
    {
        IsoDestroy();
    }
    #endregion

    #region Public Methods
    public bool IsRenovating()
    {
        return maternityWardObjectState == ExternalRoomState.Renovating;
    }

    public bool IsEnabled()
    {
        return maternityWardObjectState == ExternalRoomState.Enabled;
    }

    public ExternalRoomState State
    {
        get { return maternityWardObjectState; }
        private set { }
    }

    public void EmulateTime(TimePassedObject timePassed)
    {
        if (maternityWardObjectState == ExternalRoomState.Renovating)
        {
            Debug.LogError("Emulating renoveting maternity by " + timePassed.GetTimePassed());
            timeSinceRenovationStarted += timePassed.GetTimePassed();
        }
    }
    #endregion

    #region Private Methods
    private void ChangeState(ExternalRoomState newState)
    {
        maternityWardObjectState = newState;
        Init();
        UIController.getHospital.GoToMaternityButton.UpdateUIButton(IsEnabled());
    }

    protected virtual void Init()
    {
        maternityWardLocked.SetActive(false);
        maternityWardRenovating.SetActive(false);
        maternityWardGift.SetActive(false);
        maternityWardWorking.SetActive(false);

        switch (maternityWardObjectState)
        {
            case ExternalRoomState.Disabled:
                {
                    dOnClick = OnClickDisabled;
                    maternityWardLocked.SetActive(true);
                    if (Info.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel())
                    {
                        ChangeState(ExternalRoomState.WaitingForRenovation);
                    }
                }
                break;
            case ExternalRoomState.WaitingForRenovation:
                {
                    TryToRandomizeRequiredItems();
                    maternityWardLocked.SetActive(true);
                    dOnClick = OnClickWaitingForRenew;
                }
                break;
            case ExternalRoomState.Renovating:
                {
                    maternityWardRenovating.SetActive(true);
                    dOnClick = OnClickRenovating;
                    if (buildingCoroutine != null)
                    {
                        Timing.KillCoroutine(buildingCoroutine);
                        buildingCoroutine = null;
                    }
                    buildingCoroutine = Timing.RunCoroutine(Counting(ExternalRoomState.WaitingForUser));
                }
                break;
            case ExternalRoomState.WaitingForUser:
                {
                    if (hover != null)
                        hover.Close();

                    maternityWardGift.SetActive(true);
                    dOnClick = OnClickWaitingForUser;
                }
                break;
            case ExternalRoomState.Enabled:
                {
                    maternityWardWorking.SetActive(true);
                    dOnClick = OnClickEnabled;
                    onInitEnabled();
                }
                break;
        }

    }

    public virtual void OnClickDisabled()
    {
        if (visitingMode)
        {
            return;
        }
        if (Game.Instance.gameState().GetHospitalLevel() < Info.UnlockLvl)
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.MaternityWard, false, false);
        }
    }

    public virtual void OnClickWaitingForRenew()
    {
        if (visitingMode)
        {
            return;
        }
        if (DefaultConfigurationProvider.GetConfigCData().IsMaternityWardFeatureEnabled())
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.MaternityWard, true, false, () => ConfirmRenew());
            NotificationCenter.Instance.MaternityWardObjectClikedNotif.Invoke(new BaseNotificationEventArgs());
        }
        else
        {
            MessageController.instance.ShowMessage(69);
        }
    }

    public virtual void OnClickRenovating()
    {
        if (visitingMode)
        {
            return;
        }
        hover = MaternityWardBuildingHover.Open(this);
        hover.UpdateHover();
        hover.SetWorldPointHovering(hoverPoint.position);
        SoundsController.Instance.PlayConstruction();
    }

    public virtual void OnClickWaitingForUser()
    {
        if (visitingMode)
        {
            return;
        }
        AnalyticsController.instance.ReportRenovate(ExternalRoom.ActionType.unwrap, GetBuildingTag());
        Animator maternityWardGiftAnimator = maternityWardGift.GetComponent<Animator>();
        if (maternityWardGiftAnimator != null)
            maternityWardGiftAnimator.SetTrigger("Unwrap");
        Timing.RunCoroutine(DelayedFX());

        int expRecieved = Info.ExpRecived;
        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
        GameState.Get().AddResource(ResourceType.Exp, expRecieved, EconomySource.BuildingBuilt, false, GetBuildingTag());
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expRecieved, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
        {
            GameState.Get().UpdateCounter(ResourceType.Exp, expRecieved, currentExpAmount);
        });
        ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, Info.roomName, ObjectiveRotatableEventArgs.EventType.Unwrap));
        dOnClick -= OnClickWaitingForUser;
        Timing.CallDelayed(0.667f, () =>
        {
            maternityWardGift.SetActive(false);
            maternityWardWorking.gameObject.SetActive(true);
            ChangeState(ExternalRoomState.Enabled);
            NotificationCenter.Instance.MaternityWardObjectClikedNotif.Invoke(new BaseNotificationEventArgs());
        });
    }

    private IEnumerator<float> DelayedFX()
    {
        yield return Timing.WaitForSeconds(0.5f);

        Instantiate(ResourcesHolder.GetHospital().ParticleUnpackVIP, particleTransform.position, Quaternion.identity);
        SoundsController.Instance.PlayCheering();
    }

    public virtual void OnClickEnabled()
    {
        if (DefaultConfigurationProvider.GetConfigCData().IsMaternityWardFeatureEnabled())
        {
            NotificationCenter.Instance.MaternityWardObjectClikedNotif.Invoke(new BaseNotificationEventArgs());
            VisitingController.Instance.RedirectToMaternityMap(sourceFromButton);
        }
        else
        {
            MessageController.instance.ShowMessage(69);
        }
    }
    #endregion

    #region OnClick
    private bool sourceFromButton = false;

    public void OnClickFromButton()
    {
        sourceFromButton = true;
        if (UIController.get.drawer.IsVisible)
        {
            UIController.get.drawer.SetVisible(false);
            return;
        }
        if (UIController.get.FriendsDrawer.IsVisible)
        {
            UIController.get.FriendsDrawer.SetVisible(false);
            return;
        }
        dOnClick();
    }

    public override void OnClick()
    {
        sourceFromButton = false;
        if (UIController.get.drawer.IsVisible)
        {
            UIController.get.drawer.SetVisible(false);
            return;
        }
        if (UIController.get.FriendsDrawer.IsVisible)
        {
            UIController.get.FriendsDrawer.SetVisible(false);
            return;
        }
        dOnClick();
    }

    public void OnClickSpeedUp(IDiamondTransactionMaker diamondTransactionMaker)
    {
        BuyWithDiamonds(diamondTransactionMaker);
    }

    protected virtual void BuyWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker)
    {
        int cost = DiamondCostCalculator.GetCostForBuilding(Info.RenovatingTimeSeconds - TimeSinceRenovationStarted, Info.RenovatingTimeSeconds);
        if (Game.Instance.gameState().GetDiamondAmount() >= cost)
        {
            DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
            {
                GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpBuilding);
                timeSinceRenovationStarted = Info.RenovatingTimeSeconds;

                ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                Instantiate(ResourcesHolder.Get().ParticleDiamondBuilding, particleTransform.position, Quaternion.identity);
                NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                if (MaternityWardBuildingHover.activeHover != null)
                    MaternityWardBuildingHover.activeHover.Close();
                ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ResetOntouchAction();
                //progressBar = null;
            }, diamondTransactionMaker);
        }
        else
        {
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
            UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
        }
    }

    public void OnLvlUp()
    {
        if (Info.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel() && maternityWardObjectState == ExternalRoomState.Disabled)
        {
            ChangeState(ExternalRoomState.WaitingForRenovation);
        }
    }

    protected void ConfirmRenew()
    {
        bool hasRequiredCoins = Game.Instance.gameState().GetCoinAmount() >= Info.RenovationCost;
        List<KeyValuePair<int, MedicineDatabaseEntry>> missingItems = new List<KeyValuePair<int, MedicineDatabaseEntry>>();

        foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in RequiredItems)
        {
            MedicineRef item = pair.Key.GetMedicineRef();
            int currentCureCount = GameState.Get().GetCureCount(item);
            if (pair.Value > currentCureCount)
            {
                missingItems.Add(new KeyValuePair<int, MedicineDatabaseEntry>(pair.Value - currentCureCount, pair.Key));
            }
        }

        bool hasRequiredItems = missingItems.Count == 0;
        if (hasRequiredCoins && hasRequiredItems)
        {
            GameState.Get().RemoveCoins(Info.RenovationCost, EconomySource.RenewBuilding);
            SoundsController.Instance.PlayConstruction();
            UIController.getHospital.LockedFeatureArtPopUpController.Exit();

            foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in RequiredItems)
            {
                GameState.Get().GetCure(pair.Key.GetMedicineRef(), pair.Value, EconomySource.RenewBuilding);
            }

            ReferenceHolder.Get().giftSystem.CreateItemUsed(ReferenceHolder.Get().engine.MainCamera.LookingAt, Info.RenovationCost, .1f, ReferenceHolder.Get().giftSystem.particleSprites[0]);
            timeSinceRenovationStarted = 0;
            ChangeState(ExternalRoomState.Renovating);
            hover = MaternityWardBuildingHover.Open(this);
            hover.SetWorldPointHovering(hoverPoint.position);

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.RenovationStarted);

            ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, Info.roomName));
            NotificationCenter.Instance.MaternityWardRenovateNotif.Invoke(new BaseNotificationEventArgs());
            AnalyticsController.instance.ReportRenovate(ExternalRoom.ActionType.renovateStart, GetBuildingTag());
        }
        else
        {
            UIController.get.BuyResourcesPopUp.Open(missingItems, false, false, false, () =>
            {
                ConfirmRenew();
            }, null, null, 1, false, -1, hasRequiredCoins ? -1 : Info.RenovationCost - Game.Instance.gameState().GetCoinAmount());
        }
    }

    protected virtual void onInitEnabled() { }

    public float GetTimeToEndRenovation()
    {
        return Info.RenovatingTimeSeconds - timeSinceRenovationStarted;
    }

    IEnumerator<float> Counting(ExternalRoomState state)
    {
        for (; ; )
        {
            if (hover != null)
                hover.UpdateHover();

            if (timeSinceRenovationStarted >= Info.RenovatingTimeSeconds)
            {
                ChangeState(state);
                NotificationCenter.Instance.MaternityWardBuildEndNotif.Invoke(new BaseNotificationEventArgs());
                if (MaternityWardBuildingHover.activeHover != null)
                    MaternityWardBuildingHover.activeHover.Close();
                break;
            }

            timeSinceRenovationStarted++;
            //Debug.LogError("Time since renovation started: " + timeSinceRenovationStarted);
            yield return Timing.WaitForSeconds(1.0f);
        }
    }
    #endregion

    private void TryToRandomizeRequiredItems()
    {
        if (RequiredItems.Count > 0)
            return;
        foreach (KeyValuePair<string, int> pair in DefaultConfigurationProvider.GetConfigCData().ItemsRequiredToUnlockMaternity)
        {
            try
            {
                MedicineRef med = MedicineRef.Parse(pair.Key);
                if (med != null)
                {
                    RequiredItems.Add(ResourcesHolder.Get().GetMedicineInfos(med), pair.Value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    #region Save Load
    private void GenerateDefaultSave()
    {
        timeSinceRenovationStarted = 0;
        ChangeState(ExternalRoomState.Disabled);
        if (Info.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel() && maternityWardObjectState == ExternalRoomState.Disabled)
        {
            ChangeState(ExternalRoomState.WaitingForRenovation);
        }
    }

    private const char MAIN_SEPARATOR = '!';
    private const char SUB_SEPARATOR = '#';

    public List<string> SaveToString()
    {
        List<string> saveData = new List<string>();

        saveData.Add(maternityWardObjectState.ToString());
        saveData.Add(timeSinceRenovationStarted.ToString());
        string RequiredItemsSave = SaveRequiredItems();
        if (!string.IsNullOrEmpty(RequiredItemsSave))
            saveData.Add(RequiredItemsSave);
        return saveData;
    }

    private string SaveRequiredItems()
    {
        StringBuilder builder = new StringBuilder();
        bool first = true;
        foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in RequiredItems)
        {
            if (!first)
            {
                builder.Append(MAIN_SEPARATOR);
            }
            builder.Append(pair.Key.GetMedicineRef().ToString());
            builder.Append(SUB_SEPARATOR);
            builder.Append(pair.Value);
            first = false;
        }
        return builder.ToString();
    }

    public void LoadFromString(List<string> saveData, TimePassedObject timeSinceLastSave)
    {
        if (buildingCoroutine != null)
        {
            Timing.KillCoroutine(buildingCoroutine);
            buildingCoroutine = null;
        }

        if (saveData == null || saveData.Count == 0)
        {
            GenerateDefaultSave();
            return;
        }

        try
        {
            RequiredItems.Clear();
            float loadedRenovationTimeLeft = float.Parse(saveData[1], System.Globalization.CultureInfo.InvariantCulture);
            timeSinceRenovationStarted = loadedRenovationTimeLeft + timeSinceLastSave.GetTimePassed();
            if (saveData.Count > 2)
                LoadRequiredItems(saveData[2]);
            ChangeState((ExternalRoomState)Enum.Parse(typeof(ExternalRoomState), saveData[0]));
        }
        catch (Exception)
        {
            //SettingsPopup.SendLogByEmail(e);
            //Debug.LogError(e.Message + " | " + e.Source + " | " + e.StackTrace);
            GenerateDefaultSave();
        }
    }

    private void LoadRequiredItems(string itemsSave)
    {
        if (string.IsNullOrEmpty(itemsSave))
            return;
        foreach (string itemSave in itemsSave.Split(MAIN_SEPARATOR))
        {
            if (string.IsNullOrEmpty(itemSave))
                continue;
            string[] itemSaveArray = itemSave.Split(SUB_SEPARATOR);
            if (itemSaveArray.Length > 1)
            {
                try
                {
                    MedicineRef med = MedicineRef.Parse(itemSaveArray[0]);
                    if (med != null)
                    {
                        RequiredItems.Add(ResourcesHolder.Get().GetMedicineInfos(med), int.Parse(itemSaveArray[1], System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Item parse Error: " + e.Message);
                }
            }
        }
    }
    #endregion

    #region Indicators Logic
    public void TurnOffIndicator()
    {
        MaternityNotificationIndicators.gameObject.SetActive(false);
    }

    private void SetUpIndicator()
    {
        MaternityNotificationIndicators.gameObject.SetActive(false);
        SubscribeIndicator();
    }

    private void SubscribeIndicator()
    {
        UnsubscribeIndicators();
        ReadyForLabourInformation.ReadyForLabour += MaternityInformationHasArrived;
        PatientArrivedToBedInformation.PatientInBed += MaternityInformationHasArrived;
        BloodTestCompletedInformation.BloodTestCompleted += MaternityInformationHasArrived;
        LabourEndednInformation.LaborEnded += MaternityInformationHasArrived;
        BondingEndedInformation.BondingEnded += MaternityInformationHasArrived;
        PatientCanBeVitaminizedInformation.PatientCanBeVitaminized += MaternityInformationHasArrived;
    }

    private void UnsubscribeIndicators()
    {
        ReadyForLabourInformation.ReadyForLabour -= MaternityInformationHasArrived;
        PatientArrivedToBedInformation.PatientInBed -= MaternityInformationHasArrived;
        BloodTestCompletedInformation.BloodTestCompleted -= MaternityInformationHasArrived;
        LabourEndednInformation.LaborEnded -= MaternityInformationHasArrived;
        BondingEndedInformation.BondingEnded -= MaternityInformationHasArrived;
        PatientCanBeVitaminizedInformation.PatientCanBeVitaminized -= MaternityInformationHasArrived;
    }

    private void MaternityInformationHasArrived()
    {
        if (HospitalAreasMapController.Map.VisitingMode)
        {
            MaternityNotificationIndicators.gameObject.SetActive(false);
            return;
        }

        if (MaternityNotificationIndicators.gameObject.activeInHierarchy != true)
        {
            MaternityNotificationIndicators.gameObject.SetActive(true);
            try
            {
                MaternityNotificationIndicators.Play(AnimHash.ToggleBadge, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
    }
    #endregion

    public override void IsoDestroy()
    {
        UnsubscribeIndicators();
    }

    private string GetBuildingTag()
    {
        return "maternity";
    }

    public enum ExternalRoomState
    {
        Disabled,
        WaitingForRenovation,
        Renovating,
        WaitingForUser,
        Enabled,
    }
}
