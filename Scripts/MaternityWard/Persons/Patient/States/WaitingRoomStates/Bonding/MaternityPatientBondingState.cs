using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientBondingState : MaternityPatientBaseBondingState
    {
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWithBaby;
        public MaternityPatientBondingState(MaternityWaitingRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent, timeLeft)
        {
            stateTag = MaternityPatientStateTag.B;
        }

        public static MaternityPatientBondingStateParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            return new MaternityPatientBondingStateParsedData(timeLeft);
        }

        public static MaternityPatientBondingState GetInstance(MaternityWaitingRoom room, MaternityPatientAI parent, MaternityPatientBondingStateParsedData data)
        {
            return new MaternityPatientBondingState(room, parent, data.timeLeft);
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, false, false);
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.OfficeMoved:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, false);
                    break;
                case StateNotifications.OfficeAnchored:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, false);
                    break;
                default:
                    break;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if(data.timeLeft <= 0)
            {
                parent.Person.State = new MaternityPatientWaitingForCollectRewardState(room, parent);
            }
        }
    }

    public class MaternityPatientBondingStateParsedData
    {
        public float timeLeft;

        public MaternityPatientBondingStateParsedData(float timeLeft)
        {
            this.timeLeft = timeLeft;
        }
    }
}
