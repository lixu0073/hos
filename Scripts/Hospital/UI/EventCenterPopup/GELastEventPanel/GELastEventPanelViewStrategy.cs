public class GELastEventPanelViewStrategy
{
    public void SetupPopupAccordingToData(GELastEventPanelData data, GELastEventPanelUI uiElement)
    {
        uiElement.SetContributionItem(data.contributionItemSprite);
        uiElement.SetPlayerScore(data.playerScore);
        uiElement.SetRewardAmount(data.rewardsAmount);
        uiElement.SetRewardImage(data.rewardSprite);
        uiElement.SetLeaderboard(data.leaderboardData);

        uiElement.SetRewardObjectActive(data.rewardSprite != null);
    }
}
