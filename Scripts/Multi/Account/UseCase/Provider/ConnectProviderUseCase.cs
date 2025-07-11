using UnityEngine;
using System;

namespace Hospital
{

    public abstract class ConnectProviderUseCase : BaseUseCase
    {
        public delegate void OnSuccess(Save save = null);
        public delegate void OnChooseAccount(AccountManager.UserSaveItem onProviderItem = null);

        protected string ProviderKey;
        protected OnSuccess onSuccess = null;
        protected OnChooseAccount onChooseAccount = null;
        protected bool ForceConnect;
        protected AccountManager.UserSaveItem SaveItem;

        public bool SkipCheck;

        protected abstract void Execute(OnSuccess onSuccess = null, OnChooseAccount onChooseAccout = null, OnFailure onFailure = null);

        protected void ReloadSave(Save save, ConnectProviderUseCase.OnSuccess successCallback)
        {
            ReferenceHolder.Get().engine.AddTask(() =>
            {
                CognitoEntry.SetSaveID(save.ID);
                Debug.Log("get save: " + save.ID);
                save = SaveSynchronizer.GetSaveFromCacheOrServer(save);
                save = VersionManager.Instance.UpgradeSave(save);
                if (string.IsNullOrEmpty(save.maxGameVersion)) save.maxGameVersion = save.gameVersion;
                if (SaveLoadController.SaveVersionIsNewerThanGameVerion(save.maxGameVersion))
                {
                    // Tutorial can force-close all popups and we don't want that
                    TutorialUIController.Instance.StopShowCoroutines();
                    TutorialUIController.Instance.StartCoroutine(UIController.get.alertPopUp.Open(AlertType.SAVE_HAS_MORE_RECENT_SAVE_THAN_CLIENT));
                    return;
                }
                AreaMapController.Map.ReloadGame(save, false, true);
                UIController.get.chooseAccountPopUp.Exit();
                UIController.get.LoadingPopupController.Exit();
                successCallback?.Invoke();
            });
        }

        protected void ChangeSave(string providerKey, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (!ForceConnect || SaveItem == null)
                return;

            //CognitoEntry.saveID = null;

            UserModel UserModel = AccountManager.Instance.UserModel;
            ProviderModel ProviderModel = AccountManager.Instance.ProviderModel;
            /*if(!UserModel.IsBind)
            {
                Debug.LogError("No Bind Use Case");
                // bind save from new provider
                UserModel.SaveID = SaveItem.SaveID;
                // set usermodel provider ID
                switch(GetProviderKey())
                {
                    case UserModel.Providers.FACEBOOK:
                        UserModel.FacebookID = providerKey;
                        break;
                    case UserModel.Providers.GAMECENTER:
                        UserModel.FacebookID = providerKey;
                        break;
                }
                // set current active save provider
                UserModel.CurrentProvider = (int)GetProviderKey();
                UserModel.IsBind = true;
                // reload save
                PostUserAndSaveProvider(UserModel.SaveID, onSuccess, onFailure);
                return;
            }*/
            if (ProviderModel == null)
            {
                Debug.LogError("No Provider");
                // bind but currently not connect to any providers
                if (UserModel.SaveID != SaveItem.SaveID)
                {
                    // bind save from new provider
                    UserModel.SaveID = SaveItem.SaveID;
                    // set usermodel provider ID
                    switch (GetProviderKey())
                    {
                        case UserModel.Providers.FACEBOOK:
                            UserModel.FacebookID = providerKey;
                            break;
                        case UserModel.Providers.GAMECENTER:
                            UserModel.GameCenterID = providerKey;
                            break;
                    }
                    // set current active save provider
                    UserModel.CurrentProvider = (int)GetProviderKey();
                    // reload save
                    PostUserAndSaveProvider(UserModel.SaveID, onSuccess, onFailure);
                }
                else
                    Debug.LogError("No need to reload save - currently not connected to any provider");
            }
            else
            {
                Debug.LogError("Is current Provider Set");
                Debug.LogError("ProviderModel: SaveID: " + ProviderModel.SaveID + " || Level: " + ProviderModel.Level + " || ProviderID: " + ProviderModel.ProviderID);
                Debug.LogError("SaveItem: SaveID: " + SaveItem.SaveID + " || Level: " + SaveItem.Level + " || ProviderID: " + SaveItem.ProviderID);
                // bind and connect to some provider
                if (SkipCheck || ProviderModel.SaveID != SaveItem.SaveID)
                {
                    // bind save from new provider
                    UserModel.SaveID = SaveItem.SaveID;
                    // set usermodel new provider ID
                    // delete usermodel old provider ID
                    // client disconnect form old provider
                    switch (GetProviderKey())
                    {
                        case UserModel.Providers.FACEBOOK:
                            UserModel.FacebookID = providerKey;
                            UserModel.GameCenterID = null;
                            AccountManager.Instance.DisconnectFromGameCenter(SocialEntryPoint.ChooseHospital);
                            break;
                        case UserModel.Providers.GAMECENTER:
                            UserModel.GameCenterID = providerKey;
                            UserModel.FacebookID = null;
                            AccountManager.Instance.DisconnectFromFacebook(SocialEntryPoint.ChooseHospital);
                            break;
                    }
                    // set current active save provider
                    UserModel.CurrentProvider = (int)GetProviderKey();
                    // reload save
                    PostUserAndSaveProvider(UserModel.SaveID, onSuccess, onFailure, SkipCheck);
                }
                else
                    Debug.LogError("No need to reload the save - connected to some provider");
            }
        }

