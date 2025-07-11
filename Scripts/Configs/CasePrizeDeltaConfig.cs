using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CasePrizeDeltaConfig
{
    public enum Tiers
    {
        tier1 = 0,
        tier2 = 1,
        tier3 = 2,
    }

    //private static Dictionary<int, string> unparsedCaseTierDeltaMap = new Dictionary<int, string>();
    private static List<CaseTierRewardCData> _caseTierRewardCDatas;
    public static void Initialize(List<CaseTierRewardCData> caseTierRewardCData)
    {
        _caseTierRewardCDatas = caseTierRewardCData;
    }

    //public static void Initialize(Dictionary<string, object> parameters)
    //{
    //    foreach (KeyValuePair<string,object> item in parameters)
    //    {
    //        try
    //        {
    //            Tiers caseTier = (Tiers)Enum.Parse(typeof(Tiers), item.Key);
    //            if (!unparsedCaseTierDeltaMap.ContainsKey((int)caseTier))
    //            {
    //                unparsedCaseTierDeltaMap.Add((int)caseTier, (string)item.Value);
    //            }
    //        }
    //        catch (System.Exception)
    //        {
    //            Debug.LogError("Problem with parsing string: " + item.Key.ToString() + " to enum Tiers");
    //        }
    //    }
    //}

    public static string GetCaseTierConfigForGivenTier(int tier)
    {
        foreach (var item in _caseTierRewardCDatas)
        {
            if ((int)item.Tier == tier)
            {
                return item.Value;
            }
        }
        Debug.LogError("Could not find Case Tier Config for tier " + tier);
        return "";
        //string caseConfig = String.Empty;
        //unparsedCaseTierDeltaMap.TryGetValue(tier, out caseConfig);
        //if (String.IsNullOrEmpty(caseConfig))
        //{
        //    caseConfig = ResourcesHolder.Get().CasePrizeFallBack.GetPrizeFallbackForTier(tier);
        //}
        //return caseConfig;
    }
}
