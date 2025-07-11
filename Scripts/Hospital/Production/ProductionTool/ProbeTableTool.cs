using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using IsoEngine;
using SimpleUI;
using TMPro;

namespace Hospital
{
    public class ProbeTableTool : MonoBehaviour
    {
        #region statics

        public static List<IFillable> fillable;
        public static List<ICollectable> collectable;

        static ProbeTableTool()
        {
            fillable = new List<IFillable>();
            collectable = new List<ICollectable>();
        }

        #endregion

        #region private

        [SerializeField] private TextMeshProUGUI panaceaAmount = null;
        [SerializeField] private Image medicineImage = null;
        [SerializeField] private GameObject upperPart = null;
        [SerializeField] private TextMeshProUGUI medicineName = null;
        [SerializeField] private TextMeshProUGUI storageAmount = null;
        Vector2i positionV2I;
#pragma warning disable 0649
        [SerializeField] private Transform[] anchors;
#pragma warning restore 0649
        Vector3 position;
        MedicineRef medicine;
        ProbeTable probTable;
#pragma warning disable 0649
        [SerializeField] private RectTransform BogoTransform;
        [SerializeField] private RectTransform ToolTipWindowRectTransform;
#pragma warning restore 0649
        private RectTransform ThisRectTransform;
        public float XDistance = 4;
        public float YDistance = 0.67f;

        TableToolType type;
        private int panaceaNeed;
        private OnEvent onEnd;

        Vector3 tooltipPosition;

        #endregion

        void OnEnable()
        {
            PanaceaCollector.OnPanaceaAmountChanged += ActualisePanaceaCounter;
        }

        void OnDisable()
        {
            PanaceaCollector.OnPanaceaAmountChanged -= ActualisePanaceaCounter;
            UIController.get.ActiveTool = null;
        }

        void OnDestroy()
        {
            UIController.get.ActiveTool = null;
        }

        public void Initialize(ProbeTable table, MedicineRef med = null, OnEvent onEnd = null)
        {
            UIController.get.ActiveTool = gameObject;
            ThisRectTransform = this.GetComponent<RectTransform>();
            position = ThisRectTransform.position;
            this.onEnd = onEnd;
            medicine = med;
            probTable = table;
            this.type = med == null ? TableToolType.collect : TableToolType.fill;

            if (med != null)
            {
                medicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(med);
                medicineName.text = ResourcesHolder.Get().GetNameForCure(med);
                storageAmount.text = GameState.Get().GetCureCount(med).ToString();
                var elixirInfo = ResourcesHolder.Get().GetMedicineInfos(medicine) as BaseElixirInfo;
                if (elixirInfo)
                    panaceaNeed = elixirInfo.PanaceaAmount;
                upperPart.SetActive(true);
                //MedicineTooltip.Open(medicine, true);
            }
            else
            {
                medicineImage.sprite = ResourcesHolder.GetHospital().ProbeTableToolSprite;
                upperPart.SetActive(false);
            }

            var p = ReferenceHolder.Get().engine.MainCamera.RayCast(Input.mousePosition);
            positionV2I = new Vector2i((int)(p.x + 0.5f), (int)(p.z + 0.5f));
            ActualisePanaceaCounter();
            SetToolScale();
            SetSubWindowPosition();
        }

        public void SetToolScale()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
                transform.localScale = Vector3.one * 1.333f;
            else
                transform.localScale = Vector3.one;
        }

