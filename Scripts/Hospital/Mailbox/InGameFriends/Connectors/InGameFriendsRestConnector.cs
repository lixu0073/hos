using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Hospital;

public class InGameFriendsRestConnector : MonoBehaviour
{
    public delegate void OnSuccess(List<string> randomFriendsIds, bool forceUpdate);
    #region static

    private static InGameFriendsRestConnector instance;

    public static InGameFriendsRestConnector Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of InGameFriendsRestConnector was found on scene!");
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of Save entrypoint were found!");
        }
        else
            instance = this;
    }

    #endregion

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void GetRandomFriendsIds(OnSuccess onSucces, OnFailure onFailure, bool forceUpdate)
    {
        StartCoroutine(WaitForResponse(forceUpdate, onSucces, onFailure));
    }

 
    public IEnumerator WaitForResponse(bool forceUpdate, OnSuccess onSuccess, OnFailure onFailure)
    {
        WWW randomFriendsRequest = new WWW(DefaultConfigurationProvider.GetConfigCData().SocialRandomFriendsGetURL + Game.Instance.gameState().GetHospitalLevel());
        yield return randomFriendsRequest;
        RandomFriendsResponse response = JsonUtility.FromJson<RandomFriendsResponse>(randomFriendsRequest.text);
        if (response.IsOk())
        {
            response.ids.Remove(CognitoEntry.SaveID);
            response.ids = FriendsDataZipper.RemoveFriendsAndFb(response.ids);
            onSuccess.Invoke(response.ids, forceUpdate);
        }
        else
        {
            onFailure?.Invoke(new Exception(response.message));
        }
    }

    [Serializable]
    private class RandomFriendsResponse : MhApiResponse
    {
        public List<string> ids;
    }

 
}
