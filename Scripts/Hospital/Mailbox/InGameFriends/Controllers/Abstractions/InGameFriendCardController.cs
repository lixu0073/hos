using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    public abstract class InGameFriendCardController : BaseFriendCardController
    {
        protected InGameFriendCardView view;
        protected InGameFriendsProvider igfController;

        public override void OnViewCreate()
        {
            if(view == null)
            {
                view = gameObject.GetComponent<InGameFriendCardView>();
                igfController = ReferenceHolder.Get().inGameFriendsProvider;
            }
            base.OnViewCreate();
        }

        protected override void SetCardData(IFollower person)
        {
            view.FriendImage = person.Avatar;
            view.FriendName = person.Name;
            view.FriendLevel = person.Level.ToString();
            view.SetHelpRequests(person);
        }

        protected override void AddOnImageClick(VisitingEntryPoint visitingEntryPoint)
        {
            view.FriendImageClicked = VisitFriend;
        }

        private void VisitFriend()
        {
            if (!VisitingController.Instance.IsVisiting || (VisitingController.Instance.IsVisiting && SaveLoadController.SaveState.ID != person.GetSaveID()))
            {
                OnVisiting();
                VisitingController.Instance.Visit(person.GetSaveID());
                AnalyticsController.instance.ReportSocialVisit(entryPoint, person.GetSaveID());
            }
        }

        protected bool WasDoubleClicked()
        {
            if (person.InGameFriendData.CanIAccept())
            {
                return DoubleClicker.DoubleClick(gameObject, I2.Loc.ScriptLocalization.Get(TAP_AGAIN_KEY), I2.Loc.ScriptLocalization.Get(REQUEST_REJECTED_KEY));
            }
            return DoubleClicker.DoubleClick(gameObject, I2.Loc.ScriptLocalization.Get(TAP_AGAIN_KEY), I2.Loc.ScriptLocalization.Get(REQUEST_CANCELED_KEY));
        }

        protected override void OnVisiting()
        {
            UIController.getHospital.mailboxPopup.Exit();
        }

    }
}