using UnityEngine;
using System.Collections.Generic;
using TMPro;
using MovementEffects;
using System;

public class DailyQuestPopUpUI : MonoBehaviour, ITabControllerClient
{
    public List<DailyTaskUI> taskList;
    public List<DailyQuestUI> questList;
#pragma warning disable 0649
    [SerializeField] DayMarkerUI dayMarker;
    [SerializeField] TextMeshProUGUI topText;
    [SerializeField] TextMeshProUGUI bottomText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] Animator infoHoverAnim;
    [SerializeField] DailyQuestFlyingStarsUI flyingStars;
    [SerializeField] GameObject tutorialClickBlocker;
    [SerializeField] TextMeshProUGUI titleText;
#pragma warning restore 0649
    IEnumerator<float> topTimerCoroutine = null;
    IEnumerator<float> bottomTimerCoroutine = null;
    DailyQuestController dailyQuestController;


    public void Initialize()
    {
        //Debug.LogError("DailyQuestPopUpUI Open");
        dailyQuestController = ReferenceHolder.GetHospital().dailyQuestController;
        titleText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAILY_QUESTS");

        Refresh();

        if (!TutorialController.Instance.IsTutorialStepCompleted(StepTag.daily_quests_popup_1))
            tutorialClickBlocker.SetActive(true);

        NotificationCenter.Instance.DailyQuestPopUpOpen.Invoke(new BaseNotificationEventArgs());

        NotificationCenter.Instance.DailyQuestCardFlipped.Invoke(new BaseNotificationEventArgs()); // CV FIX TO BE TESTED

    }

    public void Refresh()
    {
        UpdateQuest();
        SetDayMarker();
        SwitchTasks();
        FlyStarsAnim();
        SetTopText();

        if (bottomTimerCoroutine == null)
            bottomTimerCoroutine = Timing.RunCoroutine(UpdateBottomTimer());
    }

    void SetTopText()
    {
        if (dailyQuestController.currentDailyQuest.GetCompletedTasksCount() < 3)
            topText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/COMPLETE_FOR_REWARD");
        else
            topText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_X_COMPLETED"), (dailyQuestController.GetCurrentDayNumber() + 1));
    }

    void SetDayMarker()
    {
        dayMarker.SetPosition(questList[dailyQuestController.GetCurrentDayNumber()],
                              !dailyQuestController.GetCurrentDailyQuest().isSwitchDayAnimationNeeded || DailyQuestSynchronizer.Instance.DayCounter == 0);
    }

    void SwitchTasks()
    {
        var currentDQ = dailyQuestController.GetCurrentDailyQuest();
        //var nextDQ = dailyQuestController.GetDailyQuestFromYesterday();

        // Animacja podmiany starych tasków na nowe (Animation of replacing old ones with new ones)
        Timing.KillCoroutine(SetDailyTaskAnimator().GetType());
        if (currentDQ.isSwitchDayAnimationNeeded && DailyQuestSynchronizer.Instance.DayCounter != 0)
        {
            UpdateTasks(false);
            currentDQ.isSwitchDayAnimationNeeded = false;
            Timing.RunCoroutine(SetDailyTaskAnimator());
        }
        else
            UpdateTasks();
    }

    void FlyStarsAnim()
    {
        var currentDQ = dailyQuestController.GetCurrentDailyQuest();
        var nextDQ = dailyQuestController.GetDailyQuestFromYesterday();

        // Animacje odlatywania gwiazdek (Star departure animations)
        if (currentDQ != null && currentDQ.IsCompleted() && currentDQ.isStarAnimationNeeded)
        {
            int day = dailyQuestController.GetCurrentDayNumber();
            questList[day].PrepareForFlyingStars();
            Invoke("FlyStarsToCurrentQuest", 1f);
        }
        else if (nextDQ != null && nextDQ.GetCompletedTasksCount() > 0 && nextDQ.isStarAnimationNeeded)
        {
            int day = dailyQuestController.GetCurrentDayNumber() - 1;
            questList[day].PrepareForFlyingStars();
            Invoke("FlyStarsToYesterdayQuest", 1f);
        }
    }

    void FlyStarsToCurrentQuest()
    {
        //THIS HAS TO BE INVOKED, BECAUSE POP UP ENTRY ANIMATION IS BEING PLAYED AND STAR POSITIONS ARE INCORRECT!
        //Debug.LogError("Day completed! Fly down the stars!");
        int day = dailyQuestController.GetCurrentDayNumber();
        int stars = 3;

        flyingStars.FlyStars(stars, day);

        questList[day].PrepareForFlyingStars();

        dailyQuestController.GetCurrentDailyQuest().isStarAnimationNeeded = false;
    }

    void FlyStarsToYesterdayQuest()
    {
        //THIS HAS TO BE INVOKED, BECAUSE POP UP ENTRY ANIMATION IS BEING PLAYED AND STAR POSITIONS ARE INCORRECT!
        //Debug.LogError("First visit today! Fly down the stars!");
        int day = dailyQuestController.GetCurrentDayNumber() - 1;
        int stars = dailyQuestController.GetDailyQuestFromYesterday().GetCompletedTasksCount();

        flyingStars.FlyStars(stars, day);

        questList[day].PrepareForFlyingStars();

        dailyQuestController.GetDailyQuestFromYesterday().isStarAnimationNeeded = false;
    }

    public void DeactivateContent()
    {
        if (topTimerCoroutine != null)
        {
            Timing.KillCoroutine(topTimerCoroutine);
            topTimerCoroutine = null;
        }

        if (bottomTimerCoroutine != null)
        {
            Timing.KillCoroutine(bottomTimerCoroutine);
            bottomTimerCoroutine = null;
        }

        NotificationCenter.Instance.DailyQuestPopUpClosed.Invoke(new BaseNotificationEventArgs());

        if (Hospital.RatePopUp.ShouldShowRate(true))
        {
            gameObject.SetActive(true);
            StartCoroutine(UIController.get.RatePopUp.Open());
        }
    }

    void UpdateTasks(bool isCurrentDay = true)
    {
        DailyTask[] tasks;

        if (isCurrentDay)
            tasks = dailyQuestController.GetCurrentDailyQuest().taskCollection;
        else
            tasks = dailyQuestController.GetDailyQuestFromYesterday().taskCollection;

        int count = tasks.Length;

        if (taskList.Count != tasks.Length)
            Debug.LogError("TASKS COUNT IS DIFFERENT FOR FRONTEND AND BACKEND! FIX IT!");

        for (int i = 0; i < count; ++i)
            taskList[i].UpdateStatus(tasks[i]);
    }

    void UpdateTask(int id)
    {
        DailyTask[] tasks = dailyQuestController.GetCurrentDailyQuest().taskCollection;

        int count = tasks.Length;

        if (taskList.Count != tasks.Length || id > tasks.Length - 1)
            Debug.LogError("TASKS COUNT IS DIFFERENT FOR FRONTEND AND BACKEND! FIX IT!");

        taskList[id].UpdateStatus(tasks[id]);
    }

    public void UpdateAllTasks()
    {
        DailyTask[] tasks = dailyQuestController.GetCurrentDailyQuest().taskCollection;

        int count = tasks.Length;

        if (taskList.Count != tasks.Length)
            Debug.LogError("TASKS COUNT IS DIFFERENT FOR FRONTEND AND BACKEND! FIX IT!");

        for (int i = 0; i < count; ++i)
            taskList[i].UpdateStatus(tasks[i]);
    }

    void UpdateQuest()
    {
        List<DailyQuest> quests = dailyQuestController.GetAllDailyQuests();

        if (questList.Count != quests.Count)
            Debug.LogError("QUESTS COUNT IS DIFFERENT FOR FRONTEND AND BACKEND! FIX IT!");

        int count = quests.Count;

        for (int i = 0; i < count; ++i)
            questList[i].Init(quests[i], i);
    }

    IEnumerator<float> SetDailyTaskAnimator()
    {
        yield return Timing.WaitForSeconds(.5f);

        for (int i = 0; i < taskList.Count; ++i)
        {
            Animator anim = taskList[i].GetComponent<Animator>();
            anim.SetTrigger("TaskChange");
            yield return Timing.WaitForSeconds(.5f);
            UpdateTask(i);
            SoundsController.Instance.PlayMagicPoof2();
        }
    }

    IEnumerator<float> UpdateBottomTimer()
    {
        int d = dailyQuestController.GetCurrentDayNumber() + 1;

        if (dailyQuestController.currentDailyQuest.GetCompletedTasksCount() == 3)
            bottomText.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/NEXT_DAY_STARTS_IN");
        else
            bottomText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_ENDS_IN"), d);

        while (true)
        {
            string t = UIController.GetFormattedTime(dailyQuestController.TimeTillNextDay());
            timerText.text = t;
            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void ButtonInfoDown()
    {
        infoHoverAnim.SetBool("Show", true);
        SoundsController.Instance.PlayInfoButton();
    }

    public void ButtonInfoUp()
    {
        infoHoverAnim.SetBool("Show", false);
    }

    [TutorialTriggerable]
    public void ShowTutorialAnimation()
    {
        if (gameObject.activeInHierarchy)
            Timing.RunCoroutine(TutorialAnimation());
        else
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.UnlockDailyQuests));
    }

    IEnumerator<float> TutorialAnimation()
    {
        tutorialClickBlocker.SetActive(true);

        yield return Timing.WaitForSeconds(0.5f);
        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.UnlockDailyQuests));
        yield return 0;
        taskList[0].UpdateStatus(dailyQuestController.GetCurrentDailyQuest().taskCollection[0]);
        taskList[0].PlayFrameParticles();
        taskList[0].Bounce();

        yield return Timing.WaitForSeconds(2f);
        tutorialClickBlocker.SetActive(false);
        NotificationCenter.Instance.ShowDailyQuestAnimation.Invoke(new BaseNotificationEventArgs());
    }

    int debugDay = -1;
    public void DebugStarTest()
    {
        ++debugDay;
        if (debugDay == 7)
            debugDay = 0;

        flyingStars.FlyStars(3, debugDay);
    }

    public void DebugResetDailyQuest()
    {
        ReferenceHolder.GetHospital().dailyQuestController.WeeklyRestart();
    }

    public void DebugCompleteCurrentQuest()
    {
        dailyQuestController.SetCurrentDailyQuestCompleted();
        Initialize();
    }

    public void DebugGoToNextDay()
    {
        dailyQuestController.DebugNextDay();
        UIController.getHospital.DailyQuestMainButtonUI.Refresh();

        if (dailyQuestController.isWeekPassed())
        {
            DeactivateContent();
            gameObject.SetActive(true);
            StartCoroutine(UIController.getHospital.DailyQuestWeeklyUI.Open());
        }
        else Initialize();
    }

    public void SetTabContentActive(Action onOpened, Action OnFailed)
    {

        if (ReferenceHolder.GetHospital().dailyQuestController.IsFirstStart())
            ReferenceHolder.GetHospital().dailyQuestController.FirstWeeklyStart();

        if (ReferenceHolder.GetHospital().dailyQuestController.isWeekPassed())
        {
            UIController.getHospital.DailyQuestAndDailyRewardUITabController.Exit();
            gameObject.SetActive(true);
            StartCoroutine(UIController.getHospital.DailyQuestWeeklyUI.Open());
        }
        else
        {
            Initialize();
            onOpened?.Invoke();
        }
    }

    public void DeactiveTabContent()
    {
        DeactivateContent();
    }
}
