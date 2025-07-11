using Maternity.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.Adapter
{
    public class InDiagnoseQueueAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public InDiagnoseQueueAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        public override void SetUp()
        {
            base.SetUp();
            ui.SetDiagnoseInQueueView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                OnCheckButtonClicked,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonCheckKey).ToUpper(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.bloodTestKey),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.queueKey)
                );
        }

        private void OnCheckButtonClicked()
        {
            ClosePopupAndRedirectToBloodTestRoom(true);
        }

    }
}