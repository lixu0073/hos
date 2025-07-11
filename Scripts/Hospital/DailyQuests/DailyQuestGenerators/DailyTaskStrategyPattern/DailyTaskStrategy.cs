using Hospital;
using System.Collections.Generic;
using UnityEngine;

public abstract class DailyTaskStrategy
{
    public abstract DailyTask GetDailyTask(DailyTask.DailyTaskType taskType, int dailyTaskOccurenceDay);

    protected int SetDailyTaskMaxProgress(int dailyTaskActivationDay, DailyTask.DailyTaskDifficulty dailyTaskDifficulty)
    {
        int maxProgressValueToReturn = 0;

        switch (dailyTaskDifficulty)
        {
            case DailyTask.DailyTaskDifficulty.Easy:
                {
                    maxProgressValueToReturn = 10 + dailyTaskActivationDay * 2;
                    break;
                }
            case DailyTask.DailyTaskDifficulty.Medium:
                {
                    maxProgressValueToReturn = Mathf.FloorToInt(5f + dailyTaskActivationDay * 0.75f);
                    break;
                }
            case DailyTask.DailyTaskDifficulty.Hard:
                {
                    maxProgressValueToReturn = Mathf.FloorToInt(1.0f + (dailyTaskActivationDay / 2.5f));
                    break;
                }
            default:
                {
                    Debug.Log("No daily task difficult type found. Fix it.");
                    break;
                }
        }

        return maxProgressValueToReturn;
    }

    protected int SetDailyTaskMaxProgressAcountingRestrictions(int calculatedMaxProgress, DailyTask.DailyTaskType taskType)
    {
        int maxProgressToReturn = 0;

        if (taskType == DailyTask.DailyTaskType.CompleteAntiEpidemicBoxes)
        {
            int playerLevel = Game.Instance.gameState().GetHospitalLevel();
            int maxAmmountOfPackages = 0;

            if (playerLevel <= 19)
            {
                maxAmmountOfPackages = 3;
            }
            else if (playerLevel > 19 && playerLevel <= 24)
            {
                maxAmmountOfPackages = 6;
            }
            else if (playerLevel > 24 && playerLevel <= 39)
            {
                maxAmmountOfPackages = 9;
            }
            else if (playerLevel > 39)
            {
                maxAmmountOfPackages = 12;
            }

            if (calculatedMaxProgress <= maxAmmountOfPackages)
            {
                maxProgressToReturn = calculatedMaxProgress;
            }
            else
            {
                maxProgressToReturn = maxAmmountOfPackages;
            }

            return maxProgressToReturn;
        }
        else if (taskType == DailyTask.DailyTaskType.LevelUp)
        {
            maxProgressToReturn = 1;
            return maxProgressToReturn;
        }
        else if (taskType == DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses)
        {
            int maxAmmountOfDoctorsOrNurses;
            List<ShopRoomInfo> listOfMakers = new List<ShopRoomInfo>();
            listOfMakers.AddRange(HospitalAreasMapController.HospitalMap.drawerDatabase.DrawerItems);
            List<ShopRoomInfo> doctorsForPlayerLevel = listOfMakers.FindAll(x => x is DoctorRoomInfo && x.unlockLVL <= Game.Instance.gameState().GetHospitalLevel());
            maxAmmountOfDoctorsOrNurses = doctorsForPlayerLevel.Count;
            if (calculatedMaxProgress <= maxAmmountOfDoctorsOrNurses)
            {
                maxProgressToReturn = calculatedMaxProgress;
            }
            else
            {
                maxProgressToReturn = maxAmmountOfDoctorsOrNurses;

            }
            return maxProgressToReturn;
        }
        else if (taskType == DailyTask.DailyTaskType.UnlockDailyQuests)
        {
            return 1;
        }
        else if (taskType == DailyTask.DailyTaskType.WhatsNext)
        {
            return 1; // according to documentation
        }
        else if (taskType == DailyTask.DailyTaskType.UseABooster)
        {
            return 1; // according to documentation
        }
        maxProgressToReturn = calculatedMaxProgress;
        return maxProgressToReturn;
    }
}