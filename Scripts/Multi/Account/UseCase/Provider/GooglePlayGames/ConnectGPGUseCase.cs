using UnityEngine;
using System;
#if UNITY_ANDROID
using GooglePlayGames;
#endif

namespace Hospital
{
    public class ConnectGPGUseCase : ConnectGameCenterUseCase
    {
        protected override UserModel.Providers GetProviderKey()
        {
            return UserModel.Providers.GAMECENTER;
        }

        protected override void TryToInitGameCenter() // We keep this name even for Google Play Games because of inheritance structure 
        {
            Social.localUser.Authenticate(ProcessAuthentication);
        }

        void ProcessAuthentication(bool success)
        {
            if (success)
            {
                LogDebugOnSuccess();
                ProviderKey = Social.localUser.id;
                AnalyticsGeneralParameters.gamecenterId = ProviderKey;

                if (ForceConnect)
                    ForceConnectToGPG();
                else
                    Connect();
            }
            else
                onFailure?.Invoke(new Exception("Failed to authenticate Google Play Games"));

            GPGSController.Instance.Authenticated(success);
        }

        private void Connect()
        {
            GetProviderEntry(ProviderKey, (save) =>
            {
                onSuccess?.Invoke();
            }, onChooseAccount, onFailure);
        }

        private void ForceConnectToGPG()
        {
            ChangeSave(ProviderKey, (save) =>
            {
                if (save != null)
                {
                    ReloadSave(save, onSuccess);
                }
            }, onFailure);
        }

        private void LogDebugOnSuccess()
        {
            Debug.LogError("Authentication successful");
            string userInfo = "Username: " + Social.localUser.userName + "\nUser ID: " + Social.localUser.id + "\nIsUnderage: " + Social.localUser.underage;
            Debug.LogError(userInfo);
        }
    }
}
