using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTimedOfferScreenViewStrategy : TimedOffersScreenViewStrategy
{
    public override void SetupPopupAccordingToData(TimedOffersScreenData screenData, TimedOffersScreenUI screenUI)
    {
        ClearScreen(screenUI);

        //screenUI.SetCloseButton(screenData.onCloseButtonClick);
        screenUI.SetPreviousOfferButton(screenData.onPreviousOfferButtonClick);
        screenUI.SetNextOfferButton(screenData.onNextOfferButtonClick);
        screenUI.SetPaginationIndicator(screenData.currentPageId, screenData.totalPagesCount);

        screenUI.SetPreviousOfferButtonActive(screenData.onPreviousOfferButtonClick != null);
        screenUI.SetNextOfferButtonActive(screenData.onNextOfferButtonClick != null);
    }
}
