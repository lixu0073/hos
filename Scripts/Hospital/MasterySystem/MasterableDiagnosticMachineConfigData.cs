using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterableDiagnosticMachineConfigData : MasterableConfigData {
    public float[] ProductionTimeMultipliers {
        get;
        private set;
    }

    public MasterableDiagnosticMachineConfigData(int[] masteryGoals, int[] masteryPrices, float[] productionTimeMultipliers):base(masteryGoals, masteryPrices){
        ProductionTimeMultipliers = productionTimeMultipliers;
    }
}
