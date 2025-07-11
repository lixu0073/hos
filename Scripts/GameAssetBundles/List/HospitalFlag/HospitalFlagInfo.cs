using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HospitalFlagInfo : ScriptableObject
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/HospitalFlagInfo")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<HospitalFlagInfo>();
    }

#endif

    public string flagName;
    public int unlockLevel;
    public CustomizableHospitalFlagDatabase.FlagType type;
    public int cost;
    public ResourceType currencyType;
    public int assetbundleVersion;
    public string route;
    public string miniature;
    public Sprite ingameTexture;
    public string flagNameKey;
    public int exp;
}