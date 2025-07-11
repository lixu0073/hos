using System;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using UnityEngine;

namespace Hospital
{
    public class VitaminsMakerUpgradeConfirmPopUp : BaseUpgradeConfirmPopup
    {
#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI PopupTitleText;
        [SerializeField] private TextMeshProUGUI UpgradeVitaminCollectorText;
#pragma warning restore 0649

        public override void Open(OnEvent onUpgrade, List<CardModel> cardModels, VitaminCollectorModel model)
        {
            gameObject.SetActive(true);
            StartCoroutine(base.Open(onUpgrade, cardModels, () =>
            {
                UpgradeVitaminCollectorText.text = String.Format(I2.Loc.ScriptLocalization.Get("UPGRADE_VIT_COLLECTOR_LEVEL"), (model.collectorLevel + 1));
                PopupTitleText.SetText(ResourcesHolder.Get().GetNameForCure(model.med).ToUpper());
            }));
        }        
    }
}
