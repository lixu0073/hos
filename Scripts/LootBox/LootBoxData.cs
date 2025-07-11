using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace LootBox
    {

        public enum Box
        {
            blue,
            xmas,
            pink
        }

        public enum StandardBooster
        {
            NONE,
            RANDOM,
            DOCTOR_COINS,
            HOSPITAL_COINS,
            BED_COINS,
            HAPPYMINUTE,
            DOCTOR_EXP,
            HOSPITAL_EXP,
            BED_EXP
        }

        public enum PremiumBoosetr
        {
            NONE,
            RANDOM,
            HOSPITAL_COINS_AND_EXP,
            HAPPYHOUR
        }

        public class LootBoxData
        {
            public string iap;
            public Box box = Box.xmas;
            public StandardBooster standardBooster = StandardBooster.NONE;
            public int standardBoosterChance = 0;
            public int standardBoosterMinAmount = 0;
            public int standardBoosterMaxAmount = 0;
            public PremiumBoosetr premiumBooster = PremiumBoosetr.NONE;
            public int premiumBoosterChance = 0;
            public int premiumBoosterMinAmount = 0;
            public int premiumBoosterMaxAmount = 0;
            public int minHearts = 0;
            public int maxHearts = 0;
            public int heartsChance = 0;
            public int minCoins = 0;
            public int maxCoins = 0;
            public int coinsChance = 0;
            public int minShovels = 0;
            public int maxShovels = 0;
            public int shovelsChance = 0;
            public int minPositiveEnergy = 0;
            public int maxPositiveEnergy = 0;
            public int positiveEnergyChance = 0;
            public int minTankTools = 0;
            public int maxTankTools = 0;
            public int tankToolsChance = 0;
            public int minStorageTools = 0;
            public int maxStorageTools = 0;
            public int storageToolsChance = 0;
            public int timeOfPromo = 0;
            public int iapDiscount = 0;
            public string tag;
            public bool visibleOnMainUI = false;
            public bool visibleInIapShop = false;
            public string IAPShopImageDecisionPoint;
        }
    }
}
