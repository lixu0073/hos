using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class CheaterCalculator
{

    private static Dictionary<Condition, int> defaults = new Dictionary<Condition, int>()
    {
        { Condition.Rich, 1000 },
        { Condition.Income, 500 },
        { Condition.QuickBuck, 1000 },
        { Condition.Progress, 1000 },
        { Condition.AppId, 50 },
        { Condition.PharmacyItems, 500 },
        { Condition.GEcontribution, 60 },
        { Condition.Platform, 20 },
        { Condition.Week, 10 },
        { Condition.HighLevel, 40 },
        { Condition.Language, 15 },
        { Condition.LowLevel, 5 }
    };

    private static Dictionary<string, int> cheatLanguages = new Dictionary<string, int>()
    {
        { "Chinese traditional", 0 },
        { "Chinese simplified", 0 },
        { "Indonesian", 0 },
        { "Russian", 0 },
        { "Thai", 0 },
        { "Turkish", 0 }
    };

    public enum Condition
    {
        Rich,
        Income,
        QuickBuck,
        Progress,
        AppId,
        PharmacyItems,
        GEcontribution,
        Platform,
        Week,
        HighLevel,
        Language,
        LowLevel
    }

    public static int GetScore()
    {
        int score = 0;
        foreach (Condition condition in Enum.GetValues(typeof(Condition)))
        {
            if (IsConditionComplete(condition))
                score += GetScoreForCondition(condition);
        }
        return score;
    }

    private static bool IsConditionComplete(Condition condition)
    {
        switch(condition)
        {
            case Condition.Rich:
                return HasMoreDiamonds(111111) || HasMoreCoins(33333333) || HasMorePositiveEnergy(3333333);
            case Condition.Income:
                return HasMoreDiamonds(1000) && HasLessIAPThan(1);
            case Condition.QuickBuck:
                return HasMoreDiamonds(50000) && PlayTimeLessThan(432000);
            case Condition.Progress:
                return HasLevelUpThan(60) && PlayTimeLessThan(432000);
            case Condition.AppId:
                return !HasCorrectPackageName();
            case Condition.PharmacyItems:
                return HasAnyOfferInPharmacyWithAmountUpTo(10);
            case Condition.GEcontribution:
                return HasContributionMoreThan(5000);
            case Condition.Platform:
                return IsAndroidPlatform();
            case Condition.Week:
                return PlayTimeLessThan(691200);
            case Condition.HighLevel:
                return HasLevelUpThan(120);
            case Condition.Language:
                return IsCheatLanguageSet();
            case Condition.LowLevel:
                return HasLevelLessThan(11);
        }
        return false;
    }

    #region Methos

    private static int GetScoreForCondition(Condition condition)
    {
        int score = DefaultConfigurationProvider.GetConfigCData().CheaterScoreConditions[condition.ToString()];
        if (score != -1)
            return score;
        if(defaults.ContainsKey(condition))
            return defaults[condition];
        return 0;
    }

    private static bool HasMoreDiamonds(int diamonds)
    {
        return Game.Instance.gameState().GetDiamondAmount() > diamonds;
    }

    private static bool HasMoreCoins(int coins)
    {
        return Game.Instance.gameState().GetCoinAmount() > coins;
    }

    private static bool HasMorePositiveEnergy(int positiveEnergy)
    {
        return Game.Instance.gameState().GetPositiveEnergyAmount() > positiveEnergy;
    }

    private static bool HasLessIAPThan(int target)
    {
        return Game.Instance.gameState().GetIAPPurchasesCount() < target;
    }

    private static bool HasLevelUpThan(int level)
    {
        return Game.Instance.gameState().GetHospitalLevel() > level;
    }

    private static bool HasLevelLessThan(int level)
    {
        return Game.Instance.gameState().GetHospitalLevel() < level;
    }

    private static bool HasCorrectPackageName()
    {
        return Application.identifier == "com.cherrypickgames.myhospital";
    }

    private static bool IsAndroidPlatform()
    {
#if ANDROID
        return true;
#else
        return false;
#endif
    }

    private static bool HasAnyOfferInPharmacyWithAmountUpTo(int amount)
    {
        foreach(PharmacyOrder order in UIController.getHospital.PharmacyPopUp.GetOrders())
        {
            if (order.amount > amount)
                return true;
        }
        return false;
    }

    private static bool HasContributionMoreThan(int contrib)
    {
        return ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress > contrib;
    }

    private static bool PlayTimeLessThan(int seconds)
    {
        string firstLaunchDateStr = CacheManager.GetFirstLaunchDate();
        try
        {
            DateTime firstLaunchDate = DateTime.ParseExact(firstLaunchDateStr + " UTC", "M/d/yyyy h:mm:ss tt UTC", System.Globalization.CultureInfo.InvariantCulture);
            return (DateTime.UtcNow - firstLaunchDate).TotalSeconds < seconds;
        }
        catch (Exception e)
        {
            Debug.Log("CHEATER SCORE: " + e.Message);
            return false;
        }
    }

    private static bool IsCheatLanguageSet()
    {
        string userLang = I2.Loc.LocalizationManager.CurrentLanguage;
        if (!string.IsNullOrEmpty(userLang))
        {
            return cheatLanguages.ContainsKey(userLang);
        }
        return false;
    }

#endregion

}
