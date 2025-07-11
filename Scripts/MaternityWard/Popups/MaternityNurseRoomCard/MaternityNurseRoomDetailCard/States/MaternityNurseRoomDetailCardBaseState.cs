using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public class MaternityNurseRoomDetailCardBaseState : IState
    {
        protected MaternityNurseRoomDetailCard card;

        public MaternityNurseRoomDetailCardBaseState(MaternityNurseRoomDetailCard card)
        {
            this.card = card;
        }

        public virtual void Notify(int id, object parameters) { }

        public virtual void OnEnter()
        {
            card.Ui.SetBedPanelButton(OnCardClick);
        }

        protected void OnCardClick()
        {
            card.OnCardClick();
        }

        public virtual void OnExit()
        {
            card.Ui.SetBedPanelButton(null);
        }

        public virtual void OnUpdate() { }

        public virtual string SaveToString()
        {
            return null;
        }

    }
}
