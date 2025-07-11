using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class OfferUICard : MonoBehaviour
{
    public OfferCard model = null;

    public TextMeshProUGUI IAPName;
    public TextMeshProUGUI PriceAmount;
    public TextMeshProUGUI RewardAmount;
    public abstract void OnClick();
    public abstract void Initialize();

    [SerializeField]
    protected bool isIAPBundle = false;

    public virtual void RefreshLayout()
    {
    }

    public virtual void RefreshData()
    {
        SetIAPPrice();
    }

    protected virtual bool CanBuy()
    {
        return Game.Instance.gameState().GetDiamondAmount() >= model.costInDiamonds;
    }

    public void SetIAPName(string newname)
    {
        if (IAPName != null)
        {
            IAPName.text = newname;
        }
    }
    protected virtual void SetIAPPrice()
    {
        if (PriceAmount != null &&  model!=null)
        {
            PriceAmount.text = model.costInDiamonds.ToString();
        }
    }

    protected virtual void SetIAPRewardAmount(string rewardAmount)
    {
        if (RewardAmount != null)
        {
            RewardAmount.text = rewardAmount;
        }
    }
}
