using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hospital
{

    public class InGameFriendInCardController : InGameFriendCardController
    {

        protected override void AddAdditionalBehaviours(IFollower person)
        {
            view.SetActiveRejectButton(true);
            view.SetActiveAcceptButton(true);
            view.SetActiveAcceptIcons(true);

            view.FriendAccept = AcceptFriend;
            view.FriendReject = RejectFriendSendByOther;
        }

        private void AcceptFriend()
        {
            view.BlockCard();
            igfController.AcceptInvite(person,
                () =>
                {
                    MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(REQUEST_ACCEPTED_KEY));
                },
                (ex) => Debug.LogError(ADD_ERRROR + ex.Message));
        }

        protected void RejectFriendSendByOther()
        {
            if (WasDoubleClicked())
            {
                view.BlockCard();
                igfController.RevokeInviteSentByOthers(person, null,
                (ex) => Debug.LogError(REMOVE_ERRROR + ex.Message));
            }
        }
    }
}