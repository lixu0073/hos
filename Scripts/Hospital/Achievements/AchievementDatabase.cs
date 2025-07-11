using UnityEngine;
using System.Collections.Generic;


public class AchievementDatabase : ScriptableObject
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/Achievements/AchievementDatabase")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<AchievementDatabase>();
    }
#endif

    public List<AchievementInfo> AchievementItem;// = new Dictionary<string, AchievementInfo>();

}
