using Hospital;
using Maternity;
using Maternity.PatientStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

namespace Maternity.PatientStates
{
    public class MaternityPatientInDiagnoseState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        public Data data = new Data();
        MaternityBloodTestRoom bloodTestRoom;
        private static float timeToEmulate = 0;
        bool patientSetupFromSave = false;
        private const int SAVE_INDICATOR_FOR_PATIENT_CURENTLY_DIAGNOSED = -1;
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWoBaby;

        public MaternityPatientInDiagnoseState(MaternityWaitingRoom room, MaternityBloodTestRoom bloodTestRoom, MaternityPatientAI parent, float timeLeft, bool patientSetupFromSave = false) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.ID;
            this.bloodTestRoom = bloodTestRoom;
            data.timeLeft = timeLeft;
            this.patientSetupFromSave = patientSetupFromSave;
            Emulate();
        }

        public static void ResetDiagnosedTimeToEmulate()
        {
            timeToEmulate = 0;
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, false, true);
            if (patientSetupFromSave)
            {
                bloodTestRoom.AddPatientFromSaveToQueue(parent, SAVE_INDICATOR_FOR_PATIENT_CURENTLY_DIAGNOSED);
            }
        }

        public override void Notify(int id, object parameters)
        {
            switch ((StateNotifications)id)
            {
                case StateNotifications.BloodTestSpeedUp:
                    parent.Person.State = new MaternityPatientInWaitingForDiagnoseResultsState(room, parent);
                    bloodTestRoom.CollectInstantHealCollectable();
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

        public enum INDEX
        {
            timeLeft = 6,
        }

        public override string SaveToString()
        {
            return base.SaveToString() + "!" + data.timeLeft;
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            timeToEmulate = timePassed.GetTimePassed();
            Emulate();
        }

        private void Emulate()
        {
            if (data.timeLeft <= timeToEmulate)
            {
                timeToEmulate -= data.timeLeft;
                data.timeLeft = 0;
            }
            else
            {
                data.timeLeft -= timeToEmulate;
                timeToEmulate = 0;
            }
        }

        public override void OnUpdate()
        {
            data.timeLeft -= Time.deltaTime;
            BroadcastData();
            if (data.timeLeft <= 0)
            {
                parent.Person.State = new MaternityPatientInWaitingForDiagnoseResultsState(room, parent);
            }
        }

        public override void OnExit()
        {

        }

        public static MaternityPatientInDiagnoseStateData Parse(string data)
        {
            string[] strs = data.Split('!');
            bool patientSetupFromSave = true;
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            return new MaternityPatientInDiagnoseStateData(timeLeft, patientSetupFromSave);
        }

        internal static MaternityIState GetInstance(MaternityWaitingRoom room, MaternityBloodTestRoom bloodTestRoom, MaternityPatientAI parent, MaternityPatientInDiagnoseStateData data)
        {
            return new MaternityPatientInDiagnoseState(room, bloodTestRoom, parent, data.timeLeft, data.patientStetupFromSave);
        }

        public override void BroadcastData()
        {
            parent.BroadcastData(data);
        }

        public class Data
        {
            public float timeLeft;
        }
    }
    public class MaternityPatientInDiagnoseStateData
    {
        public float timeLeft;
        public bool patientStetupFromSave;

        public MaternityPatientInDiagnoseStateData(float timeLeft, bool patientStetupFromSave)
        {
            this.timeLeft = timeLeft;
            this.patientStetupFromSave = patientStetupFromSave;
        }
    }
}