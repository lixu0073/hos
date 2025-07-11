using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class VipWardConfigParser : MonoBehaviour
{
    //private const string VipUnlockCostParam = "vip_unlock_cost";
    //private const string VipUnlockTimeParam = "vip_unlock_time";
    //private const string VipWardUpgradePatientsParam = "vip_upgrade_room_patients_amount";
    //private const string VipHeliUpgradePatientsParam = "vip_upgrade_heli_patients_amount";
    //private const string VipWardUpgradeCostParamFormat = "vip_upgrade_room_cost{0}"; // {0} - mastery level
    //private const string VipHeliUpgradeCostParamFormat = "vip_upgrade_heli_cost{0}"; // {0} - mastery level
    //private const string VipWardUpgradeInstantCostParam = "vip_upgrade_room_inst";
    //private const string VipHeliUpgradeInstantCostParam = "vip_upgrade_heli_inst";
    //private const string VipWardUpgradeProfitParam = "vip_upgrade_room_profit";
    //private const string VipHeliUpgradeProfitParam = "vip_upgrade_heli_profit";

    //private const char TopSeparator = '/';
    //private const char SubSeparator = '+';

    //private const int VipUnlockCostConfigLenght = 2;
    //private const int VipMasterableConfigLenght = 5; // GetDefaultVipUnlockPatients lenght
    //private const int VipUpgradeCostConfigArrayLenght = 3;
    //private const int VipUpgradeCostConfigLenght = 2;

    //private static ConfigFallback vipConfigFallback = null;
    //private static ConfigFallback VipWardConfigFallback
    //{
    //    get
    //    {
    //        if (vipConfigFallback == null)
    //        {
    //            BaseResourcesHolder rh = ResourcesHolder.Get();
    //            if (rh != null)
    //            {
    //                vipConfigFallback = rh.VipWardConfigFallback;
    //            }
    //        }
    //        return vipConfigFallback;
    //    }
    //}

    //public static KeyValuePair<ResourceType, int> GetVipUnlockCost()
    //{
    //    bool isFallback = false;
    //    string config = VipWardConfig.GetVipWardConfigParameter(VipUnlockCostParam, out isFallback);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        return GetDefaultVipUnlockCost();
    //    }

    //    return ParseVipUnlockCostConfig(config, isFallback);
    //}
    //#region VIPUnlockCost
    //public static KeyValuePair<ResourceType, int> ParseVipUnlockCostConfig(string config, bool isFallback)
    //{
    //    string[] configSplit = config.Split(TopSeparator);

    //    if (configSplit.Length < VipUnlockCostConfigLenght)
    //    {
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + VipUnlockCostParam);
    //            return GetDefaultVipUnlockCost();
    //        }
    //        return ParseVipUnlockCostConfig(VipWardConfigFallback.GetConfig(VipUnlockCostParam), true);
    //    }
    //    ResourceType resourceType;
    //    int amount;
    //    try
    //    {
    //        resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), configSplit[0]);
    //        amount = int.Parse(configSplit[1], System.Globalization.CultureInfo.InvariantCulture);
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Problem with parsing value of " + VipUnlockCostParam + " config");
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + VipUnlockCostParam);
    //            return GetDefaultVipUnlockCost();
    //        }
    //        return ParseVipUnlockCostConfig(VipWardConfigFallback.GetConfig(VipUnlockCostParam), true);
    //    }

    //    configSplit = null;

    //    return new KeyValuePair<ResourceType, int>(resourceType, amount);
    //}

    //private static KeyValuePair<ResourceType, int> GetDefaultVipUnlockCost()
    //{
    //    return new KeyValuePair<ResourceType, int>(ResourceType.Coin, 500);
    //}
    //#endregion

    //#region VIPUnlockTime
    //public static int GetVipUnlockTime()
    //{
    //    bool isFallback = false;
    //    string config = VipWardConfig.GetVipWardConfigParameter(VipUnlockTimeParam, out isFallback);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        return GetDefaultVipUnlockTime();
    //    }

    //    return ParseVipUnlockTimeConfig(config, isFallback);
    //}

    //private static int ParseVipUnlockTimeConfig(string config, bool isFallback)
    //{
    //    int amount;

    //    try
    //    {
    //        amount = int.Parse(config, System.Globalization.CultureInfo.InvariantCulture);
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Problem with parsing value of " + VipUnlockTimeParam + " config");
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + VipUnlockTimeParam);
    //            return GetDefaultVipUnlockTime();
    //        }
    //        return ParseVipUnlockTimeConfig(VipWardConfigFallback.GetConfig(VipUnlockCostParam), true);
    //    }
    //    return amount;
    //}

    //private static int GetDefaultVipUnlockTime()
    //{
    //    return 36000;
    //}
    //#endregion

    //#region VipUnlockPatients
    //public static int[] GetVipWardUnlockPatients()
    //{
    //    return GetUnlockPatients(VipWardUpgradePatientsParam);
    //}

    //public static int[] GetVipHeliUnlockPatients()
    //{
    //    return GetUnlockPatients(VipHeliUpgradePatientsParam);
    //}

    //private static int[] GetUnlockPatients(string param)
    //{
    //    bool isFallback = false;
    //    string config = VipWardConfig.GetVipWardConfigParameter(param, out isFallback);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        return GetDefaultVipUnlockPatients();
    //    }

    //    return ParseVipUnlockPatientsConfig(param, config, isFallback);
    //}

    //private static int[] ParseVipUnlockPatientsConfig(string param, string config, bool isFallback)
    //{
    //    string[] configSplit = config.Split(TopSeparator);

    //    if (configSplit.Length < VipMasterableConfigLenght)
    //    {
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipUnlockPatients();
    //        }
    //        return ParseVipUnlockPatientsConfig(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    int[] toReturn = new int[VipMasterableConfigLenght];

    //    try
    //    {
    //        for (int i = 0; i < VipMasterableConfigLenght; ++i)
    //        {
    //            toReturn[i] = int.Parse(configSplit[i], System.Globalization.CultureInfo.InvariantCulture);
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Problem with parsing value of " + param + " config");
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipUnlockPatients();
    //        }
    //        return ParseVipUnlockPatientsConfig(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    return toReturn;
    //}

    //private static int[] GetDefaultVipUnlockPatients()
    //{
    //    return new int[] { 5, 20, 50, 100, 250 }; // lenght - VipMasterableConfigLenght
    //}
    //#endregion

    //#region VipUpgradeCost
    //public static KeyValuePair<MedicineRef, int>[][] GetVipWardUpgradeCosts()
    //{
    //    KeyValuePair<MedicineRef, int>[][] costs = new KeyValuePair<MedicineRef, int>[VipMasterableConfigLenght][];

    //    for (int i = 0; i < costs.Length; ++i)
    //    {
    //        costs[i] = GetVipWardUpgradeCostForLevel(i+1);
    //    }

    //    return costs;
    //}

    //public static KeyValuePair<MedicineRef, int>[][] GetVipHeliUpgradeCosts()
    //{
    //    KeyValuePair<MedicineRef, int>[][] costs = new KeyValuePair<MedicineRef, int>[VipMasterableConfigLenght][];

    //    for (int i = 0; i < costs.Length; ++i)
    //    {
    //        costs[i] = GetVipHeliUpgradeCostForLevel(i+1);
    //    }

    //    return costs;
    //}

    //public static KeyValuePair<MedicineRef, int>[] GetVipWardUpgradeCostForLevel(int level)
    //{
    //    return GetVipUpgradeCost(string.Format(VipWardUpgradeCostParamFormat, level));
    //}

    //public static KeyValuePair<MedicineRef, int>[] GetVipHeliUpgradeCostForLevel(int level)
    //{
    //    return GetVipUpgradeCost(string.Format(VipHeliUpgradeCostParamFormat, level));
    //}

    //private static KeyValuePair<MedicineRef, int>[] GetVipUpgradeCost(string param)
    //{
    //    bool isFallback = false;
    //    string config = VipWardConfig.GetVipWardConfigParameter(param, out isFallback);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        return GetDefaultVipUpgradeCost();
    //    }

    //    return ParseVipUpgradeCost(param, config, isFallback);
    //}

    //private static KeyValuePair<MedicineRef, int>[] ParseVipUpgradeCost(string param, string config, bool isFallback)
    //{
    //    string[] configSplit = config.Split(TopSeparator);
    //    if (configSplit.Length < VipUpgradeCostConfigArrayLenght)
    //    {
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipUpgradeCost();
    //        }
    //        return ParseVipUpgradeCost(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    KeyValuePair<MedicineRef, int>[] toReturn = new KeyValuePair<MedicineRef, int>[VipUpgradeCostConfigArrayLenght];

    //    try
    //    {
    //        for (int i = 0; i < VipUpgradeCostConfigArrayLenght; ++i)
    //        {
    //            string[] subconfigSplit = configSplit[i].Split(SubSeparator);
    //            if (subconfigSplit.Length < VipUpgradeCostConfigLenght)
    //            {
    //                if (isFallback)
    //                {
    //                    Debug.LogError("Invalid falback for " + param);
    //                    return GetDefaultVipUpgradeCost();
    //                }
    //                return ParseVipUpgradeCost(param, VipWardConfigFallback.GetConfig(param), true);
    //            }

    //            MedicineRef medicine = MedicineRef.Parse(subconfigSplit[0]);
    //            if (medicine == null)
    //            {
    //                if (isFallback)
    //                {
    //                    Debug.LogError("Invalid falback for " + param);
    //                    return GetDefaultVipUpgradeCost();
    //                }
    //                return ParseVipUpgradeCost(param, VipWardConfigFallback.GetConfig(param), true);
    //            }

    //            int amount;
    //            try
    //            {
    //                amount = int.Parse(subconfigSplit[1], System.Globalization.CultureInfo.InvariantCulture);
    //            }
    //            catch (Exception)
    //            {
    //                if (isFallback)
    //                {
    //                    Debug.LogError("Invalid falback for " + param);
    //                    return GetDefaultVipUpgradeCost();
    //                }
    //                return ParseVipUpgradeCost(param, VipWardConfigFallback.GetConfig(param), true);
    //            }


    //            toReturn[i] = new KeyValuePair<MedicineRef, int>(medicine, amount);
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Problem with parsing value of " + param + " config");
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipUpgradeCost();
    //        }
    //        return ParseVipUpgradeCost(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    return toReturn;
    //}

    //private static KeyValuePair<MedicineRef, int>[] GetDefaultVipUpgradeCost()
    //{
    //    KeyValuePair<MedicineRef, int>[] toReturn = new KeyValuePair<MedicineRef, int>[VipUpgradeCostConfigArrayLenght];

    //    for (int i = 0; i < toReturn.Length; ++i)
    //    {
    //        MedicineRef tool = MedicineRef.Parse("Special(" + i + ")");
    //        toReturn[i] = new KeyValuePair<MedicineRef, int>(tool, 10);
    //    }

    //    return toReturn;
    //}
    //#endregion

    

    //#region VipInstantUpgradeCost
    //public static int[] GetVipWardInstantUpgradeCost()
    //{
    //    return GetInstantUpgradeCost(VipWardUpgradeInstantCostParam);
    //}

    //public static int[] GetVipHeliInstantUpgradeCost()
    //{
    //    return GetInstantUpgradeCost(VipHeliUpgradeInstantCostParam);
    //}

    //private static int[] GetInstantUpgradeCost(string param)
    //{
    //    bool isFallback = false;
    //    string config = VipWardConfig.GetVipWardConfigParameter(param, out isFallback);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        return GetDefaultVipInstantUpgradeCost();
    //    }

    //    return ParseVipInstantUpgradeCostConfig(param, config, isFallback);
    //}

    //private static int[] ParseVipInstantUpgradeCostConfig(string param, string config, bool isFallback)
    //{
    //    string[] configSplit = config.Split(TopSeparator);

    //    if (configSplit.Length < VipMasterableConfigLenght)
    //    {
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipInstantUpgradeCost();
    //        }
    //        return ParseVipInstantUpgradeCostConfig(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    int[] toReturn = new int[VipMasterableConfigLenght];

    //    try
    //    {
    //        for (int i = 0; i < VipMasterableConfigLenght; ++i)
    //        {
    //            toReturn[i] = int.Parse(configSplit[i], System.Globalization.CultureInfo.InvariantCulture);
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Problem with parsing value of " + param + " config");
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipInstantUpgradeCost();
    //        }
    //        return ParseVipInstantUpgradeCostConfig(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    return toReturn;
    //}

    //private static int[] GetDefaultVipInstantUpgradeCost()
    //{
    //    return new int[] { 100, 160, 320, 780, 1500 }; // lenght - VipMasterableConfigLenght
    //}
    //#endregion

    //#region VipUpgradeProfit
    //public static float[] GetVipWardUpgradeProfit()
    //{
    //    return GetVipUpgradeProfit(VipWardUpgradeProfitParam);
    //}

    //public static float[] GetVipHeliUpgradeProfit()
    //{
    //    return GetVipUpgradeProfit(VipHeliUpgradeProfitParam);
    //}

    //private static float[] GetVipUpgradeProfit(string param)
    //{
    //    bool isFallback = false;
    //    string config = VipWardConfig.GetVipWardConfigParameter(param, out isFallback);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        return GetDefaultVipUpgradeProfit();
    //    }

    //    return ParseVipUpgradeProfitConfig(param, config, isFallback);
    //}

    //private static float[] ParseVipUpgradeProfitConfig(string param, string config, bool isFallback)
    //{
    //    string[] configSplit = config.Split(TopSeparator);

    //    if (configSplit.Length < VipMasterableConfigLenght)
    //    {
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipUpgradeProfit();
    //        }
    //        return ParseVipUpgradeProfitConfig(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    float[] toReturn = new float[VipMasterableConfigLenght];

    //    try
    //    {
    //        for (int i = 0; i < VipMasterableConfigLenght; ++i)
    //        {
    //            toReturn[i] = float.Parse(configSplit[i], CultureInfo.InvariantCulture);
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        Debug.LogError("Problem with parsing value of " + param + " config");
    //        if (isFallback)
    //        {
    //            Debug.LogError("Invalid falback for " + param);
    //            return GetDefaultVipUpgradeProfit();
    //        }
    //        return ParseVipUpgradeProfitConfig(param, VipWardConfigFallback.GetConfig(param), true);
    //    }

    //    return toReturn;
    //}

    //private static float[] GetDefaultVipUpgradeProfit()
    //{
    //    return new float[] { 1f, 1f, 1f, 1f, 1f }; // lenght - VipMasterableConfigLenght
    //}
    //#endregion
}
