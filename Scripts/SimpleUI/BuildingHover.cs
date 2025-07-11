using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;
using Maternity;

namespace SimpleUI
{

    public class BuildingHover : BaseHover
    {

        RotatableObject rotatable;
        [SerializeField]
        private Image buyWithDiamondsImage = null;
        [SerializeField]
        private TextMeshProUGUI buyCost = null;
        [SerializeField]
        private ProgressBarController progressBar = null;
        [SerializeField] private TextMeshProUGUI buildingName = null;

        public RectTransform GetBuyWithDiamondsRect()
        {
            return buyWithDiamondsImage.GetComponent<RectTransform>();
        }
        public ProgressBarController GetProgressBar()
        {
            return progressBar;
        }
        public void Initialize(float buildTime, RotatableObject rotatable, string name)
        {
            base.Initialize();
            base.Init();
            progressBar.SetMaxValue(buildTime);
            progressBar.SetTextEnabled(true);
            progressBar.convertToTime = true;

            int cost = DiamondCostCalculator.GetCostForBuilding(rotatable.BuildStartTime, rotatable.BuildTime, rotatable.Tag);
            if (cost == 0)
                buyCost.text = I2.Loc.ScriptLocalization.Get("FREE");
            else
                buyCost.text = cost.ToString();
            buildingName.SetText(name);

            this.rotatable = rotatable;
        }
        public void OnSpeedUpClick()
        {
            rotatable.OnClickSpeedUp(this);
        }
        public static BuildingHover activeHover { get; private set; }

        public static BuildingHover Open(float buildTime, RotatableObject rotatable, string name)
        {
            //Debug.LogError("BuildingHover Open");

            if (activeHover == null)
            {
                var p = Instantiate(UIController.get.buildingHoverPrefab);
                activeHover = p.GetComponent<BuildingHover>();
                activeHover.Init();
                activeHover.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            //activeHover.Close();
            activeHover.Initialize(buildTime, rotatable, name);

            //tutorial stuff start
            activeHover.buyWithDiamondsImage.transform.localScale = new Vector3(0.833f, 0.833f, 0.833f);        //this might not work when button click animations are added to project.
            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.build_doctor_finish ||
                TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_lab_build ||
                TutorialController.Instance.CurrentTutorialStepTag == StepTag.green_doc_unpack ||
                TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_mixer_text ||
                TutorialController.Instance.CurrentTutorialStepTag == StepTag.yellow_doc_build ||
                activeHover.IsWaitingRoomBuildFinishStepEnabled() ||
                activeHover.IsLaborRoomBuildFinishStepEnabled())
            {   //step z przyspieszeniem doktora (lub odczekaniem az sie zbuduje)
                if (!activeHover.buyWithDiamondsImage)
                {
                    Debug.LogError("Please update reference to speed up button's Image in BuildingHover prefab!");
                }
                TutorialUIController.Instance.BlinkImage(activeHover.buyWithDiamondsImage);
                //TutorialUIController.Instance.ShowTutorialArrowUI(activeHover.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForFreeSpeedUpButton, 0, TutorialUIController.TutorialPointerAnimationType.tap);
            }
            TutorialUIController.Instance.HideIndicatorCanvas();
            //tutorial stuff end
            activeHover.gameObject.SetActive(true);
            BaseGameState.isHoverOn = true;
            return activeHover;
        }

        public bool IsWaitingRoomBuildFinishStepEnabled()
        {
            return TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_waiting_room_finish && rotatable is MaternityWaitingRoom;
        }

        public bool IsLaborRoomBuildFinishStepEnabled()
        {
            return TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_labor_room_finish && rotatable is MaternityLabourRoom;
        }

        public override void Close()
        {
            //Debug.LogError("BuildingHover Close");
            base.Close();

            BaseGameState.isHoverOn = false;
            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.build_doctor_finish || IsWaitingRoomBuildFinishStepEnabled() || IsLaborRoomBuildFinishStepEnabled())
                TutorialUIController.Instance.StopBlinking();

            if (rotatable != null)
                rotatable.SetBorderActive(false);

            TutorialController tc = TutorialController.Instance;
            if ((tc.tutorialEnabled && tc.GetCurrentStepData().StepTag == StepTag.build_doctor_finish && rotatable is DoctorRoom) || IsWaitingRoomBuildFinishStepEnabled() || IsLaborRoomBuildFinishStepEnabled())
            {
                TutorialUIController.Instance.ShowHintIndictator(rotatable);
            }

        }

        public static BuildingHover GetActive()
        {
            return activeHover;
        }
    }
}
