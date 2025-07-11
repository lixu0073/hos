using Maternity.Adapter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.UI
{
    public class MaternityNurseRoomMasterCardAdapterFactory
    {
        public static MaternityNurseRoomMasterCardController.BaseAdapter Get(MaternityNurseRoomMasterCardController controller, IMaternityPatientPanelUI ui)
        {
            if(controller.Bed == null && controller.Ai == null && controller.Info == null)
            {
                return new MaternityNurseRoomMasterCardController.UnsupportedStateAdapter(controller, ui);
            }
            if (controller.Bed != null && controller.Ai == null && controller.Info == null)
            {
                switch (controller.Bed.StateManager.State.GetTag())
                {
                    case MaternityWaitingRoomBed.State.NELR:
                        return new MaternityNurseRoomMasterCardController.NoRequiredRoomsAdapter(controller, ui);
                    case MaternityWaitingRoomBed.State.WFP:
                        return new MaternityNurseRoomMasterCardController.WaitingForSpawnAdapter(controller, ui);
                }
            }
            switch (controller.Ai.Person.State.GetTag())
            {
                case PatientStates.MaternityPatientStateTag.GTWR:
                    return new MaternityNurseRoomMasterCardController.WaitingForPatientAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFSTD:
                    return new MaternityNurseRoomMasterCardController.WaitingForSendToDiagnoseAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFC:
                    return new MaternityNurseRoomMasterCardController.WaitingForCuresAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFL:
                    return new MaternityNurseRoomMasterCardController.WaitingForLaborAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.RFL:
                    return new MaternityNurseRoomMasterCardController.ReadyForLabourAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.GTLR:
                case PatientStates.MaternityPatientStateTag.IL:
                    return new MaternityNurseRoomMasterCardController.InLaborAdapater(controller, ui);
                case PatientStates.MaternityPatientStateTag.LF:
                    return new MaternityNurseRoomMasterCardController.LaborFinishedAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.RTWR:
                case PatientStates.MaternityPatientStateTag.B:
                    return new MaternityNurseRoomMasterCardController.BondingAdapater(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFCR:
                    return new MaternityNurseRoomMasterCardController.WaitForCollectLaborRewardAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.GO:
                    return new MaternityNurseRoomMasterCardController.GoingOutAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.ID:
                    return new MaternityNurseRoomMasterCardController.InDiagnoseAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.IDQ:
                    return new MaternityNurseRoomMasterCardController.InDiagnoseQueueAdapter(controller, ui);
                case PatientStates.MaternityPatientStateTag.WFDR:
                    return new MaternityNurseRoomMasterCardController.InWaitingForDiagnoseResultsAdapter(controller, ui);
            }
            return new MaternityNurseRoomMasterCardController.UnsupportedStateAdapter(controller, ui);
        }

    }
}