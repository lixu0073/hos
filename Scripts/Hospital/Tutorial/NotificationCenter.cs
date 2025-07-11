using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections;
using Hospital;
using System.Collections.Generic;
using TutorialSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Class designed to take care about gathering information about what is happening in entire game. Contains several Notifiers that enables you to send notification of desired type to every listner, or be such a listener.
/// Example use:
/// NotificationCenter.Get().ResourceAmountChanged.Notifications+=MyListenerMethod;
/// or
/// NotificationCenter.Get().ResourceAmountChanged.Invoke(this,new ResourceAmountChangedEventArgs(...));
/// </summary>
public class NotificationCenter : ITutorialSingleton
{
    /// <summary>
    /// Generic Class responsible for commanding event and sending it's notifications.
    /// If you want to add new NotificationType to NotificationCenter - use this class. Just write your own EventArgs basend on provided BaseNotificationEventArgs and voila.
    /// </summary>
    /// <typeparam name="Sender"></typeparam>
    /// <typeparam name="EventArgs"></typeparam>

    public abstract class Notifier
    {
        public abstract void AddListener(Notifier<BaseNotificationEventArgs>.EventHandler eventHandler);
        public abstract void RemoveListener(Notifier<BaseNotificationEventArgs>.EventHandler eventHandler);
    }

    public class Notifier<EventArgs> : Notifier
        where EventArgs : BaseNotificationEventArgs
    {
        public delegate void EventHandler(EventArgs eventArgs);

        public event EventHandler Notification;

        Dictionary<Notifier<BaseNotificationEventArgs>.EventHandler, EventHandler> transformedEventHandlers =
            new Dictionary<Notifier<BaseNotificationEventArgs>.EventHandler, EventHandler>();
        /// <summary>
        /// Method used to send Notification to system.
        /// </summary>
        /// <param name="eventArgs">Object representing information about event. </param>
        public void Invoke(BaseNotificationEventArgs eventArgs)
        {
            //Debug.LogWarning("Invoke notification");
            if (Notification != null)
            {   //Debug.LogWarning("Notification not null!");

                //this is so tutorial dont fire when some notifications are sent when visiting. 
                //Except on Level 6 where tutorial forces player to go to Dr Wise's hospital
                //if (VisitingController.Instance.IsVisiting && Game.Instance.gameState().GetHospitalLevel() != 6)
                //{
                //    Debug.LogError("Tutorial notifications wont be fired because you are visiting someone");
                //    return;
                //}

                Notification.Invoke((EventArgs)eventArgs);
            }
        }

        public override void AddListener(Notifier<BaseNotificationEventArgs>.EventHandler eventHandler)
        {
            if (!transformedEventHandlers.ContainsKey(eventHandler))
            {
                transformedEventHandlers.Add(eventHandler, new EventHandler((args) => { eventHandler.Invoke(args); }));
                this.Notification += transformedEventHandlers[eventHandler];
            }
        }

        public override void RemoveListener(Notifier<BaseNotificationEventArgs>.EventHandler eventHandler)
        {
            if (transformedEventHandlers.ContainsKey(eventHandler))
            {
                this.Notification -= transformedEventHandlers[eventHandler];
                transformedEventHandlers.Remove(eventHandler);
            }
        }

        public bool IsNull()
        {
            return Notification == null;
        }
    }

    #region Notifiers
#if UNITY_EDITOR
    public readonly Notifier<BaseNotificationEventArgs> TestEvent = new Notifier<BaseNotificationEventArgs>();
#endif

