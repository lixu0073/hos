using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using UnityEngine;

public class MaternityScene_PublicSaveManager : IPublicSaveManager
{
    public bool CanSave()
    {
        return true;
    }

    public string GetSaveID()
    {
        return CognitoEntry.SaveID;
    }

    public void OnDestroy() {}

    public void Save(PublicSaveModel model, PublicSaveModel cachedModel) {}

    public void Start() {}
}
