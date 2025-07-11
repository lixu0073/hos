using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Hospital.TreatmentRoomHelpRequest.Backend;
using Hospital.Connectors;
using MovementEffects;
using Transactions;

namespace Hospital.TreatmentRoomHelpRequest
{
    public class TreatmentHelpAPI : MonoBehaviour
    {
        private List<TreatmentHelpPackage> requests = null;

        private List<string> addToLastHelpersSaves = new List<string>();

        private string prevSaveID = null;
        private bool pendingGetRequests = false;

        #region API

        public void AddRequest(TreatmentHelpPackage helpRequestPackage, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            AddRequestLocal(helpRequestPackage);
            TreatmentHelpPackageModel model = TreatmentRoomHelpMapper.Map(helpRequestPackage);
            TransactionManager.Instance.addRequestInTreatmentRoomTransactionController.AddTransaction(model);
            PostHelpRequest(model, () => {
                TransactionManager.Instance.addRequestInTreatmentRoomTransactionController.CompleteTransaction(model);
                onSuccess?.Invoke();
            }, onFailure);
        }

        public void DoHelp(List<TreatmentHelpCure> cureHelps, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            AddHelpersLocal(cureHelps);
            for(int i=0; i < cureHelps.Count; ++i)
            {
                TreatmentHelpCureModel model = TreatmentRoomHelpMapper.Map(cureHelps[i]);
                TransactionManager.Instance.addDonationInTreatmentRoomTransactionController.AddTransaction(model);
                PostHelpInMedicine(model,
                () => {
                    TransactionManager.Instance.addDonationInTreatmentRoomTransactionController.CompleteTransaction(model);
                    onSuccess?.Invoke();
                }, onFailure);
            }
        }

        public void GetRequests(string saveID, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (requests == null || prevSaveID != saveID)
            {
                DownloadRequests(saveID, onSuccess, onFailure);
                if (!VisitingController.Instance.IsVisiting)
                    StartSychronizeCoroutine(saveID);
            }
            else
                NotifyRequestsGet();
            prevSaveID = saveID;
        }

        public void RemoveRequest(TreatmentHelpPackage request, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            RemoveRequestLocal(request);
            TreatmentHelpPackageModel model = TreatmentRoomHelpMapper.Map(request);
            TransactionManager.Instance.removeRequestInTreatmentRoomTransactionController.AddTransaction(model);
            DeleteHelpRequest(request.SaveID, request.PatientID, () => {
                TransactionManager.Instance.removeRequestInTreatmentRoomTransactionController.CompleteTransaction(model);
            });
        }

        public void StopRefreshingRequests()
        {
            pendingGetRequests = false;
            prevSaveID = null;
            requests = null;
            Timing.KillCoroutine(RequestsSynchronizer(null).GetType());
        }

