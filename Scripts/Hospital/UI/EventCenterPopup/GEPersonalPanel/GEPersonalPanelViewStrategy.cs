public abstract class GEPersonalPanelViewStrategy
{
    public abstract void SetupPopupAccordingToData(GEPersonalPanelData data, GEPersonalPanelUI uiElement);

    public void ClearPanel(GEPersonalPanelUI uiElement)
    {
        uiElement.SetLockedGEPanelActive(false);
        uiElement.SetProgressTooltipActive(false);
    }
}
