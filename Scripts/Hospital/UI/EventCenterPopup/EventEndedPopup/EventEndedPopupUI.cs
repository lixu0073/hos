using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SimpleUI;
using TMPro;
using I2.Loc;

public class EventEndedPopupUI : UIElement
{
    [TermsPopup]
    [SerializeField]
    private string yourPositionTerm = "-";

    [SerializeField]
    private TextMeshProUGUI playerScore = null;
    [SerializeField]
    private TextMeshProUGUI playerPosition = null;
    [SerializeField]
    private TextMeshProUGUI rewardAmountText = null;

    [SerializeField]
    private Image prize = null;
    [SerializeField]
    private ButtonUI claimButton = null;

    public void SetRewardAmountActive(bool setActive)
    {
        rewardAmountText.gameObject.SetActive(setActive);
    }

    public void SetRewardAmount(int amount)
    {
        rewardAmountText.text = string.Format("x{0}", amount);
    }

    public void SetPlayerScore(int score)
    {
        playerScore.text = score.ToString();
    }

    public void SetPlayerPosition(int position)
    {
        playerPosition.text = string.Format("{0} {1}", I2.Loc.ScriptLocalization.Get(yourPositionTerm), position.ToString());
    }

    public void SetPrizeImage(Sprite prizeSprite)
    {
        prize.sprite = prizeSprite;
    }

    public void SetClaimButton(UnityAction onClick)
    {
        claimButton.SetButton(onClick);
    }
}
