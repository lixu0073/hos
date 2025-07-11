using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GELastEventPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject rewardObject = null;

    [SerializeField]
    private Image contributionItem = null;

    [SerializeField]
    private TextMeshProUGUI playerScore = null;
    [SerializeField]
    private TextMeshProUGUI rewardAmount = null;

    [SerializeField]
    private Image rewardImage = null;

    [SerializeField]
    private GELeaderboardController leaderboard = null;

    public void SetRewardObjectActive(bool setActive)
    {
        rewardObject.SetActive(setActive);
    }

    public void SetContributionItem(Sprite itemSprite)
    {
        contributionItem.sprite = itemSprite;
    }

    public void SetPlayerScore(int score)
    {
        playerScore.SetText(score.ToString());
    }

    public void SetRewardAmount(int number)
    {
        rewardAmount.text = string.Format("x{0}", number);
    }

    public void SetRewardImage(Sprite sprite)
    {
        rewardImage.sprite = sprite;
    }

    public void SetLeaderboard(GELeaderboardEntryData[] data)
    {
        leaderboard.ClearPanel();

        if (data == null)
        {
            return;
        }

        for (int i = 0; i < data.Length; ++i)
        {
            leaderboard.AddView(data[i]);
        }
    }
}
