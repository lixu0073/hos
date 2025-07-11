using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum BalancableKeys
{
    positiveEnergyRewardForHelpInTreatment_MIN,
    positiveEnergyRewardForHelpInTreatment_MAX,    
    shovelDraw_CHANCE,
    spawnKid_CHANCE,
    nextEpidemy_TIME_INTERVAL,
    vipCure_TIME,
    nextVIPArrive_TIME_INTERVAL,
    nextFreeBubbleBoy_TIME_INTERVAL,
    treasureChestSpawn_TIME_INTERVAL,
    bacteriaSpread_0_TIME_INTERVAL,
    bacteriaSpread_1_TIME_INTERVAL,
    bacteriaSpread_2_TIME_INTERVAL,
    expForGardenHelp_VALUE,
    patientToDiagnose_CHANCE,
    patientWithBacteria_CHANCE,
    bacteriaReward_0_VALUE,
    bacteriaReward_1_VALUE,
    bacteriaReward_2_VALUE,
    shovelDrawForHelpInGarden_CHANCE,
    diamondsInTreasureChestAfterIAP_VALUE,
    storageToolsRange_MIN,
    storageToolsRange_MAX,
    tankToolsRange_MAX,
    tankToolsRange_MIN,   
    maxGiftsPerDay_VALUE,
    rewardForTODOSCoinsMultiplikator_VALUE,
    rewardForTODOSDiamonds_VALUE,
    patientVitaminRequired_CHANCE,
    maternityTimeToSpawnNextMother_MIN,
    maternityTimeToSpawnNextMother_MAX,
    maternityExpToRealtimeSeconds_CONVERSION,
    maternityBloodTestCoinsCost_VALUE,
    maternityBloodTestDuration_TIME_INTERVAL,
    maternityLaborStageHard_CHANCE,
    maternityLevelStageDifficultyChange_VALUE,
    maternityLaborStageEasyDuration_TIME_INTERVAL_MIN,
    maternityLaborStageEasyDuration_TIME_INTERVAL_MAX,
    maternityLaborStageHardDuration_TIME_INTERVAL_MIN,
    maternityLaborStageHardDuration_TIME_INTERVAL_MAX,
    maternityBondingDuration_TIME_INTERVAL_MIN,
    maternityBondingDuration_TIME_INTERVAL_MAX,

    vitaminRandomizationLevelThreshold_VALUE,
    exponentialParameterA_VALUE_MIN,
    exponentialParameterA_VALUE_MAX,
    exponentialParameterB_VALUE_MIN,
    exponentialParameterB_VALUE_MAX,

    hospitalExpToMaternityExp_CONVERSION,
    diamondSpendLimitToShowConfirmation_VALUE,
    helpInTreatmentRoomCounter_VALUE_MAX,
    helpInTreatmentRoomRequestCooldown_TIME_INTERVAL,
    helpInTreatmentRoomPushCooldown_TIME_INTERVAL,
    helpInTreatmentRoomLevelToUnlock_MIN,
    friendsDrawerItems_VALUE_MAX,
    ktPlayUnlockLevel_MIN,

    shovelDrawFromGoodieBox1_CHANCE,
    shovelDrawFromGoodieBox2_CHANCE,
    shovelDrawFromGoodieBox3_CHANCE,
    shovelDrawFromVIP_CHANCE,
    shovelDrawFromEpidemyChest_CHANCE,
    shovelDrawFromTreasureChest_CHANCE,

    bonusMedicineProductionForWatchingAd_PERCENT,
    rewardAdMedicineProductionButtonCooldown_TIME_INTERVAL
}
public static class BalancableConfig
{
    //private static Dictionary<BalancableKeys, float> balanceValues = new Dictionary<BalancableKeys, float>();
    private static BalanceCData _balanceCData;

    public static void InitializeBalancable(BalanceCData balanceCData)
    {
        _balanceCData = balanceCData;
    }

    //public static void InitializeBalancable(Dictionary<string, object> parameters)
    //{
    //    balanceValues.Clear();
    //    foreach (KeyValuePair<string, object> pair in parameters)
    //    {
    //        try
    //        {
    //            BalancableKeys paramKey = (BalancableKeys)Enum.Parse(typeof(BalancableKeys), pair.Key);
    //            string unparsedData = (string)pair.Value;
    //            float value = float.Parse(unparsedData, System.Globalization.CultureInfo.InvariantCulture);
    //            if (balanceValues.ContainsKey(paramKey))
    //            {
    //                balanceValues[paramKey] = value;
    //            }
    //            else
    //            {
    //                balanceValues.Add(paramKey, value);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogError(e.Message);
    //            continue;
    //        }
    //    }
    //}
    public static float GetBalanceValue(BalancableKeys key)
    {
        Type t = _balanceCData.GetType();
        FieldInfo[] props = t.GetFields();
        foreach (var prop in props)
        {
            if (prop.Name == key.ToString())
            {
                try
                {
                    if (prop.FieldType == typeof(int))
                    {
                        return (int)prop.GetValue(_balanceCData);
                    }
                    else
                    {
                        return (float)prop.GetValue(_balanceCData);
                    }
                }
                catch(Exception ex)
                {
                    Debug.LogError("FAiledt cast " + key + " " + prop.FieldType);
                }
            }
        }
        Debug.LogError("GetBalanceValue Failed for " + key.ToString());
        return 0f;
    }
    //public static float GetBalanceValue(BalancableKeys key, float defaultValue)
    //{
    //    if (balanceValues.Count == 0)
    //    {
    //        return defaultValue;
    //    }
    //    if (balanceValues.ContainsKey(key))
    //    {
    //        return balanceValues[key];
    //    }
    //    else
    //    {
    //        return defaultValue;
    //    }
    //}
}
