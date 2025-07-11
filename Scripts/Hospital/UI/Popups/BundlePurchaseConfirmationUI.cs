using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BundlePurchaseConfirmationUI : UIElement
{
    public TextMeshProUGUI PackageName;
    public TextMeshProUGUI diamondCostText;
    public List<GameObject> contentSetup;
    private int diamondCost;
    private GameObject currentSetup;
    private OnEvent handler;
    private bool offerUsed = false;
    [SerializeField]

    public IEnumerator Open(IAPShopBundleID bundleID, int diamondCost, OnEvent onBought)
    {
        handler = onBought;
        offerUsed = false;
        if (UIController.getHospital != null && (UIController.getHospital.PatientCard.isActiveAndEnabled || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled))
            yield return base.Open(false, false);
        else
            yield return base.Open(true, true);
        this.diamondCost = diamondCost;
        diamondCostText.text = diamondCost.ToString();
        SetupPopupContent(bundleID);

        SoundsController.Instance.PlayButtonClick(false);
    }

    private void SetupPopupContent(IAPShopBundleID bundleID)
    {
        switch (bundleID)
        {
            case IAPShopBundleID.bundleBreastCancerDeal:
                break;
            case IAPShopBundleID.bundlePositiveEnergy50:
                if (contentSetup[0] != null)
                {
                    contentSetup[0].SetActive(true);
                    currentSetup = contentSetup[0];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/POSITIVE_ENERGY");
                }
                break;
            case IAPShopBundleID.bundleShovels9:
                if (contentSetup[1] != null)
                {
                    contentSetup[1].SetActive(true);
                    currentSetup = contentSetup[1];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SHOVEL");
                }
                break;
            case IAPShopBundleID.bundleSpecialPack:
                if (contentSetup[2] != null)
                {
                    contentSetup[2].SetActive(true);
                    currentSetup = contentSetup[2];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("ADD_CURRENCY_TITLE");
                }
                break;
            case IAPShopBundleID.bundleSuperBundle4:
                if (contentSetup[3] != null)
                {
                    contentSetup[3].SetActive(true);
                    currentSetup = contentSetup[3];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("ADD_CURRENCY_TITLE");
                }
                break;
            case IAPShopBundleID.bundleTapjoy:
                break;
            default:
                break;
        }
    }

    public void BuyBundle()
    {
        if (!gameObject.activeSelf)
            return;
        offerUsed = true;
        handler?.Invoke();
        Exit();
        //else
        //{
        //    Exit();
        //    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
        //    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
        //    MessageController.instance.ShowMessage(1);
        //}
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        if (UIController.get.IAPShopUI.isActiveAndEnabled)
            UIController.get.IAPShopUI.transform.SetAsLastSibling();
        base.Exit(hidePopupWithShowMainUI);

        if (!offerUsed)
        {
            AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.missing_resources_closed, 1f);
        }
        currentSetup.SetActive(false);
        currentSetup = null;
    }

    public void ButtonExit()
    {
        Exit();
    }
}
