using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hospital
{
    public class InGameFriendOutCardController : InGameFriendCardController
    {

        protected override void AddAdditionalBehaviours(IFollower person)
        {
            view.SetActiveRejectButton(true);
            view.SetActiveRejectButton(true);
            view.SetActiveWaitingResponse(true);
            view.FriendReject = RejectFriendSendMe;
        }

        private void RejectFriendSendMe()
        {
            if (WasDoubleClicked())
            {
                view.BlockCard();
                igfController.RevokeInviteSentByMe(person, null,
                (ex) => Debug.LogError(REMOVE_ERRROR + ex.Message));
            }
        }
    }
}
