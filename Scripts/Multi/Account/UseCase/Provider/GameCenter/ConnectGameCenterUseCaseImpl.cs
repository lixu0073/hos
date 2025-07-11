using UnityEngine;
using System;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif
namespace Hospital
{
    public class ConnectGameCenterUseCaseImpl : ConnectGameCenterUseCase
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
                ProviderKey = Social.localUser.id;
                AnalyticsGeneralParameters.gamecenterId = ProviderKey;

                if (ForceConnect)
                    ForceConnectToGameCenter();
                else
                    Connect();
            }
            else
                onFailure?.Invoke(new Exception("Failed to authenticate GameCenter"));

#if UNITY_IPHONE
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif
        }

        private void Connect()
        {
            GetProviderEntry(ProviderKey, (save) =>
            {
                onSuccess?.Invoke();
            }, onChooseAccount, onFailure);
        }

        private void ForceConnectToGameCenter()
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