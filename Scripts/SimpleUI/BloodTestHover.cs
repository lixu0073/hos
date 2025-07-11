using Hospital;
using IsoEngine;
using Maternity;
using Maternity.PatientStates;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
    public class BloodTestHover : BaseHover
    {
        private const string NO_PATIENT_AVAILABLE_KEY = "MATERNITY_NO_PATIENT_AVAILABLE";

        [SerializeField]
        Image currentPatientImageHead = null;
        [SerializeField]
        Image currentPatientImageBody = null;
        [SerializeField]
        GameObject patientContainer = null;
        [SerializeField]
        TextMeshProUGUI timeCounter = null;
        [SerializeField]
        TextMeshProUGUI machineName = null;
        [SerializeField]
        TextMeshProUGUI expandQueueCostText = null;
        [SerializeField]
        GameObject SpeedUpButton = null;
        [SerializeField]
        TextMeshProUGUI SpeedUpCostText = null;
#pragma warning disable 0649
        [SerializeField]
        GameObject CurrentPatientText;
#pragma warning restore 0649
        [SerializeField]
        GameObject Waiting = null;
        [SerializeField]
        GameObject Glow = null;
        [SerializeField]
        PointerDownListener currentPatientListener = null;
#pragma warning disable 0649
        [SerializeField]
        GameObject ShowPatientCard;
        [SerializeField]
        TextMeshProUGUI patientsToDiagnoseCounterText = null;
        [SerializeField]
        GameObject counterIndicator;
#pragma warning restore 0649
        private int diamondCostForSpeedUp = 0;

        private bool initialized;
        private int PatientWaitingToSendForBloodTestCounter = 0;

        int waitingPatientsAmount;
        Queue<IMaternityFacilityPatient> bloodPatientsQueue;
        MaternityBloodTestRoom room;
        private GameObject prefab;

        private Vector3 counterPosition = Vector3.zero;
        private static BloodTestHover hover;

        public static BloodTestHover Open()
        {
            if (hover == null)
            {
                var p = Instantiate(UIController.getMaternity.bloodTestHoverPrefab);
                hover = p.GetComponent<BloodTestHover>();
                hover.Init();
            }
            BaseGameState.isHoverOn = true;
            return hover;
        }

        public override void Close()
        {
            base.Close();
            BaseGameState.isHoverOn = false;
            ShowPatientCard.transform.SetParent(CurrentPatientText.transform.parent);
            ClearPatientContainer();
            if (bloodPatientsQueue.Count > 0)
            {
                bloodPatientsQueue.Peek().GetPatientAI().OnDataRecieved_ID -= BloodTestHover_OnDataRecieved_ID;
            }
            initialized = false;
        }

        void ClearPatientContainer()
        {
            for (int i = 0; i < patientContainer.transform.childCount - 1; i++)
                Destroy(patientContainer.transform.GetChild(i).gameObject);
        }

        public void Initialize(Vector2 worldPosition, Queue<IMaternityFacilityPatient> patients, int queueSize, MaternityBloodTestRoom room, int patientsForBloodTest)
        {
            base.Initialize();
            gameObject.SetActive(true);
            SetWorldPointHovering(new Vector3(worldPosition.x, 0, worldPosition.y));
            initialized = true;
            InitializeContainer(queueSize);
            this.room = room;
            bloodPatientsQueue = patients;

            counterIndicator.SetActive(false);

            counterIndicator.GetComponent<Image>().sprite = ResourcesHolder.GetMaternity().diagnosisBadgeGfx;

            if (counterPosition == Vector3.zero)
            {
                counterPosition = counterIndicator.GetComponent<RectTransform>().anchoredPosition;
            }

            PatientWaitingToSendForBloodTestCounter = patientsForBloodTest;
            patientsToDiagnoseCounterText.text = patientsForBloodTest.ToString();

            expandQueueCostText.text = DiamondCostCalculator.GetQueueSlotCost(queueSize).ToString();
            machineName.text = UIController.DivideStringIntoLines(I2.Loc.ScriptLocalization.Get(room.GetRoomInfo().ShopTitle), 19);
            SetShowPatientCardButton();
            ActualizePatientList(patients);
            if (bloodPatientsQueue.Count > 0)
            {
                Image img = SpeedUpButton.GetComponent<Image>();
                if (img != null)
                {
                    if (Game.Instance.gameState().IsMaternityFirstLoopCompleted)
                        TutorialUIController.Instance.StopBlinking();
                    else
                        TutorialUIController.Instance.BlinkImage(img);
                }
                bloodPatientsQueue.Peek().GetPatientAI().OnDataRecieved_ID += BloodTestHover_OnDataRecieved_ID;
            }
        }

        private void BloodTestHover_OnDataRecieved_ID(MaternityPatientInDiagnoseState.Data obj)
        {
            ActualizeCounterTime(obj.timeLeft);
            if (Game.Instance.gameState().IsMaternityFirstLoopCompleted)
            {
                diamondCostForSpeedUp = room.GetSpeedUpDiagnoseCost(obj.timeLeft);
                SpeedUpCostText.text = diamondCostForSpeedUp.ToString();
            }
            else
            {
                diamondCostForSpeedUp = 0;
                SpeedUpCostText.text = I2.Loc.ScriptLocalization.Get("FREE").ToUpper();
            }
        }

        public void OnPatientDequeue(IMaternityFacilityPatient oldPatient, IMaternityFacilityPatient newPatient)
        {
            oldPatient.GetPatientAI().OnDataRecieved_ID -= BloodTestHover_OnDataRecieved_ID;
            if (newPatient != null)
            {
                newPatient.GetPatientAI().OnDataRecieved_ID += BloodTestHover_OnDataRecieved_ID;
            }
        }

        private void InitializeContainer(int queueSize)
        {
            prefab = ResourcesHolder.GetMaternity().BloodHoverButton;
            EnlargeQueue(queueSize - 1);
        }

        public void EnlargeQueue(int count)
        {
            var p = patientContainer.transform.GetChild(patientContainer.transform.childCount - 1);
            for (int i = 0; i < count; i++)
            {
                var z = Instantiate(prefab);
                z.transform.SetParent(patientContainer.transform);
                z.transform.localScale = Vector3.one;
            }
            p.SetAsLastSibling();
            waitingPatientsAmount = patientContainer.transform.childCount;
            ActualizeEnlargeQueueCost();
        }

        public void HealNow()
        {
            if (bloodPatientsQueue.Count > 0)
            {
                if (SpeedUpDiamondTransactionMakerID != ID)
                {
                    InitializeID();
                    SpeedUpDiamondTransactionMakerID = ID;
                }
                room.HealNow(diamondCostForSpeedUp, this);
            }
        }

        public void ActualizeEnlargeQueueCost()
        {
            patientContainer.transform.GetChild((patientContainer.transform.childCount - 1))
                .GetChild(0)
                    .GetChild(0)
                    .GetComponent<TextMeshProUGUI>()
                    .text = DiamondCostCalculator.GetQueueSlotCost(waitingPatientsAmount).ToString();
        }

        public void EnlargeButton()
        {
            if (EnlargeDiamondTransactionMakerID != ID)
            {
                InitializeID();
                EnlargeDiamondTransactionMakerID = ID;
            }
            int diamondsNeeded = DiamondCostCalculator.GetQueueSlotCost(waitingPatientsAmount);
            if (Game.Instance.gameState().GetDiamondAmount() >= diamondsNeeded && room.CanEnlargeQueue())
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondsNeeded, delegate
                {
                    Game.Instance.gameState().RemoveDiamonds(diamondsNeeded, EconomySource.EnlargeQueue, room.Tag);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    room.EnlargeQueueByOne();
                    ActualizeEnlargeQueueCost();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
            SetShowPatientCardButton();
        }

        public void SetBuyingEnable(bool value)
        {
            patientContainer.transform.GetChild(patientContainer.transform.childCount - 1).gameObject.SetActive(value);
        }

        public void ActualizePatientList(Queue<IMaternityFacilityPatient> patients)
        {
            SetShowPatientCardButton();
            if (patients.Count <= 0)
            {
                SpeedUpButton.SetActive(false);
                currentPatientImageHead.enabled = false;
                currentPatientImageBody.enabled = false;
                currentPatientImageHead.material = null;
                currentPatientImageBody.material = null;
                Glow.SetActive(false);
                CurrentPatientText.SetActive(true);
                return;
            }
            Image avatarHead;
            Image avatarBody;
            if (patients.Count > 0)
            {
                CurrentPatientText.SetActive(false);
                currentPatientImageHead.enabled = true;
                currentPatientImageBody.enabled = true;
                currentPatientImageHead.sprite = patients.Peek().GetPatientAI().sprites.AvatarHead;
                currentPatientImageBody.sprite = patients.Peek().GetPatientAI().sprites.AvatarBody;
                if (patients.Peek().GetPatientAI().sprites.AvatarHead == null || patients.Peek().GetPatientAI().sprites.AvatarBody == null)
                {
                    currentPatientImageHead.enabled = false;
                    currentPatientImageBody.enabled = false;
                }
                else
                {
                    currentPatientImageHead.enabled = true;
                    currentPatientImageBody.enabled = true;
                }
                currentPatientImageHead.material = null;
                currentPatientImageBody.material = null;
                timeCounter.gameObject.SetActive(true);
                Glow.SetActive(true);
                SpeedUpButton.SetActive(true);
            }

            IMaternityFacilityPatient[] remainingPatiemts = patients.ToArray();
            for (int i = 0; i < remainingPatiemts.Length - 1; i++)
            {
                avatarHead = patientContainer.transform.GetChild(i).gameObject.transform.GetChild(2).GetComponent<Image>();
                avatarHead.enabled = true;
                avatarHead.sprite = remainingPatiemts[i + 1].GetPatientAI().sprites.AvatarHead;
                if (avatarHead.sprite == null)
                {
                    avatarHead.enabled = false;
                }

                patientContainer.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject.SetActive(false);

                avatarBody = patientContainer.transform.GetChild(i).gameObject.transform.GetChild(1).GetComponent<Image>();
                avatarBody.enabled = true;
                avatarBody.sprite = remainingPatiemts[i + 1].GetPatientAI().sprites.AvatarBody;
                if (avatarBody.sprite == null)
                {
                    avatarBody.enabled = false;
                }
            }
            if (gameObject.activeInHierarchy)
            {

                for (int i = 0; i < remainingPatiemts.Length - 1; i++)
                {
                    patientContainer.transform.GetChild(i).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                }

                for (int i = remainingPatiemts.Length - 1; i < room.QueueSize - 1; i++)
                {
                    if (i > -1)
                    {
                        avatarHead = patientContainer.transform.GetChild(i).gameObject.transform.GetChild(2).GetComponent<Image>();
                        avatarHead.enabled = false;
                        avatarBody = patientContainer.transform.GetChild(i).gameObject.transform.GetChild(1).GetComponent<Image>();
                        avatarBody.enabled = false;
                        patientContainer.transform.GetChild(i).gameObject.transform.GetChild(3).gameObject.SetActive(true);
                        patientContainer.transform.GetChild(i).gameObject.transform.GetChild(0).gameObject.SetActive(false);

                    }
                }
            }
            SetWaitingInfo();
        }

        void SetWaitingInfo()
        {
            if (bloodPatientsQueue.Count > 2)
                Waiting.SetActive(true);
            else
                Waiting.SetActive(false);
        }

        void SetShowPatientCardButton()
        {
            ShowPatientCard.SetActive(true);
            if (bloodPatientsQueue.Count == 0)
            {
                ShowPatientCard.transform.SetParent(CurrentPatientText.transform.parent);
                ShowPatientCard.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                ShowPatientCard.transform.SetAsLastSibling();

                if (PatientWaitingToSendForBloodTestCounter > 0)
                {
                    counterIndicator.GetComponent<RectTransform>().anchoredPosition = counterPosition;
                    counterIndicator.SetActive(true);
                }

            }
            else if (bloodPatientsQueue.Count >= 1 && bloodPatientsQueue.Count < patientContainer.transform.childCount) //&& waitingPatientsAmount > (bloodPatientsQueue.Count - 1)
            {
                ShowPatientCard.transform.SetParent(patientContainer.transform.GetChild(bloodPatientsQueue.Count - 1));
                ShowPatientCard.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                ShowPatientCard.transform.SetAsLastSibling();

                if (PatientWaitingToSendForBloodTestCounter > 0)
                {
                    counterIndicator.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    counterIndicator.SetActive(true);
                }
            }
            else
            {
                ShowPatientCard.SetActive(false);
                counterIndicator.SetActive(false);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (initialized)
            {
                //if (waitingPatientsAmount >= (bloodPatientsQueue.Count - 1) && (bloodPatientsQueue.Count > 0) && room != null)
                if (bloodPatientsQueue.Count == 0)
                {
                    ResetCounter();
                }
            }
        }

        private void ResetCounter()
        {
            timeCounter.text = "";
        }

        private void ActualizeCounterTime(float timeLeftForDiagnose)
        {
            if (room != null && bloodPatientsQueue.Count > 0)
            {
                //int timeToDiagnose = ((MaternityBloodTestRoomInfo)room.GetRoomInfo()).GetDiagnoseTime();
                int p = (int)(timeLeftForDiagnose);
                timeCounter.text = UIController.GetFormattedTime(p);
            }
        }

        public Vector3 GetAvatarPosition(int id)
        {
            if (id == 0)
                return currentPatientListener.transform.position;
            else
                return patientContainer.transform.GetChild(id - 1).gameObject.transform.position;
        }

        public override void SetHoverScale()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
                transform.localScale = Vector3.one * 0.6f;
            else
                transform.localScale = Vector3.one * 0.5f;
        }
        protected override void SetHoverFrame(int queueSize)
        {
            if (queueSize > 4)
            {
                hoverFrame.pivot = new Vector2(0.5f, 1f);
                hoverFrame.sizeDelta = new Vector2(850, 650);
                hoverFrame.pivot = new Vector2(0.5f, .5f);
            }
            else
            {
                hoverFrame.pivot = new Vector2(0.5f, 1f);
                hoverFrame.sizeDelta = new Vector2(900, 550);
                hoverFrame.pivot = new Vector2(0.5f, .5f);
            }
        }

        public static BloodTestHover GetActive()
        {
            return hover;
        }

        public void ButtonInfo()
        {
            UIController.getMaternity.maternityInfoPopup.OpenMaternityInfo((MaternityBloodTestRoomInfo)room.GetRoomInfo());
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(room.position + new Vector2i(2, 2), 1f, false);
        }

        public void ButtonSeePatientCards()
        {
            MaternityPatientAI patientThatNeedDiagnosis = null;
            patientThatNeedDiagnosis = FindPatientWaitingForBloodTest(patientThatNeedDiagnosis);
            MaternityWaitingRoomBed bedToMoveCameraTo = MaternityWaitingRoomController.Instance.GetBedForPatient(patientThatNeedDiagnosis);
            if (bedToMoveCameraTo != null)
            {
                UIController.getMaternity.patientCardController.Open(bedToMoveCameraTo, false, false);
                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(bedToMoveCameraTo.room.position + new Vector2i(2, 2), 1f, false);
            }
            else
            {
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(NO_PATIENT_AVAILABLE_KEY));
            }
        }

        private MaternityPatientAI FindPatientWaitingForBloodTest(MaternityPatientAI patientThatNeedDiagnosis)
        {
            foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
            {
                if (patient.Person.State.GetTag() == Maternity.PatientStates.MaternityPatientStateTag.WFSTD)
                {
                    patientThatNeedDiagnosis = patient;
                }
            }

            return patientThatNeedDiagnosis;
        }
    }
}
