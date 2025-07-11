using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public class MaternityNurseRoomDetailCardWaitingForPatientState : MaternityNurseRoomDetailCardBaseState
    {

        private MaternityWaitingRoomBed bed;

        public MaternityNurseRoomDetailCardWaitingForPatientState(MaternityWaitingRoomBed bed, MaternityNurseRoomDetailCard card) : base(card)
        {
            this.bed = bed;
        }

        public override void OnEnter()
        {
            bed.OnStateChanged += Bed_OnStateChanged;
            base.OnEnter();
            card.Ui.SetWaitingForNextPatientView(OnCardClick);
            bed.OnDataReceived_WFP += Bed_OnDataReceived_WFP;
            bed.StateManager.State.BroadcastData();
        }

        private void Bed_OnDataReceived_WFP(WaitingRoom.Bed.State.MaternityWaitingRoomBedWaitingForPatientState.Data data)
        {
            card.Ui.SetPatientTimer(UIController.GetFormattedShortTime((int)Math.Ceiling(data.timeLeft)));
        }

        private void Bed_OnStateChanged()
        {
            switch (bed.StateManager.State.GetTag())
            {
                case MaternityWaitingRoomBed.State.OR:
                    card.StateManager.State = new MaternityNurseRoomDetailCardPatientInBedState(bed, bed.GetPatient(), card);
                    break;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            bed.OnStateChanged -= Bed_OnStateChanged;
            bed.OnDataReceived_WFP -= Bed_OnDataReceived_WFP;
        }

    }
}
