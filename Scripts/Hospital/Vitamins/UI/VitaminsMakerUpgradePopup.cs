using Hospital;
using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VitaminsMakerUpgradePopup : UIElement
{
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI TitleText;
    [SerializeField] TextMeshProUGUI VitaminDescriptionText;
    [SerializeField] TextMeshProUGUI FromToUpgradeAmountText;
    [SerializeField] Image VitaminImage;
    [SerializeField] Button UpgradeButton;
#pragma warning restore 0649

    public void Open(VitaminCollectorModel model, OnEvent onUpgrade)
    {
        StartCoroutine(base.Open(false, true, () =>
        {
            MedicineDatabaseEntry medicineDatabase = ResourcesHolder.Get().GetMedicineInfos(model.med);
            VitaminImage.sprite = medicineDatabase.image;
            TitleText.text = string.Format(I2.Loc.ScriptLocalization.Get("VITAMIN_COLLECTOR_UPGRADE_INFO"), I2.Loc.ScriptLocalization.Get(medicineDatabase.Name), model.collectorLevel + 1);
            VitaminDescriptionText.text = I2.Loc.ScriptLocalization.Get(medicineDatabase.StoryKey);
            int capacityToUpgrade = model.GetNextLevelMaxCapacity();
            FromToUpgradeAmountText.text = I2.Loc.ScriptLocalization.Get("FROM") + " " + model.maxCapacity + " " + I2.Loc.ScriptLocalization.Get("TO") + " " + capacityToUpgrade;
            UpgradeButton.RemoveAllOnClickListeners();
            UpgradeButton.onClick.AddListener(() =>
            {
                UpgradeCollector(model, onUpgrade);
            });
        }));
    }

    private void UpgradeCollector(VitaminCollectorModel model, OnEvent onUpgrade)
    {
        List<BaseUpgradeConfirmPopup.CardModel> cardModels = new List<BaseUpgradeConfirmPopup.CardModel>();
        MedicineRef[] medicinesCost = model.GetMedicinesForUpgrade();

        for (int i = 0; i < medicinesCost.Length; ++i)
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

    private IEnumerator ExitWithDely(AnimatorMonitor vitaminsMakerUpgradeConfirmPopup)
    {
        yield return new WaitForPopupClose(vitaminsMakerUpgradeConfirmPopup);
        Exit();
    }

    public void ButtonExit()
    {
        Exit();
    }

}
