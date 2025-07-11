using UnityEngine;
using UnityEngine.UI;
using Hospital;
using TMPro;

namespace SimpleUI
{
	[ExecuteInEditMode]
	public class ProgressBarController : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI text = null;
		[SerializeField] private Image progressBar = null;
		public float actualValue = 50;
        public float lastValue = -1;
		public float MaxValue = 100;
		public bool showNumbers = true;
		public bool convertToTime = false;
		[SerializeField, Range(0, 1)] private float progress = 0f;

		public bool CanSpeedUp = false;
		public Text SpeedUpCostText;

		private RotatableObject rObj;
		private int lastDiamondCost = -1;
        
		public float Progress
		{
			get { return progress; }

			set
			{
				progress = value;
                lastValue = actualValue;
                actualValue = (int) (progress*MaxValue);
			}
		}

		private void Refresh()
		{
			if (progressBar != null)
				if (progressBar.fillAmount != progress)
				{
					progressBar.fillAmount = progress;
				}
            if(actualValue == lastValue)
            {
#if !UNITY_EDITOR       //this is a quick change to test quick level uping and exp bar
                return;
#endif
            }
            if (text != null)
			{
                if (convertToTime)
                {
                    string timeText = UIController.GetFormattedTime((int)MaxValue - (int)actualValue);
                    if(timeText != text.text)
                    {
                        text.text = timeText;
                    }
                }
                else
                {
                    string progressText = (int)actualValue + "/" + MaxValue;
                    if (text.text != progressText)
                    {
                        text.text = progressText;
                    }
                }
			}
			if (CanSpeedUp)
				RefreshSpeedUpCost();
		}
        
		public void SetTextEnabled(bool value)
		{
			showNumbers = value;
            if (value)
            {
                if (convertToTime)
                {
                    text.gameObject.SetActive(true);
                    text.text = UIController.GetFormattedTime((int)MaxValue - (int)actualValue);
                }
                else
                {
                    text.gameObject.SetActive(true);
                    text.text = (int)actualValue + "/" + MaxValue;
                }
            }
            else
            {
                if (text.gameObject.activeSelf)
                {
                    text.gameObject.SetActive(false);
                }
            }
		}

		public void SetMaxValue(float NewMaxValue)
		{
			MaxValue = NewMaxValue;
			progress = actualValue/MaxValue;
			Refresh();
		}

		public void AddValue(float Value)
		{
            lastValue = actualValue;
            actualValue += Value;
			progress = actualValue/MaxValue;
			Refresh();
		}

		public void SetValue(float Value)
		{
            lastValue = actualValue;
            actualValue = Value;
			progress = actualValue/MaxValue;
			Refresh();
		}
        
		public void SetRotatableObject(RotatableObject obj)
		{
			rObj = obj;
		}

		void RefreshSpeedUpCost()
		{
			if (!rObj)
			{
				Debug.Log("No rotatable object reference!");
				return;
			}

			int cost = rObj.GetSpeedUpCost();
            Debug.LogError("RefreshSpeedUpCost cost: " + cost);
            if (cost == lastDiamondCost)
			{
				return; //this is for optimization purposes, so the string operations below aren't executed every update
			}
            
			SpeedUpCostText.text = cost.ToString();
			lastDiamondCost = cost;
		}

        public void OpenNextLevelInfoPopup()
		{
            if (UIController.getHospital == null || UIController.getHospital.NextLevelPopUp.gameObject.activeSelf)
                return;
            StartCoroutine(UIController.getHospital.NextLevelPopUp.Open());
        }
	}

}
