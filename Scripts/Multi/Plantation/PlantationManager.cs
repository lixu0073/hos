using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Hospital.Connectors;

namespace Hospital
{
    public class PlantationManager : MonoBehaviour
    {

        private static PlantationManager instance;
        public float refreshPlantsDuration;

        public bool debugMode;

        #region Delegates
        public delegate void PlantsSuccessCallback(List<HelpRequest> requests);

        public delegate void FailureCallback(Exception exception);
        #endregion

        private PlantsSuccessCallback OnSuccess;
        private FailureCallback OnFailure;
        private string CurrentUserId;

        private Coroutine userPlantsSynchronizerCoroutine;

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of PlantationManager on scene!");
            }
            instance = this;
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public static PlantationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("There is no PlantationManager instance on scene!");
                }
                return instance;
            }
        }

        public bool isRefreshing()
        {
            return userPlantsSynchronizerCoroutine != null;
        }

        public void unbindAllCallbacks()
        {
            OnSuccess = null;
            OnFailure = null;
            if (userPlantsSynchronizerCoroutine != null)
            {
                try { 
                    StopCoroutine(userPlantsSynchronizerCoroutine);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                userPlantsSynchronizerCoroutine = null;
            }
        }

        public void bindCheckingForHelpCallback(string userId, PlantsSuccessCallback onSuccess, FailureCallback onFailure)
        {
            unbindAllCallbacks();
            OnSuccess = onSuccess;
            OnFailure = onFailure;
            CurrentUserId = userId;
            userPlantsSynchronizerCoroutine = StartCoroutine(userPlantsSynchronizer());
        }

        IEnumerator userPlantsSynchronizer()
        {
            while (true)
            {
                yield return new WaitForSeconds(refreshPlantsDuration);
                refreshPlantationHelp();
            }
        }

        private void refreshPlantationHelp()
        {
            if(debugMode)
            {
                Debug.Log("Refresh plants");
            }
            GetUserRequests(CurrentUserId, (x) =>
            {
                OnSuccess?.Invoke(x);
            }, (ex) =>
            {
                if (debugMode)
                {
                    Debug.LogError("Failure refresh plants help stats: "+ex.Message);
                }
                OnFailure?.Invoke(ex);
            }
            );
        }

        public async void PostHelpRequest(string placeID, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                var request = new HelpRequest() { UserID = CognitoEntry.SaveID, PlaceID = placeID, helped = false };
                await HelpRequestConnector.SaveAsync(request);
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PlantationHelpManipulation);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
                Debug.LogError("Could not save help request: " + e.Message);
            }
        }

        public async void FullfillHelpRequest(string saveID, string placeID, OnSuccess onSuccess, OnFailure onFailure = null)
        {
            try
            {
                var result = await HelpRequestConnector.LoadAsync(saveID, placeID);
                if (result != null)
                {
                    result.ByWhom = CognitoEntry.SaveID;
                    result.helped = true;
                    try
                    {
                        await HelpRequestConnector.SaveAsync(result);
                        onSuccess?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Could not save fulfilled help request: " + e.Message);
                        onFailure?.Invoke(e);
                    }
                }
                else
                {
                    // TODO: Reward visitor player even if the owner of plantation has fullfiled help request by himself seconds before visitor did.
                    onSuccess?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not load help request: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public async void DeleteHelpRequest(string placeID, OnSuccess onSuccess = null, OnFailure onFailure = null)
        {
            try
            {
                await HelpRequestConnector.DeleteAsync(CognitoEntry.SaveID, placeID);
                SaveSynchronizer.Instance.MarkToSave(SavePriorities.PlantationHelpManipulation);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Could not delete help request: " + e.Message);
                onFailure?.Invoke(e);
            }
        }

        public void GetMyRequests(PlantsSuccessCallback onSuccess)
        {
            GetUserRequests(CognitoEntry.SaveID, onSuccess);
        }

        public async void GetUserRequests(string userID, PlantsSuccessCallback onSuccess, OnFailure onFailure = null)
        {
            try
            {
                var result = await HelpRequestConnector.QueryAndGetRemainingAsync(userID);
                GetPublicSavesForHelpRequests(result, onSuccess, onFailure);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

        private async void GetPublicSavesForHelpRequests(List<HelpRequest> helpRequests, PlantsSuccessCallback onSuccess, OnFailure onFailure)
        {
            List<string> tempHelpRequests = new List<string>();
            for (int i = 0; i < helpRequests.Count; ++i)
            {
                if (helpRequests[i].helped && !string.IsNullOrEmpty(helpRequests[i].ByWhom) && !tempHelpRequests.Contains(helpRequests[i].ByWhom))
                {
                    tempHelpRequests.Add(helpRequests[i].ByWhom);
                }
            }
            if (tempHelpRequests.Count == 0)
            {
                onSuccess?.Invoke(helpRequests);
                return;
            }
            try
            {
                var results = await PublicSaveConnector.BatchGetAsync(tempHelpRequests);
                if (results == null)
                    onSuccess?.Invoke(helpRequests);
                else
                    BindSavesToHelpRequests(helpRequests, results, onSuccess, onFailure);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
                Debug.LogError("Could not get public saves: " + e.Message);
            }
        }

        private void BindSavesToHelpRequests(List<HelpRequest> helpRequests, List<PublicSaveModel> saves, PlantsSuccessCallback onSuccess, OnFailure onFailure)
        {
            for (int i = 0; i < helpRequests.Count; ++i)
            {
                for (int j = 0; j < saves.Count; ++j)
                {
                    if (helpRequests[i].ByWhom == saves[j].SaveID)
                    {
                        helpRequests[i].user = saves[j];
                        break;
                    }
                }
            }
            onSuccess?.Invoke(helpRequests);
        }
    }
}