using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventGenerator : MonoBehaviour {

    public GlobalEvent GetCurrentGlobalEvent()
    {
        System.Object obj = null;

        if (GlobalEventParser.Instance == null)
        {
            Debug.LogError("Can't start global events 'cuz you don't start game from loading screen");
            return null;
        }

        var currentGlobalEventData = GlobalEventParser.Instance.CurrentGlobalEventConfig;

        if (currentGlobalEventData != null)
        {
            Type type = Type.GetType(currentGlobalEventData.Type.ToString());
            obj = Activator.CreateInstance(type);

            // Check is properly initialized
            if (!(obj as GlobalEvent).Init(currentGlobalEventData))
                return null;

        }

        return obj==null ? null: (obj as GlobalEvent);
    }

    public GlobalEvent GetGlobalEventFromConfig(GlobalEventData eventData)
    {
        System.Object obj = null;

        if (GlobalEventParser.Instance == null)
        {
            Debug.LogError("Can't start global events 'cuz you don't start game from loading screen");
            return null;
        }

        if (eventData != null)
        {
            Type type = Type.GetType(eventData.Type.ToString());
            obj = Activator.CreateInstance(type);

            // Check is properly initialized
            if (!(obj as GlobalEvent).Init(eventData))
                return null;

        }

        return obj == null ? null : (obj as GlobalEvent);
    }

    public PreviousGlobalEvent GetPreviousGlobalEvent()
    {
        var previousGlobalEventData = GlobalEventParser.Instance.PreviousGlobalEventConfig;

        if (previousGlobalEventData != null)
            return new PreviousGlobalEvent(previousGlobalEventData);
        return null;
    }

}