    public readonly Notifier<TutorialBeginingEventArgs> TutorialBegining = new Notifier<TutorialBeginingEventArgs>();//
    public readonly Notifier<StepInfoCloseEventArgs> StepInfoClose = new Notifier<StepInfoCloseEventArgs>();//
    public readonly Notifier<InGameEmmaHiddenEventArgs> InGameEmmaHidden = new Notifier<InGameEmmaHiddenEventArgs>();//
    public readonly Notifier<ShowTutorialsInputFieldEventArgs> ShowTutorialsInputField = new Notifier<ShowTutorialsInputFieldEventArgs>();//
    public readonly Notifier<HospitalNamedEventArgs> NamedHospital = new Notifier<HospitalNamedEventArgs>();//
    public readonly Notifier<HideTutorialsInputFieldEventArgs> HideTutorialsInputField = new Notifier<HideTutorialsInputFieldEventArgs>();//
    public readonly Notifier<SheetRemoveEventArgs> SheetRemove = new Notifier<SheetRemoveEventArgs>();//
    public readonly Notifier<PanaceaCollectedEventArgs> PanaceaCollected = new Notifier<PanaceaCollectedEventArgs>();//
    public readonly Notifier<BlueDoctorOfficeAddedEventArgs> BlueDoctorOfficeAdded = new Notifier<BlueDoctorOfficeAddedEventArgs>();//
    public readonly Notifier<BlueDoctorOfficeAddedEventArgs> BlueDoctorOfficeCompleted = new Notifier<BlueDoctorOfficeAddedEventArgs>();//
    public readonly Notifier<YellowDoctorOfficeAddedEventArgs> YellowDoctorOfficeAdded = new Notifier<YellowDoctorOfficeAddedEventArgs>();//
    public readonly Notifier<GreenDoctorOfficeAddedEventArgs> GreenDoctorOfficeAdded = new Notifier<GreenDoctorOfficeAddedEventArgs>();//
    public readonly Notifier<XRayAddedEventArgs> XRayAdded = new Notifier<XRayAddedEventArgs>();//
    public readonly Notifier<ElixirMixerAddedEventArgs> ElixirMixerAdded = new Notifier<ElixirMixerAddedEventArgs>();//
    public readonly Notifier<BoughtWithDiamondsEventArgs> BoughtWithDiamonds = new Notifier<BoughtWithDiamondsEventArgs>();//
    public readonly Notifier<StaticObjectUpgradedEventArgs> StaticObjectUpgraded = new Notifier<StaticObjectUpgradedEventArgs>();//
    public readonly Notifier<BluePotionsCollectedEventArgs> BluePotionsCollected = new Notifier<BluePotionsCollectedEventArgs>();//
    public readonly Notifier<ProductionStartedEventArgs> ProductionStarted = new Notifier<ProductionStartedEventArgs>();//
    public readonly Notifier<ElixirDeliveredEventArgs> ElixirDelivered = new Notifier<ElixirDeliveredEventArgs>();//
    public readonly Notifier<CollectableCollectedEventArgs> CollectableCollected = new Notifier<CollectableCollectedEventArgs>();//
    public readonly Notifier<MoveRotateRoomEndEventArgs> MoveRotateRoomEnd = new Notifier<MoveRotateRoomEndEventArgs>();//
    public readonly Notifier<MoveRotateRoomStartChangingEventArgs> MoveRotateRoomStartChanging = new Notifier<MoveRotateRoomStartChangingEventArgs>();//
    public readonly Notifier<MedicineExistInStorageEventArgs> MedicineExistInStorage = new Notifier<MedicineExistInStorageEventArgs>();//
    public readonly Notifier<ObjectExistOnLevelEventArgs> ObjectExistOnLevel = new Notifier<ObjectExistOnLevelEventArgs>();//
    public readonly Notifier<QueueExtendedEventArgs> QueueExtended = new Notifier<QueueExtendedEventArgs>();//
    public readonly Notifier<SetCurrentlyPointedMachineEventArgs> SetCurrentlyPointedMachine = new Notifier<SetCurrentlyPointedMachineEventArgs>();//
    public readonly Notifier<FinishedBuildingEventArgs> FinishedBuilding = new Notifier<FinishedBuildingEventArgs>();//

    public readonly Notifier<DummyRemovedEventArgs> DummyRemoved = new Notifier<DummyRemovedEventArgs>();//
    public readonly Notifier<ReceptionBuiltEventArgs> ReceptionBuilt = new Notifier<ReceptionBuiltEventArgs>();//
    public readonly Notifier<TutorialArrowSetEventArgs> TutorialArrowSet = new Notifier<TutorialArrowSetEventArgs>();//
    public readonly Notifier<FullscreenTutHiddenEventArgs> FullscreenTutHidden = new Notifier<FullscreenTutHiddenEventArgs>();//
    public readonly Notifier<FirstPatientArrivingEventArgs> FirstPatientArriving = new Notifier<FirstPatientArrivingEventArgs>();//
    public readonly Notifier<SpawnFirstPatientEventArgs> SpawnFirstPatient = new Notifier<SpawnFirstPatientEventArgs>();//
    public readonly Notifier<FirstPatientNearSignOfHospitalEventArgs> FirstPatientNearSignOfHospital = new Notifier<FirstPatientNearSignOfHospitalEventArgs>();//
    public readonly Notifier<ExpandConditionsEventArgs> ExpandConditions = new Notifier<ExpandConditionsEventArgs>();//
    public readonly Notifier<SyrupCollectedEventArgs> BlueSyrupCollected = new Notifier<SyrupCollectedEventArgs>();
    public readonly Notifier<BlueSyrupExtractionCompletedArgs> BlueSyrupExtractionCompleted = new Notifier<BlueSyrupExtractionCompletedArgs>();
    public readonly Notifier<BaseNotificationEventArgs> FollowBob = new Notifier<BaseNotificationEventArgs>();//
    public readonly Notifier<BaseNotificationEventArgs> MedicopterTookOff = new Notifier<BaseNotificationEventArgs>();//
    public readonly Notifier<TreasurePopUpOpenedArgs> TreasurePopUpOpened = new Notifier<TreasurePopUpOpenedArgs>();//
    public readonly Notifier<TreasurePopUpClosedArgs> TreasurePopUpClosed = new Notifier<TreasurePopUpClosedArgs>();//

