using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GiftRewardGeneratorParser : MonoBehaviour
{
    private GiftRewardGeneratorData giftRewardGeneratorData;

    public GiftRewardGeneratorData GetGiftRewardGeneratorData()
    {
        if (giftRewardGeneratorData == null)
        {
//#if UNITY_EDITOR

//            if (BundleManager.Instance != null && BundleManager.Instance.config != null)
//            {
//                SaveDataInCache(DefaultConfigurationProvider.GetConfigCData().GiftRewardGeneratorData, "GiftRewardGeneratorData");
//                giftRewardGeneratorData = GiftRewardGeneratorData.Parse(DefaultConfigurationProvider.GetConfigCData().GiftRewardGeneratorData);
//            }
//            else
//            {
//                var globalEventsData = GetDataFromCache("GiftRewardGeneratorData");
//                if (!string.IsNullOrEmpty(globalEventsData))
//                {
//                    giftRewardGeneratorData = GiftRewardGeneratorData.Parse(globalEventsData);
//                }
//                else
//                {
//                    string devOnlyString = "Coin^60;Diamond^3;Mixture^12;StorageUpgrader^3;Shovel^9;PositiveEnergy^13?" + // chance to obtain specific reward
//                        "AdvancedElixir(0)^25;AdvancedElixir(1)^25;AdvancedElixir(2)^10;AdvancedElixir(3)^15;AdvancedElixir(4)^25?" + // chance to obtain specific reward
//                        "1.0465^2.3256?" + // parameter for gold equation (how many gold per level should player get
//                        "Diamond^1;Mixture^1;StorageUpgrader^1;Shovel^1;PositiveEnergy^1"; // how many of each reward player will get
//                    giftRewardGeneratorData = GiftRewardGeneratorData.Parse(devOnlyString);
//                    Debug.LogError("Gift generator data hardcoded");
//                }
//            }
//#else
              giftRewardGeneratorData = GiftRewardGeneratorData.Parse(DefaultConfigurationProvider.GetConfigCData().GiftRewardGeneratorData);
//#endif
        }
        return giftRewardGeneratorData;
    }

    private string GetDataFromCache(string key)
    {
        string configStr = CacheManager.GetConfigDataFromCache(key);
        if (!string.IsNullOrEmpty(configStr))
        {
            return configStr;
        }
        Debug.LogError("Load " + key + " from cache for Dev_Operations in Unity_Editor");
        return "";
    }

    private void SaveDataInCache(string giftRewardGeneratorData, string key)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(giftRewardGeneratorData);

        string dataStr = sb.ToString();
        CacheManager.SaveConfigDataInCache(key, dataStr);

    }
}
