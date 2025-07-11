public class NoEventEventCenterPopupViewStrategy : EventCenterPopupViewStrategy
{
    public override void SetupPopupAccordingToData(EventCenterPopupData data, EventCenterPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetTitle(data.titleTerm);
        uiElement.SetCloseButton(data.onCloseButtonClick);
        uiElement.SetBookmarks(data.bookmarksActions, data.isBookmarkInteractable);

        uiElement.SetNoEventPanelActive(true);
    }
}
