using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using Hospital;
using NCrontab.Advanced;
using NCrontab.Advanced.Enumerations;

public enum StandardEventKeys
{
    generalData,
    positiveEnergyRewardForHelpInTreatment_MIN_FACTOR,
    positiveEnergyRewardForHelpInTreatment_MAX_FACTOR,
    shovelDrawChance_FACTOR,
    positiveEnergyFromKids_FACTOR,
    spawnKidChance_FACTOR,
    vipCureTime_FACTOR,
    nextEpidemyCooldown_FACTOR,
    nextVIPArriveCooldown_FACTOR,
    nextVIPPenalizedArrive_FACTOR,
    nextFreeBubbleBoy_FACTOR,
    treasureChestInterval_FACTOR,
    bacteriaSpreadDuration0_FACTOR,
    bacteriaSpreadDuration1_FACTOR,
    bacteriaSpreadDuration2_FACTOR,
    expForGardenHelp_FACTOR,
    panaceaCollectorFillRate_FACTOR,
    patientToDiagnoseChance_FACTOR,
    patientWithBacteriaChance_FACTOR,
    bacteriaReward0_FACTOR,
    bacteriaReward1_FACTOR,
    bacteriaReward2_FACTOR,
    shovelDrawChanceForHelpInGarden_FACTOR,
    doctorsWorkingTime_FACTOR,
    medicineProductionTime_FACTOR,
    diagnosticStationsTime_FACTOR,
    expForDoctors_FACTOR,
    expForMedicineProduction_FACTOR,
    expForTreatmentRooms_FACTOR,
    coinsForDoctorRooms_FACTOR,
    coinsForTreatmentRooms_FACTOR,
    costOfDiagnosis_FACTOR,
    maxGiftsPerDay_FACTOR,
    rewardForTODOSCoins_FACTOR,
    rewardForTODOSDiamonds_FACTOR,
    expForHelpingInTreatmentRoom_FACTOR,
    coinsForAds_FACTOR,
    diamondsForAds_FACTOR
}
public static class StandardEventConfig
{
    private const string endCheckingCron = "0 0 * * 1";

    private static StandardEventGeneralData generalData;
    private static Dictionary<StandardEventKeys, StandardEventPartialData> eventPartialData = new Dictionary<StandardEventKeys, StandardEventPartialData>();
    public static void InitializeEvent(StandardEventsCData config)
    {
        if (config == null)
        {
            return;
        }

        if (!IsVersionSupported(config))
        {
            return;
        }

        SingleStandardEventCData currentEvent = GetCurrentEvent(config);
        //SingleStandardEventModel nextEvent = GetNextEvent(config);

        generalData = null;
        eventPartialData.Clear();

        if (currentEvent == null)
        {
            return;
        }

            generalData = new StandardEventGeneralData(
            currentEvent.EventUniqueID,
            currentEvent.StartDate,
            currentEvent.EndDate,
            currentEvent.ArtDecPoint
            );

        for (int i = 0; i < currentEvent.Effects.Count; ++i)
        {
            StandardEventPartialData partialData = new StandardEventPartialData(
                (StandardEventKeys)Enum.Parse(typeof(StandardEventKeys), currentEvent.Effects[i].Type),
                float.Parse(currentEvent.Effects[i].Parameter, CultureInfo.InvariantCulture),
                currentEvent.Effects[i].MinLevel
                );
            if (eventPartialData.ContainsKey(partialData.key))
            {
                eventPartialData[partialData.key] = partialData;
            }
            else
            {
                eventPartialData.Add(partialData.key, partialData);
            }
        }
    }

    private static void PrepareEvents(StandardEventsCData config)
    {

    }

    public static bool CanPlayerParticipateInAnyEvent()
    {
        if (generalData == null || eventPartialData.Count == 0)
        {
            return false;
        }
        if (!HasPlayerRequiredLevel())
        {
            return false;
        }
        if (!IsEventHappeningNow())
        {
            return false;
        }
        return true;
    }

