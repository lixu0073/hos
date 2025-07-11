using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Hospital;
using IsoEngine;
using TMPro;
using MovementEffects;
using System;

namespace SimpleUI
{
    public class DoctorHover : MastershipHover
    {
        [SerializeField] Image cureImage = null;
        [SerializeField] Image currentPatientHeadImage = null;
        [SerializeField] Image currentPatientTorsoImage = null;
        [SerializeField] Image currentPatientChildIndicator = null;
        [SerializeField] GameObject patientContainer = null;
        [SerializeField] PointerDownListener currentPatientListener = null;
        [SerializeField] Image elixirArrow = null;
        [SerializeField] TextMeshProUGUI timeCounter = null;
        [SerializeField] GameObject timeCounterBackground = null;
        [SerializeField] GameObject SpeedUpButton = null;
        [SerializeField] GameObject Glow = null;

        [SerializeField] GameObject DraggablePrefab = null;
        [SerializeField] TextMeshProUGUI medicineAmount = null;
        [SerializeField] TextMeshProUGUI enlargeQueueCostText = null;
        [SerializeField] TextMeshProUGUI speedUpCostText = null;
        [SerializeField] TextMeshProUGUI roomNameText = null;
        [SerializeField] GameObject Waiting = null;

        bool initialized = false;

        public int queueSizeUnlocked;
        LinkedList<BasePatientAI> patients;
        DoctorRoom room;
        private MedicineRef cure;
        private GameObject prefab;
        [SerializeField]
        Material inactiveMaterial = null;

        //vars moved out from ActualizePatientList;
        int cureAmount = 0;
        BasePatientAI currentPatient = null;
        Image avatarHeadImage = null;
        Image avatarBodyImage = null;
        Transform avatarTransform = null;
        BasePatientAI patientAI = null;

        public bool tutorialMode = false;

        public static DoctorHover hover;

        void Awake()
        {
            Setup();
        }

        void Setup()
        {
            //Debug.Log("Setup");
            hover = this;
            gameObject.SetActive(true);
        }

        public void ToggleTutorialMode(bool isOn)
        {
            tutorialMode = isOn;
            if (isOn && hover.gameObject.activeInHierarchy)
                SetSpeedUpCostText(room.CureTimeMastered - room.curationTime, room.CureTimeMastered);
        }

        public void Initialize(MedicineRef cureType, Vector2 worldPosition, LinkedList<BasePatientAI> patients, int queueSizeUnlocked, DoctorRoom room)
        {
            base.Initialize();
            timeCounterBackground.SetActive(false);

            //else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.doctor_speed_up)
            //TutorialUIController.Instance.ShowTutorialArrowUI(SpeedUpButton.GetComponent<RectTransform>(), Vector2.zero, 0, TutorialUIController.TutorialPointerAnimationType.tap);

            SetSpeedUpCostText(room.CureTimeMastered - room.curationTime, room.CureTimeMastered);

            SetWorldPointHovering(new Vector3(worldPosition.x, 0, worldPosition.y));
            initialized = true;
            SetCure(cureType);
            //patientsContainer.Initialize(patients, room.QueueSize-1);
            queueSizeUnlocked = room.QueueSize;
            InitializeContainer(room.QueueSize);
            this.room = room;
            this.patients = patients;

            roomNameText.SetText(UIController.DivideStringIntoLines(I2.Loc.ScriptLocalization.Get(room.GetRoomInfo().ShopTitle), 19));
            ActualizeMedicineAmount();
            ActualizeEnlargeQueueCost();
            SetHoverFrame(queueSizeUnlocked);
            SetMastershipButton(room.masterableProperties);
        }

        private void ActualizeMedicineAmount()
        {
            var i = GameState.Get().GetCureCount(cure);
            medicineAmount.text = i.ToString();
            SetSpeedUpCostText(room.CureTimeMastered - room.curationTime, room.CureTimeMastered);
        }

        void ActualizeEnlargeQueueCost()
        {
            enlargeQueueCostText.text = DiamondCostCalculator.GetQueueSlotCost(queueSizeUnlocked - 1).ToString();
        }

        public void SetSpeedUpCostText(float timeRemaining, float baseTime)
        {
            int cost = (room != null) ? DiamondCostCalculator.GetCostForAction(timeRemaining, baseTime, room.Tag, tutorialMode) :             
                                        DiamondCostCalculator.GetCostForAction(timeRemaining, baseTime, "", tutorialMode);

            if (cost == 0)
            {
                speedUpCostText.text = I2.Loc.ScriptLocalization.Get("FREE");
                TutorialUIController.Instance.BlinkImage(SpeedUpButton.GetComponent<Image>());
            }
            else if (cost < 0)
                Debug.LogError("NEGATIVE SPEED UP COST");
            else
                speedUpCostText.text = cost.ToString();
        }

