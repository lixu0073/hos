using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using System;
using System.Linq;

public class EpidemyPackageGenerator : MonoBehaviour
{
    /// <summary>
    /// List containing Elixirs, Advanced Elixirs, BasePlants.
    /// </summary>
    private List<MedicineDatabaseEntry> lowQualityRewards;

    /// <summary>
    /// List containing all Medicine.
    /// </summary>
    private List<MedicineDatabaseEntry> medicineRewardList;

    private int numberOfPackages;
    private const int numberOfMedicineTypePerEpidemy = 3;

    public void StartUp()
    {
        SetUpLists();
    }

    public MedicineDatabaseEntry[] GetAllMedicineTypeInPackages (int playerLevel)
    {
        MedicineDatabaseEntry[] arrayToReturn = new MedicineDatabaseEntry[numberOfMedicineTypePerEpidemy];
        arrayToReturn[0] = (AddItemFromLowQualityRewards(playerLevel));
        arrayToReturn[1] = (AddItemFromMedicineFromLast4Levels(playerLevel));
        arrayToReturn[2] = (AddItemFromMedicineFromAllLevelsExceptLast4Levels(playerLevel));
        arrayToReturn.CTShuffle();
        return arrayToReturn;
    }

    public List<PackageData> GetPackagesForGivenMedicineType (int playerLevel, MedicineDatabaseEntry[] medicineTypesInPackages)
    {
        List<PackageData> packageToReturnList = new List<PackageData>();
        numberOfPackages = GetNumberOfPackages(playerLevel);

        for (int i = 0; i < numberOfMedicineTypePerEpidemy; i++)
        {
            PackageData packageData = new PackageData();
            packageData.SetMedicine(medicineTypesInPackages[i]);

            int amountOfMedicine = CalculateAmountOfMedicine(playerLevel, packageData.Medicine.maxPrice);
            packageData.SetAmountOfMedicine(amountOfMedicine);

            int totalRewardForPackage = CalculateTotalRewardForPackage(packageData.AmountOfMedicine, packageData.Medicine.maxPrice);

            int goldRewardForPackage = CalculateGoldReward(totalRewardForPackage);
            packageData.SetGoldReward(goldRewardForPackage);

            int expRewardForPackage = CalculateExpReward(totalRewardForPackage, goldRewardForPackage);
            packageData.SetExpReward(expRewardForPackage);

            int numberOfPackagesPerRow = numberOfPackages / numberOfMedicineTypePerEpidemy;

            for (int j = 0; j < numberOfPackagesPerRow; j++)
            {
                packageToReturnList.Add(packageData);
            }
        }
        return packageToReturnList;
    }

    public int GetNumberOfPackages(int playerLevel)
    {
        if (playerLevel <= 19)
        {
            return 3;
        }
        else if (playerLevel > 19 && playerLevel <= 24)
        {
            return 6;
        }
        else if (playerLevel > 24 && playerLevel <= 39)
        {
            return 9;
        }
        else if (playerLevel > 39)
        {
            return 12;
        }
        else
        {
            Debug.Log("Number of packages has not been set properly");
            return 0;
        }
    }

    private int CalculateAmountOfMedicine(int playerLevel, int maxPrice)
    {
        int amountToReturn = 0;

        if (maxPrice <= 140)
        {
            amountToReturn = Mathf.FloorToInt((51.152f * Mathf.Pow(maxPrice, -0.483f) + 1.1f * Mathf.Log(playerLevel) + Mathf.Pow(playerLevel, 0.45f) * (playerLevel / maxPrice)) * 0.3f);
        }
        else if (maxPrice >= 141 && maxPrice < 300)
        {
            amountToReturn = Mathf.FloorToInt(1.1f + (1.1f*Mathf.Log(playerLevel)+Mathf.Pow(playerLevel, 0.15f)*(playerLevel/maxPrice))*0.4f);
        }
        else if (maxPrice >= 300)
        {
            amountToReturn = Mathf.FloorToInt(0.005f + (1.1f * Mathf.Log(playerLevel) + Mathf.Pow(playerLevel, 0.15f) * (playerLevel / maxPrice)) * 0.3f); ;
        }

        if (amountToReturn == 0)
        {
            Debug.Log("Amount of medicine was not corectly calculated");
        }

        return amountToReturn;
    }

