public class StandardEventPanelViewStrategy
{
    public void SetupPopupAccordingToData(StandardEventPanelData data, StandardEventPanelUI uiElement)
    {
        uiElement.SetTimeLeftText(data.timeLeft);
        uiElement.SetEventArt(data.artSprite);
        uiElement.SetInfoButton(data.onInfoButtonClick);
        uiElement.SetInfoPanel(data.infoPanelData);
    }
}
