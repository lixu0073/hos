public class UnavailableUpgradeVIPFacilityPanelViewStrategy : UpgradeVIPFacilityPanelViewStrategy
{
    public override void SetupPopupAccordingToData(UpgradeVIPFacilityPanelData data, UpgradeVIPFacilityPanelUI uiElement)
    {
        ClearView(uiElement);

        uiElement.SetTimeText(data.currentTime);
        uiElement.SetCurrentBonusText(data.currentBonusTime);
        uiElement.SetUpgradeBonusText(data.upgradedBonusTime);
        uiElement.SetUpgradeButton(data.onUpgradeButtonClicked, true);
        uiElement.SetUpgradePresentation(data.currentUpgradeLevel);
        uiElement.SetRequiredPatientsCounterText(data.requiredVIPs);

        uiElement.SeRequiredPatientsLabelActive(true);
        uiElement.SetRequiredPatientsCounterTextActive(true);
        uiElement.SetUpgradeToGetLabelActive(true);
        uiElement.SetBonusPanelActive(true);
        uiElement.SetUpgradeButtonActive(true);
    }
}
