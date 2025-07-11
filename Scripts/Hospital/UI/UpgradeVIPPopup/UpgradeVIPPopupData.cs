using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class UpgradeVIPPopupData
{
    public UpgradeVIPPopupViewStrategy strategy = null;

    public bool isUpgradeBadgeVisible = false;
    public UpgradeToolPanelData[] toolsData = null;
    public UnityAction onCloseButtonClick = null;
    public UnityAction onUpgradeButtonClick = null;
    public UnityAction onSpeedupButtonClick = null;
    public int speedupCost = 0;
    public UnityAction[] bookmarkActions = null;
    public string titleTerm = null;
    public SingleUpgradePanelData[] upgradePanelsData = null;
    public int currentCuredVipsCount = 0;
    public int requiredCuredVipsCount = 0;
    public int toScroll = 0;
}