    public readonly Notifier<FirstEmergencyPatientSpawnedEventArgs> FirstEmergencyPatientSpawned = new Notifier<FirstEmergencyPatientSpawnedEventArgs>();//
    public readonly Notifier<PatientReachedBedEventArgs> PatientReachedBed = new Notifier<PatientReachedBedEventArgs>();//
    public readonly Notifier<AmbulanceReachedHospitalEventArgs> AmbulanceReachedHospital = new Notifier<AmbulanceReachedHospitalEventArgs>();//
    public readonly Notifier<PatientCardOpenedEventArgs> PatientCardOpened = new Notifier<PatientCardOpenedEventArgs>();//
    public readonly Notifier<PatientCardClosedEventArgs> PatientCardClosed = new Notifier<PatientCardClosedEventArgs>();//
    public readonly Notifier<PatientCardIsOpenEventArgs> PatientCardIsOpen = new Notifier<PatientCardIsOpenEventArgs>();//
    public readonly Notifier<PatientCardIsClosedEventArgs> PatientCardIsClosed = new Notifier<PatientCardIsClosedEventArgs>();//
    public readonly Notifier<TapAnywhereEventArgs> TapAnywhere = new Notifier<TapAnywhereEventArgs>();//
    public readonly Notifier<DrawerOpenedEventArgs> DrawerOpened = new Notifier<DrawerOpenedEventArgs>();//
    public readonly Notifier<DrawerClosedEventArgs> DrawerClosed = new Notifier<DrawerClosedEventArgs>();//
    public readonly Notifier<BaseNotificationEventArgs> FriendsDrawerOpened = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> FriendsDrawerClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<RunTimeStepInfoChangedEventArgs> RunTimeStepInfoChanged = new Notifier<RunTimeStepInfoChangedEventArgs>();//
    public readonly Notifier<ObjectBuiltEventArgs> ObjectAdded = new Notifier<ObjectBuiltEventArgs>();//    
    public readonly Notifier<ObjectEventArgs> ObjectSelected = new Notifier<ObjectEventArgs>();//
    public readonly Notifier<ObjectEventArgs> ObjectMoved = new Notifier<ObjectEventArgs>();//
    public readonly Notifier<ResourceAmountChangedEventArgs> ResourceAmountChanged = new Notifier<ResourceAmountChangedEventArgs>();//
    public readonly Notifier<NotEnoughCurrencyEventArgs> NotEnoughCurrency = new Notifier<NotEnoughCurrencyEventArgs>();//
    public readonly Notifier<LevelUpEventArgs> LevelUp = new Notifier<LevelUpEventArgs>();
    public readonly Notifier<DrawerUpdateEventArgs> DrawerUpdate = new Notifier<DrawerUpdateEventArgs>();
    public readonly Notifier<DiamondUsedEventArgs> DiamondUsed = new Notifier<DiamondUsedEventArgs>();
    public readonly Notifier<PatientCuredEventArgs> PatientCured = new Notifier<PatientCuredEventArgs>();//
    public readonly Notifier<PatientCuredInPatientCardEventArgs> PatientCuredInPatientCard = new Notifier<PatientCuredInPatientCardEventArgs>();//
    public readonly Notifier<ErrorEventArgs> Error = new Notifier<ErrorEventArgs>();
    public readonly Notifier<LevelReachedAndClosedEventArgs> LevelReachedAndClosed = new Notifier<LevelReachedAndClosedEventArgs>();//
    public readonly Notifier<CharacterReachedDestinationArgs> CharacterReachedDestination = new Notifier<CharacterReachedDestinationArgs>();//
    public readonly Notifier<PharmacyIsOpenEventArgs> PharmacyIsOpen = new Notifier<PharmacyIsOpenEventArgs>();//
    public readonly Notifier<PharmacyOpenedEventArgs> PharmacyOpened = new Notifier<PharmacyOpenedEventArgs>();//
    public readonly Notifier<PharmacyClosedEventArgs> PharmacyClosed = new Notifier<PharmacyClosedEventArgs>();//
    public readonly Notifier<ResourceAmountChangedEventArgs> PharmacySoldItemClaimed = new Notifier<ResourceAmountChangedEventArgs>();//
    public readonly Notifier<PharmacyOffersClickedEventArgs> PharmacyOffersClicked = new Notifier<PharmacyOffersClickedEventArgs>();//
    public readonly Notifier<TreatmentRoomNotBuiltEventArgs> TreatmentRoomNotBuilt = new Notifier<TreatmentRoomNotBuiltEventArgs>();//
    public readonly Notifier<KidsUIOpenEventArgs> KidsUIOpen = new Notifier<KidsUIOpenEventArgs>();
    public readonly Notifier<KidsUIClosedEventArgs> KidsUIClosed = new Notifier<KidsUIClosedEventArgs>();
    public readonly Notifier<KidsRoomUnlockedEventArgs> KidsRoomUnlocked = new Notifier<KidsRoomUnlockedEventArgs>();
    public readonly Notifier<KidPatientSpawnedEventArgs> KidPatientSpawned = new Notifier<KidPatientSpawnedEventArgs>();
    public readonly Notifier<KidArrivedToKidsRoomEventArgs> KidArrivedToKidsRoom = new Notifier<KidArrivedToKidsRoomEventArgs>();

    public readonly Notifier<SyrupLabAddedEventArgs> SyrupLabAdded = new Notifier<SyrupLabAddedEventArgs>();
    public readonly Notifier<BlueSyrupProductionStartedEventArgs> BlueSyrupProductionStarted = new Notifier<BlueSyrupProductionStartedEventArgs>();//BlueSyrupProductionStarted_Notification
    public readonly Notifier<DoctorRewardCollectedEventArgs> DoctorRewardCollected = new Notifier<DoctorRewardCollectedEventArgs>();

    public readonly Notifier<HospitalRoomsExpandedEventArgs> HospitalRoomsExpanded = new Notifier<HospitalRoomsExpandedEventArgs>();//

