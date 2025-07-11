using System;
using System.Linq;
using UnityEngine;

namespace Hospital
{
    public class HostBarFriendshipController : MonoBehaviour
    {
        private const string MAILBOX_REQUEST_CANCELED = "SOCIAL_MAILBOX_REQUEST_CANCELED";
        private const string REQUEST_DENIED = "SOCIAL_REQUEST_DENIED";
        private const string REQUEST_ACCEPTED = "SOCIAL_REQUEST_ACCEPTED";
        private const string REQUEST_SENT = "SOCIAL_REQUEST_SENT";
#pragma warning disable 0649
        [SerializeField] private HostBarFriendshipView friendshipView;
        [SerializeField] FriendManagementView friendManagementView;
#pragma warning restore 0649

        private AccountManager accountManager;
        private InGameFriendsProvider igfController;
        private IFollower currentIGF;
        private bool isFacebookFriend = false;

        public void OnEnable()
        {
            friendManagementView = UIController.getHospital.friendManagementView;
            igfController = ReferenceHolder.Get().inGameFriendsProvider;
            accountManager = AccountManager.Instance;
            isFacebookFriend = accountManager.FbFriends
                .Any(
                    x =>
                    x.SaveID == SaveLoadController.SaveState.ID);

            ChangeFriendButtonStatus();

            InGameFriendsProvider.OnInGameFriendsChange += ChangeFriendButtonStatus;
        }

        public void setActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void OnDisable()
        {
            InGameFriendsProvider.OnInGameFriendsChange -= ChangeFriendButtonStatus;
        }

        private void ChangeFriendButtonStatus()
        {
            currentIGF = igfController.GetFriendById(SaveLoadController.SaveState.ID);
            friendshipView.FriendshipStatusIndicatorEnabled = true;

            if (isFacebookFriend || SaveLoadController.SaveState.ID == SaveLoadController.WISE)            
                friendshipView.FriendshipStatusIndicatorEnabled = false;

            else if (CanInvite())
            {
                friendshipView.SetFriendshipStatusPlus();
                friendshipView.FriendshipStatus = InviteFriend;
            }
            else if (IsRequestSentByMe())
            {
                friendshipView.SetFriendshipStatusSentByYou();
                friendshipView.FriendshipStatus = ConfirmRevokeInvite;
                friendManagementView.SetUnfriendUI(currentIGF);
            }
            else if (IsRequestSentToMe())
            {
                friendshipView.SetFriendshipStatusSentToYou();
                friendshipView.FriendshipStatus = ConfirmInvite;
                friendManagementView.SetRequestUI(currentIGF);
            }
            else
            {
                friendshipView.SetFriendshipStatusFriends();
                friendshipView.FriendshipStatus = ConfirmRemoveFriend;
                friendManagementView.SetUnfriendUI(currentIGF);
            }
        }

        private bool IsRequestSentByMe()
        {
            return currentIGF.InGameFriendData.CanIRevoke();
        }

        private bool IsRequestSentToMe()
        {
            return currentIGF.InGameFriendData.CanIAccept();
        }

        private bool CanInvite()
        {
            return currentIGF == null;
        }

        private void ConfirmRemoveFriend()
        {
            ConfirmRejectInvite();
            friendManagementView.Accept = RemoveFriend;
        }

        private void ConfirmRevokeInvite()
        {
            ConfirmRejectInvite();
            friendManagementView.Accept = RevokeRejectFriend;
        }

        private void ConfirmRejectInvite()
        {
            StartCoroutine(friendManagementView.Open(true, false, () =>
            {
                friendManagementView.Reject = ClosePopUp;
                friendManagementView.Accept = RevokeRejectFriend;
            }));
        }

        private void ConfirmInvite()
        {
            StartCoroutine(friendManagementView.Open(true, false, () =>
            {
                friendManagementView.Reject = InviteRejectFriend;
                friendManagementView.Accept = AcceptFriend;
            }));
        }

        private void RemoveFriend()
        {
            RejectFriend();
        }

        private void RevokeRejectFriend()
        {
            RejectFriend();
            MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(MAILBOX_REQUEST_CANCELED));
        }

        private void InviteRejectFriend()
        {
            RejectFriend();
            MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(REQUEST_DENIED));
        }

        private void RejectFriend()
        {
            igfController.RevokeInviteSentByMe(currentIGF, ChangeFriendButtonStatus, IndicateFriendFailure);
            ClosePopUp();
        }

        private void InviteFriend()
        {
            MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(REQUEST_SENT));
            igfController.SendInvitation(SaveLoadController.SaveState.ID, ChangeFriendButtonStatus, IndicateFriendFailure);
            ClosePopUp();
        }

        private void AcceptFriend()
        {
            igfController.AcceptInvite(currentIGF, ChangeFriendButtonStatus, IndicateFriendFailure);
            MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(REQUEST_ACCEPTED));
            ClosePopUp();
        }

        public void ClosePopUp()
        {
            friendManagementView.Exit();
        }

        private void IndicateFriendFailure(Exception ex)
        {
            Debug.LogError("Friend failure " + ex.Message);
            friendshipView.SetFriendshipStatusPlus();
        }
    }
}
