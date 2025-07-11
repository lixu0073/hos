using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CognitoTextDisplayer : MonoBehaviour
{
    public Text cognitoId;
    private void Update()
    {
        cognitoId.text = Hospital.CognitoEntry.SaveID;
    }
}
