using Hospital;
using Maternity.UI;
using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientLaborFinishedState : MaternityPatientBaseState<MaternityLabourRoom>
    {
        public MaternityPatientLaborFinishedState(MaternityLabourRoom room, MaternityPatientAI parent) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.LF;
        }

        public enum INDEX
        {
            labourRoomTag = 5
        }

        public static MaternityPatientLaborFinishedStateParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            string roomTag = strs[(int)INDEX.labourRoomTag];
            return new MaternityPatientLaborFinishedStateParsedData(roomTag);
        }

        public static MaternityPatientLaborFinishedState GetInstance(MaternityPatientAI parent, MaternityPatientLaborFinishedStateParsedData data)
        {
            return new MaternityPatientLaborFinishedState(MaternityLabourRoomController.Instance.Room(data.labourRoomTag), parent);
        }

        public override void OnEnter()
        {
            room.AssignPatientToLabourRoom(parent);
            parent.TeleportTo(room.GetMotherWithBabyPosition());
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_W_Baby);
            parent.SetDefaultWalkingAnimation(AnimHash.Mother_WithBaby_Walking);
            parent.PlayStandIdleAnimation();
            MaternityLabourRoomObjects objects = room.GetObjects();
            if (objects != null)
            {
                objects.SpawnCover();
            }
        }

        public override string SaveToString()
        {
            return base.SaveToString() + "!" + room.Tag;
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.OnChildCollect:
                    OnChildCollect(parent,room);
                    break;
                case StateNotifications.OfficeMoved:
                    parent.TeleportTo(room.GetMotherWithBabyPosition());
                    break;
                case StateNotifications.OfficeAnchored:
                    parent.TeleportTo(room.GetMotherWithBabyPosition());
                    break;
                default:
                    break;
            }
        }

     

        public override void OnExit()
        {
            base.OnExit();
            room.RemovePatientFromLabourRoom();
        }

        public class Data
        {

        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return new MaternityWaitingRoomIndicatorsController.ShowChildIndicator(parent, parent.waitingRoom.bed);
        }

    }

    public class MaternityPatientLaborFinishedStateParsedData
    {
        public string labourRoomTag;

        public MaternityPatientLaborFinishedStateParsedData(string labourRoomTag)
        {
            this.labourRoomTag = labourRoomTag;
        }
    }
}