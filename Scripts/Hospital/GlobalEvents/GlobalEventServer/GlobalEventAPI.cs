using Hospital;
using Hospital.Connectors;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalEventAPI
{
    public delegate void OnSuccessContributorsGet(List<Contributor> contributors);
    public delegate void OnSuccessContributorModelsGet(List<ContributorModel> contributors);
    public delegate void OnSuccessContributorModelGet(ContributorModel contributor);

    private static DateTime LastContributorsRefresh;
    private static List<Contributor> TopContributors = null;
    //private static List<Contributor> TopPastContributors = null;
    private static readonly int RefreshIntervalInSeconds = 60;

    #region API
    public static async void SynchronizeContribution(string eventID, int score, OnSuccess onSuccess, OnFailure onFailure)
    {
        try
        {
            await ContributorConnector.SaveAsync(new ContributorModel()
            {
                EventID = eventID,
                SaveID = CognitoEntry.SaveID,
                amount = score
            });
            onSuccess?.Invoke();
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }

    public static void GetTopContributorsForActiveEvent(string eventID, OnSuccessContributorsGet onSuccess, OnFailure onFailure, int limit = 10)
    {
        if(GetContributorsFromServer())
        {
            GetContributors(eventID, limit, (contributorsModlels) => {
                BindPublicSaves(MapContributors(contributorsModlels), (contributors) => {
                    LastContributorsRefresh = DateTime.UtcNow;
                    TopContributors = contributors;
                    onSuccess?.Invoke(TopContributors);
                }, onFailure);
            }, (ex) => {
                Debug.LogError(ex.Message);
                onFailure?.Invoke(ex);
            });
        }
        else
        {
            onSuccess?.Invoke(TopContributors);
        }
    }

    public static void GetTopContributorsForPreviousEvent(string eventID, OnSuccessContributorsGet onSuccess, OnFailure onFailure, int limit = 10)
    {
        if (GetPastContibutorsFromServer(eventID))
        {
            GetContributors(eventID, limit, (contributorsModels) => {
                CachePastContributors(eventID, contributorsModels);
                BindPublicSaves(MapContributors(contributorsModels), (contributors) => {
                    onSuccess?.Invoke(contributors);
                }, onFailure);
            }, (ex) => {
                Debug.LogError(ex.Message);
                onFailure?.Invoke(ex);
            });
        }
        else
        {
            List<ContributorModel> models = GetCachePastContributors(eventID);
            if (models == null)
            {
                onSuccess?.Invoke(null);
            }
            else
            {
                BindPublicSaves(MapContributors(models), (contributors) => {
                    onSuccess?.Invoke(contributors);
                }, onFailure);
            }
        }
    }
    #endregion

    #region Methods
    private static List<Contributor> FakeContributors(List<Contributor> contributors, int limit, int targetSum)
    {
        return ContributorGenerator.Fake(contributors, limit, targetSum);
    }

    private static List<ContributorModel> GetCachePastContributors(string eventID)
    {
        string unparsedList = PlayerPrefs.GetString("GE_" + eventID, "");
        if (string.IsNullOrEmpty(unparsedList))
            return null;
        List<ContributorModel> models = new List<ContributorModel>();
        foreach (string unparsedData in unparsedList.Split('#'))
        {
            try
            {
                models.Add(ContributorModel.GetInstance(unparsedData));
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }
        return models;
    }

    private static void CachePastContributors(string eventID, List<ContributorModel> contributorModels)
    {
        string unparsedData = "";
        bool first = true;
        foreach(ContributorModel model in contributorModels)
        {
            if (!first)
                unparsedData += "#";
            unparsedData += model.toString();
            first = false;
        }
        PlayerPrefs.SetString("GE_" + eventID, unparsedData);
        PlayerPrefs.Save();
    }

    private static bool GetPastContibutorsFromServer(string eventID)
    {
        return string.IsNullOrEmpty(PlayerPrefs.GetString("GE_" + eventID, ""));
    }

    private static List<Contributor> MapContributors(List<ContributorModel> list)
    {
        List<Contributor> contributors = new List<Contributor>();
        foreach(ContributorModel model in list)
        {
            contributors.Add(new Contributor(model.SaveID, model.amount));
        }
        return contributors;
    }

    public static void BindPublicSaves(List<Contributor> data, OnSuccessContributorsGet onSuccess, OnFailure onFailure)
    {
        if (data.Count == 0)
        {
            onSuccess?.Invoke(data);
            return;
        }
        List<CacheManager.IGetPublicSave> ids = new List<CacheManager.IGetPublicSave>();
        foreach (Contributor contributor in data)
        {
            ids.Add(contributor);
        }
        CacheManager.BatchPublicSavesWithResults(ids, (saves) =>
        {
            foreach (Contributor contributor in data)
            {
                foreach (PublicSaveModel save in saves)
                {
                    if (save.SaveID == contributor.GetSaveID())
                        contributor.SetSave(save);
                }
            }
            onSuccess?.Invoke(data);
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
            onFailure?.Invoke(ex);
        });
    }

    private static bool GetContributorsFromServer()
    {
        return TopContributors == null || TopContributors.Count == 0 || DateTime.UtcNow > LastContributorsRefresh.AddSeconds(RefreshIntervalInSeconds);
    }

    public static async void GetContributors(string eventID, int limit, OnSuccessContributorModelsGet onSuccess, OnFailure onFailure)
    {
        try
        {
            var result = await ContributorConnector.FromQueryAndGetNextSetAsync(eventID, limit);
            onSuccess?.Invoke(result == null ? new List<ContributorModel>() : result);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            onFailure?.Invoke(e);
        }
    }

    public static async void GetSingleContribution(string eventID, string saveID, OnSuccessContributorModelGet onSuccess, OnFailure onFailure)
    {
        try
        {
            var result = await ContributorConnector.LoadAsync(eventID, saveID);
            onSuccess?.Invoke(result);
        }
        catch (Exception e)
        {
            onFailure?.Invoke(e);
        }
    }
    #endregion
}
