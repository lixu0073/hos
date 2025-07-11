using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiseGiftController
{

    int AddWiseGiftIntervalInHours
    {
        get { return DefaultConfigurationProvider.GetConfigCData().AddWiseGiftIntervalInHours; }
    }
    private static int SECONDS_IN_HOUR = 3600;

    public bool ShouldAddGiftFromWise()
    {
        if (!TutorialController.Instance.IsTutorialStepCompleted(StepTag.wise_thank_you))
            return false;

        long Time = GiftsSynchronizer.Instance.GetLastBuyOrLastRefreshWithSomeGiftsTime();
        bool HasNoGifts = GiftsAPI.Instance.GetGifts().Count == 0;
        if(HasNoGifts)
        {
            if (Time == -1)
            {
                return true;
            }
            else
            {
                return (long)ServerTime.getTime() > Time + AddWiseGiftIntervalInHours * SECONDS_IN_HOUR;
            }
        }
        return false;
    }

    public void UpdateLastBuyOrLastRefreshWithSomeGiftsTime()
    {
        GiftsSynchronizer.Instance.GetData().LastBuyOrLastRefreshWithSomeGiftsTime = (long)ServerTime.getTime();
    }

}
