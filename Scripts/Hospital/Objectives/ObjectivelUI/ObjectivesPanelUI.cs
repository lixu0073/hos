using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MovementEffects;
using TutorialSystem;
using System;

public class ObjectivesPanelUI : MonoBehaviour, ITutorialTrigger
{
#pragma warning disable 0649
    [SerializeField] RectTransform rect;
    [SerializeField] Animator anim;
    [SerializeField] GameObject objectivesList;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI rewardTitleText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] GameObject UIObjectivePrefab;
    [SerializeField] GameObject infoObject;

    [SerializeField] GameObject rewardPanel;
    [SerializeField] GameObject rewardButton;
    [SerializeField] ScrollRect scrollRect;
#pragma warning restore 0649
    [SerializeField] private GameObject taskCompletedBadge = null;
    [SerializeField] private GameObject listCompletedBadge = null;
#pragma warning disable 0649
    [SerializeField] GameObject arrowButtonIcon;

    [Header("References to UI that shall be depended on iphone version")]
    [SerializeField] Sprite iphoneXbackground;
    [SerializeField] Image ObjectiveButtonImage;
    [SerializeField] Sprite iphoneXButton;
    [SerializeField] Transform iconTransfrom;
    [SerializeField] GameObject goalButtonIcon;
    [SerializeField] GameObject toggleButton;
