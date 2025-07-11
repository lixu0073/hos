using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;

public class PharmacyGlobalOfferAddSlot : MonoBehaviour {

    public Button button;
    public TextMeshProUGUI priceText;

    public void SetPrice(int price)
    {
        priceText.text = price.ToString();
    }

}
