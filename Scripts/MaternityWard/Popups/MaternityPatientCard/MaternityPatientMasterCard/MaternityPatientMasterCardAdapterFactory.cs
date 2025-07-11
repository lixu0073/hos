using Maternity.Adapter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.UI
{
    public class MaternityPatientMasterCardAdapterFactory
    {
        public static MaternityPatientMasterCardBaseAdapter Get(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui)
        {
            if (controller.Bed != null && controller.Ai == null && controller.Info == null)
            {
                switch (controller.Bed.StateManager.State.GetTag())
                {
                    case MaternityWaitingRoomBed.State.NELR:
                        return new NoRequiredRoomsAdapter(controller, ui);
                    case MaternityWaitingRoomBed.State.WFP:
                        return new WaitingForSpawnAdapter(controller, ui);
                }
            }
            if (controller.Ai == null && controller.Info == null)
            {
                return null;
                // return new TestMasterPatientCardController.EmptyCardAdapater(parent);
            }
            switch (controller.Ai.Person.State.GetTag())
            {
                case PatientStates.MaternityPatientStateTag.GTWR:
                    return new WaitingForPatientAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFSTD:
                    return new WaitingForSendToDiagnoseAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFC:
                    return new WaitingForCuresAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFL:
                    return new WaitingForLaborAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.RFL:
                    return new ReadyForLabourAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.GTLR:
                case PatientStates.MaternityPatientStateTag.IL:
                    return new InLaborAdapater(controller, ui);
                case PatientStates.MaternityPatientStateTag.LF:
                    return new LaborFinishedAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.RTWR:
                case PatientStates.MaternityPatientStateTag.B:
                    return new BondingAdapater(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFCR:
                    return new WaitForCollectLaborRewardAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.GO:
                    return new GoingOutAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.ID:
                    return new InDiagnoseAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.IDQ:
                    return new InDiagnoseQueueAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFDR:
                    return new InWaitingForDiagnoseResultsAdapter(controller, ui);
                //default:
                    //return new TestMasterPatientCardController.UnsupportedStateAdapter(parent);
            }
            return null;
        }

    }
}
