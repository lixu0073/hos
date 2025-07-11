using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public class MaternityWaitingRoomIndicatorsPresenter : MonoBehaviour
    {

        [SerializeField]
        public GameObject cureIndicator = null;
        [SerializeField]
        public GameObject newIndicator = null;
        [SerializeField]
        public GameObject waitIndicator = null;
        [SerializeField]
        public GameObject readyForLaborIndicator = null;
		[SerializeField]
		public GameObject laborEndedIndicator = null;
		[SerializeField]
		public GameObject claimRewardIndicatorBoy = null;
        [SerializeField]
        public GameObject claimRewardIndicatorGirl = null;
        [SerializeField]
		public GameObject patientOnWayIndicator = null;

		public Action onClick;

        public void HideAll()
        {
            /*
            cureIndicator.SetActive(false);                        
            newIndicator.SetActive(false);
            waitIndicator.SetActive(false);
            readyForLaborIndicator.SetActive(false);
			laborEndedIndicator.SetActive(false);
			claimRewardIndicatorBoy.SetActive(false);
            claimRewardIndicatorGirl.SetActive(false);
            patientOnWayIndicator.SetActive(false);
            */

            TrySetActiveIndicator(cureIndicator, false, "cureIndicator is null!");
            TrySetActiveIndicator(newIndicator, false, "newIndicator is null!");
            TrySetActiveIndicator(waitIndicator, false, "waitIndicator is null!");
            TrySetActiveIndicator(readyForLaborIndicator, false, "readyForLaborIndicator is null!");
            TrySetActiveIndicator(laborEndedIndicator, false, "laborEndedIndicator is null!");
            TrySetActiveIndicator(claimRewardIndicatorBoy, false, "claimRewardIndicatorBoy is null!");
            TrySetActiveIndicator(claimRewardIndicatorGirl, false, "claimRewardIndicatorGirl is null!");
            TrySetActiveIndicator(patientOnWayIndicator, false, "patientOnWayIndicator is null!");
        }

        private void TrySetActiveIndicator(GameObject indicator, bool active, string messageOnFail)
        {
            if(indicator == null)
            {
                Debug.LogError(messageOnFail);
                return;
            }

            indicator.SetActive(active);
        }

        public void OnClick()
        {
            onClick?.Invoke();
        }
           
    }
}
