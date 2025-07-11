using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PharmacyBuyMoreOffers : MonoBehaviour {

    public Button button;
    public TextMeshProUGUI priceText;


    public void SetPrice(int price)
    {
        priceText.text = price.ToString();
    }
}
