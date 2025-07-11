using System.Text;
using System;
using IsoEngine;

public class DailyQuest
{
    public const int TASK_PER_DAILY_QUEST = 3;
    public DailyTask[] taskCollection = new DailyTask[TASK_PER_DAILY_QUEST];
    public bool IsDailyQuestRewardPackageClaimed { get; set; }

    public bool isStarAnimationNeeded = false;
    public bool isSwitchDayAnimationNeeded = true;
    public int replacementTaskCounter = 0;

    public DailyQuest()
    {
        IsDailyQuestRewardPackageClaimed = false;
        isStarAnimationNeeded = true;
        isSwitchDayAnimationNeeded = true;
        replacementTaskCounter = 0;
    }

    public void Init()
    {
        if (taskCollection != null && taskCollection.Length > 0)
            for (int i = 0; i < taskCollection.Length; ++i)
                if (taskCollection[i] != null)
                    taskCollection[i].Init();
    }

    public void SetDailyQuestCompleted()
    {
        for (int i = 0; i < taskCollection.Length; ++i)
            if (taskCollection[i] != null)
                taskCollection[i].SetDailyTaskCompleted();
    }

    public void AssignTaskToDailyQuest(int index, DailyTask task)
    {
        if (taskCollection != null && taskCollection.Length > 0)
        {
            if (index >= taskCollection.Length)
                throw new IsoException("Index is over size of list");

            if (taskCollection[index] != null)
                taskCollection[index].OnDestroy();

            taskCollection[index] = task;
        }
        else throw new IsoException("Problem with Assign task to daily quest 'cuz list it empty");
    }

    public bool IsCompleted()
    {
        for (int i = 0; i < taskCollection.Length; ++i)
            if (!taskCollection[i].IsCompleted())
                return false;

        return true;
    }

    public bool HasTaskType(DailyTask.DailyTaskType type)
    {
        for (int i = 0; i < taskCollection.Length; ++i)
            if (taskCollection[i].taskType == type)
                return true;

        return false;
    }

    public int GetProgressGoalForTaskType(DailyTask.DailyTaskType type)
    {
        for (int i = 0; i < taskCollection.Length; ++i)
            if (taskCollection[i] != null && taskCollection[i].taskType == type)
                return taskCollection[i].TaskProgressGoal;

        return 0;
    }

    public void UpdateTasksDependentOfFacebook(bool isFacebookConnected)
    {
        for (int i = 0; i < taskCollection.Length; ++i)
        {
            if (taskCollection[i] != null &&
                taskCollection[i].taskType == DailyTask.DailyTaskType.ConnectToFacebook &&
                !taskCollection[i].IsCompleted() && taskCollection[i].taskProgressCounter == 0)
            {
                if (!isFacebookConnected)
                {
                    taskCollection[i].SetTaskObjectives(1);
                    //Debug.LogError("Facebook was disconnected so it's a task");
                }
            }
        }
    }

    public int GetCompletedTasksCount()
    {
        int temp = 0;
        for (int i = 0; i < taskCollection.Length; ++i)        
            if (taskCollection[i] != null && taskCollection[i].IsCompleted())
                ++temp;

        return temp;
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < taskCollection.Length; i++)
        {
            if (taskCollection[i] != null)
                builder.Append(taskCollection[i].SaveToString());
            builder.Append("^");
        }
        builder.Append(Checkers.CheckedBool(IsDailyQuestRewardPackageClaimed).ToString());
        builder.Append("^");
        builder.Append(Checkers.CheckedBool(isStarAnimationNeeded).ToString());
        builder.Append("^");
        builder.Append(Checkers.CheckedBool(isSwitchDayAnimationNeeded).ToString());
        builder.Append("^");
        builder.Append(Checkers.CheckedAmount(replacementTaskCounter, 0, int.MaxValue, "ReplacementTaskCounter DQ").ToString());

        return builder.ToString();
    }

    public void LoadFromString(string saveString)
    {
        if (!string.IsNullOrEmpty(saveString))
        {
            var dailyQuestDataSave = saveString.Split('^');

            for (int i = 0; i < 3; ++i)
            {
                if (i >= taskCollection.Length)
                    break;

                var taskDataSave = dailyQuestDataSave[i].Split('!');
                DailyTask.DailyTaskType type;
                type = DailyTask.DailyTaskType.Default;
                try
                {
                    type = (DailyTask.DailyTaskType)Enum.Parse(typeof(DailyTask.DailyTaskType), taskDataSave[0]);
                }
                catch (Exception)
                {
                    //Debug.LogWarning("Daily Task Parsing Failed "+e.Message);
                    return;
                }

                if (type == DailyTask.DailyTaskType.CurePatientsForGivenDoctor)
                {
                    RoomDailyTask dailyTask = new RoomDailyTask();
                    dailyTask.LoadFromString(dailyQuestDataSave[i]);
                    AssignTaskToDailyQuest(i, dailyTask);
                }
                else
                {
                    DefaultDailyTask dailyTask = new DefaultDailyTask();
                    dailyTask.LoadFromString(dailyQuestDataSave[i]);
                    AssignTaskToDailyQuest(i, dailyTask);
                }
            }
            try
            {
                IsDailyQuestRewardPackageClaimed = Convert.ToBoolean(dailyQuestDataSave[3]);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                isStarAnimationNeeded = Convert.ToBoolean(dailyQuestDataSave[4]);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                isSwitchDayAnimationNeeded = Convert.ToBoolean(dailyQuestDataSave[5]);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                replacementTaskCounter = int.Parse(dailyQuestDataSave.Length > 6 ? dailyQuestDataSave[6] : "0", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return;
            }
        }
    }

    public void GeneratDefaultDailyQuest()
    {
        for (int i = 0; i < taskCollection.Length; ++i)
        {
            taskCollection[i] = new DefaultDailyTask(DailyTask.DailyTaskType.Default, 1);
            //Debug.Log("Generate Default Tasks fo Daily Quest");
        }
    }

    public void RewardForDailyQuestClaimed()
    {
        IsDailyQuestRewardPackageClaimed = true;
    }

    public bool IsRewardForDailyQuestClaimed()
    {
        return IsDailyQuestRewardPackageClaimed;
    }

    public void OnDestroy()
    {
        if (taskCollection != null && taskCollection.Length > 0)
        {
            for (int i = 0; i < taskCollection.Length; ++i)
                if (taskCollection[i] != null)
                    taskCollection[i].OnDestroy();
        }
    }
}
