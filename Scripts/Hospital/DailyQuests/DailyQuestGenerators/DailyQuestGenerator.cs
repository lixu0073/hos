using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DailyQuestGenerator
{

    private List<DailyTask.DailyTaskType> avaiableDailyTaskList;
    private DailyTaskContext dailyTaskContext;

    private const int dailyTasksPerDailyQuest = 3;

    public DailyQuestGenerator()
    {
        avaiableDailyTaskList = new List<DailyTask.DailyTaskType>();
        dailyTaskContext = new DailyTaskContext();
    }

    public DailyQuest[] GenerateDailyQuestesForEntireWeek()
    {
        ResetAvailableDailyTaskList();
        DailyQuest[] dailyQuestsToReturn = new DailyQuest[7];

        DailyTask.DailyTaskType drawnDailyTaskType;
        DailyTask dailyTaskTemp;
        int dayInWeek;

        for (int i = 0; i < dailyQuestsToReturn.Length; i++)
        {
            dayInWeek = i;
            dailyQuestsToReturn[i] = new DailyQuest();

            for (int j = 0; j < dailyTasksPerDailyQuest; j++)
            {
                drawnDailyTaskType = WithdrawOneRandomDailyTaskType();

                dailyTaskTemp = dailyTaskContext.GetConcreteDailyTask(drawnDailyTaskType, dayInWeek);
                if (dailyTaskTemp != null)
                {
                    dailyQuestsToReturn[i].AssignTaskToDailyQuest(j, dailyTaskTemp);
                }
                else
                {
                    Debug.LogError("DailyTask not created properly. Fix it");
                    dailyQuestsToReturn[i].AssignTaskToDailyQuest(j, null);
                }
            }
        }
        return dailyQuestsToReturn;
    }

    public DailyQuest[] GenerateDailyQuestsForFirstWeekEver()
    {
        ResetAvailableDailyTaskList();
        DailyQuest[] dailyQuestsToReturn = new DailyQuest[7];

        DailyTask.DailyTaskType drawnDailyTaskType;
        DailyTask dailyTaskTemp;
        int dayInWeek;

        for (int i = 0; i < dailyQuestsToReturn.Length; i++)
        {
            dayInWeek = i;
            dailyQuestsToReturn[i] = new DailyQuest();
            if (dayInWeek <= 2)
            {
                if (dayInWeek == 0)
                {
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(0, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.UnlockDailyQuests, dayInWeek));
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(1, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.TapTheDear, dayInWeek));
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(2, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.LevelUp, dayInWeek));
                }
                else if (dayInWeek == 1)
                {
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(0, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses, dayInWeek));
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(1, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.LikeOtherHospitals, dayInWeek));
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(2, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.TreatmentRoomPatients, dayInWeek));
                    avaiableDailyTaskList.Remove(DailyTask.DailyTaskType.TreatmentRoomPatients);
                }
                else if (dayInWeek == 2)
                {
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(0, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.PatientCardSwoosh, dayInWeek));
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(1, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.WhatsNext, dayInWeek));
                    dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(2, dailyTaskContext.GetConcreteDailyTask(DailyTask.DailyTaskType.BuyGoods, dayInWeek));
                }
               
            }
            else
            {
                for (int j = 0; j < dailyTasksPerDailyQuest; j++)
                {

                    drawnDailyTaskType = WithdrawOneRandomDailyTaskType();
                    dailyTaskTemp = dailyTaskContext.GetConcreteDailyTask(drawnDailyTaskType, dayInWeek);

                    if (dailyTaskTemp != null)
                    {
                        dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(j, dailyTaskTemp);
                    }
                    else
                    {
                        Debug.LogError("DailyTask not created properly. Fix it");
                        dailyQuestsToReturn[dayInWeek].AssignTaskToDailyQuest(j, null);
                    }
                }
            }
        }
        return dailyQuestsToReturn;
    }

    private void ResetAvailableDailyTaskList()
    {
        avaiableDailyTaskList.Clear();
        foreach (DailyTask.DailyTaskType taskType in ResourcesHolder.GetHospital().dailyTaskDatabase.GetDailyTaskList())
        {
            if (ResourcesHolder.GetHospital().dailyTaskDatabase.GetDailyTaskMinimumLevel(taskType) <= Game.Instance.gameState().GetHospitalLevel() && taskType != DailyTask.DailyTaskType.Default && taskType != DailyTask.DailyTaskType.ConnectToFacebook && taskType != DailyTask.DailyTaskType.DiagnosePatients && taskType != DailyTask.DailyTaskType.CompleteAntiEpidemicBoxes && taskType != DailyTask.DailyTaskType.PreventBacteriaSpread && !ResourcesHolder.GetHospital().dailyTaskDatabase.IsDailyTaskOnllyForFirstWeek(taskType))
            {
                avaiableDailyTaskList.Add(taskType);
            }
        }
    }

    private DailyTask.DailyTaskType WithdrawOneRandomDailyTaskType()
    {
        DailyTask.DailyTaskType taskTypeToReturn;

        if (avaiableDailyTaskList.Count == 0)
        {
            ResetAvailableDailyTaskList();
        }

        if (avaiableDailyTaskList != null && avaiableDailyTaskList.Count != 0)
        {
            int index = GameState.RandomNumber(0, avaiableDailyTaskList.Count);
            taskTypeToReturn = avaiableDailyTaskList[index];
            avaiableDailyTaskList.RemoveAt(index);
            return taskTypeToReturn;
        }
        return DailyTask.DailyTaskType.Default;
    }
}
