using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using I2.Loc;

public class GELeaderboardPanelUI : MonoBehaviour
{
#pragma warning disable 0414
    [TermsPopup]
    [SerializeField] private string timerTerm = "-";
#pragma warning restore 0414
    [SerializeField] private Image contributionItem = null;

    [SerializeField] private TextMeshProUGUI playerScore = null;
    [SerializeField] private TextMeshProUGUI eventTimeLeft = null;
    [SerializeField] private Toggle friendsToggle = null;
    [SerializeField] private GELeaderboardController leaderboard = null;

    public void SetPlayerScore(int number)
    {
        playerScore.text = number.ToString();
    }

    public void SetLeaderboard(GELeaderboardEntryData[] data)
    {
        leaderboard.ClearPanel();

        if (data == null)
            return;

        for (int i = 0; i < data.Length; ++i)
        {
            leaderboard.AddView(data[i]);
        }
    }

    public void SetTimeLeft(int secondsLeft)
    {
        eventTimeLeft.text = string.Format(UIController.GetFormattedShortTime(secondsLeft));
    }

    public void SetFriendsToggle(bool isOn, UnityAction<bool> onToggle)
    {
        if (friendsToggle.isOn != isOn)
        {
            friendsToggle.isOn = isOn;
        }

        friendsToggle.onValueChanged.RemoveAllListeners();
        friendsToggle.onValueChanged.AddListener(onToggle);
    }
    
    public void SetContributionItem(Sprite itemSprite)
    {
        contributionItem.sprite = itemSprite;
    }
}
