using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeCoinOfferCard : CoinOfferCard
{
    public string DecisionPoint;

    public CreativeCoinOfferCard(string IapProductId, string DecisionPoint, int costInDiamonds, IAPShopCoinPackageID ID, float multiplier, int offerOrder, int sectionOrder) : base(IapProductId, costInDiamonds, ID, multiplier, offerOrder, sectionOrder)
    {
        this.DecisionPoint = DecisionPoint;
    }

    public override bool IsCreative()
    {
        return true;
    }
}
