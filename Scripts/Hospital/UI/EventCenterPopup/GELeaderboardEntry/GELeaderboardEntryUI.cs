using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hospital;

public class GELeaderboardEntryUI : MonoBehaviour
{
    [SerializeField]
    private GlobalEventContributorUI globalEventContributorUI = null;

    [SerializeField]
    private Image scoreBG = null;
    [SerializeField]
    private Image contributionItemImage = null;
    [SerializeField]
    private Image prizeImage = null;

    public void SetGlobalEventContributorUI(IFollower contributor, int score, UnityAction buttonAction)
    {
        globalEventContributorUI.Setup(contributor, score, buttonAction, (int)((Contributor)contributor).friendStatus);
    }

    public void SetScoreBg(Sprite bgSprite)
    {
        scoreBG.sprite = bgSprite;
    }

    public void SetContributionItem(Sprite itemSprite)
    {
        contributionItemImage.sprite = itemSprite;
    }

    public void SetPrizeImage(Sprite prizeSprite)
    {
        prizeImage.sprite = prizeSprite;
    }
}
