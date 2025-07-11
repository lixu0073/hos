using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using UnityEngine.UI;
using System;

namespace Hospital
{
    public class MainPatientCardPopUpController : UIElement
    {
        [SerializeField] TextMeshProUGUI patientName = null;
        [SerializeField] Transform treatmentContent = null;
        [SerializeField] Transform patientsContent = null;
        [SerializeField] Transform roomsContent = null;
        [SerializeField] TextMeshProUGUI coinsForCure = null;
        [SerializeField] TextMeshProUGUI expForCure = null;
        [SerializeField] Button visitButton = null;
        [SerializeField] GameObject TreatmentPrefab = null;
        [SerializeField] GameObject OtherPatientPrefab = null;
        [SerializeField] GameObject VipPlaceholoderPrefab = null;
        [SerializeField] GameObject[] RoomGridObjects = null;

        [SerializeField] GameObject treatmentView = null;
        [SerializeField] GameObject waitingView = null;
        [SerializeField] GameObject onTheWayView = null;
        [SerializeField] TextMeshProUGUI waitingTimer = null;

        [SerializeField] GameObject vipDetails = null;
        [SerializeField] TextMeshProUGUI vipTimer = null;
        [SerializeField] Button SpeedButton = null;

        [Header("Visit button indicators")]
        [SerializeField] Image visitIndicatorNew = null;
        [SerializeField] Image visitIndicatorCure = null;
        [SerializeField] Image visitIndicatorDiagnose = null;
        [SerializeField] Image visitHelpRequired = null;
        [SerializeField] Image visitIndicatorCureHelp = null;


        List<OtherPatient> otherList = new List<OtherPatient>();
        List<IDiagnosePatient> patients = new List<IDiagnosePatient>();
        GameObject tempObject;
        GameObject vipPlaceholder;
        Coroutine waitTimerCoroutine;
        Coroutine vipTimerCoroutine;

        int selected = 0;

        void Awake()
        {
            ClearPatients();
            ClearTreatments();
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        void ClearPatients()
        {
            if (patientsContent.childCount > 0)
            {
                Debug.LogError("Clearing patient game objects which were left on scene. Please remove them next time.");
                for (int i = 0; i < patientsContent.childCount; i++)
                    Destroy(patientsContent.GetChild(i).gameObject);
            }
        }

        void ClearTreatments()
        {
            if (treatmentContent.childCount > 0)
            {
                for (int i = 0; i < treatmentContent.childCount; i++)
                    Destroy(treatmentContent.GetChild(i).gameObject);
            }
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers);
            UpdatePatients();

            if (otherList[0].gameObject.activeSelf)
                SetData(otherList[0].info, 0, false);
            else
                SetData(otherList[1].info, 1, false);

            whenDone?.Invoke();
        }

        public void UpdatePatients()
        {
            //Debug.LogError("MainCard UpdatePatients");
            if (!gameObject.activeSelf)
                return;

            List<HospitalBedController.HospitalBed> beds = HospitalAreasMapController.HospitalMap.hospitalBedController.Beds;
            if (beds.Count == 0)
                return;

            for (int i = 0; i < beds.Count; i++)
            {
                //show bed infos (instantiate prefabs) which are not already created
                if (i >= otherList.Count)
                    AddBedInfo(i);

                if (beds[i].Patient == null)
                    UpdateBedInfo(null, i);
                else
                {
                    HospitalCharacterInfo tmp = ((BasePatientAI)beds[i].Patient).GetComponent<HospitalCharacterInfo>();
                    UpdateBedInfo(tmp, i);
                }
            }

            StartCoroutine(HandleHideVIP(0));

            SetData(otherList[selected].info, selected, false);
        }

        void UpdateBedInfo(HospitalCharacterInfo info, int id)
        {
            if (Game.Instance.gameState().GetHospitalLevel() == 3 && id == 1 && !TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.emma_on_george)) // to hide second patient on lvl 3
                return;

