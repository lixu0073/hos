using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
    public abstract class ConnectGameCenterUseCase : ConnectProviderUseCase
    {
        
        protected string GameCenterID;

        public void Execute(OnSuccess OnSuccess, OnChooseAccount OnChooseAccount, OnFailure OnFailure, bool forceConnect = false, AccountManager.UserSaveItem saveItem = null, string gameCenterID = null)
        {
            GameCenterID = gameCenterID;
            ForceConnect = forceConnect;
            SaveItem = saveItem;
            Exec(OnSuccess, OnChooseAccount, OnFailure);
        }

        protected override void Execute(OnSuccess OnSuccess = null, OnChooseAccount OnChooseAccount = null, OnFailure OnFailure = null)
        {
            onSuccess = OnSuccess;
            onChooseAccount = OnChooseAccount;
            TryToInitGameCenter();
        }

        private void Exec(OnSuccess OnSuccess = null, OnChooseAccount OnChooseAccount = null, OnFailure OnFailure = null)
        {
            onSuccess = OnSuccess;
            onChooseAccount = OnChooseAccount;
            TryToInitGameCenter();
        }
        
        protected abstract void TryToInitGameCenter();

        protected override void UpdateProviderData()
        {
            if (AccountManager.Instance.UserModel != null)
            {
                AccountManager.Instance.UserModel.GameCenterID = ProviderKey;
                AccountManager.Instance.UserModel.CurrentProvider = (int)UserModel.Providers.GAMECENTER;
            }
        }

    }
}
