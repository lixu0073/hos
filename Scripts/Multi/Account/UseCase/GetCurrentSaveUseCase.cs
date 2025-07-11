using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hospital
{
    public class GetCurrentSaveUseCase : BaseUseCase
    {
        
        public delegate void OnSuccess(Save save = null);
        public delegate void OnChooseAccount(AccountManager.UserSaveItem onProviderItem);

        private OnSuccess onSuccess = null;
        private OnChooseAccount onChooseAccount = null;

        private string userId;

        public override void UnbindCallbacks()
        {
            onSuccess = null;
            onFailure = null;
            onChooseAccount = null;
        }

        public void Execute(string UserId, OnSuccess OnSuccess, OnChooseAccount OnChooseAccount, OnFailure OnFailure)
        {
            userId = UserId;
            onSuccess = OnSuccess;
            onFailure = OnFailure;
            onChooseAccount = OnChooseAccount;
            AccountManager.Instance.GetUserModel(userId, (user) =>
            {
                if (user == null)
                {
                    AnalyticsController.instance.ReportBug("getcurrentsaveusecase_user_not_found");
                    CreateUserAndDefaultSave();
                }
                else
                {
                    AccountManager.Instance.UserModel = user;
                    DetectSpecificSaveType();
                }
            }, (ex) =>
            {
                AnalyticsController.instance.ReportBug("getcurrentsaveusecase_failure");
                onFailure?.Invoke(ex);
            });
        }

        private void CreateUserAndDefaultSave()
        {
            UserModel userModel = new UserModel();
            userModel.ID = userId;
            userModel.SaveID = userId;
            userModel.FacebookID = null;
            userModel.GameCenterID = null;
            userModel.CurrentProvider = (int)UserModel.Providers.DEFAULT;
            CognitoEntry.SetSaveID(userId);
            AccountManager.Instance.PostUserModel(userModel, () =>
            {
                AccountManager.Instance.UserModel = userModel;
                onSuccess?.Invoke();
            }, (ex) =>
            {
                AnalyticsController.instance.ReportBug("getcurrentsaveusecase_failed_to_insert");
                Debug.LogError("Failure During User Insert: " + ex.Message);
                onFailure?.Invoke(ex);
            });
        }

        private void DetectSpecificSaveType()
        {
            UserModel user = AccountManager.Instance.UserModel;
            if (user == null)
            {
                Debug.LogError("User Model Can Not Be Null");
                return;
            }
            switch(user.CurrentProvider)
            {
                case (int)UserModel.Providers.DEFAULT:
                    GetSave(user.SaveID);
                    break;
                case (int)UserModel.Providers.FACEBOOK:
                    if (user.FacebookID == null)
                        GetSave(user.SaveID);
                    else
                        GetProvider(user.FacebookID);
                    break;
                case (int)UserModel.Providers.GAMECENTER:
                    if (user.GameCenterID == null)
                        GetSave(user.SaveID);
                    else
                        GetProvider(user.GameCenterID);
                    break;
            }
        }

        private void GetProvider(string key)
        {
            AccountManager.Instance.GetProvider(key, (provider) =>
            {
                if (provider == null)
                    GetSave(AccountManager.Instance.UserModel.SaveID);
                else
                {
                    AccountManager.Instance.ProviderModel = provider;
                    RunProviderFunctionality();
                    GetSave(provider.SaveID, true);
                }
            }, (ex) =>
            {
                GetSave(AccountManager.Instance.UserModel.SaveID);
            });
        }

        private void RunProviderFunctionality()
        {
            UserModel user = AccountManager.Instance.UserModel;
            if (user == null)
            {
                Debug.LogError("User Model Can Not Be Null");
                return;
            }
            /*if (user.FacebookID != null)
            {
                //ReferenceHolder.Get().engine.AddTask(() =>
                //{
                    AccountManager.Instance.AutoConnectToFacebook(user.FacebookID);
                //});
            }*/
            if (user.GameCenterID != null)
            {
#if UNITY_IOS
                AccountManager.Instance.AutoConnectToGameCenter(user.GameCenterID);
#elif UNITY_ANDROID
                AccountManager.Instance.AutoConnectToGPG(user.GameCenterID);
#endif
            }
        }

        private void GetSave(string hash, bool fromProvider = false)
        {
            AccountManager.Instance.GetSave(hash, (save) =>
            {
                if (Input.GetKey(KeyCode.F4))
                    save = SaveLoadController.GenerateDefaultSave();

                if (save == null)
                {
                    AnalyticsController.instance.ReportBug("getcurrentsaveusecase_savenotfound");
                    CognitoEntry.SetSaveID(CognitoEntry.UserID);
                }
                else
                {
                    CognitoEntry.SetSaveID(save.ID);
                    if (fromProvider)
                        UpdateProviderData(save);
                }
                onSuccess?.Invoke(save);
            }, onFailure);
        }

        private void UpdateProviderData(Save save)
        {
            ProviderModel provider = AccountManager.Instance.ProviderModel;
            if (provider == null)
                return;

            provider.Level = save.Level;
            AccountManager.Instance.PostProvider(provider);
        }
    }

} // namespace Hospital
