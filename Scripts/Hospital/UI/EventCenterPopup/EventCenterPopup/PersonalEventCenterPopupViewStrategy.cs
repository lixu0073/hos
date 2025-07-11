public class PersonalEventCenterPopupViewStrategy : EventCenterPopupViewStrategy
{
    public override void SetupPopupAccordingToData(EventCenterPopupData data, EventCenterPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetTitle(data.titleTerm);
        uiElement.SetCloseButton(data.onCloseButtonClick);
        uiElement.SetBookmarks(data.bookmarksActions, data.isBookmarkInteractable);

        uiElement.SetPersonalPanel(data.personalPanelData);

        uiElement.SetPersonalPanelActive(true);
    }
}
