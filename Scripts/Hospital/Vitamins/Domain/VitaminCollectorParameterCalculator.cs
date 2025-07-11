using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Hospital
{    
    public class VitaminCollectorParameterCalculator
    {
        private CapacityDataHolder capacityData;
        private FillRatioDataHolder fillRatioData;
        private UpgradeToolCostDataHolder upgradeToolsCostData;
        private UpgradePositiveEnergyDataHolder upgradePositiveData;
        private MedicineRef vitamin;
        private List<MedicineRef> listOfItemsUsedAsUpgradeCurrency;
        private const int ID_OF_SHOVEL = 3;
        private const int AMOUNT_OF_ITEMS_USED_TO_UPGRADE = 2;
        private List<MedicineRef> specialItemsRandomlySelectedForUpgrade;
        //private int positiveEnergyCostForUpgrade = -1;
        private const char ITEM_UPGRADABLE_SEPARATOR = '&';
        private const char ITEM_AMOUNT_SEPARATOR = '|';

        public VitaminCollectorParameterCalculator(MedicineRef vitamin)
        {
            this.vitamin = vitamin;
            capacityData = new CapacityDataHolder(DefaultConfigurationProvider.GetConfigCData().VitaminsDeltaCapacityUpgrade[vitamin.ToString()]);
            fillRatioData = new FillRatioDataHolder(DefaultConfigurationProvider.GetConfigCData().VitaminsDeltaFillRatioUpgrade[vitamin.ToString()]);
            upgradeToolsCostData = new UpgradeToolCostDataHolder(DefaultConfigurationProvider.GetConfigCData().ToolsUpgradeEquationParameters[vitamin.ToString()].ToArray());
            upgradePositiveData = new UpgradePositiveEnergyDataHolder(DefaultConfigurationProvider.GetConfigCData().PositiveEnergyUpgradeEquationParameters[vitamin.ToString()].ToArray());
            CreateListWithUpgradableItems();
            specialItemsRandomlySelectedForUpgrade = new List<MedicineRef>();
        }

        public int GetAmountOfToolAmountForLevel(int level)
        {
            return (int)(ExponentialFunction(level, upgradeToolsCostData.Aparameter, upgradeToolsCostData.Bparameter, upgradeToolsCostData.Cparameter));
        }

        public int GetAmountOfPositiveEnergyForLevel(int level)
        {
            return (int)(ExponentialFunction(level, upgradePositiveData.Aparameter, upgradePositiveData.Bparameter, upgradePositiveData.Cparameter));
        }

        public float GetNextLevelFillRatio(float fillRatio)
        {
            return fillRatio + fillRatioData.deltaIncrease;
        }

        public int GetNextLevelMaxCapacity(int maxCapacity)
        {
            return maxCapacity + capacityData.deltaIncrease;
        }

        public MedicineRef[] RandomizeSpecialItemsForUpgrade(int level)
        {
            if (specialItemsRandomlySelectedForUpgrade.Count == 0)
            {
                UnityEngine.Random.InitState(Animator.StringToHash(CognitoEntry.UserID + vitamin.ToString() + level));
                MedicineRef mostSpecialItemInStorage = null;
                int amountOfSpecialItemInStorage = 0;
                for (int i = 0; i < listOfItemsUsedAsUpgradeCurrency.Count; ++i)
                {
                    if (GameState.Get().GetCureCount(listOfItemsUsedAsUpgradeCurrency[i]) > amountOfSpecialItemInStorage)
                    {
                        mostSpecialItemInStorage = listOfItemsUsedAsUpgradeCurrency[i];
                        amountOfSpecialItemInStorage = GameState.Get().GetCureCount(listOfItemsUsedAsUpgradeCurrency[i]);
                    }
                }
                specialItemsRandomlySelectedForUpgrade.AddRange(WidthrawRandomItems(AMOUNT_OF_ITEMS_USED_TO_UPGRADE));
                if (mostSpecialItemInStorage != null)
                {
                    for (int i = 0; i < specialItemsRandomlySelectedForUpgrade.Count; ++i)
                    {
                        if (specialItemsRandomlySelectedForUpgrade[i].id == mostSpecialItemInStorage.id)
                        {
                            return specialItemsRandomlySelectedForUpgrade.ToArray();
                        }
                    }
                    specialItemsRandomlySelectedForUpgrade[0] = mostSpecialItemInStorage;
                }
            }
            return specialItemsRandomlySelectedForUpgrade.ToArray();
        }

        public void ClearRandomizedItemsForUpgrade()
        {
            Debug.Log("VitaminCollectorCalculator.ClearRandomizeditemsForUpgrade");
            specialItemsRandomlySelectedForUpgrade.Clear();
        }

        public string SaveToString()
        {
            StringBuilder sb = new StringBuilder();
            if (specialItemsRandomlySelectedForUpgrade.Count > 0)
            {
                for (int i = 0; i < specialItemsRandomlySelectedForUpgrade.Count; ++i)
                {
                    sb.Append(ITEM_UPGRADABLE_SEPARATOR);
                    sb.Append(specialItemsRandomlySelectedForUpgrade[i]);
                }
            }

            return sb.ToString();
        }

        public void LoadFromString(string data)
        {
            if (!String.IsNullOrEmpty(data))
            {
                string[] unparsedItemUpgradableData = data.Split(new char[] { ITEM_UPGRADABLE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);                

                for(int i = 0; i < unparsedItemUpgradableData.Length; ++i)
                {
                    Debug.Log(unparsedItemUpgradableData[i]);
                    specialItemsRandomlySelectedForUpgrade.Add(MedicineRef.Parse(unparsedItemUpgradableData[i]));
                }
            }
        }

        private void CreateListWithUpgradableItems()
        {
            List<MedicineDatabaseEntry> medicineTypeSpecialEntries = ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Special);
            MedicineRef current;

            listOfItemsUsedAsUpgradeCurrency = new List<MedicineRef>();

            for (int i = 0; i < medicineTypeSpecialEntries.Count; ++i)
            {
                current = medicineTypeSpecialEntries[i].GetMedicineRef();

                if (!IsIDOfShovel(current.id))
                    listOfItemsUsedAsUpgradeCurrency.Add(current);
            }
        }

        private List<MedicineRef> WidthrawRandomItems(int howManyRandomItemsToWidtraw)
        {
            List<MedicineRef> listToReturn = new List<MedicineRef>();
            List<MedicineRef> listUsedForRandom = new List<MedicineRef>(listOfItemsUsedAsUpgradeCurrency);
            for (int i = 0; i < howManyRandomItemsToWidtraw; ++i)
            {
                int randomIndex = UnityEngine.Random.Range(0, listUsedForRandom.Count);
                listToReturn.Add(listUsedForRandom[randomIndex]);
                listUsedForRandom.RemoveAt(randomIndex);
            }
            return listToReturn;
        }

        private float ExponentialFunction(float x, float A, float B, float C)
        {
            return A * Mathf.Exp(B * x) + C;
            //A * Mathf.Exp(B) + C;                
        }

        private bool IsIDOfShovel(int id)
        {
            return id == ID_OF_SHOVEL;
        }

        private class CapacityDataHolder
        {
            public int deltaIncrease;
            public CapacityDataHolder(int deltaIncrease)
            {
                this.deltaIncrease = deltaIncrease;
            }
        }

        private class FillRatioDataHolder
        {
            public int deltaIncrease;
            public FillRatioDataHolder(int deltaIncrease)
            {
                this.deltaIncrease = deltaIncrease;
            }
        }

        private class UpgradeToolCostDataHolder
        {
            public float Aparameter;
            public float Bparameter;
            public float Cparameter;
            public UpgradeToolCostDataHolder(params float[] parameters)
            {
                Aparameter = parameters[0];
                Bparameter = parameters[1];
                Cparameter = parameters[2];
            }
        }

        private class UpgradePositiveEnergyDataHolder
        {
            public float Aparameter;
            public float Bparameter;
            public float Cparameter;
            public UpgradePositiveEnergyDataHolder(params float[] parameters)
            {
                Aparameter = parameters[0];
                Bparameter = parameters[1];
                Cparameter = parameters[2];
            }
        }
    }
}
