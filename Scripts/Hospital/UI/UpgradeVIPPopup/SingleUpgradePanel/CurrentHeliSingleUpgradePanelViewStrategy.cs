public class CurrentHeliSingleUpgradePanelViewStrategy : SingleUpgradePanelViewStrategy
{
    public override void SetupPopupAccordingToData(SingleUpgradePanelData data, SingleUpgradePanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetHelipadUpgradeView(data.masterableLevel);
        uiElement.SetTimerLabel(data.timerLabelTerm);
        uiElement.SetTimerText(data.time);
        uiElement.SetBadgeIconActive(data.setBadgeActive);

        uiElement.SetPanelBg(false);
        uiElement.SetGrayscaleImages(false);

        uiElement.SetTimerIconActive(true);
        uiElement.SetHelipadUpgradeViewActive(true);
        uiElement.SetTimerLabelActive(true);
        uiElement.SetTimerTextActive(true);
    }
}
