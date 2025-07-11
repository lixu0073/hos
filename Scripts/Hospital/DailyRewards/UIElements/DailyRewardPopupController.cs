public class DailyRewardPopupController : BaseUIController<DailyRewardPopupData, DailyRewardPopup>
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
        GetPopup().GetDailyCardLayout().Initialize(data.cardData);
    }

    protected override void OnViewDeInitialize()
    {
        GetPopup().Deinitialize();
    }
}
