using UnityEngine;

public class UpgradeVIPPopupController : BaseUIController<UpgradeVIPPopupData, UpgradeVIPPopupUI>
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

    public RectTransform GetFirstInactiveBookmark()
    {
        return GetPopupUI().GetFirstInactiveBookmark();
    }
}
