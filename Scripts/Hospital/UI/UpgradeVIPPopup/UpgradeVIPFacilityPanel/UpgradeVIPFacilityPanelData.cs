using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class UpgradeVIPFacilityPanelData
{
    public UpgradeVIPFacilityPanelViewStrategy strategy = null;
    public int requiredVIPs = 0;
    public int currentTime = 0;
    public int currentBonusTime = 0;
    public int upgradedBonusTime = 0;
    public int currentUpgradeLevel = 0;
    public UnityAction onUpgradeButtonClicked = null;
}