        private void PostUserAndSaveProvider(string SaveID, OnSuccess onSuccess, OnFailure onFailure, bool skipCheck = false)
        {
            if (AccountManager.Instance.UserModel == null)
            {
                Debug.LogError("User model can not be null");
                return;
            }
            AccountManager.Instance.PostUserModel(AccountManager.Instance.UserModel, () =>
            {
                ProviderModel providerModel = new ProviderModel()
                {
                    ProviderID = SaveItem.ProviderID,
                    SaveID = SaveItem.SaveID,
                    Level = SaveItem.Level
                };
                AccountManager.Instance.ProviderModel = providerModel;

                if (skipCheck)
                    PostProvider(providerModel.ProviderID); // To overwrite the provider table when needed

                GetSave(SaveID, onSuccess, onFailure);

            }, onFailure);
        }

        private void GetSave(string SaveID, OnSuccess onSuccess, OnFailure onFailure)
        {
            AccountManager.Instance.GetSave(SaveID, (save) =>
            {
                CognitoEntry.SaveID = SaveID;
                onSuccess?.Invoke(save);
            }, onFailure);
        }

        private bool CanConnect(ProviderModel targetProvider)
        {
            ProviderModel currentProvider = AccountManager.Instance.ProviderModel;
            UserModel userModel = AccountManager.Instance.UserModel;
            if (userModel == null)
            {
                Debug.LogError("user model can not be null");
                throw new Exception("user_model_is_null");
            }
            //if (!userModel.IsBind)
            //{
            //return false;
            //}
            if ((currentProvider == null && userModel.SaveID != targetProvider.SaveID) ||
                (currentProvider != null && currentProvider.SaveID != targetProvider.SaveID))
                    return false;

            return true;
        }

        protected void GetProviderEntry(string providerKey, OnSuccess onSuccess = null, OnChooseAccount onChooseAccount = null, OnFailure onFailure = null)
        {
            AccountManager.Instance.GetProvider(providerKey, (provider) =>
            {
                if (provider == null)
                {
                    Debug.LogError("provider not found");
                    PostProvider(providerKey, onSuccess, onFailure);
                    return;
                }

                if (!CanConnect(provider))
                {
                    if (onChooseAccount != null)
                    {
                        AccountManager.UserSaveItem userItemSave = new AccountManager.UserSaveItem()
                        {
                            ProviderID = provider.ProviderID,
                            Level = provider.Level,
                            SaveID = provider.SaveID,
                            Provider = GetProviderKey()
                        };
                        onChooseAccount.Invoke(userItemSave);
                    }
                    return;
                }
                AccountManager.Instance.UserModel.SaveID = provider.SaveID;
                AccountManager.Instance.ProviderModel = provider;
                PostUser(providerKey, onSuccess, onFailure);
            }, onFailure);
        }

        protected abstract void UpdateProviderData();
        protected abstract UserModel.Providers GetProviderKey();

        private void PostUser(string providerKey, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (AccountManager.Instance.UserModel == null)
            {
                Debug.LogError("User model can not be null");
                return;
            }
            UpdateProviderData();
            AccountManager.Instance.PostUserModel(AccountManager.Instance.UserModel, () =>
            {
                if (onSuccess != null)
                    onSuccess.Invoke();
            }, onFailure);
        }

        private void PostProvider(string providerKey, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (AccountManager.Instance.UserModel == null)
            {
                Debug.LogError("User model can not be null");
                return;
            }
            ProviderModel providerModel = new ProviderModel();
            providerModel.ProviderID = providerKey;
            providerModel.SaveID = AccountManager.Instance.UserModel.SaveID;

            AccountManager.Instance.PostProvider(providerModel, () =>
            {
                AccountManager.Instance.ProviderModel = providerModel;
                /*if (!AccountManager.Instance.UserModel.IsBind)
                {
                    AccountManager.Instance.UserModel.IsBind = true;
                    AccountManager.Instance.UserModel.SaveID = providerModel.SaveID;
                }*/
                PostUser(providerKey, onSuccess, onFailure);
            }, onFailure);
        }

        public override void UnbindCallbacks()
        {
            ProviderKey = null;
            onSuccess = null;
            onChooseAccount = null;
        }
    }
}
