using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleUI;
using TMPro;

public class ValentinePopUpContent : MonoBehaviour
{

    public TextMeshProUGUI availableInfo;
    public TextMeshProUGUI priceText;


    void OnEnable()
    {
        SetPrice();
        SetAvailableInfo();
    }

    void SetPrice()
    {
        priceText.text = IAPController.instance.GetValentinePrice();
    }

    public void SetAvailableInfo()
    {
        int availableAmount = 3 - GameState.Get().IAPValentineCount;
        if (availableAmount < 0)
            availableAmount = 0;

        availableInfo.text = I2.Loc.ScriptLocalization.Get("EVENTS/AVAILABLE") + " " + availableAmount;
    }
    
    public void ButtonIAP()
    {
        IAPController.instance.BuyProductID("valentines_1");
    }
}