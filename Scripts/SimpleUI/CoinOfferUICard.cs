using System;
using UnityEngine;

public class CoinOfferUICard : OfferUICard
{
    public IAPShopCoinPackageID ID;
    private string ProductID;
#pragma warning disable 0649
    [SerializeField]
    private GameObject diamondIcon;
#pragma warning restore 0649

    public override void Initialize()
    {
        if (model is CoinOfferCard)
        {
            CoinOfferCard modelAsCoin = model as CoinOfferCard;
            ProductID = modelAsCoin.IapProductId;
            if (String.IsNullOrEmpty(ProductID) && ID != IAPShopCoinPackageID.packOfCoinsForVideo)
            {
                diamondIcon.SetActive(true);
            }
        }
    }

    public override void OnClick()
    {
        if (ID != IAPShopCoinPackageID.packOfCoinsForVideo)
        {
            if (model is CoinOfferCard)
            {
                CoinOfferCard coinModel = model as CoinOfferCard;
                coinModel.Buy();
            }
        }
        else
        {
            if (AdsController.instance.IsAdAvailable(AdsController.AdType.rewarded_ad_coins))
            {
                StartCoroutine(UIController.get.BillboardAdPopUp.Open(true, false, () =>
                {
                    UIController.get.BillboardAdPopUp.ShowBeforeAdContentCoin();
                }));
            }
            else
            {
                StartCoroutine(UIController.get.BillboardAdPopUp.Open(true, false, () =>
                {
                    UIController.get.BillboardAdPopUp.ShowNoVideoContentCoin();
                }));
            }
        }
    }

    public override void RefreshData()
    {
        base.RefreshData();
        if (ID != IAPShopCoinPackageID.packOfCoinsForVideo)
        {
            SetIAPRewardAmount((model as CoinOfferCard).GetRewardAmount().ToString());
        }
    }

    protected override void SetIAPPrice()
    {
        if (ID != IAPShopCoinPackageID.packOfCoinsForVideo)
        {
            if (PriceAmount != null)
            {

                if (String.IsNullOrEmpty(ProductID))
                {

                    PriceAmount.text = model.costInDiamonds.ToString();
                }
                else
                {
                    PriceAmount.text = IAPController.instance.GetPrizeByProductID(ProductID) == null ? String.Empty : IAPController.instance.GetPrizeByProductID(ProductID);
                }
            }
        }
    }

}
