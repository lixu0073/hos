using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SuperBundleUICard : MonoBehaviour
{

    SuperBundlePackage package;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public GameObject diamondIcon;


    public void Init(SuperBundlePackage package)
    {
        this.package = package;
        SetCard();
        //gameObject.SetActive(true);
    }

    void SetCard()
    {
        //nameText.text = package.GetPackageName();
        string priceString = "";
        if (package.IsForDiamonds())
        {
            priceString = package.GetDiamondsPrice().ToString();
            diamondIcon.SetActive(true);
        }
        else
        {
            priceString = IAPController.instance.GetPriceForProduct(package.GetProductId());
            diamondIcon.SetActive(false);
        }
        priceText.text = priceString.ToString();
    }

    public void ButtonBuy()
    {
        Debug.Log("Clicked to buy bundle");

        if (package.IsForDiamonds())
        {
            UIController.get.BuyBundlePopUp.Open(package,() =>
            {
               // if (Game.Instance.gameState().GetDiamondAmount() >= package.GetDiamondsPrice())
               // {
                GameState.Get().RemoveDiamonds(package.GetDiamondsPrice(), EconomySource.SuperBundle);
                package.Collect();
                ReferenceHolder.Get().giftSystem.CreateItemUsed(Input.mousePosition, package.GetDiamondsPrice(), 0f, ReferenceHolder.Get().giftSystem.particleSprites[1], false);

                // }
                // else UIController.get.AddCurrencyPopUp.Open(1);

            }, null);
        }
        else IAPController.instance.BuyProductID(package.GetProductId());

        /*
        if (package.IsForDiamonds())
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= package.GetDiamondsPrice())
            {
                GameState.Get().RemoveDiamonds(package.GetDiamondsPrice(), EconomySource.SuperBundle);
                package.Collect();
            }
            else
            {
                UIController.get.AddCurrencyPopUp.Open(1);
            }
        }
        else
        {
            IAPController.instance.BuyProductID(package.GetProductId());
        }
        */
    }

}
