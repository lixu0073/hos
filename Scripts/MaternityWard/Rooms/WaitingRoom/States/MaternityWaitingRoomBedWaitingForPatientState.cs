using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maternity.UI;

namespace Maternity.WaitingRoom.Bed.State
{
    public class MaternityWaitingRoomBedWaitingForPatientState : MaternityWaitingRoomBedBaseState
    {
        public Data data = new Data();

        public MaternityWaitingRoomBedWaitingForPatientState(MaternityWaitingRoomBed parent, float timeLeft, float baseSpawnTime) : base(parent)
        {
            state = MaternityWaitingRoomBed.State.WFP;
            data.timeLeft = timeLeft;
            data.baseSpawnTime = baseSpawnTime;
        }

        public override void OnEnter()
        {
            
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            data.timeLeft -= timePassed.GetTimePassed();
        }

        public override void OnUpdate()
        {
            data.timeLeft -= Time.deltaTime;
            if(data.timeLeft <= 0)
            {
                data.timeLeft = 0;
                BroadcastData();
                parent.StateManager.State = new MaternityWaitingRoomBedOccupateRoomState(parent);
                return;
            }
            BroadcastData();
        }

        public override void Notify(int id, object parameters)
        {
            switch ((StateNotifications)id)
            {
                case StateNotifications.SpeedUpMotherSpawn:
                    parent.StateManager.State = new MaternityWaitingRoomBedOccupateRoomState(parent);
                    break;
                default:
                    break;
            }
        }

        public override string SaveToString()
        {
            return base.SaveToString() + "/" + data.timeLeft + "/" + data.baseSpawnTime;
        }

        public override void BroadcastData()
        {
            parent.BroadcastData(data);
        }

        public class Data
        {
            public float timeLeft;
            public float baseSpawnTime;
        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return new MaternityWaitingRoomIndicatorsController.WaitingForPatientIndicator(null, parent);
        }

    }
}
