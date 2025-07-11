public abstract class SingleUpgradePanelViewStrategy
{
    public abstract void SetupPopupAccordingToData(SingleUpgradePanelData data, SingleUpgradePanelUI uiElement);

    public void ClearPanel(SingleUpgradePanelUI uiElement)
    {
        uiElement.SetTimerIconActive(false);
        uiElement.SetBadgeIconActive(false);
        uiElement.SetLockedLabelActive(false);
        uiElement.SetLockedIconActive(false);
        uiElement.SetCheckmarkIconActive(false);
        uiElement.SetHelipadUpgradeViewActive(false);
        uiElement.SetWardUpgradeViewActive(false);
        uiElement.SetTimerLabelActive(false);
        uiElement.SetTimerTextActive(false);
    }
}
