using System;
using System.Collections.Generic;
using UnityEngine;
using Hospital.Connectors;

namespace Hospital
{
    class PackageHelpRequestManager : MonoBehaviour
    {
        #region types
        public delegate void OnQuerySuccess(List<PackageHelpRequest> requests);

        #endregion

        #region static

        private static PackageHelpRequestManager instance;

        public static PackageHelpRequestManager Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("There is no PackageHelpDynamoConnector instance on scene!");
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null)
                Debug.LogWarning("There are possibly multiple instances of PackageHelpDynamoConnector on scene!");
            instance = this;
        }
        #endregion

        public async void PostPackageHelpRequest(string packageID, OnSuccess onSuccess = null)
        {
            try
            {
                var request = new PackageHelpRequest() { UserID = CognitoEntry.SaveID, BoxID = packageID, helped = false };
                await PackageHelpRequestConnector.SaveAsync(request);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Could not save package help request: " + e.Message);
            }
        }

        public async void FullfillPackageHelpRequest(string saveID, string packageID, OnSuccess onSuccess)
        {
            try
            {
                var result = await PackageHelpRequestConnector.LoadAsync(saveID, packageID);
                result.ByWhom = CognitoEntry.SaveID;
                result.helped = true;
                try
                {
                    await PackageHelpRequestConnector.SaveAsync(result);
                    onSuccess?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not save fulfilled package help request: " + e.Message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not load package help request: " + e.Message);
            }
        }

        public async void DeletePackageHelpRequest(string packageID)
        {
            try
            {
                await PackageHelpRequestConnector.DeleteAsync(CognitoEntry.SaveID, packageID);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not delete package help request: " + e.Message);
            }
        }

        public void GetMyPackageRequests(OnQuerySuccess onSuccess)
        {
            GetUserPackageRequests(CognitoEntry.SaveID, onSuccess);
        }

        public async void GetUserPackageRequests(string userID, OnQuerySuccess onSuccess)
        {
            try
            {
                var result = await PackageHelpRequestConnector.QueryAndGetRemainingAsync(userID);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not get package help request query: " + e.Message);
            }
        }
    }
}
