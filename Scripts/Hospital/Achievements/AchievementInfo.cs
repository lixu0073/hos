using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;

public class AchievementInfo : ScriptableObject
{
    public string achievementID;

    public string titleString;
    public string questString;

    public bool timeControlled;

	public AchievementType achievementType = AchievementType.Standard;
	public enum AchievementType
	{
		Upgrade,
		Standard,
		StandardIncremental
	}

    public List<int> requiredValues;
    public List<int> starRewards;
    public List<int> diamondRewards;
    public List<int> requiredTimes;

    public List<string> gameCenterIds;
    public List<string> googlePlayIds;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/Achievements/AchievementInfo")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<AchievementInfo>();
    }
#endif

}
