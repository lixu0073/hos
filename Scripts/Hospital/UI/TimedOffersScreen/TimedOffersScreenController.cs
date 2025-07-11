using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedOffersScreenController : BaseUIController<TimedOffersScreenData, TimedOffersScreenUI>
{
    protected override void OnViewInitialize()
    {
        ApplyData();
    }

    protected override void OnRefreshDataWhileOpened()
    {
        ApplyData();
    }

    private void ApplyData()
    {
        data.strategy.SetupPopupAccordingToData(data, GetPopup());
    }

    protected override void OnViewDeInitialize()
    {
        base.DeInitialize();
    }
}
