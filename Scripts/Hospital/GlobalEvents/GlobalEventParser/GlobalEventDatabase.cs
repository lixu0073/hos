using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using NCrontab.Advanced;
using NCrontab.Advanced.Enumerations;

public class GlobalEventDatabase{

    private const string endCheckingCron = "0 0 * * *";

    private List<GlobalEventData> globalEvents = new List<GlobalEventData>();

    public List<GlobalEventData> GetAllEvents()
    {
        if (globalEvents != null)
            return globalEvents;
        else return null;
    }

    public GlobalEventData GetCurrentGlobalEvent(GlobalEventsCData config)
    {
        int maxEventduration = GetMaxEventDuration(config);

        DateTime now = DateTime.UtcNow;
        DateTime startCheckingDate = now.Subtract(TimeSpan.FromSeconds(maxEventduration));

        CrontabSchedule schedule = CrontabSchedule.Parse(config.StartTime, CronStringFormat.WithYears);
        DateTime configStartDate = schedule.CronToDateTime();

        startCheckingDate = CrontabSchedule.MaxDate(startCheckingDate, configStartDate);

        CrontabSchedule scheduleEnd = CrontabSchedule.Parse(endCheckingCron, CronStringFormat.Default);
        DateTime endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);

        List<SingleGlobalEventCData> weekModels = new List<SingleGlobalEventCData>();

