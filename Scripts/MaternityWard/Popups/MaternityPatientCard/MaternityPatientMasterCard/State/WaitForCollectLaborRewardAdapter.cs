using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;
using SimpleUI;

namespace Maternity.Adapter
{
    public class WaitForCollectLaborRewardAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public WaitForCollectLaborRewardAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) {}

        private bool processingGiftsGet = false;

        public override void SetUp()
        {
            base.SetUp();
            ui.SetHealingAndBoundingGiftReadyView(
                GetPatientInfo(),
                GetPatientBabyInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.HealingAndBounding).ToString(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.healingAndBondingKey),
                OnActionButtonClicked,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonOpenKey).ToUpper(),
                null
                );
        }

        private void OnActionButtonClicked()
        {
            if (processingGiftsGet)
                return;
            processingGiftsGet = true;

            UIController.getMaternity.patientCardController.Exit();
            AddHealingAndBondingExp();
            MaternityBoxRewardGenerator.Get(OnBoxRewardsReceived);
        }

    }
}
