using System.Collections;
using UnityEngine;
using Hospital;
using System;

public class PersonalFriendCodeRestConnector : MonoBehaviour
{
    public delegate void OnSuccessPFC(string personalFriendCode);

    #region static
    private static PersonalFriendCodeRestConnector instance;

    public static PersonalFriendCodeRestConnector Instance
    {
        get
        {
            if (instance == null)
                Debug.LogWarning("No instance of PersonalFriendCodeConnector was found on scene!");
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)    
            Debug.LogWarning("Multiple instances of Save entrypoint were found!");
        else
            instance = this;
    }
    #endregion

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void GetPersonalFriendCode(OnSuccessPFC onSucces, OnFailure onFailure)
    {
        StartCoroutine(WaitForResponse(onSucces, onFailure));
    }

    public IEnumerator WaitForResponse(OnSuccessPFC onSucces, OnFailure onFailure)
    {
        WWW personalCodeRequest = new WWW(DefaultConfigurationProvider.GetConfigCData().SocialPersonalFriendCodeURL + CognitoEntry.SaveID);
        yield return personalCodeRequest;
        PersonalFriendCodeRespones response = JsonUtility.FromJson<PersonalFriendCodeRespones>(personalCodeRequest.text);
        if (response.IsOk())        
            onSucces.Invoke(response.code);
        else        
            onFailure?.Invoke(new Exception(response.message));
    }

    [Serializable]
    private class PersonalFriendCodeRespones : MhApiResponse
    {
#pragma warning disable 0649
        public string code;
#pragma warning restore 0649
    }
}
