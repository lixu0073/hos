using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventComingSoonPopupViewStrategy
{
    public void SetupPopupAccordingToData(EventComingSoonPopupData data, NextEventPopup uiElement)
    {
        uiElement.SetNextEventTime(data.secondsToEvent);
        uiElement.SetContributionImage(data.contributionItem);
        uiElement.SetOkButton(data.onOkButtonClick);
        uiElement.SetCloseButton(data.onCloseButtonClick);
    }
}
