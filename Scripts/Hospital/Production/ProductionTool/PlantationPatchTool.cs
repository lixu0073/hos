using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using IsoEngine;
using SimpleUI;
using MovementEffects;
using TMPro;
using System;

namespace Hospital
{

    public class PlantationPatchTool : MonoBehaviour
    {
        #region private
        [SerializeField] private TextMeshProUGUI goldAmount = null;
        [SerializeField] private Image medicineImage = null;
        [SerializeField] private GameObject upperPart = null;
        [SerializeField] private TextMeshProUGUI medicineName = null;
        [SerializeField] private TextMeshProUGUI productionTime = null;
        [SerializeField] private TextMeshProUGUI storageAmount = null;
        Vector2i position = Vector2i.zero;
        MedicineRef medicine;
        PlantationPatch plantationPatch;
        public PlantationPatchToolType type;
        private int goldNeed;
        private int timeNeed;
        Vector3 firstPos = Vector3.zero;
        private OnEvent onEnd;
        bool shouldEnd = true;
        /// <summary> Can seeds be planted and collected from all sources or just the first one. </summary>
        private bool canWorkOnOthers = false;
        #endregion

        delegate void DToolWork(Transform target);
        DToolWork toolWork;
        private bool b_stopCoroutine;
#pragma warning disable 0649
        [SerializeField] private RectTransform collectPoint;
        [SerializeField] private RectTransform ToolTipWindowRectTransform;
        [SerializeField] private RectTransform BogoTransform;
        [SerializeField] private Transform[] anchors;
#pragma warning restore 0649
        public float XDistance = 4;
        public float YDistance = 0.62f;

        void OnDisable()
        {
            UIController.get.ActiveTool = null;
        }

        void OnDestroy()
        {
            UIController.get.ActiveTool = null;
        }
        /// <summary>
        /// Initialise the tool. Set values to default and start coroutines for the tool.
        /// </summary>
        public void Initialize(PlantationPatch patch, PlantationPatchToolType toolType, MedicineRef med = null, OnEvent onEnd = null)
        {
            UIController.get.ActiveTool = gameObject;
            this.onEnd = onEnd;
            medicine = med;
            plantationPatch = patch;
            this.type = toolType;
            //this.type = med == null ? TableToolType.collect : TableToolType.seed;

            if (med != null)
            {
                goldAmount.transform.parent.gameObject.SetActive(true);
                productionTime.gameObject.SetActive(true);

                medicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(med);
                medicineName.text = ResourcesHolder.Get().GetNameForCure(med);
                var bei = ResourcesHolder.Get().GetMedicineInfos(medicine) as BasePlantInfo;
                goldNeed = (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.garden_text) ? 0 : bei.Price;
                timeNeed = (int)bei.ProductionTime;
                upperPart.SetActive(true);
                upperPart.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 140);
                ActualiseTooltip();
                //MedicineTooltip.Open(medicine, true);
            }
            else
            {
                upperPart.SetActive(false);

                switch (toolType)
                {
                    case PlantationPatchToolType.collect:
                        medicineImage.sprite = ResourcesHolder.GetHospital().PlantHarvestingSprite;
                        break;
                    case PlantationPatchToolType.renew:
                        medicineName.text = ResourcesHolder.Get().GetNameForCure(ResourcesHolder.Get().medicines.cures[15].medicines[3].GetMedicineRef());
                        storageAmount.text = GameState.Get().GetCureCount(ResourcesHolder.Get().medicines.cures[15].medicines[3].GetMedicineRef()).ToString();
                        goldAmount.transform.parent.gameObject.SetActive(false);
                        productionTime.gameObject.SetActive(false);
                        medicineImage.sprite = ResourcesHolder.GetHospital().PatchCultivatorSprite;
                        upperPart.SetActive(true);
                        upperPart.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 85);
                        break;
                    case PlantationPatchToolType.help:
                        medicineImage.sprite = ResourcesHolder.GetHospital().PatchHelpSignSprite;
                        break;
                    default:
                        break;
                }
                //medicineImage.sprite = ResourcesHolder.Get().ProbeTableToolSprite;
            }

