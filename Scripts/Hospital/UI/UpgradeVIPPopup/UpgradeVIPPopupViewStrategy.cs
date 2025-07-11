public abstract class UpgradeVIPPopupViewStrategy
{
    public abstract void SetupPopupAccordingToData(UpgradeVIPPopupData data, UpgradeVIPPopupUI uiElement);

    public void ClearPanel(UpgradeVIPPopupUI uiElement)
    {
        uiElement.SetRequirementsLabelActive(false);
        uiElement.SetSeparatorActive(false);
        uiElement.SetMaxLevelLabelActive(false);
        uiElement.SetRequiredVIPCounterActive(false);
        uiElement.SetVipsCounterTextActive(false);
        uiElement.SetUpgradeButtonBadgeActive(false);
        uiElement.SetRequiredToolsPanelActive(false);
        uiElement.SetUpgradeButtonActive(false);
        uiElement.SetSpeedupButtonActive(false);
    }
}
