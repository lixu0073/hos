using UnityEngine;
using Facebook.Unity;
using System.Collections.Generic;
using System;

namespace Hospital
{
    public class ConnectFacebookUseCase : ConnectProviderUseCase
    {

        public delegate void OnSuccessGetParam(string facebookID);
        public delegate void OnSuccessGetUserData(AccountManager.FacebookUser user);
        public delegate void OnSuccessGetFacebookFriends(List<IFollower> friends);

        private OnSuccessGetUserData onSuccessGetUserData;
        private OnSuccessGetFacebookFriends onSuccessGetFacebookFriends;

        public void Execute(OnSuccess OnSuccess, OnChooseAccount OnChooseAccount, OnSuccessGetUserData OnSuccessGetUserData, OnSuccessGetFacebookFriends OnSuccessGetFacebookFriends, OnFailure OnFailure, bool forceConnect = false, AccountManager.UserSaveItem saveItem = null)
        {
            ForceConnect = forceConnect;
            SaveItem = saveItem;
            onSuccessGetUserData = OnSuccessGetUserData;
            onSuccessGetFacebookFriends = OnSuccessGetFacebookFriends;
            Execute(OnSuccess, OnChooseAccount, OnFailure);
        }

        protected override void Execute(OnSuccess OnSuccess = null, OnChooseAccount OnChooseAccount = null, OnFailure OnFailure = null)
        {
            onSuccess = OnSuccess;
            onChooseAccount = OnChooseAccount;
            TryToInitFacebook();
        }

        private void TryToInitFacebook()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(onInitCompleteCallback, onHideUnityCallback);
            }
            else if(FB.IsLoggedIn)
            {
                GetFacebookID();
            }
            else
            {
                FacebookLogin();
            }
        }

        private void onInitCompleteCallback()
        {
            ReferenceHolder.Get().engine.AddTask(() =>
            {
                if (FB.IsLoggedIn)
                {
                    GetFacebookID();
                }
                else
                {
                    FacebookLogin();
                }
            });
        }

        private void onHideUnityCallback(bool isGameShown)
        {
            Time.timeScale = isGameShown ? 1 : 0;
        }

        private void FacebookLogin()
        {
            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, this.AuthCallback);
        }

        private void AuthCallback(IResult result)
        {            
            if (result.Error != null)
            {
                onFailure?.Invoke(new Exception(result.Error));
                return;
            }
            if (FB.IsLoggedIn)
            {
#if UNITY_IOS                
                FB.Mobile.SetAutoLogAppEventsEnabled(LoadingGame.IDFA_Status);
                FB.Mobile.SetAdvertiserIDCollectionEnabled(LoadingGame.IDFA_Status);
                Debug.LogError("<color=yellow>FB loggeg in - IDFAStatus: </color>" + LoadingGame.IDFA_Status);
#endif
                GetFacebookID();
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
                Debug.LogError(facebookID);
                ProviderKey = facebookID;
                AccountManager.FacebookID = facebookID;
                if(ForceConnect)
                {
                    ForceConnectToFacebook();
                }
                else
                {
                    Connect();
                }
                AnalyticsGeneralParameters.facebookConnected = FB.IsLoggedIn;
                AnalyticsGeneralParameters.facebookId = AccountManager.FacebookID;
                AnalyticsGeneralParameters.UpdateFacebookEmailAttribute();
                
            }, (ex) =>
            {
                Debug.LogError("Failure Get FacebookID");
                onFailure?.Invoke(ex);
            });
        }

        private void ForceConnectToFacebook()
        {
            ChangeSave(ProviderKey, (save) =>
            {
                GetFriends();
                if(save != null)
                {
                    ReloadSave(save, onSuccess);
                }
            }, onFailure);
        }

        private void Connect()
        {
            GetProviderEntry(ProviderKey, (save) =>
            {
                //GetUserData();
                GetFriends();
                onSuccess?.Invoke();
            }, onChooseAccount, onFailure);
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

        protected override void UpdateProviderData()
        {
            if (AccountManager.Instance.UserModel != null)
            {
                AccountManager.Instance.UserModel.FacebookID = ProviderKey;
                AccountManager.Instance.UserModel.CurrentProvider = (int)UserModel.Providers.FACEBOOK;
            }
        }

        protected override UserModel.Providers GetProviderKey()
        {
            return UserModel.Providers.FACEBOOK;
        }
    }
}