            //ActualisePanaceaCounter();
            //startToolCoroutine
            switch (toolType)
            {
                case PlantationPatchToolType.seed:
                    toolWork = seedWork;
                    break;
                case PlantationPatchToolType.collect:
                    toolWork = collectWork;
                    break;
                case PlantationPatchToolType.renew:
                    toolWork = cultivateWork;
                    break;
                case PlantationPatchToolType.help:
                    toolWork = helpAskWork;
                    break;
                default:
                    break;
            }
            b_stopCoroutine = false;
            canWorkOnOthers = false;
            Timing.RunCoroutine(ToolWorks());
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
        {//TODO_Duobix
            var temp = Input.mousePosition;

            float XCutoff = Screen.width / XDistance;
            float YCutoff = Screen.height * YDistance;

            bool right = false;
            bool down = false;
            //check where is the window as of now
            Vector3[] vrect = new Vector3[4];

            BogoTransform.GetWorldCorners(vrect);

            if (vrect[1].x < 0)
            {//temp.x < XCutoff) {
                right = true;
            }
            if (vrect[1].y > Screen.height)
            {//temp.y > YCutoff) {
                down = true;
            }
            //if (temp.x < XCutoff)
            //{
            //    right = true;
            //}
            //if (temp.y > YCutoff)
            //{
            //    down = true;
            //}
            //set position of the window:
            if (right && down)
            {
                ToolTipWindowRectTransform.gameObject.transform.position = anchors[2].position; //RD
                return;
            }
            if (right && !down)
            {
                ToolTipWindowRectTransform.gameObject.transform.position = anchors[1].position;//RU
                return;
            }
            if (!right && down)
            {
                ToolTipWindowRectTransform.gameObject.transform.position = anchors[0].position;//LD
                return;
            }
            ToolTipWindowRectTransform.gameObject.transform.position = anchors[3].position;//LU
            return;
        }

        private void ActualiseTooltip()
        {
            goldAmount.text = goldNeed.ToString();
            productionTime.text = UIController.GetFormattedTime(timeNeed);
            storageAmount.text = GameState.Get().GetCureCount(medicine).ToString();
            //var am = Game.Instance.gameState().GetCoinAmount();
            //goldAmount.color = am >= goldNeed ? Color.white : Color.red;
        }

        public void Close()
        {
            b_stopCoroutine = true;
            Debug.Log("PlanatationPatchTool Close()");
            //plantationPatch.Selection.SetActive(false);
            if (shouldEnd)
                onEnd?.Invoke();
            GameObject.Destroy(gameObject);
        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0) || VisitingController.Instance.IsVisiting)
            {
                b_stopCoroutine = true;
                Close();
            }

            transform.position = Input.mousePosition;
            SetSubWindowPosition();
        }

        void seedWork(Transform patchT)
        {
            if (patchT.GetComponent<PlantationPatch>().patchSelected || canWorkOnOthers)            
                canWorkOnOthers = true;
            else
                return;
            if (patchT.GetComponent<PlantationPatch>().Seed(medicine, false, this))            
                ActualiseTooltip();
        }

        void collectWork(Transform patchT)
        {
            if (patchT.GetComponent<PlantationPatch>().patchSelected || canWorkOnOthers)
                canWorkOnOthers = true;
            else
                return;

            if (!patchT.GetComponent<PlantationPatch>().Collect())
            {
                b_stopCoroutine = true;
                Close();
            }
        }

        void cultivateWork(Transform patchT)
        {
            if (patchT.GetComponent<PlantationPatch>().patchSelected || canWorkOnOthers)
                canWorkOnOthers = true;
            else
                return;

            patchT.GetComponent<PlantationPatch>().Cultivate(this);
        }

        void helpAskWork(Transform patchT)
        {
            patchT.GetComponent<PlantationPatch>().HelpAsk();
        }

        IEnumerator<float> ToolWorks()
        {
            for (; ; )
            {
                if (b_stopCoroutine)
                    break;

                RaycastHit hit;
                //Ray ray = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenPointToRay (Input.mousePosition);
                Ray ray = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenPointToRay(collectPoint.position);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;
                    try
                    {
                        if (objectHit.GetComponent<PlantationPatch>() != null)
                        {
                            objectHit.GetComponent<PlantationPatch>().CloseHover();
                            toolWork(objectHit);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Getting of component failed: " + e.Message);
                    }
                }
                //tool works every frame
                yield return 0f;
            }
        }
    }

    public enum PlantationPatchToolType
    {
        seed,
        collect,
        help,
        renew,
    }
}
