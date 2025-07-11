using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using System;

[System.Serializable]
public abstract class DailyQuestRewardFactory
{
    public abstract RewardPackage GetRewardPackage(int dayNumber);

    protected List<MedicineDatabaseEntry> AllMedicineList;
    protected List<MedicineDatabaseEntry> AvaiableMedicineForCurrentLevel;
    protected List<MedicineRef> CurrentlyNeededMedicine;
    protected RewardPackage rewardPackage;

    public DailyQuestRewardFactory()
    {
        AllMedicineList = new List<MedicineDatabaseEntry>();
        AvaiableMedicineForCurrentLevel = new List<MedicineDatabaseEntry>();
        CurrentlyNeededMedicine = new List<MedicineRef>();
        SetupMedicineList();
    }

    private void SetupMedicineList()
    {
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.BaseElixir));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.AdvancedElixir));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.BasePlant));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Fake));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Syrop));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.NoseDrops));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.EyeDrops));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Capsule));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Pill));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.FizzyTab));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.InhaleMist));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Shot));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Extract));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Drips));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Jelly));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Balm));
        AllMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Bacteria));
    }

    protected void GetAllAvaiableMedicine()
    {
        AvaiableMedicineForCurrentLevel.Clear();
        AvaiableMedicineForCurrentLevel = AllMedicineList.FindAll(x => x.minimumLevel <= Game.Instance.gameState().GetHospitalLevel());
    }

    protected void GetAllCurrentlyNeededMedicine()
    {
        CurrentlyNeededMedicine.Clear();
        CurrentlyNeededMedicine.AddRange(MedicineBadgeHintsController.Get().GetMedicineNeedToHeal());
    }

    protected MedicineRef GetMedicine()
    {
        if (CurrentlyNeededMedicine.Count>=3)
        {
            int index = GameState.RandomNumber(0, CurrentlyNeededMedicine.Count);
            return CurrentlyNeededMedicine[index];
        }
        else
        {
            int index = GameState.RandomNumber(0, AvaiableMedicineForCurrentLevel.Count);
            return AvaiableMedicineForCurrentLevel[index].GetMedicineRef();
        }
    }

    protected int AmountOfMmedicineAsReward(MedicineRef medicine)
    {
        if (medicine.type == MedicineType.BaseElixir || medicine.type == MedicineType.AdvancedElixir || medicine.type == MedicineType.BasePlant)
        {
            return 3;
        }
        else
        {
            return 1;
        }
    }

    protected int AmountOfGoldAsReward(int dayNumber)
    {
        return 50 + dayNumber * 50;
    }

    protected int AmountOfDiamondsAsReward()
    {
        return 1;
    }

    public enum DailyQuestRewardType
    {
        Diamond,
        Decoration,
        Booster,
        Medicine,
        Coin,
    }

}
