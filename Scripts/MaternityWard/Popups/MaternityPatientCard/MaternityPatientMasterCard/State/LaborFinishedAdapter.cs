using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;

namespace Maternity.Adapter
{
    public class LaborFinishedAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public LaborFinishedAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) {}

        public override void SetUp()
        {
            base.SetUp();
            ui.SetLaborEndedView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.InLabor).ToString(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.laborEndedKey),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.laborEndedKey).ToUpper(),
                OnActionButtonCliked,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonShowKey).ToUpper());
        }

        protected void OnActionButtonCliked()
        {
            controller.Ai.Notify((int)StateNotifications.OnChildCollect, null);
        }

    }
}
