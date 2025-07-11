using Hospital;
using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity
{
    public static class MaternityCoreLoopParametersHolder
    {
        private static BalanceableInt maternityMaxTimeToNextMotherSpawnBalanceable;
        private static int MaternityMaxTimeToNextMotherSpawn
        {
            get
            {
                if(maternityMaxTimeToNextMotherSpawnBalanceable == null)
                {
                    maternityMaxTimeToNextMotherSpawnBalanceable = BalanceableFactory.CreateMaternityTimeToSpawnNextMotherMaxBalanceable();
                }
                return maternityMaxTimeToNextMotherSpawnBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt maternityMinTimeToNextMotherSpawnBalanceable;
        private static int MaternityMinTimeToNextMotherSpawn
        {
            get
            {
                if (maternityMinTimeToNextMotherSpawnBalanceable == null)
                {
                    maternityMinTimeToNextMotherSpawnBalanceable = BalanceableFactory.CreateMaternityTimeToSpawnNextMotherMinBalanceable();
                }
                return maternityMinTimeToNextMotherSpawnBalanceable.GetBalancedValue();
            }
        }

        public static int GetNextMotherSpawnDuration()
        {
            //return Random.Range(BundleManager.GetMaternityMinTimeToNextMotherSpawn(), BundleManager.GetMaternityMaxTimeToNextMotherSpawn() + 1);
            return Random.Range(MaternityMinTimeToNextMotherSpawn, MaternityMaxTimeToNextMotherSpawn + 1);
        }

        public static Vector2i GetMotherSpawnPosition()
        {
            return new Vector2i(53, 69);
        }


        private static BalanceableInt maternityBloodTestCoinsCost;
        public static int GetBloodTestCostInCoins()
        {
            //return BundleManager.GetMaternityBloodTestCoinsCost();
            if(maternityBloodTestCoinsCost == null)
            {
                maternityBloodTestCoinsCost = BalanceableFactory.CreateMaternityBloodTestCoinsCostBalanceable();
            }

            return maternityBloodTestCoinsCost.GetBalancedValue();
        }

        private static BalanceableInt maternityBloodTestDuration;
        public static int BloodTestTime
        {
            get
            {//return BundleManager.GetMaternityBloodTestTime();
                if (maternityBloodTestDuration == null)
                {
                    maternityBloodTestDuration = BalanceableFactory.CreateMaternityBloodTestDurationBalanceable();
                }

                return maternityBloodTestDuration.GetBalancedValue();
            }
        }

        private static BalanceableFloat maternityExpToRealtimeSecondBalanceable;        
        private static float MaternityExpToRealtimeSecond
        {
            get
            {
                if(maternityExpToRealtimeSecondBalanceable == null)
                {
                    maternityExpToRealtimeSecondBalanceable = BalanceableFactory.CreateMaternityExpToRealtimeSecondsBalanceable();
                }

                return maternityExpToRealtimeSecondBalanceable.GetBalancedValue();
            }
        }

        public static int GetBloodTestExpReward()
        {
            //return Mathf.CeilToInt(BundleManager.GetMaternityBloodTestTime() / MaternityExpToRealtimeSecond);
            return Mathf.CeilToInt(BloodTestTime / MaternityExpToRealtimeSecond);
        }

        private static BalanceableInt maternityMinBondingBalanceable;
        private static int MaternityMinBonding
        {
            get
            {
                if (maternityMinBondingBalanceable == null)
                {
                    maternityMinBondingBalanceable = BalanceableFactory.CreateMaternityLaborStageMinDurationEasyBalanceable();
                }
                return maternityMinBondingBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt maternityMaxBondingBalanceable;
        private static int MaternityMaxBonding
        {
            get
            {
                if (maternityMaxBondingBalanceable == null)
                {
                    maternityMaxBondingBalanceable = BalanceableFactory.CreateMaternityLaborStageMaxDurationHardBalanceable();
                }
                return maternityMaxBondingBalanceable.GetBalancedValue();
            }
        }

        public static int GetBondingTime()
        {
            return Random.Range(MaternityMinBonding, MaternityMaxBonding + 1);
        }

        private static BalanceableFloat laborHardStageChanceBalanceable;
        private static float LaborHardStageChance
        {
            get
            {
                if(laborHardStageChanceBalanceable == null) { laborHardStageChanceBalanceable = BalanceableFactory.CreateMaternityHardLaborStageChanceBalanceable(); }
                return laborHardStageChanceBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt levelIncreaseForDifficultyBalanceable;
        private static int LevelIncreaseForDifficulty
        {
            get
            {
                if(levelIncreaseForDifficultyBalanceable == null)
                {
                    levelIncreaseForDifficultyBalanceable = BalanceableFactory.CreateMaternityLevelStageDifficultyChangeBalanceable();
                }
                return levelIncreaseForDifficultyBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt maternityMinLaborEasyBalanceable;
        private static int MaternityMinLaborEasy
        {
            get
            {
                if (maternityMinLaborEasyBalanceable == null)
                {
                    maternityMinLaborEasyBalanceable = BalanceableFactory.CreateMaternityLaborStageMinDurationEasyBalanceable();
                }
                return maternityMinLaborEasyBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt maternityMinLaborHardBalanceable;
        private static int MaternityMinLaborHard
        {
            get
            {
                if (maternityMinLaborHardBalanceable == null)
                {
                    maternityMinLaborHardBalanceable = BalanceableFactory.CreateMaternityLaborStageMinDurationHardBalanceable();
                }
                return maternityMinLaborHardBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt maternityMaxLaborEasyBalanceable;
        private static int MaternityMaxLaborEasy
        {
            get
            {
                if (maternityMaxLaborEasyBalanceable == null)
                {
                    maternityMaxLaborEasyBalanceable = BalanceableFactory.CreateMaternityLaborStageMaxDurationEasyBalanceable();
                }
                return maternityMaxLaborEasyBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableInt maternityMaxLaborHardBalanceable;
        private static int MaternityMaxLaborHard
        {
            get
            {
                if (maternityMaxLaborHardBalanceable == null)
                {
                    maternityMaxLaborHardBalanceable = BalanceableFactory.CreateMaternityLaborStageMaxDurationHardBalanceable();
                }
                return maternityMaxLaborHardBalanceable.GetBalancedValue();
            }
        }

        public static int GetLaborStageTime()
        {
            float chanceForHardStage = LaborHardStageChance * (int)(Game.Instance.gameState().GetMaternityLevel() / LevelIncreaseForDifficulty);

            if (Random.value < LaborHardStageChance)
            {
                return Random.Range(MaternityMinLaborHard, MaternityMaxLaborHard + 1);
            }
            else
            {
                return Random.Range(MaternityMinLaborEasy,  MaternityMaxLaborEasy + 1);
            }
        }

        private static BalanceableFloat hospitalExpToMaternityExpBalanceable;
        private static float HospitalExpToMaternityExp
        {
            get
            {
                if (hospitalExpToMaternityExpBalanceable == null)
                {
                    hospitalExpToMaternityExpBalanceable = BalanceableFactory.CreateHospitalExpToMaternityExpConversionBalanceable();
                }
                return hospitalExpToMaternityExpBalanceable.GetBalancedValue();
            }
        }

        public static int GetMaternityExpForVitamin(MedicineDatabaseEntry vitamin, int amount)
        {
            if (vitamin.GetMedicineRef().type == MedicineType.Vitamins && amount > 0)
            {
                return Mathf.CeilToInt((vitamin.maxPrice / 2) * HospitalExpToMaternityExp) * amount;
            }
            return 0;
        }

        public static int GetMaternityExpForLaborStage(int LaborTime)
        {
            return Mathf.CeilToInt(LaborTime / MaternityExpToRealtimeSecond);  //BundleManager.GetOneMaternityExpCostInSec();
        }

        public static int GetMaternityExpForBondingStage(int BondingTime)
        {
            return Mathf.CeilToInt(BondingTime / MaternityExpToRealtimeSecond); //BundleManager.GetOneMaternityExpCostInSec();
        }

        private static Dictionary<MaternityCharacterInfo.Stage, int> firstLoopExp = new Dictionary<MaternityCharacterInfo.Stage, int>()
        {
            { MaternityCharacterInfo.Stage.HealingAndBounding, 96 },
            { MaternityCharacterInfo.Stage.Vitamines, 8 },
            { MaternityCharacterInfo.Stage.InLabor, 24 },
            { MaternityCharacterInfo.Stage.WaitingForLabor, 24 }
        };

        public static int GetExpInFirstLoopForStage(MaternityCharacterInfo.Stage stage, float factor = 0)
        {
            switch(stage)
            {
                case MaternityCharacterInfo.Stage.HealingAndBounding:
                case MaternityCharacterInfo.Stage.Vitamines:
                    return firstLoopExp[stage];
                case MaternityCharacterInfo.Stage.InLabor:
                    return Mathf.FloorToInt(firstLoopExp[stage] * factor);
                case MaternityCharacterInfo.Stage.WaitingForLabor:
                    return Mathf.CeilToInt(firstLoopExp[stage] * factor);
                default:
                    return 0;
            }
        }

        private static BalanceableInt maternityVitaminRandomizationLevelThresholdBalanceable;
        private static int MaternityVitaminRandomizationLevelThreshold
        {
            get
            {
                if (maternityVitaminRandomizationLevelThresholdBalanceable == null)
                {
                    maternityVitaminRandomizationLevelThresholdBalanceable = BalanceableFactory.CreateMaternityVitaminRandomizationLevelThresholdBalanceable();
                }

                return maternityVitaminRandomizationLevelThresholdBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableFloat minExponentialParameterABalanceable;
        private static float MinExponentialParameterA
        {
            get
            {
                if(minExponentialParameterABalanceable == null)
                {
                    minExponentialParameterABalanceable = BalanceableFactory.CreateExponentialParametreAMinBalanceable();
                }
                return minExponentialParameterABalanceable.GetBalancedValue();
            }
        }

        private static BalanceableFloat maxExponentialParameterABalanceable;
        private static float MaxExponentialParameterA
        {
            get
            {
                if (maxExponentialParameterABalanceable == null)
                {
                    maxExponentialParameterABalanceable = BalanceableFactory.CreateExponentialParametreAMaxBalanceable();
                }
                return maxExponentialParameterABalanceable.GetBalancedValue();
            }
        }

        private static BalanceableFloat minExponentialParametreBBalanceable;
        private static float MinExponentialParametreB
        {
            get
            {
                if (minExponentialParametreBBalanceable == null)
                {
                    minExponentialParametreBBalanceable = BalanceableFactory.CreateExponentialParametreBMinBalanceable();
                }
                return minExponentialParametreBBalanceable.GetBalancedValue();
            }
        }

        private static BalanceableFloat maxExponentialParametreBBalanceable;
        private static float MaxExponentialParametreB
        {
            get
            {
                if (maxExponentialParametreBBalanceable == null)
                {
                    maxExponentialParametreBBalanceable = BalanceableFactory.CreateExponentialParametreBMaxBalanceable();
                }
                return maxExponentialParametreBBalanceable.GetBalancedValue();
            }
        }

        public static void RandomizeVitaminsForMaternityCharacterInfo(MaternityCharacterInfo mother)
        {
            float minAParamnetr = MinExponentialParameterA; //BundleManager.GetMinExponentialParameterA();
            float maxAParameter = MaxExponentialParameterA; //BundleManager.GetMaxExponentialParameterA();
            float minBParameter = MinExponentialParametreB; //BundleManager.GetMinExponentialParameterB();
            float maxBParameter = MaxExponentialParametreB; //BundleManager.GetMaxExponentialParameterB();

            int playerLevel = Game.Instance.gameState().GetMaternityLevel();
            if (!DefaultConfigurationProvider.GetConfigCData().EndlessVitaminRandomization)
            {
                playerLevel = Mathf.Clamp(playerLevel, 1, MaternityVitaminRandomizationLevelThreshold);
            }

            int MinTimeRandomRange = Mathf.CeilToInt(minAParamnetr * Mathf.Exp(playerLevel * minBParameter));
            int MaxTimeRandomRange = Mathf.CeilToInt(maxAParameter * Mathf.Exp(playerLevel * maxBParameter));

            List<MedicineDatabaseEntry> vitamines = ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Vitamins);

            foreach (MedicineDatabaseEntry vitamine in vitamines)
            {
                if (Game.Instance.gameState().GetHospitalLevel() >= vitamine.minimumLevel)
                {
                    float time = Random.Range(MinTimeRandomRange, MaxTimeRandomRange + 1);                    
                    float vitaminProductionTime = DefaultConfigurationProvider.GetConfigCData().VitaminsProductionTime[vitamine.GetMedicineRef().ToString()];
                    if (time > 0 && vitaminProductionTime > 0)
                    {
                        int amount = Mathf.FloorToInt(time / vitaminProductionTime);
                        if (amount > 0)
                        {
                            mother.RequiredCures.Add(vitamine, amount);
                        }
                    }
                }
            }
        }
    }
}
