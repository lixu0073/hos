using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class ReadyForLabourAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public ReadyForLabourAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) {}

        public override void SetUp()
        {
            base.SetUp();
            ui.SetReadyForLaborView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.WaitingForLabor).ToString(),
                OnActionButtonCliked,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonSendKey).ToUpper(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.sendToLabourKey).ToUpper()
                );
        }

        protected void OnActionButtonCliked()
        {
            controller.Ai.Notify((int)StateNotifications.SendToLabor, null);
        }

    }
}
