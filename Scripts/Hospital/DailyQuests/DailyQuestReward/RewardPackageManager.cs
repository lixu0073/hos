using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RewardPackageManager
{
    private RewardPackageGenerator rewardPackageGenerator;

    public RewardPackageManager()
    {
        rewardPackageGenerator = new RewardPackageGenerator();
    }


    public RewardPackage GetRewardPackageForConcreteDailyQuest(DailyQuest dailyQuest)
    {
        if (!dailyQuest.IsDailyQuestRewardPackageClaimed)
        {
            return rewardPackageGenerator.GetRewardPackageForDailyQuest(dailyQuest, DailyQuestSynchronizer.Instance.GetAllDailyQuests().IndexOf(dailyQuest));
        }
        return null;
    }

    public RewardPackage GetRewardPackageForConcreteDay(int day)
    {
        if (!DailyQuestSynchronizer.Instance.GetAllDailyQuests()[day].IsDailyQuestRewardPackageClaimed)
        {
            return rewardPackageGenerator.GetRewardPackageForDailyQuest(DailyQuestSynchronizer.Instance.GetAllDailyQuests()[day], day);
        }
        return null;
    }

    public RewardPackage[] GetRewardPackagesForAllDailyQuests()
    {
        RewardPackage[] rewardPackageToReturn = new RewardPackage[7];
        for (int i = 0; i < rewardPackageToReturn.Length; i++)
        {
            if (!DailyQuestSynchronizer.Instance.GetAllDailyQuests()[i].IsDailyQuestRewardPackageClaimed)
            {
                rewardPackageToReturn[i] = rewardPackageGenerator.GetRewardPackageForDailyQuest(DailyQuestSynchronizer.Instance.GetAllDailyQuests()[i], i);
            }
            else
            {
                rewardPackageToReturn[i] = null;
            }
        }
        return rewardPackageToReturn;
    }

    public List<RewardPackage> GetRewardPackageForWeeklySummary()
    {
        List<RewardPackage> rewardPackageListToReturn = new List<RewardPackage>();

        int DailyQuestCompleted = 0;

        rewardPackageListToReturn.Add(null);        //Super Grand Reward placeholder
        rewardPackageListToReturn.Add(null);        //Super Reward placeholder

        for (int i = 0; i < DailyQuestSynchronizer.Instance.GetAllDailyQuests().Count; i++)
        {
            if (DailyQuestSynchronizer.Instance.GetAllDailyQuests()[i].IsCompleted())
            {
                DailyQuestCompleted++;
            }
            if (!DailyQuestSynchronizer.Instance.GetAllDailyQuests()[i].IsDailyQuestRewardPackageClaimed)
            {
                RewardPackage reward = rewardPackageGenerator.GetRewardPackageForDailyQuest(DailyQuestSynchronizer.Instance.GetAllDailyQuests()[i], i);
                rewardPackageListToReturn.Add(reward);
            }
            else
            {
                rewardPackageListToReturn.Add(null);
            }
        }

        if (DailyQuestCompleted == 7)
        {
            RewardPackage SuperGrandReward = rewardPackageGenerator.GetRewardForSevenDays();
            RewardPackage SuperReward = rewardPackageGenerator.GetRewardForThreeDays();
            rewardPackageListToReturn[0] = SuperGrandReward;
            rewardPackageListToReturn[1] = SuperReward;
        }
        else if (DailyQuestCompleted >= 3)
        {
            RewardPackage SuperReward = rewardPackageGenerator.GetRewardForThreeDays();
            rewardPackageListToReturn[1] = SuperReward;
        }

        return rewardPackageListToReturn;
    }
}