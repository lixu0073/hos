using UnityEngine;
using SimpleUI;
using System.Collections;
using System;

namespace Hospital
{
    public class WarningInfoPopup : UIElement
    {
        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open();
        }

        public void ButtonOK()
        {
            StartCoroutine(UIController.get.SettingsPopUp.Open());
            Exit(true);
        }

        public void ButtonExit()
        {
            Exit(true);
        }
    }
}
