using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityWardSceneLoader : BaseSceneLoader
{
    protected override void Loaded()
    {
        if(GlobalData.Instance().HomeSave == null)
        {
           FetchHomeSave();
        }
        else
        {
            OnSaveReceive();
        }
    }

    private void FetchHomeSave()
    {
        AccountManager.Instance.GetSave(DeveloperParametersController.Instance().parameters.GetTestCognito(), (save) => {
            GlobalData.Instance().HomeSave = save;
            OnSaveReceive();
        }, (ex) => {
            Debug.LogError("Cant load save from AWS: " + ex.Message);
        });
    }

    private void OnSaveReceive()
    {
        if (GlobalData.Instance().HomeSave == null)
        {
            Debug.LogError("No home save set!");
            return;
        }
        //MaternityAreasMapController.MaternityMap.save = GlobalData.Instance().HomeSave;
        //MaternityAreasMapController.MaternityMap.ReloadGame(MaternityAreasMapController.MaternityMap.save);
        //AreaMapController.Map.ReloadGame(GlobalData.Instance().HomeSave);
    }

}
