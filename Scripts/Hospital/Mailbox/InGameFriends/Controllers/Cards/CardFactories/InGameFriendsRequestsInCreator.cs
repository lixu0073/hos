using System;
using UnityEngine;

namespace Hospital
{
    public class InGameFriendsRequestsInCreator : InGameFriendsRequestCreatorBase
    {
#pragma warning disable 0649
        [SerializeField] MailboxFriendsTabView view;
#pragma warning restore 0649
        InGameFriendsProvider igfProvider;

        protected override void OnInitalize()
        {
            igfProvider = ReferenceHolder.Get().inGameFriendsProvider;
        }

        protected override void SubscribeToEvents()
        {
            InGameFriendsProvider.OnInGameFriendsChange += OnChange;
        }

        protected override void UnsubscribeFromEvents()
        {
            InGameFriendsProvider.OnInGameFriendsChange -= OnChange;
        }

        protected override void GetList()
        {
            friendsRequests = igfProvider.GetPendingInvitationsSendByOthers();
            view.SetNotificationBadgeActive(friendsRequests.Count > 0);
        }

        protected override void AdditionalSetup()
        {
            view.EmptInboxTextAvailable = friendsRequests.Count == 0;
        }

        protected override Type GetCardType()
        {
            return typeof(InGameFriendInCardController);
        }
    }
}