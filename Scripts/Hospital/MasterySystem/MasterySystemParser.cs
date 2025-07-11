using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MasterySystemParser : MonoBehaviour
{
    private const string VIP_ROOM_NAME = "vipWard";
    private const string VIP_HELIPAD_NAME = "vipHelipad";

    public static MasterySystemParser Instance;

    Dictionary<string, MasterableConfigData> masterySystemConfigData = null;

    //Dictionary<string, string> defaultConfig = new Dictionary<string, string>
    //{
    //    {"BalmLab","240;680;2470?600;1260;5600?1.1;1.1;0.9"},
    //    {"BloodPressure","50;230;1110?600;2040;5600?0.95;0.9;0.85"},
    //    {"BlueDoc","1000;2600;3680;6600?760;1780;2460;4340?1.1;1.1;1.1;0.9"},
    //    {"CapsuleLab","110;410;1610?600;2800;5600?1.1;1.1;0.9"},
    //    {"DripsLab","60;190;680?680;3000;5600?1.1;1.1;0.9"},
    //    {"ElixirLab","400;1940;9740?600;2000;5600?0.95;0.9;0.85"},
    //    {"ExtractLab","30;110;410?820;4080;5600?1.1;1.1;0.9"},
    //    {"EyeDropsLab","140;510;1930?600;2680;5600?1.1;1.1;0.9"},
    //    {"FizzyTabLab","70;240;840?820;3960;5600?1.1;1.1;0.9"},
    //    {"GreenDoc","110;330;3680;6600?600;1460;2340;4340?1.1;1.1;1.1;0.9"},
    //    {"Inhaler Maker","70;210;730?680;2920;5600?1.1;1.1;0.9"},
    //    {"JellyLab","30;80;270?880;3000;5600?1.1;1.1;0.9"},
    //    {"Laser","30;110;530?680;2860;5600?0.95;0.9;0.85"},
    //    {"MicroscopeLab","80;250;920?620;2680;5600?1.1;1.1;0.9"},
    //    {"Mri","40;180;870?600;2360;5600?0.95;0.9;0.85"},
    //    {"NoseLab","140;510;2020?620;3000;5600?1.1;1.1;0.9"},
    //    {"PillLab","110;380;1440?620;3000;5600?1.1;1.1;0.9"},
    //    {"PinkDoc","50;190;390;760?620;2100;4120;5600?1.1;1.1;1.1;0.9"},
    //    {"PurpleDoc","50;290;880;1910?680;4040;12160;20000?1.1;1.1;1.1;0.9"},
    //    {"RedDoc","80;280;490;940?600;1640;2800;5300?1.1;1.1;1.1;0.9"},
    //    {"ShotLab","60;190;660?680;3060;5600?1.1;1.1;0.9"},
    //    {"SkyDoc","50;200;460;950?640;2680;6040;9600?1.1;1.1;1.1;0.9"},
    //    {"SunnyDoc","60;210;390;750?600;1840;3360;5600?1.1;1.1;1.1;0.9"},
    //    {"SyrupLab","200;640;2420?600;1780;5600?1.1;1.1;0.9"},
    //    {"UltraSound","30;130;650?620;2600;5600?0.95;0.9;0.85"},
    //    {"WhiteDoc","70;240;420;800?600;1640;2760;5240?1.1;1.1;1.1;0.9"},
    //    {"Xray","50;270;1340?600;1840;5600?0.95;0.9;0.85"},
    //    {"YellowDoc","280;760;1120;2040?600;1320;1900;3380?1.1;1.1;1.1;0.9"},
    //    {"VitaminMaker","320;990;6680?820;3560;5600?1.1;1.1;0.9"},
    //};

    void Awake()
    {
        if (Instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public MasterableConfigData GetMasterableConfigData(string key)
    {
        if (masterySystemConfigData == null)
        {
            //if (BundleManager.Instance == null || BundleManager.Instance.config == null)
            //{
            //    LoadMasterySystemConfigData(null);
            //}
            //else
            //{
                LoadMasterySystemConfigData(DefaultConfigurationProvider.GetConfigCData().MasterySystemConfig);
            //}
            //masterySystemConfigData.Add(VIP_ROOM_NAME, GetVipRoomConfigData());
            //masterySystemConfigData.Add(VIP_HELIPAD_NAME, GetVipHelipadConfigData());

        }

        if (masterySystemConfigData.ContainsKey(key) && masterySystemConfigData[key] != null)
        {
            return masterySystemConfigData[key];
        }
        else if (masterySystemConfigData.ContainsKey(key))
        {
            Debug.LogError("masterySystemConfigData does not contain key: " + key);
            return null;
        }
        else
        {
            Debug.LogError("masterySystemConfigData value at key: " + key + " is null");
            return null;
        }
    }

    private void LoadMasterySystemConfigData(Dictionary<string, string> config)
    {
        if (masterySystemConfigData != null)
        {
            masterySystemConfigData.Clear();
        }
        masterySystemConfigData = new Dictionary<string, MasterableConfigData>();
        //if (config == null || config.Count == 0)
        //{
        //    foreach (KeyValuePair<string, string> entry in defaultConfig)
        //    {
        //        var info = Hospital.HospitalAreasMapController.HospitalMap.GetPrefabInfo(entry.Key);
        //        if (info == null)
        //        {
        //            continue;
        //        }
        //        if (info.infos.roomController is Hospital.DoctorRoom)
        //        {
        //            masterySystemConfigData.Add(entry.Key, ParseDoctorRoomConfigData(entry.Key, entry.Value, true));
        //            continue;
        //        }
        //        if (info.infos.roomController is DiagnosticRoom)
        //        {

        //            masterySystemConfigData.Add(entry.Key, ParseDiagnosticMachineConfigData(entry.Key, entry.Value, true));
        //            continue;

        //        }
        //        if (info.infos.roomController is Hospital.MedicineProductionMachine || info.infos.roomController is Hospital.VitaminMaker)
        //        {
        //            if (string.Compare(info.infos.Tag, "ElixirLab") == 0)
        //            {
        //                masterySystemConfigData.Add(entry.Key, ParseElixirLabConfigData(entry.Key, entry.Value, true));
        //            }
        //            else
        //            {
        //                masterySystemConfigData.Add(entry.Key, ParseProductionMachineConfigData(entry.Key, entry.Value, true));
        //            }
        //            continue;
        //        }

        //        info = null;
        //    }
        //    return;
        //}
        foreach (KeyValuePair<string, string> entry in config)
        {
            if (entry.Key.Equals(VIP_ROOM_NAME))
            {
                masterySystemConfigData.Add(entry.Key, ParseMasterableVIPRoomConfigData(entry.Key, entry.Value));
                continue;
            }
            else if (entry.Key.Equals(VIP_HELIPAD_NAME))
            {
                masterySystemConfigData.Add(entry.Key, ParseMasterableVIPHelipadConfigData(entry.Key, entry.Value));
                continue;
            }
            else
            {
                var info = Hospital.HospitalAreasMapController.HospitalMap.GetPrefabInfo(entry.Key);
                if (info == null)
                {
                    continue;
                }
                if (info.infos.roomController is Hospital.DoctorRoom)
                {
                    masterySystemConfigData.Add(entry.Key, ParseDoctorRoomConfigData(entry.Key, entry.Value, false));
                    continue;
                }
                if (info.infos.roomController is DiagnosticRoom)
                {
                    masterySystemConfigData.Add(entry.Key, ParseDiagnosticMachineConfigData(entry.Key, entry.Value, false));
                    continue;

                }
                if (info.infos.roomController is Hospital.MedicineProductionMachine || info.infos.roomController is Hospital.VitaminMaker)
                {
                    if (string.Compare(info.infos.Tag, "ElixirLab") == 0)
                    {
                        masterySystemConfigData.Add(entry.Key, ParseElixirLabConfigData(entry.Key, entry.Value, false));
                    }
                    else
                    {
                        masterySystemConfigData.Add(entry.Key, ParseProductionMachineConfigData(entry.Key, entry.Value, false));
                    }
                    continue;
                }

                info = null;
            }
        }
    }


    //private MasterableVIPRoomConfigData GetVipRoomConfigData()
    //{

    //    int[] masteryGoals = VipWardConfigParser.GetVipWardUnlockPatients();

    //    int[] masteryPrices = VipWardConfigParser.GetVipWardInstantUpgradeCost();
        
    //    float[] vipWaitingTimeFactors = VipWardConfigParser.GetVipWardUpgradeProfit();

    //    KeyValuePair<MedicineRef, int>[][] upgradeCosts = VipWardConfigParser.GetVipWardUpgradeCosts();

    //    return new MasterableVIPRoomConfigData(masteryGoals, masteryPrices, vipWaitingTimeFactors, upgradeCosts);
    //}

    //private MasterableVIPHelipadConfigData GetVipHelipadConfigData()
    //{
    //    int[] masteryGoals = VipWardConfigParser.GetVipHeliUnlockPatients();

    //    int[] masteryPrices = VipWardConfigParser.GetVipHeliInstantUpgradeCost();

    //    float[] vipCoolDownsFactors = VipWardConfigParser.GetVipHeliUpgradeProfit();

    //    KeyValuePair<MedicineRef, int>[][] upgradeCosts = VipWardConfigParser.GetVipHeliUpgradeCosts();

    //    return new MasterableVIPHelipadConfigData(masteryGoals, masteryPrices, vipCoolDownsFactors, upgradeCosts);
    //}

    private MasterableVIPHelipadConfigData ParseMasterableVIPHelipadConfigData(string key, string value)
    {
        //5;20;50;100;250?100;160;320;780;1500?0.8333;0.6667;0.5;0.3333;0.1667?Special(0)+2/Special(1)+2/Special(2)+2;Special(0)+5/Special(1)+5/Special(2)+5;Special(0)+10/Special(1)+10/Special(2)+10;Special(0)+25/Special(1)+25/Special(2)+25;Special(0)+50/Special(1)+50/Special(2)+50
        try
        {
            var values = value.Split('?');
            var masteryGoalsData = values[0].Split(";");
            int[] masteryGoals = new int[5];
            for (int i = 0; i < masteryGoalsData.Length; ++i)
            {
                masteryGoals[i] = int.Parse(masteryGoalsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            var masteryPricesData = values[1].Split(";");
            int[] masteryPrices = new int[5];
            for (int i = 0; i < masteryPricesData.Length; ++i)
            {
                masteryPrices[i] = int.Parse(masteryPricesData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            var vipWaitingTimeFactorsData = values[2].Split(";");
            float[] vipWaitingTimeFactors = new float[5];
            for (int i = 0; i < vipWaitingTimeFactorsData.Length; ++i)
            {
                vipWaitingTimeFactors[i] = float.Parse(vipWaitingTimeFactorsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            var upgradeCostsData = values[3].Split(";");
            KeyValuePair<MedicineRef, int>[][] upgradeCosts = new KeyValuePair<MedicineRef, int>[5][];
            for (int i = 0; i < upgradeCostsData.Length; i++)
            {
                var medSplit = upgradeCostsData[i].Split("/");
                var upgradeCostsSingle = new KeyValuePair<MedicineRef, int>[medSplit.Length];
                for (int k = 0; k < medSplit.Length; k++)
                {
                    var subconfigSplit = medSplit[k].Split("+");
                    MedicineRef medicine = MedicineRef.Parse(subconfigSplit[0]);
                    var amount = int.Parse(subconfigSplit[1], System.Globalization.CultureInfo.InvariantCulture);
                    upgradeCostsSingle[k] = new KeyValuePair<MedicineRef, int>(medicine, amount);
                }
                upgradeCosts[i] = upgradeCostsSingle;
            }

            return new MasterableVIPHelipadConfigData(masteryGoals, masteryPrices, vipWaitingTimeFactors, upgradeCosts);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse MasterableVIPHelipadConfigData " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }

    private MasterableVIPRoomConfigData ParseMasterableVIPRoomConfigData(string key, string value)
    {
        //"5;20;50;100;250?100;160;320;780;1500?1.1667;1.3333;1.5;1.6667;1.8333?Special(4)+2/Special(5)+2/Special(6)+2;Special(4)+5/Special(5)+5/Special(6)+5;Special(4)+10/Special(5)+10/Special(6)+10;Special(4)+25/Special(5)+25/Special(6)+25;Special(4)+50/Special(5)+50/Special(6)+50"
        try
        {
            var values = value.Split('?');
            var masteryGoalsData = values[0].Split(";");
            int[] masteryGoals = new int[5];
            for (int i = 0; i < masteryGoalsData.Length; ++i)
            {
                masteryGoals[i] = int.Parse(masteryGoalsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            var masteryPricesData = values[1].Split(";");
            int[] masteryPrices = new int[5];
            for (int i = 0; i < masteryPricesData.Length; ++i)
            {
                masteryPrices[i] = int.Parse(masteryPricesData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            var vipWaitingTimeFactorsData = values[2].Split(";");
            float[] vipWaitingTimeFactors = new float[5];
            for (int i = 0; i < vipWaitingTimeFactorsData.Length; ++i)
            {
                vipWaitingTimeFactors[i] = float.Parse(vipWaitingTimeFactorsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            var upgradeCostsData = values[3].Split(";");
            KeyValuePair<MedicineRef, int>[][] upgradeCosts = new KeyValuePair<MedicineRef, int>[5][];
            for (int i = 0; i < upgradeCostsData.Length; i++)
            {
                var medSplit = upgradeCostsData[i].Split("/");
                var upgradeCostsSingle = new KeyValuePair<MedicineRef, int>[medSplit.Length];
                for (int k = 0; k < medSplit.Length; k++)
                {
                    var subconfigSplit = medSplit[k].Split("+");
                    MedicineRef medicine = MedicineRef.Parse(subconfigSplit[0]);
                    var amount = int.Parse(subconfigSplit[1], System.Globalization.CultureInfo.InvariantCulture);
                    upgradeCostsSingle[k] = new KeyValuePair<MedicineRef, int>(medicine, amount);
                }
                upgradeCosts[i] = upgradeCostsSingle;
            }

            return new MasterableVIPRoomConfigData(masteryGoals, masteryPrices, vipWaitingTimeFactors, upgradeCosts);
        }
        catch(Exception ex)
        {
            Debug.LogError("Failed to parse MasterableVIPRoomConfigData " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }

    private MasterableDoctorRoomConfigData ParseDoctorRoomConfigData(string key, string value, bool isDefault = false)
    {
        try
        {
            var data = value.Split('?');
            var masteryGoalsData = data[0].Split(';');
            var masteryPricesData = data[1].Split(';');
            var multipliersData = data[2].Split(';');

            int[] masteryGoals = new int[4];
            for (int i = 0; i < masteryGoalsData.Length; ++i)
            {
                masteryGoals[i] = int.Parse(masteryGoalsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            int[] masteryPrices = new int[4];
            for (int i = 0; i < masteryPricesData.Length; ++i)
            {
                masteryPrices[i] = int.Parse(masteryPricesData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            float goldMultiplier = float.Parse(multipliersData[0], CultureInfo.InvariantCulture);
            float expMultiplier = float.Parse(multipliersData[1], CultureInfo.InvariantCulture);
            float positiveEnergyMultiplier = float.Parse(multipliersData[2], CultureInfo.InvariantCulture);
            float cureTimeMultiplier = float.Parse(multipliersData[3], CultureInfo.InvariantCulture);

            return new MasterableDoctorRoomConfigData(masteryGoals, masteryPrices, goldMultiplier, expMultiplier, positiveEnergyMultiplier, cureTimeMultiplier);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse MasterableDoctorRoomConfigData " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }

    //private MasterableDoctorRoomConfigData ParseDoctorRoomDefaultConfigData(string key)
    //{
    //    if (defaultConfig == null)
    //    {
    //        Debug.LogError("DefaultConfig is null");
    //        return null;
    //    }
    //    if (!defaultConfig.ContainsKey(key))
    //    {
    //        Debug.LogError("DefaultConfig does not contain key: " + key);
    //        return null;
    //    }

    //    return ParseDoctorRoomConfigData(key, defaultConfig[key], true);
    //}
    
    private MasterableDiagnosticMachineConfigData ParseDiagnosticMachineConfigData(string key, string value, bool isDefault = false)
    {
        try 
        { 
            var data = value.Split('?');
            var masteryGoalsData = data[0].Split(';');
            var masteryPricesData = data[1].Split(';');
            var multipliersData = data[2].Split(';');
 
            int[] masteryGoals = new int[3];
            for (int i = 0; i < masteryGoalsData.Length; ++i)
            {
                masteryGoals[i] = int.Parse(masteryGoalsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            int[] masteryPrices = new int[4];
            for (int i = 0; i < masteryPricesData.Length; ++i)
            {
                masteryPrices[i] = int.Parse(masteryPricesData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            float[] productionTimeMultipliers = new float[3];
            for (int i = 0; i < multipliersData.Length; ++i)
            {
                productionTimeMultipliers[i] = float.Parse(multipliersData[i], CultureInfo.InvariantCulture);
            }

            return new MasterableDiagnosticMachineConfigData(masteryGoals, masteryPrices, productionTimeMultipliers);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse MasterableDiagnosticMachineConfigData " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }

    //private MasterableDiagnosticMachineConfigData ParseDiagnosticMachineDefaultConfigData(string key)
    //{
    //    if (defaultConfig == null)
    //    {
    //        Debug.LogError("DefaultConfig is null");
    //        return null;
    //    }
    //    if (!defaultConfig.ContainsKey(key))
    //    {
    //        Debug.LogError("DefaultConfig does not contain key: " + key);
    //        return null;
    //    }

    //    return ParseDiagnosticMachineConfigData(key, defaultConfig[key], true);
    //}

    private MasterableProductionMachineConfigData ParseProductionMachineConfigData(string key, string value, bool isDefault = false)
    {
        try 
        { 
            var data = value.Split('?');
            var masteryGoalsData = data[0].Split(';');
            var masteryPricesData = data[1].Split(';');
            var multipliersData = data[2].Split(';');
  
            int[] masteryGoals = new int[3];
            for (int i = 0; i < masteryGoalsData.Length; ++i)
            {
                masteryGoals[i] = int.Parse(masteryGoalsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            int[] masteryPrices = new int[4];
            for (int i = 0; i < masteryPricesData.Length; ++i)
            {
                masteryPrices[i] = int.Parse(masteryPricesData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            float goldMultiplier = float.Parse(multipliersData[0], CultureInfo.InvariantCulture);
            float expMultiplier = float.Parse(multipliersData[1], CultureInfo.InvariantCulture);
            float cureTimeMultiplier = float.Parse(multipliersData[2], CultureInfo.InvariantCulture);

            return new MasterableProductionMachineConfigData(masteryGoals, masteryPrices, goldMultiplier, expMultiplier, cureTimeMultiplier);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse MasterableProductionMachineConfigData " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }

    //private MasterableProductionMachineConfigData ParseProductionMachineDefaultConfigData(string key)
    //{
    //    if (defaultConfig == null)
    //    {
    //        Debug.LogError("DefaultConfig is null");
    //        return null;
    //    }
    //    if (!defaultConfig.ContainsKey(key))
    //    {
    //        Debug.LogError("DefaultConfig does not contain key: " + key);
    //        return null;
    //    }

    //    return ParseProductionMachineConfigData(key, defaultConfig[key], true);
    //}

    private MasterableElixirLabConfigData ParseElixirLabConfigData(string key, string value, bool isDefault = false)
    {
        try 
        { 
            var data = value.Split('?');
            var masteryGoalsData = data[0].Split(';');
            var masteryPricesData = data[1].Split(';');
            var multipliersData = data[2].Split(';');
 
            int[] masteryGoals = new int[3];
            for (int i = 0; i < masteryGoalsData.Length; ++i)
            {
                masteryGoals[i] = int.Parse(masteryGoalsData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            int[] masteryPrices = new int[4];
            for (int i = 0; i < masteryPricesData.Length; ++i)
            {
                masteryPrices[i] = int.Parse(masteryPricesData[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            float[] productionTimeMultipliers = new float[3];
            for (int i = 0; i < multipliersData.Length; ++i)
            {
                productionTimeMultipliers[i] = float.Parse(multipliersData[i], CultureInfo.InvariantCulture);
            }

            return new MasterableElixirLabConfigData(masteryGoals, masteryPrices, productionTimeMultipliers);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse MasterableElixirLabConfigData " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }

    //private MasterableElixirLabConfigData ParseElixirLabDefaultConfigData(string key)
    //{
    //    if (defaultConfig == null)
    //    {
    //        Debug.LogError("DefaultConfig is null");
    //        return null;
    //    }
    //    if (!defaultConfig.ContainsKey(key))
    //    {
    //        Debug.LogError("DefaultConfig does not contain key: " + key);
    //        return null;
    //    }

    //    return ParseElixirLabConfigData(key, defaultConfig[key], true);
    //}
}
