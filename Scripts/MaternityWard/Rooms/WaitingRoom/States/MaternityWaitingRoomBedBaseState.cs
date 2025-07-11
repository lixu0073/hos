using Maternity.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.WaitingRoom.Bed.State
{
    public abstract class MaternityWaitingRoomBedBaseState : BedIState
    {
        protected MaternityWaitingRoomBed parent;
        protected MaternityWaitingRoomBed.State state;

        public MaternityWaitingRoomBedBaseState(MaternityWaitingRoomBed parent)
        {
            this.parent = parent;
        }

        public virtual void BroadcastData()
        {
            
        }

        public virtual void EmulateTime(TimePassedObject timePassed)
        {
            
        }

        public MaternityWaitingRoomBed.State GetTag()
        {
            return state;
        }

        public virtual void Notify(int id, object parameters)
        {

        }

        public virtual void OnEmulationEnded()
        {

        }

        public virtual void OnEnter()
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual string SaveToString()
        {
            return state.ToString();
        }

        public virtual MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return null;
        }

    }
}