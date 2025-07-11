public class MaxedoutUpgradeVIPPopupViewStrategy : UpgradeVIPPopupViewStrategy
{
    public override void SetupPopupAccordingToData(UpgradeVIPPopupData data, UpgradeVIPPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetTitle(data.titleTerm);
        uiElement.SetSingleUpgradePanels(data.upgradePanelsData);
        uiElement.SetCloseButton(data.onCloseButtonClick);
        uiElement.SetBookmarks(data.bookmarkActions);
        uiElement.SetScrollToPosition(data.toScroll);

        uiElement.SetMaxLevelLabelActive(true);
    }
}
