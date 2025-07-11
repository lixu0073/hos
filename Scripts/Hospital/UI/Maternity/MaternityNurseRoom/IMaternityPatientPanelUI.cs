using System.Collections.Generic;  
using UnityEngine.Events;
using UnityEngine;

namespace Maternity.UI
{
    public interface IMaternityPatientPanelUI
    {
        void SetDiagnoseAndGiftTimer(string timerText);
        void SetDiagnoseEndedObjectsActive();
        void SetDiagnoseEndedView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, string diagnoseStatus);
        void SetDiagnoseInProgressObjectsActive();
        void SetDiagnoseInProgressView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, string diagnoseStatus);
        void SetDiagnoseInQueueObjectsActive();
        void SetDiagnoseInQueueView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, string diagnoseStatus);
        void SetDiagnoseRequiredObjectsActive();
        void SetDiagnoseRequiredView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle);
        void SetGiftObjectsActive();
        void SetHealingAndBoundingGiftReadyView(string patientName, UnityAction buttonAction, string treatmentTitle, bool isBoy);
        void SetHealingAndBoundingGiftTimerView(string patientName, UnityAction buttonAction, string treatmentTitle, string speedupPrice, bool isBoy);
        void SetLaborEndedObjectsActive();
        void SetLaborEndedView(string patientName, UnityAction butonAction, string treatmentTitle);
        void SetLaborInProgressObjectsActive();
        void SetLaborInProgressView(string patientName, UnityAction butonAction, string treatmentTitle);
        void SetLaborRoomObjectsActive();
        void SetLaborRoomRequiredView(UnityAction butonAction);
        void SetNextPatientOnHisWayView();
        void SetNurseRoomPanelActive(bool setActive);
        void SetReadyForLaborView(string patientName, UnityAction butonAction, string treatmentTitle);
        void SetTreatmentButton(UnityAction action, string buttonText, Sprite buttonIcon = null, bool isBlinking = false);
        void SetTreatmentTimer(string timerText);
        void SetVitaminesObjectsActive();
        void SetVitaminesView(string patientName, string expReward, UnityAction butonAction, List<TreatmentPanelData> treatmentDataList);
        void SetWaitingForLaborView(string patientName, UnityAction butonAction, string treatmentTitle);
        void SetWaitingForNextPatientView(UnityAction butonAction, string speedupPrice);
        void SetWaitingObjectsActive();
        void SetWaitingOrReadyForLaborObjectsActive();
        void SetWaitingTimer(string timerText);
    }
}