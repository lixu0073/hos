using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterableVIPRoomConfigData : MasterableConfigData
{
    public KeyValuePair<MedicineRef, int>[][] UpgradeCosts
    {
        get;
        private set;
    }

    public float[] PatientInBedTimeMultipliers
    {
        get;
        private set;
    }
    
    public MasterableVIPRoomConfigData(int[] masteryGoals, int[] masteryPrices, float[] patientInBedTImeMultipliers, KeyValuePair<MedicineRef, int>[][] upgradeCosts) : base(masteryGoals, masteryPrices)
    {
        PatientInBedTimeMultipliers = patientInBedTImeMultipliers;
        UpgradeCosts = upgradeCosts;
    }
}

public class MasterableVIPHelipadConfigData : MasterableConfigData
{
    public KeyValuePair<MedicineRef, int>[][] UpgradeCosts
    {
        get;
        private set;
    }

    public float[] VipArrivalTimeMultipliers
    {
        get;
        private set;
    }

    public MasterableVIPHelipadConfigData(int[] masteryGoals, int[] masteryPrices, float[] vipArrivalTImeMultipliers, KeyValuePair<MedicineRef, int>[][] upgradeCosts) : base(masteryGoals, masteryPrices)
    {
        VipArrivalTimeMultipliers = vipArrivalTImeMultipliers;
        UpgradeCosts = upgradeCosts;
    }
}
