using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using IsoEngine;

public class DailyQuestSynchronizer
{
    private DailyQuestSaveData data = new DailyQuestSaveData();

    private static DailyQuestSynchronizer instance = null;

    public static DailyQuestSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new DailyQuestSynchronizer();

            return instance;
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(Checkers.CheckedAmount(data.weeklyEnd, 0, int.MaxValue, "DailyQuest weeklyEnd: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(data.dayCounter, 0, 7, "DailyQuest dayCounter: ").ToString()); // 7 if week end
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(data.weekCounter, 0, int.MaxValue, "DailyQuest weekCounter: ").ToString()); // if week is over max int value
        builder.Append(";");

        for (int i = 0; i < data.dailyQuests.Count; i++)
        {
            builder.Append(data.dailyQuests[i].SaveToString());

            if (i < data.dailyQuests.Count - 1)
                builder.Append("?");
        }

        builder.Append(";");

        for (int i = 0; i < data.tasksForReplace.Count; i++)
        {
            builder.Append(data.tasksForReplace[i].ToString());

            if (i < data.tasksForReplace.Count - 1)
                builder.Append("?");
        }

        builder.Append(";");

        return builder.ToString();
    }

    public void LoadFromString(string saveString, bool visitingMode)
    {
        if (!visitingMode)
        {
            if (!string.IsNullOrEmpty(saveString))
            {
                var save = saveString.Split(';');

                data.weeklyEnd = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
                data.dayCounter = int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture);
                data.weekCounter = int.Parse(save[2], System.Globalization.CultureInfo.InvariantCulture);

                if (data.dailyQuests!=null && data.dailyQuests.Count>0)
                {
                    for (int i = 0; i < data.dailyQuests.Count; i++)
                        data.dailyQuests[i].OnDestroy();
                }

                data.dailyQuests.Clear();
                data.tasksForReplace.Clear();

                var questSaveData = save[3].Split('?');
                if (questSaveData.Length > 0)
                {
                    for (int i = 0; i < questSaveData.Length; i++)
                    {
                        var tmpDaily = new DailyQuest();
                        tmpDaily.LoadFromString(questSaveData[i]);
                        data.dailyQuests.Add(tmpDaily);
                    }
                }
                else
                {
                    throw new IsoEngine.IsoException("Can't load DailyQuest Data");
                }

                if (save.Length > 4)
                {
                    var tasksForReplace = save[4].Split('?');
                    if (tasksForReplace.Length > 0 && tasksForReplace[0] != "")
                    {
                        for (int i = 0; i < tasksForReplace.Length; i++)
                        {
                            var tmpTask = (DailyTask.DailyTaskType)Enum.Parse(typeof(DailyTask.DailyTaskType), tasksForReplace[i]);
                            data.tasksForReplace.Add(tmpTask);
                        }
                    }
                    else GenerateTaskReplace();
                }
                else GenerateTaskReplace();

                ReferenceHolder.GetHospital().dailyQuestController.StartDailyQuestsSystem();
                UIController.getHospital.DailyQuestMainButtonUI.InitTimer();
                UIController.getHospital.DailyQuestMainButtonUI.Refresh();
            }
            else
            {
                GenerateDefaultSave();
            }
        }
        else GenerateDefaultSave();
    }

    public void GenerateDefaultSave()
    {
        Debug.Log("Generating default save for DailyQuest.");

        // temporary solution 'cuz at default save should set weeklyEnd = 0 till player get interact with daily quest button during game
        data.weeklyEnd = 0;
        data.dayCounter = 0;

        // GENERATOR FOR TEST
        //SetDailyQuests(ReferenceHolder.Get().dailyQuestController.dailyQuestCreator.GenerateDailyQuestesForEntireWeek());

        for (int i = 0; i < 7; i++)
        {
            var tmpDailyQuest = new DailyQuest();
            tmpDailyQuest.GeneratDefaultDailyQuest();
            data.dailyQuests.Add(tmpDailyQuest);
        }

        GenerateTaskReplace();
    }

    public List<DailyTask.DailyTaskType> GetTaskListForReplace()
    {
        return data.tasksForReplace;
    }

    public void GenerateTaskReplace()
    {
        if (data.weeklyEnd == 0)
        {
            for (int i = 0; i < 7; i++)
            {
                data.tasksForReplace.Add(DailyTask.DailyTaskType.Default);
            }
        }
        else
        {
             if (ReferenceHolder.GetHospital().dailyQuestController.dailyTaskReplacerController == null)
                    ReferenceHolder.GetHospital().dailyQuestController.dailyTaskReplacerController = new DailyTaskReplacerController();

            ReferenceHolder.GetHospital().dailyQuestController.dailyTaskReplacerController.ResetDailyTaskForReplacement();
            SetTasksForReplace(ReferenceHolder.GetHospital().dailyQuestController.dailyTaskReplacerController.GetDailyTaskListForReplacement());


        }
    }

    public bool IsDailyQuestFuncionalityStarted()
    {
        return WeeklyEnd != 0;
    }

    public int WeeklyEnd
    {
        get { return data.weeklyEnd; }
        set
        {
            data.weeklyEnd = value;
        }
    }

    public int DayCounter
    {
        get { return data.dayCounter; }
        set
        {
            data.dayCounter = value;
        }
    }

    public int WeekCounter
    {
        get { return data.weekCounter; }
        set
        {
            data.weekCounter = value;
        }
    }

    public int ReplacementTaskCounter
    {
        private set { }
        get {
            int tmp = 0;

            if (data.tasksForReplace.Count > 0)
            {
                for (int i = 0; i< data.tasksForReplace.Count; i++)
                {
                    if (data.tasksForReplace[i] == DailyTask.DailyTaskType.Default)
                        tmp++;
                }
            }
            return tmp;
        }
    }

    public List<DailyQuest> GetAllDailyQuests()
    {
        return data.dailyQuests;
    }

    public List<DailyTask.DailyTaskType> GetAllCurrentWeekDailyTaskTypes()
    {
        List<DailyTask.DailyTaskType> listToReturn = new List<DailyTask.DailyTaskType>();
        for (int i = 0; i < data.dailyQuests.Count; i++)
        {
            for (int j = 0; j < DailyQuest.TASK_PER_DAILY_QUEST; j++)
            {
                listToReturn.Add(data.dailyQuests[i].taskCollection[j].taskType);
            }
        }
        return listToReturn;
    }

    public DailyQuest GetDailyQuestWithID(int index)
    {
        if (data.dailyQuests != null && data.dailyQuests.Count > 0)
        {
            if (index >= data.dailyQuests.Count)
                return data.dailyQuests[data.dailyQuests.Count - 1];

            return data.dailyQuests[index];
        }
        return null;
    }

    public void SetDailyQuests(DailyQuest[] dailyQuests)
    {
        if (data.dailyQuests != null)
        {
            if (data.dailyQuests.Count > 0)
                for (int i = 0; i < data.dailyQuests.Count; i++)
                    data.dailyQuests[i].OnDestroy();

            data.dailyQuests.Clear();
        }

        for (int i = 0; i < 7; i++)
        {
            data.dailyQuests.Add(dailyQuests[i]);
        }
    }

    public void SetTasksForReplace(List<DailyTask.DailyTaskType> tasks)
    {
        if (data.tasksForReplace != null)
            data.tasksForReplace.Clear();

        for (int i = 0; i < tasks.Count; i++)
        {
            data.tasksForReplace.Add(tasks[i]);
        }
    }

    public void RemoveTaskFromReplaceTaskList(DailyTask.DailyTaskType taskType)
    {
        if (data.tasksForReplace == null || data.tasksForReplace!=null && data.tasksForReplace.Count==0)
        {
            Debug.LogError("Can't remove from taskForReplace list cuz' is empty");
            return;
        }

        data.tasksForReplace.Remove(taskType);
        data.tasksForReplace.Add(DailyTask.DailyTaskType.Default);
    }

    public bool CanReplaceTasks()
    {
        if (data.tasksForReplace == null || data.tasksForReplace != null && data.tasksForReplace.Count == 0)
        {
            return false;
        }

        for (int i = 0; i<data.tasksForReplace.Count; i++)
        {
            if (data.tasksForReplace[i] != DailyTask.DailyTaskType.Default)
                return true;
        }

        return false;
    }

}
