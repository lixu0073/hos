using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPublicSaveManager
{
    void Start();
    void OnDestroy();
    void Save(PublicSaveModel model, PublicSaveModel cachedModel);
    bool CanSave();
    string GetSaveID();
}
