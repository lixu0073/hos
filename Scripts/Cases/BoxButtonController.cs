using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using MovementEffects;
using System;

namespace Hospital
{
    public class BoxButtonController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI caseCounter = null;
        [SerializeField]
        private GameObject[] dummies = null;
		private Animator animator = null;

		void Awake(){
			animator = GetComponent<Animator> ();
		}

		public void turnOnCounting()
        {
			//animator.SetTrigger ("Normal");
            for (int i = 0; i < dummies.Length; i++)
            {
                dummies[i].SetActive(false);
            }
            caseCounter.transform.parent.gameObject.SetActive(true);
            Timing.RunCoroutine(Counting());
        }

        public void turnOffCounting()
        {
			turnOnAlert ();
            for (int i = 0; i < dummies.Length; i++)
            {
                dummies[i].SetActive(true);
            }
            caseCounter.transform.parent.gameObject.SetActive(false);
            Timing.KillCoroutine(Counting().GetType());
        }


		public void turnOnAlert(){
			if(animator != null)
				animator.SetBool("AlertBool", true);
		}
		public void turnOffAlert(){
			if(animator != null){
				animator.SetTrigger ("Normal");
				animator.SetBool ("AlertBool", false);
			}
		}

        IEnumerator<float> Counting()
        {
            for (;;)
            {
                caseCounter.text = UIController.GetFormattedShortTime(((HospitalCasesManager)AreaMapController.Map.casesManager).deliveryIntervalSeconds - (Convert.ToInt32((long)ServerTime.getTime()) - ((HospitalCasesManager)AreaMapController.Map.casesManager).countingStartTime));
                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}