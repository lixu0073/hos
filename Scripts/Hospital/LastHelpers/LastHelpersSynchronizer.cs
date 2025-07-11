using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastHelpersSynchronizer
{

    #region Static

    private static LastHelpersSynchronizer instance = null;

    public static LastHelpersSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new LastHelpersSynchronizer();

            return instance;
        }
    }

    #endregion

    private List<string> dataToSave = new List<string>();
    public LinkedList<IFollower> data = new LinkedList<IFollower>();

    public static int GetMaxLastHelpers()
    {
        return 10;
    }

    public BaseUserModel GetBaseUserModelBySaveID(string SaveID)
    {
        foreach(BaseUserModel userModel in data)
        {
            if(userModel.GetSaveID() == SaveID)
            {
                return userModel;
            }
        }
        return null;
    }

    public void LoadFromList(List<string> unparsedList)
    {
        data.Clear();
        if (unparsedList != null)
        {
            foreach (string saveID in unparsedList)
            {
                data.AddLast(new BaseUserModel(saveID));
            }
        }
        LastHelpersProvider.Instance.OnDataLoad();
    }

    public List<string> SaveToString()
    {
        dataToSave.Clear();
        foreach (BaseUserModel userModel in data)
        {
            dataToSave.Add(userModel.GetSaveID());
        }
        return dataToSave;
    }

}
