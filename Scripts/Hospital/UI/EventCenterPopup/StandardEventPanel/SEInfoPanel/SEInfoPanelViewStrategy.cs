public class SEInfoPanelViewStrategy
{
    public void SetupPopupAccordingToData(SEInfoPanelData data, SEInfoPanelUI uiElement)
    {
        uiElement.SetInfoLoc(data.infoTerm);
        uiElement.SetSeparatorActive(data.isSeparatorActive);
    }
}
