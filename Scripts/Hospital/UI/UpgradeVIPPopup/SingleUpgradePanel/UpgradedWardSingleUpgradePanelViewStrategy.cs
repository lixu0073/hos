public class UpgradedWardSingleUpgradePanelViewStrategy : SingleUpgradePanelViewStrategy
{
    public override void SetupPopupAccordingToData(SingleUpgradePanelData data, SingleUpgradePanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetWardUpgradeView(data.masterableLevel);
        uiElement.SetTimerLabel(data.timerLabelTerm);
        uiElement.SetTimerText(data.time);

        uiElement.SetPanelBg(true);
        uiElement.SetGrayscaleImages(false);

        uiElement.SetTimerIconActive(true);
        uiElement.SetCheckmarkIconActive(true);
        uiElement.SetWardUpgradeViewActive(true);
        uiElement.SetTimerLabelActive(true);
        uiElement.SetTimerTextActive(true);
    }
}
