using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maternity.UI;

namespace Maternity.PatientStates
{
    public class MaternityPatientWaitingForSendToDiagnoseState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWoBaby;
        public MaternityPatientWaitingForSendToDiagnoseState(MaternityWaitingRoom room, MaternityPatientAI parent) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.WFSTD;
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            room.SetupBed(true, false, true);
            parent.position = room.GetBedPosition();
            NotificationCenter.Instance.MotherReachedWaitingRoomNotif.Invoke(new BaseNotificationEventArgs());
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.SendToDiagnose:
                    MaternityBloodTestRoom bloodTestRoom = MaternityBloodTestRoomController.Instance.GetBloodTestRoom();
                    long diagnoseSendTime = (long)ServerTime.getMilliSecTime();
                    parent.Person.State = new MaternityPatientInDiagnoseQueueState(room, bloodTestRoom, parent, diagnoseSendTime);
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

        public class Data
        {

        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return PatientShown() ? null : new MaternityWaitingRoomIndicatorsController.AlertIndicator(parent, room.bed);
        }

        private bool PatientShown()
        {
            return
                parent != null &&
                parent.GetInfoPatient() != null &&
                parent.GetInfoPatient().PatientShown;
        }
    }
}
