using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace LootBox
    {
        public static class LootBoxRandomizer
        {
            
            public static List<GiftReward> Get(LootBoxData data)
            {
                List<GiftReward> rewards = new List<GiftReward>();

                // hearts
                int heartsAmount = RandomizeReward(data.minHearts, data.maxHearts, data.heartsChance);
                if (heartsAmount > 0)
                    rewards.Add(new GiftRewardDiamond(heartsAmount));

                // coins
                int coinsAmount = RandomizeReward(data.minCoins, data.maxCoins, data.coinsChance);
                if (coinsAmount > 0)
                    rewards.Add(new GiftRewardCoin(coinsAmount));

                // positive energy
                int positiveEnergyAmount = RandomizeReward(data.minPositiveEnergy, data.maxPositiveEnergy, data.positiveEnergyChance);
                if (positiveEnergyAmount > 0)
                    rewards.Add(new GiftRewardPositiveEnergy(positiveEnergyAmount));

                // shovels
                int shovelsAmount = RandomizeReward(data.minShovels, data.maxShovels, data.shovelsChance);
                if (shovelsAmount > 0)
                    rewards.Add(new GiftRewardShovel(shovelsAmount));

                // tank tools
                AddTools(new List<int>() { 4, 5, 6 }, data, rewards, data.minTankTools, data.maxTankTools, data.tankToolsChance);

                // storage tools
                AddTools(new List<int>() { 0, 1, 2 }, data, rewards, data.minStorageTools, data.maxStorageTools, data.storageToolsChance);

                // standard booster
                if(data.standardBooster != StandardBooster.NONE)
                {
                    if(data.standardBooster == StandardBooster.RANDOM)
                    {
                        List<StandardBooster> boosters = new List<StandardBooster>();
                        foreach (StandardBooster booster in Enum.GetValues(typeof(StandardBooster)))
                        {
                            if (booster == StandardBooster.NONE || booster == StandardBooster.RANDOM)
                                continue;
                            boosters.Add(booster);
                        }
                        int randomIndex = UnityEngine.Random.Range(0, boosters.Count);
                        AddBoosterReward(rewards, data.standardBoosterMinAmount, data.standardBoosterMaxAmount, data.standardBoosterChance, GetBoosterIndex(boosters[randomIndex].ToString()));
                    }
                    else
                    {
                        AddBoosterReward(rewards, data.standardBoosterMinAmount, data.standardBoosterMaxAmount, data.standardBoosterChance, GetBoosterIndex(data.standardBooster.ToString()));
                    }
                }

                // premium booster
                if (data.premiumBooster != PremiumBoosetr.NONE)
                {
                    if (data.premiumBooster == PremiumBoosetr.RANDOM)
                    {
                        List<PremiumBoosetr> boosters = new List<PremiumBoosetr>();
                        foreach (PremiumBoosetr booster in Enum.GetValues(typeof(PremiumBoosetr)))
                        {
                            if (booster == PremiumBoosetr.NONE || booster == PremiumBoosetr.RANDOM)
                                continue;
                            boosters.Add(booster);
                        }
                        int randomIndex = UnityEngine.Random.Range(0, boosters.Count);
                        AddBoosterReward(rewards, data.premiumBoosterMinAmount, data.premiumBoosterMaxAmount, data.premiumBoosterChance, GetBoosterIndex(boosters[randomIndex].ToString()));
                    }
                    else
                    {
                        AddBoosterReward(rewards, data.premiumBoosterMinAmount, data.premiumBoosterMaxAmount, data.premiumBoosterChance, GetBoosterIndex(data.premiumBooster.ToString()));
                    }
                }

                return rewards;
            }

            private static int GetBoosterIndex(string tag)
            {
                for(int i=0; i<ResourcesHolder.Get().boosterDatabase.boosters.Length; ++i)
                {
                    if(ResourcesHolder.Get().boosterDatabase.boosters[i].info == "BOOSTERS/"+tag+"_INFO")
                    {
                        return i;
                    }
                }
                return 0;
            }

            private static void AddBoosterReward(List<GiftReward> rewards, int minAmount, int maxAmount, int chance, int boosterID)
            {
                int amount = RandomizeReward(minAmount, maxAmount, chance);
                if(amount > 0)
                    rewards.Add(new GiftRewardBooster(amount, boosterID));
            }

            private static void AddTools(List<int> SpecialItemsIndexes, LootBoxData data, List<GiftReward> rewards, int minAmount, int maxAmount, int chance)
            {
                for(int i=0; i<SpecialItemsIndexes.Count; ++i)
                {
                    int toolsAmount = RandomizeReward(minAmount, maxAmount, chance);
                    if (toolsAmount > 0)
                    {
                        rewards.Add(new GiftRewardStorageUpgrader(toolsAmount, SpecialItemsIndexes[i]));
                    }
                }
            }

            private static int RandomizeReward(int minAmount, int maxAmount, int chance)
            {
                return UnityEngine.Random.Range(0, 101) <= chance ? UnityEngine.Random.Range(minAmount, maxAmount+1) : 0;
            }

        }
    }
}