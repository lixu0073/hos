using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleUI
{
    public class SimpleProgressBarController : MonoBehaviour {
        [SerializeField]
        private Slider progressBar = null;
        [SerializeField]
        private TextMeshProUGUI progressText = null;

        public void SetProgressBar(int progress, int goal) {
            if (goal <= 0) {
                Debug.LogError("invalid goal");
                return;
            }

            progress = Mathf.Clamp(progress, 0, goal);

            float currentProgress = ((float)progress / (float)goal);
            string progTxt = progress + " / " + goal;

            SetProgressBarValue(currentProgress);
            SetProgressBarText(progTxt);
        }

        private void SetProgressBarValue(float value) {
            if (progressBar == null) {
                Debug.LogError("progressBar is null");
                return;
            }

            progressBar.value = value;
        }

        private void SetProgressBarText(string text) {
            if (progressText == null)
            {
                Debug.LogError("progressText is null");
                return;
            }

            if (string.IsNullOrEmpty(text)) {
                progressText.gameObject.SetActive(false);
                return;
            }

            progressText.gameObject.SetActive(true);
            progressText.text = text;
        }
    }
}