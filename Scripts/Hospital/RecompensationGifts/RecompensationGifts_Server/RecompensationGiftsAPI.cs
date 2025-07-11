using System;
using System.Collections.Generic;
using UnityEngine;
using Hospital.Connectors;

namespace Hospital
{
    public class RecompensationGiftsAPI : MonoBehaviour
    {
        #region static
        private static RecompensationGiftsAPI instance;
        public static RecompensationGiftsAPI Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("No instance of RecompensationGiftsAPI");
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogError("Multiple instances of RecompensationGiftsAPI");
            }
            instance = this;
        }
        #endregion

        public async void FetchRecompensationGifts(Action<List<RecompensationGiftModel>> onSuccess, Action<Exception> onFailure)
        {
            try
            {
                var result = await RecompensationGiftConnector.QueryAndGetRemainingAsync(CognitoEntry.SaveID);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void DeleteRecompensationGifts(List<RecompensationGiftClientModel> clientModelGifts, Action onSuccess, Action<Exception> onFailure)
        {
            if (clientModelGifts.Count == 0)
                return;

            List<RecompensationGiftModel> giftsToRemove = new List<RecompensationGiftModel>();
            for (int i = 0; i < clientModelGifts.Count; i++)
            {
                giftsToRemove.Add(new RecompensationGiftModel() { UserID = CognitoEntry.SaveID, TransactionID = clientModelGifts[i].TransactionID.ToString() });
            }

            try
            {
                await RecompensationGiftConnector.DeleteAsync(giftsToRemove);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }
    }
}
