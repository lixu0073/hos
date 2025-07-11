using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Facebook.MiniJSON;
using Facebook.Unity;

namespace Hospital
{
    public class GetFacebookFriendsUseCase : BaseUseCase
    {
        public delegate void OnSuccess(List<IFollower> friends);
        
        private OnSuccess onSuccess = null;

        private List<IFollower> FbFriendsList;

        public void Execute(OnSuccess OnSuccess, OnFailure OnFailure)
        {
            onSuccess = OnSuccess;
            onFailure = OnFailure;
            FB.API("/me/friends?fields=id,first_name,picture.width(100).height(100)", HttpMethod.GET, GetFriendsCallback);
        }

        private void GetFriendsCallback(IGraphResult result)
        {
            string FBresult = result.RawResult;
            var dictionary = Json.Deserialize(FBresult) as Dictionary<string, object>;
            var friendList = new List<object>();
            friendList = (List<object>)(dictionary["data"]);
            FetchDataForFriends(friendList);
        }

        private void FetchDataForFriends(List<object> friends)
        {
            FbFriendsList = new List<IFollower>();
            for (int i = 0; i < friends.Count; i++)
            {
                string friendFBID = FacebookAPIManager.GetDataValueForKey((Dictionary<string, object>)(friends[i]), "id");
                string friendName = FacebookAPIManager.GetDataValueForKey((Dictionary<string, object>)(friends[i]), "first_name");
                string urlString = FacebookAPIManager.GetDataPictureForKey((Dictionary<string, object>)(friends[i]), "picture");
                
                AccountManager.FacebookFriend friend = new AccountManager.FacebookFriend();
                friend.FacebookID = friendFBID;
                friend.Name = friendName;
                friend.AvatarURL = urlString;
                FbFriendsList.Add(friend);
            }
            GetGameSavesToFriends();
        }

        private void GetGameSavesToFriends()
        {
            if (FbFriendsList.Count == 0)
            {
                onSuccess?.Invoke(FbFriendsList);
                return;
            }
            AccountManager.Instance.FindFriendsSaves(FbFriendsList, (fbPlayers) =>
            {
                BindGameSavesToFriends(fbPlayers);
            }, onFailure);
        }

        private void BindGameSavesToFriends(List<ProviderModel> fbPlayers)
        {
            for (int i = 0; i < FbFriendsList.Count; ++i)
            {
                for(int j = 0; j < fbPlayers.Count; ++j)
                {
                    if(FbFriendsList[i].FacebookID == fbPlayers[j].ProviderID)
                    {
                        FbFriendsList[i].SaveID = fbPlayers[j].SaveID;
                        FbFriendsList[i].Level = fbPlayers[j].Level;
                        break;
                    }
                }
            }
            List<IFollower> list = new List<IFollower>();
            for(int i = 0; i < FbFriendsList.Count; ++i)
            {
                if(!string.IsNullOrEmpty(FbFriendsList[i].SaveID))
                {
                    list.Add(FbFriendsList[i]);
                }
            }
            if (list.Count == 0)
            {
                onSuccess?.Invoke(list);
            }
            else
            {
                BindPublicSavesToFriends(list);
            }
        }

        private void BindPublicSavesToFriends(List<IFollower> list)
        {
            List<CacheManager.IGetPublicSave> ids = new List<CacheManager.IGetPublicSave>();
            foreach(AccountManager.FacebookFriend friend in list)
            {
                ids.Add(friend);
            }
            CacheManager.BatchPublicSavesWithResults(ids, (saves)=>
            {
                for (int i=0; i<list.Count; ++i)
                {
                    foreach (PublicSaveModel save in saves)
                    {
                        if (save.SaveID == list[i].GetSaveID())
                        {
                            list[i].Level = save.Level;
                            list[i].HasPlantationHelpRequest = save.PlantationHelp;
                            list[i].HasEpidemyHelpRequest = save.EpidemyHelp;
                            list[i].HasTreatmentHelpRequest = save.TreatmentHelp;
                            list[i].Reward = save.BestWonItem;
                            list[i].LastActivity = save.lastActivity;
                        }
                    } 
                }
                onSuccess?.Invoke(list);
            }, onFailure);
        }

        public override void UnbindCallbacks()
        {
            onSuccess = null;
            onFailure = null;
        }
    }
}