        public async void DeleteHelpRequest(string saveID, long patientID, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                await TreatmentHelpConnector.DeletePackageAsync(saveID, patientID);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void PostHelpRequest(TreatmentHelpPackageModel helpPackageModel, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                await TreatmentHelpConnector.SavePackageAsync(helpPackageModel);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        public async void PostHelpInMedicine(TreatmentHelpCureModel helpCureModel, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                await TreatmentHelpConnector.SaveCureAsync(helpCureModel);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        #endregion

        private void NotifyRequestsGet()
        {
            ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.NotifyRequestsGet(requests);
            UpdatePublicSave();
        }

        private void NotifyRequests()
        {
            TryToAddToLastHelpers();
            ReferenceHolder.GetHospital().treatmentHelpNotificationCenter.NotifyRequests(requests);
            Invoke("UpdatePublicSave", 1);
        }

        private void UpdatePublicSave()
        {
            if (VisitingController.Instance.IsVisiting)
                return;
            bool hasAnyTreatmentRoomHelpRequests = ReferenceHolder.GetHospital().treatmentRoomHelpController.IsAnyPatientHasHelpRequest();
            if((GameState.Get().HasAnyTreatmentRoomHelpRequests && !hasAnyTreatmentRoomHelpRequests) || (!GameState.Get().HasAnyTreatmentRoomHelpRequests && hasAnyTreatmentRoomHelpRequests))
            {
                GameState.Get().HasAnyTreatmentRoomHelpRequests = hasAnyTreatmentRoomHelpRequests;
                PublicSaveManager.Instance.UpdatePublicSave();
            }
            GameState.Get().HasAnyTreatmentRoomHelpRequests = hasAnyTreatmentRoomHelpRequests;
        }

        private void TryToAddToLastHelpers()
        {
            if (VisitingController.Instance.IsVisiting)
                return;
            addToLastHelpersSaves.Clear();
            for (int i = 0; i < requests.Count; ++i)
            {
                if (requests[i].Helpers != null)
                {
                    for (int j = 0; j < requests[i].Helpers.Count; ++j)
                    {
                        if (addToLastHelpersSaves.Contains(requests[i].Helpers[j].HelperSaveID))
                            continue;
                        string key = requests[i].Helpers[j].HelperSaveID + requests[i].Helpers[j].PatientID + requests[i].Helpers[j].ID;
                        if (!CacheManager.WasHelped(key))
                        {
                            addToLastHelpersSaves.Add(requests[i].Helpers[j].HelperSaveID);
                            CacheManager.AddToHelpers(key);
                        }
                    }
                }
            }
            for (int i = 0; i < addToLastHelpersSaves.Count; ++i)
            {
                try
                {
                    LastHelpersProvider.Instance.AddLastHelper(addToLastHelpersSaves[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        private async void DownloadRequests(string saveID, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            if (pendingGetRequests)
                return;
            pendingGetRequests = true;

            var helpRequestsTask = TreatmentHelpConnector.QueryAndGetRemainingPackageAsync(saveID);
            var helpersTask = TreatmentHelpConnector.QueryAndGetRemainingCureAsync(saveID);
            try
            {
                await Task.WhenAll(helpRequestsTask, helpersTask);
                bool getHelpRequestsReceived = helpRequestsTask.Status == TaskStatus.RanToCompletion;
                bool getHelpersReveived = helpersTask.Status == TaskStatus.RanToCompletion;
                if (getHelpRequestsReceived && getHelpersReveived)
                {
                    onSuccess?.Invoke();
                    requests = TreatmentRoomHelpMapper.Map(helpRequestsTask.Result, helpersTask.Result);
                    CheckingTransactions(helpRequestsTask.Result, helpersTask.Result);
                    NotifyRequests();
                    pendingGetRequests = false;
                }
            }
            catch
            {
                pendingGetRequests = false;
                if (helpRequestsTask.Exception != null)
                    onFailure?.Invoke(helpRequestsTask.Exception);
                else if (helpersTask.Exception != null)
                    onFailure?.Invoke(helpersTask.Exception);
            }
        }

        private void CheckingTransactions(List<TreatmentHelpPackageModel> requestsModels, List<TreatmentHelpCureModel> helpersModels)
        {
            List<ITransaction<TreatmentHelpPackageModel>> iRequestsModels = new List<ITransaction<TreatmentHelpPackageModel>>();
            for (int i = 0; i < requestsModels.Count; ++i)
            {
                iRequestsModels.Add(requestsModels[i]);
            }
            TransactionManager.Instance.addRequestInTreatmentRoomTransactionController.CheckingForResendActions(iRequestsModels);
            TransactionManager.Instance.removeRequestInTreatmentRoomTransactionController.CheckingForResendActions(iRequestsModels, false);

            List<ITransaction<TreatmentHelpCureModel>> iHelpersModels = new List<Transactions.ITransaction<TreatmentHelpCureModel>>();
            for (int i = 0; i < helpersModels.Count; ++i)
            {
                iHelpersModels.Add(helpersModels[i]);
            }
            TransactionManager.Instance.addDonationInTreatmentRoomTransactionController.CheckingForResendActions(iHelpersModels);
        }

        #region Local

        public void AddRequestLocal(TreatmentHelpPackage helpRequestPackage)
        {
            if(requests != null)
            {
                requests.Add(helpRequestPackage);
                NotifyRequests();
            }
        }

        public void RemoveRequestLocal(TreatmentHelpPackage request)
        {
            if (requests != null)
            {
                for (int requestIndex = 0; requestIndex < requests.Count; ++requestIndex)
                {
                    if (requests[requestIndex].SaveID == request.SaveID && requests[requestIndex].PatientID == request.PatientID)
                    {
                        requests.Remove(requests[requestIndex]);
                        NotifyRequests();
                        return;
                    }
                }
            }
        }

        public void AddHelpersLocal(List<TreatmentHelpCure> helpers)
        {
            if (requests != null)
            {
                long patientID = helpers.Count > 0 ? helpers[0].PatientID : -1;
                if (patientID == -1)
                    return;
                for (int requestIndex = 0; requestIndex < requests.Count; ++requestIndex)
                {
                    if (requests[requestIndex].PatientID == patientID)
                    {
                        requests[requestIndex].Helpers.AddRange(helpers);
                        NotifyRequests();
                        return;
                    }
                }
            }
        }

        #endregion

        private void StartSychronizeCoroutine(string saveID)
        {
            Timing.KillCoroutine(RequestsSynchronizer(saveID).GetType());
            Timing.RunCoroutine(RequestsSynchronizer(saveID));
        }

        IEnumerator<float> RequestsSynchronizer(string saveID)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(10);
                DownloadRequests(saveID);
            }
        }
    }
}