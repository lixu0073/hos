using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterableDoctorRoomConfigData : MasterableConfigData {
    public float GoldMultiplier {
        get;
        private set;
    }
    public float ExpMultiplier {
        get;
        private set;
    }
    public float PositiveEnergyMultiplier {
        get;
        private set;
    }
    public float CureTimeMultiplier {
        get;
        private set;
    }

    public MasterableDoctorRoomConfigData(int[] masteryGoals, int[] masteryPrices, float goldMultiplier, float expMultiplier, float positiveEnergyMultiplier, float cureTimeMultiplier):base(masteryGoals, masteryPrices) {
        GoldMultiplier = goldMultiplier;
        ExpMultiplier = expMultiplier;
        PositiveEnergyMultiplier = positiveEnergyMultiplier;
        CureTimeMultiplier = cureTimeMultiplier;
    }
}
