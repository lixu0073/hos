using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    public class GetPersonalFriendCodeUseCase
    {
        private PersonalFriendCodeRestConnector.OnSuccessPFC onSuccess;

        public void Execute(PersonalFriendCodeRestConnector.OnSuccessPFC onSuccess, OnFailure onFailure)
        {
            this.onSuccess = onSuccess;
            string personalFriendCode = GameState.Get().PersonalFriendCode;
            if (!string.IsNullOrEmpty(personalFriendCode))
            {
                onSuccess?.Invoke(personalFriendCode);
            }
            else
            {
                PersonalFriendCodeRestConnector.Instance.GetPersonalFriendCode(OnCodeGet, onFailure);
            }
        }

        private void OnCodeGet(string code)
        {
            GameState.Get().PersonalFriendCode = code;
            onSuccess?.Invoke(code);
        }


    }
}
