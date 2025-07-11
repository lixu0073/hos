using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyDealResourceUI : MonoBehaviour {

    public void ShowTooltip() {
        if (ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item.type == MedicineType.Fake && ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item.id == 0) {
            TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/POSITIVE_ENERGY"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/POSITIVE_ENERGY_TOOLTIP"));
        }
        if (ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item.type == MedicineType.Special) {
            switch (ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal.Item.id) {
                case 0:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SCREWDRIVER"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_STORAGE_TOOLTIP"));
                    break;
                case 1:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/HAMMER"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_STORAGE_TOOLTIP"));
                    break;
                case 2:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPANNER"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_STORAGE_TOOLTIP"));
                    break;
                case 3:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SHOVEL"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SHOVEL_TOOLTIP"));
                    break;
                case 4:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/WASHER"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_TANK_TOOLTIP"));
                    break;
                case 5:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/PLANK"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_TANK_TOOLTIP"));
                    break;
                case 6:
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/PIPE"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_TANK_TOOLTIP"));
                    break;
                default:
                    break;
            }
        }
    }
}
