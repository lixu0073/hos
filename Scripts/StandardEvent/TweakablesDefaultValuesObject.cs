using UnityEngine;

[CreateAssetMenu(fileName = "Tweakables Database", menuName = "Tweakables Database")]
public class TweakablesDefaultValuesObject : ScriptableObject
{
    public float GetValueFloat(StandardEventKeys key)
    {
        switch (key)
        {
            default: return 1f;
        }
    }

    public int GetValueInt(BalancableKeys key)
    {
        return GetValueInt(key, false);
    }

    private int GetValueInt(BalancableKeys key, bool revisit)
    {
        switch (key)
        {
            case BalancableKeys.positiveEnergyRewardForHelpInTreatment_MIN: return positiveEnergyRewardForHelpInTreatment_MIN;
            case BalancableKeys.positiveEnergyRewardForHelpInTreatment_MAX: return positiveEnergyRewardForHelpInTreatment_MAX;
            case BalancableKeys.expForGardenHelp_VALUE: return expForGardenHelp_VALUE;
            case BalancableKeys.nextEpidemy_TIME_INTERVAL: return nextEpidemy_TIME_INTERVAL;
            case BalancableKeys.nextVIPArrive_TIME_INTERVAL: return nextVIPArrive_TIME_INTERVAL;
            case BalancableKeys.vipCure_TIME:return vipCure_TIME;
            case BalancableKeys.nextFreeBubbleBoy_TIME_INTERVAL: return nextFreeBubbleBoy_TIME_INTERVAL;
            case BalancableKeys.treasureChestSpawn_TIME_INTERVAL: return treasureChestSpawn_TIME_INTERVAL;
            case BalancableKeys.bacteriaSpread_0_TIME_INTERVAL: return bacteriaSpread_0_TIME_INTERVAL;
            case BalancableKeys.bacteriaSpread_1_TIME_INTERVAL: return bacteriaSpread_1_TIME_INTERVAL;
            case BalancableKeys.bacteriaSpread_2_TIME_INTERVAL: return bacteriaSpread_2_TIME_INTERVAL;
            case BalancableKeys.bacteriaReward_0_VALUE: return bacteriaReward_0_VALUE;
            case BalancableKeys.bacteriaReward_1_VALUE: return bacteriaReward_1_VALUE;
            case BalancableKeys.bacteriaReward_2_VALUE: return bacteriaReward_2_VALUE;
            case BalancableKeys.diamondsInTreasureChestAfterIAP_VALUE: return diamondsInTreasureChestAfterIAP_VALUE;
            case BalancableKeys.storageToolsRange_MIN: return storageToolsRange_MIN;
            case BalancableKeys.storageToolsRange_MAX: return storageToolsRange_MAX;
            case BalancableKeys.tankToolsRange_MIN: return tankToolsRange_MIN;
            case BalancableKeys.tankToolsRange_MAX: return tankToolsRange_MAX;
            case BalancableKeys.maxGiftsPerDay_VALUE: return maxGiftsPerDay_VALUE;

            case BalancableKeys.maternityTimeToSpawnNextMother_MIN: return maternityTimeToSpawnNextMother_MIN;
            case BalancableKeys.maternityTimeToSpawnNextMother_MAX: return maternityTimeToSpawnNextMother_MAX;            
            case BalancableKeys.maternityBloodTestCoinsCost_VALUE: return maternityBloodTestCoinsCost_VALUE;
            case BalancableKeys.maternityBloodTestDuration_TIME_INTERVAL: return maternityBloodTestDuration_TIME_INTERVAL;
            
            case BalancableKeys.maternityLevelStageDifficultyChange_VALUE: return maternityLevelStageDifficultyChange_VALUE;
            case BalancableKeys.maternityLaborStageEasyDuration_TIME_INTERVAL_MIN: return maternityLaborStageEasyDuration_TIME_INTERVAL_MIN;
            case BalancableKeys.maternityLaborStageEasyDuration_TIME_INTERVAL_MAX: return maternityLaborStageEasyDuration_TIME_INTERVAL_MAX;
            case BalancableKeys.maternityLaborStageHardDuration_TIME_INTERVAL_MIN: return maternityLaborStageHardDuration_TIME_INTERVAL_MIN;
            case BalancableKeys.maternityLaborStageHardDuration_TIME_INTERVAL_MAX: return maternityLaborStageHardDuration_TIME_INTERVAL_MAX;
            case BalancableKeys.maternityBondingDuration_TIME_INTERVAL_MIN: return maternityBondingDuration_TIME_INTERVAL_MIN;
            case BalancableKeys.maternityBondingDuration_TIME_INTERVAL_MAX: return maternityBondingDuration_TIME_INTERVAL_MAX;

            case BalancableKeys.vitaminRandomizationLevelThreshold_VALUE: return vitaminRandomizationLevelThreshold_VALUE;
            case BalancableKeys.diamondSpendLimitToShowConfirmation_VALUE: return diamondSpendLimitToShowConfirmation_VALUE;
            case BalancableKeys.helpInTreatmentRoomCounter_VALUE_MAX: return helpInTreatmentRoomCounter_VALUE_MAX;
            case BalancableKeys.helpInTreatmentRoomRequestCooldown_TIME_INTERVAL: return helpInTreatmentRoomRequestCooldown_TIME_INTERVAL;
            case BalancableKeys.helpInTreatmentRoomPushCooldown_TIME_INTERVAL: return helpInTreatmentRoomPushCooldown_TIME_INTERVAL;
            case BalancableKeys.helpInTreatmentRoomLevelToUnlock_MIN: return helpInTreatmentRoomLevelToUnlock_MIN;
            case BalancableKeys.friendsDrawerItems_VALUE_MAX: return friendsDrawerItems_VALUE_MAX;
            case BalancableKeys.ktPlayUnlockLevel_MIN: return ktPlayUnlockLevel_MIN;

            case BalancableKeys.rewardAdMedicineProductionButtonCooldown_TIME_INTERVAL: return 30;

            default:
                if (revisit)
                {
                    Debug.LogError("WTF: " + key);
                    throw new System.NotImplementedException();
                }
                else
                {
                    return Mathf.RoundToInt(GetValueFloat(key));
                }
        }
    }

