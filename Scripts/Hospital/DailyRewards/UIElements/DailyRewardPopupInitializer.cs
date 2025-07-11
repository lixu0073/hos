using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DailyRewardPopupInitializer : BaseUIInitializer<DailyRewardPopupData, DailyRewardPopupController>
{

    public override void Initialize(Action OnSuccess, Action OnFailure)
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= ContentUnlockLevel.DailyRewardUnlockLevel)
        {

            base.Initialize(OnSuccess, OnFailure);
            DailyRewardPopupData data = PreparePopupData();
            popupController.Initialize(data);
            if (data != null)
            {
                ReferenceHolder.GetHospital().DailyRewardController.NewDayArrised -= DailyRewardController_NewDayArrised;
                ReferenceHolder.GetHospital().DailyRewardController.NewDayArrised += DailyRewardController_NewDayArrised;
                OnSuccess?.Invoke();
            }
            else
            {
                OnFailure?.Invoke();
            }
        }
        else
        {
            MessageController.instance.ShowMessage(string.Format(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT_LEVEL"), ContentUnlockLevel.DailyRewardUnlockLevel.ToString()));
            OnFailure?.Invoke();
        }

    }

    protected override void AddPopupControllerRuntime()
    {
        popupController = gameObject.AddComponent<DailyRewardPopupController>();
    }

    protected override DailyRewardPopupData PreparePopupData()
    {
        if (ReferenceHolder.GetHospital().DailyRewardController.weeklyRewards.Count == WorldWideConstants.DAYS_IN_WEEK)
        {
            DailyRewardPopupData data = new DailyRewardPopupData();
            data.animatorMonitor = UIController.getHospital.DailyQuestAndDailyRewardUITabController;
            data.strategy = new DailyRewardPopupStandardStrategy();
            int currentDay = ReferenceHolder.GetHospital().DailyRewardController.GetCurrentDayInWeek();
            List<DailyRewardModel> dailyRewardStatus = ReferenceHolder.GetHospital().DailyRewardController.weeklyRewards;
            // prepare data for daily reward popup
            if (!ReferenceHolder.GetHospital().DailyRewardController.weeklyRewards[currentDay].isClaimed)
            {
                data.GrayOutClaimButton = false;
                data.onClaimButtonRewardClick = new UnityEngine.Events.UnityAction(delegate
                {
                    ReferenceHolder.GetHospital().DailyRewardController.CollectRewardForDay(currentDay);
                    ReinitializeWhileOpened();
                });
            }
            else
            {
                data.GrayOutClaimButton = true;
                data.onClaimButtonRewardClick = new UnityEngine.Events.UnityAction(delegate
                {
                    MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("DAILY_REWARD_CLAIMED_FLOAT"));
                });
            }
            // prepare data for daily reward cards
            for (int i = 0; i < dailyRewardStatus.Count - 1; ++i)
            {
                int currentLoopIndex = i;

                data.cardData.Add(new DailyRewardCardData());
                data.cardData[i].cardStrategy = new StandardDayDailyRewardCardStrategy();
                data.cardData[i].currentDayNumber = currentDay;
                data.cardData[i].rewardClaimed = dailyRewardStatus[i].isClaimed;
                data.cardData[i].AmountOfRewardToWin = dailyRewardStatus[i].GetDailyRewardGift().GetGiftAmount();
                data.cardData[i].MainImageGiftRepresentation = dailyRewardStatus[i].GetDailyRewardGift().GetMainImageForGift();
                data.cardData[i].GiftIcon = dailyRewardStatus[i].GetDailyRewardGift().GetIconForGift();
                data.cardData[i].dayRespresentation = currentLoopIndex;

                if (currentLoopIndex == currentDay)
                {
                    if (!dailyRewardStatus[i].isClaimed)
                    {
                        data.cardData[currentLoopIndex].onCardClick = new UnityEngine.Events.UnityAction(delegate
                        {
                            ReferenceHolder.GetHospital().DailyRewardController.CollectRewardForDay(currentLoopIndex);
                            ReinitializeWhileOpened();
                        });
                    }
                }
                else if ((currentLoopIndex == currentDay - 1) && !dailyRewardStatus[currentLoopIndex].isClaimed)
                {
                    bool isAdEnable = AdsController.instance.IsAdAvailable(AdsController.AdType.rewarded_ad_missed_daily_reward);
                    if (isAdEnable)
                    {
                        data.cardData[currentLoopIndex].onCardClick = new UnityEngine.Events.UnityAction(delegate
                        {
                            AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_missed_daily_reward, delegate
                            {
                                Debug.Log("AdSusscesful");
                                ReferenceHolder.GetHospital().DailyRewardController.CollecRewardDueToAd();
                                ReinitializeWhileOpened();
                            }
                        );
                        });
                    }
                    else
                    {
                        data.cardData[currentLoopIndex].onCardClick = new UnityEngine.Events.UnityAction(delegate { MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("NO_OFFER_AVAILABLE")); });
                    }
                }
            }
            // prepare data for big prize
            int lastDayIndex = dailyRewardStatus.Count - 1;

            data.cardData.Add(new DailyRewardCardData()
            {
                currentDayNumber = currentDay,
                dayRespresentation = lastDayIndex,
                cardStrategy = new BIgRewardDailyRewardCardStrategy(),
                MainImageGiftRepresentation = ReferenceHolder.GetHospital().DailyRewardController.grandRewardForEntireWeek.GetMainImageForGift(),
                PackageLocalizedString = I2.Loc.ScriptLocalization.Get(ReferenceHolder.GetHospital().DailyRewardController.grandRewardForEntireWeek.GetLocalizationKey()),
                rewardClaimed = dailyRewardStatus[lastDayIndex].isClaimed,
            });
            if (lastDayIndex == currentDay)
            {
                if (!dailyRewardStatus[lastDayIndex].isClaimed)
                {
                    data.cardData[lastDayIndex].onCardClick = new UnityEngine.Events.UnityAction(delegate
                    {
                        ReferenceHolder.GetHospital().DailyRewardController.CollectRewardForDay(lastDayIndex);
                        ReinitializeWhileOpened();
                    });
                }
            }
            data.characterResourcePath = GetPathForCharacterGraph();
            return data;
        }
        return null;
    }

    protected virtual string GetPathForCharacterGraph()
    {
        return "AnimatedCharacterVIP_LeoPose1";
    }

    protected override void Refresh(DailyRewardPopupData dataType)
    {
        popupController.RefreshDataWhileOpened(dataType);
    }

    private void DailyRewardController_NewDayArrised(int obj)
    {
        ReinitializeWhileOpened();
    }

    public override void DeInitialize()
    {
        base.DeInitialize();
        if (ReferenceHolder.GetHospital() != null && ReferenceHolder.GetHospital().DailyRewardController != null)
        {
            ReferenceHolder.GetHospital().DailyRewardController.NewDayArrised -= DailyRewardController_NewDayArrised;
        }
        popupController.DeInitialize();
    }

}
