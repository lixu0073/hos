using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondOfferUICard : OfferUICard
{

    public int index;

    public override void Initialize()
    {
    }

    public override void OnClick()
    {
        if (DeveloperParametersController.Instance().parameters.IapShopControllerIsTestBuild)
        {
            int[] amounts = IAPController.instance.GetDiamondPackageAmounts();
            Game.Instance.gameState().AddDiamonds(amounts[index], EconomySource.TestModeIAP);
            Game.Instance.gameState().SetIAPBoughtLately(true);
        }
        else
            IAPController.instance.BuyDiamondPackage(index);
    }

    public override void RefreshData()
    {
        base.RefreshData();
        int[] amounts = IAPController.instance.GetDiamondPackageAmounts();
        SetIAPRewardAmount(amounts[index].ToString());
    }

    protected override void SetIAPPrice()
    {
        if (PriceAmount != null)
        {
            PriceAmount.text = IAPController.instance.GetDiamondPriceByIndex(index) == null ? string.Empty : IAPController.instance.GetDiamondPriceByIndex(index);
        }
    }
}
