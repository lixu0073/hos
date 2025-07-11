using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventComingSoonPopupData
{
    public EventComingSoonPopupData strategy = null;

    public int secondsToEvent = 0;
    public Sprite contributionItem = null;
    public UnityAction onOkButtonClick = null;
    public UnityAction onCloseButtonClick = null;
}
