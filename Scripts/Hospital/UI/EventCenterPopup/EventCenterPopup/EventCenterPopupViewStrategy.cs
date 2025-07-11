public abstract class EventCenterPopupViewStrategy
{
    public abstract void SetupPopupAccordingToData(EventCenterPopupData data, EventCenterPopupUI uiElement);

    public void ClearPanel(EventCenterPopupUI uiElement)
    {
        uiElement.SetNoEventPanelActive(false);
        uiElement.SetPersonalPanelActive(false);
        uiElement.SetLeaderboardPanelActive(false);
        uiElement.SetLastEventPanelActive(false);
        uiElement.SetStandardEventPanelActive(false);
    }
}
