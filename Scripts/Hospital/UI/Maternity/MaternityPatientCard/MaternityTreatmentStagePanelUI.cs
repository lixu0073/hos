using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Maternity.UI
{
    public class MaternityTreatmentStagePanelUI : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        MaternityPatientCardAnimationObjects animationObjects;
#pragma warning restore 0649
        [SerializeField]
        private MaternityVitaminesPanelUI vitaminesPanel = null;

        [SerializeField]
        private TextMeshProUGUI treatmentLabel = null;
        [SerializeField]
        private TextMeshProUGUI treatmentTimer = null;
        [SerializeField]
        private TextMeshProUGUI healingAndBondingInfo = null;
        [SerializeField]
        private Image ClockFill = null;

        public void SetTreatmentStagePanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        #region Set View
        public void SetDiagnoseRequiredView(string stageTitle, string time)
        {
            SetDiagnoseLabel(stageTitle);
            SetTime(time);

            SetDiagnoseRequiredObjectsActive();
        }

        public void SetDiagnoseInQueueView(string stageTitle,string inQueue)
        {
            SetDiagnoseLabel(stageTitle);
            SetTime(inQueue);

            SetDiagnoseInQueueObjectsActive();
        }

        public void SetDiagnoseInProgressView(string stageTitle, string status)
        {
            SetDiagnoseLabel(stageTitle);
            SetDiagnoseStatusText(status);

            SetDiagnoseInProgressObjectsActive();
        }

        public void SetDiagnoseEndedView(string stageTitle, string status)
        {
            SetDiagnoseLabel(stageTitle);
            SetDiagnoseStatusText(status);

            SetDiagnoseResultsObjectsActive();
        }

        public void SetVitaminesView(string stageTitle, List<TreatmentPanelData> treatmentDataList)
        {
            SetDiagnoseLabel(stageTitle);
            vitaminesPanel.SetVitaminesPanel(treatmentDataList);

            SetVitaminesObjectsActive();
        }

        public void SetWaitingForLaborView(string treatmentTitle)
        {
            SetTreatmentLabel(treatmentTitle);
            SetWaitingForLaborObjectsActive();
        }

        public void SetReadyForLaborView(string treatmentTitle)
        {
            SetTreatmentLabel(treatmentTitle);
            SetReadyForLaborObjectsActive();
        }

        public void SetLaborInProgressView(string treatmentTitle)
        {
            SetTreatmentLabel(treatmentTitle);
            SetLaborInProgressObjectsActive();
        }

        public void SetLaborEndedView(string treatmentTitle)
        {
            SetTreatmentLabel(treatmentTitle);
            SetLaborEndedObjectsActive();
        }

        public void SetGiftTimerView(string treatmentTitle, bool isBoy)
        {
            SetTreatmentLabel(treatmentTitle);
            SetGiftTimerObjectsActive();
            SetAnimationGift(isBoy);

        }

        public void SetGiftReadyView(string treatmentTitle, bool isBoy)
        {
            SetTreatmentLabel(treatmentTitle);
            SetGiftReadyObjectsActive();
            SetAnimationGift(isBoy);

        }

        public void SetSendHomeView(string treatmentTitle, string infoText)
        {
            SetTreatmentLabel(treatmentTitle);
            SetHealingAndBondingInfo(infoText);
            SetSendHomeObjectsActive();
        }
        #endregion

        public void SetClockFill(float fill)
        {
            ClockFill.fillAmount = fill;
        }

        #region Timers        
        public void SetDiagnoseTimer(string timerText)
        {
            treatmentTimer.text = timerText;
        }

        public void SetTreatmentTimer(string timerText)
        {
            treatmentTimer.text = timerText;
        }
        #endregion

        private void SetDiagnoseLabel(string stageTitle)
        {
            treatmentLabel.text = stageTitle;
        }

        private void SetTime(string time)
        {
            treatmentTimer.text = time;
        }
        
        private void SetDiagnoseStatusText(string status)
        {

            treatmentLabel.text = status;
        }

        private void SetTreatmentLabel(string treatmentTitle)
        {
            treatmentLabel.text = treatmentTitle;
        }

        private void SetHealingAndBondingInfo(string infoText)
        {
            healingAndBondingInfo.text = infoText;
        }

        #region Object Activation
        private void ClearPanel()
        {
            animationObjects.Clear();
            vitaminesPanel.SetVitaminesPanelActive(false);
            treatmentLabel.gameObject.SetActive(false);
            treatmentTimer.gameObject.SetActive(false);
            healingAndBondingInfo.gameObject.SetActive(false);
        }

        private void SetDiagnoseRequiredObjectsActive()
        {
            ClearPanel();

            animationObjects.bloodSample.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
        }

        private void SetDiagnoseInQueueObjectsActive()
        {
            ClearPanel();

            animationObjects.bloodSample.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
        }

        private void SetDiagnoseInProgressObjectsActive()
        {
            ClearPanel();

            animationObjects.bloodSampleAndMagnifier.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);

        }

        private void SetDiagnoseResultsObjectsActive()
        {
            ClearPanel();
            animationObjects.resoultsDelivered.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
        }

        private void SetVitaminesObjectsActive()
        {
            ClearPanel();
            treatmentLabel.gameObject.SetActive(true);
            vitaminesPanel.SetVitaminesPanelActive(true);
        }

        private void SetWaitingForLaborObjectsActive()
        {
            ClearPanel();
            animationObjects.stork.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
        }

        private void SetReadyForLaborObjectsActive()
        {
            ClearPanel();
            animationObjects.readyForLabour.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
        }

        private void SetLaborInProgressObjectsActive()
        {
            ClearPanel();

            animationObjects.babySleeping.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
        }

        private void SetLaborEndedObjectsActive()
        {
            ClearPanel();
            animationObjects.babyAwaken.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
        }

        private void SetGiftTimerObjectsActive()
        {
            ClearPanel();

            treatmentLabel.gameObject.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
        }

        private void SetGiftReadyObjectsActive()
        {
            ClearPanel();
            treatmentLabel.gameObject.SetActive(true);
        }

        private void SetSendHomeObjectsActive()
        {
            ClearPanel();

            treatmentLabel.gameObject.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
            healingAndBondingInfo.gameObject.SetActive(true);
        }

        private void SetAnimationGift(bool isBoy)
        {
            if (isBoy)
            {
                animationObjects.giftBoy.SetActive(true);
            }
            else
            {
                animationObjects.giftGirl.SetActive(true);
            }
        }
        #endregion

    }
}
