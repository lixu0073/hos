using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Hospital
{
    public class ProbeTableHover : BaseHover
    {
        private static ProbeTableHover hover;

        [SerializeField] private RectTransform ElixirHover = null;
        [SerializeField] private TextMeshProUGUI ElixirNameText = null;

        [SerializeField] private RectTransform ElixirMenuFrame = null;
        [SerializeField] private GameObject waitingMenu = null;
        [SerializeField] Slider progressBar = null;
        [SerializeField] TextMeshProUGUI progressText = null;
        [SerializeField] private GameObject probeTableToolPrefab = null;
        [SerializeField] private Transform itemsParent = null;
        [SerializeField] private GameObject selectItemPrefab = null;
        [SerializeField] private Image leftArrow = null;
        [SerializeField] private Image rightArrow = null;
        [SerializeField] private GameObject leftBadge = null;
        [SerializeField] private GameObject rightBadge = null;
        [SerializeField] private GameObject pageIndicators = null;
        private List<Image> indicators;
        [SerializeField] private GameObject collectingToolPosition = null;
        //[SerializeField] private ScrollRect scrollRect;
        private ProbeTable table;
        [SerializeField] private TextMeshProUGUI speedUpCostText = null;

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
#pragma warning restore 0649
        [HideInInspector]
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

        public void Initialize(ProbeTable table, TableState state)
        {
            base.Initialize();
            SetWorldPointHovering(new Vector3(table.position.x + 0.5f, 0, table.position.y + 0.5f));
            this.table = table;
            switch (state)
            {
                case TableState.empty:
                    InitializeSelectTable();
                    break;
                case TableState.producing:
                    InitializeWaitingTable();
                    break;
                case TableState.waitingForUser:
                    InitializeCollectingTable();
                    break;
                default:
                    break;
            }

            //Activate arrow blinking on level 6, to help player find white elixir. Bool marker will be set to false just after first click on arrow.
            if (Game.Instance.gameState().GetHospitalLevel() >= 6 && TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir)
            {
                TutorialUIController.Instance.BlinkImage(leftArrow.gameObject.GetComponent<Image>());
                TutorialUIController.Instance.BlinkImage(rightArrow.gameObject.GetComponent<Image>());
            }

            // TutorialUIController.Instance.StopBlinking();
        }

        int lastCurr = -1;
        public void SetProductionBar(int curr, int max)
        {
            //if (lastCurr != curr)

            progressBar.value = curr / (float)max;
            progressText.text = UIController.GetFormattedTime(max - curr);
            lastCurr = curr;

            //	progressBar.SetValue(curr);
        }

        //int lastCost = -1;
        public void SetSpeedUpCostText(float timeRemaining, float baseTime)
        {
            int cost = DiamondCostCalculator.GetCostForAction(timeRemaining, baseTime);
            //if (cost != lastCost)
            {
                speedUpCostText.text = cost.ToString();
                //lastCost = cost;
            }
        }

        public void ChangedFromProductionToWaiting()
        {
            InitializeCollectingTable();
        }

        public void SpeedUpWithDiamonds()
        {
            table.SpeedUpWithDiamonds(this, delegate
            {
                SelectNextToSpeedUp();
            });
        }

        void SelectNextToSpeedUp()
        {
            ProbeTable nextWorkingTable = GetNextWorkingProbeTable();
            if (nextWorkingTable != null)
            {
                nextWorkingTable.SelectForSpeedUp();
            }
        }

        ProbeTable GetNextWorkingProbeTable()
        {
            ProbeTable[] allTables = FindObjectsOfType<ProbeTable>();

            for (int i = 0; i < allTables.Length; i++)
            {
                if (allTables[i].ProductionTimeLeft > 0)
                {
                    return allTables[i];
                }
            }
            Debug.Log("Did not find working probe table");
            return null;
        }

        private void InitializeWaitingTable()
        {
            ElixirMenuFrame.gameObject.SetActive(false);
            ElixirHover.gameObject.SetActive(false);
            waitingMenu.SetActive(true);
            if (table.producedElixir != null)
            {
                var rh = ResourcesHolder.Get();
                ElixirNameText.text = rh.GetNameForCure(table.producedElixir);
            }
        }

        private void InitializeCollectingTable()
        {
            waitingMenu.SetActive(false);
            ElixirHover.gameObject.SetActive(false);
            ElixirMenuFrame.gameObject.SetActive(true);
            ElixirMenuFrame.sizeDelta = new Vector2(140, ElixirMenuFrame.sizeDelta.y);
            var temp = GameObject.Instantiate(selectItemPrefab);
            temp.GetComponent<ProbeTableHoverElement>().Initialize(table, null, probeTableToolPrefab);
            temp.transform.position = collectingToolPosition.transform.position;
            temp.transform.SetParent(itemsParent);
            temp.transform.localScale = Vector3.one;
            temp.transform.SetAsLastSibling();
            //scrollRect.horizontalNormalizedPosition = 0;
            //	scrollRect.enabled = false;
            temp.SetActive(true);

            if (TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.BluePotionsCollected)
            {
                TutorialUIController.Instance.BlinkImage(temp.GetComponent<ProbeTableHoverElement>().GetMedicineImage());
            }
        }

        private void InitializeSelectTable()
        {
            waitingMenu.SetActive(false);
            ElixirMenuFrame.gameObject.SetActive(false);
            ElixirHover.gameObject.SetActive(true);
            medicines = ResourcesHolder.Get().GetMedicinesOfType(MedicineType.BaseElixir).OrderBy(x => x.minimumLevel).ToList();
            firstLockedMedicine = ResourcesHolder.Get().GetFirstLockedMedicine(MedicineType.BaseElixir);
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
        }

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
                temp.GetComponent<ProbeTableHoverElement>().Initialize(table, medicines[i].GetMedicineRef(), probeTableToolPrefab);
                temp.transform.SetParent(ElixirHover);
                temp.transform.localScale = Vector3.one;
                temp.transform.localPosition = spots[iterator];
                temp.transform.SetAsLastSibling();
                temp.SetActive(true);
                ++iterator;
            }

            if (currentPage == totalPages && firstLockedMedicine != null)
            {
                var temp = GameObject.Instantiate(selectItemPrefab);
                temp.GetComponent<ProbeTableHoverElement>().Initialize(table, firstLockedMedicine.GetMedicineRef(), null);
                temp.transform.SetParent(ElixirHover);
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
                            else
                                has_left = true;
                        }
                        else
                            has_left = true;
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

            if (Game.Instance.gameState().GetHospitalLevel() >= 6 && TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir)
            {
                TutorialUIController.Instance.StopBlinking();
                TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir = false;
            }
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
            if (Game.Instance.gameState().GetHospitalLevel() >= 6 && TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir)
            {
                TutorialUIController.Instance.StopBlinking();
                TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir = false;
            }
        }

        public bool IsArrowEnable()
        {
            foreach (Transform child in table.gameObject.transform) if (child.CompareTag("Arrow")) { return true; }
            return false;
        }

        public override void Close()
        {
            base.Close();
            GameState.isHoverOn = false;
            if (table.Selection != null)
                table.Selection.SetActive(false);
            indicators[currentPage - 1].sprite = pageInactive;
            for (int i = 0; i < itemsParent.childCount; i++)
                GameObject.Destroy(itemsParent.GetChild(i).gameObject);

            if (TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_collect_text || TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_seed_text_after)
            {
                TutorialUIController.Instance.Indicator.transform.localScale = Vector3.one;
                //TutorialUIController.Instance.tutorialArrowUI.Hide();
            }

            //TutorialUIController.Instance.Indicator.transform.localScale = Vector3.one;
            //TutorialUIController.Instance.ShowIndicatorCanvas();
            //TutorialUIController.Instance.StopFading(); //in case its fading blue elixir
        }

        protected override void Init()
        {
            base.Init();
            spots.Add(ElixirHover.Find("FirstSpot").gameObject.transform.localPosition);
            spots.Add(ElixirHover.Find("SecondSpot").gameObject.transform.localPosition);
            spots.Add(ElixirHover.Find("ThirdSpot").gameObject.transform.localPosition);

            indicators = new List<Image>();
            int count = pageIndicators.transform.childCount;
            Image p;
            for (int i = 0; i < count; i++)
            {
                p = pageIndicators.transform.GetChild(i).GetComponent<Image>();
                if (p != null)
                    indicators.Add(p);
            }
        }

        public static ProbeTableHover Open(ProbeTable probTable)
        {
            if (hover == null)
            {
                var p = GameObject.Instantiate(UIController.getHospital.probeTableHoverPrefab);
                hover = p.GetComponent<ProbeTableHover>();
                hover.Init();
                hover.table = probTable;
            }
            hover.Close();
            hover.Initialize();
            hover.table = probTable;
            //TutorialUIController.Instance.Indicator.transform.localScale = Vector3.zero;
            //TutorialUIController.Instance.HideIndicatorCanvas();
            //if (TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_collect_text || TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_seed_text_after)
            //{
            //    TutorialUIController.Instance.Indicator.transform.localScale = Vector3.zero;
            //}
            GameState.isHoverOn = true;
            return hover;
        }

        public static ProbeTableHover GetActive()
        {
            if (!hover)
            {
                return null;
            }

            return hover.gameObject.activeSelf ? hover : null;
        }

        public static RectTransform GetRect()
        {
            if (hover == null) return null;
            return hover.GetComponent<RectTransform>();
        }
    }
}