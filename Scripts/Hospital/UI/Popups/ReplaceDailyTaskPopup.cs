using UnityEngine;
using TMPro;
using SimpleUI;
using System.Collections;

namespace Hospital
{
    public class ReplaceDailyTaskPopup : UIElement
    {
        private DailyTask currentDailyTask;
        [SerializeField] private TextMeshProUGUI costText = null;
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] private TextMeshProUGUI taskInfo = null;
#pragma warning disable 0649
        [SerializeField] private GameObject videoButtonContent;
        [SerializeField] private GameObject costButtonContent;
#pragma warning restore 0649
        private int diamondCost;
        private OnEvent onBought = null;
        private OnEvent onResigned = null;
        private bool offerUsed = false;
        private bool isHidden = false;
        private bool isAdAvailable = false;

        public IEnumerator Open(DailyTask currentDailyTask, OnEvent onBought, OnEvent onResigned)
        {
            this.onBought = onBought;
            this.onResigned = onResigned;
            offerUsed = false;
            diamondCost = ReferenceHolder.GetHospital().dailyQuestController.GetReplacementCost();

            isAdAvailable = false;
            videoButtonContent.gameObject.SetActive(false);
            costButtonContent.gameObject.SetActive(false);

            yield return null; //CV: to force 1-frame pause before render the TMPro

            CheckAdsAvailability();

            costText.text = diamondCost.ToString();

            Debug.Log("ReplaceDailyTaskPopup Open.");
            
            yield return base.Open(false, !UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled);

            this.currentDailyTask = currentDailyTask;
            this.taskInfo.text = currentDailyTask.GetInfo();

            UnHide();

            if (diamondCost > Game.Instance.gameState().GetDiamondAmount())
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingResources.ToString(), (int)FunnelStepIAPMissingResources.MissingResourcesPopUp, FunnelStepIAPMissingResources.MissingResourcesPopUp.ToString());

            canvasGroup.interactable = true;
        }

        public void BuyTaskReplacement()
        {
            if (!gameObject.activeSelf)
                return;

            if (diamondCost == 1 && isAdAvailable)
            {
                AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_dailyquest);
            }
            else if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondCost, delegate
                {
                    GameState.Get().RemoveDiamonds(diamondCost, EconomySource.DailyQuestTaskReplace);
                    offerUsed = true;
                    Exit();
                }, this);
            }
            else
            {
                Hide();
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds, () =>
                {
                    UnHide();
                });
            }
        }

        public void ReplaceVideoAdReward()
        {
            offerUsed = true;
            Exit();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            CancelInvoke("CheckAdsAvailability");
            isAdAvailable = false;

            if (UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled)
                UIController.getHospital.DailyQuestPopUpUI.transform.SetAsLastSibling();
            base.Exit(hidePopupWithShowMainUI);

            if (!offerUsed)
            {
                onResigned?.Invoke();
            }
            else if (offerUsed)
            {
                onBought?.Invoke();
            }

            canvasGroup.interactable = false;
        }

        void Hide()
        {
            isHidden = true;
            canvasGroup.alpha = 0;
        }

        public void UnHide()
        {
            isHidden = false;
            canvasGroup.alpha = 1;
            transform.SetAsLastSibling();
        }

        public bool IsHidden()
        {
            return isHidden;
        }

        public void ButtonExit()
        {
            Exit();
        }

        void CheckAdsAvailability()
        {
            CancelInvoke("CheckAdsAvailability");

            if (diamondCost == 1)
            {
                isAdAvailable = AdsController.instance.IsAdAvailable(AdsController.AdType.rewarded_ad_dailyquest);

                if (!isAdAvailable)
                {
                    videoButtonContent.gameObject.SetActive(false);
                    costButtonContent.gameObject.SetActive(true);

                    Invoke("CheckAdsAvailability", 2f);
                }
                else
                {
                    videoButtonContent.gameObject.SetActive(true);
                    costButtonContent.gameObject.SetActive(false);
                }
            }
            else
            {
                videoButtonContent.gameObject.SetActive(false);
                costButtonContent.gameObject.SetActive(true);
            }
        }
    }
}