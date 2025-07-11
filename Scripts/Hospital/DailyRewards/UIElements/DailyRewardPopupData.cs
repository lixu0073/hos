using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DailyRewardPopupData
{
    public BaseUIElementStrategy<DailyRewardPopupData, DailyRewardPopup> strategy;
    public List<DailyRewardCardData> cardData;
    public UnityAction onClaimButtonRewardClick;
    public bool GrayOutClaimButton;
    public string characterResourcePath;
    public AnimatorMonitor animatorMonitor;

    public DailyRewardPopupData()
    {
        cardData = new List<DailyRewardCardData>();
    }
}
