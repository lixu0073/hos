using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleUI;
using TMPro;

public class EasterPopUpContent : MonoBehaviour {

    public TextMeshProUGUI availableCount;
    public TextMeshProUGUI priceText;


    void OnEnable()
    {
        SetPrice();
        SetAvailableInfo();
		SoundsController.Instance.PlayBirds ();
    }

    void SetPrice()
    {
        priceText.text = IAPController.instance.GetEasterPrize();
    }

    public void SetAvailableInfo()
    {
        int availableAmount = 3 - GameState.Get().IAPEasterCount;
        if (availableAmount < 0)
            availableAmount = 0;

        availableCount.text = availableAmount.ToString();
    }

    public void ButtonIAP()
    {
        IAPController.instance.BuyProductID("easter_1");
    }
}