    public static bool IsThereAnyEventButPlayerCannotParticipateInAny()
    {
        return generalData != null && eventPartialData.Count != 0 && IsEventHappeningNow() && !HasPlayerRequiredLevel();
    }

    public static bool IsPartialDataIsActiveInEvent(StandardEventKeys key)
    {
        if (!CanPlayerParticipateInAnyEvent())
        {
            return false;
        }
        if (!eventPartialData.ContainsKey(key))
        {
            return false;
        }
        if (Game.Instance.gameState().GetHospitalLevel() < eventPartialData[key].minLevel)
        {
            return false;
        }
        return true;
    }

    public static StandardEventGeneralData GetGeneralEventData()
    {
        return generalData;
    }

    public static float GetValueFromPartialEvent(StandardEventKeys key, float defaultValue)
    {
        if (!IsPartialDataIsActiveInEvent(key))
        {
            return defaultValue;
        }
        else
        {
            return eventPartialData[key].eventValue;
        }
    }

    public static DateTime GetEventEndTime()
    {
        if (generalData != null)
        {
            return generalData.endTime;
        }

        return DateTime.UtcNow;
    }

    public static List<string> GetDescriptionList()
    {
        List<string> listToReturn = new List<string>();
        foreach (KeyValuePair<StandardEventKeys, StandardEventPartialData> item in eventPartialData)
        {
            if (item.Value.IsPartialDataActive() && !String.IsNullOrEmpty(item.Value.GetDetailInfoTerm()) && !listToReturn.Contains(item.Value.GetDetailInfoTerm()))
            {
                listToReturn.Add(item.Value.GetDetailInfoTerm());
            }
        }
        return listToReturn;
    }

    public static int GetMinimumLevelFromPartialEventData()
    {
        int minLevel = int.MaxValue;

        foreach (KeyValuePair<StandardEventKeys, StandardEventPartialData> partialData in eventPartialData)
        {
            if (partialData.Value.minLevel < minLevel)
            {
                minLevel = partialData.Value.minLevel;
            }
        }
        return minLevel;
    }

    public static void OnActivate()
    {
        foreach (KeyValuePair<StandardEventKeys, StandardEventPartialData> eventPartialData in eventPartialData)
        {
            if (IsPartialDataIsActiveInEvent(eventPartialData.Key))
            {
                switch (eventPartialData.Key)
                {
                    case StandardEventKeys.nextFreeBubbleBoy_FACTOR:
                        BubbleBoyDataSynchronizer.Instance.OnGameEventActivate();
                        break;
                    case StandardEventKeys.nextEpidemyCooldown_FACTOR:
                        Hospital.HospitalAreasMapController.HospitalMap.epidemy.OnGameEventActivate();
                        break;
                    case StandardEventKeys.nextVIPArriveCooldown_FACTOR:
                        Hospital.HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().OnGameEventActivate();
                        break;
                }
            }
        }

        if (!CacheManager.IsGameEventDisplayedCached(generalData))
        {
            CacheManager.SetEventDisplayed(generalData, false);
        }
    }
    #region Private
    private static bool IsEventHappeningNow()
    {
        DateTime now = DateTime.UtcNow;

        return now > generalData.startTime && now < generalData.endTime;
    }
    private static bool HasPlayerRequiredLevel()
    {
        return Game.Instance.gameState().GetHospitalLevel() >= GetMinimumLevelFromPartialEventData();
    }
    private static StandardEventPartialData CheckingForPartialEvent(StandardEventKeys key, bool CheckingLevelConstraint)
    {
        if (eventPartialData.ContainsKey(key))
        {
            if (eventPartialData[key].IsPartialDataActive(CheckingLevelConstraint))
            {
                return eventPartialData[key];
            }
        }
        return null;
    }

    private static int GetWeekId(DateTime date, StandardEventsCData config)
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
        int weekFromStart = (timeSpan.Days + day)/ 7;
        weekId = weekFromStart % config.LenghtInWeeks;