        private void InitializeContainer(int size)
        {
            prefab = ResourcesHolder.GetHospital().DoctorHoverButton;
            EnlargeQueue(size);
        }

        private void SetCure(MedicineRef cure)
        {
            this.cure = cure;
            cureImage.sprite = ResourcesHolder.Get().GetSpriteForCure(cure);
        }

        GameObject draggable = null;
        public void SpawnTool()
        {
            timeCounterBackground.SetActive(false);
            cureImage.enabled = false;
            elixirArrow.enabled = false;
            draggable = Instantiate(DraggablePrefab);
            draggable.transform.SetParent(transform);
            draggable.transform.SetAsLastSibling();
            draggable.transform.localScale = Vector3.one * 1.5f;
            draggable.GetComponent<DraggableElixirImage>().Initialize(room, patientContainer.GetComponent<RectTransform>(), cure, () =>
            {
                ShowCureAndArrow();
            });
        }

        void ShowCureAndArrow()
        {
            cureImage.enabled = true;
            elixirArrow.enabled = true;
        }

        public void EnlargeQueue(int count)
        {
            var p = patientContainer.transform.GetChild(patientContainer.transform.childCount - 1);
            for (int i = 0; i < count; i++)
            {
                var z = GameObject.Instantiate(prefab);
                z.transform.SetParent(patientContainer.transform);
                z.transform.localScale = Vector3.one;
                z.transform.GetChild(3).gameObject.SetActive(false);
                z.transform.GetChild(4).GetComponent<Image>().enabled = false;      //kid indicator
            }
            p.SetAsLastSibling();
            this.queueSizeUnlocked = count;
        }

        public void HealNow()
        {
            if (SpeedUpDiamondTransactionMakerID != ID)
            {
                InitializeID();
                SpeedUpDiamondTransactionMakerID = ID;
            }
            room.HealNow(this);
        }

