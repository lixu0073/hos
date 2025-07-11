public class MaxedUpgradeVIPFacilityPanelViewStrategy : UpgradeVIPFacilityPanelViewStrategy
{
    public override void SetupPopupAccordingToData(UpgradeVIPFacilityPanelData data, UpgradeVIPFacilityPanelUI uiElement)
    {
        ClearView(uiElement);

        uiElement.SetTimeText(data.currentTime);
        uiElement.SetUpgradePresentation(data.currentUpgradeLevel);

        uiElement.SetUpgradeMaxedLabelActive(true);
    }
}
