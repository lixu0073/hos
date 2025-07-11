using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTimedOfferScreenViewStrategy : TimedOffersScreenViewStrategy
{
    public override void SetupPopupAccordingToData(TimedOffersScreenData screenData, TimedOffersScreenUI screenUI)
    {
        ClearScreen(screenUI);

        //screenUI.SetTimer(screenData.timeLeft);
        //screenUI.SetCloseButton(screenData.onCloseButtonClick);
        screenUI.SetPreviousOfferButton(screenData.onPreviousOfferButtonClick);
        screenUI.SetNextOfferButton(screenData.onNextOfferButtonClick);
        screenUI.SetPaginationIndicator(screenData.currentPageId, screenData.totalPagesCount);

        screenUI.SetTimerActive(true);
        screenUI.SetPreviousOfferButtonActive(screenData.onPreviousOfferButtonClick != null);
        screenUI.SetNextOfferButtonActive(screenData.onNextOfferButtonClick != null);
    }
}
