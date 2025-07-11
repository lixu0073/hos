using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Maternity.UI
{
    public class MaternityBedPanelUI : MonoBehaviour, IMaternityBedPanelUI
    {
        [SerializeField]
        private GameObject motherContent = null;
        [SerializeField]
        private GameObject babyContent = null;
        [SerializeField]
        private GameObject roomContent = null;
        [SerializeField]
        private GameObject roomRequiredBadge = null;

        [SerializeField]
        private PatientAvatarUI motherAvatar = null;
        [SerializeField]
        private PatientAvatarUI babyAvatar = null;
        [SerializeField]
        private StageIndicatorUI stageIndicator = null;
            
        [SerializeField]
        private ButtonUI bedPanelButton = null;

        [SerializeField]
        private TextMeshProUGUI patientTimer = null;
        [SerializeField]
        private GameObject selectedIndicator = null;

        public void SetBedPanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void SetSelectedIndicatorActive(bool setActive)
        {
            selectedIndicator.SetActive(setActive);
        }

        public void DestroyBedPanel()
        {
            Destroy(gameObject);
        }
        #region Set View
        public void SetLaborRoomRequiredView(UnityAction buttonAction)
        {
            SetBedPanelButton(buttonAction);

            SetRoomObjectsActive();
        }

        public void SetWaitingForNextPatientView(UnityAction buttonAction)
        {
            SetBedPanelButton(buttonAction);
            motherAvatar.SetTimerView();
            babyAvatar.SetUnknownView();

            SetPatientObjectsActive();
            SetWaitingForNextPatientObjectActive(true);
        }

        public void SetNextPatientOnHisWayView(UnityAction buttonAction)
        {
            SetBedPanelButton(buttonAction);
            motherAvatar.SetOnWayView();
            babyAvatar.SetUnknownView();

            SetPatientObjectsActive();
        }

        private void SetTreatmentView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetBedPanelButton(buttonAction);
            motherAvatar.SetAvatarView(head, body);
            babyAvatar.SetUnknownView();

            SetPatientObjectsActive();
            SetPatientIndicatorActive(true);
        }

        public void SetDiagnoseRequiredView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetDiagnosisStage(buttonAction, head, body);
        }

        public void SetDiagnoseInQueueView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetDiagnosisStage(buttonAction, head, body);
        }

        public void SetDiagnoseInProgressView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetDiagnosisStage(buttonAction, head, body);
        }

        public void SetDiagnosisStage(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetTreatmentView(buttonAction, head, body);
            SetPatientIndicator("1", ResourcesHolder.GetMaternity().diagnosisStageBadge);

        }

        public void SetVitaminesView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetTreatmentView(buttonAction, head, body);
            SetPatientIndicator("2", ResourcesHolder.GetMaternity().vitaminesBadge);
        }

        public void SetWaitingForLaborView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetWaitingForLaborStage(buttonAction, head, body);
        }

        public void SetReadyForLaborView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetWaitingForLaborStage(buttonAction, head, body);
        }

        public void SetWaitingForLaborStage(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetTreatmentView(buttonAction, head, body);
            SetPatientIndicator("3", ResourcesHolder.GetMaternity().waitingStageBadge);
        }

        public void SetInLaborView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetrLaborStage(buttonAction, head, body);
        }

        public void SetLaborEndedView(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetrLaborStage(buttonAction, head, body);
        }

        public void SetrLaborStage(UnityAction buttonAction, Sprite head, Sprite body)
        {
            SetTreatmentView(buttonAction, head, body);
            SetPatientIndicator("4", ResourcesHolder.GetMaternity().inLaborStageBadge);
        }


        public void SetHealingAndBoundingGiftView(UnityAction buttonAction, Sprite head_mother, Sprite body_mother, Sprite head_baby, Sprite body_baby, PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            SetAfterLaborView(buttonAction, head_mother, body_mother, head_baby, body_baby, backgroundType);
        }

        public void SetHealingAndBoundingEndedView(UnityAction buttonAction, Sprite head_mother, Sprite body_mother, Sprite head_baby, Sprite body_baby, PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            SetAfterLaborView(buttonAction, head_mother, body_mother, head_baby, body_baby, backgroundType);
        }


        public void SetAfterLaborView(UnityAction buttonAction, Sprite head_mother, Sprite body_mother, Sprite head_baby, Sprite body_baby, PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            SetBedPanelButton(buttonAction);
            motherAvatar.SetAvatarView(head_mother, body_mother);
            babyAvatar.SetAvatarView(head_baby, body_baby, backgroundType);
            SetPatientIndicator("5", ResourcesHolder.GetMaternity().bondingStageBadge);

            SetPatientObjectsActive();
            SetPatientIndicatorActive(true);
        }

        public void SetNewPatientBadge(bool setActive)
        {
            SetPatientIndicator("!", ResourcesHolder.GetMaternity().newPatientBadge);
        }

        public void SetRoomRequiredBadgeActive(bool setActive)
        {
            roomRequiredBadge.SetActive(setActive);
        }
        #endregion

        public void SetPatientTimer(string timer)
        {
            patientTimer.text = timer;
        }

        public void SetBedPanelButton(UnityAction buttonAction, bool isBlinking = false)
        {
            bedPanelButton.SetButton(buttonAction, null, null, isBlinking);
        }

        #region Object Activation
        private void ClearPanel()
        {
            motherContent.SetActive(false);
            babyContent.SetActive(false);
            roomContent.SetActive(false);
            SetPatientIndicatorActive(false);
            patientTimer.gameObject.SetActive(false);
        }

        public void SetRoomObjectsActive()
        {
            ClearPanel();

            roomContent.SetActive(true);
        }

        public void SetPatientObjectsActive()
        {
            ClearPanel();

            motherContent.SetActive(true);
            babyContent.SetActive(true);
        }

        public void SetWaitingForNextPatientObjectActive(bool setActive)
        {
            patientTimer.gameObject.SetActive(setActive);
        }
        
        public void SetPatientIndicatorActive(bool setActive)
        {
            stageIndicator.SetStageIndicatorActive(setActive);
        }

        private void SetPatientIndicator(string badgeText, Sprite stageImage)
        {
            stageIndicator.SetStageIndicator(badgeText, stageImage);
        }

        #endregion
    }
}
