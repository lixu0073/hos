using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleUI;
using MovementEffects;
using TMPro;

namespace Hospital
{
    public class BoosterButtonController : UIElement
    {
#pragma warning disable 0649
        [SerializeField] private Sprite noBoosterActive;
#pragma warning restore 0649
        [SerializeField] private Image boosterIcon = null;
        [SerializeField] private Image inactiveBoosterIcon = null;
        [SerializeField] private Image boosterBgg = null;
#pragma warning disable 0649
        [SerializeField] private Sprite inactiveBgg;
        [SerializeField] private Sprite activeBgg;
#pragma warning restore 0649
        //	[SerializeField] private GameObject[] dummies = null;
        [SerializeField] private TextMeshProUGUI boosterCounter = null;
        public GameObject Badge;
        public TextMeshProUGUI BadgeText;
        public GameObject timerBg;

        public void TurnOnCounting(int boosterID)
        {
            /*	for(int i=0; i < dummies.Length; i++){
                    dummies [i].SetActive (false);
                }*/
            boosterIcon.gameObject.SetActive(true);
            inactiveBoosterIcon.gameObject.SetActive(false);
            boosterIcon.sprite = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon;

            /*
            timerBg.SetActive(true);
            boosterCounter.gameObject.SetActive(true);
            */

            boosterBgg.sprite = activeBgg;
            Timing.RunCoroutine(Counting());
        }

        public void TurnOffCounting()
        {
            /*for(int i=0; i < dummies.Length; i++){
				dummies [i].SetActive (true);
			}*/
            boosterIcon.gameObject.SetActive(false);
            inactiveBoosterIcon.gameObject.SetActive(true);

            boosterIcon.sprite = noBoosterActive;
            boosterCounter.gameObject.SetActive(false);
            timerBg.SetActive(false);
            boosterBgg.sprite = inactiveBgg;
            Timing.KillCoroutine(Counting().GetType());
        }

        public void SetPulse(bool pulse)
        {
            GetComponent<Animator>().SetBool("AlertBool", pulse);
        }

        IEnumerator<float> Counting()
        {
            //initial wait so that boosterManager can init booster data like BoosterTimeLeft
            yield return Timing.WaitForSeconds(0.1f);

            //set active after WaitForSeconds() so that no old text values are seen
            timerBg.SetActive(true);
            boosterCounter.gameObject.SetActive(true);

            for (; ; )
            {
                boosterCounter.text = UIController.GetFormattedShortTime(HospitalAreasMapController.HospitalMap.boosterManager.BoosterTimeLeft);
                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}