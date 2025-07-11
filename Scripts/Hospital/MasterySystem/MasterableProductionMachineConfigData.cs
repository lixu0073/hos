using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterableProductionMachineConfigData : MasterableConfigData {
    public float GoldMultiplier {
        get;
        private set;
    }
    public float ExpMultiplier {
        get;
        private set;
    }
    public float ProductionTimeMultiplier {
        get;
        private set;
    }

    public MasterableProductionMachineConfigData(int[] masteryGoals, int[] masteryPrices, float goldMultiplier, float expMultiplier, float cureMultiplier):base(masteryGoals, masteryPrices) {
        GoldMultiplier = goldMultiplier;
        ExpMultiplier = expMultiplier;
        ProductionTimeMultiplier = cureMultiplier;
    }
}
