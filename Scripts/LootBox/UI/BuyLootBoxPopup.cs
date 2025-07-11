using SimpleUI;
using System.Collections;
using System;
using TMPro;

namespace Hospital
{
    namespace LootBox
    {
        public class BuyLootBoxPopup : UIElement
        {

            public TextMeshProUGUI priceText;

            private LootBoxData boxData;

            public IEnumerator Open(LootBoxData boxData)
            {
                this.boxData = boxData;
                yield return Open();
            }

            public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
            {
                yield return base.Open();
                SetPrice();

                // AnalyticsController.instance.ReportEventOpened(currentEvent);
                whenDone?.Invoke();
            }

            void SetPrice()
            {
                priceText.text = IAPController.instance.GetPriceForLootBox(boxData.iap);
            }

            public void ButtonIAP()
            {
                IAPController.instance.BuyProductID(boxData.iap);
                Exit();
            }

            public void ButtonExit()
            {
                Exit();
            }

        }

    }
}