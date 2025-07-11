using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientInLaborState : MaternityPatientBaseLaborState
    {
        public MaternityPatientInLaborState(MaternityLabourRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent, timeLeft)
        {
            stateTag = MaternityPatientStateTag.IL;
        }

        public static MaternityPatientInLabourStateParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            string roomTag = strs[(int)INDEX.labourRoomTag];
            return new MaternityPatientInLabourStateParsedData(timeLeft, roomTag);
        }

        public static MaternityPatientInLaborState GetInstance(MaternityPatientAI parent, MaternityPatientInLabourStateParsedData data)
        {
            return new MaternityPatientInLaborState(MaternityLabourRoomController.Instance.Room(data.labourRoomTag), parent, data.timeLeft);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            parent.TeleportTo(room.GetMachinePosition());
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_WO_Baby);
            parent.PlayStandIdleAnimation();
            MaternityLabourRoomObjects objects = room.GetObjects();
            if (objects != null)
            {
                objects.SpawnCover();
            }
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            switch ((StateNotifications)id)
            {
                case StateNotifications.OfficeMoved:
                    parent.TeleportTo(room.GetMachinePosition());
                    break;
                case StateNotifications.OfficeAnchored:
                    parent.TeleportTo(room.GetMachinePosition());
                    break;
                default:
                    break;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (data.timeLeft <= 0)
            {
                parent.Person.State = new MaternityPatientLaborFinishedState(room, parent);
            }
        }

    }

    public class MaternityPatientInLabourStateParsedData
    {
        public float timeLeft;
        public string labourRoomTag;

        public MaternityPatientInLabourStateParsedData(float timeLeft, string labourRoomTag)
        {
            this.timeLeft = timeLeft;
            this.labourRoomTag = labourRoomTag;
        }
    }
}