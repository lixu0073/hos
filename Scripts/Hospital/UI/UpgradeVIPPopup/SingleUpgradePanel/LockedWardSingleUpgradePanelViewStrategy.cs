public class LockedWardSingleUpgradePanelViewStrategy : SingleUpgradePanelViewStrategy
{
    public override void SetupPopupAccordingToData(SingleUpgradePanelData data, SingleUpgradePanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetWardUpgradeView(data.masterableLevel);

        uiElement.SetPanelBg(false);
        uiElement.SetGrayscaleImages(true);

        uiElement.SetWardUpgradeViewActive(true);
        uiElement.SetLockedIconActive(true);
        uiElement.SetLockedLabelActive(true);
    }
}
