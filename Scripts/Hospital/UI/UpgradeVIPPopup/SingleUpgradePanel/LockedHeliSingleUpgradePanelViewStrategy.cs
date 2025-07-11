public class LockedHeliSingleUpgradePanelViewStrategy : SingleUpgradePanelViewStrategy
{
    public override void SetupPopupAccordingToData(SingleUpgradePanelData data, SingleUpgradePanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetHelipadUpgradeView(data.masterableLevel);

        uiElement.SetPanelBg(false);
        uiElement.SetGrayscaleImages(true);

        uiElement.SetHelipadUpgradeViewActive(true);
        uiElement.SetLockedIconActive(true);
        uiElement.SetLockedLabelActive(true);
    }
}
