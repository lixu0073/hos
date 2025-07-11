using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GlobalEventParser : MonoBehaviour
{

    GlobalEventDatabase globalEventDatabase = new GlobalEventDatabase();
    GlobalEventData currentGlobalEventDataConfig = null;
    GlobalEventData previousGlobalEventDataConfig = null;
    GlobalEventData nextGlobalEventDataConfig = null;

    public static GlobalEventParser Instance;

    public GlobalEventData CurrentGlobalEventConfig
    {
        private set { }
        get { return currentGlobalEventDataConfig; }
    }

    public GlobalEventData PreviousGlobalEventConfig
    {
        private set { }
        get { return previousGlobalEventDataConfig; }
    }

    public GlobalEventData NextGlobalEventConfig
    {
        private set { }
        get { return nextGlobalEventDataConfig; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            Debug.LogError("EVENT PARSER AVAILABLE");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Load(GlobalEventsCData globalEventsModel)
    {
        Debug.LogError("Loading global events, remove this log if they load");
        SetNewEvents(globalEventsModel);
    }

    public List<GlobalEventData> GetAllGlobalEvents()
    {
        return globalEventDatabase.GetAllEvents();
    }

    public bool CanUpdateGlobalEventConfig()
    {
        bool eventPassed = EventHasPassed(currentGlobalEventDataConfig);

        if (eventPassed)
        {
            UIController.getHospital.EventCenterPopup.GetComponent<EventCenterPopupInitializer>().DeInitialize();
            UIController.getHospital.EventCenterPopup.GetPopup().Exit();
        }

        if (eventPassed || EventHasStarted(nextGlobalEventDataConfig))
            return true;

        return false;
    }

    public void SetNewEvents(GlobalEventsCData globalEventsModel)
    {
        currentGlobalEventDataConfig = globalEventDatabase.GetCurrentGlobalEvent(globalEventsModel);
        previousGlobalEventDataConfig = globalEventDatabase.GetPreviousGlobalEvent(globalEventsModel);
        nextGlobalEventDataConfig = globalEventDatabase.GetNextGlobalEvent(globalEventsModel);

        if (GlobalEventParser.Instance != null && GlobalEventParser.Instance.CurrentGlobalEventConfig != null)
        {
            CollectOnMapGEGraphicsManager.SetGlobalEventType(GlobalEventParser.Instance.CurrentGlobalEventConfig.Type);
        }
    }

    private bool EventHasPassed(GlobalEventData _event)
    {
        if (_event != null)
        {
            double currentTime = ServerTime.getTime();

            return currentTime >= _event.GlobalEventEndTime;
        }

        return false;
    }

    private bool EventHasStarted(GlobalEventData _event)
    {
        if (_event != null)
        {
            double currentTime = ServerTime.getTime();

            return currentTime >= _event.GlobalEventStartTime;
        }

        return false;
    }
}
