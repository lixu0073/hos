using Maternity.Adapter;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maternity.UI
{
    public interface IMaternityTreatmentPanelUI
    {
        void SetButtonBedInfoPanel(UnityAction action, string buttonText, Sprite buttonIcon = null, bool isBlinking = false);
        void SetButtonTreatmentStage(UnityAction action, string buttonText, Sprite buttonIcon = null, bool isBlinking = false);
        void SetCureButtonCurePanel(UnityAction cureButtonAction, string cureButtonText, Sprite buttonIcon = null, bool isBlinking = false);
        void SetCureButtonInteractive(bool isInteractive);
        void SetDiagnoseEndedView(MaternityPatientInfo patientInfo, string exp,  UnityAction cureButtonAction, string cureButtonText, string stageTitle, string diagnoseStatus);
        void SetDiagnoseInProgressView(MaternityPatientInfo patientInfo, string exp,  UnityAction cureButtonAction, string cureButtonText, string stageTitle, string diagnoseStatus);
        void SetDiagnoseInQueueView(MaternityPatientInfo patientInfo, string exp,  UnityAction cureButtonAction, string cureButtonText, string stageTitle, string inQueue);
        void SetDiagnoseRequiredView(MaternityPatientInfo patientInfo, string exp,  UnityAction cureButtonAction, string buttontext, string stageTitle, string diagnoseCost, string diagnoseTime, Sprite buttonSprite, bool isBlinking = false);
        void SetHealingAndBoundingEndedView(MaternityPatientInfo patientInfo, MaternityPatientInfo babyInfo, string exp, string treatmentTitle, UnityAction buttonAction, string buttonText, string infoText);
        void SetHealingAndBoundingGiftReadyView(MaternityPatientInfo patientInfo, MaternityPatientInfo babyInfo, string exp, string treatmentTitle, UnityAction buttonAction, string speedupPrice, Sprite currencyIcon);
        void SetHealingAndBoundingGiftTimerView(MaternityPatientInfo patientInfo, MaternityPatientInfo babyInfo, string exp, string treatmentTitle, UnityAction buttonAction, string speedupPrice, Sprite currencyIcon);
        void SetInLaborView(MaternityPatientInfo patientInfo, string exp, string waitingText, string laborTitle, UnityAction laborButtonAction, string speedupPrice, Sprite currencyIcon);
        void SetLaborEndedView(MaternityPatientInfo patientInfo, string exp, string endedText, string laborTitle, UnityAction laborButtonAction, string laborButtonText);
        void SetLaborRoomRequiredView(string info, UnityAction buttonAction, string buttonText, Sprite buttonIcon, Sprite machineIcon);
        void SetNextPatientOnHisWayView(string info, string infoText);
        void SetReadyForLaborView(MaternityPatientInfo patientInfo, string exp,  UnityAction cureButtonAction, string buttonText, string laborTitle);
        void SetTimerBedInfoPanelWaiting(string timerText);
        void SetTimerDiagnoseTreatmentStagePanel(string timerText);
        void SetTimerTreatmentTreatmentStagePanel(string timerText);
        void SetVitaminesView(MaternityPatientInfo patientInfo, string exp,  UnityAction cureButtonAction, Sprite cureButtonIcon, string stageTitle, List<TreatmentPanelData> treatmentDataList);
        void SetWaitingForLaborView(MaternityPatientInfo patientInfo, string exp, UnityAction cureButtonAction, string speedupCost, Sprite currencyIcon, string laborTitle);
        void SetWaitingForNextPatientView(UnityAction buttonAction, string buttonText, Sprite buttonIcon, string infoText);
        Vector3 GetExperienceSource();
        void SetClockFill(float fill);
    }
}