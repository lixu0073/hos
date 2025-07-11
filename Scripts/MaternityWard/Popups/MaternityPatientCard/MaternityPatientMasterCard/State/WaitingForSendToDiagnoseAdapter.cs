using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{

    public class WaitingForSendToDiagnoseAdapter : MaternityPatientMasterCardBaseAdapter
    {

        private const string QUEUE_FULL_KEY = "MATERNITY_QUEUE_FULL";

        public WaitingForSendToDiagnoseAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        public override void SetUp()
        {
            base.SetUp();
            string buttonText = I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonSendKey);

            ui.SetDiagnoseRequiredView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                OnActionButtonCliked,
                buttonText.ToUpper(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.bloodTestKey),
                Game.Instance.gameState().IsMaternityFirstLoopCompleted ? MaternityCoreLoopParametersHolder.GetBloodTestCostInCoins().ToString() : I2.Loc.ScriptLocalization.Get("FREE").ToUpper(),
                UIController.GetFormattedShortTime(((MaternityBloodTestRoomInfo)MaternityBloodTestRoomController.Instance.GetBloodTestRoom().GetRoomInfo()).GetDiagnoseTime()),
                Game.Instance.gameState().IsMaternityFirstLoopCompleted ? ResourcesHolder.Get().coinSprite : null,
                !Game.Instance.gameState().IsMaternityFirstLoopCompleted
                );
            ui.SetCureButtonInteractive(MaternityBloodTestRoomController.Instance.GetBloodTestRoom().CanAddPatientToQueue());
        }

        protected void OnActionButtonCliked()
        {
            if (!MaternityBloodTestRoomController.Instance.GetBloodTestRoom().CanAddPatientToQueue())
            {
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(QUEUE_FULL_KEY));
                ClosePopupAndRedirectToBloodTestRoom(true);
                return;
            }
            if(!Game.Instance.gameState().IsMaternityFirstLoopCompleted)
            {
                ClosePopupAndRedirectToBloodTestRoom(true);
                controller.Ai.Notify((int)StateNotifications.SendToDiagnose, null);
                return;
            }
            if(Game.Instance.gameState().GetCoinAmount() >= MaternityCoreLoopParametersHolder.GetBloodTestCostInCoins())
            {
                ClosePopupAndRedirectToBloodTestRoom(true);
                Game.Instance.gameState().RemoveCoins(MaternityCoreLoopParametersHolder.GetBloodTestCostInCoins(), EconomySource.SendToBloodTest);
                controller.Ai.Notify((int)StateNotifications.SendToDiagnose, null);    
            }
            else
            {
                UIController.get.BuyResourcesPopUp.Open(
                    MaternityCoreLoopParametersHolder.GetBloodTestCostInCoins() - Game.Instance.gameState().GetCoinAmount(), 
                            () => {
                                Game.Instance.gameState().RemoveCoins(MaternityCoreLoopParametersHolder.GetBloodTestCostInCoins(), EconomySource.SendToBloodTest);
                                controller.Ai.Notify((int)StateNotifications.SendToDiagnose, null);
                            }
                           , null);
            }
        }

    }
}