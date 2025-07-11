using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEventParser : MonoBehaviour {

    public static GameEventParser Instance;

    Dictionary<GameEventType, GameEventData> gameEventsConfigData = null;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadGameEventConfigData(Dictionary<string, string> config)
    {
        if (gameEventsConfigData != null)
        {
            gameEventsConfigData.Clear();
        }

        gameEventsConfigData = new Dictionary<GameEventType, GameEventData>();

        if (config != null && config.Count > 0)
        {
                foreach (KeyValuePair<string, string> entry in config)
                {
                    try
                    {
                        GameEventData gameEventData = GameEventData.Parse(entry.Value);
                        GameEventType gameEventType = (GameEventType)Enum.Parse(typeof(GameEventType), entry.Key);
                        gameEventsConfigData.Add(gameEventType, gameEventData);
                    }
                    catch (Exception e)
                    {
                        AnalyticsController.instance.ReportException("gameEvent.config_failed", e);
                        Debug.LogError(e.Message);
                    }
            }
        }
    }

    public DateTime GetGameEventEndTime(GameEventType key)
    {
        if (gameEventsConfigData == null)
        {
            //if (BundleManager.Instance == null || BundleManager.Instance.config == null)
            //    LoadGameEventConfigData(null);
            //else 
            LoadGameEventConfigData(DefaultConfigurationProvider.GetConfigCData().GameEvents);
        }

        if (gameEventsConfigData.ContainsKey(key) && gameEventsConfigData[key] != null)
            return gameEventsConfigData[key].EndTime;

        return DateTime.MinValue;
    }


    public DateTime GetGameEventStartTime(GameEventType key)
    {
        if (gameEventsConfigData == null)
        {
            //if (BundleManager.Instance == null || BundleManager.Instance.config == null)
            //    LoadGameEventConfigData(null);
            //else 
            LoadGameEventConfigData(DefaultConfigurationProvider.GetConfigCData().GameEvents);

        }

        if (gameEventsConfigData.ContainsKey(key) && gameEventsConfigData[key] != null)
            return gameEventsConfigData[key].StartTime;

        return DateTime.MinValue;
    }

}
