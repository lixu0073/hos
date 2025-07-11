using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;

public class RoomDailyTask : DailyTask
{
    //string roomTag;
    public string doctorRoomTag;

    public RoomDailyTask() : base() { }

    public RoomDailyTask(DailyTaskType taskType, DoctorRoomInfo doctorRoom, int amount)
    {
        base.taskType = taskType;
        this.doctorRoomTag = doctorRoom.Tag;
       // base.taskDifficulty = taskDifficulty;
        //this.roomTag = roomTag;
        base.SetTaskObjectives(amount);
    }

    public override void OnSetDailyTaskCompleted()
    {
        SetDailyTaskCompleted();
    }

    public override string GetDescription()
    {
        string roomName = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(doctorRoomTag));
        return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_PATIENTS_FOR_DOCTOR"), progressGoal, roomName.ToUpper());
    }

    public override string GetInfo()
    {
        return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_PATIENTS_FOR_DOCTOR_INFO"), doctorRoomTag.ToUpper());
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(doctorRoomTag);
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        var taskDataSave = saveString.Split('!');

        if (ResourcesHolder.GetHospital().dailyTaskDatabase == null)
            Debug.LogError("dailyTaskDatabase jest odpięty ~ call Lukasz !");

        taskType = (DailyTaskType)Enum.Parse(typeof(DailyTaskType), taskDataSave[0]);
        taskProgressCounter = int.Parse(taskDataSave[1], System.Globalization.CultureInfo.InvariantCulture);
        SetTaskObjectives(int.Parse(taskDataSave[2], System.Globalization.CultureInfo.InvariantCulture));

        if (taskProgressCounter >= progressGoal)
            completed = true;
        else completed = false;


        doctorRoomTag = taskDataSave[3];
    }


    public override void AddProgress(DailyQuestProgressEventArgs args)
    {
        if (completed || args.roomTag != doctorRoomTag || taskType != args.taskType)
            return;

        taskProgressCounter += args.amount;

        if (taskProgressCounter >= progressGoal)
        {
            taskProgressCounter = progressGoal;
            SetDailyTaskCompleted();
            SaveSynchronizer.Instance.MarkToSave(SavePriorities.DailyQuest);
        }
    }
}
