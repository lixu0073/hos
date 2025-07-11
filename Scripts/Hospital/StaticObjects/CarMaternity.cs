using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MovementEffects;

namespace Hospital
{
    public class CarMaternity : SuperObject
    {

        public GameObject alarm;
        public GameObject lights;
        public int tapCount;

        private void Start()
        {
            tapCount = 0;
            alarm.SetActive(false);
            lights.SetActive(true);
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public override void OnClick()
        {
            //DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TapOnCampFire));
			SoundsController.Instance.PlayCarChirp();
            tapCount++;
            CarAlarm(tapCount);
        }

        public override void IsoDestroy()
        {
        }

        public void CarAlarm (int count)
        {
            if (count > 4)
            {
                alarm.SetActive(true);
                SoundsController.Instance.PlayCarAlarm();
				StartCoroutine(Cooldown());

				if (count > 5) lights.SetActive(false);
            }
            
        }

        public void ResetAlarm()
        {
            alarm.SetActive(false);
            lights.SetActive(true);
            tapCount = 0;
            SoundsController.Instance.StopAnySound(107);
        }

		IEnumerator Cooldown()
		{
			yield return new WaitForSeconds(5f);
			ResetAlarm();
		}


	}
}
