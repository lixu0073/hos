public class SingleRewardEventEndedPopupViewStrategy : EventEndedPopupViewStrategy
{
    public override void SetupPopupAccordingToData(EventEndedPopupData data, EventEndedPopupUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetPlayerScore(data.playerScore);
        uiElement.SetPlayerPosition(data.playerPosition);
        uiElement.SetPrizeImage(data.prizeSprite);
        uiElement.SetClaimButton(data.onClaimButtonClick);
        uiElement.SetRewardAmount(data.rewardAmount);

        uiElement.SetRewardAmountActive(true);
    }
}
