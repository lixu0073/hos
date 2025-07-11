using UnityEngine;
using System.Collections.Generic;

public class DailyTaskDataBase : ScriptableObject
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/DailyTaskDatabase")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<DailyTaskDataBase>();
    }
#endif

#pragma warning disable 0649
    [SerializeField] List<DailyTaskRestrictions> DailyTaskRestrictionList;
#pragma warning restore 0649

    public List<DailyTask.DailyTaskType> GetDailyTaskList()
    {
        List<DailyTask.DailyTaskType> listToReturn = new List<DailyTask.DailyTaskType>();

        if (DailyTaskRestrictionList != null && DailyTaskRestrictionList.Count > 0)
        {
            for (int i = 0; i < DailyTaskRestrictionList.Count; i++)
            {
                if (DailyTaskRestrictionList[i].TaskType != DailyTask.DailyTaskType.Default)
                    listToReturn.Add(DailyTaskRestrictionList[i].TaskType);
            }
            return listToReturn;
        }

        Debug.Log("No daily task in DailyTaskDataBase scriptable object. Fix it.");
        return null;
    }

    public List<DailyTaskRestrictions> GetDailyTaskRestrictions()
    {
        return DailyTaskRestrictionList;
    }

    public int GetDailyTaskMinimumLevel(DailyTask.DailyTaskType dailyTaskType)
    {
        if (DailyTaskRestrictionList != null && DailyTaskRestrictionList.Count > 0)
        {
            int valueToReturn = DailyTaskRestrictionList.Find(x => x.TaskType == dailyTaskType).MinimumLevel;
            return valueToReturn;
        }

        return -1;
    }

    public DailyTask.DailyTaskDifficulty GetDailyTaskDifficulty(DailyTask.DailyTaskType dailyTaskType)
    {
        if (DailyTaskRestrictionList != null && DailyTaskRestrictionList.Count > 0)
        {
            DailyTask.DailyTaskDifficulty valueToReturn = DailyTaskRestrictionList.Find(x => x.TaskType == dailyTaskType).TaskDifficulty;
            return valueToReturn;
        }

        return DailyTask.DailyTaskDifficulty.Default;
    }

    public bool IsDailyTaskOnllyForFirstWeek(DailyTask.DailyTaskType dailyTaskType)
    {
        if (DailyTaskRestrictionList != null && DailyTaskRestrictionList.Count > 0)
        {
            bool valueToReturn = DailyTaskRestrictionList.Find(x => x.TaskType == dailyTaskType).IsOneTimeOnlyDailyTask;
            return valueToReturn;
        }

        return false;
    }
}
