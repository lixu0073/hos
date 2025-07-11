using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientBaseLaborState : MaternityPatientBaseState<MaternityLabourRoom>
    {
        public Data data = new Data();
        public enum INDEX
        {
            labourRoomTag = 5,
            timeLeft = 6
        }

        public override void OnEnter()
        {
            base.OnEnter();
            room.AssignPatientToLabourRoom(parent);
        }

        public MaternityPatientBaseLaborState(MaternityLabourRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent)
        {
            data.timeLeft = timeLeft;
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            switch ((StateNotifications)id)
            {
                case StateNotifications.SpeedUpLabor:
                    parent.abortPath();
                    parent.walkingStateManager.State = null;
                    MaternityPatientBaseState<MaternityLabourRoom>.OnChildCollect(parent, room);
                    break;
                default:
                    break;
            }
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            data.timeLeft -= timePassed.GetTimePassed() ;
        }

        public override void OnUpdate()
        {
            data.timeLeft -= Time.deltaTime;
            if (data.timeLeft < 0)
                data.timeLeft = 0;
            BroadcastData();
        }

        public override void OnExit()
        {
            base.OnExit();
            room.RemovePatientFromLabourRoom();
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