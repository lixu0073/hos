using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleUI;
using IsoEngine;
using Hospital;
using System.Collections.Generic;
using System;
using Maternity;
using Maternity.PatientStates;

public class NotificationListener : MonoBehaviour
{
    private static NotificationListener instance = null;

    public static NotificationListener Instance
    {
        get { return instance; }
    }
    //Not used.
    //public TutorialUIController instaceTUI;

    private int AmountOfBlueElixirsToCollect = 6;
    private int AmountOfBlueElixirsToSeed = 6;

    //private bool tapAnywhere = false;
    private int levelRequired;
    private Coroutine probeTableToolCoroutine;
    private Coroutine waitForPatientSpawn;
    private Coroutine invokeCloseUI;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        NotificationCenter instanceNC = NotificationCenter.Instance;
    }

    void Update()
    {
        //if (tapAnywhere)
        //{
        //    if (Input.GetKeyUp(KeyCode.Mouse0))
        //    {
        //        if (UIController.getHospital.PatientCard.gameObject.activeSelf && Input.mousePosition.normalized.y < .35f && TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.bacteria_george_2))    //last one is so in the bacteria tutorial there's no tap area restriction
        //            NotificationCenter.Instance.TapAnywhere.Invoke(new TapAnywhereEventArgs());
        //        else if (!UIController.getHospital.PatientCard.gameObject.activeSelf || TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.bacteria_george_2))
        //            NotificationCenter.Instance.TapAnywhere.Invoke(new TapAnywhereEventArgs());
        //    }
        //}
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    private void TutorialBegining_Notification(TutorialBeginingEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - ropoczęcie tutorialu.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TutorialBegining.Notification -= TutorialBegining_Notification;
    }

    public void SubscribeTutorialBeginingNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TutorialBegining.Notification += TutorialBegining_Notification;
    }

    private void StepInfoClose_Notification(StepInfoCloseEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zamknięcie popupu z animacją tutorialową po kliku.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.StepInfoClose.Notification -= StepInfoClose_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeStepInfoCloseNotification()
    {
        //Debug.Log("Subskrypcja notyfikacji zamkniecie animacji tutorialowej");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.StepInfoClose.Notification += StepInfoClose_Notification;
    }

    private void SetCurrentlyPointedMachine_Notification(SetCurrentlyPointedMachineEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - przypisanie maszyny.");
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.SetCurrentlyPointedMachine.Notification -= SetCurrentlyPointedMachine_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeSetCurrentlyPointedMachineNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.SetCurrentlyPointedMachine.Notification += SetCurrentlyPointedMachine_Notification;
    }

    GameObject patientWithDiagnosis = null;
    GameObject patientWithBacteria = null;

    private void TutorialArrowSet_Notification(TutorialArrowSetEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialController tc = TutorialController.Instance;
        HospitalTutorialController htc = HospitalTutorialController.HospitalTutorialInstance;

        if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            return;
        }
        //Debug.LogError("TutorialArrowSet_Notification " + tc.CurrentTutorialStepTag);
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - wejście w ustawianie strzałki." );

        if (tc.GetCurrentStepData().NotificationType == NotificationType.MaternityWardClicked)
        {
            TutorialUIController.Instance.ShowIndictator(
                HospitalAreasMapController.HospitalMap.maternityWard.gameObject, new Vector3(2.54f, 0, 5.5f));
        }
        else if (tc.GetCurrentStepData().NotificationType == NotificationType.KidsUIOpen)
        {
            TutorialUIController.Instance.ShowIndictator(HospitalAreasMapController.HospitalMap.playgroud.gameObject,
                Vector3.zero);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.patient_card_open)
        {
            TutorialUIController.Instance.ShowIndictator(eventArgs.rObj);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.diagnose_open_patient_card)
        {
            //Debug.LogError("StepTag.diagnose_open_patient_card");
            if (eventArgs.gObj != null)
            {
                TutorialUIController.Instance.ShowIndictator(eventArgs.gObj.transform.GetChild(0).gameObject,
                    Vector3.zero,
                    true); //child(0) is the animator of the patient which scales to -1 in some rotations which caused the arrow to be in wrong place in 2/4 room rotations
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(eventArgs.gObj.transform.position, 1f, true);
                ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7, 1, false);
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.bacteria_george_1)
        {
            if (eventArgs.gObj != null)
            {
                TutorialUIController.Instance.ShowIndictator(eventArgs.gObj.transform.GetChild(0).gameObject,
                    Vector3.zero,
                    true); //child(0) is the animator of the patient which scales to -1 in some rotations which caused the arrow to be in wrong place in 2/4 room rotations
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(eventArgs.gObj.transform.position, 1f, true);
                ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7, 1, false);
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.bacteria_george_2)
        {
            if (eventArgs.gObj != null)
            {
                TutorialUIController.Instance.ShowIndictator(eventArgs.gObj.transform.GetChild(0).gameObject,
                    Vector3.zero,
                    true); //child(0) is the animator of the patient which scales to -1 in some rotations which caused the arrow to be in wrong place in 2/4 room rotations
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(eventArgs.gObj.transform.position, 1f, true);
                ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7, 1, false);
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.package_click)
        {
            TutorialUIController.Instance.ShowIndictator(tc.GetCurrentStepData().CameraTargetVectorPosition);
        }
        else if (tc.GetCurrentStepData().TargetMachineTag == "Reception")
        {
            HospitalAreasMapController.HospitalMap.reception.IsClickable = true;
            TutorialUIController.Instance.ShowIndictator(HospitalAreasMapController.HospitalMap.reception.gameObject,
                Vector3.zero);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.lab_intro_big)
        {
            //Debug.Log("USUNIECIE PLACHTY z probe tables");
            CoverController cover = (CoverController)eventArgs.rObj;
            if (cover != null)
                cover.RemoveCover();
            TutorialUIController.Instance.ShowIndictator(tc.GetAllFullProbeTables()[0]);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.expand_arrow)
        {
            foreach (var tmp in HospitalAreasMapController.HospitalMap.areas)
            {
                if (tmp.Key == HospitalArea.Clinic)
                {
                    if (tmp.Value.GetAreaToBuy(54) == null)
                    {
                        //Debug.LogError("Area doesnt exist or is already unlocked!");
                        tc.IncrementCurrentStep();
                        break;
                    }

                    Vector3 offset = new Vector3(-2.5f, 0, .5f);
                    Debug.LogError(tmp.Value.GetAreaToBuy(54).GetRectPos());
                    TutorialUIController.Instance.ShowIndictator(tmp.Value.GetAreaToBuy(54).GetRectPos() + offset);
                    break;
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.first_yellow_patient_popup_open)
        {
            var yellowDoc = HospitalAreasMapController.HospitalMap.FindRotatableObject("YellowDoc");
            if (yellowDoc)
            {
                if (yellowDoc.GetState() == RotatableObject.State.working ||
                    yellowDoc.GetState() == RotatableObject.State.waitingForUser ||
                    yellowDoc.GetState() == RotatableObject.State.building)
                {
                    instanceNC.TutorialArrowSet.Notification -= TutorialArrowSet_Notification;
                    Debug.LogError("Skipping");
                    tc.IncrementCurrentStep();
                    return;
                }
            }

            //set arrow above patient in reception
            GameObject target = null;

            ClinicCharacterInfo[] clinicPatients = FindObjectsOfType<ClinicCharacterInfo>();
            for (int i = 0; i < clinicPatients.Length; ++i)
            {
                if (clinicPatients[i].clinicDisease != null)
                {
                    int diseaseId = clinicPatients[i].clinicDisease.id;
                    //Debug.Log("diseaseId = " + diseaseId);
                    if (diseaseId == 1 /* && eventArgs.patientState is ClinicPatientAI.WaitingInReceptionState*/)
                    {
                        //mild tummy illness for yellow doctor
                        //Debug.LogError("Found patient with yellow illness! His name is: " + clinicPatients[i].Name);
                        target = clinicPatients[i].gameObject;
                        break;
                    }
                }
                //Debug.LogError("YELLOW PATIENT NOT FOUND!");
            }

            if (target != null)
            {
                //Debug.LogError("Showing inicator on guy");
                TutorialUIController.Instance.ShowIndictator(target, Vector3.zero, true);
            }
            else
            {
                //Debug.LogError("Returning");
                return;
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.first_green_patient_popup_open)
        {
            var greenDoc = HospitalAreasMapController.HospitalMap.FindRotatableObject("GreenDoc");
            if (greenDoc)
            {
                if (greenDoc.GetState() == RotatableObject.State.working ||
                    greenDoc.GetState() == RotatableObject.State.waitingForUser ||
                    greenDoc.GetState() == RotatableObject.State.building)
                {
                    instanceNC.TutorialArrowSet.Notification -= TutorialArrowSet_Notification;
                    tc.IncrementCurrentStep();
                    return;
                }
            }

            //set arrow above patient in reception
            GameObject target = null;

            ClinicCharacterInfo[] clinicPatients = FindObjectsOfType<ClinicCharacterInfo>();
            for (int i = 0; i < clinicPatients.Length; ++i)
            {
                if (clinicPatients[i].clinicDisease != null)
                {
                    int diseaseId = clinicPatients[i].clinicDisease.id;
                    ////Debug.Log("diseaseId = " + diseaseId);
                    if (diseaseId == 2 /* && eventArgs.patientState is ClinicPatientAI.WaitingInReceptionState*/)
                    {
                        //Appetite Disorder for green doctor
                        ////Debug.LogError("Found patient with yellow illness! His name is: " + clinicPatients[i].Name);
                        target = clinicPatients[i].gameObject;
                        break;
                    }
                }
                // Debug.LogError("GREEN PATIENT NOT FOUND! ");
            }

            if (target != null)
                TutorialUIController.Instance.ShowIndictator(target, Vector3.zero, true);
            else
                return;
            //     //Debug.LogError("Patient not found");
        }
        else if (tc.CurrentTutorialStepTag == StepTag.kids_arrow)
        {
            htc.KidsClickArea.SetActive(true);
            TutorialUIController.Instance.ShowIndictator(htc.KidsClickArea, Vector3.zero, false, true);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.epidemy_openpopup)
        {
            TutorialUIController.Instance.ShowIndictator(ReferenceHolder.GetHospital().Epidemy.gameObject, Vector3.zero,
                false, false);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.treasure_arrow)
        {
            GameObject treasure = HospitalAreasMapController.HospitalMap.treasureManager.GetTreasureObject();
            if (!treasure.activeSelf)
                HospitalAreasMapController.HospitalMap.treasureManager.ShowTreasureFromTutorial();

            TutorialUIController.Instance.ShowIndictator(treasure.transform.position);

            UIController.get.ExitAllPopUps();
            UIController.get.CloseActiveHover();
            //
            Vector2i pos2i = new Vector2i((int)treasure.transform.position.x, (int)treasure.transform.position.z);
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(pos2i, 1.5f, true);
            ReferenceHolder.Get().engine.MainCamera.SmoothZoom(6, 1.5f, false);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.bubble_boy_arrow)
        {
            //GameObject treasure = ReferenceHolder.Get().bubb;
            //if (!treasure.activeSelf)
            //    HospitalAreasMapController.Map.treasureManager.ShowTreasureFromTutorial();
            //Debug.LogError("bubble_boy_arrow");
            TutorialUIController.Instance.ShowIndictator(new Vector3(21.5f, 0f, 36.5f));
        }
        else if (tc.CurrentTutorialStepTag == StepTag.maternity_waiting_room_finish ||
                 tc.CurrentTutorialStepTag == StepTag.maternity_labor_room_finish)
        {
            StartCoroutine(DeleyedArrow(eventArgs.rObj));
        }
        else if (tc.CurrentTutorialStepTag == StepTag.arrange_text_before)
        {
            TutorialUIController.Instance.ShowIndictator(eventArgs.rObj.gameObject,
                TutorialUIController.OnMapPointerTreatmentRoomOffset, false, false,
                TutorialUIController.TutorialPointerAnimationType.tap_hold_arrow);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.follow_ambulance)
        {
            TutorialUIController.Instance.ShowTutorialArrowUI(
                HospitalUIController.get.ObjectivesPanelUI.GetComponent<RectTransform>(), Vector2.zero);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.positive_energy_text)
        {
            TutorialUIController.Instance.ShowIndictator(eventArgs.rObj);
            //.gameObject, TutorialUIController.OnMapPointerTreatmentRoomOffset, false);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.garden_text)
        {
            TutorialUIController.Instance.ShowIndictator(new Vector3(69.2f, 0f, 45f));
            //.gameObject, TutorialUIController.OnMapPointerTreatmentRoomOffset, false);
        }
        else if (tc.CurrentTutorialStepTag == StepTag.vip_sign_tap)
        {
            TutorialUIController.Instance.ShowIndictator(new Vector3(14.5f, 0f, 45f));
        }
        else if (tc.CurrentTutorialStepTag == StepTag.vip_speedup_1)
        {
            TutorialUIController.Instance.ShowIndictator(new Vector3(14f, -0.5f, 51.5f));
        }
        else
        {
            //Rotatable search to hook arrow(Translated from Polish)
            if (tc.CurrentTutorialStepTag != StepTag.build_doctor_finish)
            {
                TutorialUIController.Instance.ShowIndictator(eventArgs.rObj);
            }
        }

        if (tc.CurrentTutorialStepTag == StepTag.maternity_waiting_room_finish ||
            tc.CurrentTutorialStepTag == StepTag.maternity_waiting_room_unpack)
        {
            StartCoroutine(DeleyedWaitingRoomLock());
        }

        //Debug.LogError("-= TutorialArrowSet_Notification;");
        instanceNC.TutorialArrowSet.Notification -= TutorialArrowSet_Notification;
        tc.ConfirmConditionExecution();
        tc.SetCurrentStep();
    }

    private IEnumerator DeleyedWaitingRoomLock()
    {
        yield return new WaitForEndOfFrame();
        Debug.LogError("OLD DRAWER CODE!");
        //((MaternityShopDrawer)UIController.get.drawer).TutorialChangeRoomStatus(false);
    }

    private IEnumerator DeleyedArrow(RotatableObject room)
    {
        yield return new WaitForSeconds(0.1f);
        BuildingHover hover = FindObjectOfType<BuildingHover>();
        if (hover == null)
            TutorialUIController.Instance.ShowIndictator(room);
    }

    //for all objects
    public void SubscribeTutorialArrowSetNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;

        //Debug.LogError("+= TutorialArrowSet_Notification;");
        instanceNC.TutorialArrowSet.Notification += TutorialArrowSet_Notification;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_yellow_patient_popup_open)
        {
            StartCoroutine(DelayedArrowSet(0));
            //StartCoroutine(DelayedArrowSet(8.5f));
            return;
        }

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_green_patient_popup_open)
        {
            StartCoroutine(DelayedArrowSet(0));
            //StartCoroutine(DelayedArrowSet(8.5f));
            return;
        }

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_open)
        {
            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Exists(x =>
                    x._BedStatus == HospitalBedController.HospitalBed.BedStatus.OccupiedBed))
            {
                NotificationCenter.Instance.TutorialArrowSet.Invoke(
                    new TutorialArrowSetEventArgs(TutorialController.Instance.FindObjectForStep()));
                StartCoroutine(DelayedArrowSet(0));
            }
            else
                return;
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.diagnose_open_patient_card)
        {
            //Debug.LogError("TutorialArrowSet patientWithDiagnosis = " + patientWithDiagnosis);
            List<HospitalCharacterInfo> patientsList = new List<HospitalCharacterInfo>();
            HospitalBedController bedController = HospitalAreasMapController.HospitalMap.hospitalBedController;
            for (int i = 0; i < bedController.Beds.Count; ++i)
            {
                if (bedController.Beds[i].Patient != null)
                    patientsList.Add(bedController.Beds[i].Patient.GetHospitalCharacterInfo());
            }

            for (int i = 0; i < patientsList.Count; ++i)
            {
                if (patientsList[i].RequiresDiagnosis)
                {
                    //Debug.LogError("Found a patient with a disease for this diagnostic room! Disease Type: " + patientsList[i].DisaseDiagnoseType + " patient index = " + i);
                    patientWithDiagnosis = patientsList[i].gameObject;
                    break;
                }
            }

            if (patientWithDiagnosis != null)
            {
                //this will also be invoked by the first patient with diagnosis who Lays in bed.
                NotificationCenter.Instance.TutorialArrowSet.Invoke(
                    new TutorialArrowSetEventArgs(patientWithDiagnosis));
            }

            return;
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_2)
        {
            List<HospitalCharacterInfo> patientsList = new List<HospitalCharacterInfo>();
            HospitalBedController bedController = HospitalAreasMapController.HospitalMap.hospitalBedController;
            for (int i = 0; i < bedController.Beds.Count; ++i)
            {
                if (bedController.Beds[i].Patient != null)
                    patientsList.Add(bedController.Beds[i].Patient.GetHospitalCharacterInfo());
            }

            for (int i = 0; i < patientsList.Count; ++i)
            {
                if (patientsList[i].HasBacteria && !patientsList[i].IsVIP)
                {
                    patientWithBacteria = patientsList[i].gameObject;
                    break;
                }
            }

            if (patientWithBacteria != null)
            {
                //this will also be invoked by the first patient with bacteria who Lays in bed.
                NotificationCenter.Instance.TutorialArrowSet.Invoke(new TutorialArrowSetEventArgs(patientWithBacteria));
            }

            return;
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_lab_unpack)
        {
            var syrupLab = HospitalAreasMapController.HospitalMap.FindRotatableObject("SyrupLab");

            if (syrupLab.GetState() == RotatableObject.State.working)
            {
                instanceNC.TutorialArrowSet.Notification -= TutorialArrowSet_Notification;
                TutorialController.Instance.IncrementCurrentStep();
                return;
            }

            NotificationCenter.Instance.TutorialArrowSet.Invoke(
                new TutorialArrowSetEventArgs(TutorialController.Instance.FindObjectForStep()));
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.vip_speedup_1)
        {
            NotificationCenter.Instance.TutorialArrowSet.Invoke(
                new TutorialArrowSetEventArgs(ReferenceHolder.GetHospital().vipSystemManager.gameObject));
        }
        else
            NotificationCenter.Instance.TutorialArrowSet.Invoke(
                new TutorialArrowSetEventArgs(TutorialController.Instance.FindObjectForStep()));
    }

    public void SubscribeVitaminesMakerEmma1ClosedCond()
    {
        NotificationCenter.Instance.VitaminesMakerEmma1ClosedCond.Notification +=
            VitaminesMakerEmma1ClosedCond_Notification;
    }

    private void VitaminesMakerEmma1ClosedCond_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.Level10WiseClosedCond.Notification -= VitaminesMakerEmma1ClosedCond_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vitamines_maker_emma_2);
        tc.SetNonLinearStep(StepTag.NL_vitamines_maker_emma_2);
    }

    IEnumerator DelayedArrowSet(float delay)
    {
        yield return new WaitForSeconds(delay);
        NotificationCenter.Instance.TutorialArrowSet.Invoke(
            new TutorialArrowSetEventArgs(TutorialController.Instance.FindObjectForStep()));
    }

    public void SubscribeFirstVitaminesMakerPopupOpened()
    {
        NotificationCenter.Instance.VitaminMakerPopupOpen.Notification += VitaminMakerPopupOpen_Notification;
    }

    private void VitaminMakerPopupOpen_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VitaminMakerPopupOpen.Notification -= VitaminMakerPopupOpen_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vitamines_maker_emma_1);
        tc.SetNonLinearStep(StepTag.NL_vitamines_maker_emma_1);
    }

    public void SubscribeCuredVipCountIsEnough()
    {
        NotificationCenter.Instance.CuredVipCountIsEnough.Notification -= CuredVipCountIsEnough_Notification;
        NotificationCenter.Instance.CuredVipCountIsEnough.Notification += CuredVipCountIsEnough_Notification;
    }

    private void CuredVipCountIsEnough_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.CuredVipCountIsEnough.Notification -= CuredVipCountIsEnough_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vip_upgrade_1);
        tc.SetNonLinearStep(StepTag.NL_vip_upgrade_1);
        tc = null;
    }

    public void SubscribeVipUpgradeTutorial1Closed()
    {
        NotificationCenter.Instance.VipUpgradeTutorial1Closed.Notification -= VipUpgradeTutorial1Closed_Notification;
        NotificationCenter.Instance.VipUpgradeTutorial1Closed.Notification += VipUpgradeTutorial1Closed_Notification;
        ReferenceHolder.GetHospital().vipSystemManager.SubscribeShowTutorialArrows();
    }

    private void VipUpgradeTutorial1Closed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VipUpgradeTutorial1Closed.Notification -= VipUpgradeTutorial1Closed_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vip_upgrade_2);
        tc.SetNonLinearStep(StepTag.NL_vip_upgrade_2);
        tc = null;
    }

    public void SubscribeVipUpgradeTutorial2Closed()
    {
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Notification -= VipUpgradeTutorial2Closed_Notification;
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Notification += VipUpgradeTutorial2Closed_Notification;
        ReferenceHolder.GetHospital().vipSystemManager.SubscribeHideTutorialArrows();
    }

    private void VipUpgradeTutorial2Closed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VipUpgradeTutorial2Closed.Notification -= VipUpgradeTutorial2Closed_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vip_upgrade_3);
        tc.SetNonLinearStep(StepTag.NL_vip_upgrade_3);
        tc = null;
    }

    public void SubscribeVipUpgradeTutorial3Closed()
    {
        NotificationCenter.Instance.VipUpgradeTutorial3Closed.Notification -= VipUpgradeTutorial3Closed_Notification;
        NotificationCenter.Instance.VipUpgradeTutorial3Closed.Notification += VipUpgradeTutorial3Closed_Notification;
    }

    private void VipUpgradeTutorial3Closed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VipUpgradeTutorial3Closed.Notification -= VipUpgradeTutorial3Closed_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vip_upgrade_4);
        tc.SetNonLinearStep(StepTag.NL_vip_upgrade_4);
        tc = null;
    }

    public void SubscribeVipUpgradeTutorial4Closed()
    {
        NotificationCenter.Instance.VipUpgradeTutorial4Closed.Notification -= VipUpgradeTutorial4Closed_Notification;
        NotificationCenter.Instance.VipUpgradeTutorial4Closed.Notification += VipUpgradeTutorial4Closed_Notification;
        TutorialUIController.Instance.ShowArrowOnInactiveVipUpgradeBookmark();
    }

    private void VipUpgradeTutorial4Closed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VipUpgradeTutorial4Closed.Notification -= VipUpgradeTutorial4Closed_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_vip_upgrade_5);
        tc.SetNonLinearStep(StepTag.NL_vip_upgrade_5);
        tc = null;
    }

    public void SubscribeVipSpeedup0Closed()
    {
        NotificationCenter.Instance.VipSpeedup0Closed.Notification -= VipSpeedup0Closed_Notification;
        NotificationCenter.Instance.VipSpeedup0Closed.Notification += VipSpeedup0Closed_Notification;
    }

    private void VipSpeedup0Closed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VipSpeedup0Closed.Notification -= VipSpeedup0Closed_Notification;
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification -= SkipVipSpeedupTutorial_Notification;

        TutorialController tc = TutorialController.Instance;
        ReferenceHolder.GetHospital().vipSystemManager.DepartVIP();
        tc.IncrementCurrentStep();
        tc = null;
    }

    public void SubscribeSkipVipSpeedupTutorial()
    {
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification -= SkipVipSpeedupTutorial_Notification;
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification += SkipVipSpeedupTutorial_Notification;
    }

    private void SkipVipSpeedupTutorial_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification -= VipSpeedup0Closed_Notification;
        NotificationCenter.Instance.PackageCollected.Notification -= VIPGiftPopupClosed;
        NotificationCenter.Instance.VipSpeedup0Closed.Notification -= VipSpeedup0Closed_Notification;
        NotificationCenter.Instance.VipSpeedupPopupOpened.Notification -= VipSpeedupPopupOpen_Notification;
        NotificationCenter.Instance.VipSpeedupPopupClosed.Notification -= VipSpeedupPopupClosed_Notification;

        TutorialController tc = TutorialController.Instance;
        //if (TutorialUIController.Instance.currentStep.CloseAfterClick)
        //    TutorialUIController.Instance.TutorialPopupsAC.SetBool(AnimHash.FullscreenCharacterVisible, false);
        tc.SetStep(StepTag.level_10);
        tc = null;
    }

    void PharmacyInvokedArrow()
    {
        TutorialUIController.Instance.ShowIndictator(new Vector3(12.48f, 0.1359f, 40.3f));
    }

    void MaternityWardInvokedArrow()
    {
        TutorialUIController.Instance.ShowIndictator(new Vector3(48.46003f, 0, 11.50212f));
    }

    //dla obiektow statycznych jak recepcja
    public void SubscribeTutorialArrowSetNotification(GameObject g)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TutorialArrowSet.Notification += TutorialArrowSet_Notification;
        NotificationCenter.Instance.TutorialArrowSet.Invoke(new TutorialArrowSetEventArgs(g));
    }

    public void LevelUp_Notification(LevelUpEventArgs eventArgs)
    {
        //Debug.Log("LevelUp_Notification. Checking tutorial exceptions. Current Step: " + TutorialController.Instance.CurrentTutorialStepTag);
        bool isTuotrialOnProgrses = TutorialProgressChecker.GetInstance().IsTutorialMatchLevel(eventArgs.newLevel);
        //Debug.LogError("isTuotrialOnProgrses = " + isTuotrialOnProgrses);
        if (!isTuotrialOnProgrses)
        {
            TutorialProgressChecker.GetInstance().MatchTutorialStepToLevel(eventArgs.newLevel);
            if (eventArgs.newLevel == 7)
            {
                TutorialUIController.Instance.BlinkFriendsButton(false);
            }
            else if (eventArgs.newLevel == 2)
            {
                CoverController cover =
                    (CoverController)HospitalAreasMapController.HospitalMap.FindRotatableObject("ProbTabCover");
                if (cover != null)
                    cover.RemoveCover();
            }
        }

        if (eventArgs.newLevel == 3)
        {
            TutorialController tc = TutorialController.Instance;
            switch (tc.CurrentTutorialStepTag)
            {
                case StepTag.elixir_deliver_again:
                    //Debug.LogWarning("SKIPPING STEPS ON PURPOSE!");
                    tc.SetStep(StepTag.follow_ambulance);
                    break;
                case StepTag.doctor_speed_up_again:
                    //Debug.LogWarning("SKIPPING STEPS ON PURPOSE!");
                    tc.SetStep(StepTag.follow_ambulance);
                    break;
                case StepTag.doctor_reward_again:
                    //Debug.LogWarning("SKIPPING STEPS ON PURPOSE!");
                    tc.SetStep(StepTag.follow_ambulance);
                    break;
                default:
                    break;
            }
        }

        TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir |= eventArgs.newLevel == 6;
    }

    public void DrawerUpdate_Notification(DrawerUpdateEventArgs eventArgs)
    {
        //Debug.LogWarning("DrawerUpdate_Notification");

        //RefactoredDrawerController drawer = UIController.get.drawer;

        //if (drawer != null)
        //    drawer.UpdateAllItems();
    }

    public void SubscribeLevelUpNotification()
    {
        NotificationCenter.Instance.LevelUp.Notification += LevelUp_Notification;
    }

    public void SubscribeDrawerUpdateNotification()
    {
        //Debug.LogWarning("Subsktypcja otwarci drawera");
        NotificationCenter.Instance.DrawerUpdate.Notification += DrawerUpdate_Notification;
    }

    private void CharacterReachedDestination_Notification(CharacterReachedDestinationArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.CharacterReachedDestination.Notification -= CharacterReachedDestination_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeCharacterReachedDestinationNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.CharacterReachedDestination.Notification += CharacterReachedDestination_Notification;
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.vip_tease_leo1)
            HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().SpawnVipTease();
        /*if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.Vip_Leo_Sick_1)
        {

        }*/
    }

    #region Maternity

    // run condition - maternity_ready_to_be_unlocked
    public void SubscribeToUnlockMaternity()
    {
        if (!DefaultConfigurationProvider.GetConfigCData().IsMaternityWardFeatureEnabled())
            return;
        if (GameState.Get().hospitalLevel >= HospitalAreasMapController.HospitalMap.maternityWard.Info.UnlockLvl)
        {
            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController.Instance.SetCurrentStep();
        }
        else
        {
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.LevelReachedAndClosed.Notification += LevelReachedAndClosed_ForUnlockMaternity_Notification;
        }
    }

    // run condition - maternity_ready_to_be_unlocked
    private void LevelReachedAndClosed_ForUnlockMaternity_Notification(LevelReachedAndClosedEventArgs eventArgs)
    {
        if (eventArgs.level >= HospitalAreasMapController.HospitalMap.maternityWard.Info.UnlockLvl)
        {
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.LevelReachedAndClosed.Notification -= LevelReachedAndClosed_ForUnlockMaternity_Notification;
            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController.Instance.SetCurrentStep();
        }
    }

    // complete notification - maternity_tap_to_open
    public void SubscribeMaternityWardObjectClicked()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MaternityWardObjectClikedNotif.Notification += MaternityWardObjectClikedNotif_Notification;

        CancelInvoke("MaternityWardInvokedArrow");
        //Invoke("MaternityWardInvokedArrow", 2f);
    }

    // complete notification - maternity_tap_to_open
    private void MaternityWardObjectClikedNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MaternityWardObjectClikedNotif.Notification -= MaternityWardObjectClikedNotif_Notification;
        CancelInvoke("MaternityWardInvokedArrow");

        TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - maternity_ward_renovate
    public void SubscribeMaternityWardRenovate()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.InGameCloud.Hide();
        instanceNC.MaternityWardRenovateNotif.Notification += MaternityWardRenovateNotif_Notification;
    }

    // complete notification - maternity_ward_renovate
    private void MaternityWardRenovateNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MaternityWardRenovateNotif.Notification -= MaternityWardRenovateNotif_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - maternity_ward_build_end
    public void SubscribeMaternityWardBuildEnd()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MaternityWardBuildEndNotif.Notification += MaternityWardBuildEndNotif_Notification;
    }

    // complete notification - maternity_ward_build_end
    private void MaternityWardBuildEndNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MaternityWardBuildEndNotif.Notification -= MaternityWardBuildEndNotif_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - waiting room
    public void SubscribeWaitingRoomBlueOrchidNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;

        var roomExist = MaternityAreasMapController.MaternityMap.FindRotatableObject("WaitingRoomBlueOrchid");

        if (!roomExist)
        {
            TutorialUIController.Instance.BlinkDrawerButton(true);
            instanceNC.WaitingRoomBlueOrchidAdded.Notification += WaitingRoomBlueOrchidAdded_Notification;
        }
        else
            TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - waiting room
    private void WaitingRoomBlueOrchidAdded_Notification(BlueDoctorOfficeAddedEventArgs eventArgs)
    {
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.BlinkDrawerButton(false);
        instanceNC.WaitingRoomBlueOrchidAdded.Notification -= WaitingRoomBlueOrchidAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - labor room
    public void SubscribeLaborRoomBlueOrchidNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;

        var roomExist = MaternityAreasMapController.MaternityMap.FindRotatableObject("LabourRoomBlueOrchid");

        if (!roomExist)
        {
            TutorialUIController.Instance.BlinkDrawerButton(true);
            instanceNC.LaborRoomBlueOrchidAdded.Notification += LaborRoomBlueOrchidAdded_Notification;
        }
        else
            TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - labor room
    private void LaborRoomBlueOrchidAdded_Notification(BlueDoctorOfficeAddedEventArgs eventArgs)
    {
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.BlinkDrawerButton(false);
        instanceNC.LaborRoomBlueOrchidAdded.Notification -= LaborRoomBlueOrchidAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    // complete notification - mother reached waiting room
    public void SubscribeMotherReachedWaitingRoomNotification()
    {
        MaternityPatientAI firstPatient = null;
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            firstPatient = patient;
        }

        if (firstPatient == null)
            Debug.LogError("TODODODOODODOO: nie powinno się to zdarzyć");
        else
        {
            if (firstPatient.Person.State is MaternityPatientWaitingForSendToDiagnoseState)
                TutorialController.Instance.IncrementCurrentStep();
            else
            {
                NotificationCenter instanceNC = NotificationCenter.Instance;
                instanceNC.MotherReachedWaitingRoomNotif.Notification += MotherReachedWaitingRoomNotif_Notification;
                ReferenceHolder.Get().engine.MainCamera.SmoothZoom(8, 1, true);
                ReferenceHolder.Get().engine.MainCamera.FollowGameObject(firstPatient.gameObject.transform);
                UIController.get.CloseActiveHover();
                UIController.get.ExitAllPopUps();
            }
        }
    }

    // complete notification - mother reached waiting room
    private void MotherReachedWaitingRoomNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MotherReachedWaitingRoomNotif.Notification -= MotherReachedWaitingRoomNotif_Notification;
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        TutorialController.Instance.IncrementCurrentStep();
        TutorialUIController.Instance.InGameCloud.Hide();
    }

    public void SubscribeWaitingRoomWorkingClickedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.WaitingRoomWorkingClickedNotif.Notification += WaitingRoomWorkingClickedNotif_Notification;
    }

    private void WaitingRoomWorkingClickedNotif_Notification(SetCurrentlyPointedMachineEventArgs eventArgs)
    {
        if (eventArgs.obj.Tag == "WaitingRoomBlueOrchid")
        {
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.WaitingRoomWorkingClickedNotif.Notification -= WaitingRoomWorkingClickedNotif_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    // condition - maternity waiting for labor
    public void SubscribeVitamiesDeliverdNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MotherVitaminazed.Notification += MotherVitaminazed_Notification;
    }

    private void MotherVitaminazed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MotherVitaminazed.Notification -= MotherVitaminazed_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    #endregion

    private void LevelReachedAndClosed_Notification(LevelReachedAndClosedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - level wbity i zamknięty. Wymagany level: " + levelRequired);
        if (levelRequired != eventArgs.level)
        {
            //Debug.Log("This is not the level we are waiting for");
            return;
        }

        TutorialController tc = TutorialController.Instance;
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.LevelReachedAndClosed.Notification -= LevelReachedAndClosed_Notification;
        tc.ConfirmConditionExecution();
        tc.SetCurrentStep();
        switch (eventArgs.level)
        {
            //here if needed you can put special stuff that happens on the particular levels (tutorial related)
            case 3:
                if (TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.follow_ambulance, true))
                    ReferenceHolder.GetHospital().Ambulance.IsBlockedByTutorial = false;
                break;
            case 4:
                Invoke("RegisterForNotifications", 1.1f);
                break;
            case 5:
                TutorialUIController.Instance.BlinkDrawerButton(false);
                break;
            default:
                break;
        }
    }

    public void SubscribeLevelReachedAndClosedNotification(int requiredLevel)
    {
        //Debug.Log("Subscribed to level reached and pop up closed notification for level: " + requiredLevel);
        levelRequired = requiredLevel;

        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.LevelReachedAndClosed.Notification += LevelReachedAndClosed_Notification;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.InGameCloud.Hide();
        TutorialUIController.Instance.StopShowCoroutines();
        if (Game.Instance.gameState().GetHospitalLevel() >= requiredLevel &&
            !UIController.get.LevelUpPopUp.gameObject.activeSelf)
        {
            Debug.Log("Level: " + requiredLevel + " already reached before this step. Invoking levelreachedandclosed");
            instanceNC.LevelReachedAndClosed.Invoke(new LevelReachedAndClosedEventArgs(requiredLevel));
        }
        else
        {
            //Debug.LogWarning("Level: " + requiredLevel + "not yet reached or pop up enabled. Waiting for completion");
            ReferenceHolder.GetHospital().Ambulance.IsBlockedByTutorial |= requiredLevel == 3;
        }

        if (requiredLevel == 9 && RatePopUp.ShouldShowRate(false))
            StartCoroutine(UIController.get.RatePopUp.Open());
    }

    private void FullscreenTutHidden_Notification(FullscreenTutHiddenEventArgs eventArgs)
    {
        //Debug.LogError("Wychwycenie wystąpienia poprawnej notyfikacji - full screen tutorial zamkniety.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.FullscreenTutHidden.Notification -= FullscreenTutHidden_Notification;
        //Debug.LogError("FullscreenTutHidden.Notification is null: " + instanceNC.FullscreenTutHidden.IsNull());

        switch (TutorialController.Instance.CurrentTutorialStepTag)
        {
            case StepTag.keep_curing_text_1:
                MedicineRef blueSyrup = new MedicineRef(MedicineType.Syrop, 1);
                UIController.get.storageCounter.Add(blueSyrup.IsMedicineForTankElixir());

                GameState.Get().AddResource(blueSyrup, 1, true, EconomySource.Tutorial);

                bool isTank = blueSyrup.IsMedicineForTankElixir();

                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, Vector3.zero, 1, 0, 1.75f,
                    Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(blueSyrup), null,
                    () => { UIController.get.storageCounter.Remove(1, isTank); });
                break;
            case StepTag.pharmacy_offers:
                TutorialUIController.Instance.BlinkImage(UIController.getHospital.PharmacyPopUp.globalOffers
                    .GetComponent<Image>());
                TutorialUIController.Instance.ShowTutorialArrowUI(
                    UIController.getHospital.PharmacyPopUp.globalOffers.GetComponent<RectTransform>(), Vector2.zero);
                break;
            case StepTag.patio_tidy_4:
                //ShopRoomInfo[] decos = HospitalTutorialController.HospitalTutorialInstance.grantedDecorations;
                //GameState.Get().AddToObjectStored(decos[0], 1);
                //GameState.Get().AddToObjectStored(decos[1], 1);
                //GameState.Get().AddToObjectStored(decos[2], 1);
                //Vector3 startPoint = Vector3.zero;
                //ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, 1, 0f, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[0].ShopImage, null, null);
                //ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, 1, 0.35f, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[1].ShopImage, null, null);
                //ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, 1, 0.7f, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[2].ShopImage, null, null);
                break;
            case StepTag.bed_patient_arrived:
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(33.5f, 0, 32.5f), 1f, false);
                ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7, 1, false);
                break;
            case StepTag.garden_text:
                //Plantation.AnimateGardener ();
                //Debug.Log ("WYSYLAM TRIGGER");
                break;
            case StepTag.bubble_boy_intro:
                ReferenceHolder.GetHospital().bubbleBoyEntryOverlayController.OnClickEnabled();
                break;
            default:
                break;
        }

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeFullscreenTutHiddenNotification()
    {
        //Debug.LogError("Subscribed to full screen emma hidden notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        if (!instanceNC.FullscreenTutHidden.IsNull())
            Debug.Log("Notification already subscribed. Will not subscribe again.");
        else
            instanceNC.FullscreenTutHidden.Notification += FullscreenTutHidden_Notification;
    }

    private void FirstPatientArriving_Notification(FirstPatientArrivingEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - spawn pacjenta w łóżku.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.FirstPatientArriving.Notification -= FirstPatientArriving_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
        //ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(eventArgs.gObj.transform.position, 1f, true);
    }

    public void SubscribeFirstPatientArrivingNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.FirstPatientArriving.Notification += FirstPatientArriving_Notification;
        NotificationCenter.Instance.FirstPatientArriving.Invoke(new FirstPatientArrivingEventArgs());
    }

    private void SpawnFirstPatient_Notification(SpawnFirstPatientEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji SpawnFirstPatient_Notification");
        if (ClinicPatientAI.Patients.Count < 1)
        {
            ClinicPatientAI.SpawnPatientForNewDoctor(new Vector2i(21, 56),
                ResourcesHolder.GetHospital().GetClinicDisease(0), true);
        }

        NotificationCenter.Instance.SpawnFirstPatient.Notification -= SpawnFirstPatient_Notification;
        ClinicPatientAI.SendFirstPatientToReception();
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeVitaminesMakerEmma1ClosedNotif()
    {
        NotificationCenter.Instance.VitaminesMakerEmma1ClosedNotif.Notification +=
            VitaminesMakerEmma1ClosedNotif_Notification;
    }

    private void VitaminesMakerEmma1ClosedNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VitaminesMakerEmma1ClosedNotif.Notification -=
            VitaminesMakerEmma1ClosedNotif_Notification;
        NotificationCenter.Instance.VitaminesMakerEmma1ClosedCond.Invoke(new BaseNotificationEventArgs());
    }

    public void SubscribeSpawnFirstPatientNotification()
    {
        //Debug.Log("Subscribing to first emergency patient spawned");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.SpawnFirstPatient.Notification += SpawnFirstPatient_Notification;
        NotificationCenter.Instance.SpawnFirstPatient.Invoke(new SpawnFirstPatientEventArgs());
    }

    public void SubscribeFirstPatientNearSignOfHospitalNotification()
    {
        NotificationCenter.Instance.FirstPatientNearSignOfHospital.Notification +=
            FirstPatientNearSignOfHospital_Notification;

        foreach (BasePatientAI p in BasePatientAI.patients)
        {
            if (p.GetType() == typeof(ClinicPatientAI)) //) && ((ClinicPatientAI)p)()) 
            {
                if (((ClinicPatientAI)p).IsStandNearSignState())
                {
                    //Debug.Log("Subscribing to patient near sign");
                    NotificationCenter.Instance.FirstPatientNearSignOfHospital.Notification -=
                        FirstPatientNearSignOfHospital_Notification;
                    TutorialController.Instance.ConfirmConditionExecution();
                    TutorialController.Instance.SetCurrentStep();
                }
            }
        }

        NotificationCenter.Instance.FirstPatientNearSignOfHospital.Invoke(
            new FirstPatientNearSignOfHospitalEventArgs());
    }

    private void FirstPatientNearSignOfHospital_Notification(FirstPatientNearSignOfHospitalEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji dla FirstPatientNearSignOfHospital");

        NotificationCenter.Instance.FirstPatientNearSignOfHospital.Notification -=
            FirstPatientNearSignOfHospital_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeExpandConditionsNotification()
    {
        NotificationCenter.Instance.ExpandConditions.Notification += ExpandConditions_Notification;
    }

    private void ExpandConditions_Notification(ExpandConditionsEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji dla ExpandConditions");

        NotificationCenter.Instance.ExpandConditions.Notification -= ExpandConditions_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    private void SubscribeBlueSyrupExtractionCompleted_Notification(BlueSyrupExtractionCompletedArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BlueSyrupExtractionCompleted.Notification -= SubscribeBlueSyrupExtractionCompleted_Notification;
        if (!eventArgs.SpeedUpUsed)
            TutorialController.Instance.IncrementCurrentStep();
        else
            TutorialController.Instance.SetStep(StepTag.cure_bed_patient);
    }

    public void SubscribeBlueSyrupExtractionCompleted()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BlueSyrupExtractionCompleted.Notification += SubscribeBlueSyrupExtractionCompleted_Notification;
    }

    private void SubscribeTreasurePopUpOpened_Notification(TreasurePopUpOpenedArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasurePopUpOpened.Notification -= SubscribeTreasurePopUpOpened_Notification;

        //UIController.get.vIPGiftBoxPopup.Open();
        ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromVIP = true;
        UIController.getHospital.unboxingPopUp.OpenVIPCasePopup();

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.treasure_from_Leo)
            Hospital.HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>()
                .MoveVipToHeliTease();

        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeTreasurePopUpOpened()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasurePopUpOpened.Notification += SubscribeTreasurePopUpOpened_Notification;
        instanceNC.TreasurePopUpOpened.Invoke(new TreasurePopUpOpenedArgs());
    }

    private void SubscribeTreasurePopUpClosed_Notification(TreasurePopUpClosedArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasurePopUpClosed.Notification -= SubscribeTreasurePopUpClosed_Notification;
        //if (Hospital.HospitalAreasMapController.Map.vipRoom.GetComponent<VIPSystemManager>().transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().Person.State is VIPPersonController.TutorialTeaseGoToHeli && Hospital.HospitalAreasMapController.Map.vipRoom.GetComponent<VIPSystemManager>().transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().state == VIPPersonController.CharacterStatus.TutorialWaitForPopUpClose)
        //{
        //     Hospital.HospitalAreasMapController.Map.vipRoom.GetComponent<VIPSystemManager>().DepartVIP();//??
        //  }
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeTreasurePopUpClosed()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasurePopUpClosed.Notification += SubscribeTreasurePopUpClosed_Notification;
    }

    private void SubscribeBlueSyrupCollected_Notification(SyrupCollectedEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BlueSyrupCollected.Notification -= SubscribeBlueSyrupCollected_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeBlueSyrupCollected()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BlueSyrupCollected.Notification += SubscribeBlueSyrupCollected_Notification;
    }

    private void FollowBob_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.FollowBob.Notification -= FollowBob_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();

        if (waitForPatientSpawn == null)
            waitForPatientSpawn = StartCoroutine(WaitUntilPatientSpawn());
    }

    public IEnumerator WaitUntilPatientSpawn()
    {
        while (true)
        {
            if (BasePatientAI.patients.Count > 0)
            {
                ReferenceHolder.Get().engine.MainCamera.FollowGameObject(BasePatientAI.patients[0].transform);

                try
                {
                    if (waitForPatientSpawn != null)
                        StopCoroutine(waitForPatientSpawn);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }

                waitForPatientSpawn = null;
                yield return null;
            }
            else
                yield return new WaitForEndOfFrame();
        }
    }

    public void SubscribeFollowBobNotification()
    {
        //Debug.Log("Subscribing to FollowBob spawned");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.FollowBob.Notification += FollowBob_Notification;
        instanceNC.FollowBob.Invoke(new BaseNotificationEventArgs());
    }

    private void MedicopterTookOff_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.MedicopterTookOff.Notification -= MedicopterTookOff_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeMedicopterTookOffNotification()
    {
        //Debug.Log("Subscribing to MedicopterTookOff spawned");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MedicopterTookOff.Notification += MedicopterTookOff_Notification;
    }

    private void FirstEmergencyPatientSpawned_Notification(FirstEmergencyPatientSpawnedEventArgs eventArgs)
    {
        Debug.Log("Notification for FirstEmergencyPatientSpawned received");
        NotificationCenter.Instance.FirstEmergencyPatientSpawned.Notification -=
            FirstEmergencyPatientSpawned_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();

        ReferenceHolder.Get().engine.MainCamera.FollowGameObject(eventArgs.patientGO.transform);
    }

    public void SubscribeFirstEmergencyPatientSpawnedNotification()
    {
        //Debug.Log("Subscribing to first emergency patient spawned");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.FirstEmergencyPatientSpawned.Notification += FirstEmergencyPatientSpawned_Notification;
    }

    public void SubscribeBacteriaPatientReachedBedNotification()
    {
        //Debug.LogError("SubscribeDiagnosePatientReachedBedNotification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientReachedBed.Notification += PatientReachedBed_Notification;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_spawn &&
            HospitalPatientAI.Patients.Count > 0)
        {
            foreach (HospitalCharacterInfo patient in HospitalPatientAI.Patients)
            {
                if (patient.HasBacteria)
                {
                    NotificationCenter.Instance.PatientReachedBed.Invoke(
                        new PatientReachedBedEventArgs(patient.RequiresDiagnosis, patient.HasBacteria));
                    break;
                }
            }
        }
    }

    private void PatientReachedBed_Notification(PatientReachedBedEventArgs eventArgs)
    {
        //Debug.LogError("PatientReachedBed_Notification diagnosis = " + eventArgs.requiresDiagnosis);
        NotificationCenter instanceNC = NotificationCenter.Instance;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.diagnose_spawn)
        {
            if (eventArgs.requiresDiagnosis)
            {
                UIController.get.ExitAllPopUps();
                instanceNC.PatientReachedBed.Notification -= PatientReachedBed_Notification;
                TutorialController.Instance.IncrementCurrentStep();
            }
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_spawn)
        {
            if (eventArgs.hasBacteria)
            {
                UIController.get.ExitAllPopUps();
                instanceNC.PatientReachedBed.Notification -= PatientReachedBed_Notification;
                TutorialController.Instance.IncrementCurrentStep();
            }
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_open)
        {
            instanceNC.PatientReachedBed.Notification -= PatientReachedBed_Notification;
            ReferenceHolder.Get().engine.MainCamera.StopFollowing();
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribeFirstEmergencyPatientReachedBedNotification()
    {
        //Debug.Log("Subscribing to first emergency patient reached bed notif. Current Patient count = " + HospitalPatientAI.Patients.Count);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientReachedBed.Notification += PatientReachedBed_Notification;

        if (HospitalPatientAI.Patients.Count > 0)
            NotificationCenter.Instance.PatientReachedBed.Invoke(new PatientReachedBedEventArgs(false, false));
    }

    public void SubscribeDiagnosePatientReachedBedNotification()
    {
        //Debug.LogError("SubscribeDiagnosePatientReachedBedNotification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientReachedBed.Notification += PatientReachedBed_Notification;
    }

    float lastTimer = 0;

    private void AmbulanceReachedHospital_Notification(AmbulanceReachedHospitalEventArgs eventArgs)
    {
        ////Debug.LogError("Wychwycenie wystąpienia poprawnej notyfikacji - AmbulanceReachedHospital " + Time.time);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        if (lastTimer == Time.time)
        {
            instanceNC.AmbulanceReachedHospital.Notification -= AmbulanceReachedHospital_Notification;
            return;
        }

        lastTimer = Time.time;
        instanceNC.AmbulanceReachedHospital.Notification -= AmbulanceReachedHospital_Notification;
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeAmbulanceReachedHospitalNotification()
    {
        ReferenceHolder.GetHospital().Ambulance.IsBlockedByTutorial = false;

        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.AmbulanceReachedHospital.Notification += AmbulanceReachedHospital_Notification;
        Invoke("FollowAmbulanceDelayed", 1.5f);
    }

    void FollowAmbulanceDelayed()
    {
        ReferenceHolder.Get().engine.MainCamera
            .FollowGameObject(ReferenceHolder.GetHospital().Ambulance.GetAmbulanceTransform());
        UIController.get.CloseActiveHover();
        UIController.get.ExitAllPopUps();

        //InvokeRepeating("CloseUI", 0.1f, 0.1f);
        if (invokeCloseUI == null)
            invokeCloseUI = StartCoroutine(InvokeRepeatingCloseUI());
    }

    int invokeRepeatCount = 0;

    private IEnumerator InvokeRepeatingCloseUI()
    {
        while (invokeRepeatCount < 15)
        {
            UIController.get.CloseActiveHover();
            UIController.get.ExitAllPopUps();
            invokeRepeatCount++;

            if (invokeRepeatCount >= 15)
                StopCoroutine(invokeCloseUI);
            else
                yield return null;
        }

        invokeRepeatCount = 0;
        yield return null;
    }

    //this is a dirty fix of the rare case when player can open a hover after the camera starts following the ambulance. This caused some weird hover position jumping.
    void CloseUI()
    {
        UIController.get.CloseActiveHover();
        UIController.get.ExitAllPopUps();
        invokeRepeatCount++;
        if (invokeRepeatCount >= 15)
            CancelInvoke("CloseUI");
    }

    private void PatientCardOpened_Notification(PatientCardOpenedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - karta pacjenta otwarta.");
        if (TutorialController.Instance.CurrentTutorialStepIndex >=
            TutorialController.Instance.GetStepId(StepTag.bacteria_george_2))
        {
            if (eventArgs.info.HasBacteria && !eventArgs.info.IsVIP)
            {
                NotificationCenter.Instance.PatientCardOpened.Notification -= PatientCardOpened_Notification;
                Debug.LogError("GOT Bacteria Patient!");
                TutorialController.Instance.IncrementCurrentStep();
            }
        }
        else if (TutorialController.Instance.CurrentTutorialStepIndex >=
                 TutorialController.Instance.GetStepId(StepTag.diagnose_spawn))
        {
            if (eventArgs.info.RequiresDiagnosis)
            {
                NotificationCenter.Instance.PatientCardOpened.Notification -= PatientCardOpened_Notification;
                Debug.LogError("GOT IT!");
                TutorialController.Instance.SetStep(StepTag.diagnose_text);
            }
        }
        else
        {
            NotificationCenter.Instance.PatientCardOpened.Notification -= PatientCardOpened_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribePatientCardOpenedNotification()
    {
        //Debug.Log("Subskrypcja Patient Card Opened");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCardOpened.Notification += PatientCardOpened_Notification;
    }

    private void PatientCardClosed_Notification(PatientCardClosedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - karta pacjenta zamknięta.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCardClosed.Notification -= PatientCardClosed_Notification;
        TutorialUIController.Instance.StopBlinking();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePatientCardClosedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCardClosed.Notification += PatientCardClosed_Notification;
        if (UIController.getHospital.PatientCard.gameObject.activeSelf == false)
            NotificationCenter.Instance.PatientCardClosed.Invoke(new PatientCardClosedEventArgs());
        else
        {
            ////Debug.LogError("Blinking exit button!");
            TutorialUIController.Instance.BlinkImage(UIController.getHospital.PatientCard.ExitImage);
        }

        UIController.getHospital.PatientCard.ExitButton.interactable = true;
    }

    private void TapAnywhere_Notification(TapAnywhereEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - tap anywhere.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TapAnywhere.Notification -= TapAnywhere_Notification;
        CancelInvoke("EnableTapAnywhere");
        //tapAnywhere = false;
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_text_2)
            UIController.getHospital.PatientCard.Exit();
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_5)
        {
            UIController.getHospital.PatientCard.ExitButton.interactable = true;
            UIController.getHospital.PatientCard.HideBacteriaTutorialMask();
            UIController.getHospital.PatientCard.HideBacteriaTutorialInfo();
            TutorialUIController.Instance.InGameCloud.Hide();
        }

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_mixer_text)
            TutorialUIController.Instance.InGameCloud.Hide();

        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.HideTapToContinue();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeTapAnywhereNotification()
    {
        //Debug.Log("Sub Tap anywhere step = " + TutorialController.Instance.CurrentTutorialStepIndex);
        NotificationCenter instanceNC = NotificationCenter.Instance;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_3)
        {
            UIController.getHospital.PatientCard.ShowBacteriaTutorialMask();
            UIController.getHospital.PatientCard.ShowBacteriaTutorialArrow();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_4 ||
                 TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_5)
        {
            UIController.getHospital.PatientCard.ShowBacteriaTutorialMask();
            UIController.getHospital.PatientCard.ShowBacteriaTutorialInfo();
            UIController.getHospital.PatientCard.HideBacteriaTutorialArrow();
        }

        instanceNC.TapAnywhere.Notification += TapAnywhere_Notification;
        TutorialUIController.Instance.HideTapToContinue();
        //tapAnywhere = false;
        Invoke("EnableTapAnywhere", 2f);
        Invoke("StartBlinkingPatientCard", .25f);
    }

    public class TapAnywhereActionWrapper<T>
    {
        Action<T> action;
        NotificationCenter instanceNC;
        T args;

        public TapAnywhereActionWrapper(Action<T> action, T args, NotificationCenter instanceNC)
        {
            this.action = action;
            this.instanceNC = instanceNC;
            this.args = args;
        }

        public void DoAction(TapAnywhereEventArgs parameters)
        {
            action(args);
            //TutorialUIController.Instance.HideTapToContinue();
            instanceNC.TapAnywhere.Notification -= DoAction;
        }
    }

    public TapAnywhereActionWrapper<T> SubscribeNonTutorialActionToTapAnywhere<T>(Action<T> action, T args)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TapAnywhereActionWrapper<T> wrapper = new TapAnywhereActionWrapper<T>(action, args, instanceNC);
        instanceNC.TapAnywhere.Notification += wrapper.DoAction;
        TutorialUIController.Instance.HideTapToContinue();
        //tapAnywhere = true;
        return wrapper;
    }

    public void UnsubscribeNonTutorialActionToTapAnywhere<T>(TapAnywhereActionWrapper<T> action)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TapAnywhere.Notification -= action.DoAction;
        TutorialUIController.Instance.HideTapToContinue();
        //tapAnywhere = false;
    }

    void StartBlinkingPatientCard()
    {
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_text_1)
        {
            // if you load game on this step
            if (!UIController.getHospital.PatientCard.IsVisible)
            {
                var pat = FindObjectOfType<HospitalPatientAI>();
                if (pat != null)
                    UIController.getHospital.PatientCard.Open(pat.GetComponent<HospitalCharacterInfo>(), 0);
            }

            ////Debug.LogError("BlinkImage image: " + UIController.get.PatientCard.TreatmentContent.transform.GetChild(0).name);
            TutorialUIController.Instance.BlinkImage(
                UIController.getHospital.PatientCard.TreatmentContent.transform.GetChild(0).GetChild(0)
                    .GetComponent<Image>(), 1.4f, false);
            TutorialUIController.Instance.BlinkImage(UIController.getHospital.PatientCard.Silhouette[4], 1.4f, false);
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_text_2)
        {
            // if you load game on this step
            if (!UIController.getHospital.PatientCard.IsVisible)
            {
                var pat = FindObjectOfType<HospitalPatientAI>();
                if (pat != null)
                    UIController.getHospital.PatientCard.Open(pat.GetComponent<HospitalCharacterInfo>(), 0);
            }

            ////Debug.LogError("BlinkImage image: " + UIController.get.PatientCard.TreatmentContent.transform.GetChild(0).GetChild(1).name);
            TutorialUIController.Instance.BlinkImage(UIController.getHospital.PatientCard.TreatmentContent.transform
                .GetChild(0).GetChild(1).GetComponent<Image>());
        }
    }

    void EnableTapAnywhere()
    {
        if (!TutorialController.Instance.GetCurrentStepData().InGameTapToContinueShown &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bacteria_emma_micro_2 &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bacteria_george_3 &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bacteria_george_4 &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bacteria_george_5)
            TutorialUIController.Instance.ShowTapToContinue();
        //tapAnywhere = true;
    }

    private void PatientCardIsOpen_Notification(PatientCardIsOpenEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnego condition - karta pacjenta otwarta.");
        NotificationCenter instanceNC = NotificationCenter.Instance;


        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_2 ||
            TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_3 ||
            TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_4 ||
            TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_george_5)
        {
            if (eventArgs.hasBacteria)
            {
                instanceNC.PatientCardIsOpen.Notification -= PatientCardIsOpen_Notification;
                TutorialController.Instance.ConfirmConditionExecution();
                TutorialController.Instance.SetCurrentStep();
                ReferenceHolder.Get().engine.MainCamera.BlockUserInput = true;
            }
        }
        else
        {
            instanceNC.PatientCardIsOpen.Notification -= PatientCardIsOpen_Notification;
            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController.Instance.SetCurrentStep();
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = true;
        }
    }

    public void SubscribePatientCardIsOpenNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCardIsOpen.Notification += PatientCardIsOpen_Notification;

        if (UIController.getHospital.PatientCard.gameObject.activeSelf)
            NotificationCenter.Instance.PatientCardIsOpen.Invoke(new PatientCardIsOpenEventArgs(true));

        if ((TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_text_1) ||
            (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_text_2) ||
            (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_open))
        {
            SubscribeTutorialArrowSetNotification();
        }
    }

    private void PatientCardIsClosed_Notification(PatientCardIsClosedEventArgs eventArgs)
    {
        Debug.Log("Wychwycenie wystąpienia poprawnego condition - karta pacjenta zamknięta.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCardIsClosed.Notification -= PatientCardIsClosed_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
        ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
    }

    public void SubscribePatientCardIsClosedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.Emma_on_VIP_Special_rewards)
        {
            var currentVipObj = ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>().currentVip;
            if (currentVipObj != null)
            {
                VIPPersonController vip = currentVipObj.GetComponent<VIPPersonController>();

                if ((vip == null || vip != null && vip.Person.State.GetType() == typeof(VIPPersonController.GoHome)) &&
                    !HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    TutorialController.Instance.ConfirmConditionExecution();
                    TutorialController.Instance.SetCurrentStep();
                    ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
                    return;
                }
            }
        }

        instanceNC.PatientCardIsClosed.Notification += PatientCardIsClosed_Notification;
        if (UIController.getHospital.PatientCard.gameObject.activeSelf == false)
        {
            NotificationCenter.Instance.PatientCardIsClosed.Invoke(new PatientCardIsClosedEventArgs());
        }
    }
    /*
    public void SubscribeVIPGiftPopupClosed()
    {
        if (!UIController.getHospital.PatientCard.IsVisible)
        {
            TutorialController.Instance.IncrementCurrentStep();
            return;
        }
        var patientCard = UIController.getHospital.PatientCard;
        patientCard.isExitBlocked = true;
        TutorialUIController.Instance.ShowTutorialArrowUI(patientCard.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForPatientCureButton);
        TutorialController.Instance.GivePlayerFreeMeds(UIController.getHospital.PatientCard.CurrentCharacter.requiredMedicines);
        patientCard.RefreshView(patientCard.CurrentCharacter);
        NotificationCenter.Instance.PackageCollected.Notification += VIPGiftPopupClosed;
    }*/

    public void SubscribeVIPGiftPopupClosed()
    {
        var patientCard = UIController.getHospital.PatientCard;
        if (!UIController.getHospital.PatientCard.IsVisible && HospitalAreasMapController.HospitalMap.vipRoom
                .GetComponent<VIPSystemManager>().GetSecondsToLeave() > 0)
        {
            //TutorialController.Instance.SetStep(StepTag.Emma_on_Sick_Leo);
            //return;
            //UIController.getHospital.PatientCard.Open();
            HospitalAreasMapController.HospitalMap.hospitalBedController.GetVIPBedID(out int id);
            UIController.getHospital.PatientCard.Open(
                ((BasePatientAI)HospitalAreasMapController.HospitalMap.hospitalBedController
                    .GetBedWithIDFromRoom(null, 0).Patient).GetComponent<HospitalCharacterInfo>(), id);

            patientCard.isExitBlocked = true;
            NotificationCenter.Instance.PackageCollected.Notification += VIPGiftPopupClosed;
            return;
        }

        if (ReferenceHolder.GetHospital().vipSystemManager.GetSecondsToLeave() <= 0)
        {
            TutorialController.Instance.SetStep(StepTag.level_10);
            return;
        }

        patientCard.isExitBlocked = true;
        //TutorialController.Instance.GivePlayerFreeMeds(UIController.getHospital.PatientCard.CurrentCharacter.requiredMedicines);
        patientCard.RefreshView(patientCard.CurrentCharacter);
        NotificationCenter.Instance.PackageCollected.Notification += VIPGiftPopupClosed;
    }

    private void VIPGiftPopupClosed(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.PackageCollected.Notification -= VIPGiftPopupClosed;
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification -= SkipVipSpeedupTutorial_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    private void InGameEmmaHidden_Notification(InGameEmmaHiddenEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zamknięcie popupu po kliku.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.InGameEmmaHidden.Notification -= InGameEmmaHidden_Notification;
        HospitalAreasMapController.HospitalMap.ResetOnPressAction();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeInGameEmmaHiddenNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.InGameEmmaHidden.Notification += InGameEmmaHidden_Notification;
        ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ChangeOnPressType((x) =>
        {
            NotificationCenter.Instance.InGameEmmaHidden.Invoke(new InGameEmmaHiddenEventArgs());
        });
    }

    private void ShowTutorialsInputField_Notification(ShowTutorialsInputFieldEventArgs eventArgs)
    {
        //Debug.LogError("ShowTutorialsInputField_Notification");
        //Debug.Log("WychwyceniFe wystąpienia poprawnej notyfikacji - wyświetlenie input field.");
        TutorialUIController.Instance.ShowTutorialsInputField();
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ShowTutorialsInputField.Notification -= ShowTutorialsInputField_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeShowTutorialsInputFieldNotification()
    {
        //Debug.LogError("SubscribeShowTutorialsInputFieldNotification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ShowTutorialsInputField.Notification += ShowTutorialsInputField_Notification;
        Invoke("DelayedInputField", 1.5f);
    }

    void DelayedInputField()
    {
        //Debug.LogError("DelayedInputField");
        NotificationCenter.Instance.ShowTutorialsInputField.Invoke(new ShowTutorialsInputFieldEventArgs());
    }

    private void DrawerClosed_Notification(DrawerClosedEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        /*if (TutorialController.Instance.GetCurrentStepData().NecessaryCondition == Condition.Level5ReachedAndClosed)
        {
            //TutorialUIController.Instance.tutorialArrowUI.Hide();
        }
        else*/
        if
            (TutorialController.Instance.GetCurrentStepData().NotificationType ==
             NotificationType.BlueDoctorsOfficeAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.SyrupLabAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType ==
             NotificationType.YellowDoctorsOfficeAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.ElixirMixerAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType ==
             NotificationType.GreenDoctorsOfficeAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.PillMakerAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType ==
             NotificationType.PatioDecorationsAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType ==
             NotificationType.WaitingRoomBlueOrchidAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.XRayAdded ||
             TutorialController.Instance.GetCurrentStepData().NotificationType ==
             NotificationType.LaborRoomBlueOrchidAdded)
        {
            //if (TutorialController.Instance.ConditionFulified && !UIController.get.drawer.IsDragingObjectDummy())
            //{
            //}
        }
        else
        {
            TutorialUIController.Instance.BlinkDrawerButton(false);
            TutorialUIController.Instance.StopBlinking();
            TutorialController.Instance.IncrementCurrentStep();
        }

        instanceNC.DrawerClosed.Notification -= DrawerClosed_Notification;
    }

    public void SubscribeDrawerClosedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.DrawerClosed.Notification += DrawerClosed_Notification;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_again_lvl6)
            TutorialUIController.Instance.BlinkDrawerButton(true);
    }

    private void DrawerOpened_Notification(DrawerOpenedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - otwarcie drawera. Current step = " + TutorialController.Instance.CurrentTutorialStepIndex);
        //RefactoredDrawerController drawer = UIController.get.drawer;
        IDrawer drawer = UIController.get.drawer;
        TutorialController tutorialController = TutorialController.Instance;
        Image blinkingImage = null;
        if (tutorialController.GetCurrentStepData().NotificationType == NotificationType.DrawerOpened)
        {
            //Debug.Log ("Dla wszystkich notyfikacji z DrawerOpened");
            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == tutorialController.GetCurrentStepData().TargetMachineTag)
                {
                    if (tutorialController.CurrentTutorialStepTag == StepTag.new_probe_tables)
                    {
                        drawer.CenterCameraToArea(HospitalArea.Laboratory, false);
                        drawer.SwitchToTab(1);
                        NotificationCenter.Instance.DrawerOpened.Notification -= DrawerOpened_Notification;
                        TutorialUIController.Instance.BlinkDrawerButton(false);
                        break;
                    }

                    //blinkingImage = item.drawerItem.image;
                    //TutorialUIController.Instance.BlinkImage(blinkingImage);
                    break;
                }
            }

            TutorialUIController.Instance.StopBlinking();
            TutorialUIController.Instance.BlinkDrawerButton(false);
            TutorialController.Instance.IncrementCurrentStep();
        }

        UIDrawerController.DrawerRotationItem target = null;
        if (tutorialController.GetCurrentStepData().NotificationType == NotificationType.BlueDoctorsOfficeAdded)
        {
            //Debug.Log ("Starting blinking shop item in tutorial step 15");
            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == tutorialController.GetCurrentStepData().TargetMachineTag)
                {
                    target = item;
                    drawer.SwitchToTab(0);
                    blinkingImage = item.drawerItem.image;
                    TutorialUIController.Instance.BlinkImage(blinkingImage);
                    break;
                }
            }

            if (blinkingImage != null)
            {
                //TutorialUIController.Instance.tutorialArrowUI.Hide();
                //if (target != null)
                //{
                //    //TutorialUIController.Instance.ShowTutorialArrowUI(UIController.get.drawer.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForBlueDoctorDrag, 0, TutorialUIController.TutorialPointerAnimationType.swipe_right);
                //}
                SubscribeDrawerClosedNotification();
            }

            return;
        }

        if (tutorialController.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds ||
            tutorialController.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_again ||
            tutorialController.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_lvl6 ||
            tutorialController.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_again_lvl6)
        {
            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == tutorialController.GetCurrentStepData().TargetMachineTag)
                {
                    drawer.CenterCameraToArea(0, false);
                    drawer.SwitchToTab(0);
                    blinkingImage = item.drawerItem.image;
                    TutorialUIController.Instance.BlinkImage(blinkingImage);
                    TutorialController.Instance.ConfirmConditionExecution();
                    TutorialController.Instance.SetCurrentStep();
                    break;
                }
            }
        }
        else if (tutorialController.GetCurrentStepData().NotificationType == NotificationType.SyrupLabAdded)
        {
            //Debug.Log ("Starting blinking shop item in tutorial step SyrupLabAdded");
            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == tutorialController.GetCurrentStepData().TargetMachineTag)
                {
                    drawer.SwitchToTab(1);
                    TutorialUIController.Instance.BlinkDrawerButton(true);
                    break;
                }
            }
        }

        NotificationCenter.Instance.DrawerOpened.Notification -= DrawerOpened_Notification;
    }

    public void SubscribeFriendsDrawerOpenedNotification()
    {
        NotificationCenter.Instance.FriendsDrawerOpened.Notification += FriendsDrawerOpened_Notification;
    }

    private void FriendsDrawerOpened_Notification(BaseNotificationEventArgs baseArgs)
    {
        NotificationCenter.Instance.FriendsDrawerOpened.Notification -= FriendsDrawerOpened_Notification;
        SubscribeFriendsDrawerClosedNotification();
        //TutorialUIController.Instance.tutorialArrowUI.Hide();
        TutorialUIController.Instance.InGameCloud.Hide();
    }

    public void SubscribeFriendsDrawerClosedNotification()
    {
        NotificationCenter.Instance.FriendsDrawerClosed.Notification += FriendsDrawerClosed_Notification;
    }

    private void FriendsDrawerClosed_Notification(BaseNotificationEventArgs baseArgs)
    {
        NotificationCenter.Instance.FriendsDrawerClosed.Notification -= FriendsDrawerClosed_Notification;
        SubscribeFriendsDrawerOpenedNotification();
        //TutorialUIController.Instance.tutorialArrowUI.Hide();
        TutorialUIController.Instance.InGameCloud.Show();
    }

    public void SubscribeDrawerOpenedNotification(bool openDrawer = false)
    {
        //Debug.Log("SubscribeDrawerOpenedNotification openDrawer = " + openDrawer);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.DrawerOpened.Notification += DrawerOpened_Notification;

        // added because when player is in drawer then current step stop and go next // Lukasz
        if (UIController.get.drawer.IsVisible)
        {
            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController tutorialController = TutorialController.Instance;
            //RefactoredDrawerController drawer = UIController.get.drawer;
            IDrawer drawer = UIController.get.drawer;
            Image blinkingImage;
            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == tutorialController.GetCurrentStepData().TargetMachineTag)
                {
                    drawer.IncrementMainButtonBadge();
                    drawer.SwitchToTab((int)item.drawerItem.GetInfo.infos.DrawerArea);
                    drawer.CenterToItem(item.drawerItem.GetComponent<RectTransform>());
                    blinkingImage = item.drawerItem.image;
                    TutorialUIController.Instance.BlinkImage(blinkingImage);
                    break;
                }
            }

            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController.Instance.SetCurrentStep();
        }
        //this is for condition in step 16, going into notification caused some issues (multiple steps increments etc, so I moved it here //mikko//)
        else if (openDrawer)
        {
            TutorialUIController.Instance.BlinkDrawerButton(true);
            TutorialController tutorialController = TutorialController.Instance;
            //Debug.Log("Blinking drawer button for doctor");
            if (tutorialController.CurrentTutorialStepTag != StepTag.build_doctor_text)
                instanceNC.DrawerOpened.Notification -= DrawerOpened_Notification;

            Invoke("DelayedOpenDrawer", .5f);
            //RefactoredDrawerController drawer = UIController.get.drawer;
            IDrawer drawer = UIController.get.drawer;
            Image blinkingImage;

            foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == tutorialController.GetCurrentStepData().TargetMachineTag)
                {
                    List<Rotations> single = new List<Rotations>();
                    single.Add(item.rotations);
                    if (!UIController.get.drawer.CheckIsBadgeForItem(item.rotations))
                    {
                        UIController.get.drawer.IncrementMainButtonBadge();
                        try
                        {
                            UIController.get.drawer.AddBadgeForItems(single);
                            UIController.get.drawer.AddBadgeForItems(single);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("TODO:::: " + e.Message);
                        }

                        if (item.drawerItem.GetInfo.infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                            UIController.get.drawer.IncrementTabButtonBadges(1);
                        else if (item.drawerItem.GetInfo.infos.DrawerArea == HospitalAreaInDrawer.Laboratory)
                            UIController.get.drawer.IncrementTabButtonBadges(2);
                        else UIController.get.drawer.IncrementTabButtonBadges(3);
                    }

                    drawer.SwitchToTab((int)item.drawerItem.GetInfo.infos.DrawerArea);
                    drawer.CenterToItem(item.drawerItem.GetComponent<RectTransform>());
                    blinkingImage = item.drawerItem.image;
                    TutorialUIController.Instance.BlinkImage(blinkingImage);
                    break;
                }
            }

            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController.Instance.SetCurrentStep();
        }
        else
            TutorialUIController.Instance.BlinkDrawerButton(true);
    }

    void DelayedOpenDrawer()
    {
        UIController.get.drawer.ToggleVisible();

        TutorialController tc = TutorialController.Instance;
        if (tc.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds ||
            tc.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_again ||
            tc.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_lvl6 ||
            tc.CurrentTutorialStepTag == StepTag.drawer_opened_for_beds_again_lvl6)
        {
            UIController.get.drawer.CenterCameraToArea(0, false);
            UIController.get.drawer.SwitchToTab(0);
        }
    }

    private void HospitalNamed_Notification(HospitalNamedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - podanie nowej nazwy szpitala. Name = " + eventArgs.Name);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.NamedHospital.Notification -= HospitalNamed_Notification;
        GameState.Get().HospitalName =
            eventArgs.Name; //TutorialUIController.Instance.HospitalsNameInputController.TutorialsInputField.text;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();

        NotificationCenter.Instance.HideTutorialsInputField.Invoke(new HideTutorialsInputFieldEventArgs());
        SoundsController.Instance.PlayCheering();
    }

    public void SubscribeHospitalNamedNotification()
    {
        if (!UIController.getHospital.hospitalSignPopup.gameObject.activeSelf)
        {
            //UIController.get.HospitalNamePopUp.Open();
            UIController.getHospital.hospitalSignPopup.Open();
        }

        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.NamedHospital.Notification += HospitalNamed_Notification;
    }

    private void RunTimeStepInfoChanged_Notification(RunTimeStepInfoChangedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - podmiana tekstu w stepie.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.RunTimeStepInfoChanged.Notification -= RunTimeStepInfoChanged_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeRunTimeStepInfoChangedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.RunTimeStepInfoChanged.Notification += RunTimeStepInfoChanged_Notification;
        NotificationCenter.Instance.RunTimeStepInfoChanged.Invoke(new RunTimeStepInfoChangedEventArgs());
    }

    private void HideTutorialsInputField_Notification(HideTutorialsInputFieldEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zamknięcie input field.");
        TutorialUIController.Instance.HideTutorialsInputField();
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.HideTutorialsInputField.Notification -= HideTutorialsInputField_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeHideTutorialsInputFieldNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.HideTutorialsInputField.Notification += HideTutorialsInputField_Notification;
    }

    private void SheetRemove_Notification(SheetRemoveEventArgs eventArgs)
    {
        //Debug.Log("BUIDLING NAME " + eventArgs.BulidingName + " STEP NAME " + TutorialController.Instance.TutorialStepsList[TutorialController.Instance.CurrentTutorialStepIndex].TargetMachineTag);
        if (eventArgs.BulidingName == TutorialController.Instance
                .TutorialStepsList[TutorialController.Instance.CurrentTutorialStepIndex].TargetMachineTag)
        {
            //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - usunięcie płachty.");
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.SheetRemove.Notification -= SheetRemove_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribeSheetRemoveNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.SheetRemove.Notification += SheetRemove_Notification;
    }

    private void PanaceaCollected_Notification(PanaceaCollectedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - panacea zebrana.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PanaceaCollected.Notification -= PanaceaCollected_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePanaceaCollectedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PanaceaCollected.Notification += PanaceaCollected_Notification;
    }

    private void ObjectSelected_Notification(ObjectEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - wybranie obiektu.");
        if (TutorialController.Instance.GetCurrentStepData().TargetMachineTag == eventArgs.obj.Tag)
        {
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.ObjectSelected.Notification -= ObjectSelected_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribeObjectSelectedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ObjectSelected.Notification += ObjectSelected_Notification;
    }

    private void BlueDoctorOfficeAdded_Notification(BlueDoctorOfficeAddedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - budowa niebieskiego gabietu.");
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.BlinkDrawerButton(false);
        instanceNC.BlueDoctorOfficeAdded.Notification -= BlueDoctorOfficeAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeBlueDoctorOfficeNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;

        var blueDocExist = HospitalAreasMapController.HospitalMap.FindRotatableObject("BlueDoc");

        if (!blueDocExist)
        {
            TutorialUIController.Instance.BlinkDrawerButton(true);
            instanceNC.BlueDoctorOfficeAdded.Notification += BlueDoctorOfficeAdded_Notification;
        }
        else
            TutorialController.Instance.IncrementCurrentStep();
    }

    private void XRayAdded_Notification(XRayAddedEventArgs eventArgs)
    {
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.BlinkDrawerButton(false);
        TutorialUIController.Instance.StopBlinking();
        instanceNC.XRayAdded.Notification -= XRayAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeXRayNotificatoin()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.BlinkDrawerButton(true);
        //TutorialUIController.Instance.ShowTutorialArrowUI(d);
        instanceNC.XRayAdded.Notification += XRayAdded_Notification;
    }

    private void YellowDoctorOfficeAdded_Notification(YellowDoctorOfficeAddedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - budowa żółtego gabietu.");
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.BlinkDrawerButton(false);
        TutorialUIController.Instance.StopBlinking();
        instanceNC.YellowDoctorOfficeAdded.Notification -= YellowDoctorOfficeAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeYellowDoctorOfficeNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.YellowDoctorOfficeAdded.Notification += YellowDoctorOfficeAdded_Notification;

        TutorialUIController.Instance.BlinkDrawerButton(true);
        //RefactoredDrawerController drawer = UIController.get.drawer;
        IDrawer drawer = UIController.get.drawer;
        TutorialController tutorialController = TutorialController.Instance;
        Image blinkingImage;

        foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
        {
            if (item.drawerItem.GetInfo.infos.Tag == "YellowDoc")
            {
                drawer.ToggleVisible();
                //drawer.SetTabVisible(0, false);
                //drawer.GetDrawers()[0].ChangeTab(0);
                drawer.SwitchToTab(0);

                List<Rotations> single = new List<Rotations>();
                single.Add(item.rotations);
                if (!UIController.get.drawer.CheckIsBadgeForItem(item.rotations))
                {
                    UIController.get.drawer.IncrementMainButtonBadge();
                    UIController.get.drawer.AddBadgeForItems(single);
                    UIController.getHospital.drawer.AddBadgeForItems(single);
                    UIController.get.drawer.IncrementTabButtonBadges(1);
                }

                //drawer.CenterToItem(item.drawerItem.GetComponent<RectTransform>());
                blinkingImage = item.drawerItem.image;
                TutorialUIController.Instance.BlinkImage(blinkingImage);
                break;
            }
        }
    }

    private void SyrupLabAdded_Notification(SyrupLabAddedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - budowa syrup lab.");
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.BlinkDrawerButton(false);
        instanceNC.SyrupLabAdded.Notification -= SyrupLabAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeSyrupLabAddedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.SyrupLabAdded.Notification += SyrupLabAdded_Notification;

        TutorialUIController.Instance.BlinkDrawerButton(true);
        //RefactoredDrawerController drawer = UIController.get.drawer;
        IDrawer drawer = UIController.get.drawer;
        TutorialController tutorialController = TutorialController.Instance;
        Image blinkingImage;
        foreach (UIDrawerController.DrawerRotationItem item in drawer.AllDrawerItems())
        {
            if (item.drawerItem.GetInfo.infos.Tag == "SyrupLab")
            {
                //drawer.SetTabVisible(1, false);
                //drawer.GetDrawers()[0].ChangeTab(0);

                List<Rotations> single = new List<Rotations>();
                single.Add(item.rotations);
                if (!UIController.get.drawer.CheckIsBadgeForItem(item.rotations))
                {
                    UIController.get.drawer.IncrementMainButtonBadge();
                    UIController.get.drawer.AddBadgeForItems(single);
                    UIController.getHospital.drawer.AddBadgeForItems(single);
                    UIController.get.drawer.IncrementTabButtonBadges(2);
                }

                drawer.CenterToItem(item.drawerItem.GetComponent<RectTransform>());
                blinkingImage = item.drawerItem.image;
                TutorialUIController.Instance.BlinkImage(blinkingImage);
                break;
            }
        }
    }

    private void BlueSyrupProductionStarted_Notification(BlueSyrupProductionStartedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - rozpoczęcie produkcji niebieskiego syropu.");
        NotificationCenter.Instance.BlueSyrupProductionStarted.Notification -= BlueSyrupProductionStarted_Notification;
        TutorialUIController.Instance.StopBlinking();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeBlueSyrupProductionStartedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BlueSyrupProductionStarted.Notification += BlueSyrupProductionStarted_Notification;
    }

    private void DoctorRewardCollected_Notification(DoctorRewardCollectedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zebranie reward za wyleczenie u doktora.");
        TutorialUIController.Instance.InGameCloud.Hide();
        NotificationCenter.Instance.DoctorRewardCollected.Notification -= DoctorRewardCollected_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeDoctorRewardCollectedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.DoctorRewardCollected.Notification += DoctorRewardCollected_Notification;
    }

    private void GreenDoctorOfficeAdded_Notification(GreenDoctorOfficeAddedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - budowa zielonego gabietu.");
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.BlinkDrawerButton(false);
        instanceNC.GreenDoctorOfficeAdded.Notification -= GreenDoctorOfficeAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeGreenDoctorOfficeAddedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.BlinkDrawerButton(true);
        instanceNC.GreenDoctorOfficeAdded.Notification += GreenDoctorOfficeAdded_Notification;
    }

    private void ElixirMixerAdded_Notification(ElixirMixerAddedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - budowa mixera elixirow.");
        TutorialUIController.Instance.SetCurrentlyPointedMachine(eventArgs.obj.gameObject);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialUIController.Instance.StopBlinking();
        TutorialUIController.Instance.BlinkDrawerButton(false);
        TutorialUIController.Instance.InGameCloud.Hide();
        instanceNC.ElixirMixerAdded.Notification -= ElixirMixerAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeElixirMixerAddedNotification()
    {
        TutorialUIController.Instance.BlinkDrawerButton(true);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ElixirMixerAdded.Notification += ElixirMixerAdded_Notification;
    }

    private void BoughtWithDiamonds_Notification(BoughtWithDiamondsEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - użycie diamentu.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BoughtWithDiamonds.Notification -= BoughtWithDiamonds_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeBoughtWithDiamondsNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BoughtWithDiamonds.Notification += BoughtWithDiamonds_Notification;
    }

    private void StaticObjectUpgraded_Notification(StaticObjectUpgradedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - upgrade obiektu.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.StaticObjectUpgraded.Notification -= StaticObjectUpgraded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeStaticObjectUpgradedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.StaticObjectUpgraded.Notification += StaticObjectUpgraded_Notification;
    }

    private void BluePotionsCollected_Notification(BluePotionsCollectedEventArgs eventArgs)
    {
        --AmountOfBlueElixirsToCollect;

        if (AmountOfBlueElixirsToCollect <= 0)
        {
            //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zebranie niebieskiego potiona.");
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.BluePotionsCollected.Notification -= BluePotionsCollected_Notification;

            ProbeTableTool tool = FindObjectOfType<ProbeTableTool>();
            if (tool && probeTableToolCoroutine == null)
                probeTableToolCoroutine = StartCoroutine(WaitForProbeTableToolClose(tool));
        }
        else
            TutorialUIController.Instance.ShowIndictator(TutorialController.Instance.GetAllFullProbeTables()[0]);
    }

    public void SubscribeBluePotionsCollectedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BluePotionsCollected.Notification += BluePotionsCollected_Notification;
    }

    private void ProductionStarted_Notification(ProductionStartedEventArgs eventArgs)
    {
        --AmountOfBlueElixirsToSeed;
        if (AmountOfBlueElixirsToSeed <= 0)
        {
            //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - rozpoczęcie produkcji.");
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.ProductionStarted.Notification -= ProductionStarted_Notification;
            ProbeTableTool tool = FindObjectOfType<ProbeTableTool>();
            if (tool && probeTableToolCoroutine == null)
                probeTableToolCoroutine = StartCoroutine(WaitForProbeTableToolClose(tool));
        }
        else
        {
            //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - rozpoczęcie produkcji. AmountOfBlueElixirToSeed: " + AmountOfBlueElixirsToSeed);
            TutorialUIController.Instance.ShowIndictator(TutorialController.Instance.GetAllEmptyProbeTables()[0]);
        }
    }

    public void SubscribeProductionStartedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ProductionStarted.Notification += ProductionStarted_Notification;
    }

    IEnumerator WaitForProbeTableToolClose(ProbeTableTool tool)
    {
        while (tool && tool.gameObject.activeSelf)
        {
            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("ProbeTableTool closed, incementing tutorial step");
        TutorialController.Instance.IncrementCurrentStep();

        probeTableToolCoroutine = null;
    }

    int elixirsDelivered = 0;
    int yellowElixirsDelivered = 0;

    private void ElixirDelivered_Notification(ElixirDeliveredEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - dostarczenie potiona.");
        NotificationCenter instanceNC = NotificationCenter.Instance;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_deliver)
        {
            TutorialUIController.Instance.StopBlinking();
            ReferenceHolder.GetHospital().Reception.EnableSpawning();
            TutorialController.Instance.IncrementCurrentStep();
            instanceNC.ElixirDelivered.Notification -= ElixirDelivered_Notification;
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_deliver_again)
        {
            ++elixirsDelivered;
            if (elixirsDelivered >= 3)
            {
                TutorialUIController.Instance.StopBlinking();
                TutorialController.Instance.IncrementCurrentStep();
                instanceNC.ElixirDelivered.Notification -= ElixirDelivered_Notification;
            }
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.yellow_doc_elixir_deliver)
        {
            ++yellowElixirsDelivered;
            if (yellowElixirsDelivered >= 3)
            {
                TutorialUIController.Instance.StopBlinking();
                TutorialController.Instance.IncrementCurrentStep();
                instanceNC.ElixirDelivered.Notification -= ElixirDelivered_Notification;
            }
        }
    }

    public void SubscribeElixirDeliveredNotification()
    {
        //Debug.Log("Subskrypcja ElixerDelivered");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ElixirDelivered.Notification += ElixirDelivered_Notification;

        //step with delivering first elixir ever.
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.yellow_doc_elixir_deliver)
        {
            MedicineDatabaseEntry yellowElixir = null;
            foreach (var medicine in ResourcesHolder.Get().medicines.cures)
            {
                if (medicine.type == MedicineType.BaseElixir)
                    yellowElixir = medicine.medicines[11];
            }

            if (yellowElixir == null)
            {
                Debug.Log("Medicine not found in the database");
            }
            else
            {
                int elixirsNeeded = 3 - GameState.Get().GetCureCount(yellowElixir.GetMedicineRef());
                float delay = 0f;
                float delayStep = 0.75f;
                for (int i = 0; i < elixirsNeeded; ++i)
                {
                    GameState.Get().AddResource(yellowElixir.GetMedicineRef(), 1, true, EconomySource.LevelUpGift);
                    Vector3 startPos = Vector3.zero;
                    UIController.get.storageCounter.AddLater(1, true);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Special, startPos, 1, delay,
                        1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), yellowElixir.image, null,
                        () => { UIController.get.storageCounter.Remove(1, true); });
                    delay += delayStep;
                }
            }
        }

        NotificationCenter.Instance.LevelUp.Notification += LevelUp_Notification;
    }

    private void CollectableCollected_Notification(CollectableCollectedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zebranie collectable");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.CollectableCollected.Notification -= CollectableCollected_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeCollectableCollectedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.CollectableCollected.Notification += CollectableCollected_Notification;
    }

    private void MoveRotateRoomStartChanging_Notification(MoveRotateRoomStartChangingEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - rozpoczęcie poruszania obiektem pokoju");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MoveRotateRoomStartChanging.Notification -= MoveRotateRoomStartChanging_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeMoveRotateRoomStartChangingNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MoveRotateRoomStartChanging.Notification += MoveRotateRoomStartChanging_Notification;
    }

    private void MoveRotateRoomEnd_Notification(MoveRotateRoomEndEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zakończenie poruszania obiektem pokoju");
        NotificationCenter instanceNC = NotificationCenter.Instance;

        instanceNC.MoveRotateRoomEnd.Notification -= MoveRotateRoomEnd_Notification;

        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
        //TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeMoveRotateRoomEndNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MoveRotateRoomEnd.Notification += MoveRotateRoomEnd_Notification;
    }

    private void PharmacyOpened_Notification(PharmacyOpenedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - apteka otwarta");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PharmacyOpened.Notification -= PharmacyOpened_Notification;
        CancelInvoke("PharmacyInvokedArrow");

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePharmacyOpenedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PharmacyOpened.Notification += PharmacyOpened_Notification;

        CancelInvoke("PharmacyInvokedArrow");
        Invoke("PharmacyInvokedArrow", 2f);
    }

    private void PharmacyOffersClicked_Notification(PharmacyOffersClickedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - Offers w aptece kliknięte");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PharmacyOffersClicked.Notification -= PharmacyOffersClicked_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePharmacyOffersClickedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PharmacyOffersClicked.Notification += PharmacyOffersClicked_Notification;
    }

    private void PharmacyClosed_Notification(PharmacyClosedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - apteka zamknieta");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PharmacyClosed.Notification -= PharmacyClosed_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePharmacyClosedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PharmacyClosed.Notification += PharmacyClosed_Notification;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.pharmacy_offers)
        {
            TutorialUIController.Instance.BlinkImage(UIController.getHospital.PharmacyPopUp.globalOffers
                .GetComponent<Image>());
            //TutorialUIController.Instance.ShowTutorialArrowUIAfterPopupOpens(UIController.getHospital.PharmacyPopUp.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForGlobalOffersButton, UIController.getHospital.PharmacyPopUp);
        }

        if (!UIController.getHospital.PharmacyPopUp.gameObject.activeSelf)
            instanceNC.PharmacyClosed.Invoke(new PharmacyClosedEventArgs());
    }

    private void QueueExtended_Notification(QueueExtendedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zwiększenie kolejki.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.QueueExtended.Notification -= QueueExtended_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeQueueExtendedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.QueueExtended.Notification += QueueExtended_Notification;
    }

    private void HospitalRoomsExpanded_Notification(HospitalRoomsExpandedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zwiększenie kolejki.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.HospitalRoomsExpanded.Notification -= HospitalRoomsExpanded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeHospitalRoomsExpandedNotification()
    {
        //Debug.Log("SubscribeHospitalRoomsExpandedNotification()");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.HospitalRoomsExpanded.Notification += HospitalRoomsExpanded_Notification;
    }

    private void ReceptionBuilt_Notification(ReceptionBuiltEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ReceptionBuilt.Notification -= ReceptionBuilt_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeReceptionBuiltdNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ReceptionBuilt.Notification += ReceptionBuilt_Notification;
    }

    private void PatientCured_Notification(PatientCuredEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - wyleczenie pacjenta (blue doc to tylko powinien miec).");
        NotificationCenter instanceNC = NotificationCenter.Instance;

        instanceNC.PatientCured.Notification -= PatientCured_Notification;
        TutorialController.Instance.IncrementCurrentStep();
        TutorialUIController.Instance.StopBlinking();
    }

    public void SubscribePatientCuredNotification()
    {
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.doctor_speed_up &&
            GameState.Get().PatientsCount.PatientsCuredCountDoctor >= 1)
        {
            TutorialController.Instance.IncrementCurrentStep();
            return;
        }

        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCured.Notification += PatientCured_Notification;

        if (DoctorHover.GetActive() == null)
            return;

        TutorialUIController.Instance.BlinkImage(DoctorHover.GetActive().GetSpeedUpButton().GetComponent<Image>());
    }

    private void PatientCuredInPatientCard_Notification(PatientCuredInPatientCardEventArgs eventArgs)
    {
        BasePatientAI curedPatient = eventArgs.patient;
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - zamknięcie popupu po kliku.");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCuredInPatientCard.Notification -= PatientCuredInPatientCard_Notification;
        TutorialUIController.Instance.StopBlinking();
        UIController.get.CloseActiveHover();
        TutorialUIController.Instance.StopShowCoroutines();
        TutorialUIController.Instance.HideIndicator();
        delayedIncrementCoroutine =
            StartCoroutine(DelayedIncrementCoroutine(5)); //waitinf for hurray animation on cured patient
    }

    Coroutine delayedIncrementCoroutine;

    private IEnumerator DelayedIncrementCoroutine(float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        TutorialController.Instance.IncrementCurrentStep();
        delayedIncrementCoroutine = null;
    }

    public void StopDelayedIncrementCoroutine()
    {
        try
        {
            if (delayedIncrementCoroutine != null)
                StopCoroutine(delayedIncrementCoroutine);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    public void SubscribePatientCuredInPatientCardNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientCuredInPatientCard.Notification += PatientCuredInPatientCard_Notification;
        Image imageToBlink = UIController.getHospital.PatientCard.CureButton.GetComponent<Image>();
        TutorialUIController.Instance.BlinkImage(imageToBlink);

        //Check if Olivia is present (bug when player leaves and saves the game during 5 seconds after healing Olivia)
        //on level 3 there's only 1 patient in bed possible, so it should be Olivia
        HospitalCharacterInfo[] infos = FindObjectsOfType<HospitalCharacterInfo>();
        if (infos.Length == 0)
            TutorialController.Instance.IncrementCurrentStep();
    }

    private void FinishedBuilding_Notification(FinishedBuildingEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.build_doctor_finish)
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.IncrementCurrentStep();
            TutorialUIController.Instance.StopBlinking();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_waiting_room_finish &&
                 eventArgs.Obj.Tag == "WaitingRoomBlueOrchid")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            Debug.LogError("OLD DRAWER CODE!");
            //((MaternityShopDrawer)UIController.get.drawer).TutorialChangeRoomStatus(true);
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_labor_room_finish &&
                 eventArgs.Obj.Tag == "LabourRoomBlueOrchid")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_lab_build &&
                 eventArgs.Obj.Tag == "SyrupLab")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.yellow_doc_build &&
                 eventArgs.Obj.Tag == "YellowDoc")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.green_doc_build &&
                 eventArgs.Obj.Tag == "GreenDoc")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.diagnose_xray_build &&
                 eventArgs.Obj.Tag == "Xray")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_mixer_text &&
                 eventArgs.Obj.Tag == "ElixirLab")
        {
            instanceNC.FinishedBuilding.Notification -= FinishedBuilding_Notification;
            TutorialController.Instance.ConfirmConditionExecution();
            TutorialController.Instance.SetCurrentStep();
        }
    }

    public void SubscribeFinishedBuildingNotification()
    {
        var building =
            HospitalAreasMapController.HospitalMap.FindRotatableObject(TutorialController.Instance.GetCurrentStepData()
                .TargetMachineTag);
        if (building != null && building.BuildStartTime <= 0)
        {
            TutorialController.Instance.IncrementCurrentStep();
            return;
        }

        //Debug.Log("Subskrypcja do notyfikacji Koniec budowy");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.FinishedBuilding.Notification += FinishedBuilding_Notification;
    }

    private void DummyRemoved_Notification(DummyRemovedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - usunięcie atrapy. Tag: " + eventArgs.Obj.Tag);
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialController tc = TutorialController.Instance;

        if (tc.CurrentTutorialStepTag == StepTag.build_doctor_unpack)
        {
            if (eventArgs.Obj.Tag == "BlueDoc")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.maternity_waiting_room_unpack)
        {
            if (eventArgs.Obj.Tag == "WaitingRoomBlueOrchid")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                Debug.LogError("OLD DRAWER CODE!");
                //((MaternityShopDrawer)UIController.get.drawer).TutorialChangeRoomStatus(true);
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.maternity_labor_room_unpack)
        {
            if (eventArgs.Obj.Tag == "LabourRoomBlueOrchid")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.syrup_lab_unpack)
        {
            if (eventArgs.Obj.Tag == "SyrupLab")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.yellow_doc_build)
        {
            if (eventArgs.Obj.Tag == "YellowDoc")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.green_doc_unpack)
        {
            if (eventArgs.Obj.Tag == "GreenDoc")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.diagnose_xray_unpack)
        {
            if (eventArgs.Obj.Tag == "Xray")
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.Vip_Room_Unpack)
        {
            if (eventArgs.ExternalObject is VipRoom)
            {
                instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;
                tc.IncrementCurrentStep();
            }
        }
    }

    public void SubscribeDummyRemovedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.DummyRemoved.Notification += DummyRemoved_Notification;
    }

    private void PatientZeroOpen_Notification(PatientZeroOpenEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_green_patient_popup_open &&
            eventArgs.CharacterInfo.clinicDisease.id == 2)
        {
            instanceNC.PatientZeroOpen.Notification -= PatientZeroOpen_Notification;
            //instanceNC.DummyRemoved.Notification -= DummyRemoved_Notification;  //to jump over the next steps related to placing the green doc(Translated from Polish)
            TutorialController.Instance.IncrementCurrentStep();
        }
        else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_yellow_patient_popup_open &&
                 eventArgs.CharacterInfo.clinicDisease.id == 1)
        {
            instanceNC.PatientZeroOpen.Notification -= PatientZeroOpen_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribePatientZeroOpenedNotification()
    {
        StopDelayedIncrementCoroutine();
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientZeroOpen.Notification += PatientZeroOpen_Notification;

        //Debug.LogError("That");
        //instanceNC.DummyRemoved.Notification += DummyRemoved_Notification;  //to jump over the next steps related to placing the green doc(Translated from Polish)
    }

    private void PatientZeroClose_Notification(PatientZeroClosedEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientZeroClosed.Notification -= PatientZeroClose_Notification;
        instanceNC.DummyRemoved.Notification -=
            DummyRemoved_Notification; //to jump over the next steps related to placing the green doc(Translated from Polish)
        TutorialController.Instance.IncrementCurrentStep();
        UIController.get.DestroyAllHighlights();
    }

    public void SubscribePatientZeroClosedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.PatientZeroClosed.Notification += PatientZeroClose_Notification;
        instanceNC.DummyRemoved.Notification +=
            DummyRemoved_Notification; //to jump over the next steps related to placing the green doc(Translated from Polish)
    }

    private void KidsClicked_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsClicked.Notification -= KidsClicked_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    /// <summary>
    /// INFO: this isn't used currently. 
    /// </summary>
    public void SubscribeKidsClickedNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsClicked.Notification += KidsClicked_Notification;
        //if (HospitalAreasMapController.HospitalMap.playgroud.DummyKidsCount == 0)
        //{
        //    //ReferenceHolder.GetHospital().ClinicAI.SpawnTimmyOnPosition(new Vector2i(27, 48));
        //}
    }

    private void WiseHospitalLoaded_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.WiseHospitalLoaded.Notification -= WiseHospitalLoaded_Notification;
        NotificationCenter.Instance.FriendsDrawerOpened.Notification -= FriendsDrawerOpened_Notification;
        NotificationCenter.Instance.FriendsDrawerClosed.Notification -= FriendsDrawerClosed_Notification;
        TutorialUIController.Instance.BlinkFriendsButton(false);
        TutorialUIController.Instance.StopBlinking();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeWiseHospitalLoadedNotification()
    {
        //Debug.Log("Subscribing to FollowBob spawned");
        if (TutorialController.Instance.WiseVisitedThisSession)
        {
            //this happens when you visit wise, your saves stop updating so the last save is from this step, and all the visiting steps loop indefinitely
            TutorialController.Instance.SetStep(StepTag.emma_about_wise);
            return;
        }

        SubscribeFriendsDrawerOpenedNotification();
        TutorialUIController.Instance.BlinkFriendsButton(true);
        ReferenceHolder.GetHospital().drWiseCardController.BlinkDrWise();
        NotificationCenter.Instance.WiseHospitalLoaded.Notification += WiseHospitalLoaded_Notification;
    }

    private void HomeHospitalLoaded_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.HomeHospitalLoaded.Notification -= HomeHospitalLoaded_Notification;

        //stop blinking return button
        TutorialUIController.Instance.StopBlinking();

        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeHomeHospitalLoaded()
    {
        //Debug.Log("Subscribing to FollowBob spawned");
        NotificationCenter.Instance.HomeHospitalLoaded.Notification += HomeHospitalLoaded_Notification;

        if (VisitingController.Instance.IsVisiting)
        {
            TutorialUIController.Instance.BlinkImage(UIController.get.returnButton.GetComponent<Image>());
            //blink return button
        }
        else
            NotificationCenter.Instance.HomeHospitalLoaded.Invoke(new BaseNotificationEventArgs());
    }

    private void PackageCollected_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PackageCollected.Notification -= PackageCollected_Notification;
        ((HospitalCasesManager)AreaMapController.Map.casesManager).StartCounting();
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribePackageCollected()
    {
        //Debug.Log("Subscribing to FollowBob spawned");
        NotificationCenter.Instance.PackageCollected.Notification += PackageCollected_Notification;

        if (((HospitalCasesManager)AreaMapController.Map.casesManager).casesStack.Count == 0)
            NotificationCenter.Instance.PackageCollected.Invoke(new BaseNotificationEventArgs());
    }

    private void PackageArrived_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PackageArrived.Notification -= PackageArrived_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePackageArrived()
    {
        //Debug.LogError("SubscribePackageArrived");
        NotificationCenter.Instance.PackageArrived.Notification += PackageArrived_Notification;
        ((HospitalCasesManager)AreaMapController.Map.casesManager).StartDeliverCarTutorial();
    }

    private void PackageClicked_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PackageClicked.Notification -= PackageClicked_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePackageClicked()
    {
        //Debug.Log("Subscribing to FollowBob spawned");
        NotificationCenter.Instance.PackageClicked.Notification += PackageClicked_Notification;
    }

    RemovableDecoration removableDeco = null;
    int counter = 0;

    private void PatioElementCleared_Notification(BaseNotificationEventArgs eventArgs)
    {
        removableDeco = FindObjectOfType<RemovableDecoration>();
        //Debug.LogError("removableDeco = " + removableDeco);
        if (removableDeco != null && removableDeco.gameObject.activeSelf)
        {
            ++counter;
            Vector3 offset = Vector3.zero;
            if (counter == 2)
                offset.x = 1f;
            else if (counter == 6)
            {
                offset.x = .75f;
                offset.y = -1.5f;
            }

            TutorialUIController.Instance.ShowIndictator(removableDeco.gameObject, offset);
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(removableDeco.transform.position, 1f, true);
            ReferenceHolder.Get().engine.MainCamera.SmoothZoom(8, 1, false);
            HospitalAreasMapController.HospitalMap.removableDeco = removableDeco;
            //Debug.LogError("Camera should be moving now to: " + removableDeco.transform.position);
        }
        else
        {
            removableDeco = null;

            //ShopRoomInfo[] decos = HospitalTutorialController.HospitalTutorialInstance.grantedDecorations;
            //GameState.Get().AddToObjectStored(decos[0], 1);
            //GameState.Get().AddToObjectStored(decos[1], 1);
            //GameState.Get().AddToObjectStored(decos[2], 1);
            //Vector3 startPoint = Vector3.zero;
            //ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, 1, 0f, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[0].ShopImage, null, null);
            //ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, 1, 0.35f, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[1].ShopImage, null, null);
            //ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Drawer, startPoint, 1, 0.7f, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[2].ShopImage, null, null);

            NotificationCenter.Instance.PatioElementCleared.Notification -= PatioElementCleared_Notification;
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribePatioElementCleared()
    {
        //check if something went wrong and if there are no removable decorations increment step
        if (FindObjectsOfType<RemovableDecoration>().Length == 0)
        {
            TutorialController.Instance.IncrementCurrentStep();
            return;
        }

        NotificationCenter.Instance.PatioElementCleared.Notification += PatioElementCleared_Notification;
        NotificationCenter.Instance.PatioElementCleared.Invoke(new BaseNotificationEventArgs());
    }

    private void VipPopUpOpen_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.VipPopUpOpen.Notification -= VipPopUpOpen_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeVipPopUpOpen()
    {
        NotificationCenter.Instance.VipPopUpOpen.Notification += VipPopUpOpen_Notification;
    }

    private void BoxPopUpClosed_Notification(BaseNotificationEventArgs eventArgs)
    {
        TutorialUIController.Instance.StopBlinking();
        NotificationCenter.Instance.BoxPopupClosed.Notification -= BoxPopUpClosed_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeBoxPopUpClosed()
    {
        TutorialUIController.Instance.BlinkImage(UIController.get.BoxButton.GetComponent<Image>(), 1.2f, false);
        NotificationCenter.Instance.BoxPopupClosed.Notification += BoxPopUpClosed_Notification;

        SaveSynchronizer.Instance.MarkToSave(SavePriorities
            .SaveThreshold); //this is here because UnboxingPoUpController.cs instant saves and triggers this which leads to tutorial lock when player leaves the game in previous step
    }

    private void PatioDecorationsAdded_Notification(BaseNotificationEventArgs eventArgs)
    {
        int decoCount = FindObjectsOfType<Decoration>().Length;
        //Debug.LogError("DecoAdded. count = " + decoCount);
        if (decoCount >= 4)
        {
            TutorialUIController.Instance.InGameCloud.Hide();
            //TutorialUIController.Instance.tutorialArrowUI.Hide();
            NotificationCenter.Instance.PatioDecorationsAdded.Notification -= PatioDecorationsAdded_Notification;
            TutorialUIController.Instance.BlinkDrawerButton(false);
            TutorialController.Instance.IncrementCurrentStep();
        }
    }

    public void SubscribePatioDecorationsAdded()
    {
        //Debug.LogError("SubscribePatioDecorationsAdded");
        TutorialUIController.Instance.BlinkDrawerButton(true);
        NotificationCenter.Instance.PatioDecorationsAdded.Notification += PatioDecorationsAdded_Notification;
        NotificationCenter.Instance.PatioDecorationsAdded.Invoke(new BaseNotificationEventArgs());
    }

    private void WisePharmacyVisited_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PharmacyClosed.Notification -= WisePharmacyVisited_Notification;
        //TutorialUIController.Instance.ShowTutorialArrowUI(UIController.get.returnButton.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForButton, 0, TutorialUIController.TutorialPointerAnimationType.tap);
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
        //TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeWisePharmacyVisited()
    {
        NotificationCenter.Instance.PharmacyClosed.Notification += WisePharmacyVisited_Notification;
        NotificationCenter.Instance.PharmacyClosed.Notification += ShowWiseButtonArrow;
        TutorialUIController.Instance.BlinkImage(UIController.get.returnButton.GetComponent<Image>());
    }

    private void ShowWiseButtonArrow(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PharmacyClosed.Notification -= ShowWiseButtonArrow;
    }

    private void VIPTeaseMedicopter_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("VIPTeaseMedicopter_Notification");
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        NotificationCenter.Instance.VIPTeaseMedicopter.Notification -= VIPTeaseMedicopter_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeVIPTeaseMedicopterNotification()
    {
        //Debug.LogError("Subscribing to VIPTeaseMedicopter");
        NotificationCenter.Instance.VIPTeaseMedicopter.Notification += VIPTeaseMedicopter_Notification;
        HeliController.instance.EngineOn();
        ReferenceHolder.Get().engine.MainCamera.FollowGameObject(HospitalAreasMapController.HospitalMap.vipRoom
            .GetComponent<VIPSystemManager>().Helipod.transform);
    }

    private void VIPMedicopterStarted_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("VIPMedicopterStarted_Notification");
        NotificationCenter.Instance.VIPMedicopterStarted.Notification -= VIPMedicopterStarted_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
        ReferenceHolder.Get().engine.MainCamera.FollowGameObject(HospitalAreasMapController.HospitalMap.vipRoom
            .GetComponent<VIPSystemManager>().Helipod.transform);
    }

    public void SubscribeVIPMedicopterStartedNotification()
    {
        //Debug.LogError("Subscribing to VIPMedicopterStartedNotification");
        NotificationCenter.Instance.VIPMedicopterStarted.Notification += VIPMedicopterStarted_Notification;
    }

    private void VIPSpawned_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("VIPSpawned_Notification");
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        NotificationCenter.Instance.VIPSpawned.Notification -= VIPSpawned_Notification;
        TutorialController.Instance.IncrementCurrentStep();
        ReferenceHolder.Get().engine.MainCamera
            .FollowGameObject(ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>().currentVip.transform);
    }

    public void SubscribeVIPSpawnedNotification()
    {
        //Debug.LogError("Subscribing to SubscribeVIPSpawnedNotification.");
        NotificationCenter.Instance.VIPSpawned.Notification += VIPSpawned_Notification;
    }

    private void VIPReachedBed_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("VIPReachedBed_Notification");
        NotificationCenter.Instance.VIPReachedBed.Notification -= VIPReachedBed_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();

        //Debug.LogError("ShowIndicator");
        TutorialUIController.Instance.ShowIndictator(
            ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>().currentVip, Vector3.zero, true);
    }

    public void SubscribeVIPReachedBedNotification()
    {
        //Debug.LogError("SubscribeVIPReachedBedNotification");
        var currentVipObj = ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>().currentVip;
        if (currentVipObj != null)
        {
            NotificationCenter.Instance.VIPReachedBed.Notification += VIPReachedBed_Notification;

            VIPPersonController vip = currentVipObj.GetComponent<VIPPersonController>();
            //Debug.LogError("vip state = " + vip.Person.State.GetType().ToString());
            if (vip && vip.Person.State.GetType() == typeof(VIPPersonController.InBed) &&
                !HospitalAreasMapController.HospitalMap.VisitingMode)
                NotificationCenter.Instance.VIPReachedBed.Invoke(new BaseNotificationEventArgs());
            else
                ReferenceHolder.Get().engine.MainCamera.FollowGameObject(vip.transform);
        }
        else
            NotificationCenter.Instance.VIPReachedBed.Invoke(new BaseNotificationEventArgs());
    }

    public void SubscribeVIPNotCuredNotification()
    {
        //Debug.LogError("SubscribeVIPNotCuredNotification");
        NotificationCenter.Instance.VipNotCured.Notification += VIPReachedBed_Notification;
        GameObject vip = ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>().currentVip;

        if (vip != null)
        {
            if (vip.GetComponent<VIPPersonController>().Person.State.GetType() == typeof(VIPPersonController.InBed))
                NotificationCenter.Instance.VipNotCured.Invoke(new BaseNotificationEventArgs());
        }
        else
            TutorialController.Instance.IncrementCurrentStep();
    }

    private void MedicineExistInStorage_Notification(MedicineExistInStorageEventArgs eventArgs)
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        //Debug.Log ("Med exist: " + eventArgs.medicineRate);

        if ((eventArgs.medicineType.type == MedicineType.BaseElixir) && (eventArgs.medicineType.id == 1) &&
            (TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_lab_elixirs_missing))
        {
            if ((eventArgs.medicineRate) > 2)
            {
                //Debug.Log ("Wychwycenie wystąpienia " + (eventArgs.medicineRate + 1).ToString () + " " + eventArgs.medicineType + " w storage");
                instanceNC.MedicineExistInStorage.Notification -= MedicineExistInStorage_Notification;
                TutorialController.Instance.IncrementCurrentStep();
                return;
            }
        }
        else if ((eventArgs.medicineType.type == MedicineType.Syrop) && (eventArgs.medicineType.id == 1) &&
                 (TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_in_production))
        {
            if ((eventArgs.medicineRate) > 0)
            {
                //Debug.Log ("Wychwycenie wystąpienia " + (eventArgs.medicineRate + 1).ToString () + " " + eventArgs.medicineType + " w storage");
                instanceNC.MedicineExistInStorage.Notification -= MedicineExistInStorage_Notification;
                TutorialController.Instance.IncrementCurrentStep();
                TutorialUIController.Instance.StopBlinking();
                return;
            }
        }
    }

    public void SubscribeMedicineExistInStorageNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.MedicineExistInStorage.Notification += MedicineExistInStorage_Notification;

        if (TutorialController.Instance.CurrentTutorialStepTag != StepTag.syrup_in_production)
        {
            MedicineRef producedElixir = new MedicineRef(MedicineType.BaseElixir, 1); // blue elixir
            NotificationCenter.Instance.MedicineExistInStorage.Invoke(
                new MedicineExistInStorageEventArgs(producedElixir, GameState.Get().GetCureCount(producedElixir)));
        }
        else
        {
            MedicineRef producedElixir = new MedicineRef(MedicineType.Syrop, 1); //itchy throat syrup
            NotificationCenter.Instance.MedicineExistInStorage.Invoke(
                new MedicineExistInStorageEventArgs(producedElixir, GameState.Get().GetCureCount(producedElixir)));
            if (MedicineProductionHover.GetActive() != null)
                MedicineProductionHover.GetActive().BlinkSpeedUpButton();
            else
                Debug.LogError("MedicineProductionHover is null");

            ////Debug.LogError("Test");
        }
    }

    private void ObjectExistOnLevel_Notification(ObjectExistOnLevelEventArgs eventArgs)
    {
        //Debug.Log("ObjectExistOnLevel_Notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        TutorialController tc = TutorialController.Instance;

        if (tc.CurrentTutorialStepTag == StepTag.elixir_mixer_text ||
            tc.CurrentTutorialStepTag == StepTag.elixir_mixer_add)
        {
            var elixirMaker = HospitalAreasMapController.HospitalMap.FindRotatableObject("ElixirLab");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (elixirMaker)
                tc.IncrementCurrentStep();
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.syrup_lab_add)
        {
            var syrupLab = HospitalAreasMapController.HospitalMap.FindRotatableObject("SyrupLab");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (syrupLab)
            {
                TutorialUIController.Instance.BlinkDrawerButton(false);
                TutorialUIController.Instance.StopBlinking();
                tc.IncrementCurrentStep();
            }
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.syrup_lab_build)
        {
            var syrupLab = HospitalAreasMapController.HospitalMap.FindRotatableObject("SyrupLab");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
            if (syrupLab)
            {
                if (syrupLab.GetState() == RotatableObject.State.building)
                {
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else if (syrupLab.GetState() == RotatableObject.State.working ||
                         syrupLab.GetState() == RotatableObject.State.waitingForUser)
                {
                    TutorialUIController.Instance.BlinkDrawerButton(false);
                    TutorialUIController.Instance.StopBlinking();
                    tc.IncrementCurrentStep();
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.syrup_lab_unpack)
        {
            var syrupLab = HospitalAreasMapController.HospitalMap.FindRotatableObject("SyrupLab");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (syrupLab)
            {
                if (syrupLab.GetState() == RotatableObject.State.building ||
                    syrupLab.GetState() == RotatableObject.State.waitingForUser)
                {
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else if (syrupLab.GetState() == RotatableObject.State.working)
                    tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.yellow_doc_add)
        {
            var yellowBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("YellowDoc");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (yellowBuilding)
            {
                TutorialUIController.Instance.BlinkDrawerButton(false);
                TutorialUIController.Instance.StopBlinking();
                tc.IncrementCurrentStep();
            }
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.yellow_doc_build)
        {
            var yellowBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("YellowDoc");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
            if (yellowBuilding)
            {
                if (yellowBuilding.GetState() == RotatableObject.State.building)
                {
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else if (yellowBuilding.GetState() == RotatableObject.State.working ||
                         yellowBuilding.GetState() == RotatableObject.State.waitingForUser)
                {
                    TutorialUIController.Instance.BlinkDrawerButton(false);
                    TutorialUIController.Instance.StopBlinking();
                    tc.IncrementCurrentStep();
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.yellow_doc_unpack)
        {
            var yellowBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("YellowDoc");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (yellowBuilding.GetState() == RotatableObject.State.building ||
                yellowBuilding.GetState() == RotatableObject.State.waitingForUser)
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
            else if (yellowBuilding.GetState() == RotatableObject.State.working)
                tc.IncrementCurrentStep();
        }
        else if (tc.CurrentTutorialStepTag == StepTag.green_doc_add ||
                 tc.CurrentTutorialStepTag == StepTag.green_doc_add_text)
        {
            var greenDocBuildingExist = HospitalAreasMapController.HospitalMap.FindRotatableObject("GreenDoc");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (greenDocBuildingExist)
            {
                TutorialUIController.Instance.BlinkDrawerButton(false);
                TutorialUIController.Instance.StopBlinking();
                tc.IncrementCurrentStep();
            }
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.green_doc_build)
        {
            var greenDocBuildingExist = HospitalAreasMapController.HospitalMap.FindRotatableObject("GreenDoc");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
            if (greenDocBuildingExist)
            {
                if (greenDocBuildingExist.GetState() == RotatableObject.State.building)
                {
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else if (greenDocBuildingExist.GetState() == RotatableObject.State.working ||
                         greenDocBuildingExist.GetState() == RotatableObject.State.waitingForUser)
                {
                    TutorialUIController.Instance.BlinkDrawerButton(false);
                    TutorialUIController.Instance.StopBlinking();
                    tc.IncrementCurrentStep();
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.green_doc_unpack)
        {
            var greenBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("GreenDoc");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (greenBuilding.GetState() == RotatableObject.State.building ||
                greenBuilding.GetState() == RotatableObject.State.waitingForUser)
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
            else if (greenBuilding.GetState() == RotatableObject.State.working)
                tc.IncrementCurrentStep();
        }

        else if (tc.CurrentTutorialStepTag == StepTag.emma_on_Xray)
        {
            var xRayBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("Xray");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (xRayBuilding)
            {
                TutorialUIController.Instance.BlinkDrawerButton(false);
                TutorialUIController.Instance.StopBlinking();
                tc.IncrementCurrentStep();
            }
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.diagnose_xray_add)
        {
            var xRayBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("Xray");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (xRayBuilding)
            {
                TutorialUIController.Instance.BlinkDrawerButton(false);
                TutorialUIController.Instance.StopBlinking();
                tc.IncrementCurrentStep();
            }
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.diagnose_xray_build)
        {
            var xRayBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("Xray");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
            if (xRayBuilding)
            {
                if (xRayBuilding.GetState() == RotatableObject.State.building)
                {
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else if (xRayBuilding.GetState() == RotatableObject.State.working ||
                         xRayBuilding.GetState() == RotatableObject.State.waitingForUser)
                {
                    TutorialUIController.Instance.BlinkDrawerButton(false);
                    TutorialUIController.Instance.StopBlinking();
                    tc.IncrementCurrentStep();
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.diagnose_xray_unpack)
        {
            var xRayBuilding = HospitalAreasMapController.HospitalMap.FindRotatableObject("Xray");
            //Debug.LogError("xRayBuilding = " + xRayBuilding);
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
            if (xRayBuilding)
            {
                RotatableObject.State state = xRayBuilding.GetState();
                if (state == RotatableObject.State.waitingForUser || state == RotatableObject.State.building)
                {
                    //Debug.LogError("This");
                    instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else if (state == RotatableObject.State.working)
                {
                    //Debug.LogError("That");
                    instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
                    tc.IncrementCurrentStep();
                }
                else
                    Debug.LogError("Thaaaaaaaat " + state);
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.new_cures_2)
        {
            var pillMaker = HospitalAreasMapController.HospitalMap.FindRotatableObject("PillLab");
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;

            if (pillMaker)
                tc.IncrementCurrentStep();
            else
            {
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else
        {
            instanceNC.ObjectExistOnLevel.Notification -= ObjectExistOnLevel_Notification;
            tc.IncrementCurrentStep();
        }
    }

    public void SubscribeObjectExistOnLeveldNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ObjectExistOnLevel.Notification += ObjectExistOnLevel_Notification;

        NotificationCenter.Instance.ObjectExistOnLevel.Invoke(new ObjectExistOnLevelEventArgs());
    }

    private void KidsUIOpen_Notification(KidsUIOpenEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - KidsUIOpen");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsUIOpen.Notification -= KidsUIOpen_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeKidsUIOpenNotification()
    {
        //Debug.Log("Subskrypcja do notyfikacji KidsUIOpen");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsUIOpen.Notification += KidsUIOpen_Notification;
    }

    private void KidsUIClosed_Notification(KidsUIClosedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - KidsUIClosed");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsUIClosed.Notification -= KidsUIClosed_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeKidsUIClosedNotification()
    {
        //Debug.Log("Subskrypcja do notyfikacji KidsUIClosed");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsUIClosed.Notification += KidsUIClosed_Notification;
    }

    private void KidsRoomUnlocked_Notification(KidsRoomUnlockedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - KidsRoomUnlocked");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            GameState.Get().canSpawnKids = true;

        instanceNC.KidsRoomUnlocked.Notification -= KidsRoomUnlocked_Notification;

        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeKidsRoomUnlockedNotification()
    {
        //Debug.Log("Subskrypcja do notyfikacji KidsRoomUnlocked");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidsRoomUnlocked.Notification += KidsRoomUnlocked_Notification;
    }

    private void KidPatientSpawned_Notification(KidPatientSpawnedEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - KidPatientSpawned");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidPatientSpawned.Notification -= KidPatientSpawned_Notification;

        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeKidPatientSpawnedNotification()
    {
        //Debug.Log("Subskrypcja do notyfikacji KidPatientSpawned");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidPatientSpawned.Notification += KidPatientSpawned_Notification;
        instanceNC.KidPatientSpawned.Invoke(new KidPatientSpawnedEventArgs());
    }

    public void SubscribeOpenWelcomePopUp()
    {
        NotificationCenter.Instance.OpenWelcomePopUp.Notification += OpenWelcomePopUp_Notification;
        NotificationCenter.Instance.OpenWelcomePopUp.Invoke(new BaseNotificationEventArgs());
    }

    private void OpenWelcomePopUp_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.OpenWelcomePopUp.Notification -= OpenWelcomePopUp_Notification;
        //UIController.getHospital.welcomePopupController.Open();
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeCloseWelcomePopUp()
    {
        NotificationCenter.Instance.CloseWelcomePopUp.Notification += CloseWelcomePopUp_Notification;
    }

    private void CloseWelcomePopUp_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.CloseWelcomePopUp.Notification -= CloseWelcomePopUp_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeEpidemyClicked()
    {
        NotificationCenter.Instance.EpidemyClicked.Notification += EpidemyClicked_Notification;
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.epidemy_starting)
            EpidemyArrowDelayed();
    }

    private void EpidemyClicked_Notification(BaseNotificationEventArgs eventArgs)
    {
        CancelInvoke("EpidemyArrowDelayed");
        NotificationCenter.Instance.EpidemyClicked.Notification -= EpidemyClicked_Notification;
        TutorialController.Instance.IncrementCurrentStep();
        TutorialUIController.Instance.InGameCloud.Hide();
    }

    public void SubscribeEpidemyStarting()
    {
        NotificationCenter.Instance.EpidemyStarting.Notification += EpidemyStarting_Notification;
        if (ReferenceHolder.GetHospital().Epidemy.Outbreak &&
            !ReferenceHolder.GetHospital().Epidemy.EpidemyObject.IsRenovating())
            NotificationCenter.Instance.EpidemyStarting.Invoke(new BaseNotificationEventArgs());
    }

    private void EpidemyStarting_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("EpidemyStarting_Notification");
        Invoke("EpidemyArrowDelayed", 6f);
        NotificationCenter.Instance.EpidemyStarting.Notification -= EpidemyStarting_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    void EpidemyArrowDelayed()
    {
        TutorialUIController.Instance.ShowIndictator(ReferenceHolder.GetHospital().Epidemy.gameObject, Vector3.zero,
            false, false);
    }

    private void ExpAmountChanged_Notification(ExpAmountChangedEventArgs eventArgs)
    {
        //Debug.LogError("ExpAmountChanged_Notification");
        TutorialController tc = TutorialController.Instance;
        if (tc.CurrentTutorialStepTag == StepTag.patio_tidy_5)
        {
            int expReq = 25;

            if (eventArgs.level >= 4 && eventArgs.expOnLevel >= expReq)
            {
                //Debug.LogError("ExpAmountChanged_Notification done");
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.green_doc_unpack)
        {
            RotatableObject greenDoc = HospitalAreasMapController.HospitalMap.FindRotatableObject("GreenDoc");

            if ((eventArgs.level >= 5 && eventArgs.expOnLevel >= 50) ||
                (greenDoc && greenDoc.GetState() == RotatableObject.State.working))
            {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.new_probe_tables)
        {
            if (eventArgs.level >= 5 && eventArgs.expOnLevel >= 65)
            {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;

                ProbeTable[] pts = FindObjectsOfType<ProbeTable>();
                Debug.LogWarning("PROBE TABLES ON MAP = " + pts.Length);

                if (pts.Length > 0 && pts.Length < 9)
                {
                    Debug.Log("Moving camera on probae table: " + pts[0]);
                    ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(pts[0].transform.position, 1f, true);
                    ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7, 1, false);

                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
                else
                    tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.package_arrive)
        {
            int expReq = 120;

            if (eventArgs.level >= 6 && eventArgs.expOnLevel >= expReq)
            {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.bubble_boy_arrow) /*StepTag.bubble_boy_intro)*/
        {
            int expReq = 160;

            if (eventArgs.level >= 6 && eventArgs.expOnLevel >= expReq)
            {
                TutorialUIController.Instance.ShowIndictator(new Vector3(21f, 0f, 35.5f));
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                ReferenceHolder.GetHospital().bubbleBoyCharacterAI.Initialize();
                HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState =
                    ExternalRoom.EExternalHouseState.enabled;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
                //tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.beds_room_text_lvl6)
        {
            int bedRoomsCount = FindObjectsOfType<HospitalRoom>().Length;
            if (bedRoomsCount > 1)
            {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.SetStep(StepTag.level_7);
            }
            else
            {
                int expReq = 210;

                if (eventArgs.level >= 6 && eventArgs.expOnLevel >= expReq)
                {
                    NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.beds_room_text_again_lvl6)
        {
            tc.SetStep(StepTag.level_7);

            //int bedRoomsCount = FindObjectsOfType<HospitalRoom>().Length;
            //if (bedRoomsCount > 1)
            //{
            //    NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
            //    tc.SetStep(StepTag.level_7);
            //}
            //else
            //{
            //    if (eventArgs.level >= 6 && eventArgs.expOnLevel >= 200)
            //    {
            //        NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
            //        tc.ConfirmConditionExecution();
            //        tc.SetCurrentStep();
            //    }
            //}
        }
        /*else if (tc.CurrentTutorialStepTag == StepTag.treasure_first_spawn)
        {
            int expReq = 75;

            //Debug.LogError("treasure_first_spawn this");
            if (eventArgs.level >= 7 && eventArgs.expOnLevel >= expReq) {
                //Debug.LogError("treasure_first_spawn that");

                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
                HospitalAreasMapController.Map.treasureManager.ShowTreasureFromTutorial();
            }
        } 
        else if (tc.CurrentTutorialStepTag == StepTag.daily_quests_unlocked)
        {
            int expReq = 75;

            if (eventArgs.level >= 7 && eventArgs.expOnLevel >= expReq) {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        } 
        */
        else if (tc.CurrentTutorialStepTag == StepTag.vip_flyby_emma_1)
        {
            int expReq = 150;

            //Debug.LogError("vip_flyby_emma_1 before if");
            if (eventArgs.level >= 7 && eventArgs.expOnLevel >= expReq)
            {
                //Debug.LogError("vip_flyby_emma_1 if");
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.beds_room_text)
        {
            int bedRoomsCount = FindObjectsOfType<HospitalRoom>().Length;
            if (bedRoomsCount > 2)
            {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.SetStep(StepTag.level_8);
            }
            else
            {
                int expReq = 200;

                if (eventArgs.level >= 7 && eventArgs.expOnLevel >= expReq)
                {
                    NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                }
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.beds_room_text_again)
        {
            tc.SetStep(StepTag.level_8);

            //int bedRoomsCount = FindObjectsOfType<HospitalRoom>().Length;
            //if (bedRoomsCount > 2)
            //{
            //    NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
            //    tc.SetStep(StepTag.level_8);
            //}
            //else
            //{
            //    if (eventArgs.level >= 7 && eventArgs.expOnLevel >= 200)
            //    {
            //        NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
            //        tc.ConfirmConditionExecution();
            //        tc.SetCurrentStep();
            //    }
            //}
        }
        else if (tc.CurrentTutorialStepTag == StepTag.Vip_Room_Reminder)
        {
            if (HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState ==
                ExternalRoom.EExternalHouseState.waitingForRenew)
            {
                if (eventArgs.level >= 9 && eventArgs.expOnLevel >= 100)
                {
                    NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                    tc.ConfirmConditionExecution();
                    tc.SetCurrentStep();
                    Debug.LogError("Show indicator on VIP ROOM");
                    TutorialUIController.Instance.ShowIndictator(ReferenceHolder.GetHospital().vipSpawner.gameObject,
                        Vector3.zero);
                }
            }
            else if (HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState ==
                     ExternalRoom.EExternalHouseState.disabled)
            {
                Debug.LogError("Why VIP room is disabled at level 9?");
            }
            else
            {
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.IncrementCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.wise_1)
        {
            int expReq = 30;

            if (eventArgs.level >= 6 && eventArgs.expOnLevel >= expReq)
            {
                //Debug.LogError("ExpAmountChanged_Notification done");
                NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
                tc.ConfirmConditionExecution();
                tc.SetCurrentStep();
            }
        }
        else if (tc.CurrentTutorialStepTag == StepTag.bacteria_newspaper)
        {
            NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
            tc.ConfirmConditionExecution();
            tc.SetCurrentStep();
        }
        else if (tc.CurrentTutorialStepTag == StepTag.bacteria_emma_micro_1)
        {
            NotificationCenter.Instance.ExpAmountChanged.Notification -= ExpAmountChanged_Notification;
            tc.ConfirmConditionExecution();
            tc.SetCurrentStep();
        }
    }

    public void SubscribeExpAmountChangedNotification()
    {
        //Debug.LogError("SubscribeExpAmountChangedNotification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ExpAmountChanged.Notification += ExpAmountChanged_Notification;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.green_doc_unpack)
        {
            //there are 2 different conditions for this step. One is reaching 50xp, other one is unpacking green doctor.
            instanceNC.DummyRemoved.Notification += DummyRemoved_Notification;
        }

        if (TutorialController.Instance.CurrentTutorialStepTag != StepTag.wise_1 &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bubble_boy_intro &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.vip_flyby_emma_1 &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bacteria_newspaper &&
            TutorialController.Instance.CurrentTutorialStepTag != StepTag.bacteria_emma_micro_1)
        {
            //steps above we want to be triggered on next natural exp gain, not instant when previous step is compelted and enough exp is gathered
            instanceNC.ExpAmountChanged.Invoke(new ExpAmountChangedEventArgs(
                Game.Instance.gameState().GetHospitalLevel(), Game.Instance.gameState().GetExperienceAmount()));
        }
    }

    public void SubscribeExpAmountChangedNonLinear()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ExpAmountChangedNonLinear.Notification += ExpAmountChangedNonLinear_Notification;
    }

    private void ExpAmountChangedNonLinear_Notification(ExpAmountChangedEventArgs eventArgs)
    {
        if (eventArgs.level == 9 && eventArgs.expOnLevel >= 400 || eventArgs.level >= 9)
        {
            NotificationCenter.Instance.ExpAmountChangedNonLinear.Notification -=
                ExpAmountChangedNonLinear_Notification;
            TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_10);
            TutorialController.Instance.SetNonLinearStep(StepTag.NL_newspaper_lvl_10);
        }
    }

    private void KidArrivedToKidsRoom_Notification(KidArrivedToKidsRoomEventArgs eventArgs)
    {
        //Debug.Log("Wychwycenie wystąpienia poprawnej notyfikacji - KidArrivedToKidsRoom");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidArrivedToKidsRoom.Notification -= KidArrivedToKidsRoom_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeKidArrivedToKidsRoomNotification()
    {
        //Debug.Log("Subskrypcja do notyfikacji KidArrivedToKidsRoom");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.KidArrivedToKidsRoom.Notification += KidArrivedToKidsRoom_Notification;
        instanceNC.KidArrivedToKidsRoom.Invoke(new KidArrivedToKidsRoomEventArgs());
    }

    public void SubscribeBubbleBoyClicked()
    {
        //Debug.Log("SubscribeTreasureCollected");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BubbleBoyClicked.Notification += BubbleBoyClicked_Notification;
        TutorialUIController.Instance.ShowIndictator(new Vector3(21f, 0f, 35.5f));
    }

    private void BubbleBoyClicked_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.Log("TreasureCollected_Notification");
        TutorialUIController.Instance.StopShowCoroutines();
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BubbleBoyClicked.Notification -= BubbleBoyClicked_Notification;
        TutorialController.Instance.IncrementCurrentStep();
        //if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bubble_boy_intro)
        //    TutorialUIController.Instance.ShowGuidesInformation(TutorialController.Instance.GetCurrentStepData());
    }

    public void SubscribeBubbleBoyAvailable()
    {
        //Debug.Log("SubscribeTreasureCollected");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BubbleBoyAvailable.Notification += BubbleBoyAvailable_Notification;
        //NotificationCenter.Instance.BubbleBoyAvailable.Invoke(new BaseNotificationEventArgs()); //Mikko: TO JEST TU TYMCZASOWO DOPOKI NIE BEDZIE METODY TO OGARNIAJĄCEJ. USUNĄĆ JAK BB BĘDZIE SKONCZONY
    }

    private void BubbleBoyAvailable_Notification(BaseNotificationEventArgs eventArgs)
    {
        Debug.Log("BubbleBoyAvailable_Notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.BubbleBoyAvailable.Notification -= BubbleBoyAvailable_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();

        UIController.get.ExitAllPopUps();
        if (UIController.get.drawer.IsVisible)
            UIController.get.drawer.ToggleVisible();
        if (UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.ToggleVisible();
    }

    public void SubscribeTreasureCollected()
    {
        //Debug.Log("SubscribeTreasureCollected");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasureCollected.Notification += TreasureCollected_Notification;
    }

    private void TreasureCollected_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.Log("TreasureCollected_Notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasureCollected.Notification -= TreasureCollected_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeTreasureClicked()
    {
        //Debug.Log("SubscribeTreasureClicked");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasureClicked.Notification += TreasureClicked_Notification;
    }

    private void TreasureClicked_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.Log("TreasureClicked_Notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.TreasureClicked.Notification -= TreasureClicked_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeVipFlyByStart()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            return;
        }
        //Debug.LogError("SubscribeVipFlyByStart");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.VipFlyByStart.Notification += VIPFlyByStart_Notification;
        instanceNC.VipFlyByStart.Invoke(new BaseNotificationEventArgs());
    }

    private void VIPFlyByStart_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("VIPFlyByStart_Notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.VipFlyByStart.Notification -= VIPFlyByStart_Notification;
        TutorialController instanceTC = TutorialController.Instance;

        instanceTC.ConfirmConditionExecution();
        instanceTC.SetCurrentStep();

        HospitalTutorialController.HospitalTutorialInstance.VipcopterFlybyDummy.SetActive(true);
        //instanceTC.VipcopterFlybyDummy.SetActive(true);
        //ReferenceHolder.Get().engine.MainCamera.FollowGameObject(instanceTC.VipcopterFlybyDummy.transform);
        ReferenceHolder.Get().engine.MainCamera
            .FollowGameObject(HospitalTutorialController.HospitalTutorialInstance.VipcopterFlybyDummy.transform);
        ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7f, 1f, true);
        UIController.get.CloseActiveHover();
        UIController.get.ExitAllPopUps();

        HeliController.instance.StartTutorialFlyBy();
    }

    public void SubscribeVipFlyByEnd()
    {
        //Debug.LogError("SubscribeVipFlyByEnd");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.VipFlyByEnd.Notification += VipFlyByEnd_Notification;
    }

    private void VipFlyByEnd_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("VipFlyByEnd_Notification");
        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
        HospitalTutorialController.HospitalTutorialInstance.VipcopterFlybyDummy.SetActive(false);
        //TutorialController.Instance.VipcopterFlybyDummy.SetActive(false);

        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.VipFlyByEnd.Notification -= VipFlyByEnd_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeDailyQuestPopUpOpen()
    {
        //Debug.LogError("SubscribeDailyQuestPopUpOpen");
        if (UIController.getHospital != null)
        {
            NotificationCenter instanceNC = NotificationCenter.Instance;
            instanceNC.DailyQuestPopUpOpen.Notification += DailyQuestPopUpOpen_Notification;

            if (UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled)
                instanceNC.DailyQuestPopUpOpen.Invoke(new BaseNotificationEventArgs());
        }
    }

    private void DailyQuestPopUpOpen_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("DailyQuestPopUpOpen_Notification");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.DailyQuestPopUpOpen.Notification -= DailyQuestPopUpOpen_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeShowDailyQuestAnimation()
    {
        //Debug.LogError("SubscribeShowDailyQuestAnimation");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.ShowDailyQuestAnimation.Notification += ShowDailyQuestAnimation_Notification;
        instanceNC.ShowDailyQuestAnimation.Invoke(new BaseNotificationEventArgs());
    }

    private void ShowDailyQuestAnimation_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("ShowDailyQuestAnimation_Notification");
        UIController.getHospital.DailyQuestPopUpUI.ShowTutorialAnimation();

        NotificationCenter.Instance.ShowDailyQuestAnimation.Notification -= ShowDailyQuestAnimation_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeDailyQuestPopUpClosed()
    {
        //Debug.LogError("SubscribeDailyQuestPopUpClosed");
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.DailyQuestPopUpClosed.Notification += DailyQuestPopUpClosed_Notification;
        instanceNC.DailyQuestCardFlipped.Notification += DailyQuestPopUpClosed_Notification;

        if (!UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled)
            instanceNC.DailyQuestPopUpClosed.Invoke(new BaseNotificationEventArgs());
    }

    private void DailyQuestPopUpClosed_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("DailyQuestPopUpClosed_Notification");
        NotificationCenter.Instance.DailyQuestPopUpClosed.Notification -= DailyQuestPopUpClosed_Notification;
        NotificationCenter.Instance.DailyQuestCardFlipped.Notification -= DailyQuestPopUpClosed_Notification;
        TutorialUIController.Instance.InGameCloud.Hide();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeLevelGoalsActive()
    {
        if (DailyQuestSynchronizer.Instance.IsDailyQuestFuncionalityStarted())
        {
            //this is a case when player has been on level 7-9 before updating the game, so the daily quest system was active already
            //and in this case we want to skip daily quest tutorial
            TutorialController.Instance.SetStep(StepTag.level_12);
        }
        else if (CampaignConfig.hintSystemEnabled)
        {
            TutorialController.Instance.IncrementCurrentStep();
        }
        else
        {
            NotificationCenter.Instance.LevelGoalsActive.Notification += LevelGoalsActive_Notification;
            NotificationCenter.Instance.LevelGoalsActive.Invoke(new BaseNotificationEventArgs());
        }
    }

    private void LevelGoalsActive_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("DailyQuestPopUpClosed_Notification");
        NotificationCenter.Instance.LevelGoalsActive.Notification -= LevelGoalsActive_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeMicroscopeShow()
    {
        TutorialUIController.Instance.tutorialMicroscope.ShowMicroscope();
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribeMicroscopeGoodBacteriaAdded()
    {
        NotificationCenter.Instance.MicroscopeGoodBacteriaAdded.Notification +=
            MicroscopeGoodBacteriaAdded_Notification;
    }

    private void MicroscopeGoodBacteriaAdded_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("DailyQuestPopUpClosed_Notification");
        NotificationCenter.Instance.MicroscopeGoodBacteriaAdded.Notification -=
            MicroscopeGoodBacteriaAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeMicroscopeClosed()
    {
        NotificationCenter.Instance.MicroscopeClosed.Notification += SubscribeMicroscopeClosed_Notification;
    }

    private void SubscribeMicroscopeClosed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.MicroscopeClosed.Notification -= SubscribeMicroscopeClosed_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeOliviaLetterClosed()
    {
        NotificationCenter.Instance.OliviaLetterClosed.Notification += SubscribeOliviaLetterClosed_Notification;
    }

    private void SubscribeOliviaLetterClosed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.OliviaLetterClosed.Notification -= SubscribeOliviaLetterClosed_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeObjectivePanelOpened()
    {
        NotificationCenter.Instance.ObjectivePanelOpened.Notification += ObjectivePanelOpened_Notification;
    }

    private void ObjectivePanelOpened_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.ObjectivePanelOpened.Notification -= ObjectivePanelOpened_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeMailBoxPopupClosed()
    {
        NotificationCenter.Instance.MailBoxClosed.Notification += MailBoxClosed_Notification;
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.emma_about_wise)
            TutorialUIController.Instance.ShowIndictator(new Vector3(21.4f, 0f, 43.7f));
    }

    private void MailBoxClosed_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.MailBoxClosed.Notification -= MailBoxClosed_Notification;
        AnalyticsController.instance.starterPack.ShowStarterPackHard(2f);
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePackageText()
    {
        NotificationCenter.Instance.PackageText.Notification += PackageText_Notification;
        StartCoroutine(InvokePackageEvent(TutorialController.Instance.GetCurrentStepData().StepCameraDelay));
    }

    IEnumerator InvokePackageEvent(float time)
    {
        yield return new WaitForSeconds(time);
        NotificationCenter.Instance.PackageText.Invoke(new BaseNotificationEventArgs());
    }

    private void PackageText_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PackageText.Notification -= PackageText_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePillMakerAdded()
    {
        NotificationCenter.Instance.PillMakerAdded.Notification += PillMakerAdded_Notification;
        bool pillMakerExists = HospitalAreasMapController.HospitalMap.FindRotatableObject("PillLab");

        if (pillMakerExists)
            NotificationCenter.Instance.PillMakerAdded.Invoke(new BaseNotificationEventArgs());
        else
            TutorialUIController.Instance.BlinkDrawerButton(true);
    }

    private void PillMakerAdded_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.PillMakerAdded.Notification -= PillMakerAdded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeBoosterPopupClosed()
    {
        NotificationCenter.Instance.BoosterPopupClosed.Notification += BoosterPopUpClosed_Notification;
    }

    private void BoosterPopUpClosed_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.BoosterPopupClosed.Notification -= BoosterPopUpClosed_Notification;

        TutorialUIController.Instance.InGameCloud.Hide();
        TutorialUIController.Instance.StopShowCoroutines();
    }

    public void SubscribeObjectivePanelClosed()
    {
        NotificationCenter.Instance.ObjectivePanelClosed.Notification += ObjectivePanelClosed_Notification;
        if ( /*TutorialController.Instance.CurrentTutorialStepTag == StepTag.level_goals_ended &&*/
            !HospitalUIController.get.ObjectivesPanelUI.isSlidIn)
            HospitalUIController.get.ObjectivesPanelUI.SlideIn();
    }

    private void ObjectivePanelClosed_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.ObjectivePanelClosed.Notification -= ObjectivePanelClosed_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribePatientSentToXRay()
    {
        NotificationCenter.Instance.PatientSentToXRay.Notification += PatientSentToXRay_Notification;
    }

    private void PatientSentToXRay_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.PatientSentToXRay.Notification -= PatientSentToXRay_Notification;
        TutorialUIController.Instance.HideIndicator();
        //TutorialUIController.Instance.tutorialArrowUI.Hide();
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeFirstPlantPlanted()
    {
        NotificationCenter.Instance.FirstPlantPlanted.Notification += FirstPlantPlanted_Notification;
    }

    private void FirstPlantPlanted_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.FirstPlantPlanted.Notification -= FirstPlantPlanted_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeEpidemyCenterBuilt()
    {
        NotificationCenter.Instance.EpidemyCenterBuilt.Notification += EpidemyCenterBuilt_Notification;
    }

    private void EpidemyCenterBuilt_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.EpidemyCenterBuilt.Notification -= EpidemyCenterBuilt_Notification;
        TutorialController.Instance.ConfirmConditionExecution();
        TutorialController.Instance.SetCurrentStep();
    }

    public void SubscribePanaceaCollectorUpgraded()
    {
        AchievementNotificationCenter.Instance.PanaceaCollectorUpgraded.Notification +=
            PanaceaCollectorUpgraded_Notification;
    }

    private void PanaceaCollectorUpgraded_Notification(AchievementProgressEventArgs args)
    {
        AchievementNotificationCenter.Instance.PanaceaCollectorUpgraded.Notification -=
            PanaceaCollectorUpgraded_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeVipSpeedupPopupOpened()
    {
        NotificationCenter.Instance.VipSpeedupPopupOpened.Notification -= VipSpeedupPopupOpen_Notification;
        NotificationCenter.Instance.VipSpeedupPopupOpened.Notification += VipSpeedupPopupOpen_Notification;
    }

    private void VipSpeedupPopupOpen_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.VipSpeedupPopupOpened.Notification -= VipSpeedupPopupOpen_Notification;
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification -= SkipVipSpeedupTutorial_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    public void SubscribeVipSpeedupPopupClosed()
    {
        NotificationCenter.Instance.VipSpeedupPopupClosed.Notification -= VipSpeedupPopupClosed_Notification;
        NotificationCenter.Instance.VipSpeedupPopupClosed.Notification += VipSpeedupPopupClosed_Notification;
    }

    private void VipSpeedupPopupClosed_Notification(BaseNotificationEventArgs args)
    {
        NotificationCenter.Instance.VipSpeedupPopupClosed.Notification -= VipSpeedupPopupClosed_Notification;
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Notification -= SkipVipSpeedupTutorial_Notification;

        TutorialController.Instance.IncrementCurrentStep();
    }

    #region non linear tutorial steps

    public void SubscribeNotEnoughPanaceaNotification()
    {
        NotificationCenter instanceNC = NotificationCenter.Instance;
        instanceNC.NotEnoughPanacea.Notification += NotEnoughPanacea_Notification;
    }

    private void NotEnoughPanacea_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.NotEnoughPanacea.Notification -= NotEnoughPanacea_Notification;
        TutorialController.Instance.IncrementCurrentStep();
    }

    private void ShowPanaceaPopUp(FullscreenTutHiddenEventArgs eventArgs)
    {
        NotificationCenter.Instance.FullscreenTutHidden.Notification -= ShowPanaceaPopUp;
        UIController.getHospital.PanaceaPopUp.Open(GameState.Get().PanaceaCollector);
        TutorialUIController.Instance.BlinkImage(UIController.getHospital.PanaceaPopUp.GetUpgradeButton()
            .GetComponent<Image>());
    }

    public void SubscribePushNotificationsDisabled()
    {
        NotificationCenter.Instance.PushNotificationsDisabled.Notification += PushNotificationsDisabled_Notification;
    }

    private void PushNotificationsDisabled_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.PushNotificationsDisabled.Notification -= PushNotificationsDisabled_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_push_disabled);
        tc.SetNonLinearStep(StepTag.NL_push_disabled);
        TutorialUIController.Instance.ShowPushReminderButtons();
    }

    public void SubscribeBoostersPopUpOpen()
    {
        NotificationCenter.Instance.BoostersPopUpOpen.Notification += BoostersPopUpOpen_Notification;
    }

    private void BoostersPopUpOpen_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.BoostersPopUpOpen.Notification -= BoostersPopUpOpen_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_boosters);
        tc.SetNonLinearStep(StepTag.NL_boosters);
    }

    public void SubscribeGiftReady()
    {
        NotificationCenter.Instance.GiftReady.Notification += GiftReady_Notification;
    }

    private void GiftReady_Notification(BaseNotificationEventArgs eventArgs)
    {
        if (VisitingController.Instance.IsVisiting)
            return;

        NotificationCenter.Instance.GiftReady.Notification -= GiftReady_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_gifts);
        tc.SetNonLinearStep(StepTag.NL_gifts);
    }

    public void SubscribeEpidemyCompleted()
    {
        //Debug.LogError("SubscribeEpidemyCompleted");
        NotificationCenter.Instance.EpidemyCompleted.Notification += EpidemyCompleted_Notification;
    }

    private void EpidemyCompleted_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("EpidemyCompleted_Notification");
        NotificationCenter.Instance.EpidemyCompleted.Notification -= EpidemyCompleted_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_epidemy);
        tc.SetNonLinearStep(StepTag.NL_newspaper_epidemy);
    }

    public void SubscribeTenPatioDecorations()
    {
        NotificationCenter.Instance.TenPatioDecorations.Notification += TenPatioDecorations_Notification;
    }

    private void TenPatioDecorations_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.TenPatioDecorations.Notification -= TenPatioDecorations_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_patio_decos);
        tc.SetNonLinearStep(StepTag.NL_newspaper_patio_decos);
    }

    public void SubscribeTwentyProbeTables()
    {
        if (FindObjectsOfType<ProbeTable>().Length >= 21)
        {
            TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_patio_decos);
            return;
        }

        NotificationCenter.Instance.TwentyProbeTables.Notification += TwentyProbeTables_Notification;
    }

    private void TwentyProbeTables_Notification(BaseNotificationEventArgs eventArgs)
    {
        NotificationCenter.Instance.TwentyProbeTables.Notification -= TwentyProbeTables_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_probe_tables);
        tc.SetNonLinearStep(StepTag.NL_newspaper_probe_tables);
    }

    public void SubscribeSecondDiagnosticMachineOpen()
    {
        //Debug.LogError("SubscribeSecondDiagnosticMachineOpen " + FindObjectsOfType<DiagnosticRoom>().Length);
        if (FindObjectsOfType<DiagnosticRoom>().Length >= 2)
        {
            TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_diagnosis);
            return;
        }

        NotificationCenter.Instance.SecondDiagnosticMachineOpen.Notification +=
            SecondDiagnosticMachineOpen_Notification;
    }

    private void SecondDiagnosticMachineOpen_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("SecondDiagnosticMachineOpen_Notification ");

        NotificationCenter.Instance.SecondDiagnosticMachineOpen.Notification -=
            SecondDiagnosticMachineOpen_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_diagnosis);
        tc.SetNonLinearStep(StepTag.NL_newspaper_diagnosis);
    }

    public void SubscribeLevel10NewspaperClosedCond()
    {
        //Debug.LogError("SubscribeLevel10NewspaperClosed");
        if (Game.Instance.gameState().GetHospitalLevel() >= 10)
        {
            TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_lvl_10_wise_2);
            return;
        }

        NotificationCenter.Instance.Level10NewspaperClosedCond.Notification += Level10NewspaperClosedCond_Notification;
    }

    private void Level10NewspaperClosedCond_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("Level10NewspaperClosed_Notification ");

        NotificationCenter.Instance.Level10NewspaperClosedCond.Notification -= Level10NewspaperClosedCond_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_lvl_10_wise_2);
        if (!TutorialController.Instance.IsNonLinearStepCompleted(StepTag.NL_boosters))
            tc.SetNonLinearStep(StepTag.NL_lvl_10_wise_2);

        HospitalUIController.get.SetBoosterAndBoxButtons();
    }

    public void SubscribeLevel10NewspaperClosedNotif()
    {
        //Debug.LogError("SubscribeLevel10NewspaperClosedNotif");
        NotificationCenter.Instance.Level10NewspaperClosedNotif.Notification +=
            Level10NewspaperClosedNotif_Notification;
    }

    private void Level10NewspaperClosedNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("Level10NewspaperClosedNotif_Notification ");
        NotificationCenter.Instance.Level10NewspaperClosedNotif.Notification -=
            Level10NewspaperClosedNotif_Notification;
        NotificationCenter.Instance.Level10NewspaperClosedCond.Invoke(new BaseNotificationEventArgs());
    }

    public void SubscribeLevel10WiseGiveBooster()
    {
        NotificationCenter.Instance.Level10NewspaperClosedNotif.Notification += Level10WiseGiveBooster_Notification;

        if (TutorialController.Instance.IsNonLinearStepCompleted(StepTag.NL_boosters))
        {
            HospitalUIController.get.BoosterButton.GetComponent<Button>().interactable = true;
            return;
        }
    }

    private void Level10WiseGiveBooster_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("Level10WiseGiveBooster_Notification ");
        NotificationCenter.Instance.Level10NewspaperClosedNotif.Notification -= Level10WiseGiveBooster_Notification;
    }

    public void SubscribeLevel10WiseClosedCond()
    {
        //Debug.LogError("SubscribeLevel10WiseClosedCond");
        if (Game.Instance.gameState().GetHospitalLevel() > 10)
        {
            TutorialController.Instance.MarkNonLinearStepAsCompleted(StepTag.NL_lvl_10_wise_2);
            return;
        }

        NotificationCenter.Instance.Level10WiseClosedCond.Notification += Level10WiseClosedCond_Notification;
    }

    private void Level10WiseClosedCond_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("Level10WiseClosedCond_Notification ");
        NotificationCenter.Instance.Level10WiseClosedCond.Notification -= Level10WiseClosedCond_Notification;

        TutorialController tc = TutorialController.Instance;
        tc.MarkNonLinearStepAsCompleted(StepTag.NL_lvl_10_wise_2);
        tc.SetNonLinearStep(StepTag.NL_lvl_10_wise_2);
    }

    public void SubscribeLevel10WiseClosedNotif()
    {
        //Debug.LogError("SubscribeLevel10WiseClosedNotif");
        NotificationCenter.Instance.Level10WiseClosedNotif.Notification += Level10WiseClosedNotif_Notification;
    }

    private void Level10WiseClosedNotif_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("Level10WiseClosedNotif_Notification ");
        NotificationCenter.Instance.Level10WiseClosedNotif.Notification -= Level10WiseClosedNotif_Notification;
        NotificationCenter.Instance.Level10WiseClosedCond.Invoke(new BaseNotificationEventArgs());
    }

    List<int> levelsSubscribed = new List<int>();

    public void SubscribeLevelReachedAndClosedNonLinear(int level)
    {
        TutorialController tc = TutorialController.Instance;

        if (Game.Instance.gameState().GetHospitalLevel() >=
            level) //when someone with higher level updates the game we have to mark lower level tutorial steps as completed so we do not show 5+ newspapers at once
        {
            switch (level)
            {
                case 11:
                    tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_11);
                    break;
                case 13:
                    tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_13);
                    break;
                case 20:
                    tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_20);
                    break;
                case 22:
                    tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_19);
                    break;
                case 30:
                    tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_30);
                    break;
                case 42:
                    tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_42);
                    break;
                default:
                    break;
            }

            return;
        }
        //Debug.LogError("SubscribeLevelReachedAndClosedNonLinear " + level);

        if (NotificationCenter.Instance.LevelReachedAndClosedNonLinear.IsNull())
            NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Notification +=
                LevelReachedAndClosedNonLinear_Notification;

        if (!levelsSubscribed.Contains(level))
            levelsSubscribed.Add(level);
    }

    private void LevelReachedAndClosedNonLinear_Notification(LevelReachedAndClosedEventArgs eventArgs)
    {
        //Debug.LogError("LevelReachedAndClosedNonLinear_Notification " + eventArgs.level);
        levelsSubscribed.Remove(eventArgs.level);
        if (levelsSubscribed.Count == 0)
        {
            //Debug.LogError("Unsubscribing NL level");
            NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Notification -=
                LevelReachedAndClosedNonLinear_Notification;
        }

        TutorialController tc = TutorialController.Instance;
        switch (eventArgs.level)
        {
            case 11:
                tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_11);
                tc.SetNonLinearStep(StepTag.NL_newspaper_lvl_11);
                break;
            case 13:
                tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_13);
                tc.SetNonLinearStep(StepTag.NL_newspaper_lvl_13);
                break;
            case 22:
                tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_19);
                tc.SetNonLinearStep(StepTag.NL_newspaper_lvl_19);
                break;
            case 20:
                tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_20);
                tc.SetNonLinearStep(StepTag.NL_newspaper_lvl_20);
                break;
            case 30:
                tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_30);
                tc.SetNonLinearStep(StepTag.NL_newspaper_lvl_30);
                break;
            case 42:
                tc.MarkNonLinearStepAsCompleted(StepTag.NL_newspaper_lvl_42);
                tc.SetNonLinearStep(StepTag.NL_newspaper_lvl_42);
                break;
            default:
                break;
        }
    }

    public void SubscribeNewspaperRewardDiamond()
    {
        //Debug.LogError("SubscribeNewspaperRewardDiamond");
        NotificationCenter.Instance.NewspaperRewardDiamond.Notification += NewspaperRewardDiamond_Notification;
    }

    private void NewspaperRewardDiamond_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("NewspaperRewardDiamond_Notification");
        NotificationCenter.Instance.NewspaperRewardDiamond.Notification -= NewspaperRewardDiamond_Notification;

        int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
        GameState.Get().AddResource(ResourceType.Diamonds, 1, EconomySource.Tutorial, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, 1, 0, 1.75f, Vector3.one,
            new Vector3(1, 1, 1), null, null,
            () => { GameState.Get().UpdateCounter(ResourceType.Diamonds, 1, currentDiamondAmount); });
    }

    public void SubscribeNewspaperRewardExp()
    {
        //Debug.LogError("SubscribeNewspaperRewardExp");
        NotificationCenter.Instance.NewspaperRewardExp.Notification += NewspaperRewardExp_Notification;
    }

    private void NewspaperRewardExp_Notification(BaseNotificationEventArgs eventArgs)
    {
        //Debug.LogError("NewspaperRewardExp_Notification");
        NotificationCenter.Instance.NewspaperRewardExp.Notification -= NewspaperRewardExp_Notification;

        int currentExperienceAmount = Game.Instance.gameState().GetExperienceAmount();
        GameState.Get().AddResource(ResourceType.Exp, 15, EconomySource.Tutorial, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Exp, Vector3.zero, 15, 0, 1.75f, Vector3.one,
            new Vector3(1, 1, 1), null, null,
            () => { GameState.Get().UpdateCounter(ResourceType.Exp, 15, currentExperienceAmount); });
    }

    #endregion
}