        return weekId;
    }

    private static int GetCurrenWeekId(StandardEventsCData config)
    {
        return GetWeekId(DateTime.Now, config);
    }

    private static int GetMaxEventDuration(StandardEventsCData config)
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

    private static List<SingleStandardEventCData> GetEventsForWeek(int weekId, StandardEventsCData config)
    {
        return config.Events.FindAll((x) => x.WeekNumber == weekId); // boxing?
    }

    private static SingleStandardEventCData GetCurrentEvent(StandardEventsCData config)
    {
        int maxEventduration = GetMaxEventDuration(config);
        DateTime now = DateTime.UtcNow;
        DateTime startCheckingDate = now.Subtract(TimeSpan.FromSeconds(maxEventduration));

        CrontabSchedule schedule = CrontabSchedule.Parse(config.StartTime, CronStringFormat.WithYears);
        DateTime configStartDate = schedule.CronToDateTime();

        startCheckingDate = CrontabSchedule.MaxDate(startCheckingDate, configStartDate);

        CrontabSchedule scheduleEnd = CrontabSchedule.Parse(endCheckingCron, CronStringFormat.Default);
        DateTime endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);

        //find event
        List<SingleStandardEventCData> weekModels = new List<SingleStandardEventCData>();
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

            weekModels.Sort((x,y) => x.StartDate.CompareTo(y.StartDate));

            for (int i = 0; i < weekModels.Count; ++i)
            {
                if (weekModels[i].StartDate < now && weekModels[i].EndDate > now)
                {
                    SingleStandardEventCData model = new SingleStandardEventCData
                    {
                        EventUniqueID = weekModels[i].EventUniqueID,
                        EventDuration = weekModels[i].EventDuration,
                        WeekNumber = weekModels[i].WeekNumber,
                        StartTimeCron = weekModels[i].StartTimeCron,
                        ArtDecPoint = weekModels[i].ArtDecPoint,
                        Effects = weekModels[i].Effects,
                        StartDate = weekModels[i].StartDate,
                        EndDate = weekModels[i].EndDate
                    };
                    weekModels.Clear();
                    return model;
                }
            }

            startCheckingDate = endCheckingDate;
            endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);
        }
        weekModels.Clear();
        return null;
    }

    private static SingleStandardEventCData GetNextEvent(StandardEventsCData config)
    {
        DateTime now = DateTime.UtcNow;
        DateTime startCheckingDate = now;

        CrontabSchedule schedule = CrontabSchedule.Parse(config.StartTime, CronStringFormat.WithYears);
        DateTime configStartDate = schedule.CronToDateTime();

        startCheckingDate = CrontabSchedule.MaxDate(startCheckingDate, configStartDate);

        CrontabSchedule scheduleEnd = CrontabSchedule.Parse(endCheckingCron, CronStringFormat.Default);
        DateTime endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);

        List<SingleStandardEventCData> weekModels = new List<SingleStandardEventCData>();
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
                if (weekModels[i].StartDate > now)
                {
                    SingleStandardEventCData model = new SingleStandardEventCData
                    {
                        EventUniqueID = weekModels[i].EventUniqueID,
                        EventDuration = weekModels[i].EventDuration,
                        WeekNumber = weekModels[i].WeekNumber,
                        StartTimeCron = weekModels[i].StartTimeCron,
                        ArtDecPoint = weekModels[i].ArtDecPoint,
                        Effects = weekModels[i].Effects,
                        StartDate = weekModels[i].StartDate,
                        EndDate = weekModels[i].EndDate
                    };
                    weekModels.Clear();
                    return model;
                }
            }
            startCheckingDate = endCheckingDate;
            endCheckingDate = scheduleEnd.GetNextOccurrence(startCheckingDate);

            ++loopCounter;
        }
        weekModels.Clear();
        return null;
    }

    private static bool IsVersionSupported(StandardEventsCData config)
    {
        var clientVersion = new Version(Application.version);
        if (config.MinVersion != "-" && clientVersion < new Version(config.MinVersion))
            return false;
        if (config.MaxVersion != "-" && clientVersion > new Version(config.MaxVersion))
            return false;
        return true;
    }
    #endregion
}
public class StandardEventGeneralData
{
    public string EventID = "";
    public DateTime startTime;
    public DateTime endTime;
    public string imageDecisionPoint = "";
    public DateTime nextEventStartDate;
    public DateTime nextEventStartTeaseDate;

