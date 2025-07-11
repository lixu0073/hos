using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData
{

    #region Static

    private static GlobalData instance;

    public static GlobalData Instance()
    {
        if (instance == null)
            instance = new GlobalData();
        return instance;
    }

    #endregion

    public string HomeUserID;
    public string HomeSaveID;

    public string VisitingSaveID;

    public Save HomeSave;
    public Save VisitingSave;

    public bool IsVisitingMode = false;

}
