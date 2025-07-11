using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum LeaderboardPosition
{
    First,
    Second,
    Third,
    Rest
}

public class PlayerEventScore : MonoBehaviour
{
    [SerializeField]
    private GlobalEventContributorUI globalEventContributorUI = null;
    [SerializeField]
    private Image scoreBG = null;
    [SerializeField]
    private Image contributionItem = null;
    [SerializeField]
    private Image prizeImage = null;

    [Space(10)]
    [SerializeField]
    private Sprite firstBg = null;
    [SerializeField]
    private Sprite secondBg = null;
    [SerializeField]
    private Sprite thirdBg = null;
    [SerializeField]
    private Sprite restBg = null;

    [Space(10)]
    [SerializeField]
    private Sprite firstPrize = null;
    [SerializeField]
    private Sprite secondPrize = null;
    [SerializeField]
    private Sprite restPrize = null;

    public void SetContributor(Contributor contributor, LeaderboardPosition leaderboardPosition, UnityAction buttonAction)
    {
        globalEventContributorUI.Setup(contributor, buttonAction, (int)contributor.friendStatus);
        SetSprites(leaderboardPosition);
    }

    public void SetContributor(FriendContributor contributor, LeaderboardPosition leaderboardPosition, UnityAction buttonAction)
    {
        globalEventContributorUI.Setup(contributor.Friend, contributor.Score, buttonAction, (int)contributor.friendStatus);
        SetSprites(leaderboardPosition);
    }

    public void SetSprites(LeaderboardPosition leaderboardPosition)
    {
        switch (leaderboardPosition)
        {
            case LeaderboardPosition.First:
                scoreBG.sprite = firstBg;
                prizeImage.sprite = firstPrize;
                break;
            case LeaderboardPosition.Second:
                scoreBG.sprite = secondBg;
                prizeImage.sprite = secondPrize;
                break;
            case LeaderboardPosition.Third:
                scoreBG.sprite = thirdBg;
                prizeImage.sprite = restPrize;
                break;
            case LeaderboardPosition.Rest:
                scoreBG.sprite = restBg;
                prizeImage.sprite = restPrize;
                break;
            default:
                break;
        }
    }

    public void SetContributionItem(Sprite contributionSprite)
    {
        if (contributionSprite == null)
        {
            contributionItem.gameObject.SetActive(false);
        }
        else
        {
            contributionItem.gameObject.SetActive(true);
            contributionItem.sprite = contributionSprite;
        }          
    }
}
