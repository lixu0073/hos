using UnityEngine;
using System.Collections;
using System;

public class DefaultDailyTaskStrategy : DailyTaskStrategy
{
    public override DailyTask GetDailyTask(DailyTask.DailyTaskType taskType, int dailyTaskOccurenceDay)
    {
        int amountOfMaxProgress = SetDailyTaskMaxProgress(dailyTaskOccurenceDay, ResourcesHolder.GetHospital().dailyTaskDatabase.GetDailyTaskDifficulty(taskType));
        amountOfMaxProgress = SetDailyTaskMaxProgressAcountingRestrictions(amountOfMaxProgress, taskType);
        return new DefaultDailyTask(taskType, amountOfMaxProgress);
    }
}