    public readonly Notifier<PatientZeroOpenEventArgs> PatientZeroOpen = new Notifier<PatientZeroOpenEventArgs>();
    public readonly Notifier<PatientZeroClosedEventArgs> PatientZeroClosed = new Notifier<PatientZeroClosedEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> KidsClicked = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> DiagnoseSpawn = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<ExpAmountChangedEventArgs> ExpAmountChanged = new Notifier<ExpAmountChangedEventArgs>();
    public readonly Notifier<ExpAmountChangedEventArgs> ExpAmountChangedNonLinear = new Notifier<ExpAmountChangedEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> WiseHospitalLoaded = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VIPTeaseMedicopter = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VIPMedicopterStarted = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VIPSpawned = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VIPReachedBed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipNotCured = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipRoomStartedBuilding = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PackageArrived = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PackageClicked = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PackageCollected = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> HomeHospitalLoaded = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> PatioElementCleared = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PatioNotCleared = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipPopUpOpen = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> BoxPopupClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PatioDecorationsAdded = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> NotEnoughPanacea = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PushNotificationsDisabled = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> VitaminMakerPopupOpen = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> CuredVipCountIsEnough = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipUpgradeTutorial1Closed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipUpgradeTutorial2Closed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipUpgradeTutorial3Closed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipUpgradeTutorial4Closed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipSpeedup0Closed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> SkipVipSpeedupTutorial = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> BoostersPopUpOpen = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> GiftReady = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> OpenWelcomePopUp = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> CloseWelcomePopUp = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> EpidemyStarting = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> EpidemyClicked = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> EpidemyCompleted = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> TwentyProbeTables = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> TenPatioDecorations = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> SecondDiagnosticMachineOpen = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> Level10NewspaperClosedCond = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> Level10NewspaperClosedNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> Level10WiseClosedCond = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> Level10WiseClosedNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VitaminesMakerEmma1ClosedNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> Level10WiseGiveBooster = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<LevelReachedAndClosedEventArgs> LevelReachedAndClosedNonLinear = new Notifier<LevelReachedAndClosedEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> GlobalEventStarted = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VitaminesMakerEmma1ClosedCond = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> MaternityWardObjectClikedNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> MaternityWardRenovateNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> MaternityWardBuildEndNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BlueDoctorOfficeAddedEventArgs> WaitingRoomBlueOrchidAdded = new Notifier<BlueDoctorOfficeAddedEventArgs>();
    public readonly Notifier<BlueDoctorOfficeAddedEventArgs> LaborRoomBlueOrchidAdded = new Notifier<BlueDoctorOfficeAddedEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> BubbleBoyClicked = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> BubbleBoyAvailable = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> TreasureCollected = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> TreasureClicked = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipFlyByStart = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipFlyByEnd = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> DailyQuestPopUpOpen = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> DailyQuestPopUpClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> ShowDailyQuestAnimation = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> LevelGoalsActive = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> MicroscopeGoodBacteriaAdded = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> MicroscopeClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> OliviaLetterClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> MotherReachedWaitingRoomNotif = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<SetCurrentlyPointedMachineEventArgs> WaitingRoomWorkingClickedNotif = new Notifier<SetCurrentlyPointedMachineEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> MotherVitaminazed = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> NewspaperRewardDiamond = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> NewspaperRewardExp = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> MailBoxClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PackageText = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PillMakerAdded = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> BoosterPopupClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> ObjectivePanelOpened = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> ObjectivePanelClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> PatientSentToXRay = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> FirstPlantPlanted = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> EpidemyCenterBuilt = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> DailyQuestCardFlipped = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> VipSpeedupPopupOpened = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipSpeedupPopupClosed = new Notifier<BaseNotificationEventArgs>();
    public readonly Notifier<BaseNotificationEventArgs> VipPatientCured = new Notifier<BaseNotificationEventArgs>();

    public readonly Notifier<BaseNotificationEventArgs> BubbleBoyGameClosed = new Notifier<BaseNotificationEventArgs>();

    #endregion

    #region constructors and initialization
    private NotificationCenter() { }

    public ITutorialSingleton SingletonInstance { get { return Instance; } }

    private static NotificationCenter instance = null;

    public static NotificationCenter Instance
    {
        get
        {
            if (instance == null)
                instance = new NotificationCenter();

            return instance;
        }
    }

    public string[] GetAllEventsNames()
    {
        if (notifierDict != null && notifierDict.Count != 0)
            return notifierDict.Keys.ToArray();

        return typeof(NotificationCenter).GetFields().
            Where(x => x.FieldType.GetGenericArguments()[0].BaseType == typeof(BaseNotificationEventArgs) ||
            x.FieldType.GetGenericArguments()[0] == typeof(BaseNotificationEventArgs)).Select(x => x.Name).
            ToArray();
    }

    public Dictionary<string, Notifier> CreateNotifierDictionary()
    {
        var dict = new Dictionary<string, Notifier>();
        FieldInfo[] fields = typeof(NotificationCenter).GetFields().
            Where(x => x.FieldType.GetGenericArguments()[0].BaseType == typeof(BaseNotificationEventArgs) ||
            x.FieldType.GetGenericArguments()[0] == typeof(BaseNotificationEventArgs)).
            ToArray();
        foreach (var field in fields)
        {
            dict.Add(field.Name, field.GetValue(instance) as Notifier);
        }
        return dict;
    }

    //Doesnt it need a rework or is it a solid solution?
    public static void UnsubscribeAllNotification()
    {
        //Debug.LogError("UnsubscribeAllNotification");
        if (instance != null)
        {
            instance = null;
            // http://www.informit.com/articles/article.aspx?p=101722&seqNum=2
            System.GC.Collect();
        }
        instance = new NotificationCenter();

        //NotificationListener.Instance.SubscribeLevelUpNotification();
        //NotificationListener.Instance.SubscribeDrawerUpdateNotification();
    }


    //public static void UnsuscribeAllNotification(NotificationListener nL)
    //{
    //    if (instance != null)
    //    {
    //        instance = null;
    //        // http://www.informit.com/articles/article.aspx?p=101722&seqNum=2
    //        System.GC.Collect();
    //    }
    //    instance = new NotificationCenter();
    //
    //    nL.SubscribeLevelUpNotification();
    //    nL.SubscribeDrawerUpdateNotification();
    //
    //}

