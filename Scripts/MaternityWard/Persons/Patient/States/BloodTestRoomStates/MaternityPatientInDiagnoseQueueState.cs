using Hospital;
using Maternity;
using Maternity.PatientStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Maternity.PatientStates
{
    public class MaternityPatientInDiagnoseQueueState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        Data data = new Data();
        MaternityBloodTestRoom bloodTestRoom;
        bool fromSave = false;
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWoBaby;
        public MaternityPatientInDiagnoseQueueState(MaternityWaitingRoom room, MaternityBloodTestRoom bloodTestRoom, MaternityPatientAI parent, long diagnoseTimeSend, bool fromSave = false) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.IDQ;
            this.bloodTestRoom = bloodTestRoom;
            data.timeDiagnoseSend = diagnoseTimeSend;
            this.fromSave = fromSave;
        }

        public override void Notify(int id, object parameters)
        {
            switch ((StateNotifications)id)
            {
                case StateNotifications.FirstInQueue:
                    int diagnoseTime = ((MaternityBloodTestRoomInfo)bloodTestRoom.GetRoomInfo()).GetDiagnoseTime();
                    parent.Person.State = new MaternityPatientInDiagnoseState(room, bloodTestRoom, parent, diagnoseTime, false);
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

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, false, true);
            if (fromSave)
            {
                bloodTestRoom.AddPatientFromSaveToQueue(parent, data.timeDiagnoseSend);
            }
            else
            {
                bloodTestRoom.AddBloodTestToQueue(parent);
            }
        }

        public override void OnExit()
        {

        }

        public enum INDEX
        {
            diagnoseStartedTime = 6,
        }

        public override void OnUpdate()
        {

        }

        public override string SaveToString()
        {
            return base.SaveToString() + "!" + data.timeDiagnoseSend;
        }

    public override void EmulateTime(TimePassedObject timePassed) { }

        public override void BroadcastData()
        {
            Debug.LogError("Nothing To Broadcast");
        }

        public class Data
        {
            public long timeDiagnoseSend;
        }

        public static MaternityPatientInDiagnoseQueueStateData Parse(string data)
        {
            string[] strs = data.Split('!');
            bool patientFromSave = true;
            long diagnoseStartedTime = long.Parse(strs[(int)INDEX.diagnoseStartedTime]);
            return new MaternityPatientInDiagnoseQueueStateData(diagnoseStartedTime, patientFromSave);
        }

        public static MaternityIState GetInstance(MaternityWaitingRoom waitingRoom, MaternityBloodTestRoom bloodTestRoom, MaternityPatientAI maternityPatientAI, MaternityPatientInDiagnoseQueueStateData data)
        {
            return new MaternityPatientInDiagnoseQueueState(waitingRoom, bloodTestRoom, maternityPatientAI, data.timediagnoseStarted, data.patientSetupFromSave);
        }

    }
    public class MaternityPatientInDiagnoseQueueStateData
    {
        public long timediagnoseStarted;
        public bool patientSetupFromSave;

        public MaternityPatientInDiagnoseQueueStateData(long timediagnoseStarted, bool patientSetupFromSave)
        {
            this.timediagnoseStarted = timediagnoseStarted;
            this.patientSetupFromSave = patientSetupFromSave;
        }
    }
}
