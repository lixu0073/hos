using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using IsoEngine;
using SimpleUI;
using TMPro;

namespace Hospital
{
    public class MedicineProductionHover : MastershipHover
    {
        [SerializeField] private Image currentMedicineImage = null;
#pragma warning disable 0414
        [SerializeField] private Image currentMedicineBackground = null;
#pragma warning restore 0414
        [SerializeField] private TextMeshProUGUI currentMedicineEmpty = null;
        [SerializeField] private GameObject medicinesList = null;
        [SerializeField] private TextMeshProUGUI timeCounter = null;
        [SerializeField] private TextMeshProUGUI speedUpCostText = null;
        [SerializeField] private TextMeshProUGUI enlargeQueueCostText = null;
        [SerializeField] private RectTransform MedicineMenuFrame = null;
        [SerializeField] private PointerDownListener producedMedicineImage = null;
        [SerializeField] private GameObject SpeedUpButton = null;
        //[SerializeField]
        //Transform MenuEntrysParent;
        [SerializeField] private GameObject Glow = null;

        [SerializeField] private RectTransform CuresFrame = null;
        [SerializeField] private TextMeshProUGUI machineName = null;
        [SerializeField] GameObject Waiting = null;
#pragma warning disable 0649
        [SerializeField] private AdButtonScript adButtonScript;
#pragma warning restore 0649
        [SerializeField] private TextMeshProUGUI decreaseTimeLeftValue = null;

        private bool initialized = false;

        int queueSize;

        private LinkedList<MedicineRef> medicines;
        private MedicineProductionMachine machine;

        private GameObject prefab;
        [SerializeField] private GameObject PanelElementPrefab = null;
        [SerializeField] private GameObject miniaturePrefab = null;
        [SerializeField] private Sprite pageActive = null;
        [SerializeField] private Sprite pageInactive = null;

        private List<Vector3> spots = new List<Vector3>();

        private int totalPages = 1;
        private int currentPage = 1;
        private int totalItems = 0;
        private static readonly int itemsPerPage = 3;
        [SerializeField] private GameObject leftArrow = null;
        [SerializeField] private GameObject rightArrow = null;
        [SerializeField] private GameObject leftBadge = null;
        [SerializeField] private GameObject rightBadge = null;
        [SerializeField] private GameObject pageIndicators = null;
        private List<Image> indicators;
        private List<MedicineDatabaseEntry> meds;
        MedicineDatabaseEntry firstLockedMedicine;

        [SerializeField] private GameObject gfxForThree = null;
        [SerializeField] private GameObject gfxForTwo = null;

        public static bool tutorialMode = false;

