using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class InLaborAdapater : MaternityPatientMasterCardBaseAdapter
    {
        public InLaborAdapater(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) {}

        private int costInDiamonds = 0;
        private bool isFristLoop = false;

        public override void SetUp()
        {
            base.SetUp();
            isFristLoop = !Game.Instance.gameState().IsMaternityFirstLoopCompleted;
            ui.SetInLaborView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.InLabor).ToString(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.waitingForLaborKey),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.inLaborKey).ToUpper(),
                SpeedUpButton,
                EMPTY,
                ResourcesHolder.GetMaternity().diamondSprite
                );
            if(isFristLoop)
            {
                costInDiamonds = 0;
                ui.SetButtonTreatmentStage(SpeedUpButton, I2.Loc.ScriptLocalization.Get("FREE").ToUpper(), null, true);
            }
            SetListeners();
            controller.Ai.Person.State.BroadcastData();
        }

        private void SpeedUpButton()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                {
                    Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.SpeedUpLabor, controller.Ai.Person.State.GetRoom().Tag);
                    controller.Ai.Person.State.Notify((int)StateNotifications.SpeedUpLabor, null);
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void SetListeners()
        {
            controller.Ai.OnDataReceived_BLS += Ai_OnDataReceived_BLS;
        }

        private void SetSpeedUpButton(int timeLeft)
        {
            costInDiamonds = DiamondCostCalculator.GetCostForAction(timeLeft, controller.Info.MaxLabourTime);
            ui.SetButtonTreatmentStage(SpeedUpButton, costInDiamonds.ToString(), ResourcesHolder.GetMaternity().diamondSprite);
        }

        private void Ai_OnDataReceived_BLS(PatientStates.MaternityPatientBaseLaborState.Data data)
        {
            int secondsLeft = (int)Math.Ceiling(data.timeLeft);
            ui.SetTimerTreatmentTreatmentStagePanel(UIController.GetFormattedTime(secondsLeft));
            if(!isFristLoop)
                SetSpeedUpButton(secondsLeft);
        }

        public override void OnDestroy()
        {
            if(controller.Ai != null)
                controller.Ai.OnDataReceived_BLS -= Ai_OnDataReceived_BLS;
            base.OnDestroy();
        }

    }
}
