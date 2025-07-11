using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

namespace Maternity.UI
{
    public class MaternityPatientPanelUI : MonoBehaviour, IMaternityPatientPanelUI
    {
        [SerializeField]
        private GameObject nameLabel = null;
        [SerializeField]
        private GameObject separator = null;
        [SerializeField]
        private GameObject onTheWayBadge = null;
        [SerializeField]
        private GameObject timerBadge = null;
        [SerializeField]
        private GameObject waitingTimerLabel = null;
        [SerializeField]
        private GameObject getNowLabel = null;

        [SerializeField]
        private ButtonUI treatmentButton = null;
        [SerializeField]
        private MaternityVitaminesPanelUI vitaminesPanel = null;
        [SerializeField]
        private RewardAreaUI rewardArea = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;
        [SerializeField]
        private TextMeshProUGUI treatmentLabel = null;
        [SerializeField]
        private TextMeshProUGUI diagnoseStatus = null;
        [SerializeField]
        private TextMeshProUGUI diagnoseAndGiftTimer = null;
        [SerializeField]
        private Vector3 diagnoseTimerPosition = Vector3.zero;
        [SerializeField]
        private Vector3 giftTimerPosition = Vector3.zero;
        private float timerHeights = 20.0f;
        [SerializeField]
        private TextMeshProUGUI treatmentTimer = null;
        [SerializeField]
        private TextMeshProUGUI waitingTimer = null;
        [SerializeField]
        private TextMeshProUGUI emptyBedLabel = null;

        //[SerializeField]
        //private Image treatmentIcon = null;
        //[SerializeField]
        //private Image treatmentIconAddition = null;
        //[SerializeField]
        //private Image roomIcon = null;

        [Header("Treatment Objects")]
        [SerializeField]
        private GameObject diagnoseRequiredObject = null;
        [SerializeField]
        private GameObject duringDiagnoseObject = null;
        [SerializeField]
        private GameObject readyForLabourObject = null;
        [SerializeField]
        private GameObject StorkObject = null;
        [SerializeField]
        private GameObject resultsObject = null;
        [SerializeField]
        private GameObject babyObjectSleeping = null;
        [SerializeField]
        private GameObject waitingRoomObject = null;
        [SerializeField]
        private GameObject laborRoomObject = null;
        [SerializeField]
        private GameObject babyBoyGift = null;
        [SerializeField]
        private GameObject babyGirlGift = null;


        //[SerializeField]
        //private Sprite diagnoseIcon = null;
        //[SerializeField]
        //private Sprite storkIcon = null;
        //[SerializeField]
        //private Sprite readyForLabor = null;
        //[SerializeField]
        //private Sprite resultsIcon = null;
        //[SerializeField]
        //private Sprite waitingRoomIcon = null;
        //[SerializeField]
        //private Sprite laborRoomIcon = null;

        public void SetNurseRoomPanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        #region Set View

        public void SetLaborRoomRequiredView(UnityAction butonAction)
        {
            SetTreatmentButton(butonAction, I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonBuildKey));
            SetEmbtyBedLabel(I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.roomRequiredKey));
            SetLaborRoomObjectsActive();
            SetRoomObject(laborRoomObject);

