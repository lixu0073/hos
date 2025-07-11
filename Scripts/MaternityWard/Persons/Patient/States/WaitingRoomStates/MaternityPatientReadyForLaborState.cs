using Hospital;
using Maternity.UI;
using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientReadyForLaborState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        int defaultLayInBedAnimation = AnimHash.Mother_ReadyForLabour;
        public MaternityPatientReadyForLaborState(MaternityWaitingRoom room, MaternityPatientAI parent) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.RFL;
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, false, true);
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.SendToLabor:
                    OnSendToLabor(parent);
                    parent.Person.State = new MaternityPatientGoToLaborRoomState(room.GetLabourRoom(), parent, parent.GetInfoPatient().MaxLabourTime);
                    break;
                case StateNotifications.OfficeMoved:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, true);
                    break;
                case StateNotifications.OfficeAnchored:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, true);
                    break;
                default:
                    break;
            }
        }

        public override void OnExit()
        {
            room.SetupBed(true, true, true);
        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return new MaternityWaitingRoomIndicatorsController.ReadyForLaborIndicator(parent, room.bed);
        }
    }
}
