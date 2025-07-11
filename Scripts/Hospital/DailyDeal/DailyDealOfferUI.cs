using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Hospital
{
    public class DailyDealOfferUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI discountText = null;
        [SerializeField]
        private TextMeshProUGUI amountText = null;
        [SerializeField]
        private TextMeshProUGUI oldPriceText = null;
        [SerializeField]
        private TextMeshProUGUI newPriceText = null;

        [SerializeField]
        private Image itemImage = null;

        public void SetDailyDealOfferUI(DailyDeal dailyDeal) {
            int discountPercent = Mathf.RoundToInt(100 * (1 - dailyDeal.Discount));
            string tempString = I2.Loc.ScriptLocalization.Get("DISCOUNT_OFF");
            discountText.SetText(tempString.Replace("{0}", discountPercent.ToString()));
            amountText.SetText(dailyDeal.Amount.ToString() + "x");
            int oldPrice = ResourcesHolder.Get().GetDiamondPriceForCure(dailyDeal.Item) * dailyDeal.Amount;
            oldPriceText.SetText(oldPrice.ToString());
            int newPrice = Mathf.RoundToInt(oldPrice * dailyDeal.Discount);
            newPriceText.SetText(newPrice.ToString());
            itemImage.sprite = ResourcesHolder.Get().GetSpriteForCure(dailyDeal.Item);
        }

        public void BuyDailyDealOffer() {
            ReferenceHolder.GetHospital().dailyDealController.OpenDailyDealConfirmationPopup();
        }
    }
}