    private int CalculateTotalRewardForPackage(int medicineAmount, int medicineMaxPrice)
    {
        return Mathf.FloorToInt(medicineAmount * medicineMaxPrice * 1.1f);
    }

    private int CalculateGoldReward(int totalReward)
    {
        float ratio = UnityEngine.Random.Range(1.95f, 2.05f);
        return Mathf.FloorToInt(totalReward / ratio);
    }

    private int CalculateExpReward(int totalRewardForPackage, int goldRewardForPackage)
    {
        return totalRewardForPackage - goldRewardForPackage;
    }

    private MedicineDatabaseEntry AddItemFromLowQualityRewards(int playerLevel)
    {
        List<MedicineDatabaseEntry> tempList = lowQualityRewards.FindAll(x => x.minimumLevel <= playerLevel);
        if (tempList.Count == 0)
        {
            Debug.Log("Something went wrong. Medicine list is empty.");
            return null;
        }
        else
        {
            int index = UnityEngine.Random.Range(0, tempList.Count);
            return tempList[index];
        }
    }

    private MedicineDatabaseEntry AddItemFromMedicineFromLast4Levels(int playerLevel)
    {
        EpidemyPackageComparer packageComparerByMinimumLevel = new EpidemyPackageComparer();
        List<MedicineDatabaseEntry> tempList = new List<MedicineDatabaseEntry>();
        tempList = medicineRewardList.FindAll(x => x.minimumLevel <= playerLevel);
        tempList.Sort(packageComparerByMinimumLevel);
        HashSet<int> levelsIncluded = new HashSet<int>();

        for (int i = 0; i < tempList.Count; i++)
        {
            levelsIncluded.Add(tempList[i].minimumLevel);
            if (levelsIncluded.Count == 3)
            {
                tempList.RemoveAll(x => x.minimumLevel < Enumerable.ElementAt<int>(levelsIncluded,2));
                break;
            }
        }

        if (tempList.Count == 0)
        {
            Debug.Log("Something went wrong. Medicine list is empty.");
            return null;
        }
        else
        {
            int index = UnityEngine.Random.Range(0, tempList.Count);
            return tempList[index];
        }
    }

    private MedicineDatabaseEntry AddItemFromMedicineFromAllLevelsExceptLast4Levels(int playerLevel)
    {
        EpidemyPackageComparer packageComparerByMinimumLevel = new EpidemyPackageComparer();
        List<MedicineDatabaseEntry> tempList = new List<MedicineDatabaseEntry>();
        tempList = medicineRewardList.FindAll(x => x.minimumLevel <= playerLevel);
        tempList.Sort(packageComparerByMinimumLevel);
        HashSet<int> levelsIncluded = new HashSet<int>();

        for (int i = 0; i < tempList.Count; i++)
        {
            levelsIncluded.Add(tempList[i].minimumLevel);
            if (levelsIncluded.Count == 3)
            {
                tempList = tempList.FindAll(x => x.minimumLevel < Enumerable.ElementAt<int>(levelsIncluded, 2));
                break;
            }
        }

        if (tempList.Count == 0)
        {
            Debug.Log("Something went wrong. Medicine list is empty.");
            return null;
        }
        else
        {
            int index = UnityEngine.Random.Range(0, tempList.Count);
            return tempList[index];
        }
    }

    #region Setting up lists with all rewards at game startup


    private void SetUpLists()
    {
        SetupLowQualityRewardsList();
        SetupMedicineList();
    }

    private void SetupLowQualityRewardsList()
    {
        lowQualityRewards = new List<MedicineDatabaseEntry>();
        lowQualityRewards.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.BaseElixir));
        lowQualityRewards.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.AdvancedElixir));
        lowQualityRewards.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.BasePlant));
        lowQualityRewards.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Fake));
    }

    private void SetupMedicineList()
    {
        medicineRewardList = new List<MedicineDatabaseEntry>();
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Syrop));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.NoseDrops));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.EyeDrops));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Capsule));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Pill));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.FizzyTab));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.InhaleMist));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Shot));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Extract));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Drips));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Jelly));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Balm));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Bacteria));
        medicineRewardList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Vitamins));
    }
    #endregion
}
