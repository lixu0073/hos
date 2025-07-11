using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BundleOfferUICard : OfferUICard, IDiamondTransactionMaker
{
    public IAPShopBundleID ID;

    [SerializeField]
    protected int amount;

    [SerializeField]
    protected string ProductID;

    protected BubbleBoyReward reward = null;

    private Guid DiamondTransactionMakerID;

    public override void Initialize()
    {
        SetupBackgroundColor();
    }

    protected abstract void Collect();

    public override void OnClick()
    {
        if (isIAPBundle)
        {
            IAPController.instance.BuyProductID(ProductID);
        }
        else
        {
            Buy();
        }
    }

    public override void RefreshLayout()
    {
        base.RefreshLayout();
        gameObject.SetActive(HasRequiredLevel());
    }

    private void SetupBackgroundColor()
    {
        if (ID != IAPShopBundleID.bundleBreastCancerDeal && ID != IAPShopBundleID.bundleTapjoy)
        {
            if (model is BundleOfferCard)
            {
                BundleOfferCard modelasBundleOfferCard = model as BundleOfferCard;
                switch (modelasBundleOfferCard.Color)
                {
                    case IAPShopBundleColor.none:
                        break;
                    case IAPShopBundleColor.yellow:
                        gameObject.GetComponent<Image>().sprite = ResourcesHolder.Get().yellowBackground;
                        break;
                    case IAPShopBundleColor.green:
                        gameObject.GetComponent<Image>().sprite = ResourcesHolder.Get().greenBackground;
                        break;
                    case IAPShopBundleColor.red:
                        gameObject.GetComponent<Image>().sprite = ResourcesHolder.Get().redBackground;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void Buy()
    {
        if (!CanBuy())
        {
            Debug.LogError("No diamonds!");
            return;
        }
        else
        {
            DiamondTransactionController.Instance.AddDiamondTransaction(model.costInDiamonds, delegate
            {
                model.Buy();
                Collect();
            }, this);
        }
    }

    public abstract bool HasRequiredLevel();

    public void InitializeID()
    {
        DiamondTransactionMakerID = Guid.NewGuid();
    }

    public Guid GetID()
    {
        return DiamondTransactionMakerID;
    }

    public void EraseID()
    {
        DiamondTransactionMakerID = Guid.Empty;
    }
}
