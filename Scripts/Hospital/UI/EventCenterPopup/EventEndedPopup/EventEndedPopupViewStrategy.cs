public abstract  class EventEndedPopupViewStrategy
{
    public abstract void SetupPopupAccordingToData(EventEndedPopupData data, EventEndedPopupUI uiElement);

    public void ClearPanel(EventEndedPopupUI uiElement)
    {
        uiElement.SetRewardAmountActive(false);
    }
}