        public void EnlargeButton()
        {
            if (EnlargeDiamondTransactionMakerID != ID)
            {
                InitializeID();
                EnlargeDiamondTransactionMakerID = ID;
            }
            int enlargeCost = DiamondCostCalculator.GetQueueSlotCost(queueSizeUnlocked - 1);
            if (Game.Instance.gameState().GetDiamondAmount() >= enlargeCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(enlargeCost, delegate
                {
                    GameState.Get().RemoveDiamonds(enlargeCost, EconomySource.EnlargeQueue, room.Tag);

                    Vector3 pos = new Vector3(transform.position.x + room.actualData.rotationPoint.x, 1, transform.position.z + room.actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, enlargeCost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    room.EnlargeQueue();
                    SetNextPatientsAvatar();
                }, this);
            }
            else
            {
                //MessageController.instance.ShowMessage(1);
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
            hover.ActualizeEnlargeQueueCost();
        }

        public void SetBuyingEnable(bool value)
        {
            patientContainer.transform.GetChild(patientContainer.transform.childCount - 1).gameObject.SetActive(value);
        }

        public void ActualizePatientList(BasePatientAI currentPatient, int cureAmount)
        {
            if (!initialized)
            {
                Debug.Log("Hover Not initialized. returning");    //this might be related to saving/loading when hover is open
                return;
            }
            this.currentPatient = currentPatient;
            this.cureAmount = cureAmount;

            SetWaitingInfo();
            ActualizeMedicineAmount();
            if (IsQueueEmpty())
                return;
            SetCurrentPatientAvatar();
            SetNextPatientsAvatar();
            SetWaitingInfo();
            SetSpeedUpCostText(room.CureTimeMastered - room.curationTime, room.CureTimeMastered);
        }

        bool IsQueueEmpty()
        {
            if (currentPatient == null && patients.Count < 1)
            {
                currentPatientHeadImage.gameObject.SetActive(false);
                currentPatientTorsoImage.gameObject.SetActive(false);
                Glow.SetActive(false);
                SpeedUpButton.SetActive(false);
                //Debug.LogError("Does that ever happen?? Or is this method redundant?");   //it does when doctor is build with tutorial disabled
                return true;
            }
            return false;
        }

        void SetCurrentPatientAvatar()
        {
            if (currentPatient != null)
            {
                currentPatientHeadImage.gameObject.SetActive(true);
                currentPatientHeadImage.sprite = currentPatient.sprites.AvatarHead;
                currentPatientTorsoImage.gameObject.SetActive(true);
                currentPatientTorsoImage.sprite = currentPatient.sprites.AvatarBody;
                currentPatientHeadImage.material = null;
                currentPatientTorsoImage.material = null;
                timeCounter.gameObject.SetActive(true);
                SpeedUpButton.SetActive(true);
                Glow.SetActive(true);

                currentPatientChildIndicator.enabled = currentPatient.GetType() == typeof(ChildPatientAI);                

                currentPatientListener.SetDelegate(() =>
                {
                    PatientTooltip.Open(room, currentPatient.sprites);
                });
            }
            else
            {
                timeCounter.gameObject.SetActive(false);
                SpeedUpButton.SetActive(false);
                currentPatientHeadImage.gameObject.SetActive(true);
                currentPatientHeadImage.sprite = patients.First.Value.sprites.AvatarHead;
                currentPatientTorsoImage.gameObject.SetActive(true);
                currentPatientTorsoImage.sprite = patients.First.Value.sprites.AvatarBody;
                currentPatientHeadImage.material = inactiveMaterial;
                currentPatientTorsoImage.material = inactiveMaterial;
                currentPatientListener.SetDelegate(null);
                Glow.SetActive(false);
                currentPatientChildIndicator.enabled = patients.First.Value.GetType() == typeof(ChildPatientAI);
            }
        }

        public void SetNextPatientsAvatar()
        {
            for (int j = queueSizeUnlocked - 1; j < patientContainer.transform.childCount - 1; ++j)
            {
                Destroy(patientContainer.transform.GetChild(j).gameObject);
            }

            for (int i = 0; i < queueSizeUnlocked - 1; i++)
            {
                avatarTransform = patientContainer.transform.GetChild(i).gameObject.transform;
                avatarHeadImage = avatarTransform.GetChild(2).GetComponent<Image>();
                avatarHeadImage.enabled = true;
                patientAI = patients.ElementAt(i + (currentPatient == null ? 1 : 0));
                avatarHeadImage.sprite = patientAI.sprites.AvatarHead;
                avatarHeadImage.material = ((i < cureAmount - 1) ? null : inactiveMaterial);
                avatarBodyImage = avatarTransform.GetChild(1).GetComponent<Image>();
                avatarBodyImage.enabled = true;
                avatarBodyImage.sprite = patientAI.sprites.AvatarBody;
                avatarBodyImage.material = ((i < cureAmount - 1) ? null : inactiveMaterial);
                GameObject glow = avatarTransform.GetChild(0).gameObject;
                glow.SetActive(false);// i < cureAmount - 1);
                avatarTransform.GetChild(4).GetComponent<Image>().enabled = patientAI.GetType() == typeof(ChildPatientAI); // Kid indicator
            }
        }

        public void ShowTempElixirAdded()
        {
            if (queueSizeUnlocked <= cureAmount)
                return;

            if (currentPatient == null)     //doctor isnt healing anyone, modify first patient
            {
                currentPatientHeadImage.material = null;
                currentPatientTorsoImage.material = null;
                Glow.SetActive(true);
            }
            else    //modify next patient's avatars
            {
                avatarTransform = patientContainer.transform.GetChild(Mathf.Max(0, cureAmount - 1)).gameObject.transform;
                avatarHeadImage = avatarTransform.GetChild(2).GetComponent<Image>();
                avatarHeadImage.material = null;
                avatarBodyImage = avatarTransform.GetChild(1).GetComponent<Image>();
                avatarBodyImage.material = null;
                avatarTransform.GetChild(0).gameObject.SetActive(false);
            }
        }

        void SetWaitingInfo()
        {            
            Waiting.SetActive(cureAmount > 1);
        }

        public void HideTempElixirAdded()
        {
            if (queueSizeUnlocked <= cureAmount)
                return;

            if (currentPatient == null)     //doctor isnt healing anyone, modify first patient
            {
                currentPatientHeadImage.material = inactiveMaterial;
                currentPatientTorsoImage.material = inactiveMaterial;
                Glow.SetActive(false);
            }
            else    //modify next patient's avatars
            {
                avatarTransform = patientContainer.transform.GetChild(Mathf.Max(0, cureAmount - 1)).gameObject.transform;
                avatarHeadImage = avatarTransform.GetChild(2).GetComponent<Image>();
                avatarHeadImage.material = inactiveMaterial;
                avatarBodyImage = avatarTransform.GetChild(1).GetComponent<Image>();
                avatarBodyImage.material = inactiveMaterial;
                avatarTransform.GetChild(0).gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            //Debug.Log("hover = " + hover);
            base.Update();
            if (initialized)
            {
                if ((patients.Count > 0) && (room.isHealing))// || room.isHealingWhileEntering))
                    ActualizeCounterTime();
                else
                    ResetCounter();

                //timeCounter.text = "-";

                if (base.gameObject.activeSelf == true)
                {
                    TutorialUIController.Instance.HideIndicatorCanvas();
                }
            }

            //if (checkInput)
            //{
            //	if (!Input.GetMouseButton(0))
            //	{
            //		checkInput = false;
            //	}
            //	else
            //	{
            //		var pos = Input.mousePosition;
            //		if ((Input.mousePosition - firstPos).magnitude > 30)
            //		{
            //			SpawnTool();
            //			checkInput = false;
            //		}
            //	}
            //}
        }

        private void ResetCounter()
        {
            timeCounter.text = "-";
        }

        private void ActualizeCounterTime()
        {
            var p = (int)(room.CureTimeMastered - room.curationTime);
            timeCounter.text = UIController.GetFormattedTime(p);
            //timeCounter.text = (p / 60 < 10 ? "0" : "") + (p / 60).ToString() + ":" + (p % 60 < 10 ? "0" : "") + (p % 60).ToString();
        }

        public override void Close()
        {
            base.Close();
            GameState.isHoverOn = false;
            if (room != null && room.GetBorder() != null)
                room.SetBorderActive(false);

            if (room != null)
            {
                room.ClearHoverVariable();

                TutorialController tc = TutorialController.Instance;
                if (tc.tutorialEnabled && tc.GetCurrentStepData().NecessaryCondition == Condition.SetTutorialArrow && room.Tag == "BlueDoc" && tc.GetCurrentStepData().CameraTargetRotatableObjectTag == "BlueDoc")
                {
                    TutorialUIController.Instance.ShowIndictator(room);
                }
            }

            if (draggable != null)
            {
                Destroy(draggable);
                draggable = null;
                ShowCureAndArrow();
            }

            initialized = false;
            TutorialUIController.Instance.ShowIndicatorCanvas();
            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.doctor_speed_up ||
                TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_deliver ||
                TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_deliver_again)
            {
                TutorialUIController.Instance.StopBlinking();
                //TutorialUIController.Instance.tutorialArrowUI.Hide();
            }
        }

        public static DoctorHover Open()
        {
            if (hover == null)
            {
                var p = Instantiate(UIController.getHospital.doctorHoverPrefab);
                hover = p.GetComponent<DoctorHover>();
                hover.Init();
                //Debug.LogError("Instantiating new hover!");
            }
            hover.Initialize();
                        
            GameState.isHoverOn = true;

            return hover;
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
            hoverFrame.pivot = new Vector2(0.5f, 1f);
            hoverFrame.sizeDelta = new Vector2(900, (queueSize > 4) ? 830 : 740);
            hoverFrame.pivot = new Vector2(0.5f, 0.5f);           
        }

        public static DoctorHover GetActive()
        {
            return hover;
        }

        public GameObject GetSpeedUpButton()
        {
            return SpeedUpButton;
        }

        public Vector3 GetAvatarPosition(bool isCurrent, int id)
        {
            if (isCurrent)
                return currentPatientListener.transform.position;
            else
            {
                if (patientContainer.transform.childCount < id - 1)
                {
                    Debug.LogError("BEEP BEEP BEEP! RED ALERT! HERE WOULD BE A NULL! = " + patientContainer.transform.childCount);
                    return Vector3.zero;
                }
                else
                    return patientContainer.transform.GetChild(id - 1).gameObject.transform.position;
            }
        }

        public void BumpAvatar(bool isCurrent, int id)
        {
            if (isCurrent)
            {
                try
                { 
                    currentPatientListener.GetComponent<Animator>().Play("Bump", 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else
            {
                try
                {
                    patientContainer.transform.GetChild(id - 1).GetComponent<Animator>().Play("Bump", 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
        }

        public void ButtonInfo()
        {
            StartCoroutine(UIController.getHospital.HospitalInfoPopUp.Open(room.GetRoomInfo(), InfoType.Doctor, 0));
        }

        #region Mastership
        public override void ButtonMastership()
        {
            StartCoroutine(UIController.getHospital.HospitalInfoPopUp.Open(room.GetRoomInfo(), InfoType.Doctor, 1));
        }

        public override void UpdateStars()
        {
            SetMastershipButton(room.masterableProperties);
        }
        #endregion
    }
}
