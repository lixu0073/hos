using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradeVIPFacilityPanelViewStrategy
{
    public abstract void SetupPopupAccordingToData(UpgradeVIPFacilityPanelData data, UpgradeVIPFacilityPanelUI uiElement);

    public void ClearView(UpgradeVIPFacilityPanelUI uiElement)
    {
        uiElement.SetReadyForUpgradeLabelActive(false);
        uiElement.SeRequiredPatientsLabelActive(false);
        uiElement.SetUpgradeToGetLabelActive(false);
        uiElement.SetBonusPanelActive(false);
        uiElement.SetUpgradeMaxedLabelActive(false);
        uiElement.SetRequiredPatientsCounterTextActive(false);
        uiElement.SetUpgradeButtonActive(false);
    }
}
