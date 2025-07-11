using Hospital;
using SimpleUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MovementEffects;

public class VitaminsMakerRefillmentPopup : UIElement
{
#pragma warning disable 0649
    [SerializeField] private Button FullfillButton;
    [SerializeField] private Button WatchAdButton;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Transform WatchAdButtonVideoContent;
    [SerializeField] private Transform WatchAdButtonDiamondsContentContent;
    [SerializeField] private TextMeshProUGUI oneVitamineCostText;
    [SerializeField] private Image WatchAdMedicineImage;
    [SerializeField] private Image FullfillMedicineImage;
#pragma warning restore 0649
    private InputData data;
    private int singleMedDiamondCost;
    private IEnumerator<float> coroutine;
    private AdButtonState adButtonState = AdButtonState.None;

    private enum AdButtonState
    {
        AdActive,
        OnCooldown,
        ForDimonds,
        None
    }

    public void Open(InputData data)
    {
        gameObject.SetActive(true);
        StartCoroutine(Open(false, false, () =>
        {
            this.data = data;
            Sprite medSprite = ResourcesHolder.Get().GetSpriteForCure(data.collectorModel.med);
            WatchAdMedicineImage.sprite = medSprite;
            FullfillMedicineImage.sprite = medSprite;
            singleMedDiamondCost = ResourcesHolder.Get().GetMedicineInfos(data.collectorModel.med).diamondPrice;
            SetDiamondCost();
            oneVitamineCostText.SetText(singleMedDiamondCost.ToString());
            this.data.collectorModel.capacityChanged += CollectorModel_capacityChanged;
            KillCoroutine();
            coroutine = Timing.RunCoroutine(CheckingAdStatus());
            TutorialUIController.Instance.StopBlinking();
        }));
    }

    IEnumerator<float> CheckingAdStatus()
    {
        while (true)
        {
            UpdateAdState();
            yield return Timing.WaitForSeconds(1);
        }
    }

    private void CollectorModel_capacityChanged(float fill, float current, int max, int producedAmount, MedicineRef vitamin, int timeToDrop, VitaminCollectorModel.VitaminSource source)
    {
        if (producedAmount > 0)
            SetDiamondCost();
        if (current + 1 >= max)
            OnExitButtonClick();
    }

    private void SetDiamondCost()
    {
        costText.SetText(GetSumCostInDiamonds().ToString());
    }

    private int GetSumCostInDiamonds()
    {
        return singleMedDiamondCost * data.collectorModel.GetAmountToFillToMaxCapacity();
    }

    public void OnFullfillButtonClick()
    {
        int amountToAdd = data.collectorModel.GetAmountToFillToMaxCapacity();
        if (amountToAdd > 1)
        {
            int sumCostInDiamonds = GetSumCostInDiamonds();
            if (Game.Instance.gameState().GetDiamondAmount() >= sumCostInDiamonds)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(sumCostInDiamonds, delegate
                {
                    data.collectorModel.FillToMaxCapacity();
                    Game.Instance.gameState().RemoveDiamonds(sumCostInDiamonds, EconomySource.FullfillVitaminCollector);
                    OnExitButtonClick();
                }, this);
            }
            else
            {
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }
    }

    public void RewardPlayerForWatchingAd()
    {
        if (data != null && data.collectorModel != null)
        {
            int reward = AdsController.instance.GetRewardAmount(AdsController.AdType.rewarded_ad_vitamin_collector);
            data.collectorModel.Fill(reward < 1 ? 1 : reward, VitaminCollectorModel.VitaminSource.Advertisement);
        }
    }

    public void OnWatchAdButtonClick()
    {
        UpdateAdState();
        switch (adButtonState)
        {
            case AdButtonState.AdActive:
                AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_vitamin_collector);
                OnExitButtonClick();
                break;
            case AdButtonState.OnCooldown:
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("NEXT_VIDEO_IN") + " " + UIController.GetFormattedTime((int)AdsController.instance.GetSecondsToNextAd(AdsController.AdType.rewarded_ad_vitamin_collector)));
                break;
            case AdButtonState.ForDimonds:
                if (Game.Instance.gameState().GetDiamondAmount() >= singleMedDiamondCost)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(singleMedDiamondCost, delegate
                    {
                        data.collectorModel.Fill(1, VitaminCollectorModel.VitaminSource.Advertisement);
                        Game.Instance.gameState().RemoveDiamonds(singleMedDiamondCost, EconomySource.PlusOneVitamineCollector);
                        OnExitButtonClick();
                    }, this);
                }
                else
                {
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
                break;
        }
    }


    private void UpdateAdState()
    {
        bool isAdEnable = AdsController.instance.IsAdAvailable(AdsController.AdType.rewarded_ad_vitamin_collector);
        if (isAdEnable)
        {
            if (adButtonState != AdButtonState.AdActive)
            {
                SetWatchAdButtonGrayscale(false);
                SetAdButtonView(true);
            }
            adButtonState = AdButtonState.AdActive;
        }
        else
        {
            if (!AdsController.instance.IsAdOnDailyLimit(AdsController.AdType.rewarded_ad_vitamin_collector) &&
                AdsController.instance.IsAdOnCooldown(AdsController.AdType.rewarded_ad_vitamin_collector))
            {
                // on cooldown
                if (adButtonState != AdButtonState.OnCooldown)
                {
                    SetWatchAdButtonGrayscale(true);
                    SetAdButtonView(true);
                }
                adButtonState = AdButtonState.OnCooldown;
            }
            else
            {
                // get vitamin for diamonds
                if (adButtonState != AdButtonState.ForDimonds)
                {
                    SetWatchAdButtonGrayscale(false);
                    SetAdButtonView(false);
                }
                adButtonState = AdButtonState.ForDimonds;
            }
        }
    }

    public void SetWatchAdButtonGrayscale(bool setGrayscale)
    {
        Image buttonImg = WatchAdButton.GetComponent<Image>();
        if (setGrayscale)
            buttonImg.material = ResourcesHolder.Get().GrayscaleMaterial;
        else
            buttonImg.material = null;
    }

    public void SetAdButtonView(bool adIcon)
    {
        if (adIcon)
        {
            WatchAdButtonVideoContent.gameObject.SetActive(true);
            WatchAdButtonDiamondsContentContent.gameObject.SetActive(false);
        }
        else
        {
            WatchAdButtonVideoContent.gameObject.SetActive(false);
            WatchAdButtonDiamondsContentContent.gameObject.SetActive(true);
        }
    }

    public void OnExitButtonClick()
    {
        Exit();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        data.collectorModel.capacityChanged -= CollectorModel_capacityChanged;
        KillCoroutine();
        base.Exit(hidePopupWithShowMainUI);
        CacheManager.PlayerHasSeenRefillVitaminPopup(data.collectorModel.med.ToString());
    }

    public void OnDestroy()
    {
        data.collectorModel.capacityChanged -= CollectorModel_capacityChanged;
        KillCoroutine();
    }

    private void KillCoroutine()
    {
        if (coroutine != null)
        {
            Timing.KillCoroutine(coroutine);
            coroutine = null;
        }
    }

    public class InputData
    {
        public VitaminCollectorModel collectorModel;
    }
}
