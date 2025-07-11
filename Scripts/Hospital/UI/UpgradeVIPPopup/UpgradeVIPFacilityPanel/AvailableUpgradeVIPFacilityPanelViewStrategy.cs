using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvailableUpgradeVIPFacilityPanelViewStrategy : UpgradeVIPFacilityPanelViewStrategy
{
    public override void SetupPopupAccordingToData(UpgradeVIPFacilityPanelData data, UpgradeVIPFacilityPanelUI uiElement)
    {
        ClearView(uiElement);

        uiElement.SetTimeText(data.currentTime);
        uiElement.SetCurrentBonusText(data.currentBonusTime);
        uiElement.SetUpgradeBonusText(data.upgradedBonusTime);
        uiElement.SetUpgradeButton(data.onUpgradeButtonClicked, false);
        uiElement.SetUpgradePresentation(data.currentUpgradeLevel);

        uiElement.SetReadyForUpgradeLabelActive(true);
        uiElement.SetUpgradeToGetLabelActive(true);
        uiElement.SetBonusPanelActive(true);
        uiElement.SetUpgradeButtonActive(true);
    }
}
