using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SocialPlatforms;

namespace Hospital
{
    public class AutoConnectGameCenterUseCase : ConnectGameCenterUseCase
    {
        protected override UserModel.Providers GetProviderKey()
        {
            return UserModel.Providers.GAMECENTER;
        }

        protected override void TryToInitGameCenter()
        {
            Social.localUser.Authenticate(ProcessAuthentication);
        }

        void ProcessAuthentication(bool success)
        {
            if (success)
            {
                LogDebugOnSuccess();
                if(Social.localUser.id == GameCenterID)
                {
                    ProviderKey = Social.localUser.id;
                    GetAchievements();
                    onSuccess?.Invoke();
                }
                else
                {
                    onFailure?.Invoke(new Exception("inny user na AWS a inny lokalnie"));
                }
            }
            else
            {
                onFailure?.Invoke(new Exception("Failed to authenticate GameCenter"));
            }
        }

        private void GetAchievements()
        {

        }

        private void LogDebugOnSuccess()
        {
            Debug.LogError("Authentication successful");
            string userInfo = "Username: " + Social.localUser.userName +
                "\nUser ID: " + Social.localUser.id +
                "\nIsUnderage: " + Social.localUser.underage;
            Debug.LogError(userInfo);
        }

    }
}