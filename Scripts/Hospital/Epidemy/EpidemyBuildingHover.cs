using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;
namespace SimpleUI
{

    public class EpidemyBuildingHover : BaseHover
    {
        private EpidemyObjectController epidemyObjectController;
        [SerializeField] private Image buyWithDiamondsImage = null;
        [SerializeField] private TextMeshProUGUI buyCost = null;
        [SerializeField] private ProgressBarController progressBar = null;

        public ProgressBarController GetProgressBar()
        {
            return progressBar;
        }

        public void Initialize(EpidemyObjectController epidemyObjectController)
        {
            base.Initialize();
            base.Init();
            this.epidemyObjectController = epidemyObjectController;

            progressBar.SetMaxValue(epidemyObjectController.EpidemyObjectInfo.RenovatingTimeSeconds);
            progressBar.SetTextEnabled(true);
            progressBar.convertToTime = true;

            epidemyObjectController.ObjectBorder.SetActive(true);

            int cost = DiamondCostCalculator.GetCostForBuilding(epidemyObjectController.TimeSinceRenovationStarted, epidemyObjectController.EpidemyObjectInfo.RenovatingTimeSeconds);
            buyCost.text = cost.ToString();
            //Debug.LogError("buyCost " + cost);

            this.epidemyObjectController = epidemyObjectController;
        }

        public void OnSpeedUpClick()
        {
            epidemyObjectController.OnClickSpeedUp(this);
        }

        public static EpidemyBuildingHover activeHover
        {
            get; private set;
        }

        public static EpidemyBuildingHover Open(EpidemyObjectController epidemyObject)
        {
            if (activeHover == null)
            {
                var p = GameObject.Instantiate(UIController.getHospital.EpidemyObjectHoverPrefab);
                activeHover = p.GetComponent<EpidemyBuildingHover>();
                activeHover.Init();
                activeHover.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ChangeOnTouchType(x =>
            {
                if (activeHover != null)
                    activeHover.Close();
                ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ResetOntouchAction();
            });

            //activeHover.Close();
            activeHover.Initialize(epidemyObject);
            activeHover.UpdateHover();

            //move camera on epidemy building (this is a quick workaround of adjusting camera to Hovers like in rotatableObjects)
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(epidemyObject.transform.position, 1.0f, true);

            //tutorial stuff start
            activeHover.buyWithDiamondsImage.transform.localScale = new Vector3(0.833f, 0.833f, 0.833f);        //this might not work when button click animations are added to project.

            TutorialUIController.Instance.HideIndicatorCanvas();
            //tutorial stuff end
            return activeHover;
        }

        public override void Close()
        {
            base.Close();
            epidemyObjectController.ObjectBorder.SetActive(false);
        }

        public void UpdateHover()
        {
            progressBar.SetValue(epidemyObjectController.TimeSinceRenovationStarted);

            int cost = DiamondCostCalculator.GetCostForBuilding(epidemyObjectController.EpidemyObjectInfo.RenovatingTimeSeconds - epidemyObjectController.TimeSinceRenovationStarted, epidemyObjectController.EpidemyObjectInfo.RenovatingTimeSeconds);
            //Debug.LogError("UpdateHover cost = " + cost);
            buyCost.text = cost.ToString();
        }
    }
}
