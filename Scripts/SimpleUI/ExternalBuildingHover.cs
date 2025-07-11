using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;
namespace SimpleUI
{

    public class ExternalBuildingHover : BaseHover {


		ExternalRoom externalRoom;
        [SerializeField]
        private Image buyWithDiamondsImage = null;
        [SerializeField]
		private TextMeshProUGUI buyCost = null;
		[SerializeField]
		private ProgressBarController progressBar = null;
		[SerializeField] private TextMeshProUGUI buildingName = null;

		public ProgressBarController GetProgressBar()
		{
			return progressBar;
		}
		public void Initialize(ExternalRoom externalRoom)
        {
            base.Initialize();
            base.Init();
			progressBar.SetMaxValue(externalRoom.roomInfo.RenovatingTimeSeconds);
			progressBar.SetTextEnabled(true);
			progressBar.convertToTime = true;

			int cost = DiamondCostCalculator.GetCostForBuilding(externalRoom.roomInfo.RenovatingTimeSeconds - externalRoom.TimeFromRenovationStarts, externalRoom.roomInfo.RenovatingTimeSeconds);
            buyCost.text = cost.ToString();
			buildingName.SetText (I2.Loc.ScriptLocalization.Get(externalRoom.roomInfo.roomName));

			this.externalRoom = externalRoom;
		}
		public void OnSpeedUpClick()
		{
			externalRoom.OnClickSpeedUp(this);
		}
		public static ExternalBuildingHover activeHover { get; private set; }

		public static ExternalBuildingHover Open(ExternalRoom externalRoom)
        {
            if (activeHover == null)
            {
                var p = GameObject.Instantiate(UIController.get.externalBuildingHoverPrefab);
				activeHover = p.GetComponent<ExternalBuildingHover>();
				activeHover.Init();
				activeHover.transform.localScale = new Vector3(1f, 1f, 1f);
            }
			//ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ResetOntouchAction();
			ReferenceHolder.Get().engine.GetMap<AreaMapController>().ChangeOnTouchType(x =>
				{
					//activeHover.Close();
					if (ExternalBuildingHover.activeHover != null)
						ExternalBuildingHover.activeHover.Close();
					ReferenceHolder.Get().engine.GetMap<AreaMapController>().ResetOntouchAction();
					//	progressBar = null;
				});


			activeHover.Close();
			activeHover.Initialize(externalRoom);
            activeHover.UpdateHover();

            //tutorial stuff start
            activeHover.buyWithDiamondsImage.transform.localScale = new Vector3(0.833f, 0.833f, 0.833f);        //this might not work when button click animations are added to project.
            
            TutorialUIController.Instance.HideIndicatorCanvas();
            //tutorial stuff end
		
            return activeHover;


        }

        public override void Close() {

           base.Close();

        }

		public void UpdateHover(){
			progressBar.SetValue(externalRoom.TimeFromRenovationStarts);
            
			int cost = DiamondCostCalculator.GetCostForBuilding(externalRoom.roomInfo.RenovatingTimeSeconds - externalRoom.TimeFromRenovationStarts, externalRoom.roomInfo.RenovatingTimeSeconds);
			buyCost.text = cost.ToString();
        }
    }
}