            if (info != null)   //patient info
            {
                otherList[id].SetInfo(info, -1);
                if (info.IsVIP && !vipPlaceholder)
                {
                    AddVipPlaceholder();
                }

                otherList[id].SetButton(delegate ()
                {
                    SetData(info, id);
                });
            }
            else //bed info
            {
                HospitalBedController hbc = HospitalAreasMapController.HospitalMap.hospitalBedController;
                otherList[id].SetInfo(null, hbc.GetBedStatusForID(id), hbc.GetWaitTimerForBed(id));
                otherList[id].SetButton(delegate ()
                {
                    SetData(null, id);
                });
            }
            otherList[id].SetIndicator();

            tempObject = patientsContent.transform.GetChild(id).gameObject;
            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[id].room == null)
            {
                if (!HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().VIPInHeli && (HospitalAreasMapController.HospitalMap.vipRoom.currentVip != null && !HospitalAreasMapController.HospitalMap.vipRoom.currentVip.gameObject.GetComponent<VIPPersonController>().GetGoHome()))
                {
                    tempObject.gameObject.SetActive(true);
                    if (vipPlaceholder)
                        vipPlaceholder.SetActive(true);
                }
                else
                {
                    tempObject.gameObject.SetActive(false);
                    if (vipPlaceholder)
                        vipPlaceholder.SetActive(false);
                }
            }
            SetRoomObjects();
        }

        void AddVipPlaceholder()
        {
            //this takes one space in the grid so VIP takes '2' spaces. This way rooms are aligned by 2 each row
            vipPlaceholder = Instantiate(VipPlaceholoderPrefab);
            vipPlaceholder.transform.SetParent(patientsContent.transform);
            vipPlaceholder.transform.localScale = Vector3.one;
            vipPlaceholder.transform.SetSiblingIndex(1);
        }

        void SetRoomObjects()
        {
            for (int i = 0; i < RoomGridObjects.Length; i++)
                RoomGridObjects[i].SetActive(false);

            int activeRooms = Mathf.CeilToInt(patientsContent.childCount / 2);
            for (int i = 0; i < activeRooms; i++)
            {
                if (RoomGridObjects.Length > i)
                    RoomGridObjects[i].SetActive(true);
            }
        }

        void AddBedInfo(int id)
        {
            if (Game.Instance.gameState().GetHospitalLevel() == 3 && id == 1 && !TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.emma_on_george))  // to hide second patient on lvl 3
                return;

            tempObject = Instantiate(OtherPatientPrefab);
            tempObject.transform.SetParent(patientsContent.transform);
            tempObject.transform.localScale = Vector3.one;
            OtherPatient op = tempObject.GetComponent<OtherPatient>();
            HospitalBedController hbc = HospitalAreasMapController.HospitalMap.hospitalBedController;

            op.SetInfo(null, hbc.GetBedStatusForID(id), hbc.GetWaitTimerForBed(id));
            op.SetButton(delegate ()
            {
                SetData(null, id);
            });

            otherList.Add(op);
        }

        void SetData(HospitalCharacterInfo info, int bedId, bool moveCamera = true)
        {
            DeselectAll();
            Select(bedId);

            if (info)
                ShowTreamentView(info, bedId);
            else
            {
                if (HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedStatusForID(bedId) == 0)
                    ShowWaitingView(bedId);
                else
                    ShowOnTheWayView();
            }

            if (moveCamera)
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[bedId].Bed.gameObject.transform.position, 1f, false);
        }

        void ShowTreamentView(HospitalCharacterInfo info, int bedId)
        {
            treatmentView.SetActive(true);
            waitingView.SetActive(false);
            onTheWayView.SetActive(false);

            SetName(info);
            SetReward(info);
            SetTreatments(info);
            SetVisitButton(info, bedId);
            SetVipStatus(info);
            SetVisitIndicator(info);
            SetHelpIndicator(info);
        }

        void SetVisitIndicator(HospitalCharacterInfo info)
        {
            visitIndicatorCure.gameObject.SetActive(false);
            visitIndicatorCureHelp.gameObject.SetActive(false);
            visitIndicatorNew.gameObject.SetActive(false);
            visitIndicatorDiagnose.gameObject.SetActive(false);

            if (info.CheckCurePosible(out bool cureWithHelp))
            {
                if (cureWithHelp)
                    visitIndicatorCureHelp.gameObject.SetActive(true);
                else
                    visitIndicatorCure.gameObject.SetActive(true);
            }
            else if (!info.WasPatientCardSeen)
            {
                visitIndicatorNew.gameObject.SetActive(true);
            }
            else if (info.RequiresDiagnosis)
            {
                visitIndicatorDiagnose.GetComponent<Image>().sprite = ResourcesHolder.GetHospital().diagnosisBadgeGfx.GetDiagnosisBadge(HospitalDataHolder.Instance.ReturnDiseaseTypeRoomType((int)(info.DisaseDiagnoseType)));
                visitIndicatorDiagnose.gameObject.SetActive(true);
            }
        }

        void SetHelpIndicator(HospitalCharacterInfo info)
        {
            if (ReferenceHolder.GetHospital().treatmentRoomHelpController.IsHelpRequestForPatient(info))
                visitHelpRequired.gameObject.SetActive(true);
            else
                visitHelpRequired.gameObject.SetActive(false);
        }

        void ShowWaitingView(int bedId)
        {
            treatmentView.SetActive(false);
            waitingView.SetActive(true);
            onTheWayView.SetActive(false);

            SetSpeedButton(bedId);

            try
            { 
                if (waitTimerCoroutine != null)
                {
                    StopCoroutine(waitTimerCoroutine);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            waitTimerCoroutine = StartCoroutine(WaitingTimerCoroutine(bedId));
        }

        IEnumerator WaitingTimerCoroutine(int bedId)
        {
            int currentWaitTimer;

            while (true)
            {
                currentWaitTimer = HospitalAreasMapController.HospitalMap.hospitalBedController.GetWaitTimerForBed(bedId);

                if (currentWaitTimer >= 1)
                    waitingTimer.text = UIController.GetFormattedShortTime(currentWaitTimer);
                else
                {
                    waitingTimer.text = "";
                    waitTimerCoroutine = null;
                    yield break;
                }
                yield return new WaitForSeconds(1);
            }
        }

        void ShowOnTheWayView()
        {
            treatmentView.SetActive(false);
            waitingView.SetActive(false);
            onTheWayView.SetActive(true);
        }

        IEnumerator HandleHideVIP(int bedId)
        {
            //VIP patient should not be shown when he is not in bed (i.e. no vip, or vip walking to room, or vip walking after cure/dismiss
            //for some reason during first opening of this popup VIP patient would not be disabled because Other Patients were just instantiated
            //so I have put this in a coroutine to delay this check a couple of frames
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            GameObject temp = patientsContent.transform.GetChild(bedId).gameObject;
            temp.gameObject.SetActive(true);

            if (HospitalAreasMapController.HospitalMap.hospitalBedController.Beds[bedId].room == null)
            {
                if (!HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().VIPInHeli && (HospitalAreasMapController.HospitalMap.vipRoom.currentVip != null && !HospitalAreasMapController.HospitalMap.vipRoom.currentVip.gameObject.GetComponent<VIPPersonController>().GetGoHome()))
                    temp.gameObject.SetActive(true);
                else
                    temp.gameObject.SetActive(false);
            }
        }

        void SetName(HospitalCharacterInfo info)
        {
            if (!info.Name.Contains("_"))
            {
                patientName.text = info.Name + " " + info.Surname;
            }
            else
            {
                patientName.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + info.Surname);
            }

        }

        void SetReward(HospitalCharacterInfo info)
        {
            BalanceableInt rewardCoins = BalanceableFactory.CreateGoldForTreatmentRoomsBalanceable(info.CoinsForCure);
            BalanceableInt rewardExp = BalanceableFactory.CreateXPForTreatmentRoomsBalanceable(info.EXPForCure);

            coinsForCure.text = rewardCoins.GetBalancedValue().ToString() ; //info.CoinsForCure.ToString();
            coinsForCure.enableVertexGradient = false;
            expForCure.text = rewardExp.GetBalancedValue().ToString(); //info.EXPForCure.ToString();
            expForCure.enableVertexGradient = false;

            coinsForCure.color = Color.white;
            coinsForCure.outlineColor = Color.black;
            expForCure.color = Color.white;
            expForCure.outlineColor = Color.black;

            bool boosterActive = HospitalAreasMapController.HospitalMap.boosterManager.boosterActive;
            BoosterType boosterType;
            BoosterTarget boosterTarget;

            bool increaseXPForTreatmentRoomsEventActive = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.expForTreatmentRooms_FACTOR);
            bool increaseGoldForTreatmentRoomsEventActive = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.coinsForTreatmentRooms_FACTOR);

            if (boosterActive)
            {
                boosterType = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterType;
                boosterTarget = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterTarget;

                if (boosterTarget == BoosterTarget.PatientCard || boosterTarget == BoosterTarget.AllPatients)
                {
                    if (boosterType == BoosterType.Coin || boosterType == BoosterType.CoinAndExp || increaseGoldForTreatmentRoomsEventActive)
                    {
                        BoosterManager.TintWithBoosterGradient(coinsForCure);
                    }
                    if (boosterType == BoosterType.Exp || boosterType == BoosterType.CoinAndExp || increaseXPForTreatmentRoomsEventActive)
                    {
                        BoosterManager.TintWithBoosterGradient(expForCure);
                    }
                }
            }
            else
            {
                if (increaseXPForTreatmentRoomsEventActive)
                {
                    BoosterManager.TintWithBoosterGradient(expForCure);
                }

                if (increaseGoldForTreatmentRoomsEventActive)
                {
                    BoosterManager.TintWithBoosterGradient(coinsForCure);
                }
            }
        }

        void SetTreatments(HospitalCharacterInfo info)
        {
            ClearTreatments();

            int whichToBeDiagnosed = -1;
            int diagnoseType = -1;
            bool InDiagnosisState = false;

            if (info.IsVIP)
            {
                VIPPersonController infoAI = info.GetComponent<VIPPersonController>();
                if (infoAI.state == VIPPersonController.CharacterStatus.Diagnose)
                    InDiagnosisState = true;
            }
            else
            {
                HospitalPatientAI infoAI = info.GetComponent<HospitalPatientAI>();
                if (infoAI.state == HospitalPatientAI.CharacterStatus.Diagnose)
                    InDiagnosisState = true;
            }

            Dictionary<MedicineRef, int> requestedMeds = new Dictionary<MedicineRef, int>();
            requestedMeds = ReferenceHolder.GetHospital().treatmentRoomHelpController.GetHelpedMedicinesForPatient(info);

            for (int i = 0; i < info.requiredMedicines.Length; ++i)
            {
                switch ((int)info.requiredMedicines[i].Key.Disease.DiseaseType)
                {
                    case (int)DiseaseType.Brain:
                        whichToBeDiagnosed = i;
                        if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            diagnoseType = (int)DiseaseType.Brain;
                        break;
                    case (int)DiseaseType.Bone:
                        whichToBeDiagnosed = i;
                        if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            diagnoseType = (int)DiseaseType.Bone;
                        break;
                    case (int)DiseaseType.Ear:
                        whichToBeDiagnosed = i;
                        if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            diagnoseType = (int)DiseaseType.Ear;
                        break;
                    case (int)DiseaseType.Lungs:
                        whichToBeDiagnosed = i;
                        if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            diagnoseType = (int)DiseaseType.Lungs;
                        break;
                    case (int)DiseaseType.Kidneys:
                        whichToBeDiagnosed = i;
                        if ((info.RequiresDiagnosis) || (InDiagnosisState))
                            diagnoseType = (int)DiseaseType.Kidneys;
                        break;
                    default:
                        AddTreatment(info.requiredMedicines[i].Key, info.requiredMedicines[i].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[i].Key.GetMedicineRef()));
                        break;
                }
            }

            // SET REQUIRED DIAGNOSE AS LAST IF IT'S EXIST
            if ((whichToBeDiagnosed != -1))
            {
                if (diagnoseType != -1)
                    AddDiagnosis((DiseaseType)diagnoseType);
                else
                    AddTreatment(info.requiredMedicines[whichToBeDiagnosed].Key, info.requiredMedicines[whichToBeDiagnosed].Value, GetHelpedCureAmount(requestedMeds, info.requiredMedicines[whichToBeDiagnosed].Key.GetMedicineRef()));
            }
        }

        private void AddTreatment(MedicineDatabaseEntry data, int cureAmount, int helpedAmount)
        {
            GameObject temp = Instantiate(TreatmentPrefab);
            temp.transform.SetParent(treatmentContent.transform);
            temp.transform.localScale = Vector3.one;
            TreatmentPanel tp = temp.GetComponent<TreatmentPanel>();
            tp.Initialize(data, cureAmount, helpedAmount);
        }

        private void AddDiagnosis(DiseaseType disaseType)
        {
            GameObject temp = Instantiate(TreatmentPrefab);
            temp.transform.SetParent(treatmentContent.transform);
            temp.transform.localScale = Vector3.one;
            TreatmentPanel tp = temp.GetComponent<TreatmentPanel>();
            tp.Initialize(disaseType);
        }

        void SetVipStatus(HospitalCharacterInfo info)
        {
            if (info.IsVIP)
            {
                vipDetails.SetActive(true);
                try
                { 
                    if (vipTimerCoroutine != null)
                    {
                        StopCoroutine(vipTimerCoroutine);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                vipTimerCoroutine = StartCoroutine(UpdateVipTimer(info));
            }
            else
                vipDetails.SetActive(false);
        }

        IEnumerator UpdateVipTimer(HospitalCharacterInfo info)
        {
            int currentWaitTimer;

            while (true)
            {
                currentWaitTimer = (int)info.VIPTime;

                if (currentWaitTimer >= 1)
                    vipTimer.text = UIController.GetFormattedShortTime(currentWaitTimer);
                else
                {
                    vipTimer.text = "";
                    vipTimerCoroutine = null;
                    yield break;
                }
                yield return new WaitForSeconds(1);
            }
        }

        void SetVisitButton(HospitalCharacterInfo info, int bedId)
        {
            visitButton.onClick.RemoveAllListeners();
            visitButton.onClick.AddListener(delegate ()
            {
                Visit(info);
                UIController.getHospital.PatientCard.Open(info, bedId);
            });
        }

        public void Visit(HospitalCharacterInfo info)
        {
            Exit();
            //ReferenceHolder.Get().engine.MainCamera.MoveToPoint(info.gameObject.transform.position);
        }

        void DeselectAll()
        {
            for (int i = 0; i < otherList.Count; i++)
                otherList[i].SetSelected(false);
        }

        void Select(int i)
        {
            otherList[i].SetSelected(true);

            selected = i;
        }

        void SetSpeedButton(int bedId)
        {
            SpeedButton.onClick.AddListener(delegate ()
            {
                int diamondSpeedCost = 3;

                if (Game.Instance.gameState().GetDiamondAmount() >= diamondSpeedCost)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(diamondSpeedCost, delegate
                    {
                        HospitalAreasMapController.HospitalMap.hospitalBedController.SpeedBedWaitingForID(bedId);

                        UpdatePatients();
                        SetData(otherList[bedId].info, bedId);

                        GameState.Get().RemoveDiamonds(diamondSpeedCost, EconomySource.SpeedUpBed);
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            });
        }

        public void ButtonExit()
        {
            base.Exit();
        }

        void Update()
        {
            roomsContent.transform.localPosition = patientsContent.transform.localPosition;
        }

        /// <summary>
        /// To implement!!!
        /// </summary>
        /// <returns>Amount of cures provided by helpers</returns>
        int GetHelpedCureAmount(Dictionary<MedicineRef, int> requestedMeds, MedicineRef medToFind)
        {
            if (requestedMeds != null && requestedMeds.Count > 0)
            {
                if (requestedMeds.TryGetValue(medToFind, out int val))
                {
                    return val;
                }
            }

            return 0;
        }
    }
}