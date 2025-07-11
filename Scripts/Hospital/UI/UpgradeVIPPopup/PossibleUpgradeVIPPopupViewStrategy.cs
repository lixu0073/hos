public class PossibleUpgradeVIPPopupViewStrategy : UpgradeVIPPopupViewStrategy
{
    public override void SetupPopupAccordingToData(UpgradeVIPPopupData data, UpgradeVIPPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetTitle(data.titleTerm);
        uiElement.SetCloseButton(data.onCloseButtonClick);
        uiElement.SetBookmarks(data.bookmarkActions);
        uiElement.SetSingleUpgradePanels(data.upgradePanelsData);
        uiElement.SetRequiredToolsPanel(data.toolsData);
        uiElement.SetVipsCounter(data.currentCuredVipsCount, data.requiredCuredVipsCount);
        uiElement.SetUpgradeButton(data.onUpgradeButtonClick);
        uiElement.SetSpeedupButton(data.onSpeedupButtonClick, data.speedupCost);
        uiElement.SetScrollToPosition(data.toScroll);

        uiElement.SetUpgradeButtonActive(data.onUpgradeButtonClick != null);
        uiElement.SetSpeedupButtonActive(data.onSpeedupButtonClick != null);
        uiElement.SetUpgradeButtonBadgeActive(data.isUpgradeBadgeVisible);

        uiElement.SetRequirementsLabelActive(true);
        uiElement.SetSeparatorActive(true);
        uiElement.SetRequiredVIPCounterActive(true);
        uiElement.SetVipsCounterTextActive(true);
        uiElement.SetRequiredToolsPanelActive(true);
    }
}
