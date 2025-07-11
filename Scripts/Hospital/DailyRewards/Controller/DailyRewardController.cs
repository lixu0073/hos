using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using System.Timers;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public class DailyRewardController : MonoBehaviour, ITimerClient
{
    private const char IS_COMPLETED_SEPARATOR = '!';

    public event Action<int> NewDayArrised;

    public List<DailyRewardModel> weeklyRewards { get; private set; }
    public BaseGiftableResource grandRewardForEntireWeek;
    int cycleCounter = 0; // number of weeks that has passed. It will be reset when player loop over entire delta reward list lenght.
    private DateTime startDateOfNewCycle_Day;
    SimpleTimer simpleTimer;
    private DateTime dayAtWhichPopupShowsItselfAutomatic_DAY;
    private int devAdder;
#pragma warning disable 0649
    Coroutine _delayedOpen;
#pragma warning restore 0649

    private enum SaveIndexes
    {
        startDateOfNewCycle_Day = 0,
        cycleCounter = 1,
        dailyRewardClaimStatusString = 2,
        dailyRewardList = 3,
        grandReward = 4,
        dayOfAutomativePopupOpen = 5,
    }

    private void Awake()
    {
        weeklyRewards = new List<DailyRewardModel>();
    }

    private void OnEnable()
    {
        BaseGameState.OnLevelUp -= UpdateRewardsOnLevelUp;
        BaseGameState.OnLevelUp += UpdateRewardsOnLevelUp;
    }

    private void OnDisable()
    {
        BaseGameState.OnLevelUp -= UpdateRewardsOnLevelUp;
        if (_delayedOpen != null)
        {
            try
            {
                StopCoroutine(_delayedOpen);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    private void UpdateRewardsOnLevelUp()
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= ContentUnlockLevel.DailyRewardUnlockLevel && SceneManager.GetActiveScene().name == "MainScene")
        {
            for (int i = 0; i < weeklyRewards.Count; ++i)
            {
                if (weeklyRewards[i].GetDailyRewardGift().rewardType == BaseGiftableResourceFactory.BaseResroucesType.coin)
                {
                    bool tempIsClaimed = weeklyRewards[i].isClaimed;
                    BaseGiftableResource temp = BaseGiftableResourceFactory.CreateCoinGiftableResource(DailyRewardResourceParser.GetAmountOfCoins(i), weeklyRewards[i].GetDailyRewardGift().GetSpriteType(), weeklyRewards[i].GetDailyRewardGift().GetEconomySource());
                    weeklyRewards[i] = new DailyRewardModel(temp);
                    weeklyRewards[i].isClaimed = tempIsClaimed;
                }
            }
        }
    }

    public List<string> SaveToString()
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= ContentUnlockLevel.DailyRewardUnlockLevel)
        {
            StringBuilder sb = new StringBuilder();
            List<string> saveToReturn = new List<string>();
            saveToReturn.Add(startDateOfNewCycle_Day.ToString());
            saveToReturn.Add(((DailyRewardParser.WeekNomenclature)cycleCounter).ToString());
            if (weeklyRewards != null && weeklyRewards.Count > 0)
            {
                foreach (DailyRewardModel reward in weeklyRewards)
                {
                    sb.Append(reward.isClaimed ? Boolean.TrueString : Boolean.FalseString);
                    sb.Append(IS_COMPLETED_SEPARATOR);
                }
                sb.Remove(sb.Length - 1, 1);
                saveToReturn.Add(sb.ToString());
                saveToReturn.Add(weeklyRewards.SaveWeeklyDailyRewardsToString());
                saveToReturn.Add(grandRewardForEntireWeek.GiftableToString());
                saveToReturn.Add(dayAtWhichPopupShowsItselfAutomatic_DAY.ToString());
                return saveToReturn;
            }

            Debug.LogError("Weekly reward list is null. No save will be generated");
            return null;
        }
        return null;
    }

    public void LoadFromString(List<string> save)
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= ContentUnlockLevel.DailyRewardUnlockLevel)
        {
            DailyRewardParser.InitializeDailyRewards(DefaultConfigurationProvider.GetDailyRewardsCData());

            weeklyRewards.Clear();
            if (save != null && save.Count > 0)
            {
                DateTime.TryParse(save[(int)SaveIndexes.startDateOfNewCycle_Day], out startDateOfNewCycle_Day);
                DateTime.TryParse(save[(int)SaveIndexes.dayOfAutomativePopupOpen], out dayAtWhichPopupShowsItselfAutomatic_DAY);
                cycleCounter = (int)Enum.Parse(typeof(DailyRewardParser.WeekNomenclature), save[(int)SaveIndexes.cycleCounter]);
                int howManyDaysHasPassedSinceStartOfCycle = (DateTime.Today - startDateOfNewCycle_Day).Days;
                if (howManyDaysHasPassedSinceStartOfCycle < WorldWideConstants.DAYS_IN_WEEK)
                {
                    if (TryLoadConfigData(save[(int)SaveIndexes.dailyRewardList], save[(int)SaveIndexes.grandReward]))
                    {
                        ActualizeRewardClaimStatusFromSave(save[(int)SaveIndexes.dailyRewardClaimStatusString]);
                        if (ResetCurrentCycleCheck())
                            ResetCurrentWeek();

                        StartTimterToNextDay();
                    }
                }
                else
                {
                    int howManyCyclesHasPassed = howManyDaysHasPassedSinceStartOfCycle / WorldWideConstants.DAYS_IN_WEEK;
                    cycleCounter += howManyCyclesHasPassed;
                    CheckCycleCounterValue();
                    ResetCurrentWeek();
                    if (TryLoadConfigData())
                        StartTimterToNextDay();
                }
            }
            else
                InitializeFirstDailyRewardEver();

            OpenPopupIfFirstDay();
        }
        else
            SubscribetoLevelUpEvent();
    }

    private void BaseGameState_OnLevelUp()
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= ContentUnlockLevel.DailyRewardUnlockLevel && SceneManager.GetActiveScene().name == "MainScene")
        {
            Debug.LogError("INIT daily rewards");
            //DailyRewardDeltaConfig.InitializeDailyRewards(DailyRewardDeltaConfig.unparsedParameters);
            weeklyRewards.Clear();
            InitializeFirstDailyRewardEver();
            SubscribeActionToLevelUpPopupClose();
            BaseGameState.OnLevelUp -= BaseGameState_OnLevelUp;
        }
    }

    private void SubscribeActionToLevelUpPopupClose()
    {
        NotificationCenter.Instance.LevelReachedAndClosed.Notification -= OpenPopupOnLevelupClosed;
        NotificationCenter.Instance.LevelReachedAndClosed.Notification += OpenPopupOnLevelupClosed;
    }

    private void OpenPopupOnLevelupClosed(LevelReachedAndClosedEventArgs eventArgs)
    {
        if (eventArgs.level >= ContentUnlockLevel.DailyRewardUnlockLevel)
        {
            OpenPopupIfFirstDay();
            NotificationCenter.Instance.LevelReachedAndClosed.Notification -= OpenPopupOnLevelupClosed;
        }
    }

    public bool EntireWeekClaimed()
    {
        return AreRewardsUpToGivenDayClaimed(WorldWideConstants.DAYS_IN_WEEK - 1);
    }

    public bool ResetCurrentCycleCheck()
    {
        if (GetCurrentDayInWeek() >= 2)
            return !AreRewardsUpToGivenDayClaimed(GetDayBeforeYesterday());

        return false;
    }

    public bool AreRewardsUpToGivenDayClaimed(int day)
    {
        if (day >= 0)
        {
            for (int i = 0; i <= day; ++i)
            {
                if (!weeklyRewards[i].isClaimed)                
                    return false;
            }
            return true;
        }

        Debug.LogError("Number is less than zero. No sense");
        return true;
    }

    public void CollectRewardForDay(int dayNumber, bool fromAdd = false)
    {
        if (dayNumber <= (WorldWideConstants.DAYS_IN_WEEK - 2) && dayNumber >= 0)
        {
            weeklyRewards[dayNumber].CollectReward(false, true);
            AnalyticsController.instance.ReportDailyRewardClaimed(dayNumber, fromAdd);
            return;
        }

        if (dayNumber == WorldWideConstants.DAYS_IN_WEEK - 1)
        {
            weeklyRewards[dayNumber].isClaimed = true;
            grandRewardForEntireWeek.Collect(true);
        }
    }

    public void CollecRewardDueToAd()
    {
        CollectRewardForDay(GetCurrentDayInWeek() - 1, true);
    }

    public void TimeHasElapsed(ElapsedEventArgs timerEvents)
    {
        SetupNewDay();
        OpenPopupIfFirstDay();
    }

    public bool IsCurrentDayRewardClaimed()
    {
        return weeklyRewards[GetCurrentDayInWeek()].isClaimed;
    }

    private bool TryLoadConfigData(string dailyRewardString = "", string grandRewardString = "")
    {
        weeklyRewards.Clear();
        List<DailyRewardModel> listWithRewardModels;
        BaseGiftableResource grandReward;
        if (String.IsNullOrEmpty(dailyRewardString))
        {
            DailyRewardParser.rewardsPerWeekMap.TryGetValue(cycleCounter, out listWithRewardModels);
            DailyRewardParser.bigRewardBoxForWeek.TryGetValue(cycleCounter, out grandReward);
            if (listWithRewardModels != null && listWithRewardModels.Count == 7 && grandReward != null)
            {
                weeklyRewards.AddRange(listWithRewardModels);
                grandRewardForEntireWeek = grandReward;
                return true;
            }
        }
        else
        {
            listWithRewardModels = DailyRewardParser.LoadWeeklyDailyRewardsFromString(dailyRewardString);
            grandRewardForEntireWeek = BaseGiftableResourceFactory.CreateGiftableFromString(grandRewardString, EconomySource.DailyRewards);
            if (listWithRewardModels != null && listWithRewardModels.Count == 7)
            {
                weeklyRewards.AddRange(listWithRewardModels);
                return true;
            }
        }
        return false;
    }

    private void OpenPopupIfFirstDay()
    {
        if (!AreaMapController.Map.VisitingMode)
        {
            if (dayAtWhichPopupShowsItselfAutomatic_DAY == null || dayAtWhichPopupShowsItselfAutomatic_DAY <= DateTime.Today)
            {
                if (UIController.get.LevelUpPopUp.isActiveAndEnabled)
                    CoroutineInvoker.Instance.StartCoroutine(DelayedOpen());
                else
                {
                    dayAtWhichPopupShowsItselfAutomatic_DAY = DateTime.Today.AddDays(1);
					
					//point to look into
                    try
                    {
                        UIController.getHospital.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyRewardStandard);
                    }
                    catch(Exception)
                    {
                        Debug.LogWarning("Probably something went wrong here");
                    }					
                }
            }
        }
    }

    private IEnumerator DelayedOpen()
    {
        while (UIController.get.LevelUpPopUp.isActiveAndEnabled)
        {
            yield return null;
        }
        dayAtWhichPopupShowsItselfAutomatic_DAY = DateTime.Today.AddDays(1);
        UIController.getHospital.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyRewardStandard);
    }

    private void ActualizeRewardClaimStatusFromSave(string stringFromSave)
    {
        string[] separatedBooleans = stringFromSave.Split(IS_COMPLETED_SEPARATOR);
        if (separatedBooleans.Length == WorldWideConstants.DAYS_IN_WEEK)
        {
            for (int i = 0; i < separatedBooleans.Length; i++)
            {
                Boolean.TryParse(separatedBooleans[i], out bool boolFromSave);
                weeklyRewards[i].isClaimed = boolFromSave;
            }
        }
    }

    private void InitializeFirstDailyRewardEver()
    {
        cycleCounter = 0;
        startDateOfNewCycle_Day = DateTime.Today;
        if (TryLoadConfigData())
            StartTimterToNextDay();
    }

    private void SetupNewDay()
    {
        if (GetCurrentDayInWeek() >= WorldWideConstants.DAYS_IN_WEEK)
        {
            devAdder = 0;
            TriggerNextWeek();
        }
        if (ResetCurrentCycleCheck())
        {
            devAdder = 0;
            ResetCurrentWeek();
        }
        OnNextDayArrived(GetCurrentDayInWeek());
    }

    private void TriggerNextWeek()
    {
        cycleCounter++;
        CheckCycleCounterValue();
        ApplyWeekData(cycleCounter);
    }

    private void CheckCycleCounterValue()
    {
        if (cycleCounter >= DailyRewardParser.HowManyWeeksAreUnique())        
            cycleCounter = 0;
    }

    private void ApplyWeekData(int cycleNumber)
    {
        startDateOfNewCycle_Day = DateTime.Today;
        weeklyRewards.Clear();
        for (int i = 0; i < DailyRewardParser.rewardsPerWeekMap[cycleNumber].Count; ++i)
        {
            DailyRewardParser.rewardsPerWeekMap[cycleNumber][i].isClaimed = false;
        }
        weeklyRewards.AddRange(DailyRewardParser.rewardsPerWeekMap[cycleNumber]);
    }

    private void ResetCurrentWeek()
    {
        ApplyWeekData(cycleCounter);
    }

    private void StartTimterToNextDay()
    {
        DateTime now = DateTime.Now;
        DateTime nextDayMidnight = GetNextDayDate();
        TimeSpan timeToNextDay = nextDayMidnight - now;
        simpleTimer = new SimpleTimer(this, timeToNextDay.TotalMilliseconds);
    }

    private DateTime GetNextDayDate()
    {
        return DateTime.Today.AddDays(1);
    }

    public int GetCurrentDayInWeek()
    {
        return (DateTime.Today - startDateOfNewCycle_Day).Days + devAdder;
    }

    private void OnNextDayArrived(int newDayNumber)
    {
        NewDayArrised?.Invoke(newDayNumber);
    }

    private int GetDayBeforeYesterday()
    {
        return GetCurrentDayInWeek() - 2;
    }

    private int ConvertBoolToInt(bool boolean)
    {
        return boolean ? 1 : 0;
    }

    private bool ConvertIntToBoolean(int integer)
    {
        if (integer >= 0 && integer < 2)     
            return integer == 1;

        return false;
    }

    private void SubscribetoLevelUpEvent()
    {
        BaseGameState.OnLevelUp -= BaseGameState_OnLevelUp;
        BaseGameState.OnLevelUp += BaseGameState_OnLevelUp;
    }

    [ContextMenu("Move forward day")]
    public void MoveEntireDay()
    {
        if (!DeveloperParametersController.Instance().parameters.devTestButtonVisible)
            return;

        ++devAdder;
        SetupNewDay();
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public SimpleTimer GetTimer()
    {
        return simpleTimer;
    }

    private List<string> SavedData = new List<string>();

    [ContextMenu("Save daily rewards")]
    public void SaveDailyRewards()
    {
        SavedData = SaveToString();
        foreach (var item in SavedData)
        {
            Debug.LogError(item);
        }
    }

    [ContextMenu("Move forward with week")]
    public void MoveEntireWeek()
    {
        CollectRewardForDay(GetCurrentDayInWeek());
        for (int i = 0; i < 5; ++i)
        {
            devAdder++;
            CollectRewardForDay(GetCurrentDayInWeek());
            //UIController.getHospital.DailyQuestAndDailyRewardUITabController.gameObject.GetComponent<DailyRewardPopupInitializer>().ReInitializeWhileOpened();
        }
        ++devAdder;
        //UIController.getHospital.DailyQuestAndDailyRewardUITabController.gameObject.GetComponent<DailyRewardPopupInitializer>().ReInitializeWhileOpened();
    }

    [ContextMenu("Load daily rewards when save not empty")]
    public void LoadDailyRewardsNoEmpty()
    {
        LoadFromString(SavedData);
    }

    [ContextMenu("Load daily rewards when save when empty")]
    public void LoadDailyRewardsWhenEmpty()
    {
        SavedData[(int)SaveIndexes.startDateOfNewCycle_Day] = String.Empty;
        LoadFromString(SavedData);
    }

    private void OnDestroy()
    {
        SubscribetoLevelUpEvent();
    }
#endif
}

