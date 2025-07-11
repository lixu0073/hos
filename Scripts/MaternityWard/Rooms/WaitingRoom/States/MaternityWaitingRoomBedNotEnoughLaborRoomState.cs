using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.WaitingRoom.Bed.State
{
    public class MaternityWaitingRoomBedNotEnoughLaborRoomState : MaternityWaitingRoomBedBaseState
    {
        public MaternityWaitingRoomBedNotEnoughLaborRoomState(MaternityWaitingRoomBed parent) : base(parent)
        {
            state = MaternityWaitingRoomBed.State.NELR;
        }

        public override void OnEnter()
        {
            bool isWaitingRoomWorking = IsWaitingRoomWorking();
            bool isLabourRoomWorking = IsLabourRoomWorking();
            if (isWaitingRoomWorking && isLabourRoomWorking)
            {
                GoToWaitingRoomBedWaitingForPatientState();
            }
            else
            {
                if (!isWaitingRoomWorking)
                {
                    MaternityWaitingRoom.Unwrap -= MaternityWaitingRoom_Unwrap;
                    MaternityWaitingRoom.Unwrap += MaternityWaitingRoom_Unwrap;
                }
                if (!isLabourRoomWorking)
                {
                    MaternityLabourRoom.Unwrap -= MaternityLabourRoom_Unwrap;
                    MaternityLabourRoom.Unwrap += MaternityLabourRoom_Unwrap;
                }
            }
        }

        private void MaternityLabourRoom_Unwrap(MaternityLabourRoom r)
        {
            if(r == parent.room.GetLabourRoom())
            {
                if(IsLabourRoomWorking() && IsWaitingRoomWorking())
                {
                    GoToWaitingRoomBedWaitingForPatientState();
                }
            }
        }

        private void MaternityWaitingRoom_Unwrap(MaternityWaitingRoom r)
        {
            if(r == parent.room)
            {
                if (IsLabourRoomWorking() && IsWaitingRoomWorking())
                {
                    GoToWaitingRoomBedWaitingForPatientState();
                }
            }
        }
        
        private void GoToWaitingRoomBedWaitingForPatientState()
        {
            if (parent.room.state == Hospital.RotatableObject.State.working && parent.room.HasWorkingLabourRoom())
            {
                parent.StateManager.State = new MaternityWaitingRoomBedOccupateRoomState(parent);
            }
        }

        private bool IsWaitingRoomWorking()
        {
            return parent.room.state == Hospital.RotatableObject.State.working;
        }

        private bool IsLabourRoomWorking()
        {
            return parent.room.HasWorkingLabourRoom();
        }

        public override void OnExit()
        {
            base.OnExit();
            MaternityWaitingRoom.Unwrap -= MaternityWaitingRoom_Unwrap;
            MaternityLabourRoom.Unwrap -= MaternityLabourRoom_Unwrap;
        }


    }
}
