using UnityEngine;
using UnityEngine.Events;

namespace Maternity.UI
{
    public interface IMaternityBedPanelUI
    {
        void DestroyBedPanel();
        void SetAfterLaborView(UnityAction buttonAction, Sprite head_mother, Sprite body_mother, Sprite head_baby, Sprite body_baby, PatientAvatarUI.PatientBackgroundType backgroundType);
        void SetBedPanelActive(bool setActive);
        void SetBedPanelButton(UnityAction buttonAction, bool isBlinking = false);
        void SetDiagnoseInProgressView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetDiagnoseInQueueView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetDiagnoseRequiredView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetDiagnosisStage(UnityAction buttonAction, Sprite head, Sprite body);
        void SetHealingAndBoundingEndedView(UnityAction buttonAction, Sprite head_mother, Sprite body_mother, Sprite head_baby, Sprite body_baby, PatientAvatarUI.PatientBackgroundType backgroundType);
        void SetHealingAndBoundingGiftView(UnityAction buttonAction, Sprite head_mother, Sprite body_mother, Sprite head_baby, Sprite body_baby, PatientAvatarUI.PatientBackgroundType backgroundType);
        void SetInLaborView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetLaborEndedView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetLaborRoomRequiredView(UnityAction buttonAction);
        void SetNewPatientBadge(bool setActive);
        void SetNextPatientOnHisWayView(UnityAction buttonAction);
        void SetPatientIndicatorActive(bool setActive);
        void SetPatientObjectsActive();
        void SetPatientTimer(string timer);
        void SetReadyForLaborView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetrLaborStage(UnityAction buttonAction, Sprite head, Sprite body);
        void SetRoomObjectsActive();
        void SetRoomRequiredBadgeActive(bool setActive);
        void SetVitaminesView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetWaitingForLaborStage(UnityAction buttonAction, Sprite head, Sprite body);
        void SetWaitingForLaborView(UnityAction buttonAction, Sprite head, Sprite body);
        void SetWaitingForNextPatientObjectActive(bool setActive);
        void SetWaitingForNextPatientView(UnityAction buttonAction);
        void SetSelectedIndicatorActive(bool setActive);
    }
}