using UnityEngine;
using System.Collections;

public class RewardPackageGenerator {

    private DailyQuestRewardFactory rewardForOneDailyTask;
    private DailyQuestRewardFactory rewardForTwoDailyTask;
    private DailyQuestRewardFactory rewardForThreeDailyTask;
    private DailyQuestRewardFactory rewardForThreeDailyQuests;
    private DailyQuestRewardFactory rewardForSevenDailyQuests;

    public RewardPackageGenerator()
    {
         rewardForOneDailyTask = new RewardsForOneDailyTask();
         rewardForTwoDailyTask = new RewardsForTwoDailyTask();
         rewardForThreeDailyTask = new RewardsForThreeTask();
         rewardForThreeDailyQuests = new RewardsForThreeDailyQuests();
         rewardForSevenDailyQuests = new RewardsForSevenDailyQuests();
    }

    public RewardPackage GetRewardPackageForDailyQuest(DailyQuest dailyQuest, int dayNumber)
    {
        RewardPackage rewardToReturn;
        int numberOfTaskCompleted = 0;
        for (int i = 0; i < dailyQuest.taskCollection.Length; i++)
        {
            if (dailyQuest.taskCollection[i].IsCompleted())
            {
                numberOfTaskCompleted++;
            }
        }

        switch (numberOfTaskCompleted)
        {
            case 1:
                rewardToReturn = rewardForOneDailyTask.GetRewardPackage(dayNumber);
                break;
            case 2:
                rewardToReturn = rewardForTwoDailyTask.GetRewardPackage(dayNumber);
                break;
            case 3:
                rewardToReturn = rewardForThreeDailyTask.GetRewardPackage(dayNumber);
                break;
            default:
                rewardToReturn = null;
                Debug.Log("No rewardPackage");
                break;
        }

        return rewardToReturn;
    }

    public RewardPackage GetRewardForThreeDays()
    {
        RewardPackage rewardPackageToReturn = rewardForThreeDailyQuests.GetRewardPackage(0);
        return rewardPackageToReturn;
    }

    public RewardPackage GetRewardForSevenDays()
    {
        RewardPackage rewardPackageToReturn = rewardForSevenDailyQuests.GetRewardPackage(0);
        return rewardPackageToReturn;
    }
}
