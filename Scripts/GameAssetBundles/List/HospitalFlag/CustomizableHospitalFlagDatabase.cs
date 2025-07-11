using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizableHospitalFlagDatabase : ScriptableObject
{

    public List<HospitalFlagInfo> flags;

    public HospitalFlagInfo GetFlagInfo(string flagName)
    {
        foreach(HospitalFlagInfo info in flags)
        {
            if (string.Compare(info.flagName, flagName) == 0)
                return info;
        }
        return null;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/CustomizableHospitalFlagDatabase")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CustomizableHospitalFlagDatabase>();
    }

    public void SetFlagsExp() {
        for (int i = 0; i < flags.Count; ++i)
        {
            flags[i].exp = (int)(Mathf.FloorToInt(flags[i].cost / (float)5));
        }
    }
#endif
    public enum FlagType
    {
        Default,
        Free,
        Premium
    }
}
