using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CoinOfferCard : OfferCard
{
    public IAPShopCoinPackageID ID;
    private float multiplier;
    private int offerOrder;
    private int sectionOrder;
    public string IapProductId;

    public CoinOfferCard(string IapProductId, int costInDiamonds, IAPShopCoinPackageID ID, float multiplier, int offerOrder, int sectionOrder) : base(costInDiamonds)
    {
        this.IapProductId = IapProductId;
        this.ID = ID;
        this.multiplier = multiplier;
        this.offerOrder = offerOrder;
        this.sectionOrder = sectionOrder;
        InitializeID();
    }

    public override void Buy()
    {
        Debug.LogError("Buy: " + ID.ToString());
        int amount = IAPController.instance.GetCoinAmount(multiplier);

        if (DeveloperParametersController.Instance().parameters.IapShopControllerIsTestBuild)
        {

            Game.Instance.gameState().AddCoins(amount, EconomySource.TestModeIAP);
            Game.Instance.gameState().SetIAPBoughtLately(true);
        }
        else
        {
            if (String.IsNullOrEmpty(IapProductId))
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                    {
                        int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                        Game.Instance.gameState().AddCoins(amount, EconomySource.IAPShopBundle, false);
                        Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.IAPShopBundle);
                        Game.Instance.gameState().SetIAPBoughtLately(true);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, new Vector2(0, -130), amount, 0f, 2f, new Vector3(1.2f, 1.2f, 1), new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[0], null, () =>
                         {
                             Game.Instance.gameState().UpdateCounter(ResourceType.Coin, amount, currentCoinAmount);
                         });
                        AnalyticsController.instance.ReportBuyShopOffer(GetAnalyticData());
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                    MessageController.instance.ShowMessage(1);
                    AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.missing_resources_closed, 1f);
                }
            }
            else
            {
                IAPController.instance.BuyProductID(IapProductId);
            }
        }
    }

    public int GetRewardAmount()
    {
        return IAPController.instance.GetCoinAmount(multiplier);
    }
    protected override BaseAnalyticParams GetAnalyticData()
    {
        return new ShopCoinsOfferAnalyticParams(offerOrder, sectionOrder, costInDiamonds, IAPShopSection.sectionCoins, ID, multiplier);
    }
}
