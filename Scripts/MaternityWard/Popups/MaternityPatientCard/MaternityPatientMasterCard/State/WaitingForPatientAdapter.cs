using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;

namespace Maternity.Adapter
{
    public class WaitingForPatientAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public WaitingForPatientAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui)
        {
        }

        public override void SetUp()
        {
            base.SetUp();
            ui.SetNextPatientOnHisWayView(
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.nextOnWayKey), 
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.waitingRoomKey));
        }
    }
}
