using UnityEngine;
using System.Collections;

[System.Serializable]
public class DailyTaskRestrictions
{
    public DailyTask.DailyTaskType TaskType;

    public DailyTask.DailyTaskDifficulty TaskDifficulty;

    public int MinimumLevel;

    public bool IsOneTimeOnlyDailyTask;
}
