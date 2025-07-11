using Hospital;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
    public class MaternityWardBuildingHover : BaseHover
    {
        private MaternityWard maternityWardObjectController;
        [SerializeField]
        private Image buyWithDiamondsImage = null;
        [SerializeField]
        private TextMeshProUGUI buyCost = null;
        [SerializeField]
        private ProgressBarController progressBar = null;

        public ProgressBarController GetProgressBar()
        {
            return progressBar;
        }

        public void Initialize(MaternityWard maternityWardObjectController)
        {
            base.Initialize();
            base.Init();
            this.maternityWardObjectController = maternityWardObjectController;

            progressBar.SetMaxValue(maternityWardObjectController.Info.RenovatingTimeSeconds);
            progressBar.SetTextEnabled(true);
            progressBar.convertToTime = true;
            
            int cost = DiamondCostCalculator.GetCostForBuilding(maternityWardObjectController.TimeSinceRenovationStarted, maternityWardObjectController.Info.RenovatingTimeSeconds);
            buyCost.text = cost.ToString();
        }

        public void OnSpeedUpClick()
        {
            maternityWardObjectController.OnClickSpeedUp(this);
        }

        public static MaternityWardBuildingHover activeHover
        {
            get; private set;
        }

        public static MaternityWardBuildingHover Open(MaternityWard maternityWardObject)
        {
            if (activeHover == null)
            {
                var p = GameObject.Instantiate(UIController.getHospital.MaternityWardObjectHoverPrefab);
                activeHover = p.GetComponent<MaternityWardBuildingHover>();
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
            activeHover.Initialize(maternityWardObject);
            activeHover.UpdateHover();

            //move camera on epidemy building (this is a quick workaround of adjusting camera to Hovers like in rotatableObjects)
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(maternityWardObject.transform.position, 1.0f, true);

            //tutorial stuff start
            activeHover.buyWithDiamondsImage.transform.localScale = new Vector3(0.833f, 0.833f, 0.833f);        //this might not work when button click animations are added to project.

            TutorialUIController.Instance.HideIndicatorCanvas();
            //tutorial stuff end
            return activeHover;
        }

        public override void Close()
        {
            base.Close();
            maternityWardObjectController.ObjectBorder.SetActive(false);
        }

        public void UpdateHover()
        {
            progressBar.SetValue(maternityWardObjectController.TimeSinceRenovationStarted);
            int cost = DiamondCostCalculator.GetCostForBuilding(maternityWardObjectController.Info.RenovatingTimeSeconds - maternityWardObjectController.TimeSinceRenovationStarted, maternityWardObjectController.Info.RenovatingTimeSeconds);
            buyCost.text = cost.ToString();
        }

    }
}
