public class KnownRequiredCureViewStrategy : RequiredCureViewStrategy
{
    public override void SetupPopupAccordingToData(RequiredCureData data, RequiredCureUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetCureImage(data.cureSprite);
        uiElement.SetPointerDownListener(data.onPointerDown);

        uiElement.SetCureImageActive(true);
    }
}
