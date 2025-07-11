using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SimpleUI;
using TMPro;

public class NextEventPopup : UIElement
{
    [SerializeField]
    private TextMeshProUGUI nextEventStartTime = null;
    [SerializeField]
    private Image contributionItem = null;
    [SerializeField]
    private ButtonUI okButton = null;
    [SerializeField]
    private ButtonUI closeButton = null;

    public void SetNextEventTime(int timeInSeconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(timeInSeconds);
        nextEventStartTime.text = string.Format("{0}H {1}M", Math.Floor(t.TotalHours), t.Minutes);
    }

    public void SetContributionImage(Sprite contributionSprite)
    {
        contributionItem.sprite = contributionSprite;
    }

    public void SetOkButton(UnityAction onClick)
    {
        okButton.SetButton(onClick);
    }

    public void SetCloseButton(UnityAction onClick)
    {
        closeButton.SetButton(onClick);
    }
}
