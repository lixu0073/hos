using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedOffersScreenViewStrategy
{
    public abstract void SetupPopupAccordingToData(TimedOffersScreenData screenData, TimedOffersScreenUI screenUI);

    protected void ClearScreen(TimedOffersScreenUI screenUI)
    {
        screenUI.SetTimerActive(false);
    }
}
