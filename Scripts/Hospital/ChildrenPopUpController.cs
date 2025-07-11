using UnityEngine;
using System.Collections;
using SimpleUI;
using TMPro;
using System;

namespace Hospital
{
    public class ChildrenPopUpController : UIElement
    {
        public TextMeshProUGUI positiveEnergyAmountText;
        public RectTransform defaultInfo;
        public RectTransform lowLevelInfo;


        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers);
            SetLayout();
            whenDone?.Invoke();
        }

        void SetLayout()
        {
            GameState gs = GameState.Get();
            if (gs.hospitalLevel >= 14)
            {
                defaultInfo.anchoredPosition = new Vector2(0, -50f);
                lowLevelInfo.gameObject.SetActive(false);
            }
            else
            {
                defaultInfo.anchoredPosition = new Vector2(0, 0);
                lowLevelInfo.gameObject.SetActive(true);
            }

            positiveEnergyAmountText.text = gs.PositiveEnergyAmount.ToString();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            NotificationCenter.Instance.KidsUIClosed.Invoke(new KidsUIClosedEventArgs());
            base.Exit(hidePopupWithShowMainUI);
        }

        public void ButtonExit()
        {
            Exit();
        }

    }
}