        void Awake()
        {
            Setup();
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        void Setup()
        {
            hover = this;
            gameObject.SetActive(false);
        }

        public static RectTransform GetRect()
        {
            return (hover != null) ? hover.GetComponent<RectTransform>() : null;
        }

        public static void SetTutorialMode(bool isOn)
        {
            tutorialMode = isOn;
        }

        public void Initialize(Vector2 worldPosition, LinkedList<MedicineRef> patients, int queueSize, MedicineProductionMachine machine)
        {
            base.Initialize();

            //trans = GetComponent<RectTransform>();
            tr = gameObject.GetComponent<RectTransform>();
            SetWorldPointHovering(new Vector3(worldPosition.x, 0, worldPosition.y));
            initialized = true;
            InitializeContainer(queueSize);
            this.machine = machine;
            this.medicines = patients;
            var type = machine.productedMedicines;

            //var medicineList = ResourcesHolder.Get().GetMedicinesOfType(type);
            meds = ResourcesHolder.Get().GetMedicinesOfType(type).OrderBy(x => x.minimumLevel).ToList();
            firstLockedMedicine = ResourcesHolder.Get().GetFirstLockedMedicine(type);

            totalItems = meds.Count + (firstLockedMedicine != null ? 1 : 0);
            totalPages = (int)Mathf.Ceil(totalItems / (float)itemsPerPage);

            leftBadge.SetActive(false);
            rightBadge.SetActive(false);

            if (totalPages < 2)
            {
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                pageIndicators.SetActive(false);
            }
            else
            {
                leftArrow.SetActive(true);
                rightArrow.SetActive(true);
                pageIndicators.SetActive(true);
            }
            currentPage = 1;
            for (int i = 0; i < indicators.Count; i++)
                indicators[i].enabled = true;
            for (int i = totalPages; i < indicators.Count; i++)
                indicators[i].enabled = false;
            indicators[0].sprite = pageActive;
            UpdateCurrentPage();
            ActualizeEnlargeQueueCost();
            machineName.SetText(UIController.DivideStringIntoLines(I2.Loc.ScriptLocalization.Get(machine.GetRoomInfo().ShopTitle), 19));
            if (machine.masterableProperties != null)
            {
                SetMastershipButton(machine.masterableProperties);
            }
            SetHoverFrame(queueSize);

            adButtonScript.Initialize(machine.AdvanceProductionTimeAfterWatchingAdvertisement, AdsController.AdType.rewarded_ad_medicine_production);
            if (machine.GetActualMedicine() == null)
            {
                adButtonScript.SetButtonEnable(false);
            }
            decreaseTimeLeftValue.SetText(string.Format("-{0}%", Mathf.RoundToInt(100 * MedicineProductionMachine.PercentBonusToProductionAfterWatchingAd).ToString()));

            return;
        }

        protected override void Init()
        {
            base.Init();
            spots.Add(MedicineMenuFrame.Find("FirstSpot").gameObject.transform.localPosition);
            spots.Add(MedicineMenuFrame.Find("SecondSpot").gameObject.transform.localPosition);
            spots.Add(MedicineMenuFrame.Find("ThirdSpot").gameObject.transform.localPosition);
            indicators = new List<Image>();
            int count = pageIndicators.transform.childCount;
            for (int i = 0; i < count; ++i)
            {
                if (pageIndicators.transform.GetChild(i).TryGetComponent<Image>(out Image p))
                    indicators.Add(p);
            }
        }

        private void UpdateCurrentPage()
        {
            int iterator = 0;
            foreach (Transform child in CuresFrame)
            {
                GameObject.Destroy(child.gameObject);
            }
            for (int i = (currentPage - 1) * itemsPerPage; i < Mathf.Min(totalItems - (firstLockedMedicine != null ? 1 : 0), currentPage * itemsPerPage); ++i)
            {
                var temp = GameObject.Instantiate(PanelElementPrefab);
                temp.GetComponent<ProductionHoverDraggableElement>().Initialize(meds[i].GetMedicineRef(), miniaturePrefab, medicinesList.GetComponent<RectTransform>(), machine);
                temp.transform.SetParent(CuresFrame);
                temp.transform.SetAsLastSibling();
                temp.transform.localPosition = spots[iterator];
                temp.SetActive(true);
                ++iterator;
                if (tutorialMode)
                {
                    Image image = temp.GetComponent<ProductionHoverDraggableElement>().GetMedicineImage();
                    TutorialUIController.Instance.BlinkImage(image);
                }
            }

            if (currentPage == totalPages && firstLockedMedicine != null)
            {
                var temp = GameObject.Instantiate(PanelElementPrefab);
                temp.GetComponent<ProductionHoverDraggableElement>().Initialize(firstLockedMedicine.GetMedicineRef(), null, medicinesList.GetComponent<RectTransform>(), machine);
                temp.transform.SetParent(CuresFrame);
                temp.transform.localScale = Vector3.one;
                temp.transform.SetAsLastSibling();
                //temp.transform.localScale = new Vector3(1, 1, 1);
                temp.transform.localPosition = spots[iterator];
                temp.SetActive(true);
            }

            if (totalItems >= 3)
            {
                gfxForThree.SetActive(true);
                gfxForTwo.SetActive(false);
            }
            else
            {
                gfxForThree.SetActive(false);
                gfxForTwo.SetActive(true);
            }

            UpdateArrowsBadge();
        }

        public void UpdateArrowsBadge()
        {
            List<int> padgesWithBadge = new List<int>();
            // check if any meds on before or next page need meds to heal and show badge o arrow
            bool has_left = false, has_right = false;

            int left_item_border = (currentPage - 1) * itemsPerPage;
            int right_item_border = left_item_border + itemsPerPage;

            for (int i = 0; i < totalItems; i++)
            {
                if (i < left_item_border)
                {
                    if (MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(meds[i].GetMedicineRef()) > 0)
                    {
                        int padgeItem = i / ((int)Mathf.Ceil(totalItems / (float)itemsPerPage));
                        if (!padgesWithBadge.Contains(padgeItem))
                            padgesWithBadge.Add(padgeItem);

                        if (totalPages == 2)
                        {
                            if ((currentPage - 1) == 1)
                                has_right = true;
                            else
                                has_left = true;
                        }
                        else has_left = true;
                    }
                }
                else if (i >= right_item_border && i < meds.Count)
                {
                    if (MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(meds[i].GetMedicineRef()) > 0)
                    {
                        int padgeItem = i / ((int)Mathf.Ceil(totalItems / (float)itemsPerPage));
                        if (!padgesWithBadge.Contains(padgeItem))
                            padgesWithBadge.Add(padgeItem);

                        has_right = true;
                    }
                }
            }

            if (totalPages > 2) // skrajny aktywna strona
            {
                if ((currentPage) == totalPages && padgesWithBadge.Contains(0))
                    has_right = true;

                if ((currentPage) == 1 && padgesWithBadge.Contains(totalPages - 1))
                    has_left = true;
            }

            leftBadge.SetActive(has_left);
            rightBadge.SetActive(has_right);

            padgesWithBadge.Clear();
            padgesWithBadge = null;
        }

        public void OnLeftArrowClick()
        {
            SoundsController.Instance.PlayButtonClick2();
            if (totalPages > 1)
                MovePage(-1);
        }

        private void MovePage(int i)
        {
            indicators[(currentPage - 1) % totalPages].sprite = pageInactive;
            currentPage += i;
            if (currentPage < 1)
                currentPage += totalPages;
            else if (currentPage > totalPages)
                currentPage = 1;
            indicators[(currentPage - 1) % totalPages].sprite = pageActive;
            UpdateCurrentPage();
        }

        public void OnRightArrowClick()
        {
            SoundsController.Instance.PlayButtonClick2();
            if (totalPages > 1)
                MovePage(1);
        }

        private void ShowTooltip(int i)
        {
            if (i < medicines.Count)
                TextTooltip.Open(medicines.ElementAt(i), true);
        }

        public void ActualizeList(MedicineRef actualMedicine)
        {
            //print("actualising");
            //print(actualMedicine);
            //print("medicines count: "+medicines.Count);
            if (actualMedicine != null)
            {
                currentMedicineImage.gameObject.SetActive(true);
                currentMedicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(actualMedicine);
                SpeedUpButton.SetActive(true);
                Glow.SetActive(true);
                timeCounter.enabled = true;
                ActualizeCounterTime();
                currentMedicineEmpty.enabled = false;
                producedMedicineImage.SetDelegate(() =>
                {
                    TextTooltip.Open(actualMedicine, true);
                });
            }
            else
            {
                producedMedicineImage.SetDelegate(null);
                currentMedicineEmpty.enabled = true;
                timeCounter.enabled = false;
                currentMedicineImage.gameObject.SetActive(false);
                SpeedUpButton.SetActive(false);
                Glow.SetActive(false);
            }
            //print("current actualised");
            Image imag;

            if (medicines != null && medicines.Count > 0)
            {
                for (int i = 0; i < medicines.Count; i++)
                {
                    var g = medicinesList.transform.GetChild(i).gameObject;
                    var p = g.transform.GetChild(1).gameObject;

                    ShowGlow(g.transform, true);

                    p.SetActive(true);
                    imag = p.GetComponent<Image>();
                    g.transform.Find("EmptyText").gameObject.SetActive(false);
                    if (imag != null)
                    {
                        imag.enabled = true;
                        imag.sprite = ResourcesHolder.Get().GetSpriteForCure(medicines.ElementAt(i));
                    }
                    else throw new IsoException("image in MedicineProductionHover doesn't exist!");
                }
            }

            if (medicinesList != null && medicinesList.transform.childCount > 0)
            {
                for (int i = medicines.Count; i < medicinesList.transform.childCount - (buyingEnabled ? 1 : 0); i++)
                {
                    var g = medicinesList.transform.GetChild(i).gameObject;
                    var p = g.transform.GetChild(1).gameObject;
                    ShowGlow(g.transform, false);
                    p.SetActive(false);
                    if (g != null && g.transform.Find("EmptyText") != null)
                    {
                        g.transform.Find("EmptyText").gameObject.SetActive(true);
                    }
                }
            }
            SetWaitingInfo();
            adButtonScript.SetButtonEnable(actualMedicine != null && !tutorialMode);
        }

        private void ShowGlow(Transform parent, bool show)
        {
            parent.GetChild(0).gameObject.SetActive(false);
        }

        void SetWaitingInfo()
        {
            Waiting.SetActive(medicines.Count > 0);
        }

        public void ShowAdditionalOnList(MedicineRef medicine, MedicineRef actualMedicine)
        {
            if (actualMedicine == null)
            {
                currentMedicineImage.gameObject.SetActive(true);
                currentMedicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
                currentMedicineEmpty.enabled = false;
                Glow.SetActive(true);
            }
            else
            {
                var p = medicinesList.transform.GetChild(medicines.Count).gameObject.transform;
                var g = p.GetChild(1).gameObject;
                ShowGlow(p, true);
                //p.GetChild(2).gameObject.SetActive(false);      //"Empty" text
                p.Find("EmptyText").gameObject.SetActive(false);
                g.SetActive(true);
                g.GetComponent<Image>().enabled = true;
                g.GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
            }
        }

        public void RemoveAdditionalFromList(MedicineRef actualMedicine)
        {
            if (actualMedicine == null)
            {
                currentMedicineImage.gameObject.SetActive(false);
                currentMedicineEmpty.enabled = true;
                Glow.SetActive(false);
            }
            else
            {
                var p = medicinesList.transform.GetChild(medicines.Count).gameObject.transform;
                var g = p.GetChild(1).gameObject;
                ShowGlow(p, false);
                p.GetChild(0).gameObject.SetActive(false);
                //p.GetChild(2).gameObject.SetActive(true);       //"Empty" text
                p.Find("EmptyText").gameObject.SetActive(true);
                g.GetComponent<Image>().enabled = false;
            }
        }

        private void InitializeContainer(int size)
        {
            prefab = ResourcesHolder.GetHospital().ProductionHoverButton;
            //EnlargeQueue(size - 1);
            SetQueueSize(size - 1);
        }

        public void SetQueueSize(int count, bool setUpFromView = false)
        {
            int dif = count - (medicinesList.transform.childCount - 1);

            var cnt = medicinesList.transform.childCount - 1;
            var p = medicinesList.transform.GetChild(medicinesList.transform.childCount - 1);
            if (dif > 0)
            {
                for (int i = 0; i < dif; i++)
                {
                    var z = GameObject.Instantiate(prefab);
                    z.transform.SetParent(medicinesList.transform);
                    z.transform.localScale = Vector3.one;
                    var g = z.transform.GetChild(0).gameObject;
                    g.GetComponent<Image>().enabled = true;
                    var c = cnt + i;
                    z.GetComponent<PointerDownListener>().SetDelegate(() => { ShowTooltip(c); });
                    g.SetActive(false);
                }
            }
            else if (dif < 0)
            {
                for (int i = 0; i < -dif; i++)
                {
                    medicinesList.transform.GetChild(count + i).gameObject.SetActive(false);
                    Destroy(medicinesList.transform.GetChild(count + i).gameObject);
                }
            }

            p.SetAsLastSibling();
            queueSize = setUpFromView ? medicinesList.transform.childCount - 1 : count;
        }

        public void EnlargeQueue(int count, bool setUpFromView = false)
        {
            int diff = count - (medicinesList.transform.childCount - 1);
            var cnt = medicinesList.transform.childCount - 1;
            var p = medicinesList.transform.GetChild(medicinesList.transform.childCount - 1);
            for (int i = 1; i < diff; i++)
            {
                var z = GameObject.Instantiate(prefab);
                z.transform.SetParent(medicinesList.transform);
                z.transform.localScale = Vector3.one;
                var g = z.transform.GetChild(0).gameObject;
                g.GetComponent<Image>().enabled = true;
                var c = cnt + i;
                z.GetComponent<PointerDownListener>().SetDelegate(() => { ShowTooltip(c); });
                g.SetActive(false);
            }
            p.SetAsLastSibling();
            queueSize = setUpFromView ? medicinesList.transform.childCount - 1 : count;
        }

        public void SpeedUpWithDiamonds()
        {
            machine.SpeedUpWithDiamonds(this);
        }

        public void EnlargeButton()
        {
            int enlargeCost = DiamondCostCalculator.GetQueueSlotCost(queueSize);
            if (Game.Instance.gameState().GetDiamondAmount() >= enlargeCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(enlargeCost, delegate
                {
                    GameState.Get().RemoveDiamonds(enlargeCost, EconomySource.EnlargeQueue, machine.Tag);

                    Vector3 pos = new Vector3(transform.position.x + machine.actualData.rotationPoint.x, 1, transform.position.z + machine.actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, enlargeCost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    machine.EnlargeQueue();
                    hover.ActualizeEnlargeQueueCost();
                }, this);
            }
            else
            {
                // MessageController.instance.ShowMessage(1);
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        bool buyingEnabled = true;
        public void SetBuyingEnable(bool value)
        {
            GameObject enlargeButton = medicinesList.transform.GetChild(medicinesList.transform.childCount - 1).gameObject;
            if (enlargeButton)
            {
                enlargeButton.SetActive(value);
                enlargeButton.transform.GetChild(0).gameObject.SetActive(value);
                enlargeButton.transform.GetChild(1).gameObject.SetActive(value);
            }
            buyingEnabled = value;
        }

        protected override void Update()
        {
            base.Update();
            if (initialized)
            {
                //SetWorldPosition(position);
                if (machine)
                    ActualizeCounterTime();
                else
                    timeCounter.text = "-";
            }
        }

        private void ActualizeCounterTime()
        {
            var p = (int)(machine.ActualMedicineProductionTime);
            timeCounter.text = UIController.GetFormattedTime(p);
            //(p / 60 < 10 ? "0" : "") + (p / 60).ToString() + ":" + (p % 60 < 10 ? "0" : "") + (p % 60).ToString();
        }

        void ActualizeEnlargeQueueCost()
        {
            enlargeQueueCostText.text = DiamondCostCalculator.GetQueueSlotCost(queueSize).ToString();
        }

        public void SetSpeedUpCostText(float timeRemaining, float baseTime)
        {
            //Debug.LogError("SetSpeedUpCostText " + timeRemaining + "  " + baseTime);
            int cost = DiamondCostCalculator.GetCostForAction(timeRemaining, baseTime, machine != null ? machine.Tag : "", tutorialMode);            

            if (cost == 0)
            {
                speedUpCostText.text = I2.Loc.ScriptLocalization.Get("FREE");
                if (tutorialMode)
                    hover.BlinkSpeedUpButton();
            }
            else if (cost < 0)
                Debug.LogError("NEGATIVE SPEED UP COST");
            else
                speedUpCostText.text = cost.ToString();
        }

        private static MedicineProductionHover hover;

        public override void Close()
        {
            Debug.Log("Med Prod Hover close");
            //TutorialUIController.Instance.tutorialArrowUI.Hide();
            base.Close();
            if (machine != null)
            {
                machine.NullHover();
                machine.SetBorderActive(false);

                //TutorialController tc = TutorialController.Instance;
                //if (tc.tutorialEnabled && (tc.GetCurrentStepData().StepTag == StepTag.syrup_production_start || tc.GetCurrentStepData().StepTag == StepTag.syrup_in_production || tc.GetCurrentStepData().StepTag == StepTag.syrup_collect_text))
                //{
                //    TutorialUIController.Instance.ShowIndictator(machine);
                //}
            }

            foreach (Transform child in CuresFrame)
            {
                GameObject.Destroy(child.gameObject);
            }
            indicators[currentPage - 1].sprite = pageInactive;

            /*for (int i = 0; i < medicinesList.transform.childCount - 1; i++)
                GameObject.Destroy(medicinesList.transform.GetChild(i).gameObject);
                */
            initialized = false;
            GameState.isHoverOn = false;
            //TutorialUIController.Instance.ShowIndicatorCanvas();
            HospitalAreasMapController.HospitalMap.ResetOntouchAction();
        }

        public static MedicineProductionHover Open()
        {
            if (hover == null)
            {
                var p = GameObject.Instantiate(UIController.get.productionHoverPrefab);
                hover = p.GetComponent<MedicineProductionHover>();
                hover.Init();
            }
            //hover.Close();
            hover.Initialize();
            //TutorialUIController.Instance.HideIndicatorCanvas();

            //TutorialController tc = TutorialController.Instance;
            if (tutorialMode)
                hover.BlinkSpeedUpButton();
            GameState.isHoverOn = true;
            return hover;
        }

        public void BlinkSpeedUpButton()
        {
            Image image = hover.SpeedUpButton.GetComponent<Image>();
            TutorialUIController.Instance.BlinkImage(image);
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
            hoverFrame.sizeDelta = new Vector2(900, queueSize > 4 ? 880 : 790);
            hoverFrame.pivot = new Vector2(0.5f, 0.5f);
        }

        public static MedicineProductionHover GetActive()
        {
            return hover;
        }

        public void ButtonInfo()
        {
            StartCoroutine(UIController.getHospital.HospitalInfoPopUp.Open(machine.GetRoomInfo(), InfoType.Machine, 0));
        }

        #region Mastership
        public override void ButtonMastership()
        {
            StartCoroutine(UIController.getHospital.HospitalInfoPopUp.Open(machine.GetRoomInfo(), InfoType.Machine, 1));
        }

        public override void UpdateStars()
        {
            SetMastershipButton(machine.masterableProperties);
        }
        #endregion
    }
}