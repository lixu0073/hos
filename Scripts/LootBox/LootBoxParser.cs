using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace LootBox
    {
        public static class LootBoxParser
        {
            private const string iapProduct = "iapProduct";
            private const string box = "lootBoxType";
            private const string standard_booster = "lootBoxStandardBooster";
            private const string standard_booster_chance = "lootBoxStandardBoosterChance";
            private const string premium_booster = "lootBoxPremiumBooster";
            private const string premium_booster_chance = "lootBoxPremiumBoosterChance";
            private const string min_hearts = "hardCurrency";
            private const string max_hearts = "hardMaxCurrency";
            private const string hearts_chance = "lootBoxHardCurrencyChance";
            private const string min_coins = "softCurrency";
            private const string max_coins = "softMaxCurrency";
            private const string coins_chance = "lootBoxSoftCurrencyChance";
            private const string min_shovels = "lootBoxMinShovels";
            private const string max_shovels = "lootBoxMaxShovels";
            private const string shovels_chance = "lootBoxShovelsChance";
            private const string min_positive_energy = "lootBoxMinPositiveEnergy";
            private const string max_positive_energy = "lootBoxMaxPositiveEnergy";
            private const string positive_energy_chance = "lootBoxPositiveEnergyChance";
            private const string min_tank_tools = "lootBoxMinTankTools";
            private const string max_tank_tools = "lootBoxMaxTankTools";
            private const string tank_tools_chance = "lootBoxTankToolsChance";
            private const string min_storage_tools = "lootBoxMinStorageTools";
            private const string max_storage_tools = "lootBoxMaxStorageTools";
            private const string storage_tools_chance = "lootBoxStorageToolsChance";
            private const string time_of_promo = "lootBoxTimeOfPromo";
            private const string iap_discount = "lootBoxIapDiscount";
            private const string min_standard_booster_amount = "lootBoxStandardBoosterMinAmount";
            private const string max_standard_booster_amount = "lootBoxStandardBoosterMaxAmount";
            private const string min_premium_booster_amount = "lootBoxPremiumBoosterMinAmount";
            private const string max_premium_booster_amount = "lootBoxPremiumBoosterMaxAmount";
            private const string box_name = "lootBoxName";
            private const string visibleOnMainUI = "visibleMainUI";
            private const string visibleInIapShop = "visibleIapShop";

            private const string INPUT_DATA_NULL_MESSAGE = "input_data_null";
            private const string IAP_PRODUCT_REQUIRED = "iap_product_required";

            public static LootBoxData Parse(Dictionary<string, object> data)
            {
                LootBoxData lootBoxData = new LootBoxData();
                if (data == null)
                {
                    ThrowException(INPUT_DATA_NULL_MESSAGE);
                }
                // validate iap Product
                if (!data.ContainsKey(iapProduct) || data[iapProduct] == null || string.IsNullOrEmpty(data[iapProduct].ToString()))
                {
                    ThrowException(IAP_PRODUCT_REQUIRED);
                }
                lootBoxData.iap = data[iapProduct].ToString();

                lootBoxData.box = ValidateEnum(data[box], Box.xmas);
                lootBoxData.standardBooster = ValidateEnum(data[standard_booster], StandardBooster.NONE);
                lootBoxData.premiumBooster = ValidateEnum(data[premium_booster], PremiumBoosetr.NONE);

                lootBoxData.standardBoosterMinAmount = ValidatePositiveInt(min_standard_booster_amount, data);
                lootBoxData.standardBoosterMaxAmount = ValidatePositiveInt(max_standard_booster_amount, data);
                lootBoxData.standardBoosterMinAmount = Math.Min(lootBoxData.standardBoosterMinAmount, lootBoxData.standardBoosterMaxAmount);

                lootBoxData.premiumBoosterMinAmount = ValidatePositiveInt(min_premium_booster_amount, data);
                lootBoxData.premiumBoosterMaxAmount = ValidatePositiveInt(max_premium_booster_amount, data);
                lootBoxData.premiumBoosterMinAmount = Math.Min(lootBoxData.premiumBoosterMinAmount, lootBoxData.premiumBoosterMaxAmount);

                lootBoxData.standardBoosterChance = ValidateInt_0_100(standard_booster_chance, data);
                lootBoxData.premiumBoosterChance = ValidateInt_0_100(premium_booster_chance, data);

                lootBoxData.minHearts = ValidatePositiveInt(min_hearts, data);
                lootBoxData.maxHearts = ValidatePositiveInt(max_hearts, data);
                lootBoxData.minHearts = Math.Min(lootBoxData.minHearts, lootBoxData.maxHearts);
                lootBoxData.heartsChance = ValidateInt_0_100(hearts_chance, data);

                lootBoxData.minCoins = ValidatePositiveInt(min_coins, data);
                lootBoxData.maxCoins = ValidatePositiveInt(max_coins, data);
                lootBoxData.minCoins = Math.Min(lootBoxData.minCoins, lootBoxData.maxCoins);
                lootBoxData.coinsChance = ValidateInt_0_100(coins_chance, data);

                lootBoxData.minShovels = ValidatePositiveInt(min_shovels, data);
                lootBoxData.maxShovels = ValidatePositiveInt(max_shovels, data);
                lootBoxData.minShovels = Math.Min(lootBoxData.minShovels, lootBoxData.maxShovels);
                lootBoxData.shovelsChance = ValidateInt_0_100(shovels_chance, data);

                lootBoxData.minPositiveEnergy = ValidatePositiveInt(min_positive_energy, data);
                lootBoxData.maxPositiveEnergy = ValidatePositiveInt(max_positive_energy, data);
                lootBoxData.minPositiveEnergy = Math.Min(lootBoxData.minPositiveEnergy, lootBoxData.maxPositiveEnergy);
                lootBoxData.positiveEnergyChance = ValidateInt_0_100(positive_energy_chance, data);

                lootBoxData.minTankTools = ValidatePositiveInt(min_tank_tools, data);
                lootBoxData.maxTankTools = ValidatePositiveInt(max_tank_tools, data);
                lootBoxData.minTankTools = Math.Min(lootBoxData.minTankTools, lootBoxData.maxTankTools);
                lootBoxData.tankToolsChance = ValidateInt_0_100(tank_tools_chance, data);

                lootBoxData.minStorageTools = ValidatePositiveInt(min_storage_tools, data);
                lootBoxData.maxStorageTools = ValidatePositiveInt(max_storage_tools, data);
                lootBoxData.minStorageTools = Math.Min(lootBoxData.minStorageTools, lootBoxData.maxStorageTools);
                lootBoxData.storageToolsChance = ValidateInt_0_100(storage_tools_chance, data);

                lootBoxData.timeOfPromo = ValidatePositiveInt(time_of_promo, data);
                lootBoxData.iapDiscount = ValidateInt_0_100(iap_discount, data);
                lootBoxData.tag = ValidateString(box_name, data);
                lootBoxData.visibleOnMainUI = ValidateBool(visibleOnMainUI, data);
                lootBoxData.visibleInIapShop = ValidateBool(visibleInIapShop, data);
                return lootBoxData;
            }

            private static bool ValidateBool(string key, Dictionary<string, object> data)
            {
                if (!data.ContainsKey(key))
                {
                    return false;
                }
                int value = 0;
                int.TryParse(data[key].ToString(), out value);
                return value == 0 ? false : true;
            }

            private static T ValidateEnum<T>(object obj, T _default)
            {
                T value = _default;
                if (obj == null)
                    return value;
                try
                {
                    value = (T)Enum.Parse(typeof(T), obj.ToString());
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                return value;
            }

            private static int ValidateInt_0_100(string key, Dictionary<string, object> data, int _default = 0)
            {
                int val = ValidatePositiveInt(key, data, _default);
                if (val > 100)
                    val = 100;
                return val;
            }

            private static string ValidateString(string key, Dictionary<string, object> data, string _default = null)
            {
                if (!data.ContainsKey(key))
                    return _default;
                return data[key].ToString();
            }

            private static int ValidatePositiveInt(string key, Dictionary<string, object> data, int _default = 0)
            {
                if (!data.ContainsKey(key))
                    return _default;
                int value = _default;
                int.TryParse(data[key].ToString(), out value);
                if (value < 0)
                    value = 0;
                return value;
            }

            private static void ThrowException(string message)
            {
                throw new System.Exception(message);
            }

        }
    }
}