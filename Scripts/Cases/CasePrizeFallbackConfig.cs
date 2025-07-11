using System;
using System.Collections.Generic;
using UnityEngine;

public class CasePrizeFallbackConfig : ScriptableObject
{
#pragma warning disable 0649
    [SerializeField] private List<CasePrizeFallBackData> prizesFallback;
#pragma warning restore 0649

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/CasePrizeFallbackConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CasePrizeFallbackConfig>();
    }
#endif

    public string GetPrizeFallbackForTier(int tier)
    {
        foreach (CasePrizeFallBackData item in prizesFallback)
        {
            if (item.Tier == tier)
                return item.rewardConfig;
        }
        return String.Empty;
    }
}

[Serializable]
public struct CasePrizeFallBackData
{
    public int Tier;
    public string rewardConfig;
}
