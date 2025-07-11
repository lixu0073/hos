using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Maternity.UI
{
    public class MaternityEmptyMachinePanelUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject timerBadge = null;
        [SerializeField]
        private GameObject onThewayBadge = null;

        [SerializeField]
        private TextMeshProUGUI info = null;
        [SerializeField]
        private Image roomIcon = null;

        [SerializeField]
        private Sprite bedSprite = null;

        public void SetEmptyMachinePanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void SetRoomRequiredView(Sprite roomIcon)
        {
            SetRoomIcon(roomIcon);

            ClearPanel();
        }

        public void SetWaitingForPatientView(string infoText)
        {
            SetRoomIcon(bedSprite);
            SetInfo(infoText);

            ClearPanel();

            info.gameObject.SetActive(true);
            timerBadge.SetActive(true);
        }

        public void SetPatientCommingView(string infoText)
        {
            SetRoomIcon(bedSprite);
            SetInfo(infoText);

            ClearPanel();

            info.gameObject.SetActive(true);
            onThewayBadge.SetActive(true);
        }

        private void SetRoomIcon(Sprite roomImage)
        {
            roomIcon.sprite = roomImage;
        }

        private void SetInfo(string infoText)
        {
            info.text = infoText;
        }

        private void ClearPanel()
        {
            info.gameObject.SetActive(false);
            timerBadge.SetActive(false);
            onThewayBadge.SetActive(false);
        }

    }
}