    Dictionary<string, Notifier> notifierDict;

    public void SubscribeToEventByName(string name, Notifier<BaseNotificationEventArgs>.EventHandler handler)
    {
        if (notifierDict == null)
            notifierDict = CreateNotifierDictionary();

        if (notifierDict.ContainsKey(name))
            notifierDict[name].AddListener(handler);
        else
            Debug.LogError("No such event in dictionary!");
    }

    public void UnsubscribeFromEventByName(string name, Notifier<BaseNotificationEventArgs>.EventHandler handler)
    {
        if (notifierDict == null)
            notifierDict = CreateNotifierDictionary();

        if (notifierDict.ContainsKey(name))
            notifierDict[name].RemoveListener(handler);
    }
    #endregion
}

#region NotificationEventArgs classes
[System.Serializable]
public class BaseNotificationEventArgs : System.EventArgs
{
    public string message;
    public BaseNotificationEventArgs(string message = null)
    {
        this.message = message;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public override bool Equals(object obj)
    {
        var other = obj as BaseNotificationEventArgs;
        return other != null && message == other.message;
    }

    public override int GetHashCode()
    {
        return 795549828 + EqualityComparer<string>.Default.GetHashCode(message);
    }

    public virtual object[] ToObjectArray()
    {
        return new object[] { message };
    }
}
public class TutorialBeginingEventArgs : BaseNotificationEventArgs
{
    public TutorialBeginingEventArgs(string message = null) : base(message) { }
}
public class StepInfoCloseEventArgs : BaseNotificationEventArgs
{
    public StepInfoCloseEventArgs(string message = null) : base(message) { }
}

public class InGameEmmaHiddenEventArgs : BaseNotificationEventArgs
{
    public InGameEmmaHiddenEventArgs(string message = null) : base(message) { }
}

public class ShowTutorialsInputFieldEventArgs : BaseNotificationEventArgs
{
    public ShowTutorialsInputFieldEventArgs(string message = null) : base(message) { }
}

public class HospitalNamedEventArgs : BaseNotificationEventArgs
{
    public readonly string Name;
    public HospitalNamedEventArgs(string name, string message = null) : base(message)
    {
        this.Name = name;
    }
}

public class TreasurePopUpOpenedArgs : BaseNotificationEventArgs
{
    public TreasurePopUpOpenedArgs(string message = null) : base(message) { }
}

public class TreasurePopUpClosedArgs : BaseNotificationEventArgs
{
    public TreasurePopUpClosedArgs(string message = null) : base(message) { }
}

public class SyrupCollectedEventArgs : BaseNotificationEventArgs
{
    public SyrupCollectedEventArgs(string message = null) : base(message) { }
}

public class BlueSyrupExtractionCompletedArgs : BaseNotificationEventArgs
{
    public bool SpeedUpUsed;
    public BlueSyrupExtractionCompletedArgs(bool IsSpeedUpUsed, string message = null) : base(message)
    {
        SpeedUpUsed = IsSpeedUpUsed;
    }
}

public class HideTutorialsInputFieldEventArgs : BaseNotificationEventArgs
{
    public HideTutorialsInputFieldEventArgs(string message = null) : base(message) { }
}

public class SheetRemoveEventArgs : BaseNotificationEventArgs
{
    public readonly string BulidingName;
    public SheetRemoveEventArgs(string buildingName, string message = null) : base(message)
    {
        this.BulidingName = buildingName;
    }
}
public class PanaceaCollectedEventArgs : BaseNotificationEventArgs
{
    public PanaceaCollectedEventArgs(string message = null) : base(message) { }
}

public class SetCurrentlyPointedMachineEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject obj;

    public SetCurrentlyPointedMachineEventArgs(RotatableObject rotObj, string message = null) : base(message)
    {
        this.obj = rotObj;
    }
}

public class BlueDoctorOfficeAddedEventArgs : BaseNotificationEventArgs
{
    public readonly string RoomName;
    public readonly RotatableObject obj;
    public BlueDoctorOfficeAddedEventArgs(RotatableObject rotObj, string roomName, string message = null) : base(message)
    {
        this.RoomName = roomName;
        this.obj = rotObj;
    }
}

public class YellowDoctorOfficeAddedEventArgs : BaseNotificationEventArgs
{
    public readonly string RoomName;
    public readonly RotatableObject obj;
    public YellowDoctorOfficeAddedEventArgs(RotatableObject rotObj, string roomName, string message = null) : base(message)
    {
        this.RoomName = roomName;
        this.obj = rotObj;
    }
}

public class GreenDoctorOfficeAddedEventArgs : BaseNotificationEventArgs
{
    public readonly string RoomName;
    public readonly RotatableObject obj;
    public GreenDoctorOfficeAddedEventArgs(RotatableObject rotObj, string roomName, string message = null) : base(message)
    {
        this.RoomName = roomName;
        this.obj = rotObj;
    }
}

public class XRayAddedEventArgs : BaseNotificationEventArgs
{
    public readonly string RoomName;
    public readonly RotatableObject obj;
    public XRayAddedEventArgs(RotatableObject rotObj, string roomName, string message = null) : base(message)
    {
        this.RoomName = roomName;
        this.obj = rotObj;
    }
}

public class ElixirMixerAddedEventArgs : BaseNotificationEventArgs
{
    public readonly string RoomName;
    public readonly RotatableObject obj;
    public ElixirMixerAddedEventArgs(RotatableObject rotObj, string roomName, string message = null) : base(message)
    {
        this.RoomName = roomName;
        this.obj = rotObj;
    }
}


