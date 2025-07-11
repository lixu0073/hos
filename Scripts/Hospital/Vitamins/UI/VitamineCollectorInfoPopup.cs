using Hospital;
using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class VitamineCollectorInfoPopup : UIElement
{
#pragma warning disable 0649
    [SerializeField] private TextMeshProUGUI StoryText;
    [SerializeField] private TextMeshProUGUI CapacityText;
    [SerializeField] private TextMeshProUGUI CollectRateText;
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private TextMeshProUGUI fromCapacityNumber;
    [SerializeField] private TextMeshProUGUI toCapacityNumber;
    [SerializeField] private TextMeshProUGUI fromCollectRateNumber;
    [SerializeField] private TextMeshProUGUI toCollectRateNumber;
    [SerializeField] private TextMeshProUGUI shortInfoText;
    [SerializeField] Button UpgradeButton;
    [SerializeField] private Image MedicineImage;
#pragma warning restore 0649
    private OnEvent onSuccess;    

    private InputData data;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Open(InputData data, OnEvent onSuccess)
    {
        gameObject.SetActive(true);
        StartCoroutine(Open(false, false, () =>
        {
            this.data = data;
            MedicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(data.collectorModel.med);
            TitleText.SetText(ResourcesHolder.Get().GetNameForCure(data.collectorModel.med).ToUpper());
            StoryText.SetText(ResourcesHolder.GetHospital().GetStoryForCure(data.collectorModel.med));
            SetShortInfoText(data.collectorModel);
            CollectRateText.SetText((int)data.collectorModel.FillRatio + "/H");
            SetCapacity(data.collectorModel.capacity, data.collectorModel.maxCapacity);
            SetLevelText(data.collectorModel.collectorLevel);
            SetUpgradeCapacityText(data.collectorModel.maxCapacity, data.collectorModel.GetNextLevelMaxCapacity());
            SetUpgradeCollectRateText((int)data.collectorModel.FillRatio, (int)data.collectorModel.GetNextLevelFillRatio());
            this.data.collectorModel.capacityChanged += CollectorModel_capacityChanged;
            UpgradeButton.RemoveAllOnClickListeners();
            this.onSuccess = onSuccess;
            UpgradeButton.onClick.AddListener(() =>
            {
                UpgradeCollector(data.collectorModel, OnSuccess);
            });
        }));
    }

    private void SetShortInfoText(VitaminCollectorModel model)
    {
        shortInfoText.text = String.Format(I2.Loc.ScriptLocalization.Get("UPGRADE_VIT_ANY"), ResourcesHolder.Get().GetNameForCure(model.med));            
    }

    private void UpgradeCollector(VitaminCollectorModel model, OnEvent onUpgrade)
    {
        List<BaseUpgradeConfirmPopup.CardModel> cardModels = new List<BaseUpgradeConfirmPopup.CardModel>();
        MedicineRef[] medicinesCost = model.GetMedicinesForUpgrade();

        for(int i = 0; i < medicinesCost.Length; ++i)
        {
            cardModels.Add(new BaseUpgradeConfirmPopup.MedicineCardModel(model.GetAmountOfSpecialItemForUpgrade(), medicinesCost[i]));
        }

        cardModels.Add(new BaseUpgradeConfirmPopup.PositiveEnergyCardModel(model.GetAmountOfPositiveEnergyForUpgrade()));    

        UIController.getHospital.vitaminsMakerUpgradeConfirmPopup.Open(
            () =>
                {
                    model.Upgrade();
                    StartCoroutine(ExitWithDely(UIController.getHospital.vitaminsMakerUpgradeConfirmPopup));
                    onUpgrade.Invoke();
            
                },             
            cardModels,
            model
        );
    }

    private void CollectorModel_capacityChanged(float fill, float current, int max, int producedAmount, MedicineRef vitamin, int timeToDrop, VitaminCollectorModel.VitaminSource source)
    {
        SetCapacity(current, max);
    }

    private void SetCapacity(float capacity, int maxCapacity)
    {
        CapacityText.SetText((int)capacity + "/" + maxCapacity);
    }

    private void SetUpgradeCapacityText(int currentMaxCapacity, int nextLevelMaxCapacity)
    {
        fromCapacityNumber.text = currentMaxCapacity.ToString();
        toCapacityNumber.text = nextLevelMaxCapacity.ToString();
    }

    private void SetUpgradeCollectRateText(int currentCollectRate, int nextLevelCollectRate)
    {
        fromCollectRateNumber.text = currentCollectRate.ToString();
        toCollectRateNumber.text = nextLevelCollectRate.ToString() + "/H";
    }

    private void SetLevelText(int currentLevel)
    {
        currentLevelText.SetText(I2.Loc.ScriptLocalization.Get("LEVEL_U") + " " + currentLevel);
        int nextLevel = ++currentLevel;
        nextLevelText.SetText(I2.Loc.ScriptLocalization.Get("LEVEL_U") + " " + nextLevel);
    }

    public void OnExitButtonClick()
    {
        Exit();
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        data.collectorModel.capacityChanged -= CollectorModel_capacityChanged;
        base.Exit(hidePopupWithShowMainUI);
    }

    public void OnDestroy()
    {
        data.collectorModel.capacityChanged -= CollectorModel_capacityChanged;
    }

    private void OnSuccess()
    {
        StartCoroutine(OnSuccessWithDely(UIController.getHospital.vitaminsMakerUpgradePopup));
    }

    private IEnumerator OnSuccessWithDely(AnimatorMonitor vitaminsMakerUpgradeConfirmPopup)
    {
        yield return new WaitForPopupClose(vitaminsMakerUpgradeConfirmPopup);
        onSuccess.Invoke();
    }

    private IEnumerator ExitWithDely(AnimatorMonitor vitaminsMakerUpgradeConfirmPopup)
    {
        yield return new WaitForPopupClose(vitaminsMakerUpgradeConfirmPopup);
        Exit();
    }

    public class InputData
    {
        public VitaminCollectorModel collectorModel;
    }
}
