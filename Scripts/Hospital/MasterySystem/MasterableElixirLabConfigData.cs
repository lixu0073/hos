using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterableElixirLabConfigData : MasterableConfigData
{
    public float[] ProductionTimeMultipliers
    {
        get;
        private set;
    }

    public MasterableElixirLabConfigData(int[] masteryGoals, int[] masteryPrices, float[] productionTimeMultipliers):base(masteryGoals, masteryPrices){
        ProductionTimeMultipliers = productionTimeMultipliers;
    }
}
