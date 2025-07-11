using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maternity
{
    class MaternityPatientDetailCardNoRequiredRoomState : MaternityPatientDetailCardBaseState
    {
        private MaternityWaitingRoomBed bed;

        public MaternityPatientDetailCardNoRequiredRoomState(MaternityWaitingRoomBed bed, MaternityPatientDetailCard card) : base(card)
        {
            this.bed = bed;
        }

        public override void OnEnter()
        {
            bed.OnStateChanged += Bed_OnStateChanged;
            base.OnEnter();
            card.Ui.SetLaborRoomRequiredView(OnCardClick);
        }

        private void Bed_OnStateChanged()
        {
            switch (bed.StateManager.State.GetTag())
            {
                case MaternityWaitingRoomBed.State.WFP:
                    card.StateManager.State = new MaternityPatientDetailCardWaitingForPatientState(bed, card);
                    break;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            bed.OnStateChanged -= Bed_OnStateChanged;
        }

    }
}
