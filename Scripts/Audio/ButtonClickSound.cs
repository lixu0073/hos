using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonClickSound : MonoBehaviour
{
    public bool click2 = false;
    public bool clickInactive = false;
    public GameObject customAudio;
    public ClickEvent clickEvent;

    Button btn;
    EventTrigger eventTrigger;
    Toggle tgl;

    void Awake()
    {
        ReassignSound();
    }

    public void ReassignSound()
    {
        if (clickEvent == ClickEvent.Button)
        {
            btn = GetComponent<Button>();
            if (btn == null)
                return;
            btn.onClick.AddListener(() =>
            {
                ClickSound();
            });
        }
        else if (clickEvent == ClickEvent.TriggerPointerDown)
        {
            eventTrigger = GetComponent<EventTrigger>();
            if (eventTrigger == null)
                return;
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) =>
            {
                ClickSound();
            });
            eventTrigger.triggers.Add(entry);
        }
        else if (clickEvent == ClickEvent.TriggerPointerUp)
        {
            eventTrigger = GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((eventData) =>
            {
                ClickSound();
            });
            eventTrigger.triggers.Add(entry);
        }
        else if (clickEvent == ClickEvent.Toggle)
        {
            tgl = GetComponent<Toggle>();
            if (tgl == null)
                return;
            tgl.onValueChanged.AddListener((value) =>
            {
                ClickSound();
            });
        }
    }

    public void ClickSound()
    {
        if (SoundsController.Instance == null)
            return;

        if (clickInactive && customAudio == null)
        {
            SoundsController.Instance.PlayButtonClickInactive(false);
            return;
        }

        if (click2 && customAudio == null)
        {
            SoundsController.Instance.PlayButtonClick2();
        }
        else if (customAudio != null)
        {
            if (SoundsController.Instance.IsSoundEnabled())
            {
                Instantiate(customAudio);
            }
        }
        else
        {
            SoundsController.Instance.PlayButtonClick(false);
        }
    }
}

public enum ClickEvent
{
    Button,
    TriggerPointerDown,
    TriggerPointerUp,
    Toggle
}