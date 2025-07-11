using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HospitalSignInfo : ScriptableObject
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/HospitalSignInfo")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<HospitalSignInfo>();
    }

#endif
    
    public string signName;
    public int unlockLevel;
    public CustomizableHospitalSignDatabase.SignType type;
    public int cost;
    public ResourceType currencyType;
    public int assetbundleVersion;
    public string route;
    public Sprite miniature;
    public string signNameKey;
    public int exp; 
}