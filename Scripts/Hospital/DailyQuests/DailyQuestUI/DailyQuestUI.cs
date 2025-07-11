using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;
using System;

public class DailyQuestUI : MonoBehaviour
{
    public Transform backgroundTransform;
#pragma warning disable 0649
    [SerializeField] GameObject starContainer;
    [SerializeField] GameObject[] stars;
    [SerializeField] TextMeshProUGUI footerText;
    [SerializeField] GameObject inProgress;
    [SerializeField] Image clockFill;
    [SerializeField] Transform clockHandTotal;
    [SerializeField] Transform clockHandSeconds;
    [SerializeField] GameObject questionmark;
    [SerializeField] GameObject[] rewards;
	[SerializeField] ParticleSystem rewardParticles;
	[SerializeField] ParticleSystem rewardBornShine;
    [SerializeField] Color footerColorDefault;
    [SerializeField] Color footerColorUpcoming;
#pragma warning restore 0649
    DailyQuest quest;
    DailyQuestUIState state;
    int day;
    int starCount;
#pragma warning disable 0649
    Coroutine _delayedRefresh;
#pragma warning restore 0649

    private void OnDisable()
    {
        if (_delayedRefresh != null)
        {
            try
            {
                StopCoroutine(_delayedRefresh);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void Init(DailyQuest quest, int day)
    {
        this.quest = quest;
        this.day = day;
        starCount = quest.GetCompletedTasksCount();

        SetDefaults();
        SetState();
        SetTooltip();
    }

    void SetDefaults()
    {
        HideRewards();
        HideInProgress();
        HideUpcoming();
        HideStarContainer();
        HideStars();
        HideRewardParticles();
        
        footerText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_X"), (day + 1));
    }
    
    public void PrepareForFlyingStars()
    {
        SetDefaults();
        ShowStarContainer();
    }

    void SetState()
    {
        //Debug.LogError("SetState");

        int currentDay = ReferenceHolder.GetHospital().dailyQuestController.GetCurrentDayNumber();
		if (day == currentDay && quest.GetCompletedTasksCount () < 3)
        {
			state = DailyQuestUIState.InProgress;
			SoundsController.Instance.PlayClockTicking ();
		}
        else if (day > currentDay)
            state = DailyQuestUIState.Upcoming;
        else if (!quest.IsDailyQuestRewardPackageClaimed && quest.GetCompletedTasksCount() > 0)
            state = DailyQuestUIState.CollectReward;
        else
            state = DailyQuestUIState.Completed;

        switch (state)
        {
			case DailyQuestUIState.Completed:
				ShowStarContainer ();
				ShowStars (starCount);
                break;
		case DailyQuestUIState.CollectReward:
				ShowReward (starCount);
				ShowRewardParticles (starCount);
                break;
            case DailyQuestUIState.InProgress:
                ShowInProgress();
                break;
            case DailyQuestUIState.Upcoming:
                ShowUpcoming();
                break;
            default:
                Debug.LogError("this QuestUI state is not handled! CALL MIKKO!");
                break;
        }

        if (day == currentDay)
            backgroundTransform.localScale = Vector3.one * 1.1f;
        else
            backgroundTransform.localScale = Vector3.one;
    }

	/*void SetTreasureAnimator (string toReset, string toSet, GameObject reward)
	{
		Animator anim = reward.GetComponent<Animator> ();
		anim.ResetTrigger (toReset);
		anim.SetTrigger (toSet);
	}

	void SetStarsAnimator (string toReset, string toSet, GameObject star)
	{
		Animator anim = star.GetComponent<Animator> ();
		anim.ResetTrigger (toReset);
		anim.SetTrigger (toSet);
	}*/

    void ShowReward(int rewardId)
    {
		Debug.Log("rewardId " + rewardId);
        rewards[rewardId-1].SetActive(true);
        footerText.text = I2.Loc.ScriptLocalization.Get("GIFT_BOXES_OPEN");
    }

    void HideRewards()
    {
        for (int i = 0; i < rewards.Length; i++)
            rewards[i].SetActive(false);
    }

    void ShowInProgress()
    {
        inProgress.SetActive(true);
    }

    void HideInProgress()
    {
        inProgress.SetActive(false);
    }
    
    void ShowUpcoming()
    {
        questionmark.SetActive(true);
    }

    void HideUpcoming()
    {
        questionmark.SetActive(false);
    }

    void HideStarContainer()
    {
        starContainer.SetActive(false);
    }

    void ShowStarContainer()
    {
        starContainer.SetActive(true);
    }

    private void HideStars()     
    {
        for (int i = 0; i < 3; ++i)
        {
			stars[i].SetActive(false);
        }
    }

    void ShowStars(int count)
    {
        starContainer.SetActive(true);
        for (int i = 1; i <= count; ++i)
        {
            stars[i-1].SetActive(true);
        }
    }

    public void ShowStar(int starIndex, bool showReward)
    {
        stars[starIndex].SetActive(true);

        if (showReward)
            StartCoroutine(DelayedRefresh());
    }

    IEnumerator DelayedRefresh()
    {
        //MARIO TU MOZESZ OPÓŹNIĆ ZMIANE GWIAZDKI NA REWARD (MARIO HERE YOU CAN DELAY THE CHANGE OF STARS FOR REWARD)
        yield return new WaitForSeconds(.5f);
        SetDefaults();
        SetState();
		rewardBornShine.Play ();
		SoundsController.Instance.PlayZip ();
    }

    void HideRewardParticles()
    {
        rewardParticles.gameObject.SetActive(false);
    }

    void ShowRewardParticles(int level) 
	{
        if (level == 0)
        {
            HideRewardParticles();
            return;
        }

        rewardParticles.gameObject.SetActive (true);

		var em = rewardParticles.emission;
        //var rate = em.rate;
        var rate = em.rateOverTime;		
		float minRate = 7f;
		float maxRate = 10f;

		for (int i = 0; i < level; ++i)
		{
			rate.constantMin = minRate;
			minRate += minRate;
			rate.constantMax = maxRate;
			maxRate += maxRate;
		}
        //em.rate = rate;
        em.rateOverTime = rate;		
	}

    public Vector2 GetStarPosition(int starIndex)
    {
        return stars[starIndex].transform.position;
    }

    public Vector2 GetStarSize(int starIndex)
    {
        return stars[starIndex].transform.parent.GetComponent<RectTransform>().sizeDelta;
    }

    public void ButtonClick()
    {
        if (state == DailyQuestUIState.CollectReward && starCount > 0)
            ReferenceHolder.GetHospital().dailyQuestController.ClaimRewardForQuest(quest);
        else if (state == DailyQuestUIState.CollectReward && starCount == 0)
            Debug.LogError("SOMETHING WENT WRONG! CALL MIKKO!");
    }

    void SetTooltip()
    {
        PointerDownListener pdl = GetComponent<PointerDownListener>();

        switch (state)
        {
            case DailyQuestUIState.Completed:
                pdl.SetDelegate(() =>
                {
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TOOLTIP_FINISHED"));
                });
                break;
            case DailyQuestUIState.InProgress:
                pdl.SetDelegate(() =>
                {
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TOOLTIP_IN_PROGRESS"));
                });
                break;
            case DailyQuestUIState.Upcoming:
                pdl.SetDelegate(() =>
                {
                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TOOLTIP_UPCOMING"));
                });
                break;
            default:
                pdl.SetDelegate(null);
                break;
        }
    }

    void Update()
    {
        if (state == DailyQuestUIState.InProgress)
        {
            //Debug.LogError("TimeTillNextDay() = " + ReferenceHolder.Get().dailyQuestController.TimeTillNextDay());
            int secondsPassed = ReferenceHolder.GetHospital().dailyQuestController.TimeTillNextDay();
            float dayProgress = 1 - (secondsPassed / 3600f / 24f);
            clockFill.fillAmount = dayProgress;
            clockHandTotal.localRotation = Quaternion.Euler(0, 0, 180 - (dayProgress * 360));

            float minuteProgress = (secondsPassed % 60) / 60f;
            clockHandSeconds.localRotation = Quaternion.Euler(0, 0, 180 + (minuteProgress * 360));
        }
    }
}

public enum DailyQuestUIState
{
    Completed,
    InProgress,
    CollectReward,
    Upcoming
}