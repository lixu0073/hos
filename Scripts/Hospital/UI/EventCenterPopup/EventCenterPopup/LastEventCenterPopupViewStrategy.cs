public class LastEventCenterPopupViewStrategy : EventCenterPopupViewStrategy
{
    public override void SetupPopupAccordingToData(EventCenterPopupData data, EventCenterPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetTitle(data.titleTerm);
        uiElement.SetCloseButton(data.onCloseButtonClick);
        uiElement.SetBookmarks(data.bookmarksActions, data.isBookmarkInteractable);

        uiElement.SetLastEventPanel(data.lastEventPanelData);

        uiElement.SetLastEventPanelActive(true);
    }
}
