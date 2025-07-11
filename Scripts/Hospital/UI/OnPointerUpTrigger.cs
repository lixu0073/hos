using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

[AddComponentMenu("Event/OnPointerUp Trigger")]
public class OnPointerUpTrigger : MonoBehaviour, IPointerUpHandler
{
    [SerializeField]
    private List<Entry> delegates;

    public List<Entry> triggers
    {
        get
        {
            if (delegates == null)
                delegates = new List<Entry>();
            return delegates;
        }
        set
        {
            delegates = value;
        }
    }

    private void Execute(BaseEventData eventData)
    {
        for (int i = 0; i < triggers.Count; ++i)
        {
            Entry trigger = triggers[i];
            trigger.callback?.Invoke(eventData);
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Execute(eventData);
    }

    [Serializable]
    public class Entry
    {
        public EventTrigger.TriggerEvent callback = new EventTrigger.TriggerEvent();
    }
}
