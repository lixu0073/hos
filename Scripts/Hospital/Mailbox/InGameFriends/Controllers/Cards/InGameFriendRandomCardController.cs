using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hospital
{

    public class InGameFriendRandomCardController : InGameFriendCardController
    {
        protected override void AddAdditionalBehaviours(IFollower person)
        {
            view.SetActiveAcceptButton(!IsInvited(person));
            view.SetActiveSendIcons(!IsInvited(person));
            view.SetActiveWaitingResponse(IsInvited(person));

            view.FriendAccept = SendInvite;
        }



        private void SendInvite()
        {
            view.BlockCard();
            ReferenceHolder.Get().inGameFriendsProvider.SendInvitation(person.SaveID,
                () =>
                {
                    view.SetActiveWaitingResponse(true);
                },
                (ex) =>
                {
                    Debug.LogError("Friend failure " + ex.Message);
                }
                );
        }

        private bool IsInvited(IFollower person)
        {
            return person.InGameFriendData != null;
        }
    }


}