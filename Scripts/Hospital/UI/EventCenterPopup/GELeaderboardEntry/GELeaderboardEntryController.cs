public class GELeaderboardEntryController : BaseUIController<GELeaderboardEntryData, GELeaderboardEntryUI>
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