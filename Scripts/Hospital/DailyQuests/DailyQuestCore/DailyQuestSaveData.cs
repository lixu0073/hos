using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DailyQuestSaveData
{ 
    public int weeklyEnd;
    public int dayCounter;
    public int weekCounter;

    public List<DailyQuest> dailyQuests = new List<DailyQuest>();
    public List<DailyTask.DailyTaskType> tasksForReplace = new List<DailyTask.DailyTaskType>();

    public DailyQuestSaveData()
    {
        weeklyEnd = 0;
        dayCounter = 0;
        weekCounter = 0;
    }
}
