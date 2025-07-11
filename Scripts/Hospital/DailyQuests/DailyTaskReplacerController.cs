using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DailyTaskReplacerController : MonoBehaviour
{
    List<DailyTask.DailyTaskType> replacableTaskList;
    DailyTaskContext dailyTaskContext;

    public DailyTaskReplacerController()
    {
        replacableTaskList = new List<DailyTask.DailyTaskType>();
        dailyTaskContext = new DailyTaskContext();
        replacableTaskList.AddRange(DailyQuestSynchronizer.Instance.GetTaskListForReplace());
    }

    public DailyTask GetRandomReplacerDailyTask(int dayInWeek)
    {
        DailyTask dailyTaskToReturn;
        if (IsReplacableTaskListEmpty())
        {
            return null;
        }
        int maxIndex = GetMaxIndexOfNonDefaultDailyTask();
        int randomIndex = GameState.RandomNumber(maxIndex);
        DailyTask.DailyTaskType taskWidhrawn = replacableTaskList[randomIndex];
        replacableTaskList.RemoveAt(randomIndex);
        replacableTaskList.Add(DailyTask.DailyTaskType.Default);
        dailyTaskToReturn = dailyTaskContext.GetConcreteDailyTask(taskWidhrawn, dayInWeek);
        return dailyTaskToReturn;
    }

    public List<DailyTask.DailyTaskType> GetDailyTaskListForReplacement()
    {
        return replacableTaskList;
    }

    public void ResetDailyTaskForReplacement()
    {
        replacableTaskList.Clear();
        replacableTaskList.AddRange(ResourcesHolder.GetHospital().dailyTaskDatabase.GetDailyTaskList());

        // temporary solution - task removed untill functionality will work good
        replacableTaskList.Remove(DailyTask.DailyTaskType.ConnectToFacebook);
        replacableTaskList.Remove(DailyTask.DailyTaskType.DiagnosePatients);
        replacableTaskList.Remove(DailyTask.DailyTaskType.PreventBacteriaSpread);

        FilterOutCurrentDailyTask();
        FilterOutTutorialDailyTask();
        FilterOutLevelThresholdDailyTask();
    }

    private void FilterOutLevelThresholdDailyTask()
    {
        replacableTaskList.RemoveAll(x => Game.Instance.gameState().GetHospitalLevel() < ResourcesHolder.GetHospital().dailyTaskDatabase.GetDailyTaskMinimumLevel(x));
    }

    private void FilterOutTutorialDailyTask()
    {
        replacableTaskList.RemoveAll(x => ResourcesHolder.GetHospital().dailyTaskDatabase.IsDailyTaskOnllyForFirstWeek(x));
    }

    private void FilterOutCurrentDailyTask()
    {
        List<DailyTask.DailyTaskType> temp = new List<DailyTask.DailyTaskType>();
        temp.AddRange(DailyQuestSynchronizer.Instance.GetAllCurrentWeekDailyTaskTypes());
        for (int i = 0; i < temp.Count; i++)
        {
            if (replacableTaskList.Contains(temp[i]))
            {
                replacableTaskList.Remove(temp[i]);
            }
        }
    }

    private int GetMaxIndexOfNonDefaultDailyTask()
    {
        int indexToReturn = replacableTaskList.IndexOf(DailyTask.DailyTaskType.Default);
        if (indexToReturn < 0)
            return 0;
        else return indexToReturn;
    }

    private bool IsReplacableTaskListEmpty()
    {
        if (replacableTaskList.Count == 0)
        {
            return true;
        }

        int indexOfDefaultTask = replacableTaskList.IndexOf(DailyTask.DailyTaskType.Default);
        List<DailyTask.DailyTaskType> defaultTaskList = replacableTaskList.FindAll(x => x == DailyTask.DailyTaskType.Default);
        if (indexOfDefaultTask == 0)
        {
            return true;
        }

        return false;
    }
}
