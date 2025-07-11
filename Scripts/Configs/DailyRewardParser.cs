using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class DailyRewardParser
{
    public enum PopupType
    {
        standard = 0,
        stairs = 1,
    }

    public enum WeekNomenclature
    {
        week1 = 0,
        week2 = 1,
        week3 = 2,
        week4 = 3,
        week5 = 4,
        week6 = 5
    }

    private const char REWARD_PACKAGE_SEPARATOR = '!';
    //private const string incrementValueKey = "incrementValue";

    public static Dictionary<int, List<DailyRewardModel>> rewardsPerWeekMap = new Dictionary<int, List<DailyRewardModel>>();
    public static Dictionary<int, BaseGiftableResource> bigRewardBoxForWeek = new Dictionary<int, BaseGiftableResource>();
    //public static Dictionary<string, object> unparsedParameters = new Dictionary<string, object>();

    public static float[] incrementValue = null;

    //public static DailyRewardsCData DailyRewardsCData;

    //public static void SaveUnparsedData(Dictionary<string, object> parameters)
    //{
    //    unparsedParameters = parameters;
    //}
    //public static void SaveUnparsedData(DailyRewardsCData dailyRewardsCData)
    //{
    //    DailyRewardsCData = dailyRewardsCData;
    //}


    //defaultIncrementValueDailyRewardConfig = "0.065!0.085!0.105!0.145!0.185!0.225!0.265",
    //defaultWeekDailyRewardConfig = "coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!coin;#coin1!bundle;1|1||1@specialBox"

    public static void InitializeDailyRewards(DailyRewardsCData dailyRewardsCData)
    {
        rewardsPerWeekMap.Clear();
        bigRewardBoxForWeek.Clear();

        string[] values = dailyRewardsCData.defaultIncrementValueDailyRewardConfig.Split(REWARD_PACKAGE_SEPARATOR);

        incrementValue = new float[values.Length];

        for (int i = 0; i < incrementValue.Length; ++i)
        {
            incrementValue[i] = float.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture);
        }

        List<DailyRewardModel> listOfDailyRewardsForWeek = new List<DailyRewardModel>();

        int weekNumber = 0;
        string unparsedData = dailyRewardsCData.defaultWeekDailyRewardConfig;
        string[] unparsedGiftPackageRewardsPerWeek = unparsedData.Split(REWARD_PACKAGE_SEPARATOR);
        for (int i = 0; i < unparsedGiftPackageRewardsPerWeek.Length; i++)
        {
            BaseGiftableResource gift = BaseGiftableResourceFactory.CreateDailyRewardGiftableFromString(unparsedGiftPackageRewardsPerWeek[i], i, EconomySource.DailyRewards);
            if (i < 7)
            {
                if (gift != null)
                    listOfDailyRewardsForWeek.Add(new DailyRewardModel(gift));
                else
                    break;
            }
            else if (i == 7)
            {
                if (gift != null)
                    bigRewardBoxForWeek.Add(weekNumber, gift);
                else
                    break;
            }
        }
        if (listOfDailyRewardsForWeek.Count == WorldWideConstants.DAYS_IN_WEEK)
            rewardsPerWeekMap.Add(weekNumber, new List<DailyRewardModel>(listOfDailyRewardsForWeek));
        else
            Debug.LogError("There are 7 days in a week");
           
    }    

    public static string SaveWeeklyDailyRewardsToString(this List<DailyRewardModel> dailyRewardModels)
    {
        StringBuilder sb = new StringBuilder();
        foreach (DailyRewardModel dailyReward in dailyRewardModels)
        {
            sb.Append(dailyReward.GetDailyRewardGift().GiftableToString());
            sb.Append(REWARD_PACKAGE_SEPARATOR);
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
    public static List<DailyRewardModel> LoadWeeklyDailyRewardsFromString(string weeklyRewardsString)
    {
        List<DailyRewardModel> toReturn = new List<DailyRewardModel>();
        string[] unparsedGiftPackageRewardsPerWeek = weeklyRewardsString.Split(REWARD_PACKAGE_SEPARATOR);
        if (unparsedGiftPackageRewardsPerWeek.Length > 0)
        {
            foreach (string unparsedReward in unparsedGiftPackageRewardsPerWeek)
            {
                try
                {
                    BaseGiftableResource resource = BaseGiftableResourceFactory.CreateGiftableFromString(unparsedReward, EconomySource.DailyRewards);
                    if (resource != null)
                        toReturn.Add(new DailyRewardModel(resource));
                    else
                        Debug.LogError("Bad string: " + unparsedReward);
                }
                catch (Exception)
                {
                    Debug.LogError("Bad string: " + unparsedReward);
                }
            }
        }
        return toReturn;
    }

    public static int HowManyWeeksAreUnique()
    {
        return rewardsPerWeekMap.Count;
    }

    public static bool IsDataExists()
    {
        return rewardsPerWeekMap.Count > 0;
    }

    //public static void InitializeMockedDailyRewards()
    //{
    //    Dictionary<string, object> parameters = new Dictionary<string, object>()
    //    {
    //        {"week1", ResourcesHolder.Get().BundledRewardsDefaults.defaultWeekDailyRewardConfig},
    //        {incrementValueKey, ResourcesHolder.Get().BundledRewardsDefaults.defaultIncrementValueDailyRewardConfig}
    //    };
    //    InitializeDailyRewards(parameters);
    //}

}
