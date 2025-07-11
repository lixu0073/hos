using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DevelopGameParameters")]
public class DevelopGameParameters : ScriptableObject
{
    [Header("Only Unity Editor related stuff")]
    public string TestCognito;
    public string DevelopSceneUsedVersion;
    public string DevelopSceneUsedSave;
    public bool useDevelopSceneForcedCognito;
    public int expDevIncrease;
    public List<MockedStandardEvent> mockedEventData;
    public List<MockedBalancedValue> mockedBalanceData;
    public List<MockedGlobalEvents> mockedGlobalEvents;
    public List<MockedDataFromDelta> mockedDailyRewards;
    public List<MockedDataFromDelta> mockedBundledDefinitions;
    public List<MockedDataFromDelta> mockedFakeContributionConfig;
    public List<MockedDataFromDelta> mockedGiftsPerLevel;
    public List<MockedDataFromDelta> MockedIapShopOrder;
    public List<MockedDataFromDelta> MockedIapShopBundles;
    public List<MockedDataFromDelta> MockedIapShopCoins;
    public List<MockedDataFromDelta> MockedDailyRewardPopupType;
    public List<MockedDataFromDelta> MockedCaseTierRewards;
    public List<MockedDataFromDelta> MockedDiamondFromTreasuresBool;
    [Tooltip("Need to select all data so that this will work properly")]
    public List<MockedDataFromDelta> MockedAdSpeedUpOfMedicine;
    [Tooltip("Setup -1 if You do not want to use it")]
    public int hospitalTimePassed;
    [Tooltip("Setup -1 if You do not want to use it")]
    public int maternityTimePassed;
    [Tooltip("Show tutorials in game.")]
    public bool showTutorials = true;
    [Tooltip("Only for Editor.")]
    public bool isIphoneX;
    [Header("Release/Dev toggles")]
    public bool IapShopControllerIsTestBuild;
    public bool blockSaving;
    public bool enableDevTestUIOnStart;
    public bool devTestButtonVisible;
    public bool sendExceptionViaEmail;
    public bool EnableDebugLog;
    public bool UseAssetBundles;
    public bool TenjinTestBuild;
    public bool ForceToShowStartingPanel;
    [Tooltip("For release purpose set it to false")]
    public bool PharmacyMigrationOffersOn;
    public string GameVersion;
    [Tooltip("For dev purpose set it to 1000")]
    public int MainSceneAssetBundleVersion;
    [Tooltip("For dev purpose set it to 1000")]
    public int MaternitySceneAssetBundleVersion;
    [Tooltip("For dev purpose set it to 1000")]
    public int FontsAssetBundleVersion;
    public string GetTestCognito()
    {
        return useDevelopSceneForcedCognito ? TestCognito + "_" + DevelopSceneUsedVersion + "_" + DevelopSceneUsedSave : TestCognito;
    }

    public void SetCoinAmount(int coinsAmountToSet)
    {
        Game.Instance.gameState().SetCoinAmount(coinsAmountToSet);
        Hospital.SaveSynchronizer.Instance.InstantSave();
    }

    public void SetDiamondAmount(int diamondAmountToSet)
    {
        Game.Instance.gameState().SetDiamondAmount(diamondAmountToSet);
        Hospital.SaveSynchronizer.Instance.InstantSave();
    }

    public void AddLevels(int additionalLevels)
    {
        for (int i = 0; i < additionalLevels; i++)
        {
            Game.Instance.gameState().LevelUp();
        }
        Hospital.SaveSynchronizer.Instance.InstantSave();
    }

    public void AddResource(MedicineRef medicine, int amount)
    {
        Game.Instance.gameState().AddResource(medicine, amount, true, EconomySource.TestModeIAP);
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public bool DevelopAWS;
    public bool UseMockedStandardEvents;
    public bool UseMockedBalancables;
    public bool UseMockedGlobalEvents;
    public bool UseMockedIapShop;
    public bool UseMockedDailyRewards;
    public bool UseMockedBundledRewardDefinitions;
    public bool UseMockedFakeContributionConfig;
    public bool UseMockedGiftsPerLevel;
    public bool UseMockedDailyRewardPopupType;
    public bool UseMockedAdSpeedUpOfMedicine;
    public bool UseMockedCaseTierPrize;
    public bool UseMockedDiamondFromTreasuresBool;
}