        /// <summary>
        /// This is used to move the little tooltip window around.
        /// </summary>
        protected void SetSubWindowPosition()
        {
            var temp = Input.mousePosition;

            //float XCutoff = Screen.width / XDistance;
            //float YCutoff = Screen.height * YDistance;

            bool right = false;
            bool down = false;
            //check where is the window as of now
            Vector3[] vrect = new Vector3[4];

            BogoTransform.GetWorldCorners(vrect);

            if (vrect[1].x < 0) //temp.x < XCutoff) {
            {
                right = true;
            }

            if (vrect[1].y > Screen.height) //temp.y > YCutoff)
            {
                down = true;
            }

            //set position of the window:
            if (right && down)
            {
                ToolTipWindowRectTransform.gameObject.transform.position = anchors[2].position;
                tooltipPosition = ToolTipWindowRectTransform.localPosition;
                return;
            }

            if (right && !down)
            {
                ToolTipWindowRectTransform.gameObject.transform.position = anchors[1].position;
                tooltipPosition = ToolTipWindowRectTransform.localPosition;
                return;
            }

            if (!right && down)
            {
                ToolTipWindowRectTransform.gameObject.transform.position = anchors[0].position;
                tooltipPosition = ToolTipWindowRectTransform.localPosition;
                return;
            }

            ToolTipWindowRectTransform.gameObject.transform.position = anchors[3].position;
            tooltipPosition = ToolTipWindowRectTransform.localPosition;
            return;
        }

        private void CheckTablesForAction(Vector2i position)
        {
            if (VisitingController.Instance.IsVisiting)
            {
                Close();
                return;
            }

            //This was used to only allow collection or placement from the first probeTable
            /* if (firstTable)
            {
                switch (type)
                {
                    case TableToolType.collect:
                        if (probTable.position == position) 
                        {
                            probTable.Collect(position);
                            probTable.Selection.SetActive(false);
                            probTable.CloseHover();
                            firstTable = false;
                        }
                        break;
                    case TableToolType.seed:
                        if (probTable.position == position)
                        {
                            probTable.Seed(position, medicine);
                            ActualisePanaceaCounter();
                            probTable.CloseHover();
                            probTable.Selection.SetActive(false);
                            firstTable = false;
                        }
                        break;
                }
            } */
            switch (type)
            {
                case TableToolType.collect:
                    foreach (var p in collectable)
                    {
                        if (p.Collect(position))
                        {
                            probTable.Selection.SetActive(false);
                            probTable.CloseHover();
                        }
                    }

                    break;
                case TableToolType.fill:
                    foreach (var p in fillable)
                    {
                        if (p.Fill(position, medicine))
                        {
                            ActualisePanaceaCounter();
                            probTable.Selection.SetActive(false);
                            probTable.CloseHover();
                        }
                    }

                    break;
            }
        }

        private void ActualisePanaceaCounter()
        {
            var am = GameState.Get().CheckPanaceaAmount();
            panaceaAmount.text = am + "/" + panaceaNeed;
            panaceaAmount.color = am >= panaceaNeed ? Color.white : Color.red;
        }

        void LateUpdate()
        {
            if (TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_collect_text ||
                TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_seed_text_after)
            {
                if (!Input.GetMouseButtonUp(0))
                {
                    TutorialUIController.Instance.Indicator.transform.localScale = Vector3.zero;
                }
            }
        }

        private void Close()
        {
            onEnd?.Invoke();

            if (TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_collect_text)
            {
                foreach (ProbeTable pt in TutorialController.Instance.GetAllFullProbeTables())
                {
                    TutorialUIController.Instance.ShowIndictator(pt);
                    break;
                }
            }
            else if (TutorialController.Instance.GetCurrentStepData().StepTag == StepTag.elixir_seed_text_after)
            {
                foreach (ProbeTable pt in TutorialController.Instance.GetAllEmptyProbeTables())
                {
                    TutorialUIController.Instance.ShowIndictator(pt);
                    break;
                }
            }

            Destroy(gameObject);
        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0) || VisitingController.Instance.IsVisiting)
            {
                Close();
            }

            transform.position = Input.mousePosition;
            var p = ReferenceHolder.Get().engine.MainCamera.RayCast(Input.mousePosition);
            var newPosition = new Vector2i((int)(p.x + 0.5f), (int)(p.z + 0.5f));
            if (positionV2I != newPosition)
            {
                CheckTablesForAction(newPosition);
                positionV2I = newPosition;
            }

            if (Input.GetKey(KeyCode.P))
                Debug.Break();

            SetSubWindowPosition();
        }
    }

    /// <summary>
    /// There is two types of ProbeTables, collectable and fillable test tubes.
    /// </summary>
    public enum TableToolType
    {
        collect,
        fill
    }
}