using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinPurchaseConfirmationUI : UIElement
{
    public delegate void OnBought(Vector2 particleSpawnPosition);
    public TextMeshProUGUI PackageName;
    public TextMeshProUGUI diamondCostText;
    public TextMeshProUGUI rewardText;
    public List<GameObject> contentSetup;
    private GameObject currentSetup;
    private OnEvent handler;
    private bool offerUsed = false;
    [SerializeField]

    public IEnumerator Open(IAPShopCoinPackageID coinPackage, int reward, int diamondCost, OnEvent onBought)
    {
        yield return base.Open();
        handler = onBought;
        offerUsed = false;
        diamondCostText.text = diamondCost.ToString();
        rewardText.text = reward.ToString();
        SetupPopupContent(coinPackage);
    }

    private void SetupPopupContent(IAPShopCoinPackageID coinPackage)
    {
        switch (coinPackage)
        {
            case IAPShopCoinPackageID.packOfCoinsForVideo:
                break;
            case IAPShopCoinPackageID.packOfCoins1:
                if (contentSetup[0] != null)
                {
                    contentSetup[0].SetActive(true);
                    currentSetup = contentSetup[0];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("ADD_COINS_TITLE_2");
                }
                break;
            case IAPShopCoinPackageID.packOfCoins2:
                if (contentSetup[1] != null)
                {
                    contentSetup[1].SetActive(true);
                    currentSetup = contentSetup[1];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("ADD_COINS_TITLE_3");
                }
                break;
            case IAPShopCoinPackageID.packOfCoins3:
                if (contentSetup[2] != null)
                {
                    contentSetup[2].SetActive(true);
                    currentSetup = contentSetup[2];
                    PackageName.text = I2.Loc.ScriptLocalization.Get("ADD_COINS_TITLE_6");
                }
                break;
            default:
                break;
        }
        if (currentSetup != null)
        {
            if (UIController.getHospital != null && (UIController.getHospital.PatientCard.isActiveAndEnabled || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled))
                StartCoroutine(base.Open(false, false));
            else
                StartCoroutine(base.Open(true, true));
        }
    }

    public void BuyBundle()
    {
        if (!gameObject.activeSelf)
            return;
        offerUsed = true;
        handler?.Invoke();
        Exit();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        if (currentSetup != null)
        {
            currentSetup.SetActive(false);
            currentSetup = null;
        }
        if (UIController.get.IAPShopUI.isActiveAndEnabled)
            UIController.get.IAPShopUI.transform.SetAsLastSibling();
        base.Exit(hidePopupWithShowMainUI);
        if (!offerUsed)
        {
            AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.missing_resources_closed, 1f);
        }
    }

    public void ButtonExit()
    {
        Exit();
    }
}
