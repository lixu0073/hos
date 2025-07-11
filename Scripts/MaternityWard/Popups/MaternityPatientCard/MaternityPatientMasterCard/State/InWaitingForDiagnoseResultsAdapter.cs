using Maternity.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.Adapter
{
    public class InWaitingForDiagnoseResultsAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public InWaitingForDiagnoseResultsAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        public override void SetUp()
        {
            base.SetUp();
            ui.SetDiagnoseEndedView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                OnCheckButtonCliked,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonCheckKey).ToUpper(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.diagnoseEndedKey),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.diagnoseResultsKey)
                );
        }

        protected virtual void OnCheckButtonCliked()
        {
            ClosePopupAndRedirectToBloodTestRoom(false);
        }

    }
}