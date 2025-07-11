using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Hospital
{
    public class plantationPatchHover : BaseHover
    {
        [SerializeField] private RectTransform PlantHover = null;
        [SerializeField] private TextMeshProUGUI PlantNameText = null;

        [SerializeField] private RectTransform PlantMenuFrame = null;
        [SerializeField] private GameObject waitingMenu = null;
        [SerializeField] Slider progressBar = null;
        [SerializeField] TextMeshProUGUI progressText = null;
        [SerializeField] private GameObject plantationPatchToolPrefab = null;
        [SerializeField] private Transform itemsParent = null;
        [SerializeField] private GameObject selectItemPrefab = null;
        [SerializeField] private Image leftArrow = null;
        [SerializeField] private Image rightArrow = null;
        [SerializeField] private GameObject pageIndicators = null;

        [SerializeField] private GameObject leftBadge = null;
        [SerializeField] private GameObject rightBadge = null;

        private List<Image> indicators;

        [SerializeField] private GameObject singleToolPosition = null;

        [SerializeField] private GameObject firstToolPosition = null;
        [SerializeField] private GameObject secondToolPosition = null;
        //[SerializeField] private ScrollRect scrollRect;
        private PlantationPatch patch;
        [SerializeField] private TextMeshProUGUI speedUpCostText = null;

        private Vector3 firstSpotPosition = Vector3.zero;
        private Vector3 secondSpotPosition = Vector3.zero;
        private Vector3 thirdSpotPosition = Vector3.zero;

        private List<Vector3> spots = new List<Vector3>();

        private int totalPages = 1;
        private int currentPage = 1;
        private int totalItems = 0;
        private static int itemsPerPage = 3;

        [SerializeField] private Sprite pageActive = null;
        [SerializeField] private Sprite pageInactive = null;
        private List<MedicineDatabaseEntry> medicines;
        private MedicineDatabaseEntry firstLockedMedicine;
#pragma warning disable 0649
        [SerializeField] private GameObject gfxForThree;
        [SerializeField] private GameObject gfxForTwo;
        [SerializeField] private GameObject gfxForOneRenew;
        [SerializeField] private GameObject gfxForTwoRenew;
#pragma warning restore 0649

        public delegate void OnClose();

        void Awake()
        {
            Setup();
        }

        void Setup()
        {
            hover = this;
            gameObject.SetActive(false);
        }

        public void Initialize(PlantationPatch patch, EPatchState state)
        {
            base.Initialize();
            SetWorldPointHovering(new Vector3(patch.transform.position.x + 0.5f, 0, patch.transform.position.z + 0.5f));
            this.patch = patch;
            switch (state)
            {
                /*case EPatchState.disabled:
					break;
				case EPatchState.waitingForRenew:
					break;*/
                case EPatchState.renewing:
                    InitializeWaitingPatch();
                    break;
                case EPatchState.empty:
                    InitializeSelectPatch();
                    break;
                case EPatchState.producing:
                    InitializeWaitingPatch();
                    break;
                case EPatchState.waitingForUser:
                    InitializeCollectingPatch();
                    break;
                case EPatchState.fallow:
                    InitializeRenewPatch();
                    break;
                case EPatchState.waitingForHelp:
                    InitializeWaitForHelpPatch();
                    break;
                default:
                    break;
            }
        }

        public void SetProductionBar(int curr, int max)
        {
            progressBar.value = curr / (float)max;
            progressText.text = UIController.GetFormattedTime(max - curr);
            //	progressBar.SetValue(curr);
        }

        public void SetSpeedUpCostText(float timeRemaining, float baseTime)
        {
            int cost = DiamondCostCalculator.GetCostForAction(timeRemaining, baseTime);
            speedUpCostText.text = cost.ToString();
        }

        public void ChangedFromProductionToWaiting()
        {
            InitializeCollectingPatch();
        }

        public void SpeedUpWithDiamonds()
        {
            patch.SpeedUpWithDiamonds(this, delegate
             {
                 SelectNextToSpeedUp();
             });
        }

        void SelectNextToSpeedUp()
        {
            PlantationPatch nextWorkingPlant = GetNextWorkingPlantationPatch();
            if (nextWorkingPlant != null)            
                nextWorkingPlant.SelectForSpeedUp();
            else            
                hover.Close();
        }

        PlantationPatch GetNextWorkingPlantationPatch()
        {
            //tutaj elegancko wyszukać pracującego pola (here smartly search for a working field)
                        //ProbeTable[] allTables = FindObjectsOfType<ProbeTable>();
                        Plantation myPlantation = ReferenceHolder.GetHospital().plantation;
            for (int j = 0; j < myPlantation.actualPlantationSize.y; j++)
                for (int i = 0; i < myPlantation.plantationMaxSize.x; i++)
                {
                    if (myPlantation.patches[i, j].PatchState == patch.PatchState)
                        if (myPlantation.patches[i, j].TimeFromSeed < myPlantation.patches[i, j].productionTime)
                        {
                            return myPlantation.patches[i, j];
                        }
                }
            return null;
        }

        private void InitializeWaitingPatch()
        {
            PlantMenuFrame.gameObject.SetActive(false);
            PlantHover.gameObject.SetActive(false);
            waitingMenu.SetActive(true);
            if (patch.GrowingPlant != null)
            {
                var rh = ResourcesHolder.Get();
                PlantNameText.text = rh.GetNameForCure(patch.GrowingPlant);
            }
            else            
                PlantNameText.text = "";
        }

        private void InitializeCollectingPatch()
        {
            waitingMenu.SetActive(false);
            PlantHover.gameObject.SetActive(false);
            PlantMenuFrame.gameObject.SetActive(true);
            PlantMenuFrame.sizeDelta = new Vector2(140, PlantMenuFrame.sizeDelta.y);

            gfxForOneRenew.SetActive(true);
            gfxForTwoRenew.SetActive(false);

            InitializeTool(singleToolPosition.transform.position, PlantationPatchToolType.collect);
        }

        private void InitializeRenewPatch()
        {
            waitingMenu.SetActive(false);
            PlantHover.gameObject.SetActive(false);
            PlantMenuFrame.gameObject.SetActive(true);
            PlantMenuFrame.sizeDelta = new Vector2(140, PlantMenuFrame.sizeDelta.y);

            //InitializeTool(firstToolPosition.transform.position, PlantationPatchToolType.renew);
            print("left: " + patch.RegrowthLeft);
            if (patch.RegrowthLeft > -1)
            {
                InitializeTool(firstToolPosition.transform.position, PlantationPatchToolType.renew);
                InitializeTool(secondToolPosition.transform.position, PlantationPatchToolType.help);
                gfxForOneRenew.SetActive(false);
                gfxForTwoRenew.SetActive(true);
            }
            else
            {
                InitializeTool(singleToolPosition.transform.position, PlantationPatchToolType.renew);
                gfxForOneRenew.SetActive(true);
                gfxForTwoRenew.SetActive(false);
            }
        }

        private void InitializeWaitForHelpPatch()
        {
            waitingMenu.SetActive(false);
            PlantHover.gameObject.SetActive(false);
            PlantMenuFrame.gameObject.SetActive(true);
            PlantMenuFrame.sizeDelta = new Vector2(140, PlantMenuFrame.sizeDelta.y);

            InitializeTool(singleToolPosition.transform.position, PlantationPatchToolType.renew);
            gfxForOneRenew.SetActive(true);
            gfxForTwoRenew.SetActive(false);
        }

        void InitializeTool(Vector3 toolPosition, PlantationPatchToolType toolType)
        {
            var tool = GameObject.Instantiate(selectItemPrefab);
            tool.GetComponent<plantationPatchHoverElement>().Initialize(patch, null, plantationPatchToolPrefab, toolType);
            tool.transform.position = toolPosition;
            tool.transform.SetParent(itemsParent);
            tool.transform.localScale = Vector3.one;
            tool.transform.SetAsLastSibling();
            tool.SetActive(true);
        }

        //private void InitializeGUI()
        //{
        ////	leftArrow = ElixirHover.Find ("LeftArrow").gameObject;
        ////	rightArrow = ElixirHover.Find ("RightArrow").gameObject;
        //}

        private void UpdateCurrentPage()
        {
            int iterator = 0;
            foreach (GameObject child in GameObject.FindGameObjectsWithTag("elidrag"))
            {
                GameObject.Destroy(child);
            }
            for (int i = (currentPage - 1) * itemsPerPage; i < Mathf.Min((totalItems - (firstLockedMedicine != null ? 1 : 0)), currentPage * itemsPerPage); ++i)
            {
                var temp = GameObject.Instantiate(selectItemPrefab);
                temp.GetComponent<plantationPatchHoverElement>().Initialize(patch, medicines[i].GetMedicineRef(), plantationPatchToolPrefab, PlantationPatchToolType.seed);
                temp.transform.SetParent(PlantHover);
                temp.transform.localScale = Vector3.one;
                temp.transform.localPosition = spots[iterator];
                temp.transform.SetAsLastSibling();
                temp.SetActive(true);
                ++iterator;

                /*if (TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.ProductionStarted && medicines[i].Name == "Blue Elixir")
				{
					TutorialUIController.Instance.StartFading(temp.GetComponent<ProbeTableHoverElement>().GetMedicineImage());
				}*/
            }
            if (currentPage == totalPages && firstLockedMedicine != null)
            {
                var temp = GameObject.Instantiate(selectItemPrefab);
                temp.GetComponent<plantationPatchHoverElement>().Initialize(patch, firstLockedMedicine.GetMedicineRef(), null, PlantationPatchToolType.seed);
                temp.transform.SetParent(PlantHover);
                temp.transform.localScale = Vector3.one;
                temp.transform.localPosition = spots[iterator];
                temp.transform.SetAsLastSibling();
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
                    if (MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(medicines[i].GetMedicineRef()) > 0)
                    {
                        int padgeItem = i / ((int)Mathf.Ceil(totalItems / (float)itemsPerPage));
                        if (!padgesWithBadge.Contains(padgeItem))
                            padgesWithBadge.Add(padgeItem);

                        if (totalPages == 2)
                        {
                            if ((currentPage - 1) == 1)
                                has_right = true;
                            else has_left = true;
                        }
                        else has_left = true;
                    }
                }
                else if (i >= right_item_border && i < medicines.Count)
                {
                    if (MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(medicines[i].GetMedicineRef()) > 0)
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
                movePage(-1);
        }

        private void movePage(int i)
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
                movePage(1);
        }

        private void InitializeSelectPatch()
        {
            waitingMenu.SetActive(false);
            PlantMenuFrame.gameObject.SetActive(false);
            PlantHover.gameObject.SetActive(true);
            medicines = ResourcesHolder.Get().GetMedicinesOfType(MedicineType.BasePlant).OrderBy(x => x.minimumLevel).ToList();
            firstLockedMedicine = ResourcesHolder.Get().GetFirstLockedMedicine(MedicineType.BasePlant);
            totalItems = medicines.Count + (firstLockedMedicine != null ? 1 : 0);
            totalPages = (int)Mathf.Ceil(totalItems / (float)itemsPerPage);
            if (totalPages < 2)
            {
                pageIndicators.SetActive(false);
                leftArrow.gameObject.SetActive(false);
                rightArrow.gameObject.SetActive(false);
            }
            else
            {
                pageIndicators.SetActive(true);
                leftArrow.gameObject.SetActive(true);
                rightArrow.gameObject.SetActive(true);
            }
            for (int i = 0; i < indicators.Count; i++)
                indicators[i].enabled = true;
            for (int i = totalPages; i < indicators.Count; i++)
                indicators[i].enabled = false;
            indicators[(currentPage - 1) % totalPages].sprite = pageActive;
            UpdateCurrentPage();

            //ElixirMenuFrame.gameObject.SetActive(true);
            ////waitingMenu.SetActive(false);
            //var medicineList = ResourcesHolder.Get().GetMedicinesOfType(MedicineType.BaseElixir);
            //ElixirMenuFrame.sizeDelta = new Vector2(40 + 100 * (medicineList.Count > 3 ? 4 : (medicineList.Count>0? medicineList.Count : 1)), ElixirMenuFrame.sizeDelta.y);

            //         foreach (var p in medicineList)
            //{
            //	var temp = GameObject.Instantiate(selectItemPrefab);
            //             temp.GetComponent<ProbeTableHoverElement>().Initialize(table, p.GetMedicineRef(), probeTableToolPrefab);
            //	temp.transform.SetParent(itemsParent);
            //	temp.transform.localScale = Vector3.one;
            //	temp.transform.SetAsLastSibling();
            //	temp.SetActive(true);
            //}
            //scrollRect.horizontalNormalizedPosition = 0;
            //scrollRect.enabled = true;
        }

        private static plantationPatchHover hover;

        public override void Close()
        {
            base.Close();
            GameState.isHoverOn = false;

            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.garden_text)
                TutorialUIController.Instance.ShowIndictator(new Vector3(69.2f, 0f, 45f));

            indicators[currentPage - 1].sprite = pageInactive;
            for (int i = 0; i < itemsParent.childCount; i++)
                GameObject.Destroy(itemsParent.GetChild(i).gameObject);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected override void Init()
        {
            base.Init();
            spots.Add(PlantHover.Find("FirstSpot").gameObject.transform.localPosition);
            spots.Add(PlantHover.Find("SecondSpot").gameObject.transform.localPosition);
            spots.Add(PlantHover.Find("ThirdSpot").gameObject.transform.localPosition);

            indicators = new List<Image>();
            int count = pageIndicators.transform.childCount;
            Image p;
            for (int i = 0; i < count; i++)
            {
                p = pageIndicators.transform.GetChild(i).GetComponent<Image>();
                if (p != null)
                    indicators.Add(p);
            }
            //print("indicators: " + indicators.Count);
        }

        public static plantationPatchHover Open(PlantationPatch plantationPatch)
        {
            if (hover == null)
            {
                var p = GameObject.Instantiate(UIController.getHospital.plantationPatchHoverPrefab);
                hover = p.GetComponent<plantationPatchHover>();
                hover.Init();
                hover.patch = plantationPatch;
            }
            hover.Close();
            hover.Initialize();
            hover.patch = plantationPatch;
            hover.UpdateAccordingToMode();
            GameState.isHoverOn = true;

            return hover;
        }

        public static plantationPatchHover GetActive()
        {
            return hover != null && hover.gameObject.activeSelf ? hover : null;
        }
    }
}
