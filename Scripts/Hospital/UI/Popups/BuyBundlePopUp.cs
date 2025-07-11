using UnityEngine;
using System.Collections.Generic;
using TMPro;
using SimpleUI;
using System;

namespace Hospital
{
    public class BuyBundlePopUp : UIElement, IDiamondTransactionMaker
    {
        private RectTransform content = null;
        [SerializeField]
        private TextMeshProUGUI bundleName = null;
        [SerializeField]
        private TextMeshProUGUI cost = null;
        [SerializeField]
        private CanvasGroup canvasGroup = null;
        [SerializeField]
        private List<TextMeshProUGUI> RewardList;

        [SerializeField]
        private List<GameObject> superBundleList = null;

        private int diamondCost;
        private OnEvent onBought = null;
        private OnEvent onResigned = null;
        private bool offerUsed = false;

        public void Open(SuperBundlePackage package, OnEvent onBought, OnEvent onResigned)
        {
            Debug.Log("BuyBundlePopUp Open.");
            // AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingResources;

            if (UIController.getHospital.PatientCard.isActiveAndEnabled || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled)
                StartCoroutine(base.Open(false, false, OnPostOpen(package, onBought, onResigned)));
            else
                StartCoroutine(base.Open(true, true, OnPostOpen(package, onBought, onResigned)));
        }

        private Action OnPostOpen(SuperBundlePackage package, OnEvent onBought, OnEvent onResigned)
        {
            Initialize(package, onBought, onResigned);
            canvasGroup.interactable = true;
            return null;
        }

        void Initialize(SuperBundlePackage package, OnEvent onBought, OnEvent onResigned)
        {
            this.onBought = onBought;
            this.onResigned = onResigned;
            offerUsed = false;
            diamondCost = package.GetDiamondsPrice();
            cost.text = diamondCost.ToString();

            for (int i = 0; i < superBundleList.Count; i++)
                superBundleList[i].SetActive(false);

            string packageKey = package.GetKey();

            switch (packageKey)
            {
                case "super_bundle_1":
                    superBundleList[0].SetActive(true);
                    bundleName.text = I2.Loc.ScriptLocalization.Get("MARKETING/SPECIAL_PACK");
                    break;
                case "super_bundle_2":
                    superBundleList[1].SetActive(true);
                    bundleName.text = I2.Loc.ScriptLocalization.Get("MARKETING/UNIQUE_PACK");
                    break;
                default:
                    break;
            }
        }

        public void BuyBundle()
        {
            if (!gameObject.activeSelf)
                return;
            if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondCost, delegate
                {
                    offerUsed = true;
                    Exit();
                }, this);
            }
            else
            {
                Exit();
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                MessageController.instance.ShowMessage(1);
            }
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            if (UIController.get.IAPShopUI.isActiveAndEnabled)
                UIController.get.IAPShopUI.transform.SetAsLastSibling();
            base.Exit(hidePopupWithShowMainUI);

            if (!offerUsed)
            {
                AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.missing_resources_closed, 1f);

                onResigned?.Invoke();
            }
            else if (offerUsed)
            {
                onBought?.Invoke();
            }

            if (content != null && content.childCount > 0)
            {
                for (int i = 0; i < content.childCount; i++)
                    Destroy(content.GetChild(i).gameObject);
            }

            //Debug.LogError("Exit missing resources");
            canvasGroup.interactable = false;
        }

        public void ButtonExit()
        {
            Exit();
        }
    }
}