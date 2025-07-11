public class MedicinesGEContributionPanelViewStrategy : GEContributionPanelViewStrategy
{
    public override void SetupPopupAccordingToData(GEContributionPanelData data, GEContributionPanelUI uiElement)
    {
        ClearPanel(uiElement);

        uiElement.SetMedicineImage(data.imageSprite, data.isImageGrayscale);
        uiElement.SetMinusButton(data.onMinusButtonDown, data.onMinusButtonUp, !(data.onMinusButtonDown == null || data.onMinusButtonUp == null));
        uiElement.SetPlusButton(data.onPlusButtonDown, data.onPlusButtonUp, !(data.onPlusButtonDown == null || data.onPlusButtonUp == null));
        uiElement.SetContributeButton(data.onContributeButtonClick, !(data.onContributeButtonClick == null));
        uiElement.SetContributionCounterText(data.itemsCount);

        uiElement.SetContributeBoxActive(true);
        uiElement.SetMedicineImageActive(true);
        uiElement.SetMinusButtonActive(true);
        uiElement.SetPlusButtonActive(true);
        uiElement.SetContributeButtonActive(true);
        uiElement.SetContributionCounterTextActive(true);
    }
}
