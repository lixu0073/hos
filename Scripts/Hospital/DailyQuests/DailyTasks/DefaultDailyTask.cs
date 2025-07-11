using UnityEngine;
using System.Collections;
using System;

public class DefaultDailyTask : DailyTask
{
    public DefaultDailyTask() : base() { }

    public DefaultDailyTask(DailyTaskType taskType, int amount)
    {
        base.taskType = taskType;
        //base.taskDifficulty = taskDifficulty;
        base.SetTaskObjectives(amount);
    }

    public override void OnSetDailyTaskCompleted()
    {
        SetDailyTaskCompleted();
    }

    public override string GetDescription()
    {
        if (taskType == DailyTaskType.ConnectToFacebook)
        {
            if (TaskProgressGoal == 1)
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CONNECT_GAME_TO_FACEBOOK"), progressGoal);
            else return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_FRIENDS_CAR"), progressGoal);
        }
        else return base.GetDescription();
    }

    public override string GetInfo()
    {
        if (taskType == DailyTaskType.ConnectToFacebook)
        {
            if (TaskProgressGoal == 1)
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CONNECT_GAME_TO_FACEBOOK_INFO");
            else return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_FRIENDS_CAR_INFO");
        }
        else return base.GetInfo();
    }

    public override string SaveToString()
    {
        return base.SaveToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
    }

}
