using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Hospital
{
    public class VitaminCollectorModel
    {
        private const int HOUR_IN_SECONDS = 3600;

        public delegate void CapacityChanged(float fill, float current, int max, int producedAmount, MedicineRef vitamin, int timeToDrop, VitaminSource source);
        public event CapacityChanged capacityChanged;
        public delegate void VitaminCollected(int amount, MedicineRef vitamin);
        public event VitaminCollected vitaminCollected;

        public MedicineRef med { get; private set; }
        public int maxCapacity { get; private set; }
        public float FillRatio { get { return fillRatio * (2 - FillRatioMultiplier); } private set { } }
        private float fillRatio;
        public float capacity { get; private set; }
        public int collectorLevel { get; private set; }
        public bool hasEverBeenEmpty = false;
        //private float fillRatioMultiplier = 1;
        private int nextLevel { get { return collectorLevel + 1; } }

        public float GetNormalizedFill()
        {
            return capacity / maxCapacity;
        }

        private VitaminCollectorParameterCalculator parameterCalculator;

        public VitaminCollectorModel() { }

        public VitaminCollectorModel(MedicineRef med)
        {
            parameterCalculator = new VitaminCollectorParameterCalculator(med);
            this.med = med;
            collectorLevel = 1;
            SetStartingMaxCapacity();
            capacity = maxCapacity;
            SetStartingFillRato();
        }

        private void SetStartingMaxCapacity()
        {
            
            maxCapacity = DefaultConfigurationProvider.GetConfigCData().VitaminsCollectorCapacity[med.ToString()];
        }

        public void SetStartingFillRato()
        {
            fillRatio = (float)3600 / DefaultConfigurationProvider.GetConfigCData().VitaminsProductionTime[med.ToString()];
        }

        public int GetNextLevelMaxCapacity()
        {
            return parameterCalculator.GetNextLevelMaxCapacity(maxCapacity);
        }

        public float GetNextLevelFillRatio()
        {
            return parameterCalculator.GetNextLevelFillRatio(fillRatio) * (2 - FillRatioMultiplier);
        }

        public int GetAmountOfSpecialItemForUpgrade()
        {
            return parameterCalculator.GetAmountOfToolAmountForLevel(nextLevel);
        }

        public int GetAmountOfPositiveEnergyForUpgrade()
        {
            return parameterCalculator.GetAmountOfPositiveEnergyForLevel(nextLevel);
        }

        public MedicineRef[] GetMedicinesForUpgrade()
        {
            return parameterCalculator.RandomizeSpecialItemsForUpgrade(nextLevel);
        }

        private BalanceableFloat fillRatioMultiplierBalanceable;
        private MasterableProductionMachineConfigData configData;

        private float FillRatioMultiplier
        {
            get
            {
                if(fillRatioMultiplierBalanceable == null)
                    Debug.LogError("Balanceable Not Set!");

                return fillRatioMultiplierBalanceable.GetBalancedValue();
            }
        }

        public void SetFillRatioMultiplierBalanceable(MasterableProperties masterableObject)
        {
            fillRatioMultiplierBalanceable = BalanceableFactory.CreateVMFillRatioMultiplier(masterableObject);
        }

        public void SetFillRatioMultiplierBalanceable(MasterableProductionMachineConfigData configData)
        {
            fillRatioMultiplierBalanceable = BalanceableFactory.CreateVMFillRatioMultiplier(configData);
        }

        /*
        public void UpgradeFillRatioDueToMastery(float productionTimeMultiplier)
        {
            fillRatioMultiplier = productionTimeMultiplier;
        }*/

        public bool IsRequiredToHealPatientInTreatmentRoom()
        {
            return MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(med) > 0;
        }

        public void Update(float timePassed)
        {
            float deltaTime = timePassed;
            if (Game.Instance.gameState().GetHospitalLevel() < GetVitaminCollectorUnlockLevel())
                return;
            int oldCapacity = (int)capacity;
            if (capacity >= maxCapacity)
                return;
            capacity = Mathf.Clamp(capacity + deltaTime * FillRatio / HOUR_IN_SECONDS, 0.0f, maxCapacity);
            int newCapacity = (int)capacity;
            OnCapacityChanged(VitaminSource.Default, Math.Max(0, newCapacity - oldCapacity));
        }

        public void Update(TimePassedObject timePassed)
        {
            float deltaTime = timePassed.GetSmallestTimePassedFromAllTimes();
            if (Game.Instance.gameState().GetHospitalLevel() < GetVitaminCollectorUnlockLevel())
                return;
            int oldCapacity = (int)capacity;
            if (capacity >= maxCapacity)
                return;
            capacity = Mathf.Clamp(capacity + deltaTime * FillRatio / HOUR_IN_SECONDS, 0.0f, maxCapacity);
            int newCapacity = (int)capacity;
            OnCapacityChanged(VitaminSource.Default, Math.Max(0, newCapacity - oldCapacity));
        }

        public void FillToMaxCapacity()
        {
            int diff = maxCapacity - (int)capacity;
            if (diff > 0)
            {
                capacity = maxCapacity;
                OnCapacityChanged(VitaminSource.FullRefill, diff);
            }
        }

        private void OnCapacityChanged(VitaminSource source = VitaminSource.Default, int producedVitaminAmount = 0)
        {
            capacityChanged?.Invoke(capacity / maxCapacity, capacity, maxCapacity, Math.Max(0, producedVitaminAmount), med, GetSecondsToNextDrop(), source);
        }

        public int GetSecondsToNextDrop()
        {
            if (capacity >= maxCapacity)
                return -1;
            return (int)((HOUR_IN_SECONDS / FillRatio) * (1 - (capacity - (int)capacity)));
        }

        public int GetSecondsToFullfillCollector()
        {
            if (Game.Instance.gameState().GetHospitalLevel() < GetVitaminCollectorUnlockLevel())
                return -1;
            if (capacity >= maxCapacity)
                return -1;
            return (int)((HOUR_IN_SECONDS / FillRatio) * (maxCapacity - capacity));
        }

        public int GetCureCountInStorage()
        {
            return Game.Instance.gameState().GetCureCount(med);
        }

        private void OnVitaminCollected(int amount)
        {
            vitaminCollected?.Invoke(amount, med);
        }

        public void LoadFromString(string str, TimePassedObject timePassed, MasterableProperties masterableObject)
        {
            string[] array = str.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length > 0)
            {
                med = MedicineRef.Parse(array[0]);
                parameterCalculator = new VitaminCollectorParameterCalculator(med);
            }
            capacity = array.Length > 1 ? float.Parse(array[1], CultureInfo.InvariantCulture) : 0;
            collectorLevel = array.Length > 2 ? int.Parse(array[2], System.Globalization.CultureInfo.InvariantCulture) : 1;
            fillRatio = array.Length > 3 ? float.Parse(array[3], CultureInfo.InvariantCulture) : GetFillRatioBasedOnLevel(); //BundleManager.GetStartingVitaminFillRatio(med);
            maxCapacity = array.Length > 4 ? int.Parse(array[4], System.Globalization.CultureInfo.InvariantCulture) : GetMaxCapacityBasedOnLevel(); //BundleManager.GetStartingCollectorMaxCapacity(med);
            capacity = Mathf.Clamp(capacity, 0.0f, maxCapacity); //clamping for a very rare bugs with negative values or values over maxCapacity
            //fillRatioMultiplier = productionTimeMultiplier;
            SetFillRatioMultiplierBalanceable(masterableObject);

            if (array.Length > 5)
                parameterCalculator.LoadFromString(array[5]);

            Update(timePassed);
        }

        public void LoadFromString(string str, TimePassedObject timePassed, MasterableProductionMachineConfigData config)
        {
            string[] array = str.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length > 0)
            {
                med = MedicineRef.Parse(array[0]);
                parameterCalculator = new VitaminCollectorParameterCalculator(med);
            }
            capacity = array.Length > 1 ? float.Parse(array[1], System.Globalization.CultureInfo.InvariantCulture) : 0;
            collectorLevel = array.Length > 2 ? int.Parse(array[2], System.Globalization.CultureInfo.InvariantCulture) : 1;
            fillRatio = array.Length > 3 ? float.Parse(array[3], System.Globalization.CultureInfo.InvariantCulture) : (float)3600 / DefaultConfigurationProvider.GetConfigCData().VitaminsProductionTime[med.ToString()];
            maxCapacity = array.Length > 4 ? int.Parse(array[4], System.Globalization.CultureInfo.InvariantCulture) : DefaultConfigurationProvider.GetConfigCData().VitaminsCollectorCapacity[med.ToString()];
            capacity = Mathf.Clamp(capacity, 0.0f, maxCapacity); //clamping for a very rare bugs with negative values or values over maxCapacity
            //fillRatioMultiplier = productionTimeMultiplier;
            SetFillRatioMultiplierBalanceable(config);

            if (array.Length > 5)
                parameterCalculator.LoadFromString(array[5]);
            
            Update(timePassed);
        }

        public string SaveToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(med.ToString());
            builder.Append("%");
            builder.Append(capacity.ToString());
            builder.Append("%");
            builder.Append(collectorLevel.ToString());
            builder.Append("%");
            builder.Append(fillRatio.ToString());
            builder.Append("%");
            builder.Append(maxCapacity.ToString());
            builder.Append("%");
            builder.Append(parameterCalculator.SaveToString());

            return builder.ToString();
        }

        public int GetVitaminCollectorUnlockLevel()
        {
            return ResourcesHolder.Get().GetLvlForCure(med);
        }

        private int GetCollectStep()
        {
            return DefaultConfigurationProvider.GetConfigCData().VitaminsCollectStep[med.ToString()];
        }

        public int GetAmountToFillToMaxCapacity()
        {
            return Math.Max(0, maxCapacity - (int)capacity);
        }

        public int Fill(int amount, VitaminSource source)
        {
            int toFillStorage = Game.Instance.gameState().GetTankSorageLeftCapacity();
            int toAdd = amount;// Math.Min(toFillStorage, amount);
            capacity = Mathf.Clamp(Math.Min(capacity + toAdd, maxCapacity), 0.0f, maxCapacity);
            OnCapacityChanged(source, toAdd);
            return toAdd;
        }

        public int Collect()
        {
            if ((int)capacity == 0)
                throw new NothingToCollectException();
            int toFillStorage = Game.Instance.gameState().GetTankSorageLeftCapacity();
            int maxStep = GetCollectStep();
            int amountToCollect = Math.Min(toFillStorage, Math.Min(maxStep, (int)capacity));
            if (amountToCollect <= 0)
                throw new StorageFullException();
            if (Game.Instance.gameState().CanAddResource(med, amountToCollect, false) > 0)
            {
                Game.Instance.gameState().AddResource(med, amountToCollect, false, EconomySource.ProductionMachine);
                int expAmount = ResourcesHolder.Get().GetMedicineInfos(med).Exp * amountToCollect;
                if (Game.Instance.gameState() is GameState)
                {
                    Game.Instance.gameState().AddResource(ResourceType.Exp, expAmount, EconomySource.ProductionMachine, false, "VitaminMaker");
                }
                else if (Game.Instance.gameState() is MaternityGameState)
                {
                    MaternityGameState.Get().AddHospitalExperience(expAmount, EconomySource.ProductionMachine);
                }
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(med, Game.Instance.gameState().GetCureCount(med)));
                AchievementNotificationCenter.Instance.CureProduced.Invoke(new AchievementProgressEventArgs(1));
                MedicineBadgeHintsController.Get().RemoveMedInProduction(med);
                capacity = Mathf.Clamp(capacity - amountToCollect, 0.0f, maxCapacity);
                OnCapacityChanged(VitaminSource.Collect);
                OnVitaminCollected(amountToCollect);
                return amountToCollect;
            }
            throw new StorageFullException();
        }

        public class StorageFullException : Exception { }
        public class NothingToCollectException : Exception { }

        public void Upgrade()
        {
            ++collectorLevel;
            fillRatio = parameterCalculator.GetNextLevelFillRatio(fillRatio);
            maxCapacity = parameterCalculator.GetNextLevelMaxCapacity(maxCapacity);
            parameterCalculator.ClearRandomizedItemsForUpgrade();
            OnCapacityChanged(VitaminSource.Upgrade);            
        }

        private float GetFillRatioBasedOnLevel()
        {
            float result = (float)3600 / DefaultConfigurationProvider.GetConfigCData().VitaminsProductionTime[med.ToString()];

            for (int i = 0; i < collectorLevel; ++i)
            {
                result = parameterCalculator.GetNextLevelFillRatio(result);
            }

            return result;
        }

        private int GetMaxCapacityBasedOnLevel()
        {
            int result = DefaultConfigurationProvider.GetConfigCData().VitaminsCollectorCapacity[med.ToString()];

            for (int i = 0; i < collectorLevel; ++i)
            {
                result = parameterCalculator.GetNextLevelMaxCapacity(result);
            }

            return result;
        }

        public enum VitaminSource
        {
            Default,
            TimeEmulation,
            Advertisement,
            Diamond,
            FullRefill,
            Upgrade,
            Collect,
        }
    }
}
