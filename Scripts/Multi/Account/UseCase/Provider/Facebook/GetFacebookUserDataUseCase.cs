using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
    public class GetFacebookUserDataUseCase : BaseUseCase
    {

        private string FacebookID;

        public delegate void OnSuccess(AccountManager.FacebookUser user);
        public delegate void OnSuccessSprite(Sprite sprite);

        private string Name = null;
        private Sprite Avatar = null;

        private bool NameDownloaded = false;
        private bool AvatarDownloaded = false;

        private OnSuccess onSuccess = null;

        public void Execute(OnSuccess OnSuccess, OnFailure OnFailure, string facebookID = null)
        {
            FacebookID = facebookID;
            onSuccess = OnSuccess;
            onFailure = OnFailure;
            FacebookAPIManager.GetUserName((name) =>
            {
                Name = name;
                NameDownloaded = true;
                CheckingForComplete();
            }, (ex) => 
            {
                Debug.LogError("Cant download user name: "+ex.Message);
                Name = null;
            }, facebookID);
            try
            {
                FacebookAPIManager.GetUserAvatar((avatar) =>
                {
                    Avatar = avatar;
                    AvatarDownloaded = true;
                    CheckingForComplete();
                }, (ex) =>
                {
                    Debug.LogError("Cant download user avatar: " + ex.Message);
                    Avatar = null;
                }, facebookID);
            }
            catch(Exception e)
            {
                Debug.LogError("Cant download user avatar: " + e.Message);
                Avatar = null;
            }
        }

        private void CheckingForComplete()
        {
            if(!NameDownloaded || !AvatarDownloaded)
            {
                return;
            }
            if(Name == null || Avatar == null)
            {
                onFailure?.Invoke(new Exception("Failure during user data downloading from facebook"));
                return;
            }
            AccountManager.FacebookUser user = new AccountManager.FacebookUser();
            user.Name = Name;
            user.Avatar = Avatar;
            user.SetIsFacebookAvatarDownloaded(true);
            user.SetIsFacebookDataDownloaded(true);

            onSuccess?.Invoke(user);
        }

        public override void UnbindCallbacks()
        {
            onSuccess = null;
            onFailure = null;
        }
    }
}