        while (startCheckingDate < now)
        {
            endCheckingDate = CrontabSchedule.MinDate(endCheckingDate, now);
            int startCheckingWeekId = GetWeekId(startCheckingDate, config);
            weekModels = GetEventsForWeek(startCheckingWeekId, config);

            for (int i = 0; i < weekModels.Count; ++i)
            {
                schedule = CrontabSchedule.Parse(weekModels[i].StartTimeCron, CronStringFormat.Default);
                weekModels[i].StartDate = schedule.GetNextOccurrence(startCheckingDate.Subtract(TimeSpan.FromSeconds(60)), endCheckingDate);
                weekModels[i].EndDate = weekModels[i].StartDate.AddSeconds(weekModels[i].EventDuration);

                if (weekModels[i].StartDate == endCheckingDate)
                {
                    weekModels[i].StartDate = DateTime.MinValue;
                    weekModels[i].EndDate = DateTime.MinValue;
                }
            }

            weekModels.Sort((x, y) => x.StartDate.CompareTo(y.StartDate));

            for (int i = 0; i < weekModels.Count; ++i)
            {
                if (weekModels[i].StartDate < now && weekModels[i].EndDate > now)
                {
                    GlobalEventData data = new GlobalEventData(weekModels[i]);

                    weekModels.Clear();
                    return data;
                }
            }

            startCheckingDate = endCheckingDate;
            endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);
        }

        weekModels.Clear();
        return null;
    }

    public GlobalEventData GetNextGlobalEvent(GlobalEventsCData config)
    {
        DateTime now = DateTime.UtcNow;
        DateTime startCheckingDate = now;

        CrontabSchedule schedule = CrontabSchedule.Parse(config.StartTime, CronStringFormat.WithYears);
        DateTime configStartDate = schedule.CronToDateTime();

        startCheckingDate = CrontabSchedule.MaxDate(startCheckingDate, configStartDate);

        CrontabSchedule scheduleEnd = CrontabSchedule.Parse(endCheckingCron, CronStringFormat.Default);
        DateTime endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);

        List<SingleGlobalEventCData> weekModels = new List<SingleGlobalEventCData>();
        int loopCounter = 0;
        while (loopCounter <= config.LenghtInWeeks)
        {
            int startCheckingWeekId = GetWeekId(startCheckingDate, config);
            weekModels = GetEventsForWeek(startCheckingWeekId, config);

            for (int i = 0; i < weekModels.Count; ++i)
            {
                schedule = CrontabSchedule.Parse(weekModels[i].StartTimeCron, CronStringFormat.Default);
                weekModels[i].StartDate = schedule.GetNextOccurrence(startCheckingDate.Subtract(TimeSpan.FromSeconds(60)), endCheckingDate);
                weekModels[i].EndDate = weekModels[i].StartDate.AddSeconds(weekModels[i].EventDuration);

                if (weekModels[i].StartDate == endCheckingDate)
                {
                    weekModels[i].StartDate = DateTime.MinValue;
                }
            }

            weekModels.Sort((x, y) => x.StartDate.CompareTo(y.StartDate));

            for (int i = 0; i < weekModels.Count; ++i)
            {
                if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null &&
                    (int)weekModels[i].StartDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds < GlobalEventParser.Instance.CurrentGlobalEventConfig.GlobalEventEndTime)
                {
                    continue;
                }
                if (weekModels[i].StartDate > now)
                {
                    GlobalEventData data = new GlobalEventData(weekModels[i]);
                    weekModels.Clear();
                    return data;
                }
            }
            startCheckingDate = endCheckingDate;
            endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);

            ++loopCounter;
        }
        weekModels.Clear();
        return null;
    }

    public GlobalEventData GetPreviousGlobalEvent(GlobalEventsCData config)
    {
        DateTime now = DateTime.UtcNow;

        CrontabSchedule schedule = CrontabSchedule.Parse(config.StartTime, CronStringFormat.WithYears);
        DateTime configStartDate = schedule.CronToDateTime();

        CrontabSchedule scheduleEnd = CrontabSchedule.Parse(endCheckingCron, CronStringFormat.Default);
        DateTime endCheckingDate = scheduleEnd.GetNextOccurrence(now);
        DateTime startCheckingDate = endCheckingDate.Subtract(TimeSpan.FromDays(1));

        List<SingleGlobalEventCData> weekModels = new List<SingleGlobalEventCData>();

        int loopCounter = 0;
        while (loopCounter <= config.LenghtInWeeks * 7)
        {
            if (startCheckingDate < configStartDate)
            {
                loopCounter = config.LenghtInWeeks;
                startCheckingDate = configStartDate;
            }

            endCheckingDate = CrontabSchedule.MinDate(endCheckingDate, now);
            int startCheckingWeekId = GetWeekId(startCheckingDate, config);
            weekModels = GetEventsForWeek(startCheckingWeekId, config);

            for (int i = 0; i < weekModels.Count; ++i)
            {
                schedule = CrontabSchedule.Parse(weekModels[i].StartTimeCron, CronStringFormat.Default);
                weekModels[i].StartDate = schedule.GetNextOccurrence(startCheckingDate.Subtract(TimeSpan.FromSeconds(60)), endCheckingDate);
                weekModels[i].EndDate = weekModels[i].StartDate.AddSeconds(weekModels[i].EventDuration);

                if (weekModels[i].StartDate == endCheckingDate)
                {
                    weekModels[i].StartDate = DateTime.MaxValue;
                }
            }

            weekModels.Sort((x, y) => y.StartDate.CompareTo(x.StartDate));

            for (int i = 0; i < weekModels.Count; ++i)
            {
                if (GlobalEventParser.Instance.CurrentGlobalEventConfig != null &&
                    (int)weekModels[i].StartDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds > GlobalEventParser.Instance.CurrentGlobalEventConfig.GlobalEventStartTime)
                {
                    continue;
                }
                if (weekModels[i].StartDate < now && weekModels[i].EndDate < now)
                {
                    GlobalEventData data = new GlobalEventData(weekModels[i]);
                    weekModels.Clear();
                    if (!string.IsNullOrEmpty(data.OtherParameters.IconActivitySprite) && data.OtherParameters.IconActivitySprite != "-")
                    {
                        if (CollectOnMapGEGraphicsManager.GetInstance != null)
                        {
                            CollectOnMapGEGraphicsManager.GetInstance.LoadItemIconPreviousActivitySprite(data.OtherParameters.IconActivitySprite);
                        }
                        else
                        {
                            CollectOnMapGEGraphicsManager.onInstanceBinded += new UnityAction(() =>
                            {
                                CollectOnMapGEGraphicsManager.GetInstance.LoadItemIconPreviousActivitySprite(data.OtherParameters.IconActivitySprite);
                            });
                        }
                    }
                    return data;
                }
            }

            endCheckingDate = startCheckingDate;
            startCheckingDate = startCheckingDate.Subtract(TimeSpan.FromDays(1));

            ++loopCounter;
        }
        
        return null;
    }

    private static int GetMaxEventDuration(GlobalEventsCData config)
    {
        int maxDuration = int.MinValue;

        for (int i = 0; i < config.Events.Count; ++i)
        {
            if (config.Events[i].EventDuration > maxDuration)
            {
                maxDuration = config.Events[i].EventDuration;
            }
        }

        return maxDuration;
    }

    private static int GetWeekId(DateTime date, GlobalEventsCData config)
    {
        int weekId = 0;

        CrontabSchedule schedule = CrontabSchedule.Parse(config.StartTime, CronStringFormat.WithYears);

        DateTime configStartDate = schedule.CronToDateTime();

        if (configStartDate > date)
        {
            return -1;
        }

        TimeSpan timeSpan = date - configStartDate;
        int day = (int)configStartDate.DayOfWeek;
        if (day == 0)
        {
            day = 7;
        }
        --day;
        int weekFromStart = (timeSpan.Days + day) / 7;
        weekId = weekFromStart % config.LenghtInWeeks;

        return weekId;
    }

    private static List<SingleGlobalEventCData> GetEventsForWeek(int weekId, GlobalEventsCData config)
    {
        return config.Events.FindAll((x) => x.WeekNumber == weekId); // boxing?
    }
}
