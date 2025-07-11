using UnityEngine;
using SimpleUI;
using TMPro;
using System.Collections;
using System;

namespace Hospital
{
    public class CreditsPopup : UIElement
    {
#pragma warning disable 0649
        [SerializeField] TextMeshProUGUI version;
        [SerializeField] TextMeshProUGUI copyright;
#pragma warning restore 0649

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open();
            version.SetText("MY HOSPITAL™  v." + Application.version + GetCreditsVersionEnvironment());
            copyright.SetText("All rights reserved © {0}\nKuuhubb Oy", System.DateTime.Now.Year);

            whenDone?.Invoke();
        }

        public void ButtonBack()
        {
            Exit(false);
            StartCoroutine(UIController.get.SettingsPopUp.Open());
        }

        public void ButtonExit()
        {
            Exit(true);
        }
        
        private string GetCreditsVersionEnvironment()
        {
#if MH_DEVELOP
            return " DEV";
#elif MH_QA
            return " QA";
#else
            return String.Empty;
#endif
        }

    }
} // namespace Hospital
