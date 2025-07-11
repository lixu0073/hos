public abstract class GEContributionPanelViewStrategy
{
    public abstract void SetupPopupAccordingToData(GEContributionPanelData data, GEContributionPanelUI uiElement);

    public void ClearPanel(GEContributionPanelUI uiElement)
    {
        uiElement.SetContributeBoxActive(false);
        uiElement.SetRoomImageActive(false);
        uiElement.SetMedicineImageActive(false);
        uiElement.SetMinusButtonActive(false);
        uiElement.SetPlusButtonActive(false);
        uiElement.SetContributeButtonActive(false);
        uiElement.SetContributionCounterTextActive(false);
    }
}