    public float GetValueFloat(BalancableKeys key)
    {
        return GetValueFloat(key, false);
    }

    private float GetValueFloat(BalancableKeys key, bool revisit)
    {
        switch (key)
        {
            case BalancableKeys.shovelDraw_CHANCE: return shovelDraw_CHANCE;
            case BalancableKeys.shovelDrawForHelpInGarden_CHANCE: return shovelRewardForHelping_CHANCE;
            case BalancableKeys.patientToDiagnose_CHANCE: return patientToDiagnose_CHANCE;
            case BalancableKeys.patientWithBacteria_CHANCE: return patientWithBacteria_CHANCE;
            case BalancableKeys.patientVitaminRequired_CHANCE: return patientRequiresVitamin_CHANCE;
            case BalancableKeys.spawnKid_CHANCE: return spawnKid_CHANCE;

            case BalancableKeys.rewardForTODOSCoinsMultiplikator_VALUE: return rewardForTODOsCoinsMultiplikator_VALUE;
            case BalancableKeys.rewardForTODOSDiamonds_VALUE: return rewardForTODOsDiamonds_VALUE;

            case BalancableKeys.maternityExpToRealtimeSeconds_CONVERSION: return maternityExpToRealtimeSeconds_CONVERSION;
            case BalancableKeys.hospitalExpToMaternityExp_CONVERSION: return hospitalExpToMaternityExp_CONVERSION;
            case BalancableKeys.maternityLaborStageHard_CHANCE: return maternityLaborStageHard_CHANCE;

            case BalancableKeys.exponentialParameterA_VALUE_MIN: return exponentialParameterA_VALUE_MIN;
            case BalancableKeys.exponentialParameterA_VALUE_MAX: return exponentialParameterA_VALUE_MAX;
            case BalancableKeys.exponentialParameterB_VALUE_MIN: return exponentialParameterB_VALUE_MIN;
            case BalancableKeys.exponentialParameterB_VALUE_MAX: return exponentialParameterB_VALUE_MAX;

            case BalancableKeys.shovelDrawFromGoodieBox1_CHANCE: return shovelDrawFromGoodieBox1_CHANCE;
            case BalancableKeys.shovelDrawFromGoodieBox2_CHANCE: return shovelDrawFromGoodieBox2_CHANCE;
            case BalancableKeys.shovelDrawFromGoodieBox3_CHANCE: return shovelDrawFromGoodieBox3_CHANCE;
            case BalancableKeys.shovelDrawFromVIP_CHANCE: return shovelDrawFromVIP_CHANCE;
            case BalancableKeys.shovelDrawFromEpidemyChest_CHANCE: return shovelDrawFromEpidemyChest_CHANCE;
            case BalancableKeys.shovelDrawFromTreasureChest_CHANCE: return shovelDrawFromTreasureChest_CHANCE;

            case BalancableKeys.bonusMedicineProductionForWatchingAd_PERCENT: return 0.2f;

            default:
                if (revisit)
                {
                    Debug.LogError("WTF: " + key);
                    throw new System.NotImplementedException();
                }
                else
                {
                    return GetValueInt(key, true);
                }
        }
    }

    [Header("Helping")]
    [Tooltip("Used in Random.Range(min,max) both inclusives")]
#pragma warning disable 0649
    [SerializeField]
    private int positiveEnergyRewardForHelpInTreatment_MIN;

    [SerializeField]
    [Tooltip("Used in Random.Range(min,max) both inclusives")]
    private int positiveEnergyRewardForHelpInTreatment_MAX;

    [SerializeField]
    private int expForGardenHelp_VALUE;

    [Header("Chances")]
    [SerializeField]
    private float shovelDraw_CHANCE;

    [SerializeField]
    private float shovelRewardForHelping_CHANCE;

