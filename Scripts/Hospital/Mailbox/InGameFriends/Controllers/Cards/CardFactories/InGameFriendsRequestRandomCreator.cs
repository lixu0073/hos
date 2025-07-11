using System;
using Hospital;


public class InGameFriendsRequestRandomCreator : InGameFriendsRequestCreatorBase
{
    InGameFriendsProvider igfProvider;

    protected override void OnInitalize()
    {
        igfProvider = ReferenceHolder.Get().inGameFriendsProvider;
    }

    protected override void GetList()
    {
        friendsRequests = igfProvider.GetInGameRandomFriends();
    }

    protected override void SubscribeToEvents()
    {
        InGameFriendsProvider.OnRandomsUpdate += OnChange;
    }

    protected override void UnsubscribeFromEvents()
    {
        InGameFriendsProvider.OnRandomsUpdate -= OnChange;
    }

    protected override Type GetCardType()
    { 
        return typeof(InGameFriendRandomCardController);
    }
}
