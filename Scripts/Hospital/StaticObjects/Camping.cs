using UnityEngine;
using System.Collections;
using System;
using MovementEffects;

namespace Hospital
{
    public class Camping : SuperObject
    {

        public GameObject fire;
        public GameObject sparkles;
        public int fireCount;

        private void Start()
        {
            fireCount = 0;
            fire.SetActive(false);
            sparkles.SetActive(true);
        }

        public override void OnClick()
        {
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TapOnCampFire));
			SoundsController.Instance.PlayFire ();
            fireCount++;
            CampFire(fireCount);
        }

        public override void IsoDestroy()
        {
        }

        public void CampFire (int count)
        {
            if (count > 4)
            {
                fire.SetActive(true);
                SoundsController.Instance.PlayFireLoop();

                if (count > 5) sparkles.SetActive(false);
            }
            
        }

        public void ResetFireplace()
        {
            fire.SetActive(false);
            sparkles.SetActive(true);
            fireCount = 0;
            SoundsController.Instance.StopAnySound(96);
        }
    }
}
