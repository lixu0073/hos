using Maternity.Adapter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maternity.UI
{
    public class MaternityTreatmentPanelUI : MonoBehaviour, IMaternityTreatmentPanelUI
    {
        [SerializeField]
        private MaternityPatientInfoPanelUI motherInfoPanel = null;
        [SerializeField]
        private MaternityBabyInfoPanelUI babyInfoPanel = null;
        [SerializeField]
        private MaternityBedInfoPanelUI bedInfoPanel = null;
        [SerializeField]
        private MaternityEmptyMachinePanelUI emptyMachinePanel = null;
        [SerializeField]
        private MaternityCurePanelUI curePanel = null;
        [SerializeField]
        private MaternityTreatmentStagePanelUI treatmentStagePanel = null;
        [SerializeField]
        private MaternityStagesPanelUI stagesPanel = null;


        #region Set View
        public void SetLaborRoomRequiredView(string info, UnityAction buttonAction, string buttonText, Sprite buttonIcon, Sprite machineIcon)
        {
            bedInfoPanel.SetInfoAndButtonView(info, buttonAction, buttonText, buttonIcon);
            emptyMachinePanel.SetRoomRequiredView(machineIcon);
            SetEmptyRoomObjectsActive();
        }

        public void SetWaitingForNextPatientView(UnityAction buttonAction, string buttonText, Sprite buttonIcon, string infoText)
        {
            bedInfoPanel.SetWaitingView(buttonAction, buttonText, buttonIcon);
            emptyMachinePanel.SetWaitingForPatientView(infoText);
            SetEmptyRoomObjectsActive();
        }

        public void SetNextPatientOnHisWayView(string info, string infoText)
        {
            bedInfoPanel.SetInfoView(info);
            emptyMachinePanel.SetPatientCommingView(infoText);
            SetEmptyRoomObjectsActive();
        }

        public void SetDiagnoseRequiredView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string buttontext, string stageTitle, string diagnoseCost, string diagnoseTime, Sprite buttonSprite, bool isBlinking = false)
        {
            curePanel.SetCurePanel(exp, cureButtonAction, diagnoseCost, buttonSprite, isBlinking);
            treatmentStagePanel.SetDiagnoseRequiredView(stageTitle, diagnoseTime);
            stagesPanel.SetDiagnoseStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetDiagnoseInQueueView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string cureButtonText, string stageTitle, string inQueue)
        {
            motherInfoPanel.SetPatientInfoView(patientInfo);
            curePanel.SetCurePanel(exp, cureButtonAction, cureButtonText);
            treatmentStagePanel.SetDiagnoseInQueueView(stageTitle, inQueue);
            stagesPanel.SetDiagnoseStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetDiagnoseInProgressView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string cureButtonText, string stageTitle, string diagnoseStatus)
        {
            motherInfoPanel.SetPatientInfoView(patientInfo);
            curePanel.SetCurePanel(exp, cureButtonAction, cureButtonText);
            treatmentStagePanel.SetDiagnoseInProgressView(stageTitle, diagnoseStatus);
            stagesPanel.SetDiagnoseStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetDiagnoseEndedView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string cureButtonText, string stageTitle, string diagnoseStatus)
        {
            motherInfoPanel.SetPatientInfoView(patientInfo);
            curePanel.SetCurePanel(exp, cureButtonAction, cureButtonText);
            treatmentStagePanel.SetDiagnoseEndedView(stageTitle, diagnoseStatus);
            stagesPanel.SetDiagnoseStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetVitaminesView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, Sprite cureButtonIcon, string stageTitle, List<TreatmentPanelData> treatmentDataList)
        {
            curePanel.SetCurePanel(exp, cureButtonAction, null, cureButtonIcon);
            treatmentStagePanel.SetVitaminesView(stageTitle, treatmentDataList);
            stagesPanel.SetVitaminesStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetWaitingForLaborView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string speedupCost, Sprite currencyIcon, string laborTitle)
        {
            curePanel.SetCurePanel(exp, cureButtonAction, speedupCost, currencyIcon);
            treatmentStagePanel.SetWaitingForLaborView(laborTitle);
            stagesPanel.SetWaitingForLaborStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetReadyForLaborView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string buttonText, string laborTitle)
        {
            curePanel.SetCurePanel(exp, cureButtonAction, buttonText);
            treatmentStagePanel.SetReadyForLaborView(laborTitle);
            stagesPanel.SetWaitingForLaborStageHighlighted();

            SetTreatmentObjectsActive(patientInfo);
        }

        public void SetInLaborView(MaternityPatientInfo patientInfo, string exp, string waitingText, string laborTitle, UnityAction laborButtonAction, string speedupPrice, Sprite currencyIcon)
        {
            curePanel.SetCurePanel(exp, laborButtonAction, speedupPrice, currencyIcon);
            motherInfoPanel.SetPatientInfoView(patientInfo);
            babyInfoPanel.SetPatientWaitingView(laborTitle);
            treatmentStagePanel.SetLaborInProgressView(laborTitle);
            stagesPanel.SetInLaborStageHighlighted();

            SetLaborObjectsActive();
            babyInfoPanel.SetPatientInfoPanelActive(true);

        }

        public void SetLaborEndedView(MaternityPatientInfo patientInfo, string exp, string endedText, string laborTitle, UnityAction laborButtonAction, string laborButtonText)
        {
            curePanel.SetCurePanel(exp, laborButtonAction, laborButtonText);
            motherInfoPanel.SetPatientInfoView(patientInfo);
            babyInfoPanel.SetPatientWaitingView(endedText);
            treatmentStagePanel.SetLaborEndedView(laborTitle);
            stagesPanel.SetInLaborStageHighlighted();

            SetLaborObjectsActive();

        }

        public void SetHealingAndBoundingGiftTimerView(MaternityPatientInfo patientInfo, MaternityPatientInfo babyInfo, string exp, string treatmentTitle, UnityAction buttonAction, string speedupPrice, Sprite currencyIcon)
        {
            curePanel.SetCurePanel(exp, buttonAction, speedupPrice, currencyIcon);
            motherInfoPanel.SetPatientInfoView(patientInfo);
            babyInfoPanel.SetPatientInfoView(babyInfo);
            treatmentStagePanel.SetGiftTimerView(treatmentTitle, IsABoy(babyInfo));
            stagesPanel.SetHealingAndBondingStageHighlighted();

            SetLaborObjectsActive();
        }

        public void SetHealingAndBoundingGiftReadyView(MaternityPatientInfo patientInfo, MaternityPatientInfo babyInfo, string exp, string treatmentTitle, UnityAction buttonAction, string speedupPrice, Sprite currencyIcon)
        {
            curePanel.SetCurePanel(exp, buttonAction, speedupPrice, currencyIcon);
            motherInfoPanel.SetPatientInfoView(patientInfo);
            babyInfoPanel.SetPatientInfoView(babyInfo);
            treatmentStagePanel.SetGiftReadyView(treatmentTitle, IsABoy(babyInfo));
            stagesPanel.SetHealingAndBondingStageHighlighted();

            SetLaborObjectsActive();
        }
        /// <summary>
        /// Currently not called anywhere so not used.
        /// </summary>
        public void SetHealingAndBoundingEndedView(MaternityPatientInfo patientInfo, MaternityPatientInfo babyInfo, string exp, string treatmentTitle, UnityAction buttonAction, string buttonText, string infoText)
        {
            curePanel.SetCurePanel(exp, buttonAction, buttonText);
            motherInfoPanel.SetPatientInfoView(patientInfo);
            babyInfoPanel.SetPatientInfoView(babyInfo);
            treatmentStagePanel.SetSendHomeView(treatmentTitle, infoText);
            stagesPanel.SetHealingAndBondingStageHighlighted();

            SetLaborObjectsActive();
        }
        #endregion

        public Vector3 GetExperienceSource()
        {
            return curePanel.GetExpSource();
        }

        #region Buttons
        public void SetButtonTreatmentStage(UnityAction action, string buttonText, Sprite buttonIcon = null, bool isBlinking = false)
        {
            curePanel.SetCureButton(action, buttonText, buttonIcon, isBlinking);
        }

        public void SetCureButtonCurePanel(UnityAction cureButtonAction, string cureButtonText, Sprite buttonIcon = null, bool isBlinking = false)
        {
            curePanel.SetCureButton(cureButtonAction, cureButtonText, buttonIcon, isBlinking);
        }

        public void SetButtonBedInfoPanel(UnityAction action, string buttonText, Sprite buttonIcon = null, bool isBlinking = false)
        {
            bedInfoPanel.SetButton(action, buttonText, buttonIcon, isBlinking);
        }

        public void SetCureButtonInteractive(bool isInteractive)
        {
            curePanel.SetCureButtonInteractive(isInteractive);
        }
        #endregion

        #region Timers

        public void SetTimerTreatmentTreatmentStagePanel(string timerText)
        {
            treatmentStagePanel.SetTreatmentTimer(timerText);
        }

        public void SetTimerDiagnoseTreatmentStagePanel(string timerText)
        {
            treatmentStagePanel.SetDiagnoseTimer(timerText);
        }

        public void SetTimerBedInfoPanelWaiting(string timerText)
        {
            bedInfoPanel.SetWaitingTimer(timerText);
        }

        public void SetClockFill(float fill)
        {
            treatmentStagePanel.SetClockFill(fill);
        }

        #endregion

        #region Objectactivation
        private void ClearPanel()
        {
            motherInfoPanel.SetPatientInfoPanelActive(false);
            babyInfoPanel.SetPatientInfoPanelActive(false);
            bedInfoPanel.SetBedPanelInfoActive(false);
            emptyMachinePanel.SetEmptyMachinePanelActive(false);
            curePanel.SetCurePanelActive(false);
            treatmentStagePanel.SetTreatmentStagePanelActive(false);
            stagesPanel.SetStagesPanelActive(false);
            SetCureButtonInteractive(true);
        }

        private void SetEmptyRoomObjectsActive()
        {
            ClearPanel();

            bedInfoPanel.SetBedPanelInfoActive(true);
            emptyMachinePanel.SetEmptyMachinePanelActive(true);
        }

        private void SetTreatmentObjectsActive(MaternityPatientInfo patientInfo)
        {
            ClearPanel();
            babyInfoPanel.SetPatientWaitingObjectPreLabour(true);
            babyInfoPanel.gameObject.SetActive(true);
            motherInfoPanel.SetPatientInfoPanelActive(true);
            curePanel.SetCurePanelActive(true);
            treatmentStagePanel.SetTreatmentStagePanelActive(true);
            stagesPanel.SetStagesPanelActive(true);
            motherInfoPanel.SetPatientInfoView(patientInfo);

            //motherAvatar.SetPatientAvatarActive(true);
        }

        private void SetLaborObjectsActive()
        {
            ClearPanel();


            babyInfoPanel.SetPatientInfoPanelActive(true);
            curePanel.SetCurePanelActive(true);
            motherInfoPanel.SetPatientInfoPanelActive(true);
            treatmentStagePanel.SetTreatmentStagePanelActive(true);
            stagesPanel.SetStagesPanelActive(true);
        }
        #endregion

        private bool IsABoy(MaternityPatientInfo patientInfo)
        {
            return patientInfo.gender == PatientAvatarUI.PatientBackgroundType.boyBaby;
        }
    }
}