public class SyrupLabAddedEventArgs : BaseNotificationEventArgs
{
    public readonly string RoomName;
    public readonly RotatableObject obj;
    public SyrupLabAddedEventArgs(RotatableObject rotObj, string roomName, string message = null) : base(message)
    {
        this.RoomName = roomName;
        this.obj = rotObj;
    }
}

public class BlueSyrupProductionStartedEventArgs : BaseNotificationEventArgs
{
    public BlueSyrupProductionStartedEventArgs(string message = null) : base(message) { }
}

public class DoctorRewardCollectedEventArgs : BaseNotificationEventArgs
{
    public DoctorRewardCollectedEventArgs(string message = null) : base(message) { }
}

public class RunTimeStepInfoChangedEventArgs : BaseNotificationEventArgs
{
    public RunTimeStepInfoChangedEventArgs(string message = null) : base(message) { }
}

public class BoughtWithDiamondsEventArgs : BaseNotificationEventArgs
{
    public BoughtWithDiamondsEventArgs(string message = null) : base(message) { }
}
public class BluePotionsCollectedEventArgs : BaseNotificationEventArgs
{
    public BluePotionsCollectedEventArgs(string message = null) : base(message) { }
}

public class ProductionStartedEventArgs : BaseNotificationEventArgs
{
    public ProductionStartedEventArgs(string message = null) : base(message) { }
}
public class ElixirDeliveredEventArgs : BaseNotificationEventArgs
{
    public ElixirDeliveredEventArgs(string message = null) : base(message) { }
}
public class CollectableCollectedEventArgs : BaseNotificationEventArgs
{
    public CollectableCollectedEventArgs(string message = null) : base(message) { }
}

public class MoveRotateRoomStartChangingEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject rObj;
    public MoveRotateRoomStartChangingEventArgs(string message = null, RotatableObject rObj = null) : base(message)
    {
        this.rObj = rObj;
    }
}

public class MoveRotateRoomEndEventArgs : BaseNotificationEventArgs
{
    public MoveRotateRoomEndEventArgs(string message = null) : base(message) { }
}

public class MedicineExistInStorageEventArgs : BaseNotificationEventArgs
{
    public MedicineExistInStorageEventArgs(MedicineRef medicineType, int medicineRate, string message = null) : base(message)
    {
        this.medicineRate = medicineRate;
        this.medicineType = medicineType;
    }

    public readonly int medicineRate;
    public readonly MedicineRef medicineType;
}

public class ObjectExistOnLevelEventArgs : BaseNotificationEventArgs
{
    public ObjectExistOnLevelEventArgs(string message = null) : base(message) { }
}

public class QueueExtendedEventArgs : BaseNotificationEventArgs
{
    public QueueExtendedEventArgs(string message = null) : base(message) { }
}
public class ResourceAmountChangedEventArgs : BaseNotificationEventArgs
{
    public ResourceAmountChangedEventArgs(MedicineRef medicine, int by, int newAmount, EconomySource source, string message = null) : base(message)
    {
        this.medicine = medicine;
        this.by = by;
        this.newAmount = newAmount;
        this.source = source;
    }
    public readonly MedicineRef medicine;
    public readonly int by;
    public readonly int newAmount;
    public EconomySource source;

    public override object[] ToObjectArray()
    {
        return new object[] { medicine, by, newAmount, source };
    }
}
public class TutorialArrowSetEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject rObj;
    public readonly GameObject gObj;
    public readonly IState patientState;
    public TutorialArrowSetEventArgs(RotatableObject rObj, string message = null) : base(message)
    {
        this.rObj = rObj;
    }
    public TutorialArrowSetEventArgs(GameObject gObj, string message = null) : base(message)
    {
        this.gObj = gObj;
    }
    public TutorialArrowSetEventArgs(IState patientState)
    {
        this.patientState = patientState;
    }
}
/*public class FinishedBuildingEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject Obj;
    public FinishedBuildingEventArgs(RotatableObject rObj, string message = null) : base(message)
    {
        this.Obj = rObj;

        if (this.Obj.GetType () == typeof(DoctorRoom)) {
            NotificationCenter.Instance.ClinicRoomBuilt.Invoke (new AchievementProgressEventArgs (1));
        } else if (this.Obj.GetType () == typeof(MedicineProductionMachine)) {
            NotificationCenter.Instance.CureLabBuilt.Invoke (new AchievementProgressEventArgs (1));
        }
    }
}*/

/*public class PlantPickedEventArgs : BaseNotificationEventArgs
{
    public PlantPickedEventArgs(string message = null) : base (message)
    {
        Debug.Log ("Plant Picked");
        NotificationCenter.Instance.MedicalPlantsPicked.Invoke (new AchievementProgressEventArgs (1));
    }
}*/

