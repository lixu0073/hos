using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class WaitingForLaborAdapter : MaternityPatientMasterCardBaseAdapter
    {
        private const string EXCERCISE_BASE_KEY = "MATERNITY_STORK_EXERCISE";

        public WaitingForLaborAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        private int costInDiamonds = 0;
        private bool isFirstLoop = false;

        public override void SetUp()
        {
            base.SetUp();
            isFirstLoop = !Game.Instance.gameState().IsMaternityFirstLoopCompleted;
            ui.SetWaitingForLaborView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.WaitingForLabor).ToString(),
                OnSpeedUpButtonClicked,
                EMPTY,
                ResourcesHolder.GetMaternity().diamondSprite,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.waitingForLaborKey).ToUpper()
                );
            if (isFirstLoop)
            {
                costInDiamonds = 0;
                ui.SetCureButtonCurePanel(OnSpeedUpButtonClicked, I2.Loc.ScriptLocalization.Get("FREE").ToUpper(), null, true);
            }
            SetListeners();
            controller.Ai.Person.State.BroadcastData();
        }

        private void OnSpeedUpButtonClicked()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= costInDiamonds)
            {
                ui.SetCureButtonInteractive(false);
                DiamondTransactionController.Instance.AddDiamondTransaction(costInDiamonds, delegate
                {
                    UIController.getMaternity.patientCardController.Exit();
                    Game.Instance.gameState().RemoveDiamonds(costInDiamonds, EconomySource.SpeedUpWaitingForLabor);
                    controller.Ai.Person.State.Notify((int)StateNotifications.SpeedUpWaitingForLabour, null);
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
            controller.Ai.OnDataReceived_WFL += Ai_OnDataReceived_WFL;
        }

        private void Ai_OnDataReceived_WFL(PatientStates.MaternityPatientWaitingForLaborState.Data data)
        {
            int timeLeft = (int)Math.Ceiling(data.timeLeft);
            if (!isFirstLoop)
                SetSpeedUpButton(timeLeft);
            SetClockFill(timeLeft);
        }

        private void SetClockFill(int timeLeft)
        {
            ui.SetClockFill(1 - (float)timeLeft / controller.Info.MaxPreLabourTime);
        }

        private void SetSpeedUpButton(int timeLeft)
        {
            costInDiamonds = DiamondCostCalculator.GetCostForAction(timeLeft, controller.Info.MaxPreLabourTime);
            ui.SetCureButtonCurePanel(OnSpeedUpButtonClicked, costInDiamonds.ToString(), ResourcesHolder.GetMaternity().diamondSprite);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (controller.Ai != null)
                controller.Ai.OnDataReceived_WFL -= Ai_OnDataReceived_WFL;
        }

    }
}
