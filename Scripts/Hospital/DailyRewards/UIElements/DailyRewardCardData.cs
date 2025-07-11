using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DailyRewardCardData
{
    public BaseUIElementStrategy<DailyRewardCardData, DailyRewardCardLayout> cardStrategy;
    public int dayRespresentation = 0;
    public int currentDayNumber = 0;
    public bool rewardClaimed;
    public int AmountOfRewardToWin;
    public Sprite GiftIcon;
    public Sprite MainImageGiftRepresentation;
    public string PackageLocalizedString;
    public UnityAction onCardClick;
}
