using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using IsoEngine;
using Hospital;

public class GlobalEventSynchronizer
{
    private GlobalEventSaveData data = new GlobalEventSaveData();

    private static GlobalEventSynchronizer instance = null;

    public static GlobalEventSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new GlobalEventSynchronizer();

            return instance;
        }
    }

    public GlobalEvent GlobalEvent
    {
        set { }
        get { return data.globalEvent; }
    }

    public bool HasSeenEvent
    {
        get { return data.hasSeenEvent; }
        set { data.hasSeenEvent = value; }
    }

    public bool LastCollected
    {
        set { data.lastCollected = value; }
        get { return data.lastCollected; }
    }

    public int LastContributionMargin
    {
        set { }
        get { return data.lastContributionMargin; }
    }

    private const string FINISHED_GE_IDENTIFIER = "FINISHED";

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();

        if (data.globalEvent != null)
            builder.Append(data.globalEvent.SaveToString());
        else
            builder.Append(FINISHED_GE_IDENTIFIER);

        builder.Append("!");

        if (data.pastGlobalEventRewardPackages != null && data.pastGlobalEventRewardPackages.Count >0)
        {
			for (int i = 0; i < data.pastGlobalEventRewardPackages.Count; i++)
            {
                builder.Append(data.pastGlobalEventRewardPackages[i].Key);

                builder.Append("^");

                builder.Append(data.pastGlobalEventRewardPackages[i].Value.SaveToString());

                if (i < data.pastGlobalEventRewardPackages.Count - 1)
                    builder.Append("%");
            }
        }

        builder.Append("!");
        builder.Append(data.lastCollected.ToString());
        builder.Append("!");
        builder.Append(Checkers.CheckedAmount(data.lastContributionMargin, 0, int.MaxValue, "GlobalEvent lastContributionMargin: ").ToString());
        builder.Append("!");
        builder.Append(data.hasSeenEvent.ToString());

        return builder.ToString();
    }

    public void LoadFromString(string saveString, bool visitingMode)
    {
        if (data.globalEvent != null)
            data.globalEvent.OnDestroy();

        if (data.pastGlobalEventRewardPackages != null) {
            data.pastGlobalEventRewardPackages.Clear();
        }

        if (!visitingMode)
        {
            if (!string.IsNullOrEmpty(saveString))
            {
                var parts = saveString.Split('!');

                if (parts != null && parts.Length > 2)
                {
                    if (!string.IsNullOrEmpty(parts[0]) && !parts[0].Equals(FINISHED_GE_IDENTIFIER))
                    {
                        var typeStr = parts[0].Split(';');
                        Type type = Type.GetType(typeStr[0]);                        
                        System.Object obj = Activator.CreateInstance(type);
                        (obj as GlobalEvent).LoadFromString(parts[0]);
                        InitGlobalEvent((obj as GlobalEvent));
                    }

                    if (!string.IsNullOrEmpty(parts[1]))
                    {
                        var rewards = parts[1].Split('%');
                        if (rewards != null && rewards.Length > 0)
                        {
                            for (int i = 0; i < rewards.Length; i++)
                            {
                                string[] rewardSplit = rewards[i].Split('^');
                                KeyValuePair<string, GlobalEventRewardModel>  rewardObject = new KeyValuePair<string, GlobalEventRewardModel>(rewardSplit[0], GlobalEventRewardModel.Parse(rewardSplit[1]));
                                data.pastGlobalEventRewardPackages.Add(rewardObject);
                            }
                        }
                    }
                    data.lastCollected = bool.Parse(parts[2]);
                    data.lastContributionMargin = int.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                    data.hasSeenEvent = bool.Parse(parts[4]);
                }
            }
        }
    }

    public void InitGlobalEvent(GlobalEvent _event, int _contributionMargin = 0)
    {
        if (data.globalEvent!=null)
        {
            int contribution = data.globalEvent.PersonalProgress;
            string eventID = data.globalEvent.ID;
            if (!VisitingController.Instance.IsVisiting)
            {
                //Analytics
                GlobalEventGenerator globalEventGenerator = new GlobalEventGenerator();
                PreviousGlobalEvent prevGeEvent = globalEventGenerator.GetPreviousGlobalEvent();
                string prevGlobalEventID = CacheManager.GetPrevGlobalEventId();
                if (prevGlobalEventID != "" && prevGeEvent != null && prevGeEvent.ID == prevGlobalEventID)//true = there was a global a previous global event and it matches the data.
                {
                    int geContribution = CacheManager.GetLastGlobalEventContribution();
                    AnalyticsController.instance.ReportPlayerGlobalEventContribution(prevGlobalEventID, geContribution);
                    //Reset data to null, as we used up the data here.
                    CacheManager.SetPrevGlobalEventId("");
                    CacheManager.SetLastGlobalEventContribution();
                }
            }
            data.globalEvent.OnDestroy();
        }

        data.globalEvent = _event;
        data.lastContributionMargin = _contributionMargin;
        data.hasSeenEvent = false;

        if (data.globalEvent != null)
        {
            data.globalEvent.OnStart();
        }
    }

    public void AddGlobalEventRewardForReloadSpawn(KeyValuePair<string, GlobalEventRewardModel> reward)
    {
        data.pastGlobalEventRewardPackages.Add(reward);
    }

    public List<KeyValuePair<string, GlobalEventRewardModel>> GetGlobalEventRewardForReloadSpawn()
    {
        return data.pastGlobalEventRewardPackages;
    }

    public void SetLastCollected(bool setCollected) {
        data.lastCollected = setCollected;
    }
    public bool GetLastCollected() {
        return data.lastCollected;
    }
}
