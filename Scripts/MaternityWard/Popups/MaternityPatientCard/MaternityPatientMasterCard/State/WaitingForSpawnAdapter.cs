using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class WaitingForSpawnAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public WaitingForSpawnAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        private int costInDiamonds = 0;

        public override void SetUp()
        {
            base.SetUp();
            string info = I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.waitingRoomKey);
            ui.SetWaitingForNextPatientView(
                SpeedUpButton,
                EMPTY,
                null,
                info);
            SetListeners();
            controller.Bed.StateManager.State.BroadcastData();
        }

        private void SpeedUpButton()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                {
                    Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.SpeedUpMotherSpawn);
                    controller.Bed.StateManager.State.Notify((int)StateNotifications.SpeedUpMotherSpawn, null);
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
            controller.Bed.OnDataReceived_WFP += Bed_OnDataReceived_WFP;
        }

        private void Bed_OnDataReceived_WFP(WaitingRoom.Bed.State.MaternityWaitingRoomBedWaitingForPatientState.Data data)
        {
            int secondsLeft = (int)Math.Ceiling(data.timeLeft);
            ui.SetTimerBedInfoPanelWaiting(UIController.GetFormattedTime(secondsLeft));
            SetSpeedUpButton(secondsLeft, data.baseSpawnTime);
        }

        private void SetSpeedUpButton(int timeLeft, float baseSpawnTime)
        {
            costInDiamonds = DiamondCostCalculator.GetCostForAction(timeLeft, baseSpawnTime);
            ui.SetButtonBedInfoPanel(SpeedUpButton, costInDiamonds.ToString(), ResourcesHolder.GetMaternity().diamondSprite);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (controller.Bed != null)
                controller.Bed.OnDataReceived_WFP -= Bed_OnDataReceived_WFP;
        }

    }
}
