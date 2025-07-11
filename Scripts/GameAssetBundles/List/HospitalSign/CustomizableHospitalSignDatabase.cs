using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizableHospitalSignDatabase : ScriptableObject
{

    public List<HospitalSignInfo> signs;

    public HospitalSignInfo GetSignInfo(string signName)
    {
        foreach(HospitalSignInfo info in signs)
        {
            if (string.Compare(info.signName, signName) == 0)
                return info;
        }
        return null;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/CustomizableHospitalSignDatabase")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CustomizableHospitalSignDatabase>();
    }
    public void SetSignExp()
    {
        for (int i = 0; i < signs.Count; ++i)
        {
            signs[i].exp = (int)(Mathf.FloorToInt(signs[i].cost / (float)5));
        }
    }
#endif

    public enum SignIDBackup
    {
        Acient,
        Arizona,
        Balloon,
        Chocolate,
        Classic1,
        Classic2,
        Country,
        Floral,
        Modern,
        Tropical,
        Lvl1,
        Lvl2,
        Lvl3,
        Lvl4,
        Lvl5,
        Lvl6,
        Lvl7,
        Lvl8,
        Lvl9,
        Lvl10,
        Lvl11,
        Lvl12
    }

    public enum SignType
    {
        Default,
        Free,
        Premium
    }
}
