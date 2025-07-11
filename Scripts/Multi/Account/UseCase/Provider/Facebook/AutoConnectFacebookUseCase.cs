using UnityEngine;
using Facebook.Unity;
using System;
using System.Collections.Generic;

namespace Hospital
{
    public class AutoConnectFacebookUseCase : BaseUseCase
    {
        
        public delegate void OnSuccessGetUserData(IFollower user);
        public delegate void OnSuccessGetFacebookFriends(List<IFollower> friends);

        private string FacebookID;
        private string InputFacebookID;

        private OnSuccessGetUserData onSuccessGetUserData;
        private OnSuccessGetFacebookFriends onSuccessGetFacebookFriends;

        public void Execute(string inputFacebookId, OnSuccessGetUserData OnSuccessGetUserData, OnSuccessGetFacebookFriends OnSuccessGetFacebookFriends, OnFailure OnFailure)
        {
            onSuccessGetUserData = OnSuccessGetUserData;
            onSuccessGetFacebookFriends = OnSuccessGetFacebookFriends;
            onFailure = OnFailure;
            InputFacebookID = inputFacebookId;
            TryToInitFacebook();
        }

        private void TryToInitFacebook()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(onInitCompleteCallback);
            }
            else if (FB.IsLoggedIn)
            {
                GetFacebookID();
#if UNITY_IOS
                FB.Mobile.SetAutoLogAppEventsEnabled(LoadingGame.IDFA_Status);
                FB.Mobile.SetAdvertiserIDCollectionEnabled(LoadingGame.IDFA_Status);
                Debug.LogError("<color=yellow>FB loggeg in - IDFAStatus: </color>" + LoadingGame.IDFA_Status);
#endif
            }
            else
            {
                RaiseNotLoggedInException();
            }
        }

        private void onInitCompleteCallback()
        {
            if (FB.IsLoggedIn)
            {
                GetFacebookID();
#if UNITY_IOS
                FB.Mobile.SetAutoLogAppEventsEnabled(LoadingGame.IDFA_Status);
                FB.Mobile.SetAdvertiserIDCollectionEnabled(LoadingGame.IDFA_Status);
                Debug.LogError("<color=yellow>FB loggeg in - IDFAStatus: </color>" + LoadingGame.IDFA_Status);
#endif
            }
            else
            {
                RaiseNotLoggedInException();
            }
        }

        private void RaiseNotLoggedInException()
        {
            Debug.Log("Not logged in");
            onFailure?.Invoke(new Exception("Not logged in"));
        }

        private void GetFacebookID()
        {
            FacebookAPIManager.GetFacebookID((facebookID) =>
            {
                FacebookID = facebookID;
                if (InputFacebookID == FacebookID)
                {
                    AccountManager.FacebookID = facebookID;
                    onSuccessGetUserData?.Invoke(null);
                    GetFriends();
                }
                else
                {
                    onFailure?.Invoke(new Exception("Wrong FacebookID"));
                }
            }, (ex) =>
            {
                Debug.LogError("Failure Get FacebookID");
                onFailure?.Invoke(ex);
            });
        }

        private void GetUserData()
        {
            GetFacebookUserDataUseCase getFacebookUserDataUseCase = new GetFacebookUserDataUseCase();
            getFacebookUserDataUseCase.Execute((user) =>
            {
                AccountManager.Instance.FbUser = user;
                onSuccessGetUserData?.Invoke(user);
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
            });
        }
        //SZmmury here load Friends
        private void GetFriends()
        {
            GetFacebookFriendsUseCase getFacebookFriendsUseCase = new GetFacebookFriendsUseCase();
            getFacebookFriendsUseCase.Execute((friends) =>
            {
                onSuccessGetFacebookFriends?.Invoke(friends);
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
            });
        }

        public override void UnbindCallbacks()
        {
            onSuccessGetUserData = null;
            onSuccessGetFacebookFriends = null;
            onFailure = null;
        }
    }
}
