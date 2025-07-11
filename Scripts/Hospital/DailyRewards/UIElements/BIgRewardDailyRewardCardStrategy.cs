using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class BIgRewardDailyRewardCardStrategy : BaseUIElementStrategy<DailyRewardCardData, DailyRewardCardLayout>
{
    public override void SetupPopupAccordingToData(DailyRewardCardData data, DailyRewardCardLayout uiElement)
    {
        base.SetupPopupAccordingToData(data, uiElement);
        uiElement.SetBigNamePrizeText(data.PackageLocalizedString);
        uiElement.SetRewardBoxImage(data.MainImageGiftRepresentation);
        uiElement.SetBigNamePrizeText(data.PackageLocalizedString);
        uiElement.SetTodayGreenCheckMarkActive(data.rewardClaimed);
        uiElement.SetButtonOnClickAction(data.onCardClick);

        if (data.dayRespresentation == data.currentDayNumber)
        {
            uiElement.SetTodayText();
            uiElement.SetStarImageActive(true);
            if (data.rewardClaimed)
            {
                uiElement.SetTodayGreenCheckMarkActive(true);
            }
        }
        else if (data.dayRespresentation < data.currentDayNumber)
        {
            uiElement.SetOtherDayTextThanToday(data.dayRespresentation);
            if (!data.rewardClaimed)
            {
                if (data.currentDayNumber - data.dayRespresentation == 1)
                {
                    uiElement.SetGetForAddPanelActive(true);
                }
                else
                {
                    uiElement.SetXMarkActive(true);
                    uiElement.SetBackgroundColorToGray(true);
                    uiElement.SetUpperBarColorToGray(true);
                }

            }
            else
            {
                uiElement.SetGreenCheckMarkActive(true);
                uiElement.SetBackgroundColorToGray(true);
                uiElement.SetUpperBarColorToGray(true);
            }
        }
        else
        {
            uiElement.SetOtherDayTextThanToday(data.dayRespresentation);
        }

        uiElement.SetDayTextActive(true);
        uiElement.SetRewardBoxImageActive(true);
    }

    public override void ClearPopup(DailyRewardCardLayout uIElement)
    {
        uIElement.ClearCard();
    }
}
