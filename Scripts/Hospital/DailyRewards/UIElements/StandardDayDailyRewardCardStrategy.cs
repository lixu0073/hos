using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StandardDayDailyRewardCardStrategy : BaseUIElementStrategy<DailyRewardCardData, DailyRewardCardLayout>
{

    public override void SetupPopupAccordingToData(DailyRewardCardData data, DailyRewardCardLayout uiElement)
    {
        base.SetupPopupAccordingToData(data, uiElement);
        uiElement.SetButtonOnClickAction(data.onCardClick);
        if (data.dayRespresentation == data.currentDayNumber)
        {
            uiElement.SetTodayText();
            uiElement.SetStarImageActive(true);
            uiElement.SetAmountOfRewardText(data.AmountOfRewardToWin);
            uiElement.SetRewardIcon(data.MainImageGiftRepresentation);
            if (data.rewardClaimed)
            {
                uiElement.SetTodayGreenCheckMarkActive(true);
            }
            uiElement.SetAmountOfRewardTextActive(true);
            uiElement.SetRewardIconActive(true);
        }
        else if (data.dayRespresentation < data.currentDayNumber)
        {
            uiElement.SetOtherDayTextThanToday(data.dayRespresentation);
            if (!data.rewardClaimed)
            {
                if (data.currentDayNumber - data.dayRespresentation == 1)
                {
                    uiElement.SetGetForAddPanelActive(true);
                    uiElement.SetRewardIcon(data.MainImageGiftRepresentation);

                    uiElement.SetRewardIconActive(true);
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
            uiElement.SetDayTextActive(true);
        }
        else
        {
            uiElement.SetOtherDayTextThanToday(data.dayRespresentation);
            uiElement.SetRewardIcon(data.MainImageGiftRepresentation);
            uiElement.SetAmountOfRewardText(data.AmountOfRewardToWin);

            uiElement.SetRewardIconActive(true);
            uiElement.SetAmountOfRewardTextActive(true);
        }
        uiElement.SetDayTextActive(true);


    }

    public override void ClearPopup(DailyRewardCardLayout uIElement)
    {
        uIElement.ClearCard();
    }
}