public class DummyRemovedEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject Obj;
    public readonly ExternalRoom ExternalObject;
    public DummyRemovedEventArgs(RotatableObject rObj, string message = null) : base(message)
    {
        this.Obj = rObj;
    }
    public DummyRemovedEventArgs(ExternalRoom exObj, string message = null) : base(message)
    {
        ExternalObject = exObj;
    }
}
public class ObjectBuiltEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject obj;
    public readonly bool wasStored;
    public ObjectBuiltEventArgs(RotatableObject rotObj, bool wasStored, string message = null) : base(message)
    {
        this.obj = rotObj;
        this.wasStored = wasStored;
    }

    public override bool Equals(object obj)
    {
        var other = obj as ObjectEventArgs;
        return other != null && this.obj == other.obj;
    }

    public override int GetHashCode()
    {
        var hashCode = -327739888;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<RotatableObject>.Default.GetHashCode(obj);
        return hashCode;
    }

    public override object[] ToObjectArray()
    {
        return new object[] { message, obj, wasStored };
    }
}
public class ObjectEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject obj;
    public ObjectEventArgs(RotatableObject rotObj, string message = null) : base(message)
    {
        obj = rotObj;
    }

    public override bool Equals(object obj)
    {
        var other = obj as ObjectEventArgs;
        return other != null && this.obj == other.obj;
    }

    public override int GetHashCode()
    {
        var hashCode = -327739888;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<RotatableObject>.Default.GetHashCode(obj);
        return hashCode;
    }

    public override object[] ToObjectArray()
    {
        return new object[] { message, obj };
    }
}
public class NotEnoughCurrencyEventArgs : BaseNotificationEventArgs
{
    public NotEnoughCurrencyEventArgs(ResourceType resource, int amountMissing, string message = null) : base(message)
    {
        type = resource;
        this.amountMissing = amountMissing;
    }
    public readonly ResourceType type;
    public readonly int amountMissing;
}
public class LevelUpEventArgs : BaseNotificationEventArgs
{
    public LevelUpEventArgs(int newLevel, string message = null) : base(message)
    {
        this.newLevel = newLevel;
    }
    public readonly int newLevel;
}

public class DrawerUpdateEventArgs : BaseNotificationEventArgs
{
    public DrawerUpdateEventArgs(string message = null) : base(message)
    {
    }
}


public class ReceptionBuiltEventArgs : BaseNotificationEventArgs
{
    public readonly SuperObject SuperObj;
    public ReceptionBuiltEventArgs(SuperObject superObj, string message = null) : base(message)
    {
        this.SuperObj = superObj;
    }
}

public class HospitalRoomsExpandedEventArgs : BaseNotificationEventArgs
{
    public HospitalRoomsExpandedEventArgs(string message = null) : base(message) { }
}

public class FullscreenTutHiddenEventArgs : BaseNotificationEventArgs
{
    public FullscreenTutHiddenEventArgs(string message = null) : base(message)
    {
    }
}

public class DrawerClosedEventArgs : BaseNotificationEventArgs
{
    public DrawerClosedEventArgs(string message = null) : base(message)
    {
    }
}

public class DrawerOpenedEventArgs : BaseNotificationEventArgs
{
    public DrawerOpenedEventArgs(string message = null) : base(message)
    {
    }
}

public class FirstPatientArrivingEventArgs : BaseNotificationEventArgs
{
    public FirstPatientArrivingEventArgs(string message = null) : base(message)
    {
    }
}

public class SpawnFirstPatientEventArgs : BaseNotificationEventArgs
{
    public SpawnFirstPatientEventArgs(string message = null) : base(message)
    {
    }
}

public class FirstPatientNearSignOfHospitalEventArgs : BaseNotificationEventArgs
{
    public FirstPatientNearSignOfHospitalEventArgs(string message = null) : base(message)
    {
    }
}
public class ExpandConditionsEventArgs : BaseNotificationEventArgs
{
    public ExpandConditionsEventArgs(string message = null) : base(message)
    {
    }
}

public class PatientReachedBedEventArgs : BaseNotificationEventArgs
{
    public bool requiresDiagnosis;
    public bool hasBacteria;
    public PatientReachedBedEventArgs(bool requiresDiagnosis, bool hasBacteria, string message = null) : base(message)
    {
        this.requiresDiagnosis = requiresDiagnosis;
        this.hasBacteria = hasBacteria;
    }
}

public class FirstEmergencyPatientSpawnedEventArgs : BaseNotificationEventArgs
{
    public readonly GameObject patientGO;
    public FirstEmergencyPatientSpawnedEventArgs(GameObject patientGO, string message = null) : base(message)
    {
        this.patientGO = patientGO;
    }
}

public class AmbulanceReachedHospitalEventArgs : BaseNotificationEventArgs
{
    public AmbulanceReachedHospitalEventArgs(string message = null) : base(message)
    {
    }
}

public class PatientCardOpenedEventArgs : BaseNotificationEventArgs
{
    public HospitalCharacterInfo info;
    public PatientCardOpenedEventArgs(HospitalCharacterInfo info, string message = null) : base(message)
    {
        this.info = info;
    }
}
public class PatientCardClosedEventArgs : BaseNotificationEventArgs
{
    public PatientCardClosedEventArgs(string message = null) : base(message)
    {
    }
}

public class FirstPatientInBedEventArgs : BaseNotificationEventArgs
{
    public readonly GameObject gObj;
    public readonly HospitalRoom temp;
    public readonly RotatableObject rObj;
    public FirstPatientInBedEventArgs(string message = null) : base(message)
    {
        this.rObj = TutorialController.Instance.FindObjectForStep();
        temp = (HospitalRoom)rObj;
        this.gObj = temp.SpawnPersonInBed().gameObject;
    }
}

public class PatientCardIsOpenEventArgs : BaseNotificationEventArgs
{
    public bool hasBacteria;
    public PatientCardIsOpenEventArgs(bool hasBacteria, string message = null) : base(message)
    {
        this.hasBacteria = hasBacteria;
    }
}
public class PatientCardIsClosedEventArgs : BaseNotificationEventArgs
{
    public PatientCardIsClosedEventArgs(string message = null) : base(message)
    {
    }
}
public class StaticObjectUpgradedEventArgs : BaseNotificationEventArgs
{
    public StaticObjectUpgradedEventArgs(string message = null) : base(message)
    {

    }
}

