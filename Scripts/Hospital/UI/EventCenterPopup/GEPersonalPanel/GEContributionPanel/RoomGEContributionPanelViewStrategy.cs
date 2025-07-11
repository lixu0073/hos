public class RoomGEContributionPanelViewStrategy : GEContributionPanelViewStrategy
{
    public override void SetupPopupAccordingToData(GEContributionPanelData data, GEContributionPanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetRoomImage(data.imageSprite, data.isImageGrayscale);

        uiElement.SetRoomImageActive(true);
    }
}
