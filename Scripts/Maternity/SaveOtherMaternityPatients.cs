using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveOtherMaternityPatients : MonoBehaviour, IPatientSaver
{
    //matward to do
    public void LoadFromStringList(List<string> saveList)
    {
    }

    public List<string> SaveToStringList()
    {
        Debug.LogError("To implement");
        return null;
    }
}
