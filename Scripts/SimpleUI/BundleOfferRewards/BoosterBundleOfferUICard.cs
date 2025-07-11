using UnityEngine;

public class BoosterBundleOfferUICard : BundleOfferUICard
{
#pragma warning disable 0649
    [SerializeField]
    private int boosterID;
#pragma warning restore 0649

    public override bool HasRequiredLevel()
    {
        if (model is BundleOfferCard)
        {
            BundleOfferCard bundleModel = model as BundleOfferCard;
            return bundleModel.IsAccesibleToPlayer();
        }
        return false;
    }

    protected override void Collect()
    {
        reward.Collect();
    }

    public override void Initialize()
    {
        base.Initialize();
        reward = new SuperBundleRewardBooster(boosterID, amount);
    }
}
