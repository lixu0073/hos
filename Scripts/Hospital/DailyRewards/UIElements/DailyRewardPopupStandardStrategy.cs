using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyRewardPopupStandardStrategy : BaseUIElementStrategy<DailyRewardPopupData, DailyRewardPopup>
{
    public override void SetupPopupAccordingToData(DailyRewardPopupData data, DailyRewardPopup uiElement)
    {
        base.SetupPopupAccordingToData(data, uiElement);
        uiElement.SetUpMainPopupTitle();
        uiElement.SetOnClaimButtonAction(data.onClaimButtonRewardClick);
        uiElement.GreyOutClaimButton(data.GrayOutClaimButton);
        uiElement.InitializeCharacter(data.characterResourcePath);
        uiElement.AssignToAnimatorMonitorEvent(data.animatorMonitor);

        uiElement.SetDevNextDayButtonActive(DeveloperParametersController.Instance().parameters.devTestButtonVisible);
    }

    public override void ClearPopup(DailyRewardPopup uIElement)
    {
    }
}
