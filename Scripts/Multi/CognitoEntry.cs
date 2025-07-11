using Amazon;
using Amazon.CognitoIdentity;
using System;
using UnityEngine;

namespace Hospital
{
    public static class CognitoEntry
    {
        public static string UserID { get; private set; }
        public static string SaveID { get; set; }

        public delegate void onRetrieval(string hash);
        public static event onRetrieval OnUserIDRetrieval;
        public static event onRetrieval OnSaveIDRetrieval;

        private const string IdentityPoolID = "eu-west-1:ccf93a26-04d0-4dbd-bbfc-bcfc1c2596d5";
        private const string DevIdentityPoolID = "eu-west-1:aa42d234-e551-4b62-9e56-83c269a112eb";
        private static readonly string CognitoPoolRegionName = RegionEndpoint.EUWest1.SystemName;

        public static string GetIdentityPoolId()
        {
            return DeveloperParametersController.Instance().parameters.DevelopAWS ? DevIdentityPoolID : IdentityPoolID;
        }

        private static RegionEndpoint CognitoPoolRegion
        {
            get { return RegionEndpoint.GetBySystemName(CognitoPoolRegionName); }
        }

        private static CognitoAWSCredentials _credentials;
        public static CognitoAWSCredentials Credentials
        {
            get
            {
                if (_credentials == null)
                    _credentials = new CognitoAWSCredentials(GetIdentityPoolId(), CognitoPoolRegion);
                return _credentials;
            }
        }

        public static void SetSaveID(string saveID)
        {
            SaveID = saveID;
            OnSaveIDRetrieval?.Invoke(saveID);
            Debug.Log("AWS Save ID: " + SaveID);
        }

        public static async void AuthCognito()
        {
            string key = "CognitoIdentity:IdentityId:" + GetIdentityPoolId();
            try
            {
                string identityId;
                if (PlayerPrefs.HasKey(key))
                    identityId = PlayerPrefs.GetString(key);
                else
                {
                    identityId = await Credentials.GetIdentityIdAsync();
                    PlayerPrefs.SetString(key, identityId);
                }
#if UNITY_EDITOR
                identityId = DeveloperParametersController.Instance().parameters.GetTestCognito();
#endif
                UserID = identityId;

                if (DevelopSaveController.IsEnabled
                    && DevelopSaveHolder.Instance != null
                    && !string.IsNullOrEmpty(DevelopSaveHolder.Instance.ForcedCognito))
                {
                    UserID = identityId = (identityId + DevelopSaveHolder.Instance.ForcedCognito);
                }

                OnUserIDRetrieval?.Invoke(identityId);
                AnalyticsManager.Instance.LoadAnalyticsData();
                AnalyticsGeneralParameters.cognitoId = identityId;
                Debug.Log("AWS User ID: " + UserID);
            }
            catch (Exception e)
            {
                Debug.LogError("Failure AWS Auth: " + e.Message);
            }
        }
    }
}