public class TapAnywhereEventArgs : BaseNotificationEventArgs
{
    public TapAnywhereEventArgs(string message = null) : base(message)
    {
    }
}
public class DiamondUsedEventArgs : BaseNotificationEventArgs
{
    public DiamondUsedEventArgs(int amountUsed, DiamondUsage usage, string message = null) : base(message)
    {
        this.amountUsed = amountUsed;
        diamondUsage = usage;
    }
    public readonly int amountUsed;
    public readonly DiamondUsage diamondUsage;

}

public enum DiamondUsage
{
    FastBuild,
    FastProduce,
    AdditionalSpot,
}
public class AreaChangedEventArgs : BaseNotificationEventArgs
{
    public AreaChangedEventArgs(string message = null) : base(message)
    {

    }
}
public class PatientCuredEventArgs : BaseNotificationEventArgs
{
    public readonly BasePatientAI patient;
    public readonly string where;
    public PatientCuredEventArgs(BasePatientAI patient, string where = null, string message = null) : base(message)
    {
        this.patient = patient;
        this.where = where;
    }
}

public class PatientCuredInPatientCardEventArgs : BaseNotificationEventArgs
{
    public readonly BasePatientAI patient;
    public PatientCuredInPatientCardEventArgs(BasePatientAI patient, string message = null) : base(message)
    {
        this.patient = patient;
    }
}

public class ErrorEventArgs : BaseNotificationEventArgs
{
    public ErrorEventArgs(string message = null) : base(message)
    {
    }
}

public class LevelReachedAndClosedEventArgs : BaseNotificationEventArgs
{
    public readonly int level;
    public LevelReachedAndClosedEventArgs(int level, string message = null) : base(message)
    {
        this.level = level;
    }
}

public class CharacterReachedDestinationArgs : BaseNotificationEventArgs
{
    public CharacterReachedDestinationArgs(string message = null) : base(message)
    {

    }
}

public class PharmacyIsOpenEventArgs : BaseNotificationEventArgs
{
    public PharmacyIsOpenEventArgs(string message = null) : base(message)
    {
    }
}

public class PharmacyOpenedEventArgs : BaseNotificationEventArgs
{
    public PharmacyOpenedEventArgs(string message = null) : base(message)
    {
    }
}

public class PharmacyClosedEventArgs : BaseNotificationEventArgs
{
    public PharmacyClosedEventArgs(string message = null) : base(message)
    {
    }
}

public class PharmacyOffersClickedEventArgs : BaseNotificationEventArgs
{
    public PharmacyOffersClickedEventArgs(string message = null) : base(message)
    {
    }
}

public class TreatmentRoomNotBuiltEventArgs : BaseNotificationEventArgs
{
    public TreatmentRoomNotBuiltEventArgs(string message = null) : base(message)
    {
    }
}

public class KidsUIOpenEventArgs : BaseNotificationEventArgs
{
    public KidsUIOpenEventArgs(string message = null) : base(message)
    {
    }
}

public class KidsUIClosedEventArgs : BaseNotificationEventArgs
{
    public KidsUIClosedEventArgs(string message = null) : base(message)
    {
    }
}
public class KidsRoomUnlockedEventArgs : BaseNotificationEventArgs
{
    public KidsRoomUnlockedEventArgs(string message = null) : base(message)
    {
    }
}
public class KidPatientSpawnedEventArgs : BaseNotificationEventArgs
{
    public KidPatientSpawnedEventArgs(string message = null) : base(message)
    {
    }
}
public class KidArrivedToKidsRoomEventArgs : BaseNotificationEventArgs
{
    public KidArrivedToKidsRoomEventArgs(string message = null) : base(message)
    {
    }
}

public class PatientZeroOpenEventArgs : BaseNotificationEventArgs
{
    public ClinicCharacterInfo CharacterInfo { get; private set; }

    public PatientZeroOpenEventArgs(ClinicCharacterInfo characterInfo, string message = null) : base(message)
    {
        this.CharacterInfo = characterInfo;
    }
}
public class PatientZeroClosedEventArgs : BaseNotificationEventArgs
{
    public PatientZeroClosedEventArgs(string message = null) : base(message)
    {
    }
}

public class ExpAmountChangedEventArgs : BaseNotificationEventArgs
{
    public readonly int level = 0;
    public readonly int expOnLevel = 0;
    public ExpAmountChangedEventArgs(int level, int expOnLevel, string message = null) : base(message)
    {
        this.level = level;
        this.expOnLevel = expOnLevel;
    }
}


/*public class TimedAchievementProgressEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public readonly int occurred;
    public TimedAchievementProgressEventArgs(int amount, int occurred, string message = null) : base(message) { this.amount = amount; this.occurred = occurred;  }
}

public class AchievementProgressEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public AchievementProgressEventArgs(int amount, string message = null) : base(message) 
    { this.amount = amount; }
}

public class AchievementVIPInfoEventArgs : BaseNotificationEventArgs
{
    public BaseCharacterInfo info;
    public readonly int occurred;
    public AchievementVIPInfoEventArgs(BaseCharacterInfo info, int occurred, string message = null) : base(message) { this.info = info; this.occurred = occurred; }
}*/

public class ObjectSelectedEventArgs : BaseNotificationEventArgs
{
    public readonly RotatableObject obj;
    public ObjectSelectedEventArgs(RotatableObject rotObj, string message = null) : base(message)
    {
        obj = rotObj;
    }
}


#endregion