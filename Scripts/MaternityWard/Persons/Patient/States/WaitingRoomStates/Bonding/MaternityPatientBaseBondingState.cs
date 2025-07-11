using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientBaseBondingState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        public Data data = new Data();

        public enum INDEX
        {
            timeLeft = 6
        }

        public MaternityPatientBaseBondingState(MaternityWaitingRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent)
        {
            data.timeLeft = timeLeft;
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            data.timeLeft -= timePassed.GetTimePassed();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.SpeedUpRewardForLabor:
                    data.timeLeft = 0;
                    parent.Person.State = new MaternityPatientWaitingForCollectRewardState(room, parent);
                    break;
                case StateNotifications.CollectRewardForLabor:
                    float timeToSpawn = MaternityCoreLoopParametersHolder.GetNextMotherSpawnDuration();
                    room.SetupBed(false);
                    parent.Person.State = new MaternityPatientGoingOutState(MaternityCoreLoopParametersHolder.GetMotherSpawnPosition(), timeToSpawn, parent, timeToSpawn);
                    break;
                default:
                    break;
            }
        }
        
        public override void OnUpdate()
        {
            data.timeLeft -= Time.deltaTime;
            if (data.timeLeft < 0)
                data.timeLeft = 0;
            BroadcastData();
        }

        public override string SaveToString()
        {
            return base.SaveToString() + "!" + data.timeLeft;
        }

        public class Data
        {
            public float timeLeft;
        }

        public override void BroadcastData()
        {
            parent.BroadcastData(data);
        }

    }
}