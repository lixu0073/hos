using UnityEngine;
using SimpleUI;
using TMPro;

public class BreastCancerUI : UIElement
{
    [SerializeField]
    private TextMeshProUGUI priceText = null;

    public void ButtonExit()
    {
        Exit();
        UIController.get.IAPShopUI.Open(IAPShopSection.sectionSpecialOffers);
    }

    public void ButtonInfo()
    {
        Application.OpenURL(DefaultConfigurationProvider.GetConfigCData().BreastCancerFoundationUrl);
    }

    public void ButtonBuy()
    {
        if (DeveloperParametersController.Instance().parameters.IapShopControllerIsTestBuild)
        {
            Game.Instance.gameState().AddDiamonds(275, EconomySource.TestModeIAP);
            Game.Instance.gameState().SetIAPBoughtLately(true);
        }
        else        
            IAPController.instance.BuyProductID("diamonds3_20off");
    }

    public void UpdatePrice(string price)
    {
        UIController.SetTMProUGUITextSecure(priceText, price);
    }
}
