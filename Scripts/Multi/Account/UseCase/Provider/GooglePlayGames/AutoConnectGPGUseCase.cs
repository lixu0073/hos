using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SocialPlatforms;

namespace Hospital
{
    public class AutoConnectGPGUseCase : ConnectGPGUseCase
    {
        protected override UserModel.Providers GetProviderKey()
        {
            return UserModel.Providers.GAMECENTER;
        }

        protected override void TryToInitGameCenter() // We keep this name even for Google Play Games because of class inheritance
        {
            Social.localUser.Authenticate(ProcessAuthentication);
        }

        void ProcessAuthentication(bool success)
        {
            if (success)
            {
                LogDebugOnSuccess();
                if (Social.localUser.id == GameCenterID)
                {
                    ProviderKey = Social.localUser.id;
                    onSuccess?.Invoke();
                }
                else
                {
                    onFailure?.Invoke(new Exception("Different user on AWS and another locally"));
                }
            }
            else
            {
                onFailure?.Invoke(new Exception("Failed to authenticate Google Play Games"));
            }
        }
        
        private void LogDebugOnSuccess()
        {
            Debug.LogError("Authentication successful");
            string userInfo = "Username: " + Social.localUser.userName + "\nUser ID: " + Social.localUser.id + "\nIsUnderage: " + Social.localUser.underage;
            Debug.LogError(userInfo);
        }

    }
} // namespace Hospital
