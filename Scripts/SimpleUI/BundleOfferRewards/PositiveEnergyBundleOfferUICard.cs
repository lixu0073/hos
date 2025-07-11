public class PositiveEnergyBundleOfferUICard : BundleOfferUICard
{
    public override bool HasRequiredLevel()
    {
        if (model is BundleOfferCard)
        {
            BundleOfferCard bundleModel = model as BundleOfferCard;
            return bundleModel.IsAccesibleToPlayer();
        }
        return false;
    }

    public override void OnClick()
    {
        StartCoroutine(UIController.get.bundlePurchaseConfirmationPopup.Open(ID, model.costInDiamonds, Collect));
    }

    public override void RefreshData()
    {
        base.RefreshData();
    }

    public override void Initialize()
    {
        base.Initialize();
        reward = new SuperBundleRewardPositiveEnergy(amount);
    }

    protected override void Collect()
    {
        if (DeveloperParametersController.Instance().parameters.IapShopControllerIsTestBuild)
        {
            reward.Collect();
            Game.Instance.gameState().SetIAPBoughtLately(true);
        }
        else
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= model.costInDiamonds)
            {
                Game.Instance.gameState().RemoveDiamonds(model.costInDiamonds, EconomySource.IAPShopBundle);
                reward.Collect();
                Game.Instance.gameState().SetIAPBoughtLately(true);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                MessageController.instance.ShowMessage(1);
                AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.missing_resources_closed, 1f);
            }
        }
    }
}
