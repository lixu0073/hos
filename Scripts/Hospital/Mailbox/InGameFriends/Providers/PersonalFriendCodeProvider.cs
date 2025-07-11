using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    public class PersonalFriendCodeProvider
    {
        public static event OnSuccess OnPersonalFriendCodeGet;
        public static event OnSuccess OnFollowerGet;


        private GetPersonalFriendCodeUseCase getPersonalFriendCode = new GetPersonalFriendCodeUseCase();
        private GetFollowerByCodeUseCase getFollowerByCode = new GetFollowerByCodeUseCase();
        private string userPfc;
        private string recentlyAddedFriendSaveId;

        public string RecentlyAdddedFriendSaveID
        {
            get
            {
                return recentlyAddedFriendSaveId;
            }
        }

        public void GetPersonalFriendCode()
        {
            getPersonalFriendCode.Execute(
                (code) =>
                {
                    userPfc = code;
                    OnPersonalFriendCodeGet?.Invoke();
                },
                (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
        }

        public void GetFollowerByFriendCode(string persoanalFriendCode)
        {
            getFollowerByCode.Execute(persoanalFriendCode,
                (saveId) =>
                {
                    recentlyAddedFriendSaveId = saveId;
                    OnFollowerGet?.Invoke();
                },
                (ex) =>
                {
                    recentlyAddedFriendSaveId = null;
                    OnFollowerGet?.Invoke();
                });
        }
        
        public IFollower GetRecentlyAddedFriend()
        {          
            return ReferenceHolder.Get().inGameFriendsProvider.GetFriendById(recentlyAddedFriendSaveId);
        }

        public string PersonalFriendCode()
        {
            return userPfc;
        }
    }
     
}
