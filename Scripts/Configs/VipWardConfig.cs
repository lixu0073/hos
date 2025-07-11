using System;
using System.Collections.Generic;
using UnityEngine;

public class VipWardConfig
{
    //private static Dictionary<string, string> unparsedMap = new Dictionary<string, string>();

    //public static void Initialize(Dictionary<string, object> parameters)
    //{
    //    if (parameters == null)
    //    {
    //        Debug.LogError("null parameters");
    //        return;
    //    }

    //    foreach (KeyValuePair<string, object> item in parameters)
    //    {
    //        if (item.Value.GetType() != typeof(string))
    //        {
    //            continue;
    //        }
    //        if (!unparsedMap.ContainsKey(item.Key))
    //        {
    //            unparsedMap.Add(item.Key, (string)item.Value);
    //        }
    //        else
    //        {
    //            unparsedMap[item.Key] = (string)item.Value;
    //        }
    //    }
    //}

    //public static string GetVipWardConfigParameter(string parameter, out bool isFallback)
    //{
    //    isFallback = false;
    //    string config = string.Empty;
    //    unparsedMap.TryGetValue(parameter, out config);
    //    if (string.IsNullOrEmpty(config))
    //    {
    //        isFallback = true;
    //        config = ResourcesHolder.Get().VipWardConfigFallback.GetConfig(parameter);
    //    }
    //    return config;
    //}
}
