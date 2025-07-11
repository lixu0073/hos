using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using System;

public class DailyQuestWeeklyUI : UIElement
{
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] GameObject contentRewards;
    [SerializeField] GameObject contentEmpty;
    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] GameObject superGrandReward;
    [SerializeField] GameObject superReward;
    [SerializeField] DailyQuestUnclaimedRewardUI[] unclaimedRewards;
    [SerializeField] ScrollRect scrollRewards;
#pragma warning restore 0649
    List<RewardPackage> rewardsToClaim;
    int rewardsCount = 0;
#pragma warning disable 0649
    Coroutine _scrollUpEffect;
#pragma warning restore 0649

    private void OnDisable()
    {
        if (_scrollUpEffect != null)
        {
            try
            { 
                StopCoroutine(_scrollUpEffect);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
    {
        yield return base.Open();

        rewardsToClaim = ReferenceHolder.GetHospital().dailyQuestController.GetWeeklyRewards();
        SetRewardsCount();

        int completedQuests = ReferenceHolder.GetHospital().dailyQuestController.GetCompletedQuestsCount();
        counterText.text = completedQuests + "/7";
        
        if (rewardsCount > 0)
            SetRewards();
        else
            SetEmpty();

        whenDone?.Invoke();
    }

    void SetRewardsCount()
    {
        int count = 0;
        for (int i = 0; i < rewardsToClaim.Count; i++)
        {
            if (rewardsToClaim[i] != null)
                count++;
        }
        rewardsCount = count;
    }

    void SetRewards()
    {
        contentEmpty.SetActive(false);
        contentRewards.SetActive(true);

        HideAllRewards();
        ShowUnclaimedRewards();

        titleText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/WEEKLY_SUMMARY");

        StartCoroutine(ScrollUpEffect());
    }

    void HideAllRewards()
    {
        superGrandReward.SetActive(false);
        superReward.SetActive(false);

        for (int i = 0; i < 7; ++i)
            unclaimedRewards[i].gameObject.SetActive(false);
    }

    void ShowUnclaimedRewards()
    {
        //rewards in this last are sorted like this: (0)SuperGrand, (1)Super, (2)day1, (3)day2, ..., (8)day7
        if (rewardsToClaim[0] != null)
            superGrandReward.SetActive(true);
        if (rewardsToClaim[1] != null)
            superReward.SetActive(true);

        for (int i = 2; i < rewardsToClaim.Count; i++)
        {
            if (rewardsToClaim[i] != null)
                unclaimedRewards[i - 2].Setup(rewardsToClaim[i]);
        }
    }

    void SetEmpty()
    {
        contentEmpty.SetActive(true);
        contentRewards.SetActive(false);
        titleText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/NEW_QUESTS_AVAILABLE");
    }
    
    IEnumerator ScrollUpEffect()
    {
        //Debug.LogError("ScrollUpEffect");
        float normPos = 0;
        while (normPos < 1)
        {
            normPos += Time.deltaTime / 2;
            scrollRewards.verticalNormalizedPosition = normPos;
            yield return null;
        }
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        base.Exit(false);
    }

    public void ButtonConfirm()
    {
        Exit();
        
        ReferenceHolder.GetHospital().dailyQuestController.ClaimRewardWeekly(rewardsToClaim);
        ReferenceHolder.GetHospital().dailyQuestController.WeeklyRestart();
    }

    public void ButtonExit()
    {
        Exit();
    }
}