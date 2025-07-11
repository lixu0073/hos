using UnityEngine;
using MovementEffects;
using System.Collections.Generic;

namespace Hospital
{
    public class NoActivityController : MonoBehaviour
    {
        private static NoActivityController instance = null;
        //private Coroutine _checkingForActivity = null;

        public static NoActivityController Instance { get { return instance; } }

        public int activityDurationInSeconds;
        private double lastActivity = -1;

        void Start()
        {
            instance = this;
#if UNITY_EDITOR
            GameObject.Destroy(this);

            return;
#else
                lastActivity = ServerTime.getTime();
                StartCheckingForActivityCoroutine();
#endif
        }

        private void OnDisable()
        {
            StopCheckingForActivityCoroutine();
        }

        IEnumerator<float> checkingForActivityCoroutine = null;

        private void StopCheckingForActivityCoroutine()
        {
            if (checkingForActivityCoroutine != null)
            {
                Timing.KillCoroutine(checkingForActivityCoroutine);
                checkingForActivityCoroutine = null;
            }
        }

        private void StartCheckingForActivityCoroutine()
        {
            StopCheckingForActivityCoroutine();
            checkingForActivityCoroutine = Timing.RunCoroutine(CheckingForActivity());
        }

        IEnumerator<float> CheckingForActivity()
        {
            lastActivity = ServerTime.getTime();
            while (true)
            {
                yield return Timing.WaitForSeconds(1);
                if (lastActivity + activityDurationInSeconds < ServerTime.getTime())
                {
                    BaseUIController.ShowNonActivePopup(this, "");
                    GameObject.Destroy(this);
                    break;
                }
                yield return Timing.WaitForSeconds(10);
            }
        }

        public void UpdateLastActivityTime()
        {
            lastActivity = ServerTime.getTime();
        }

        public static void ClosePopup()
        {
            UIController.get.alertPopUp.NoSmoothExit();
            NoActivityController.Instance.lastActivity = ServerTime.getTime();
        }

        void Update()
        {
            Touch[] touches = Input.touches;
            bool activityRecorded = false;

            for (int i = 0; i < touches.Length; ++i)
            {
                if (touches[i].phase == TouchPhase.Began || touches[i].phase == TouchPhase.Ended)
                {
                    activityRecorded = true;
                    break;
                }
            }
            if (activityRecorded)
                lastActivity = ServerTime.getTime();
        }

    }
}
