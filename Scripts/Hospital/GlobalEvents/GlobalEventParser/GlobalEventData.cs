using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventData{

    public string Id { get; private set; }

    public CollectOnMapGEGraphicsManager.GlobalEventTypes Type { get; private set; }

    public int[] PersonalGoals { get; private set; }
    public string[] PersonalGoalsRewards { get; private set; }
    public string[][] ContributionRewards { get; private set; }
    public string FirstPlaceReward { get; private set; }
    public string SecondPlaceReward { get; private set; }
    public string ThirdPlaceReward { get; private set; }
    public string LastReward { get; private set; }

    public string[] SingleContributionRewards { get; private set; }

    public int MinLevel { get; private set; }

    public GlobalEventOtherParamsCData OtherParameters { get; private set; }

    public int GlobalEventStartTime { get; private set; } // epoch

    public int GlobalEventEndTime { get; private set; } // epoch

    public string EventDescription { get; private set; }

    public string EventTitle { get; private set; }

    public GlobalEventData(SingleGlobalEventCData eventConfig)
    {
        try
        {
            Type = (CollectOnMapGEGraphicsManager.GlobalEventTypes)Enum.Parse(typeof(CollectOnMapGEGraphicsManager.GlobalEventTypes), eventConfig.Type);
        }
        catch (ArgumentException)
        {
            Type = CollectOnMapGEGraphicsManager.GlobalEventTypes.NoActive;
            Debug.LogError("Invalid GE type!");
        }

        GlobalEventStartTime = (int)eventConfig.StartDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        GlobalEventEndTime = (int)eventConfig.EndDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        EventDescription = eventConfig.DescriptionTerm;
        EventTitle = eventConfig.TitleTerm;

        OtherParameters = eventConfig.OtherParams;
        MinLevel = eventConfig.MinLevel;

        PersonalGoals = new int[eventConfig.Goals.Count];
        for (int i = 0; i < eventConfig.Goals.Count; ++i)
        {
            PersonalGoals[i] = eventConfig.Goals[i].Goal;
        }

        PersonalGoalsRewards = new string[eventConfig.Goals.Count];
        for (int i = 0; i < eventConfig.Goals.Count; ++i)
        {
            PersonalGoalsRewards[i] = eventConfig.Goals[i].Reward;
        }

        ContributionRewards = new string[eventConfig.Goals.Count][];
        for (int i = 0; i < eventConfig.Goals.Count; ++i)
        {
            ContributionRewards[i] = new string[eventConfig.Goals[i].ContributionRewards.Length];
            for (int j = 0; j < eventConfig.Goals[i].ContributionRewards.Length; ++j)
            {
                ContributionRewards[i][j] = eventConfig.Goals[i].ContributionRewards[j];
            }
        }

        FirstPlaceReward = eventConfig.FirstPlaceReward;

        SecondPlaceReward = eventConfig.SecondPlaceReward;

        ThirdPlaceReward = eventConfig.ThirdPlaceReward;

        LastReward = eventConfig.LastReward;

        Id = string.Format("GE_{0}_{1}_{2}", GlobalEventStartTime, eventConfig.WeekNumber, eventConfig.Type);
    }
}
