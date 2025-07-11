using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class BondingAdapater : MaternityPatientMasterCardBaseAdapter
    {
        public BondingAdapater(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        private int speedUpCostInDiamonds = 0;
        private bool processingGiftsGet = false;
        private bool isFirstLoop = false;

        public override void SetUp()
        {
            base.SetUp();
            isFirstLoop = !Game.Instance.gameState().IsMaternityFirstLoopCompleted;
            ui.SetHealingAndBoundingGiftTimerView(
                GetPatientInfo(),
                GetPatientBabyInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.HealingAndBounding).ToString(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.healingAndBondingKey).ToUpper(),
                OnSpeedUpGiftButtonClicked,
                EMPTY,
                ResourcesHolder.GetMaternity().diamondSprite
                );
            if (isFirstLoop)
            {
                speedUpCostInDiamonds = 0;
                ui.SetButtonTreatmentStage(OnSpeedUpGiftButtonClicked, I2.Loc.ScriptLocalization.Get("FREE").ToUpper(), null, true);
            }
            SetListeners();
            controller.Ai.Person.State.BroadcastData();
        }

        /// <summary>
        /// This is called when the treatment button is called to speed up the healing and bonding process.
        /// </summary>
        private void OnSpeedUpGiftButtonClicked()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= speedUpCostInDiamonds)
            { //Player has enough gems.

                if (processingGiftsGet)
                {
                    Debug.LogWarning("Speed up pressed but we are already processing the gifts.");
                    return;
                }
                DiamondTransactionController.Instance.AddDiamondTransaction(speedUpCostInDiamonds, delegate
                {
                    UIController.getMaternity.patientCardController.Exit();
                    Game.Instance.gameState().RemoveDiamonds(speedUpCostInDiamonds, EconomySource.SpeedUpBondingAndHealing, controller.Ai.Person.State.GetRoom().Tag);
                    MaternityBoxRewardGenerator.Get(OnBoxRewardsReceived);
                    processingGiftsGet = false;
                }, this);
                processingGiftsGet = true;
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void SetListeners()
        {
            controller.Ai.OnDataReceived_BBS += Ai_OnDataReceived_BBS;
        }

        private void Ai_OnDataReceived_BBS(PatientStates.MaternityPatientBaseBondingState.Data data)
        {
            int secondsLeft = (int)Math.Ceiling(data.timeLeft);
            ui.SetTimerTreatmentTreatmentStagePanel(UIController.GetFormattedTime(secondsLeft));
            if (!isFirstLoop)
                SetSpeedUpButton(secondsLeft);
        }

        private void SetSpeedUpButton(int timeLeft)
        {
            speedUpCostInDiamonds = DiamondCostCalculator.GetCostForAction(timeLeft, controller.Info.MaxHealingAndBondingTime);
            ui.SetButtonTreatmentStage(OnSpeedUpGiftButtonClicked, speedUpCostInDiamonds.ToString(), ResourcesHolder.GetMaternity().diamondSprite);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (controller.Ai != null)
                controller.Ai.OnDataReceived_BBS -= Ai_OnDataReceived_BBS;
        }
    }
}
