using UnityEngine;
using System.Collections;
using Hospital;
using System;

public struct PackageData 
{
    public MedicineDatabaseEntry Medicine { get; private set; }
    public int AmountOfMedicine { get; private set; }
    public int GoldReward { get; private set; }
    public int ExpReward { get; private set; }

    public void SetMedicine (MedicineDatabaseEntry medicine)
    {
        Medicine = medicine;
    }

    public void SetGoldReward(int goldReward)
    {
        GoldReward = goldReward;
    }

    public void SetExpReward(int expReward)
    {
        ExpReward = expReward;
    }

    public void SetAmountOfMedicine(int amount)
    {
        AmountOfMedicine = amount;
    }
    
}
