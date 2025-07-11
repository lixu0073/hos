using Hospital.LootBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public class MaternityBoxRewardGenerator
    {
        private readonly static Dictionary<string, object> DefaultData = new Dictionary<string, object>()
        {
            {"iapProduct", "box_1"},
            {"lootBoxType", "blue"},
            {"lootBoxStandardBooster", "RANDOM"},
            {"lootBoxStandardBoosterChance", "0"},
            {"lootBoxStandardBoosterMinAmount", "0"},
            {"lootBoxStandardBoosterMaxAmount", "0"},
            {"lootBoxPremiumBooster", "RANDOM"},
            {"lootBoxPremiumBoosterChance", "0"},
            {"lootBoxPremiumBoosterMinAmount", "0"},
            {"lootBoxPremiumBoosterMaxAmount", "0"},
            {"hardCurrency", "0"},
            {"hardMaxCurrency", "0"},
            {"lootBoxHardCurrencyChance", "0"},
            {"softCurrency", "50"},
            {"softMaxCurrency", "100"},
            {"lootBoxSoftCurrencyChance", "100"},
            {"lootBoxMinShovels", "1"},
            {"lootBoxMaxShovels", "1"},
            {"lootBoxShovelsChance", "70"},
            {"lootBoxMinPositiveEnergy", "0"},
            {"lootBoxMaxPositiveEnergy", "0"},
            {"lootBoxPositiveEnergyChance", "0"},
            {"lootBoxMinTankTools", "1"},
            {"lootBoxMaxTankTools", "2"},
            {"lootBoxTankToolsChance", "50"},
            {"lootBoxMinStorageTools", "1"},
            {"lootBoxMaxStorageTools", "2"},
            {"lootBoxStorageToolsChance", "50"},
            {"lootBoxTimeOfPromo", "0"},
            {"lootBoxIapDiscount", "0"},
            {"lootBoxName","AWESOME" }
        };

        public static void Get(Action<List<GiftReward>> onSuccess)
        {
            Debug.LogError("GET maternity_box_config was in DDNA");
            //DecisionPointCalss.RequestConfig(DecisionPoint.maternity_box_config,
            //    _handleRespons: (respons, parameters) =>
            //    {
            //        try
            //        {
            //            onSuccess?.Invoke(Parse(parameters));
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.LogError("Box config parse errors: " + ex.Message);
            //            onSuccess?.Invoke(GetDefaultRewards());
            //        }
            //    },
            //    _handleException: (e) =>
            //    {
            //        onSuccess?.Invoke(GetDefaultRewards());
            //    });
        }

        private static List<GiftReward> Parse(Dictionary<string, object> data)
        {
            LootBoxData resultData = LootBoxParser.Parse(data);
            if (resultData == null)
                return null;
            return LootBoxRandomizer.Get(resultData);
        }

        private static List<GiftReward> GetDefaultRewards()
        {
            try
            {
                LootBoxData newData = LootBoxParser.Parse(DefaultData);
                return LootBoxRandomizer.Get(newData);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }

    }
}
