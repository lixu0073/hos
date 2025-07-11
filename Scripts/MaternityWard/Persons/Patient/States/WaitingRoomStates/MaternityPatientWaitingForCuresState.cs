using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using Maternity.UI;

namespace Maternity.PatientStates
{
    public class MaternityPatientWaitingForCuresState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        private bool fromSave;
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWoBaby;
        public MaternityPatientWaitingForCuresState(MaternityWaitingRoom room, MaternityPatientAI parent, bool fromSave) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.WFC;
            this.fromSave = fromSave;
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, false, true);
            if (fromSave)
            {
                room.ShowBloodTestBedTable();
            }
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.CuresDelivered:
                    NotificationCenter.Instance.MotherVitaminazed.Invoke(new BaseNotificationEventArgs());
                    MaternityWaitingRoomController.Instance.RefreshIndicators();
                    parent.Person.State = new MaternityPatientWaitingForLaborState(room, parent, parent.GetInfoPatient().MaxPreLabourTime);
                    break;
                case StateNotifications.OfficeMoved:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, true);
                    room.ShowBloodTestBedTable();
                    break;
                case StateNotifications.OfficeAnchored:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, true);
                    room.ShowBloodTestBedTable();
                    break;
                default:
                    break;
            }
        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            if(parent == null || parent.GetInfoPatient() == null || parent.GetInfoPatient().RequiredCures == null)
            {
                return null;
            }

            foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in parent.GetInfoPatient().RequiredCures)
            {
                MedicineRef item = pair.Key.GetMedicineRef();
                int currentCureCount = Game.Instance.gameState().GetCureCount(item);
                if (pair.Value > currentCureCount)
                {
                    return null;
                }
            }
            return new MaternityWaitingRoomIndicatorsController.CureIndicator(parent, room.bed);
        }

        public override void OnExit()
        {
            room.HideBloodTestBedTable();
        }
    }
}