            treatmentButton.gameObject.SetActive(true);
        }

        public void SetWaitingForNextPatientView(UnityAction butonAction, string speedupPrice)
        {
            SetTreatmentButton(butonAction, speedupPrice, ResourcesHolder.GetMaternity().diamondSprite);
            SetWaitingObjectsActive();
            SetRoomObject(waitingRoomObject);
        }

        public void SetNextPatientOnHisWayView()
        {
            SetEmbtyBedLabel(I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.nextOnWayKey));
            SetLaborRoomObjectsActive();
            SetRoomObject(waitingRoomObject);

            onTheWayBadge.SetActive(true);

        }

        public void SetDiagnoseRequiredView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle)
        {
            SetDiagnoseRequiredObjectsActive();
            SetDiagnoseObjects(patientName, expReward, butonAction, diagnoseTitle, diagnoseRequiredObject);

        }

        public void SetDiagnoseInQueueView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, string diagnoseStatus)
        {
            SetDiagnoseInQueueObjectsActive();
            SetDiagnoseObjects(patientName, expReward, butonAction, diagnoseTitle, duringDiagnoseObject);
            SetDiagnoseStatus(diagnoseStatus);

        }

        public void SetDiagnoseInProgressView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, string diagnoseStatus)
        {
            SetDiagnoseInProgressObjectsActive();
            SetDiagnoseObjects(patientName, expReward, butonAction, diagnoseTitle, duringDiagnoseObject);
            SetDiagnoseStatus(diagnoseStatus);

        }

        public void SetDiagnoseEndedView(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, string diagnoseStatus)
        {
            SetDiagnoseEndedObjectsActive();
            SetDiagnoseObjects(patientName, expReward, butonAction, diagnoseTitle, resultsObject);
            SetDiagnoseStatus(diagnoseStatus);

        }

        public void SetVitaminesView(string patientName, string expReward, UnityAction butonAction, List<TreatmentPanelData> treatmentDataList)
        {
            SetNameText(patientName);
            rewardArea.SetExpRewardAmount(expReward);
            SetTreatmentButton(butonAction, I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.seePatientCard));
            vitaminesPanel.SetVitaminesPanel(treatmentDataList);
            SetVitaminesObjectsActive();

        }

        public void SetWaitingForLaborView(string patientName, UnityAction butonAction, string treatmentTitle)
        {
            SetWaitingOrReadyForLaborObjectsActive();
            SetLaborObjects(patientName, butonAction, treatmentTitle);
            SetTreatmentObject(StorkObject, AnimHash.StorkUI);

        }

        public void SetReadyForLaborView(string patientName, UnityAction butonAction, string treatmentTitle)
        {
            SetWaitingOrReadyForLaborObjectsActive();
            SetLaborObjects(patientName, butonAction, treatmentTitle);
            SetTreatmentObject(readyForLabourObject);
        }

        public void SetLaborInProgressView(string patientName, UnityAction butonAction, string treatmentTitle)
        {
            SetLaborInProgressObjectsActive();
            SetLaborObjects(patientName, butonAction, treatmentTitle);
            SetTreatmentObject(babyObjectSleeping);
        }

        public void SetLaborEndedView(string patientName, UnityAction butonAction, string treatmentTitle)
        {
            SetLaborEndedObjectsActive();
            SetLaborObjects(patientName, butonAction, treatmentTitle);

        }

        public void SetHealingAndBoundingGiftTimerView(string patientName, UnityAction buttonAction, string treatmentTitle, string speedupPrice, bool isBoy)
        {
            SetGiftObjectsActive();
            SetGiftObjects(patientName, treatmentTitle, isBoy, buttonAction, speedupPrice, ResourcesHolder.GetMaternity().diamondSprite);
            diagnoseAndGiftTimer.gameObject.SetActive(true);
        }

        public void SetHealingAndBoundingGiftReadyView(string patientName, UnityAction buttonAction, string treatmentTitle, bool isBoy)
        {
            SetGiftObjectsActive();
            SetGiftObjects(patientName, treatmentTitle, isBoy, buttonAction, I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.seePatientCard));
        }

        #endregion

        private void SetDiagnoseObjects(string patientName, string expReward, UnityAction butonAction, string diagnoseTitle, GameObject diagnoseObject)
        {
            SetNameText(patientName);
            rewardArea.SetExpRewardAmount(expReward);
            SetTreatmentButton(butonAction, I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.seePatientCard));
            SetTreatmentLabel(diagnoseTitle);
            SetTreatmentObject(diagnoseObject, AnimHash.BloodResultDeliveredUI);
        }

        private void SetLaborObjects(string patientName, UnityAction butonAction, string treatmentTitle)
        {
            SetNameText(patientName);
            SetTreatmentButton(butonAction, I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.seePatientCard));
            SetTreatmentLabel(treatmentTitle);

        }

        private void SetGiftObjects(string patientName, string treatmentTitle, bool isBoy, UnityAction butonAction, string buttonText, Sprite buttonIcon = null)
        {
            SetNameText(patientName);
            SetTreatmentButton(butonAction, buttonText, buttonIcon);
            SetTreatmentLabel(treatmentTitle);
            SetTreatmentObject(isBoy ? babyBoyGift : babyGirlGift);
        }

        private void SetNameText(string patientName)
        {
            nameText.text = patientName;
        }

        private void SetTreatmentLabel(string treatmentTitle)
        {
            treatmentLabel.text = treatmentTitle;
        }

        private void SetEmbtyBedLabel(string labelText)
        {
            emptyBedLabel.text = labelText;
        }

        private void SetDiagnoseStatus(string status)
        {
            diagnoseStatus.text = status;
        }

        private void SetTreatmentObject(GameObject treatmentGameObject)
        {
            treatmentGameObject.SetActive(true);
        }

        private void SetTreatmentObject(GameObject treatmentGameObject, int animationHash)
        {
            treatmentGameObject.SetActive(true);
            try { 
                Animator animator = treatmentGameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play(animationHash, 0, 0.0f);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        private void SetRoomObject(GameObject roomGObject)
        {
            roomGObject.SetActive(true);
        }

        #region Timers
        public void SetDiagnoseAndGiftTimer(string timerText)
        {
            diagnoseAndGiftTimer.text = timerText;
        }

        public void SetTreatmentTimer(string timerText)
        {
            treatmentTimer.text = timerText;
        }

        public void SetWaitingTimer(string timerText)
        {
            waitingTimer.text = timerText;
        }
        #endregion

        public void SetTreatmentButton(UnityAction action, string buttonText, Sprite buttonIcon = null, bool isBlinking = false)
        {
            treatmentButton.SetButton(action, buttonText, buttonIcon, isBlinking);
        }

        #region ObjectActivation
        private void ClearPanel()
        {
            nameLabel.SetActive(false);
            nameText.gameObject.SetActive(false);
            separator.SetActive(false);
            onTheWayBadge.SetActive(false);
            timerBadge.SetActive(false);
            waitingTimerLabel.SetActive(false);
            getNowLabel.SetActive(false);
            treatmentButton.gameObject.SetActive(false);
            vitaminesPanel.SetVitaminesPanelActive(false);
            rewardArea.SetRewardAreaActive(false);
            treatmentLabel.gameObject.SetActive(false);
            diagnoseStatus.gameObject.SetActive(false);
            diagnoseAndGiftTimer.gameObject.SetActive(false);
            treatmentTimer.gameObject.SetActive(false);
            emptyBedLabel.gameObject.SetActive(false);
            waitingTimer.gameObject.SetActive(false);
            diagnoseRequiredObject.SetActive(false);
            duringDiagnoseObject.SetActive(false);
            readyForLabourObject.SetActive(false);
            StorkObject.SetActive(false);
            resultsObject.SetActive(false);
            babyObjectSleeping.SetActive(false);
            babyGirlGift.SetActive(false);
            babyBoyGift.SetActive(false);
            waitingRoomObject.SetActive(false);
            laborRoomObject.SetActive(false);
            waitingRoomObject.SetActive(false);
            laborRoomObject.SetActive(false);
        }

        public void SetLaborRoomObjectsActive()
        {
            ClearPanel();
            emptyBedLabel.gameObject.SetActive(true);
        }

        public void SetWaitingObjectsActive()
        {
            ClearPanel();

            treatmentButton.gameObject.SetActive(true);
            timerBadge.SetActive(true);
            waitingTimerLabel.SetActive(true);
            getNowLabel.SetActive(true);
            waitingTimer.gameObject.SetActive(true);
        }

        public void SetDiagnoseRequiredObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            rewardArea.SetRewardAreaActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetDiagnoseInQueueObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            rewardArea.SetRewardAreaActive(true);
            treatmentLabel.gameObject.SetActive(true);
            diagnoseStatus.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetDiagnoseInProgressObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            rewardArea.SetRewardAreaActive(true);
            treatmentLabel.gameObject.SetActive(true);
            diagnoseStatus.gameObject.SetActive(true);
            diagnoseAndGiftTimer.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
            diagnoseAndGiftTimer.rectTransform.offsetMin = new Vector3(-90, diagnoseTimerPosition.y, 0);
            diagnoseAndGiftTimer.rectTransform.offsetMax = new Vector3(90, diagnoseTimerPosition.y + timerHeights, 0);
        }

        public void SetDiagnoseEndedObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            rewardArea.SetRewardAreaActive(true);
            treatmentLabel.gameObject.SetActive(true);
            diagnoseStatus.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetVitaminesObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            vitaminesPanel.SetVitaminesPanelActive(true);
            rewardArea.SetRewardAreaActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetWaitingOrReadyForLaborObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetLaborInProgressObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentTimer.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetLaborEndedObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
        }

        public void SetGiftObjectsActive()
        {
            ClearPanel();

            nameLabel.SetActive(true);
            nameText.gameObject.SetActive(true);
            separator.SetActive(true);
            treatmentLabel.gameObject.SetActive(true);
            treatmentButton.gameObject.SetActive(true);
            diagnoseAndGiftTimer.rectTransform.offsetMin = new Vector3(-90, giftTimerPosition.y, 0);
            diagnoseAndGiftTimer.rectTransform.offsetMax = new Vector3(90, giftTimerPosition.y + timerHeights, 0);
        }

        #endregion
    }
}