    [SerializeField]
    private float patientToDiagnose_CHANCE;

    [SerializeField]
    private float patientWithBacteria_CHANCE;

    [SerializeField]
    private float patientRequiresVitamin_CHANCE;

    [Header("Kids")]
    [SerializeField]
    private float spawnKid_CHANCE;

    [Header("Epidemy")]
    [SerializeField]
    private int nextEpidemy_TIME_INTERVAL;

    [Header("VIP")]
    [SerializeField]
    private int nextVIPArrive_TIME_INTERVAL;

    [SerializeField]
    private int vipCure_TIME;

    [Header("Bubble Boy")]
    [SerializeField]
    private int nextFreeBubbleBoy_TIME_INTERVAL;

    [Header("Treasure Chests")]
    [SerializeField]
    private int treasureChestSpawn_TIME_INTERVAL;

    [Header("Bacteria")]
    [SerializeField]
    private int bacteriaSpread_0_TIME_INTERVAL;

    [SerializeField]
    private int bacteriaSpread_1_TIME_INTERVAL;

    [SerializeField]
    private int bacteriaSpread_2_TIME_INTERVAL;

    [SerializeField]
    private int bacteriaReward_0_VALUE;

    [SerializeField]
    private int bacteriaReward_1_VALUE;

    [SerializeField]
    private int bacteriaReward_2_VALUE;

    [Header("IAP")]
    [SerializeField]
    private int diamondsInTreasureChestAfterIAP_VALUE;

    [Header("Storage Tools")]
    [SerializeField]
    private int storageToolsRange_MIN;

    [SerializeField]
    private int storageToolsRange_MAX;

    [Header("Tank Tools")]
    [SerializeField]
    private int tankToolsRange_MIN;

    [SerializeField]
    private int tankToolsRange_MAX;

    [Header("Misc")]
    [SerializeField]
    private int maxGiftsPerDay_VALUE;

    [SerializeField]
    private float rewardForTODOsCoinsMultiplikator_VALUE;

    [SerializeField]
    private float rewardForTODOsDiamonds_VALUE;

    [Header("Maternity")]
    [SerializeField]
    private int maternityTimeToSpawnNextMother_MIN;

    [SerializeField]
    private int maternityTimeToSpawnNextMother_MAX;

    [SerializeField]
    private int maternityBloodTestCoinsCost_VALUE;

    [SerializeField]
    private int maternityLevelStageDifficultyChange_VALUE;

    [SerializeField]
    private int maternityBloodTestDuration_TIME_INTERVAL;

    [SerializeField]
    private int maternityLaborStageEasyDuration_TIME_INTERVAL_MIN;

    [SerializeField]
    private int maternityLaborStageEasyDuration_TIME_INTERVAL_MAX;

    [SerializeField]
    private int maternityLaborStageHardDuration_TIME_INTERVAL_MIN;

    [SerializeField]
    private int maternityLaborStageHardDuration_TIME_INTERVAL_MAX;

    [SerializeField]
    private int maternityBondingDuration_TIME_INTERVAL_MIN;

    [SerializeField]
    private int maternityBondingDuration_TIME_INTERVAL_MAX;

    [SerializeField]
    private int vitaminRandomizationLevelThreshold_VALUE;

    [SerializeField]
    private int diamondSpendLimitToShowConfirmation_VALUE;

    [SerializeField]
    private int helpInTreatmentRoomCounter_VALUE_MAX;

    [SerializeField]
    private int helpInTreatmentRoomRequestCooldown_TIME_INTERVAL;

    [SerializeField]
    private int helpInTreatmentRoomPushCooldown_TIME_INTERVAL;

    [SerializeField]
    private int helpInTreatmentRoomLevelToUnlock_MIN;

    [SerializeField]
    private int friendsDrawerItems_VALUE_MAX;

    [SerializeField]
    private int ktPlayUnlockLevel_MIN;

    [SerializeField]
    private float maternityExpToRealtimeSeconds_CONVERSION;

    [SerializeField]
    private float hospitalExpToMaternityExp_CONVERSION;

    [SerializeField]
    private float maternityLaborStageHard_CHANCE;

    [SerializeField]
    private float exponentialParameterA_VALUE_MIN;

    [SerializeField]
    private float exponentialParameterA_VALUE_MAX;

    [SerializeField]
    private float exponentialParameterB_VALUE_MIN;

    [SerializeField]
    private float exponentialParameterB_VALUE_MAX;

    [SerializeField]
    private float shovelDrawFromGoodieBox1_CHANCE;

    [SerializeField]
    private float shovelDrawFromGoodieBox2_CHANCE;

    [SerializeField]
    private float shovelDrawFromGoodieBox3_CHANCE;

    [SerializeField]
    private float shovelDrawFromVIP_CHANCE;

    [SerializeField]
    private float shovelDrawFromEpidemyChest_CHANCE;

    [SerializeField]
    private float shovelDrawFromTreasureChest_CHANCE;
#pragma warning restore 0649
}
