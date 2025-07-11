using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OfferCard: IDiamondTransactionMaker
{
    public bool IsIAPOffer = false;
    public int costInDiamonds = 0;
    private Guid DiamondTransactionMakerID;

    public OfferCard(int costInDiamonds)
    {
        this.costInDiamonds = costInDiamonds;
    }
    public virtual void Buy()
    {
    }

    public void EraseID()
    {
        DiamondTransactionMakerID = Guid.Empty;
    }

    public Guid GetID()
    {
        return DiamondTransactionMakerID;
    }

    public void InitializeID()
    {
        DiamondTransactionMakerID = Guid.NewGuid();
    }

    public virtual bool IsCreative()
    {
        return false;
    }

    protected abstract BaseAnalyticParams GetAnalyticData();

}
