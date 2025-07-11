using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{

    public abstract class DisconnectProviderUseCase : BaseUseCase
    {
        public delegate void OnSuccess();

        private OnSuccess onSuccess = null;

        public void Execute(OnSuccess OnSuccess = null, OnFailure OnFailure = null)
        {
            onSuccess = OnSuccess;
            onFailure = OnFailure;
            OnExecute();
        }

        protected abstract void OnExecute();

        protected void PostUser()
        {
            if(AccountManager.Instance.UserModel == null)
            {
                Debug.LogError("user model can not be null");
                return;
            }
            AccountManager.Instance.PostUserModel(AccountManager.Instance.UserModel, () =>
            {
                onSuccess?.Invoke();
            }, onFailure);
        }

        public override void UnbindCallbacks()
        {
            onSuccess = null;
            onFailure = null;
        }
    }
}
