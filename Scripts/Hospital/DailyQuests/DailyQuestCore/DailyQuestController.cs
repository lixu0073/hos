using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MovementEffects;
using Hospital;

public class DailyQuestController : MonoBehaviour
{
    [SerializeField]
    DailyTaskUI[] dailyTasksList;

    const int WEEK = 604800, DAY = 86400;
    public int currentDayTime = 0;

    public int timeScale = 1; // time_scale for debug

    public DailyQuestGenerator dailyQuestCreator;
    public DailyTaskReplacerController dailyTaskReplacerController;

    public DailyQuest currentDailyQuest
    {
        get; private set;
    }

    public bool IsFirstStart()
    {
        if (DailyQuestSynchronizer.Instance.WeeklyEnd == 0)
            return true;
        else return false;
    }

    public bool IsDailyQuestCompleted(DailyQuest dailyQuest)
    {
        return dailyQuest.IsCompleted();
    }

    public List<DailyQuest> GetAllDailyQuests()
    {
        return DailyQuestSynchronizer.Instance.GetAllDailyQuests();
    }

    public DailyQuest GetDailyQuestForConcreteDay(int day)
    {
        return DailyQuestSynchronizer.Instance.GetDailyQuestWithID(day);
    }

    public DailyQuest GetCurrentDailyQuest()
    {
        return DailyQuestSynchronizer.Instance.GetDailyQuestWithID(DailyQuestSynchronizer.Instance.DayCounter);
    }

    public DailyQuest GetDailyQuestFromYesterday()
    {
        int d = DailyQuestSynchronizer.Instance.DayCounter;
        if (d == 0)
            return null;
        else
            return GetDailyQuestForConcreteDay(d - 1);
    }

    public bool CurrentDailyHasTaskType(DailyTask.DailyTaskType type)
    {
        if (currentDailyQuest != null)
            return currentDailyQuest.HasTaskType(type);
        else return false;
    }

    public bool CurrentDailyHasTaskTypeWithoutCompletion(DailyTask.DailyTaskType type)
    {
        if (currentDailyQuest != null)
            return currentDailyQuest.HasTaskType(type) && !currentDailyQuest.IsCompleted();
        else return false;
    }

    public int CurrentDailyProgressGoalForTaskType(DailyTask.DailyTaskType type)
    {
        if (currentDailyQuest != null)
        {
            return currentDailyQuest.GetProgressGoalForTaskType(type);
        }
        else return 0;
    }

    private void SetCurrentDailyQuest(int currentDay)
    {
        if (currentDailyQuest != null)
            currentDailyQuest.OnDestroy();

        currentDailyQuest = GetDailyQuestForConcreteDay(currentDay);
        currentDailyQuest.Init();

        UIController.getHospital.PatientCard.ResetShowedListForDailyQuest();
        UIController.getHospital.HospitalInfoPopUp.ResetInfoShowed();

        UpdateCurrentDailyQuestWithFB();
        UIController.getHospital.DailyQuestMainButtonUI.Refresh();
    }

    public DailyTask ReplaceCurrentDailyTask(int taskID)
    {
        if (dailyTaskReplacerController == null)
            dailyTaskReplacerController = new DailyTaskReplacerController();

        DailyTask newTask = dailyTaskReplacerController.GetRandomReplacerDailyTask(DailyQuestSynchronizer.Instance.DayCounter);
        newTask.Init();

        currentDailyQuest.replacementTaskCounter++;
        currentDailyQuest.AssignTaskToDailyQuest(taskID, newTask);

        DailyQuestSynchronizer.Instance.RemoveTaskFromReplaceTaskList(newTask.taskType);

        if (newTask.taskType == DailyTask.DailyTaskType.TapOnAPatient)
            UIController.getHospital.PatientCard.ResetShowedListForDailyQuest();
        else if (newTask.taskType == DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses)
            UIController.getHospital.HospitalInfoPopUp.ResetInfoShowed();
        else if (newTask.taskType == DailyTask.DailyTaskType.ConnectToFacebook)
            UpdateCurrentDailyQuestWithFB();


        return newTask;
    }

