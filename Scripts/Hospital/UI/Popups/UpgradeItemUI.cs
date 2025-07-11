using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hospital
{
    public class UpgradeItemUI : MonoBehaviour
    {

        [SerializeField]
        protected Image image;

        [SerializeField]
        protected TextMeshProUGUI amountText;

        [SerializeField]
        protected TextMeshProUGUI priceText;

        [SerializeField]
        protected GameObject gotIt;

        [SerializeField]
        public Button buyMissingButton;

        public void SetImage(Sprite sprite)
        {
            image.sprite = sprite;
        }

        public void SetGotIt(bool haveResourcesView)
        {
            gotIt.SetActive(haveResourcesView);
            buyMissingButton.gameObject.SetActive(!haveResourcesView);
            amountText.color = haveResourcesView ? new Color(.157f, .165f, .165f) : Color.red;
        }

        public void SetPrice(int price)
        {
            priceText.SetText(price.ToString());
        }

        public void SetAmount(string amount)
        {
            amountText.SetText(amount);
        }

    }
}
