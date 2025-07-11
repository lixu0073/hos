using System.Collections.Generic;
using UnityEngine;
using Hospital;

public class MultipleRewardedBundleOfferUICard : BundleOfferUICard
{
    [SerializeField] private List<BaseRewardData> rewardsData = new List<BaseRewardData>();
#pragma warning disable 0414
    [SerializeField] private bool IsDisplay = false;
#pragma warning restore 0414


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
        if (ID != IAPShopBundleID.bundleBreastCancerDeal && ID != IAPShopBundleID.bundleTapjoy)
            StartCoroutine(UIController.get.bundlePurchaseConfirmationPopup.Open(ID, model.costInDiamonds, Collect));
    }

    public override void Initialize()
    {
        base.Initialize();
        if (model is BundleOfferCard)
        {
            BundleOfferCard bundleModel = model as BundleOfferCard;
            foreach (BaseRewardData rewardData in rewardsData)
            {
                bundleModel.FillMultipleRewards(rewardData.GetReward());
            }
        }
        RefreshData();
    }

    public override void RefreshData()
    {
        base.RefreshData();
    }

    protected override void Collect()
    {
        if (model is BundleOfferCard)
        {
            BundleOfferCard bundleModel = model as BundleOfferCard;
            if (DeveloperParametersController.Instance().parameters.IapShopControllerIsTestBuild)
            {
                float duration = 0.3f;
                foreach (BubbleBoyReward rewardItem in bundleModel.GetRewardsFromBundle())
                {
                    rewardItem.Collect(duration);
                    duration += 0.8f;
                }
                Game.Instance.gameState().SetIAPBoughtLately(true);
            }
            else
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= model.costInDiamonds)
                {
                    Game.Instance.gameState().RemoveDiamonds(model.costInDiamonds, EconomySource.IAPShopBundle);
                    float duration = 0.3f;
                    foreach (BubbleBoyReward rewardItem in bundleModel.GetRewardsFromBundle())
                    {
                        rewardItem.Collect(duration);
                        duration += 0.8f;
                    }
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
}
