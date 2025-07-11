using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MovementEffects;
using System.Collections.Generic;

public class DailyTaskUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] GameObject contentFront;
    [SerializeField] GameObject contentBack;

    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TextMeshProUGUI tapTextBack = null;

    [SerializeField] GameObject star;
    [SerializeField] Image progressbar;
    [SerializeField] ParticleSystem frameParticles;
    [SerializeField] float animatorDelay;
    [SerializeField] GameObject replaceButton;
#pragma warning restore 0649
    bool isFlipped;

    private DailyTask currentDailyTask;

    public void UpdateStatus(DailyTask task)
    {
        currentDailyTask = task;

        SetProgressBar(task.GetProgressFloat());
        descriptionText.text = task.GetDescription();
        statusText.text = task.GetProgressString();
        infoText.text = task.GetInfo();

        UpdateReplaceButton(false);
        if (task.IsCompleted())
            ShowStar();
        else
        {
            if (!ResourcesHolder.GetHospital().dailyTaskDatabase.IsDailyTaskOnllyForFirstWeek(task.taskType) && DailyQuestSynchronizer.Instance.CanReplaceTasks())
                UpdateReplaceButton(true);

            HideStar();
            Bounce();
        }
    }

    public void UpdateReplaceButton(bool isActive)
    {
        replaceButton.SetActive(isActive);
        if (isActive)
            tapTextBack.SetText(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DQ_CHANGE_TASK_TITLE"));
        else
            tapTextBack.SetText(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TAP_TO_FLIP"));
    }

    public Vector2 GetStarPosition()
    {
        return star.transform.position;
    }

    public Vector2 GetStarSize()
    {
        return star.GetComponent<RectTransform>().sizeDelta;
    }

    public void HideStar()
    {
        star.SetActive(false);
    }

    public void SetStarAnimator(string toReset, string toSet)
    {
        Animator anim = this.gameObject.GetComponent<Animator>();
        anim.ResetTrigger(toReset);
        anim.SetTrigger(toSet);
    }

    public void ShowStar()
    {
        star.SetActive(true);
        SetStarAnimator("Idle", "Bounce");
    }

    void SetProgressBar(float progress)
    {
        progressbar.fillAmount = progress;
    }

    public void Bounce()
    {
        if (gameObject.activeSelf)
            Timing.RunCoroutine(PlayBounceTask(animatorDelay));
    }

    private IEnumerator<float> PlayBounceTask(float animatorDelay)
    {
        yield return Timing.WaitForSeconds(animatorDelay);
        SetStarAnimator("Idle", "BounceTask");
    }

    public void PlayFrameParticles()
    {
        frameParticles.Play();
    }

    void ShowFront()
    {
        SetStarAnimator("ShowBack", "ShowFront");
    }

    void ShowBack()
    {
        if (!NotificationCenter.Instance.DailyQuestCardFlipped.IsNull())
            NotificationCenter.Instance.DailyQuestCardFlipped.Invoke(new BaseNotificationEventArgs());
        SetStarAnimator("ShowFront", "ShowBack");
    }

    public void ButtonFlip()
    {
        if (isFlipped)
            ShowFront();
        else
            ShowBack();
    }

    public void ButtonReplaceTask(int id)
    {
        StartCoroutine(UIController.getHospital.ReplaceDailyTaskPopup.Open(currentDailyTask, () =>
        {
            DailyTask newTask = ReferenceHolder.GetHospital().dailyQuestController.ReplaceCurrentDailyTask(id);
            ButtonFlip();
            UIController.getHospital.DailyQuestPopUpUI.UpdateAllTasks();
        }, null));
    }

    public void SetFlipTrue()
    {
        isFlipped = true;
    }

    public void SetFlipFalse()
    {
        isFlipped = false;
    }

    public void PlayFlipSfx()
    {
        SoundsController.Instance.PlayPatientCardOpen();
    }
}