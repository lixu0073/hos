using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CurePatientsDailyTaskStrategy : DailyTaskStrategy
{
    private List<DoctorRoomInfo> doctorsAvaiable;

    public CurePatientsDailyTaskStrategy()
    {
        doctorsAvaiable = new List<DoctorRoomInfo>();
        for (int i = 0; i < ResourcesHolder.GetHospital().ClinicDiseases.Count; i++)
        {
            if (ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor.unlockLVL <= Game.Instance.gameState().GetHospitalLevel())
            {
                doctorsAvaiable.Add(ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor);
            }
        }
    }

    public override DailyTask GetDailyTask(DailyTask.DailyTaskType taskType, int dailyTaskOccurenceDay)
    {
        int amountOfMaxProgress = SetDailyTaskMaxProgress(dailyTaskOccurenceDay, ResourcesHolder.GetHospital().dailyTaskDatabase.GetDailyTaskDifficulty(taskType));
        amountOfMaxProgress = SetDailyTaskMaxProgressAcountingRestrictions(amountOfMaxProgress, taskType);
        DoctorRoomInfo doctorInfoForTask = GetRandomDoctor();
        return new RoomDailyTask(taskType, doctorInfoForTask, amountOfMaxProgress);
    }

    private DoctorRoomInfo GetRandomDoctor()
    {
        int index = GameState.RandomNumber(0, doctorsAvaiable.Count);
        DoctorRoomInfo tempInfo = doctorsAvaiable[index];
        return tempInfo;
    }
}
