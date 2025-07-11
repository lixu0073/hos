using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;

namespace Hospital
{
    /// <summary>
    /// 增益道具信息弹窗控制器，负责显示增益道具的详细信息界面。
    /// 处理增益道具的详细描述、剩余时间倒计时、使用状态显示等功能。
    /// </summary>
    public class BoosterInfoPopUpController : UIElement
    {
		[SerializeField] private TextMeshProUGUI boosterInfo = null;
		[SerializeField] private TextMeshProUGUI boosterCounter = null;
#pragma warning disable 0414
        [SerializeField] private TextMeshProUGUI boosterButtonText = null;
#pragma warning restore 0414
        [SerializeField] private Button boosterButton = null;
		[SerializeField] private Image boosterImage = null;
		private int boosterID = -1;

		delegate void ButtonAction();
		ButtonAction buttonAction;

		public void ButtonExit()
		{
			Timing.KillCoroutine(Counting().GetType());
			if (!HospitalAreasMapController.HospitalMap.boosterManager.boosterActive)
				StartCoroutine(UIController.getHospital.BoosterMenuPopUp.Open(true, false, () =>
				{
					Exit();
				}));
			else
				Exit();
		}

		public void OpenOnBooster(int boosterID)
        {
			gameObject.SetActive(true);
			StartCoroutine(Open(true, false, () =>
			{
				boosterInfo.SetText(I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo));
				boosterImage.sprite = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon;

				this.boosterID = boosterID;
				if (HospitalAreasMapController.HospitalMap.boosterManager.boosterActive)
				{
					boosterButton.gameObject.SetActive(false);
					boosterCounter.gameObject.SetActive(true);
					Timing.RunCoroutine(Counting());
				}
			}));
		}

		private string SecToTime(int sec)
        {
			string time = "";
			int hours = sec / 3600;
			if (hours > 0)
				time += hours + "h ";
			int minutes = (sec % 3600)/60;
			if (minutes > 0 || hours > 0)
				time += minutes + "m ";
			int seconds = sec - hours * 3600 - minutes * 60; 
			if (seconds > 0 || minutes > 0 || hours > 0)
				time += seconds + "s";
			return time;
		}

		IEnumerator<float> Counting()
        {
			for (;;)
            {
				Debug.Log("CoroutineWorking");
				int secTillEnd = HospitalAreasMapController.HospitalMap.boosterManager.BoosterEndTime - Convert.ToInt32(ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
				if (secTillEnd > 0) 
					boosterCounter.SetText(HospitalAreasMapController.HospitalMap.boosterManager.GetBoosterTimeLeftString());
				else 
                {
					ButtonExit();
					break;
				}
				yield return Timing.WaitForSeconds(1);
			}
		}
    }
}