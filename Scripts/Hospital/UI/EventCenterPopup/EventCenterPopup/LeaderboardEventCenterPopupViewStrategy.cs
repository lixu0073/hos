public class LeaderboardEventCenterPopupViewStrategy : EventCenterPopupViewStrategy
{
    public override void SetupPopupAccordingToData(EventCenterPopupData data, EventCenterPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetTitle(data.titleTerm);
        uiElement.SetCloseButton(data.onCloseButtonClick);
        uiElement.SetBookmarks(data.bookmarksActions, data.isBookmarkInteractable);

        uiElement.SetLeaderboardPanel(data.leaderboardPanelData);

        uiElement.SetLeaderboardPanelActive(true);
    }
}
