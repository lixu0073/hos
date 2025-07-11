using System;
using Hospital.Connectors;

namespace Hospital
{
    public class GetFollowerByCodeUseCase
    {
        public delegate void OnSuccessFriendFind(string saveId);

        public async void Execute(string code, OnSuccessFriendFind onSuccess, OnFailure onFailure)
        {
            try
            {
                var result = await FriendCodesConnector.QueryAndGetRemainingAsync(code);
                if (result == null || result.Count == 0)
                {
                    onFailure?.Invoke(null);
                    return;
                }
                else
                {
                    string saveID = result[0].SaveID;
                    if (IsAlreadyFriend(saveID))
                    {
                        onSuccess?.Invoke(saveID);
                    }
                    else
                    {
                        ReferenceHolder.Get().inGameFriendsProvider.SendFriendshipAcceptance(saveID,
                            () => onSuccess?.Invoke(saveID),
                            (ex) => onFailure?.Invoke(ex));
                    }
                }
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        private bool IsAlreadyFriend(string saveId)
        {
            if (ReferenceHolder.Get().inGameFriendsProvider.GetFriendById(saveId) == null)
            {
                return false;
            }
            return ReferenceHolder.Get().inGameFriendsProvider.GetFriendById(saveId).InGameFriendData.Accepted;
        }
    }
}