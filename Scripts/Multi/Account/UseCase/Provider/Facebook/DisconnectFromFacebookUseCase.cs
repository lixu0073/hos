using UnityEngine;
using Facebook.Unity;

namespace Hospital
{
    public class DisconnectFromFacebookUseCase : DisconnectProviderUseCase
    {
        protected override void OnExecute()
        {
            if(FB.IsInitialized)
            {
                TryLogOut();
            }
            else
            {
                FB.Init(onInitComplete, onHideUnity);
            }
        }

        private void onInitComplete()
        {
            TryLogOut();
        }

        private void TryLogOut()
        {
            if (FB.IsLoggedIn)
            {
                FB.LogOut();
            }
            if(AccountManager.Instance.UserModel == null)
            {
                Debug.LogError("Account Manager Can Not Be Null");
                return;
            }
            AccountManager.Instance.FbFriends.Clear();
            ReferenceHolder.Get().inGameFriendsProvider.DisconnectFb();
            AccountManager.Instance.UserModel.FacebookID = null;
            AccountManager.Instance.UserModel.CurrentProvider = (int)UserModel.Providers.DEFAULT;
            AccountDataCache.Instance.DisconnectProvider(AccountDataCache.Providers.FACEBOOK);
            PostUser();
            
            AnalyticsGeneralParameters.facebookConnected = FB.IsLoggedIn;
        }

        private void onHideUnity(bool isGameShown){}

    }
}
