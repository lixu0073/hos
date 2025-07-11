using System.Collections.Generic;
using UnityEngine;
using Maternity.UI;

namespace Maternity.WaitingRoom.Bed.State
{
    public class MaternityWaitingRoomBedOccupateRoomState : MaternityWaitingRoomBedBaseState
    {
        protected string PatientID;
        //private bool patientLoaded = false;

        public MaternityWaitingRoomBedOccupateRoomState(MaternityWaitingRoomBed parent, string patientID = null, List<string> unparsedPatients = null) : base(parent)
        {
            this.state = MaternityWaitingRoomBed.State.OR;
            PatientID = patientID;
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            if (parent.IsBedOccupied())
                parent.GetPatient().GetPatientAI().EmulateTime(timePassed);
        }

        public override void OnEmulationEnded()
        {
            if (parent.IsBedOccupied())
                parent.GetPatient().GetPatientAI().OnEmulationEnded();
        }

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(PatientID))
            {
                MaternityPatientAI ai = parent.SpawnMother(); // ja w oogle bym pobieral od waitingRoom id pacjenta zestorowanego w lozku.
                PatientID = ai.ID; // matward poprawic??
            }
            else
            {
                string unparsedData = MaternityPatientsHolder.Instance.GetUnparsedPatientByID(PatientID);
                if (string.IsNullOrEmpty(unparsedData))
                    Debug.LogError("CRITICAL ERROR THERE NO " + PatientID + " IN SAVE");
                else
                {
                    string[] array = unparsedData.Split('^');
                    parent.LoadMother(array[0], array[1], array[2]);
                }
            }
            MaternityPatientCardController.Refresh();
            MaternityNurseRoomCardController.Refresh();
            parent.room.SetUpIndicators();
            parent.room.GetLabourRoom().SetUpIndicators();
        }

        public override void Notify(int id, object parameters)
        {
            // TODO
            // notify load patient
        }

        public override string SaveToString()
        {
            return base.SaveToString() + "/" + PatientID;
        }
    }
}
