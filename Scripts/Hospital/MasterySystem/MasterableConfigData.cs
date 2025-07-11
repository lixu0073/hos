using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterableConfigData {
    public int[] MasteryGoals {
        get;
        private set;
    }

    public int[] MasteryPrices {
        get;
        private set;
    }

    public MasterableConfigData(int[] masteryGoals, int[] masteryPrices) {
        MasteryGoals = masteryGoals;
        MasteryPrices = masteryPrices;
    }

}
