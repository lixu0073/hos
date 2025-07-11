using UnityEngine;
using System;

namespace Hospital
{
    public class DisconnectFromGameCenterUseCase : DisconnectProviderUseCase
    {
        protected override void OnExecute()
        {
            if (Social.localUser.authenticated)
                UpdateUser();
            else
                onFailure?.Invoke(new Exception("not authenticated to GameCenter"));
        }

        private void UpdateUser()
        {
            AccountManager.Instance.UserModel.GameCenterID = null;
            AccountManager.Instance.UserModel.CurrentProvider = (int)UserModel.Providers.DEFAULT;
            AccountDataCache.Instance.DisconnectProvider(AccountDataCache.Providers.GAMECENTER);
            PostUser();
        }
    }
} // Namespace Hospital
