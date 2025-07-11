using UnityEngine;
using UnityEngine.UI;

public class CreativeBundleOfferUICard : BundleOfferUICard
{
    private string decisionPoint;
#pragma warning disable 0649
    [SerializeField] private Image creativeImage;
#pragma warning restore 0649

    public override bool HasRequiredLevel()
    {
        if (creativeImage == null || creativeImage.sprite == null)
            return false;

        if (model is CreativeOfferCard)
        {
            CreativeOfferCard bundleModel = model as CreativeOfferCard;
            return bundleModel.IsAccesibleToPlayer();
        }
        return true;
    }

    public override void OnClick()
    {
        model.Buy();
    }

    protected override void Collect() { }

    public override void Initialize()
    {
        if (model is CreativeOfferCard)
        {
            CreativeOfferCard bundleModel = model as CreativeOfferCard;
            decisionPoint = bundleModel.DecisionPoint;
            ProductID = bundleModel.IapProductId;
        }
        else if (model is CreativeCoinOfferCard)
        {
            CreativeCoinOfferCard coinCreative = model as CreativeCoinOfferCard;
            decisionPoint = coinCreative.DecisionPoint;
            ProductID = coinCreative.IapProductId;
        }
        PingDeecisionPoint();
    }

    protected override void SetIAPPrice()
    {
        if (PriceAmount != null)
            PriceAmount.text = IAPController.instance.GetPrizeByProductID(ProductID) == null ? string.Empty : IAPController.instance.GetPrizeByProductID(ProductID);
    }

    private void PingDeecisionPoint()
    {
        Debug.LogError("Bundle offers were in DDNA");
        //if (creativeImage != null && creativeImage.sprite == null)
        //{
        //    DecisionPointCalss.RequestSprite(decisionPoint, (sprite) =>
        //    {
        //        if (creativeImage != null && sprite != null)
        //            creativeImage.sprite = sprite;
        //        else
        //            gameObject.SetActive(false);
        //    });
        //}
    }

}