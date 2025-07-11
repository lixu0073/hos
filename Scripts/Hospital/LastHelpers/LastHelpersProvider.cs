using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class LastHelpersProvider
{

    #region Static

    private static LastHelpersProvider instance = null;

    public static LastHelpersProvider Instance
    {
        get
        {
            if (instance == null)
                instance = new LastHelpersProvider();
            return instance;
        }
    }

    #endregion
    public List<IFollower> Helpers
    {
        get
        {
            return LastHelpersSynchronizer.Instance.data.ToList();
        }
    }

    private void OnPublicSavesBind()
    {
        UIController.get.FriendsDrawer.RefeshLastHelpersList();
    }

    public void AddLastHelper(string SaveID)
    {
        if (SaveID == CognitoEntry.SaveID)
            return;
        BaseUserModel userModel = LastHelpersSynchronizer.Instance.GetBaseUserModelBySaveID(SaveID);
        if(userModel == null)
        {
            BindPublicSaveToUser(new BaseUserModel(SaveID));
        }
        else
        {
            SwitchOrder(userModel);
        }
    }

    private void BindPublicSaveToUser(BaseUserModel userModel)
    {
        LastHelpersSynchronizer.Instance.data.AddFirst(userModel);
        CacheManager.GetPublicSaveById(userModel.GetSaveID(), (publicSave) => {
            userModel.SetSave(publicSave);
            NormalizeList();
            OnPublicSavesBind();
        }, (ex) => {
            Debug.LogError(ex.Message);
        }, true);
    }

    private void NormalizeList()
    {
        int maxHelpers = LastHelpersSynchronizer.GetMaxLastHelpers();
        int currentHelpersCount = LastHelpersSynchronizer.Instance.data.Count;
        if (currentHelpersCount > maxHelpers)
        {
            for (int i = 0; i < currentHelpersCount - maxHelpers; ++i)
            {
                LastHelpersSynchronizer.Instance.data.RemoveLast();
            }
        }
    }

    private void SwitchOrder(BaseUserModel userModel)
    {
        LastHelpersSynchronizer.Instance.data.Remove(userModel);
        BindPublicSaveToUser(userModel);
    }

    public void OnDataLoad()
    {
        LinkedList<IFollower> data = LastHelpersSynchronizer.Instance.data;
        if(data.Count == 0)
        {
            OnPublicSavesBind();
            return;
        }
        List<CacheManager.IGetPublicSave> ids = new List<CacheManager.IGetPublicSave>();
        foreach (BaseUserModel userModel in data)
        {
            ids.Add(userModel);
        }
        CacheManager.BatchPublicSavesWithResults(ids, (saves) =>
        {
            foreach (BaseUserModel userModel in data)
            {
                foreach (PublicSaveModel save in saves)
                {
                    if (save.SaveID == userModel.GetSaveID())
                    {
                        userModel.SetSave(save);
                    }
                }
            }
            OnPublicSavesBind();
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
        }, true);
    }

}