    public Sprite artSprite = null;

    public StandardEventGeneralData(string eventID, DateTime startTime, DateTime endTime, string imageDecisionPoint/*, DateTime nextEventStartDate, DateTime nextEventStartTeaseDate*/)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.imageDecisionPoint = imageDecisionPoint;
        this.EventID = eventID;
        /*this.nextEventStartDate = nextEventStartDate;
        this.nextEventStartTeaseDate = nextEventStartTeaseDate;*/
        DownloadArtSprite();
    }

    public void DownloadArtSprite(UnityAction onSucces= null)
    {
        //TODO
        Debug.Log("Get standard event art was from DDNA");

        //DecisionPointCalss.RequestSprite(imageDecisionPoint, (sprite) => {
        //    if (sprite != null)
        //        artSprite = sprite;
        //    onSucces?.Invoke();
        //}, null);
    }
}
public class StandardEventPartialData
{
    enum ParseIndexes
    {
        Value = 0,
        minLevel = 1,
    }
    public StandardEventKeys key;
    public float eventValue;
    public int minLevel;
    private const char PARSER_SEPARATOR = '#';
    private const int MIN_PARAM_DATA_LENGT = 2;

    public StandardEventPartialData(StandardEventKeys key, float eventValue, int minLevel)
    {
        this.key = key;
        this.eventValue = eventValue;
        this.minLevel = minLevel;
    }

