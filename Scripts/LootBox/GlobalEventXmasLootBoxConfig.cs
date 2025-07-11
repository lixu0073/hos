using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace LootBox
    {
        public static class GlobalEventXmasLootBoxConfig
        {
            public readonly static Dictionary<string, object> Data = new Dictionary<string, object>()
            {
                {"iapProduct", "none"},
                {"lootBoxType", "xmas"},
                {"lootBoxStandardBooster", "RANDOM"},
                {"lootBoxStandardBoosterChance", "100"},
                {"lootBoxStandardBoosterMinAmount", "1"},
                {"lootBoxStandardBoosterMaxAmount", "1"},
                {"lootBoxPremiumBooster", "NONE"},
                {"lootBoxPremiumBoosterChance", "0"},
                {"lootBoxPremiumBoosterMinAmount", "0"},
                {"lootBoxPremiumBoosterMaxAmount", "0"},
                {"hardCurrency", "10"},
                {"hardMaxCurrency", "10"},
                {"lootBoxHardCurrencyChance", "100"},
                {"softCurrency", "1000"},
                {"softMaxCurrency", "1000"},
                {"lootBoxSoftCurrencyChance", "100"},
                {"lootBoxMinShovels", "0"},
                {"lootBoxMaxShovels", "0"},
                {"lootBoxShovelsChance", "0"},
                {"lootBoxMinPositiveEnergy", "0"},
                {"lootBoxMaxPositiveEnergy", "0"},
                {"lootBoxPositiveEnergyChance", "0"},
                {"lootBoxMinTankTools", "4"},
                {"lootBoxMaxTankTools", "4"},
                {"lootBoxTankToolsChance", "100"},
                {"lootBoxMinStorageTools", "4"},
                {"lootBoxMaxStorageTools", "4"},
                {"lootBoxStorageToolsChance", "100"},
                {"lootBoxTimeOfPromo", "0"},
                {"lootBoxIapDiscount", "0"},
                {"lootBoxName","SUPER_AWESOME" }
            };
        }
    }
}