public abstract class RequiredCureViewStrategy
{
    public abstract void SetupPopupAccordingToData(RequiredCureData data, RequiredCureUI uiElement);

    public void ClearPanel(RequiredCureUI uiElement)
    {
        uiElement.SetCureImageActive(false);
        uiElement.SetQuestionmarkActive(false);
    }
}
