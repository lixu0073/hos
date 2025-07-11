using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HospitalTutorialController : TutorialController
{
    private static HospitalTutorialController instance = null;

    public static HospitalTutorialController HospitalTutorialInstance
    {
        get { return instance; }
    }

    public GameObject KidsClickArea;
    public GameObject VipcopterFlybyDummy;

    protected override void Start()
    {
        base.Start();
        instance = this;

        if (HospitalTutorialInstance != null && HospitalTutorialInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        //Debug.LogError("SERVER TIME: " + ServerTime.GetServerTimeDirectlyFromServer().ToString() + " , TIMESTAMP: " + ServerTime.DateTimeToUnixTimestamp(ServerTime.serverTime));

        instance = this;
    }

    public override void ResetTutorialSpecialAnimsPos()
    {
        VipcopterFlybyDummy.SetActive(false);
        VipcopterFlybyDummy.transform.position = new Vector3(52, 0, 60);
    }

    protected override void SetTutorialStep()
    {
        //if (CurrentTutorialStepIndex < GetStepId(StepTag.build_doctor_text))
        //    TutorialUIController.Instance.HideDrawerButton();
        //else
        //    TutorialUIController.Instance.ShowDrawerButton();

        if (!CampaignConfig.hintSystemEnabled)
        {
            if (currentTutorialStep.forceShowObjectives
                && ReferenceHolder.Get().objectiveController.ObjectivesSet
                && !ReferenceHolder.Get().objectiveController.ObjectivesCompletedAndClaimed
                && !UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled)
                if (!(CurrentTutorialStepTag == StepTag.objective_panel_closed ||
                      CurrentTutorialStepTag == StepTag.follow_ambulance))
                    UIController.getHospital.ObjectivesPanelUI.SlideIn(1f);
        }

        if (CurrentTutorialStepIndex < GetStepId(StepTag.emma_about_wise) && WiseVisitedThisSession)
        {
            //this happens when you visit wise, your saves stop updating so the last save is from this step, and all the visiting steps loop indefinitely
            TutorialController.Instance.SetStep(StepTag.emma_about_wise);
            return;
        }


        if (TutorialStepsList.Count > CurrentTutorialStepIndex)
        {
            if (currentTutorialStep.NecessaryCondition == Condition.NoCondition || ConditionFulified)
            {
                int stepID = CurrentTutorialStepIndex;
                CheckSubscriptionMoment();
                // if (stepID == CurrentTutorialStepIndex) checks whether we are still on the same step after CheckSubscriptionMoment()
                // this is to avoid targetting the camera at a place specified in a skipped step
                if (stepID == CurrentTutorialStepIndex &&
                    currentTutorialStep.CameraTargetingType != CameraTargetingType.None)
                {
                    TutorialUIController.Instance.TargetCamera(currentTutorialStep);
                }
            }
            else
            {
                SubscribeToStepCondition(currentTutorialStep.NecessaryCondition);
            }
        }
        else
        {
            Debug.Log("Tutorial is over.");
        }

        if (currentTutorialStep.additionalGraphics.Length > 0)
        {
            foreach (var graphic in currentTutorialStep.additionalGraphics)
                graphic.Show();
        }

        InvokeNewStepEvent(currentTutorialStep);
        UIController.get.SetBoosterAndBoxButtons();
        UIController.get.SetDailyQuestsButton();
    }

    public override void StopTutorialCameraFollowingForSpecialObjects()
    {
        if (ReferenceHolder.Get().engine.MainCamera.CompareFollowingObjects(VipcopterFlybyDummy.transform))
            ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        else if (ReferenceHolder.Get().engine.MainCamera.CompareFollowingObjects(HospitalAreasMapController.HospitalMap
                     .vipRoom.GetComponent<VIPSystemManager>().Helipod.transform))
            ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        else
        {
            var currentVipObj = ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>().currentVip;
            if (currentVipObj != null &&
                ReferenceHolder.Get().engine.MainCamera.CompareFollowingObjects(currentVipObj.transform))
                ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        }
    }


    public override void CheckSubscriptionMoment()
    {
        CheckNotificationTypeToSubscribe();
        ShowCurrentStepPopupInformation();
    }

    public void CheckNotificationTypeToSubscribe()
    {
        //Debug.Log("Przypisanie typu notyfikacji: " + currentTutorialStep.NotificationType);
        switch (currentTutorialStep.NotificationType)
        {
            //case NotificationType.None:
            //    IncrementCurrentStep();
            //    break;
            //case NotificationType.TutorialBegining:
            //    instanceNL.SubscribeTutorialBeginingNotification();
            //    break;
            //case NotificationType.ShowTutorialsInputField:
            //    instanceNL.SubscribeShowTutorialsInputFieldNotification();
            //    break;
            //case NotificationType.HideTutorialsInputField:
            //    instanceNL.SubscribeHideTutorialsInputFieldNotification();
            //    break;
            //case NotificationType.SheetRemove:
            //    instanceNL.SubscribeSheetRemoveNotification();
            //    break;
            //case NotificationType.ObjectSelected:
            //    instanceNL.SubscribeObjectSelectedNotification();
            //    break;
            //case NotificationType.PanaceaCollected:
            //    instanceNL.SubscribePanaceaCollectedNotification();
            //    break;
            //case NotificationType.BlueDoctorsOfficeAdded:
            //    instanceNL.SubscribeBlueDoctorOfficeNotification();
            //    break;
            //case NotificationType.FinishedBuilding:
            //    instanceNL.SubscribeFinishedBuildingNotification();
            //    break;
            //case NotificationType.BoughtWithDiamonds:
            //    instanceNL.SubscribeBoughtWithDiamondsNotification();
            //    break;
            //case NotificationType.StaticObjectUpgraded:
            //    instanceNL.SubscribeStaticObjectUpgradedNotification();
            //    break;
            //case NotificationType.BluePotionsCollected:
            //    instanceNL.SubscribeBluePotionsCollectedNotification();
            //    break;
            //case NotificationType.ProductionStarted:
            //    instanceNL.SubscribeProductionStartedNotification();
            //    break;
            //case NotificationType.ElixirDelivered:
            //    instanceNL.SubscribeElixirDeliveredNotification();
            //    break;
            //case NotificationType.CollectableCollected:
            //    instanceNL.SubscribeCollectableCollectedNotification();
            //    break;
            //case NotificationType.QueueExtended:
            //    instanceNL.SubscribeQueueExtendedNotification();
            //    break;
            //case NotificationType.InGameEmmaHidden:
            //    instanceNL.SubscribeInGameEmmaHiddenNotification();
            //    break;
            //case NotificationType.ReceptionBuilt:
            //    instanceNL.SubscribeReceptionBuiltdNotification();
            //    break;
            //case NotificationType.PatientCured:
            //    instanceNL.SubscribePatientCuredNotification();
            //    break;
            //case NotificationType.VIPPatientCured:
            //    instanceNL.SubscribeVIPGiftPopupClosed();
            //    instanceNL.SubscribeSkipVipSpeedupTutorial();
            //    break;
            //case NotificationType.DummyRemoved:
            //    instanceNL.SubscribeDummyRemovedNotification();
            //    break;
            //case NotificationType.DrawerOpened:
            //    instanceNL.SubscribeDrawerOpenedNotification();
            //    break;
            //case NotificationType.FullscreenTutHidden:
            //    instanceNL.SubscribeFullscreenTutHiddenNotification();
            //    break;
            //case NotificationType.AmbulanceReachedHospital:
            //    instanceNL.SubscribeAmbulanceReachedHospitalNotification();
            //    break;
            //case NotificationType.FirstEmergencyPatientReachedBed:
            //    instanceNL.SubscribeFirstEmergencyPatientReachedBedNotification();
            //    break;
            //case NotificationType.PatientCardOpened:
            //    instanceNL.SubscribePatientCardOpenedNotification();
            //    break;
            //case NotificationType.PatientCardClosed:
            //    instanceNL.SubscribePatientCardClosedNotification();
            //    break;
            //case NotificationType.TapAnywhere:
            //    instanceNL.SubscribeTapAnywhereNotification();
            //    break;
            //case NotificationType.SyrupLabAdded:
            //    instanceNL.SubscribeSyrupLabAddedNotification();
            //    break;
            //case NotificationType.MedicineExistInStorage:
            //    instanceNL.SubscribeMedicineExistInStorageNotification();
            //    break;
            //case NotificationType.StepInfoClose:
            //    instanceNL.SubscribeStepInfoCloseNotification();
            //    break;
            //case NotificationType.PatientCuredInPatientCard:
            //    instanceNL.SubscribePatientCuredInPatientCardNotification();
            //    break;
            //case NotificationType.YellowDoctorsOfficeAdded:
            //    instanceNL.SubscribeYellowDoctorOfficeNotification();
            //    break;
            //case NotificationType.MoveRotateRoomStartChanging:
            //    instanceNL.SubscribeMoveRotateRoomStartChangingNotification();
            //    break;
            //case NotificationType.HospitalRoomsExpanded:
            //    instanceNL.SubscribeHospitalRoomsExpandedNotification();
            //    break;
            //case NotificationType.GreenDoctorsOfficeAdded:
            //    instanceNL.SubscribeGreenDoctorOfficeAddedNotification();
            //    break;
            //case NotificationType.PharmacyOpened:
            //    instanceNL.SubscribePharmacyOpenedNotification();
            //    break;
            //case NotificationType.PharmacyOffersClicked:
            //    instanceNL.SubscribePharmacyOffersClickedNotification();
            //    break;
            //case NotificationType.PharmacyClosed:
            //    instanceNL.SubscribePharmacyClosedNotification();
            //    break;
            //case NotificationType.ElixirMixerAdded:
            //    instanceNL.SubscribeElixirMixerAddedNotification();
            //    break;
            //case NotificationType.KidsUIOpen:
            //    instanceNL.SubscribeKidsUIOpenNotification();
            //    break;
            //case NotificationType.KidsUIClosed:
            //    instanceNL.SubscribeKidsUIClosedNotification();
            //    break;
            //case NotificationType.KidArrivedToKidsRoom:
            //    instanceNL.SubscribeKidArrivedToKidsRoomNotification();
            //    break;
            //case NotificationType.BlueSyrupProductionStarted:
            //    instanceNL.SubscribeBlueSyrupProductionStartedNotification();
            //    break;
            //case NotificationType.DoctorRewardCollected:
            //    instanceNL.SubscribeDoctorRewardCollectedNotification();
            //    break;
            //case NotificationType.PatientZeroPopUpClosed:
            //    instanceNL.SubscribePatientZeroClosedNotification();
            //    break;
            //case NotificationType.PatientZeroPopUpOpened:
            //    instanceNL.SubscribePatientZeroOpenedNotification();
            //    break;
            //case NotificationType.KidsClicked:
            //    instanceNL.SubscribeKidsClickedNotification();
            //    break;
            //case NotificationType.WiseHospitalLoaded:
            //    instanceNL.SubscribeWiseHospitalLoadedNotification();
            //    break;
            //case NotificationType.VIPTeaseMedicopter:
            //    instanceNL.SubscribeVIPTeaseMedicopterNotification();
            //    break;
            //case NotificationType.XRayAdded:
            //    instanceNL.SubscribeXRayNotificatoin();
            //    break;
            //case NotificationType.BlueSyrupExtractionCompleted:
            //    instanceNL.SubscribeBlueSyrupExtractionCompleted();
            //    break;
            //case NotificationType.BlueSyrupCollected:
            //    instanceNL.SubscribeBlueSyrupCollected();
            //    break;
            //case NotificationType.TreasurePopUpClosed:
            //    instanceNL.SubscribeTreasurePopUpClosed();
            //    break;
            //case NotificationType.DrawerClosed:
            //    instanceNL.SubscribeDrawerClosedNotification();
            //    break;
            //case NotificationType.VIPSpawned:
            //    instanceNL.SubscribeVIPSpawnedNotification();
            //    break;
            //case NotificationType.PackageArrived:
            //    instanceNL.SubscribePackageArrived();
            //    break;
            //case NotificationType.PackageClicked:
            //    instanceNL.SubscribePackageClicked();
            //    break;
            //case NotificationType.PatioElementCleared:
            //    instanceNL.SubscribePatioElementCleared();
            //    break;
            //case NotificationType.VipPopUpOpen:
            //    instanceNL.SubscribeVipPopUpOpen();
            //    break;
            //case NotificationType.BoxPopUpOpen:
            //    instanceNL.SubscribeBoxPopUpClosed();
            //    break;
            //case NotificationType.PatioDecorationsAdded:
            //    instanceNL.SubscribePatioDecorationsAdded();
            //    break;
            //case NotificationType.DiagnosePatientInBed:
            //    instanceNL.SubscribeDiagnosePatientReachedBedNotification();
            //    break;
            //case NotificationType.CloseWelcomePopUp:
            //    instanceNL.SubscribeCloseWelcomePopUp();
            //    break;
            //case NotificationType.NewspaperRewardDiamond:
            //    instanceNL.SubscribeNewspaperRewardDiamond();
            //    break;
            //case NotificationType.NewspaperRewardExp:
            //    instanceNL.SubscribeNewspaperRewardExp();
            //    break;
            //case NotificationType.EpidemyClicked:
            //    instanceNL.SubscribeEpidemyClicked();
            //    break;
            //case NotificationType.TreasureClicked:
            //    instanceNL.SubscribeTreasureClicked();
            //    break;
            //case NotificationType.BubbleBoyClicked:
            //    instanceNL.SubscribeBubbleBoyClicked();
            //    break;
            //case NotificationType.VipFlyByEnd:
            //    instanceNL.SubscribeVipFlyByEnd();
            //    break;
            //case NotificationType.DailyQuestPopUpOpen:
            //    instanceNL.SubscribeDailyQuestPopUpOpen();
            //    break;
            //case NotificationType.DailyQuestPopUpClosed:
            //    instanceNL.SubscribeDailyQuestPopUpClosed();
            //    break;
            //case NotificationType.MicroscopeGoodBacteriaAdded:
            //    instanceNL.SubscribeMicroscopeGoodBacteriaAdded();
            //    break;
            //case NotificationType.MicroscopeClosed:
            //    instanceNL.SubscribeMicroscopeClosed();
            //    break;
            //case NotificationType.OliviaLetterClosed:
            //    instanceNL.SubscribeOliviaLetterClosed();
            //    break;
            //case NotificationType.BacteriaPatientInBed:
            //    instanceNL.SubscribeBacteriaPatientReachedBedNotification();
            //    break;
            //case NotificationType.MaternityWardClicked:
            //    instanceNL.SubscribeMaternityWardObjectClicked();
            //    break;
            //case NotificationType.MaternityWardRenovate:
            //    instanceNL.SubscribeMaternityWardRenovate();
            //    break;
            //case NotificationType.MaternityWardBuildEnd:
            //    instanceNL.SubscribeMaternityWardBuildEnd();
            //    break;
            //case NotificationType.MaternityReadyToBeUnlocked:
            //    instanceNL.SubscribeToUnlockMaternity();
            //    break;
            //case NotificationType.MailBoxPopupClosed:
            //    instanceNL.SubscribeMailBoxPopupClosed();
            //    break;
            //case NotificationType.PackageText:
            //    instanceNL.SubscribePackageText();
            //    break;
            //case NotificationType.PillMakerAdded:
            //    instanceNL.SubscribePillMakerAdded();
            //    break;
            //case NotificationType.ObjectivePanelOpened:
            //    instanceNL.SubscribeObjectivePanelOpened();
            //    break;
            //case NotificationType.ObjectivePanelClosed:
            //    instanceNL.SubscribeObjectivePanelClosed();
            //    break;
            //case NotificationType.PatientSentToXRay:
            //    instanceNL.SubscribePatientSentToXRay();
            //    break;
            //case NotificationType.FirstPlantPlanted:
            //    instanceNL.SubscribeFirstPlantPlanted();
            //    break;
            //case NotificationType.NotEnoughPanacea:
            //    instanceNL.SubscribeNotEnoughPanaceaNotification();
            //    break;
            //case NotificationType.PanaceaCollectorUpgraded:
            //    instanceNL.SubscribePanaceaCollectorUpgraded();
            //    break;
            //case NotificationType.VipSpeedup0Closed:
            //    instanceNL.SubscribeVipSpeedup0Closed();
            //    instanceNL.SubscribeSkipVipSpeedupTutorial();
            //    break;
            //case NotificationType.VipSpeedupPopupOpened:
            //    instanceNL.SubscribeVipSpeedupPopupOpened();
            //    instanceNL.SubscribeSkipVipSpeedupTutorial();
            //    break;
            //case NotificationType.VipSpeedupPopupClosed:
            //    instanceNL.SubscribeVipSpeedupPopupClosed();
            //    instanceNL.SubscribeSkipVipSpeedupTutorial();
            //    break;
            default:
                break;
        }
    }

    public override void SubscribeToStepCondition(Condition condition)
    {
        //Debug.Log("Przypisanie warunku do sprawdzenia. Warunek enum: " + condition);
        switch (condition)
        {
            //case Condition.NoCondition:
            //    break;
            //case Condition.HospitalNamed:
            //    instanceNL.SubscribeHospitalNamedNotification();
            //    break;
            //case Condition.SetCurrentlyPointedMachine:
            //    instanceNL.SubscribeSetCurrentlyPointedMachineNotification();
            //    break;
            //case Condition.SetTutorialArrow:
            //    UIController.get.reportPopup.canBeOpen = false;
            //    instanceNL.SubscribeTutorialArrowSetNotification();
            //    break;
            //case Condition.DrawerOpened:
            //    instanceNL.SubscribeDrawerOpenedNotification(true);
            //    break;
            //case Condition.FinishedBuilding:
            //    instanceNL.SubscribeFinishedBuildingNotification();
            //    break;
            //case Condition.Level1ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(1);
            //    break;
            //case Condition.Level2ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(2);
            //    break;
            //case Condition.Level3ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(3);
            //    break;
            //case Condition.Level4ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(4);
            //    break;
            //case Condition.Level5ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(5);
            //    break;
            //case Condition.Level6ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(6);
            //    break;
            //case Condition.Level7ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(7);
            //    break;
            //case Condition.Level8ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(8);
            //    break;
            //case Condition.Level9ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(9);
            //    break;
            //case Condition.Level10ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(10);
            //    break;
            //case Condition.Level12ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(12);
            //    break;
            //case Condition.Level14ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(14);
            //    break;
            //case Condition.Level15ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(15);
            //    break;
            //case Condition.Level16ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(16);
            //    break;
            //case Condition.Level17ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(17);
            //    break;
            //case Condition.Level18ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(18);
            //    break;
            //case Condition.Level19ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(19);
            //    break;
            //case Condition.Level20ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(20);
            //    break;
            //case Condition.Level23ReachedAndClosed:
            //    instanceNL.SubscribeLevelReachedAndClosedNotification(23);
            //    break;
            //case Condition.FirstPatientArriving:
            //    instanceNL.SubscribeFirstPatientArrivingNotification();
            //    break;
            //case Condition.PatientCardIsOpen:
            //    instanceNL.SubscribePatientCardIsOpenNotification();
            //    break;
            //case Condition.PatientCardIsClosed:
            //    instanceNL.SubscribePatientCardIsClosedNotification();
            //    break;
            //case Condition.RuntimeStepInfoChanged:
            //    instanceNL.SubscribeRunTimeStepInfoChangedNotification();
            //    break;
            //case Condition.MoveRotateRoomEnd:
            //    instanceNL.SubscribeMoveRotateRoomEndNotification();
            //    break;
            //case Condition.ObjectDoesNotExistOnLevel:
            //    instanceNL.SubscribeObjectExistOnLeveldNotification();
            //    break;
            //case Condition.KidsRoomUnlocked:
            //    instanceNL.SubscribeKidsRoomUnlockedNotification();
            //    break;
            //case Condition.KidPatientSpawned:
            //    instanceNL.SubscribeKidPatientSpawnedNotification();
            //    break;
            //case Condition.FirstEmergencyPatientSpawned:
            //    instanceNL.SubscribeFirstEmergencyPatientSpawnedNotification();
            //    break;
            //case Condition.SpawnFirstPatient:
            //    instanceNL.SubscribeSpawnFirstPatientNotification();
            //    break;
            //case Condition.FirstPatientNearSignOfHospital:
            //    instanceNL.SubscribeFirstPatientNearSignOfHospitalNotification();
            //    break;
            //case Condition.ExpandConditions:
            //    instanceNL.SubscribeExpandConditionsNotification();
            //    break;
            //case Condition.ExpAmountChanged:
            //    instanceNL.SubscribeExpAmountChangedNotification();
            //    break;
            //case Condition.NotEnoughPanacea:
            //    instanceNL.SubscribeNotEnoughPanaceaNotification();
            //    break;
            //case Condition.FollowBob:
            //    instanceNL.SubscribeFollowBobNotification();
            //    break;
            //case Condition.MedicopterTookOff:
            //    instanceNL.SubscribeMedicopterTookOffNotification();
            //    break;
            //case Condition.VIPLeoReachedPosition:
            //    instanceNL.SubscribeCharacterReachedDestinationNotification();
            //    break;
            //case Condition.TreasurePopUpOpened:
            //    instanceNL.SubscribeTreasurePopUpOpened();
            //    break;
            //case Condition.VIPMedicopterStarted:
            //    if (currentTutorialStep.ForceClosePopups)
            //        UIController.get.ExitAllPopUps(true);
            //    instanceNL.SubscribeVIPMedicopterStartedNotification();
            //    break;
            //case Condition.VIPReachedBed:
            //    instanceNL.SubscribeVIPReachedBedNotification();
            //    break;
            //case Condition.VIPNotCured:
            //    instanceNL.SubscribeVIPNotCuredNotification();
            //    break;
            //case Condition.HomeHospitalLoaded:
            //    instanceNL.SubscribeHomeHospitalLoaded();
            //    break;
            //case Condition.PackageCollected:
            //    instanceNL.SubscribePackageCollected();
            //    break;
            //case Condition.WisePharmacyVisited:
            //    instanceNL.SubscribeWisePharmacyVisited();
            //    break;
            //case Condition.BoostersPopUpOpen:
            //    instanceNL.SubscribeBoostersPopUpOpen();
            //    break;
            //case Condition.GiftReady:
            //    instanceNL.SubscribeGiftReady();
            //    break;
            //case Condition.OpenWelcomePopUp:
            //    instanceNL.SubscribeOpenWelcomePopUp();
            //    break;
            //case Condition.EpidemyStarting:
            //    instanceNL.SubscribeEpidemyStarting();
            //    break;
            //case Condition.EpidemyCompleted:
            //    instanceNL.SubscribeEpidemyCompleted();
            //    break;
            //case Condition.TwentyProbeTables:
            //    instanceNL.SubscribeTwentyProbeTables();
            //    break;
            //case Condition.SecondDiagnosticMachineOpen:
            //    instanceNL.SubscribeSecondDiagnosticMachineOpen();
            //    break;
            //case Condition.TenPatioDecorations:
            //    instanceNL.SubscribeTenPatioDecorations();
            //    break;
            //case Condition.ExpAmountChangedNonLinear:
            //    instanceNL.SubscribeExpAmountChangedNonLinear();
            //    break;
            //case Condition.Level11ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(11);
            //    break;
            //case Condition.Level13ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(13);
            //    break;
            //case Condition.Level16ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(16);
            //    break;
            //case Condition.Level22ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(22);
            //    break;
            //case Condition.Level20ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(20);
            //    break;
            //case Condition.Level30ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(30);
            //    break;
            //case Condition.Level42ReachedAndClosedNonLinear:
            //    instanceNL.SubscribeLevelReachedAndClosedNonLinear(42);
            //    break;
            //case Condition.Level10NewspaperClosed:
            //    instanceNL.SubscribeLevel10NewspaperClosedCond();
            //    break;
            //case Condition.Level10Wise1Closed:
            //    instanceNL.SubscribeLevel10WiseClosedCond();
            //    break;
            //case Condition.TreasureCollected:
            //    instanceNL.SubscribeTreasureCollected();
            //    break;
            //case Condition.BubbleBoyAvailable:
            //    instanceNL.SubscribeBubbleBoyAvailable();
            //    break;
            //case Condition.VipFlyByStart:
            //    instanceNL.SubscribeVipFlyByStart();
            //    break;
            //case Condition.PushNotificationsDisabled:
            //    instanceNL.SubscribePushNotificationsDisabled();
            //    break;
            //case Condition.ShowDailyQuestAnimation:
            //    instanceNL.SubscribeShowDailyQuestAnimation();
            //    break;
            //case Condition.MicroscopeShow:
            //    instanceNL.SubscribeMicroscopeShow();
            //    break;
            //case Condition.LevelGoalsActive:
            //    instanceNL.SubscribeLevelGoalsActive();
            //    break;
            //case Condition.MaternityReadyToBeUnlocked:
            //    instanceNL.SubscribeToUnlockMaternity();
            //    break;
            //case Condition.VitaminesMakerEmma1Closed:
            //    instanceNL.SubscribeVitaminesMakerEmma1ClosedCond();
            //    break;
            //case Condition.OnFirstVitaminesMakerPopupOpened:
            //    instanceNL.SubscribeFirstVitaminesMakerPopupOpened();
            //    break;
            //case Condition.EpidemyCenterBuilt:
            //    instanceNL.SubscribeEpidemyCenterBuilt();
            //    break;
            //case Condition.CuredVipCountIsEnough:
            //    instanceNL.SubscribeCuredVipCountIsEnough();
            //    break;
            default:
                break;
        }
    }
#if UNITY_EDITOR
    protected override string GetGUIDName()
    {
        return "TutorialStep_";
    }
#endif
}