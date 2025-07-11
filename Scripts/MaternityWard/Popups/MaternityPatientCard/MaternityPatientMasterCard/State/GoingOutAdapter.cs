using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Maternity.PatientStates;
using Hospital;

namespace Maternity.Adapter
{
    public class GoingOutAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public GoingOutAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

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
            controller.Ai.Person.State.BroadcastData();
        }

        private void SpeedUpButton()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                {
                    Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.SpeedUpMotherSpawn);
                    controller.Ai.Person.State.Notify((int)StateNotifications.SpeedUpMotherSpawn, null);
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
            controller.Ai.OnDataReceived_GO += Ai_OnDataReceived_GO;
        }

        private void Ai_OnDataReceived_GO(MaternityPatientGoingOutState.Data data)
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
            if (controller.Ai != null && controller.Ai.Person != null && controller.Ai.Person.State != null)
                controller.Ai.OnDataReceived_GO -= Ai_OnDataReceived_GO;
        }
    }
}