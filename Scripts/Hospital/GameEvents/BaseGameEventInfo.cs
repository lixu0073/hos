using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

public class BaseGameEventInfo : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/BaseGameEventInfo")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<BaseGameEventInfo>();
    }
#endif

    public GameEventType type;

    public string startTimeString;
    public string endTimeString;
    public string eventTitle;
    public string popupContentPath;

#pragma warning disable 0649
    [Header("Treasure Chests")]
    [SerializeField] private bool isChestsEnabled;
    [SerializeField] private string ChestResourcePath;
    [SerializeField] private int MaxNumberOfChests;
    [SerializeField] private int ChestInterval;

    [Header("Treasure Chests Coin Reward")]
    [SerializeField] private int MinCoinValue;
    [SerializeField] private int MaxCoinValue;

    [Header("DeltaDNA Popup")]
    [SerializeField] private bool isDeltaDNAPopup;
#pragma warning restore 0649
    [SerializeField] public DecisionPoint deltaDNAPopupDecisionPoint;

    public int GetCoinReward()
    {
        int reward = 10;
        if (MinCoinValue == 0 || MaxCoinValue == 0 || (MinCoinValue> MaxCoinValue))
        {
            Debug.LogError("Invalid Chest Coin Reward Setup");
            return reward;
        }
        int diff = MaxCoinValue - MinCoinValue;
        diff = diff / 3;
        diff = UnityEngine.Random.Range(0, diff + 1);
        return MinCoinValue + diff * 5;
    }

    public int GetChestInterval()
    {
        return ChestInterval;
    }

    public int GetMaxNumberOfChests()
    {
        return MaxNumberOfChests;
    }

    public bool IsChestsEnabled()
    {
        return isChestsEnabled;
    }

    public bool IsDeltaDNAPopupEnabled()
    {
        return isDeltaDNAPopup;
    }

    public string GetChestResoursePath()
    {
        return ChestResourcePath;
    }

    public bool IsActive()
    {
        DateTime now = DateTime.UtcNow;
        DateTime endTime = GetEndTime();
        return now > GetStartTime() && now < GetEndTime();
    }

    public long GetTimeToEnd()
    {
        return (long)((GetEndTime() - DateTime.UtcNow).TotalSeconds);
    }

    public DateTime GetStartTime()
    {
        DateTime time = GameEventParser.Instance.GetGameEventStartTime(type);

        if (time != DateTime.MinValue)
            return time;
            
        return DateTime.ParseExact(startTimeString + " UTC", "yyyy-MM-dd HH:mm:ss UTC", System.Globalization.CultureInfo.InvariantCulture);
    }

    public DateTime GetEndTime()
    {
        DateTime time = GameEventParser.Instance.GetGameEventEndTime(type);

        if (time != DateTime.MinValue)
            return time;

        return DateTime.ParseExact(endTimeString + " UTC", "yyyy-MM-dd HH:mm:ss UTC", System.Globalization.CultureInfo.InvariantCulture);
    }
}

public enum GameEventType
{
    Valentine,
    Easter,
    LabourDay,
    CancerDay
};
