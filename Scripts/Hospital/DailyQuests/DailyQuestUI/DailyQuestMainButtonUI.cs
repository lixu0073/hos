using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using MovementEffects;
using System.Collections.Generic;

public class DailyQuestMainButtonUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject starContainer;
    [SerializeField] GameObject[] stars;
    [SerializeField] Animator[] starAnimators;
    [SerializeField] ParticleSystem[] starParticles;
    [SerializeField] Image completedIcon;
    [SerializeField] TextMeshProUGUI dayText;
    [SerializeField] Animator anim;
#pragma warning restore 0649
    private int activeStars;
#pragma warning disable 0649
    Coroutine _refreshStars;
#pragma warning restore 0649

    private void OnDisable()
    {
        if (_refreshStars != null)
        {
            try
            {
                StopCoroutine(_refreshStars);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void ButtonDailyQuests()
    {
        HospitalUIController.get.DailyQuestAndDailyRewardUITabController.OpenTabContent((int)UIElementTabController.DailyQuestAndRewardIndexes.DailyQuest);
    }

    public void Refresh()
    {
        HideStars();

        if (ReferenceHolder.GetHospital().dailyQuestController.isWeekPassed())
        {
            completedIcon.gameObject.SetActive(true);
            starContainer.SetActive(false);
            dayText.text = "";
            timerText.transform.parent.gameObject.SetActive(false);
            Invoke("StartBlinking", .1f);
        }
        else
        {
            int day = ReferenceHolder.GetHospital().dailyQuestController.GetCurrentDayNumber() + 1;
            int starCount = ReferenceHolder.GetHospital().dailyQuestController.GetCurrentDailyQuest().GetCompletedTasksCount();

            completedIcon.gameObject.SetActive(false);
            dayText.text = "0" + day;

            if (DailyQuestSynchronizer.Instance.WeeklyEnd == 0) //hide timer before first week start (tutorial with arrow on this button)
                timerText.transform.parent.gameObject.SetActive(false);
            else
                timerText.transform.parent.gameObject.SetActive(true);

            StopBlinking();
            starContainer.SetActive(true);
            ShowStars(starCount);
        }
    }

    public void SpawnStar()
    {
        int starNumber = ReferenceHolder.GetHospital().dailyQuestController.GetCurrentDailyQuest().GetCompletedTasksCount();
        BornNewStar(starNumber);

        if (gameObject.activeSelf)
            StartCoroutine(RefreshStars());
    }

    private IEnumerator RefreshStars()
    {
        yield return new WaitForSeconds(1.0f);
        Refresh();
    }

    private void BornNewStar(int starNumber)
    {
        int starIndex = starNumber - 1;
        GameObject star = stars[starIndex];
        Animator starAnimator = starAnimators[starIndex];
        ParticleSystem particleParent = starParticles[starIndex];
        star.SetActive(true);
        starAnimator.SetTrigger("Born");
        ParticleSystem[] particleChildren = particleParent.gameObject.GetComponentsInChildren<ParticleSystem>();
        particleParent.Play();
        if (particleChildren != null && particleChildren.Length != 0)
        {
            for (int i = 0; i < particleChildren.Length; ++i)
            {
                particleChildren[i].Play();
            }
        }
    }

    void StartBlinking()
    {
        //this is invoked because for some reason Button disabled bool parameters on initialization or something
        anim.SetBool("AlertBool", true);
    }

    void StopBlinking()
    {
        anim.SetBool("AlertBool", false);
    }

    void HideStars()
    {
        for (int i = 0; i < 3; ++i)
            stars[i].SetActive(false);
    }

    void ShowStars(int count)
    {
        for (int i = 0; i < count; ++i)
            stars[i].SetActive(true);
    }

    public void InitTimer()
    {
        timerText.transform.parent.gameObject.SetActive(true);
        Timing.KillCoroutine(UpdateTimer());
        Timing.RunCoroutine(UpdateTimer());
    }

    IEnumerator<float> UpdateTimer()
    {
        while (true)
        {
            int timeTillNextDay = ReferenceHolder.GetHospital().dailyQuestController.TimeTillNextDay();
            //Debug.Log("timeTillNextDay = " + timeTillNextDay);

            timerText.text = UIController.GetFormattedShortTime(timeTillNextDay);
            yield return Timing.WaitForSeconds(1f);
        }
    }
}