    public static StandardEventPartialData Parse(StandardEventKeys dataKey, string parameters)
    {
        string[] data = parameters.Split(PARSER_SEPARATOR);
        if (data.Length < MIN_PARAM_DATA_LENGT)
        {
            return null;
        }
        float eventValue = 0;
        int minLevel = 0;
        if (data.Length > (int)ParseIndexes.Value)
        {
            try
            {
                eventValue = float.Parse(data[(int)ParseIndexes.Value], CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.LogError("Cant unparse standard event partial data for " + dataKey + " with error: " + e.Message);
                return null;
            }
        }
        if (data.Length > (int)ParseIndexes.minLevel)
        {
            try
            {
                minLevel = int.Parse(data[(int)ParseIndexes.minLevel], CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.LogError("Cant unparse standard event partial data for " + dataKey + " with error: " + e.Message);
                return null;
            }
        }

        return new StandardEventPartialData(dataKey, eventValue, minLevel);
    }

    public bool IsPartialDataActive(bool CheckingLevelConstraint = true)
    {
        if (CheckingLevelConstraint && !HasRequiredLevel())
        {
            return false;
        }
        return true;
    }

    public string GetDetailInfoTerm()
    {
        string tag;

        switch (key)
        {
            case StandardEventKeys.shovelDrawChance_FACTOR:
                tag = "EVENTS/EVENT_INFO_1";
                break;
            case StandardEventKeys.positiveEnergyFromKids_FACTOR:
                tag = "EVENTS/EVENT_INFO_2";
                break;
            case StandardEventKeys.spawnKidChance_FACTOR:
                tag = "EVENTS/EVENT_INFO_10";
                break;
            case StandardEventKeys.nextEpidemyCooldown_FACTOR:
                tag = "EVENTS/EVENT_INFO_4";
                break;
            case StandardEventKeys.nextVIPArriveCooldown_FACTOR:
                tag = "EVENTS/EVENT_INFO_3";
                break;
            case StandardEventKeys.nextFreeBubbleBoy_FACTOR:
                tag = "EVENTS/EVENT_INFO_5";
                break;
            case StandardEventKeys.treasureChestInterval_FACTOR:
                tag = "EVENTS/EVENT_INFO_6";
                break;
            case StandardEventKeys.bacteriaSpreadDuration0_FACTOR:
                tag = "EVENTS/EVENT_INFO_7";
                break;
            case StandardEventKeys.bacteriaSpreadDuration1_FACTOR:
                tag = "EVENTS/EVENT_INFO_7";
                break;
            case StandardEventKeys.bacteriaSpreadDuration2_FACTOR:
                tag = "EVENTS/EVENT_INFO_7";
                break;
            case StandardEventKeys.expForGardenHelp_FACTOR:
                tag = "EVENTS/EVENT_INFO_8";
                break;
            case StandardEventKeys.panaceaCollectorFillRate_FACTOR:
                tag = "EVENTS/EVENT_INFO_9";
                break;
            case StandardEventKeys.patientToDiagnoseChance_FACTOR:
                tag = "EVENTS/EVENT_INFO_11";
                break;
            case StandardEventKeys.patientWithBacteriaChance_FACTOR:
                tag = "EVENTS/EVENT_INFO_12";
                break;
            case StandardEventKeys.bacteriaReward0_FACTOR:
                tag = "EVENTS/EVENT_INFO_14";
                break;
            case StandardEventKeys.bacteriaReward1_FACTOR:
                tag = "EVENTS/EVENT_INFO_14";
                break;
            case StandardEventKeys.bacteriaReward2_FACTOR:
                tag = "EVENTS/EVENT_INFO_14";
                break;
            case StandardEventKeys.shovelDrawChanceForHelpInGarden_FACTOR:
                tag = "EVENTS/EVENT_INFO_13";
                break;
            case StandardEventKeys.doctorsWorkingTime_FACTOR:
                tag = "EVENTS/EVENT_INFO_21";
                break;
            case StandardEventKeys.medicineProductionTime_FACTOR:
                tag = "EVENTS/EVENT_INFO_22";
                break;
            case StandardEventKeys.diagnosticStationsTime_FACTOR:
                tag = "EVENTS/EVENT_INFO_23";
                break;
            case StandardEventKeys.expForDoctors_FACTOR:
                tag = "EVENTS/EVENT_INFO_24";
                break;
            case StandardEventKeys.expForMedicineProduction_FACTOR:
                tag = "EVENTS/EVENT_INFO_25";
                break;
            case StandardEventKeys.expForTreatmentRooms_FACTOR:
                tag = "EVENTS/EVENT_INFO_26";
                break;
            case StandardEventKeys.coinsForDoctorRooms_FACTOR:
                tag = "EVENTS/EVENT_INFO_27";
                break;
            case StandardEventKeys.coinsForTreatmentRooms_FACTOR:
                tag = "EVENTS/EVENT_INFO_28";
                break;
            case StandardEventKeys.costOfDiagnosis_FACTOR:
                tag = "EVENTS/EVENT_INFO_29";
                break;
            case StandardEventKeys.maxGiftsPerDay_FACTOR:
                tag = "EVENTS/EVENT_INFO_17";
                break;
            case StandardEventKeys.rewardForTODOSCoins_FACTOR:
                tag = "EVENTS/EVENT_INFO_17";
                break;
            case StandardEventKeys.rewardForTODOSDiamonds_FACTOR:
                tag = "EVENTS/EVENT_INFO_17";
                break;

            case StandardEventKeys.positiveEnergyRewardForHelpInTreatment_MIN_FACTOR:
            case StandardEventKeys.positiveEnergyRewardForHelpInTreatment_MAX_FACTOR:
            case StandardEventKeys.expForHelpingInTreatmentRoom_FACTOR:
                tag = "EVENTS/EVENT_INFO_20";
                break;
            case StandardEventKeys.coinsForAds_FACTOR:
                tag = "EVENTS/EVENT_INFO_30";
                break;
            case StandardEventKeys.diamondsForAds_FACTOR:
                tag = "EVENTS/EVENT_INFO_31";
                break;

            default:
                tag = "-";
                break;
        }
        return tag;
    }

    private string GetEventDetailedDescription(string tag)
    {
        return I2.Loc.ScriptLocalization.Get("EVENTS/" + tag);
    }

    private bool HasRequiredLevel()
    {
        return Game.Instance.gameState().GetHospitalLevel() >= minLevel;
    }
}
