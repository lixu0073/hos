using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Maternity.UI
{
    public class MaternityBedInfoPanelUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject waitingTimerLabel = null;
        [SerializeField]
        private GameObject getPatientLabel = null;

        [SerializeField]
        private ButtonUI button = null;

        [SerializeField]
        private TextMeshProUGUI bedInfo = null;
        [SerializeField]
        private TextMeshProUGUI waitingTimer = null;

        public void SetBedPanelInfoActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void SetWaitingView(UnityAction action, string buttonText, Sprite buttonIcon)
        {
            SetButton(action, buttonText, buttonIcon);

            SetBedInfoActive(false);

            SetButtonActive(true);
            SetWaitingObjectsActive(true);
        }

        public void SetInfoAndButtonView(string info, UnityAction action, string buttonText, Sprite buttonIcon)
        {
            SetInfo(info);
            SetButton(action, buttonText, buttonIcon);

            SetWaitingObjectsActive(false);

            SetButtonActive(true);
            SetBedInfoActive(true);
        }

        public void SetInfoView(string info)
        {
            SetInfo(info);

            SetButtonActive(false);
            SetWaitingObjectsActive(false);

            SetBedInfoActive(true);
        }

        public void SetWaitingTimer(string timerText)
        {
            waitingTimer.text = timerText;
        }

        private void SetButtonActive(bool setActive)
        {
            button.SetButtonActive(setActive);
        }

        private void SetBedInfoActive(bool setActive)
        {
            bedInfo.gameObject.SetActive(setActive);
        }

        private void SetWaitingObjectsActive(bool setActive)
        {
            waitingTimerLabel.SetActive(setActive);
            getPatientLabel.SetActive(setActive);
            waitingTimer.gameObject.SetActive(setActive);
        }

        public void SetButton(UnityAction action, string buttonText, Sprite buttonIcon, bool isBlinking = false)
        {
            button.SetButton(action, buttonText, buttonIcon, isBlinking);
        }

        private void SetInfo(string info)
        {
            bedInfo.text = info;
        }

    }
}
