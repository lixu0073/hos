public class UnknownRequiredCureViewStrategy : RequiredCureViewStrategy
{
    public override void SetupPopupAccordingToData(RequiredCureData data, RequiredCureUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetPointerDownListener(data.onPointerDown);

        uiElement.SetQuestionmarkActive(true);
    }
}
