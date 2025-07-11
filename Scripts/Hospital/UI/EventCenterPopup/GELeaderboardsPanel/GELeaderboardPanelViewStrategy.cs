public class GELeaderboardPanelViewStrategy
{
    public void SetupPopupAccordingToData(GELeaderboardPanelData data, GELeaderboardPanelUI uiElement)
    {
        uiElement.SetPlayerScore(data.playerScore);
        uiElement.SetLeaderboard(data.leaderboardEntryData);
        uiElement.SetTimeLeft(data.timeLeft);
        uiElement.SetFriendsToggle(data.isFriendsToggleOn, data.onFriendsToggle);
        uiElement.SetContributionItem(data.contributionItemSprite);
    }
}
