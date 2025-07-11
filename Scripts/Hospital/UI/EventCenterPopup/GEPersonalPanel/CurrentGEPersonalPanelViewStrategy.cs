public class CurrentGEPersonalPanelViewStrategy : GEPersonalPanelViewStrategy
{
    public override void SetupPopupAccordingToData(GEPersonalPanelData data, GEPersonalPanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetImagesGrayscale(false);

        uiElement.SetGoalSprite(data.contributionItemSprite);
        uiElement.SetGoalCounter(data.currentGoalID + 1);
        uiElement.SetProgressTooltipTextCounter(data.currentGoalScore, data.currentGoalMax);
        uiElement.SetProgress(data.currentGoalScore, data.currentGoalMax);
        uiElement.SetRewardSprite(data.currentEventRewardSprite);
        uiElement.SetRewardAmount(data.currentEventRewardAmount);
        uiElement.SetEventTitle(data.eventTitleTerm);
        uiElement.SetEventDescriptionText(data.eventDescriptionText);
        uiElement.SetTimeLeft(data.eventSecondsLeft);
        uiElement.SetSingleContributionReward(data.singleCoinsReward, data.singleExpReward);
        uiElement.SetContributionPanel(data.contributionPanelData);

        uiElement.SetProgressTooltipActive(true);
    }
}
