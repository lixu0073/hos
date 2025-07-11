using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Hospital
{
    public enum BundledRewardTypes
    {
        none,
        goodieBox,
        specialBox,
        premiumBox,
        awesomeBox,
        superAwesomeBox,
    }

    public class BundledRewardDefinitionConfig
    {
        //private static Dictionary<BundledRewardTypes, string> definitions;
        private static List<BundleRewardCData> _bundleRewardCDatas;
        public static void InstantiateConfig(List<BundleRewardCData> bundleRewardCDatas)
        {
            _bundleRewardCDatas = bundleRewardCDatas;
        }

        //public static void InstantiateConfig(Dictionary<string, object> parametres)
        //{
        //    definitions = new Dictionary<BundledRewardTypes, string>();

        //    foreach (KeyValuePair<string, object> pair in parametres)
        //    {

        //        try
        //        {
        //            BundledRewardTypes type = (BundledRewardTypes)Enum.Parse(typeof(BundledRewardTypes), pair.Key);

        //            if (pair.Value is string)
        //            {
        //                definitions.Add(type, pair.Value as string);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError(e.Message);
        //            continue;
        //        }
        //    }
        //}

        private static bool TryGetDefinition(BundledRewardTypes key, out string definition)
        {
            foreach (var item in _bundleRewardCDatas)
            {
                if (item.BundledRewardType == key) 
                {
                    definition = item.Value;
                    return true;
                }
            }
            Debug.LogError("Unable to find configuration for BundledRewardType " + key);
            definition = "";
            return false;
        }

        public static string RecoverRemainingBundleDefinition(BundledRewardTypes key)
        {
            string bundleFullDefinition = "";
            TryGetDefinition(key, out bundleFullDefinition);
            return bundleFullDefinition;
        }
    }
}