#pragma warning restore 0649
    [SerializeField] private GameObject objectivesPanel;
    private Image backgroundImage;
    public GameObject ToggleButton { get { return toggleButton; } }

    public Dictionary<string, TutorialTriggerEvent> triggerEvents = new Dictionary<string, TutorialTriggerEvent>()
    {
        { "Panel_Opened", new TutorialTriggerEvent() },
        { "Panel_Closed", new TutorialTriggerEvent() }
    };

    public Dictionary<string, TutorialTriggerEvent> TriggerEvents
    {
        get
        {
            return triggerEvents;
        }
    }

    [TutorialTrigger]
    public event System.EventHandler panelOpened;
    [TutorialTrigger]
    public event System.EventHandler panelClosed;

    RectTransform goalButtonIconRecttransform;

    public bool isSlidIn;

    private bool autoSlideIn = false;
    Coroutine slideInCoroutine;
    Coroutine slideOutBackgroundHideCoroutine;

    private IEnumerator<float> scrollCoroutine;
    private bool hiddenTemporary = false;

    bool claimed = false;

    [SerializeField]
    private GameObject ParticleReward = null;


    private void Start()
    {
        goalButtonIconRecttransform = goalButtonIcon.gameObject.GetComponent<RectTransform>();
        backgroundImage = GetComponent<Image>();
        if (ExtendedCanvasScaler.HasNotch())
        {
            GetComponent<Image>().sprite = iphoneXbackground;
            ObjectiveButtonImage.sprite = iphoneXButton;
            GetComponent<RectTransform>().offsetMin += new Vector2(-60, 0);
            GetComponent<RectTransform>().offsetMax += new Vector2(-60, 0);
            toggleButton.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-9, 7, 0);
            toggleButton.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(65, 65);
            iconTransfrom.localPosition = new Vector3(33.0f, 0, 0);
            taskCompletedBadge.GetComponent<RectTransform>().offsetMin = new Vector2(43, -33);
            taskCompletedBadge.GetComponent<RectTransform>().offsetMax = new Vector2(71, -3);
            SetBackgroundShowing(false);
        }
    }

    [TutorialTriggerable]
    public void SetBlinking(bool isOn)
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
        {
            TutorialUIController.Instance.StopBlinking();
            return;
        }
        if (isOn) TutorialUIController.Instance.BlinkImage(ObjectiveButtonImage);
        else TutorialUIController.Instance.StopBlinking();
    }

    public void Show()
    {
        if (ReferenceHolder.Get().objectiveController.IsDynamicObjective())
        {
            goalButtonIcon.SetActive(true);
            if (ExtendedCanvasScaler.HasNotch())
            {
                goalButtonIconRecttransform.anchorMin = new Vector2(0.5f, 0.5f);
                goalButtonIconRecttransform.anchorMax = new Vector2(0.5f, 0.5f);
                goalButtonIconRecttransform.localPosition = new Vector3(32.5f, 0, 0);
            }
            arrowButtonIcon.SetActive(false);
        }
        else
        {
            goalButtonIcon.SetActive(false);
            arrowButtonIcon.SetActive(true);
        }
        gameObject.SetActive(true);
        NotificationCenter.Instance.ObjectivePanelOpened.Invoke(new BaseNotificationEventArgs());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void SlideIn()
    {
        if (!gameObject.activeInHierarchy || UIController.get.drawer.IsVisible)
            return;
        SetBackgroundShowing(true);

        //if (ExtendedCanvasScaler.isPhone())
        //{
        UIController.get.SetEventButtonVisible(false);
        //}

        ReferenceHolder.Get().objectiveController.SetRewardSeen(true);
        SetTaskCompletedBadgeActive(false);

        StopScrollCoroutine();

        isSlidIn = true;
        anim.ResetTrigger("SlideOut");
        anim.SetTrigger("SlideIn");
        Setup();
        iconTransfrom.localScale = new Vector3(1, 1, 1);
        if (ExtendedCanvasScaler.HasNotch())
            iconTransfrom.localPosition = new Vector3(30f, 0, 0);

        hiddenTemporary = false;

        if (TriggerEvents.ContainsKey("Panel_Opened") && TriggerEvents["Panel_Opened"] != null)
            TriggerEvents["Panel_Opened"].Invoke(this, new TutorialTriggerArgs());
        panelOpened?.Invoke(this, null);

        NotificationCenter.Instance.ObjectivePanelOpened.Invoke(new BaseNotificationEventArgs());
    }

    public void SlideIn(float delay = 0)
    {
        if (slideOutBackgroundHideCoroutine != null)
        {
            StopCoroutine(slideOutBackgroundHideCoroutine);
            slideOutBackgroundHideCoroutine = null;
        }
        if (delay > 0)
            slideInCoroutine = StartCoroutine(SlideInDelayed(delay));
        else
            SlideIn();
    }

    IEnumerator SlideInDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SlideIn();

        //slideInCoroutine = null;
    }

    public void SlideOut()
    {
        //if (ExtendedCanvasScaler.isPhone())
        //{
        UIController.get.SetEventButtonVisible(true);
        //}
        if (TriggerEvents.ContainsKey("Panel_Closed") && TriggerEvents["Panel_Closed"] != null)
            TriggerEvents["Panel_Closed"].Invoke(this, new TutorialTriggerArgs());
        panelClosed?.Invoke(this, null);
        NotificationCenter.Instance.ObjectivePanelClosed.Invoke(new BaseNotificationEventArgs());

        if (slideInCoroutine != null)
        {
            try
            {
                StopCoroutine(slideInCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            slideInCoroutine = null;
        }

        StopScrollCoroutine();
        isSlidIn = false;
        anim.ResetTrigger("SlideIn");
        anim.SetTrigger("SlideOut");

        iconTransfrom.localScale = new Vector3(-1, 1, 1);
        if (ExtendedCanvasScaler.HasNotch())
        {
            iconTransfrom.localPosition = new Vector3(33.0f, 0, 0);
            slideOutBackgroundHideCoroutine = StartCoroutine(SlideOutBackgroundHide());
        }
    }
    private IEnumerator SlideOutBackgroundHide()
    {
        float time = Time.time;
        while (Mathf.Abs(anim.targetPosition.x - transform.localPosition.x) > 0.2f)
        {
            yield return null;
            if (Time.time - time > 1f)
            {
                yield break;
            }
        }
        SetBackgroundShowing(false);
    }

    private void SetBackgroundShowing(bool show)
    {
        Debug.Log("Setting Background showing: " + show);
        infoObject.SetActive(show);
        objectivesPanel.SetActive(show);
        backgroundImage.enabled = show;
    }
    public bool IsHiddenTemporary()
    {
        return hiddenTemporary;
    }

    public void HideTemporary()
    {
        hiddenTemporary = true;
    }

    void Setup()
    {
        StopScrollCoroutine();

        SetRewardPanel();
        SetInfoButton();
        SetTexts();
        SetReward();
        SetObjectives();
        SetHeight();
    }

    void SetTexts()
    {
        if (!rewardPanel.activeSelf)
            if (ReferenceHolder.Get().objectiveController.IsDynamicObjective())
                titleText.text = string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_LIST_TITLE"), ReferenceHolder.Get().objectiveController.ToDoCounter);
            else
                titleText.text = string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOALS_TITLE"), Game.Instance.gameState().GetHospitalLevel());
        else
        {
            titleText.text = I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_COMPLETED_TITLE");
            rewardTitleText.text = I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_COMPLETED").Replace("{0}", ReferenceHolder.Get().objectiveController.ToDoCounter.ToString());
            rewardText.text = I2.Loc.ScriptLocalization.Get("CINEMA_CLAIM");
        }
    }

    void SetReward()
    {
        // rewardAmountText.text = ReferenceHolder.Get().objectiveController.ObjectivesReward.ToString();
    }

    void SetInfoButton()
    {
        infoObject.SetActive(false);

        if (!rewardPanel.activeSelf)
        {
            if (ReferenceHolder.Get().objectiveController.IsDynamicObjective())
                infoObject.SetActive(true);
        }
    }

    public void SetObjectives()
    {
        if (objectivesList != null)
        {
            for (int i = 0; i < objectivesList.transform.childCount; i++)
                Destroy(objectivesList.transform.GetChild(i).gameObject);
        }

        if (!rewardPanel.activeSelf)
        {
            List<Objective> lvlObjectives = ReferenceHolder.Get().objectiveController.GetAllObjectives();

            if (lvlObjectives != null && lvlObjectives.Count > 0)
            {
                int notClaimedIndex = -1;
                int notFinishedIndex = -1;

                for (int i = 0; i < lvlObjectives.Count; i++)
                {
                    ObjectiveUI objectiveUI = GameObject.Instantiate(UIObjectivePrefab).GetComponent<ObjectiveUI>();

                    objectiveUI.transform.SetParent(objectivesList.transform);
                    objectiveUI.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    objectiveUI.gameObject.SetActive(true);
                    objectiveUI.Setup(lvlObjectives[i]);

                    if (!lvlObjectives[i].IsCompleted && notFinishedIndex == -1)
                        notFinishedIndex = i;

                    if (lvlObjectives[i].IsCompleted && !lvlObjectives[i].isRewardClaimed && notClaimedIndex == -1)
                        notClaimedIndex = i;
                }

                if (scrollCoroutine != null)
                {
                    Timing.KillCoroutine(scrollCoroutine.GetType());
                    scrollCoroutine = null;
                }

                if (lvlObjectives.Count > 3)
                {
                    if (notClaimedIndex != -1)
                        scrollCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(notClaimedIndex, lvlObjectives.Count));
                    else
                        scrollCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(notFinishedIndex, lvlObjectives.Count));
                }
                else
                    scrollCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(0, lvlObjectives.Count));
            }
        }
    }

    void SetHeight()
    {
        float activeGoals = GetActiveObjectivesCount();

        if (ReferenceHolder.Get().objectiveController.IsDynamicObjective() && ReferenceHolder.Get().objectiveController.ObjectivesCompletedAndClaimed)
            activeGoals = 5;

        if (ExtendedCanvasScaler.isPhone() || ExtendedCanvasScaler.HasNotch())
        {
            if (activeGoals > 3)
            {
                if (ExtendedCanvasScaler.CalcScreenRatioWtoH() > 1.8)
                {
                    activeGoals = 3f;
                }
                else
                {
                    activeGoals = 3.5f;
                }
            }
        }
        else
        {
            if (activeGoals > 4)
                activeGoals = 4.5f;
        }

        float width = rect.sizeDelta.x;
        float height = 50 + 55 * activeGoals;
        rect.sizeDelta = new Vector3(width, height);
    }

    public void SetRewardPanel()
    {
        rewardPanel.SetActive(false);
        infoObject.SetActive(false);

        if (ReferenceHolder.Get().objectiveController.IsDynamicObjective())
        {
            if (HasAllRewardClaimed())
            {
                claimed = false;
                rewardPanel.SetActive(true);
                //TutorialUIController.Instance.BlinkImage(rewardButton.GetComponent<Image>(), 1.1f);
            }
            else
            {
                infoObject.SetActive(true);
            }
        }
    }

    IEnumerator<float> CenterToItemCoroutine(int objectiveIndex, int size)
    {
        if (objectiveIndex == -1)
            objectiveIndex = 0;

        float targetPos = 1 - ((float)objectiveIndex / size);
        targetPos = Mathf.Clamp(targetPos, 0f, 1f);
        float timer = 0f;

        scrollRect.verticalNormalizedPosition = 0;

        if (scrollRect.verticalNormalizedPosition == targetPos)
            yield break;

        while (true)
        {
            timer += Time.deltaTime;

            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPos, .1f);

            if (timer > 5f || Mathf.Abs(scrollRect.verticalNormalizedPosition - targetPos) < .001f)
            {
                scrollRect.verticalNormalizedPosition = targetPos;
                break;
            }
            yield return 0f;
        }

        scrollRect.velocity = Vector2.zero;
    }

    private bool HasAllRewardClaimed()
    {
        List<Objective> lvlGoals = ReferenceHolder.Get().objectiveController.GetAllObjectives();
        bool areAllClaimed = true;

        if (lvlGoals != null && lvlGoals.Count > 0)
        {
            for (int i = 0; i < lvlGoals.Count; i++)
            {
                if (!lvlGoals[i].isRewardClaimed)
                {
                    areAllClaimed = false;
                    break;
                }
            }
        }

        return areAllClaimed;
    }

    private bool AllObjectivesCompleted()
    {
        List<Objective> lvlGoals = ReferenceHolder.Get().objectiveController.GetAllObjectives();
        bool areAllClaimed = true;

        if (lvlGoals != null && lvlGoals.Count > 0)
        {
            for (int i = 0; i < lvlGoals.Count; i++)
            {
                if (!lvlGoals[i].isRewardClaimed)
                {
                    areAllClaimed = false;
                    break;
                }
            }
        }

        return areAllClaimed;
    }

    public void UpdateProgress()
    {
        if (isSlidIn)
        {
            for (int i = 0; i < objectivesList.transform.childCount; i++)
            {
                ObjectiveUI objectiveUI = objectivesList.transform.GetChild(i).GetComponent<ObjectiveUI>();
                objectiveUI.UpdateProgress();
            }
        }
    }

    private void StopScrollCoroutine()
    {
        if (scrollCoroutine != null)
        {
            Timing.KillCoroutine(scrollCoroutine.GetType());
            scrollCoroutine = null;
        }
    }

    int GetActiveObjectivesCount()
    {
        int activeObjectives = 0;
        for (int i = 0; i < objectivesList.transform.childCount; i++)
        {
            ObjectiveUI objectiveUI = objectivesList.transform.GetChild(i).GetComponent<ObjectiveUI>();
            if (objectiveUI.isActiveAndEnabled)
                activeObjectives++;
        }
        return activeObjectives;
    }

    public void ButtonToggle()
    {
        ObjectiveController objectiveController = ReferenceHolder.Get().objectiveController;
        if (objectiveController.IsTODOsActive() && objectiveController.ObjectivesCompleted && !objectiveController.ObjectivesCompletedAndClaimed)
        {
            if (isSlidIn)
            {
                SlideOut();
            }
            else
            {
                ClaimHiddenUnclaimedRewards();
                SlideIn();
            }
            return;
        }
        if (isSlidIn)
            SlideOut();
        else
            SlideIn();

        objectiveController = null;
    }

    public void ButtonClaim()
    {
        if (!claimed)
        {
            Canvas canvas = UIController.get.canvas;
            Vector2 startPoint = new Vector2((rewardButton.transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (rewardButton.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);

            GameObject particle = Instantiate(ParticleReward, rewardButton.transform.position, Quaternion.identity, UIController.get.transform);
            Destroy(particle, 4);
            SlideOut();

            ReferenceHolder.Get().objectiveController.UpdateObjectives();

            ReferenceHolder.Get().objectiveController.CollectObjectivesListReward(startPoint);
            claimed = true;

            SetListCompletedBadgeActive(false);
        }
    }

    public void SlideOutWithCoroutine()
    {
        StartCoroutine(SlideOutCoroutine());
    }

    IEnumerator SlideOutCoroutine()
    {
        yield return new WaitForSeconds(2.5f);
        SlideOut();
    }

    public void ClaimUnclaimedRewards()
    {
        List<int> unclaimedRewards = new List<int>();

        for (int i = 0; i < objectivesList.transform.childCount; i++)
        {
            ObjectiveUI objectiveUI = objectivesList.transform.GetChild(i).GetComponent<ObjectiveUI>();
            if (!objectiveUI.IsRewardClaimed() && objectiveUI.IsCompleted())
                unclaimedRewards.Add(i);
        }

        if (unclaimedRewards.Count > 0)
            StartCoroutine(ClaimUnclaimedRewards(unclaimedRewards));
        else
            SlideOut();
    }

    public void ClaimHiddenUnclaimedRewards()
    {
        List<int> unclaimedRewards = new List<int>();

        for (int i = 0; i < objectivesList.transform.childCount; i++)
        {
            ObjectiveUI objectiveUI = objectivesList.transform.GetChild(i).GetComponent<ObjectiveUI>();
            if (!objectiveUI.IsRewardClaimed() && objectiveUI.IsCompleted())
                unclaimedRewards.Add(i);
        }

        if (unclaimedRewards.Count > 0)
            StartCoroutine(ClaimHiddenUnclaimedRewards(unclaimedRewards));
        else
            SlideOut();
    }

    IEnumerator ClaimUnclaimedRewards(List<int> unclaimedRewards)
    {
        SlideIn();
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < unclaimedRewards.Count; i++)
        {
            if (unclaimedRewards[i] < objectivesList.transform.childCount)
            {
                ObjectiveUI objectiveUI = objectivesList.transform.GetChild(unclaimedRewards[i]).GetComponent<ObjectiveUI>();
                objectiveUI.ButtonReward();
            }
        }

        yield return new WaitForSeconds(2.5f);
        SlideOut();
    }

    IEnumerator ClaimHiddenUnclaimedRewards(List<int> unclaimedRewards)
    {
        for (int i = 0; i < unclaimedRewards.Count; i++)
        {
            if (unclaimedRewards[i] < objectivesList.transform.childCount)
            {
                ObjectiveUI objectiveUI = objectivesList.transform.GetChild(unclaimedRewards[i]).GetComponent<ObjectiveUI>();
                objectiveUI.CollectReward(false);
            }
        }

        yield return new WaitForSeconds(2.5f);
    }

    public void ButtonInfo()
    {
        StartCoroutine(UIController.get.objectivesInfoPopup.Open());
    }

    public void SetTaskCompletedBadgeActive(bool setActive)
    {
        if (taskCompletedBadge == null)
        {
            Debug.LogError("taskCompletedBadge is null");
            return;
        }
        taskCompletedBadge.SetActive(setActive);
    }

    public void SetListCompletedBadgeActive(bool setActive)
    {
        if (listCompletedBadge == null)
        {
            Debug.LogError("listCompletedBadge is null");
            return;
        }
        Animator anim = goalButtonIcon.GetComponent<Animator>();
        if (!listCompletedBadge.activeInHierarchy && setActive)
        {
            anim.SetTrigger("Completed");
        }
        if (listCompletedBadge.activeInHierarchy && !setActive)
        {
            anim.SetTrigger("HideArrow");
        }
    }

    public void TaskCompletedUpdateListUI()
    {
        if (isSlidIn)
            TaskCompletedUpdateListUIWhenIsShown();
        else
            TaskCompletedUpdateListUIWhenIsHidden();
    }

    private void TaskCompletedUpdateListUIWhenIsShown()
    {
        if (ReferenceHolder.Get().objectiveController.IsTODOsActive())
            TaskCompletedUpdateToDoListWhenIsShown();
        else
            TaskCompletedUpdateLevelGoalList();
    }

    private void TaskCompletedUpdateListUIWhenIsHidden()
    {
        if (ReferenceHolder.Get().objectiveController.IsTODOsActive())
            TaskCompletedUpdateToDoListWhenIsHidden();
        else
            TaskCompletedUpdateLevelGoalList();
    }

    private void TaskCompletedUpdateToDoListWhenIsShown()
    {
        if (ReferenceHolder.Get().objectiveController.ObjectivesCompleted)
        {
            autoSlideIn = true;
            ClaimUnclaimedRewards();
        }
    }

    public void AutoSlideInAfterListComplete()
    {
        if (autoSlideIn)
        {
            autoSlideIn = false;
            SetListCompletedBadgeActive(true);
            SlideIn();
        }
    }

    private void TaskCompletedUpdateToDoListWhenIsHidden()
    {
        if (ReferenceHolder.Get().objectiveController.ObjectivesCompleted)
        {
            SetTaskCompletedBadgeActive(false);
            SetListCompletedBadgeActive(true);
        }
        else
        {
            ReferenceHolder.Get().objectiveController.SetRewardSeen(false);
            SetTaskCompletedBadgeActive(true);
        }
    }

    private void TaskCompletedUpdateLevelGoalList()
    {
        SlideIn();
    }
}