    public bool CanReplaceTasks()
    {
        return DailyQuestSynchronizer.Instance.CanReplaceTasks();
    }

    public void UpdateCurrentDailyQuestWithFB()
    {
        if (currentDailyQuest != null)
        {
            currentDailyQuest.UpdateTasksDependentOfFacebook(GameState.Get().EverLoggedInFB);
        }
    }

    public int GetCompletedQuestsCount()
    {
        List<DailyQuest> quests = DailyQuestSynchronizer.Instance.GetAllDailyQuests();
        int count = 0;
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].IsCompleted())
                count++;
        }
        return count;
    }

    public List<RewardPackage> GetWeeklyRewards()
    {
        RewardPackageManager rewardPackageManager = new RewardPackageManager();
        return rewardPackageManager.GetRewardPackageForWeeklySummary();
    }

    private void ResetTimer()
    {
        currentDayTime = 0;
    }

    public bool isDayPassed()
    {
        if (currentDayTime >= DAY)
            return true;
        else return false;
    }

    public bool isWeekPassed()
    {
        if (DailyQuestSynchronizer.Instance.DayCounter > 6)
            return true;
        else return false;
    }

    public int TimeTillNextDay()
    {
        int val = DAY - currentDayTime;

        if (val < 0)
            return 0;
        else return val;
    }

    public int HowManyDaysHavePassed(int calcedTime)
    {
        int currentWeekTime = calcedTime - (DailyQuestSynchronizer.Instance.WeeklyEnd - WEEK);
        int currentDay = Mathf.Clamp(currentWeekTime / DAY, 0, 7);

        return currentDay;
    }

    public int GetCurrentDayNumber()
    {
        return DailyQuestSynchronizer.Instance.DayCounter;
    }

    public int GetReplacementCost()
    {
        if (GetCurrentDailyQuest() != null)
        {
            int val = GetCurrentDailyQuest().replacementTaskCounter;

            if (val > 8) return 50;
            else
            {
                int cost = Mathf.FloorToInt(Mathf.Pow(0.1994f * val, 2) + 4.9606f * val);
                if (cost == 0) return 1;
                else return cost;
            }
        }
        else
        {
            return 1;
        }

        /* OLD STUFF BY PIERA
        int val = DailyQuestSynchronizer.Instance.ReplacementTaskCounter;

        if (val > 8) return 50;
        else
        {
            int cost = Mathf.FloorToInt(Mathf.Pow(0.3194f * val, 2) + 2.6806f * val);
            if (cost == 0) return 1;
            else return cost;
        }
        */
    }

    public void ClaimRewardForQuest(DailyQuest quest)
    {
        RewardPackageManager rewardPackageManager = new RewardPackageManager();
        RewardPackage rewardPackage = rewardPackageManager.GetRewardPackageForConcreteDailyQuest(quest);
        if (rewardPackage == null)
        {
            Debug.LogError("there is no reward for given quest! Quest has completed tasks: " + quest.GetCompletedTasksCount());
            return;
        }
        ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromDailyQuest = true;
        ((HospitalCasesManager)AreaMapController.Map.casesManager).AddDailyQuestRewardToStack(rewardPackage);
        UIController.getHospital.unboxingPopUp.OpenDailyQuestRewardPopup();

        rewardPackage.Collect();
        quest.RewardForDailyQuestClaimed();

        // Refresh daily quest popup UI
        UIController.getHospital.DailyQuestPopUpUI.Refresh();

        AnalyticsController.instance.ReportDailyQuestRewardOpened(rewardPackage);
    }

    public void ClaimRewardWeekly(List<RewardPackage> rewardsToClaim)
    {
        Debug.Log("ClaimRewardWeekly");
        if (rewardsToClaim == null || rewardsToClaim.Count < 1)
        {
            Debug.LogError("SOMETHING WENT TERRIBLY WRONG!");
            return;
        }

        RewardPackage firstReward = null;
        ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromDailyQuest = true;
        for (int i = 0; i < rewardsToClaim.Count; i++)
        {
            if (rewardsToClaim[i] != null)
            {
                if (firstReward == null)
                {
                    firstReward = rewardsToClaim[i];
                }
                ((HospitalCasesManager)AreaMapController.Map.casesManager).AddDailyQuestRewardToStack(rewardsToClaim[i]);
            }
        }

        if (firstReward == null)
        {
            Debug.LogError("There is no weekly reward to collect");
            return;
        }

        UIController.getHospital.unboxingPopUp.OpenDailyQuestRewardPopup();


        for (int i = 0; i < rewardsToClaim.Count; i++)
        {
            if (rewardsToClaim[i] != null)
            {
                rewardsToClaim[i].Collect();
                AnalyticsController.instance.ReportDailyQuestRewardOpened(rewardsToClaim[i]);
            }
        }
    }

    public void WeeklyRestart()
    {
        AnalyticsController.instance.ReportDailyWeekFinished(GetAllDailyQuests());

        if (dailyQuestCreator == null)
            dailyQuestCreator = new DailyQuestGenerator();

        if (dailyTaskReplacerController == null)
            dailyTaskReplacerController = new DailyTaskReplacerController();


        Timing.KillCoroutine(UpdateTimeCoroutine().GetType());

        DailyQuestSynchronizer.Instance.SetDailyQuests(dailyQuestCreator.GenerateDailyQuestesForEntireWeek());

        dailyTaskReplacerController.ResetDailyTaskForReplacement();
        DailyQuestSynchronizer.Instance.SetTasksForReplace(dailyTaskReplacerController.GetDailyTaskListForReplacement());

        currentDayTime = 0;
        DailyQuestSynchronizer.Instance.DayCounter = 0;
        DailyQuestSynchronizer.Instance.WeekCounter++;
        DailyQuestSynchronizer.Instance.WeeklyEnd = GlobalTime() + WEEK;

        ReferenceHolder.GetHospital().dailyQuestController.StartDailyQuestsSystem();
        UIController.getHospital.DailyQuestMainButtonUI.InitTimer();
        UIController.getHospital.DailyQuestMainButtonUI.Refresh();

        Hospital.HospitalPatientAI.ResetAllPatientsDailyQuest();
        Hospital.VIPPersonController.ResetAllVIPPatientsDailyQuest();

        UIController.getHospital.PatientCard.ResetShowedListForDailyQuest();

        if (!UIController.getHospital.unboxingPopUp.isActiveAndEnabled)
        {
            UIController.getHospital.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyQuest);
        }
        //UIController.getHospital.DailyQuestPopUpUI.Open();
    }

    public void FirstWeeklyStart()
    {
        if (dailyQuestCreator == null)
            dailyQuestCreator = new DailyQuestGenerator();

        if (dailyTaskReplacerController == null)
            dailyTaskReplacerController = new DailyTaskReplacerController();

        Timing.KillCoroutine(UpdateTimeCoroutine().GetType());

        DailyQuestSynchronizer.Instance.SetDailyQuests(dailyQuestCreator.GenerateDailyQuestsForFirstWeekEver());

        dailyTaskReplacerController.ResetDailyTaskForReplacement();
        DailyQuestSynchronizer.Instance.SetTasksForReplace(dailyTaskReplacerController.GetDailyTaskListForReplacement());

        currentDayTime = 0;
        DailyQuestSynchronizer.Instance.DayCounter = 0;
        DailyQuestSynchronizer.Instance.WeekCounter++;
        DailyQuestSynchronizer.Instance.WeeklyEnd = GlobalTime() + WEEK;

        ReferenceHolder.GetHospital().dailyQuestController.StartDailyQuestsSystem();
        UIController.getHospital.DailyQuestMainButtonUI.InitTimer();
        UIController.getHospital.DailyQuestMainButtonUI.Refresh();

        if ((TutorialController.Instance.IsTutorialStepCompleted(StepTag.daily_quests_button_arrow) && TutorialSystem.TutorialController.ShowTutorials) || TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.daily_quests_button_arrow, true))
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.UnlockDailyQuests));
    }

    private int GlobalTime()
    {
        int a = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
        return a;
    }

    public void StartDailyQuestsSystem()
    {
        //Debug.Log("StartDailyQuestsSystem");


        Timing.KillCoroutine(UpdateTimeCoroutine().GetType());

        EmulateTime();

        if (!isWeekPassed())
        {
            SetCurrentDailyQuest(DailyQuestSynchronizer.Instance.DayCounter);
            Timing.RunCoroutine(UpdateTimeCoroutine());
        }

    }


    private void EmulateTime()
    {
        if (DailyQuestSynchronizer.Instance.WeeklyEnd > 0)
        {
            int currentTime = GlobalTime();
            int dayBeforeEmulate = DailyQuestSynchronizer.Instance.DayCounter;
            int dayAfterEmulate = HowManyDaysHavePassed(currentTime);

            if (dayAfterEmulate > dayBeforeEmulate)
            {
                int difference = dayAfterEmulate - dayBeforeEmulate;
                for (int i = 0; i < difference; i++)
                {
                    int dayIndex = dayBeforeEmulate + i;
                    if (dayIndex > 6)
                    {
                        Debug.Log("Week is over. Not reporting more daily quests finished. Everything is correct.");
                        break;
                    }

                    AnalyticsController.instance.ReportDailyQuestFinished(GetAllDailyQuests()[dayIndex], dayIndex + 1);
                }
            }

            DailyQuestSynchronizer.Instance.DayCounter = dayAfterEmulate;
            int timeInWeek = currentTime - (DailyQuestSynchronizer.Instance.WeeklyEnd - WEEK);
            int timeInDay = timeInWeek - DailyQuestSynchronizer.Instance.DayCounter * DAY;
            this.currentDayTime = timeInDay;
        }
    }

    private IEnumerator<float> UpdateTimeCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(1f);

            if (!isWeekPassed())
            {
                currentDayTime = currentDayTime + timeScale;

                //Debug.Log("Current Day : " + DailyQuestSynchronizer.Instance.DayCounter + " Time to next Day: " + UIController.GetFormattedShortTime(TimeTillNextDay()));

                if (isDayPassed())
                {
                    AnalyticsController.instance.ReportDailyQuestFinished(currentDailyQuest, DailyQuestSynchronizer.Instance.DayCounter + 1);

                    currentDayTime = 0;
                    DailyQuestSynchronizer.Instance.DayCounter++;

                    if (isWeekPassed())
                    {
                        AnalyticsController.instance.ReportDailyWeekFinished(GetAllDailyQuests());
                    }

                    SetCurrentDailyQuest(DailyQuestSynchronizer.Instance.DayCounter);

                    UIController.getHospital.DailyQuestMainButtonUI.Refresh();

                    if (UIController.getHospital.DailyQuestPopUpUI.isActiveAndEnabled)
                    {
                        if (isWeekPassed())
                        {
                            UIController.getHospital.DailyQuestPopUpUI.DeactivateContent();
                            UIController.getHospital.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyQuest);
                            //UIController.getHospital.DailyQuestWeeklyUI.Open();
                        }
                        else
                            UIController.getHospital.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyQuest);
                        // UIController.getHospital.DailyQuestPopUpUI.Open();
                    }
                }
            }
            //  else Debug.LogError("Week passed");
        }
    }

    public void SetCurrentDailyQuestCompleted()
    {
        if (currentDailyQuest != null)
            currentDailyQuest.SetDailyQuestCompleted();
    }

    public void DebugNextDay()
    {
        DailyQuestSynchronizer.Instance.WeeklyEnd = DailyQuestSynchronizer.Instance.WeeklyEnd - TimeTillNextDay() - 10;
        EmulateTime();

        if (!isWeekPassed())
        {
            SetCurrentDailyQuest(DailyQuestSynchronizer.Instance.DayCounter);
        }
    }
}
