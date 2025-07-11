using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using SimpleUI;
using Hospital;

public class EventContentPersonal : MonoBehaviour
{
#pragma warning disable 0649
    [Header("Flipchart")]
    [SerializeField] TextMeshProUGUI flipchartTitleText;
    [SerializeField] TextMeshProUGUI flipchartDescriptionText;

    [Header("Progress Bar")]
    [SerializeField] RectTransform fillBar;
    [SerializeField] RectTransform fillBarEndSlice;
    [SerializeField] RectTransform progressTooltip;
    [SerializeField] TextMeshProUGUI personalProgressText;
    [SerializeField] Image itemIconFillBar;
    /*[SerializeField]*/ EventSubGoalIndicator[] subGoals;
    [SerializeField] EventSubGoalIndicatorManager subGoalManager;

    [Header("Activity")]
    Sprite activityEventSprite;
    [SerializeField] GameObject activityPanel;
    [SerializeField] Image itemIconActivity;
    [SerializeField] Sprite pumpkinIconActivity;
    [SerializeField] GameObject ActivityArt;
    [SerializeField] Sprite treatmentRoomEventGraphic;
    [SerializeField] GameObject PumpkinArt;
    [SerializeField] GameObject treatmentRoomAdditionalGraphicsWrapper;

    [Header("Contribution")]
    [SerializeField] GameObject contributionPanel;
    [SerializeField] GameObject contributionHover;
    [SerializeField] RectTransform contributionBox;
    [SerializeField] TextMeshProUGUI[] timerText;
    [SerializeField] GameObject tapToAdd;
    [SerializeField] TextMeshProUGUI contributeAmountText;
    [SerializeField] TextMeshProUGUI contributeAttractText;
    [SerializeField] Button[] buttons;
    [SerializeField] Image itemIconHover;
    [SerializeField] Image itemIconContribute;
    [SerializeField] TextMeshProUGUI itemTooltipText;
    [SerializeField] ScrollRect scroll;
#pragma warning restore 0649
    [Header("Extended Info")]
#pragma warning disable 0414
    [SerializeField] private TextMeshProUGUI eventExtendedInfo = null;
#pragma warning restore 0414
    [SerializeField] private Animator eventExtendedInfoAnim = null;

    #region Images For Which Sprites Are Loaded FromResources
    [Header("Images for Tablet which sprites will be applied form AssetBundles")]
    public Image Table;
    public Image BackgroundImage;
    public Image PersonalBarOutline;
    public Image GlowHorizontalLeft;
    public Image GlowHorizontalMain;
    public Image HorizontalSlice;
    public Image Indicator;
    public Image PersonalBarFill;
    #endregion

    public Transform HamsterParent;

    private Dictionary<GlobalEventAssetBundleModule.SpriteName, Image> mapOfImages;
    private const string anyDocEventTag = "AnyDoc";
    private const string treatmentRoomEventTag = "2xBedsRoom";
    private const string defaultAnyDocSpriteTag = "RedDoc";

    float progressStartPosX = 75;
    float progressEndPosX = 639;    //max width of BarFillMask object

    int contributeAmount = 0;

    Coroutine fillCoroutine = null;
    Coroutine timerCoroutine = null;
    Coroutine scrollCoroutine = null;

    Dictionary<string, ShopRoomInfo> docActivityPopupSprites;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Show()
    {
        if (mapOfImages == null)
        {
            mapOfImages = new Dictionary<GlobalEventAssetBundleModule.SpriteName, Image>()
            {
                {GlobalEventAssetBundleModule.SpriteName.Table,Table },
                {GlobalEventAssetBundleModule.SpriteName.PersonalCityBackground,BackgroundImage },
                {GlobalEventAssetBundleModule.SpriteName.PersonalBarOutline,PersonalBarOutline },
                {GlobalEventAssetBundleModule.SpriteName.GlowPersonal,GlowHorizontalLeft },
                {GlobalEventAssetBundleModule.SpriteName.GlowPersonal2,GlowHorizontalMain },
                {GlobalEventAssetBundleModule.SpriteName.PersonalHorizontalSlice,HorizontalSlice },
                {GlobalEventAssetBundleModule.SpriteName.PersonalIndicator,Indicator },
                {GlobalEventAssetBundleModule.SpriteName.PersonalBarFill,PersonalBarFill }
            };
        }
        GameAssetBundleManager.instance.globalEvents.LoadResources(onSuccessLoad, onFailureLoad);
        gameObject.SetActive(true);

        SetDescription();
        SetEventType();
        SetBarOnOpen();

        SetSubGoalIndicators(false);

        tapToAdd.SetActive(true);
        contributeAmount = 0;
        SetContributionAmountText();
        try
        { 
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        timerCoroutine = StartCoroutine(CountTimer());
        if (scrollCoroutine == null)
        {
            scrollCoroutine = StartCoroutine(ScrollBarCoroutine());
        }
    }

    private IEnumerator ScrollBarCoroutine()
    {
        scroll.verticalNormalizedPosition = 0;
        yield return new WaitForSeconds(.25f);
        while (scroll.verticalNormalizedPosition <= 0.97)
        {
            scroll.verticalNormalizedPosition = Mathf.Lerp(scroll.verticalNormalizedPosition, 1, .1f);
            yield return null;
        }
        scroll.verticalNormalizedPosition = 1.0f;
        scrollCoroutine = null;
    }

    private void onFailureLoad(Exception exception)
    {
        Debug.LogError("Resources could not be loaded. " + exception.Message);
    }

    private void onSuccessLoad()
    {
        //Get all assets for Hamster character
        GameAssetBundleManager.instance.globalEvents.GetGameObject(GlobalEventAssetBundleModule.GameObjectName.Hamster,
            (hamster) =>
            {
                //In future update we will put here graying out the hamster. Now it is not needed.
            },
            (ex) =>
            {
                Debug.LogError("There was problem in loading Marie gameObject. " + ex.Message);
            },
            HamsterParent);

        foreach (KeyValuePair<GlobalEventAssetBundleModule.SpriteName, Image> item in mapOfImages)
        {
            GameAssetBundleManager.instance.globalEvents.GetSprite(item.Key,
        (sprite) =>
        {
            item.Value.sprite = sprite;
            if (item.Key == GlobalEventAssetBundleModule.SpriteName.Table || item.Key == GlobalEventAssetBundleModule.SpriteName.PersonalCityBackground || item.Key == GlobalEventAssetBundleModule.SpriteName.PersonalBarOutline || item.Key == GlobalEventAssetBundleModule.SpriteName.PersonalBarFill)
            {
                item.Value.type = Image.Type.Sliced;
            }
            else
            {
                item.Value.preserveAspect = true;
            }
        },
        (ex) =>
        {
            Debug.LogError("Sprite for " + item.Value + " Image could not be loaded. " + ex.Message);
        });
        }
    }

    private void SetBarOnOpen()
    {
        int numberOfPersonalGoals = ReferenceHolder.GetHospital().globalEventController.PersonalGoals.Length;
        int numberOfPersonalRewards = ReferenceHolder.GetHospital().globalEventController.PersonalGoalRewards.Count;
        subGoals = subGoalManager.GetSubGoals(Mathf.Max(0, Mathf.Min(numberOfPersonalGoals, numberOfPersonalRewards)));

        int currentPersonalProgress = ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress;
        int maxPersonalProgress = ReferenceHolder.GetHospital().globalEventController.PersonalGoals[subGoals.Length - 1];
        int nextPersonalProgress = GetNextPersonalGoalProgress(currentPersonalProgress);
        int prevPersonalProgress = GetPrevPersonalGoalProgress(currentPersonalProgress);
        int currentPersonalGoalIndex = GetCurrentPersonalGoalIndex(currentPersonalProgress);

        //currentPersonalProgress = 620;
        float step = GetStep();
        float ratio = Mathf.Clamp(step * currentPersonalGoalIndex + ((float)(currentPersonalProgress - prevPersonalProgress) / Math.Max(nextPersonalProgress - prevPersonalProgress, 1)) * step, 0f, 1f);
        float xPos = Mathf.Lerp(progressStartPosX, progressEndPosX, ratio);
        fillBar.sizeDelta = new Vector2(xPos, fillBar.sizeDelta.y);
        fillBarEndSlice.anchoredPosition = new Vector2(xPos + 10.88f, fillBarEndSlice.anchoredPosition.y); // difference due to setup on scene
        progressTooltip.anchoredPosition = new Vector2(xPos + 8f, progressTooltip.anchoredPosition.y);

        if (currentPersonalProgress > nextPersonalProgress)
        {
            personalProgressText.text = string.Format("{0}", currentPersonalProgress);
        }
        else
        {
            personalProgressText.text = string.Format("{0} / {1}", Mathf.Clamp(currentPersonalProgress, 0, nextPersonalProgress), nextPersonalProgress);
        }
    }

    float GetStep()
    {
        return 1 / ((float)subGoals.Length); //(float)ReferenceHolder.GetHospital().globalEventController.PersonalGoals.Length;
    }

    public int GetCurrentPersonalGoalIndex(int currentProgress)
    {
        int[] goals = ReferenceHolder.GetHospital().globalEventController.PersonalGoals;

        for (int i = goals.Length - 1; i >= 0; --i)
        {
            if (goals[i] <= currentProgress)
                return i + 1;
        }
        return 0;
    }

    int GetPrevPersonalGoalProgress(int currentProgress)
    {
        int[] goals = ReferenceHolder.GetHospital().globalEventController.PersonalGoals;

        for (int i = goals.Length - 1; i >= 0; --i)
        {
            if (goals[i] <= currentProgress)
                return goals[i];
        }
        return 0;
    }

    int GetNextPersonalGoalProgress(int currentProgress)
    {
        int[] goals = ReferenceHolder.GetHospital().globalEventController.PersonalGoals;
        
        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i] > currentProgress)
                return goals[i];
        }

        return goals.Last();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (scrollCoroutine != null)
        {
            scrollCoroutine = null;
        }
    }

    void SetDescription()
    {
        flipchartTitleText.text = ReferenceHolder.GetHospital().globalEventController.GlobalEventTitle;
        flipchartDescriptionText.text = ReferenceHolder.GetHospital().globalEventController.GlobalEventDescription;
    }

    void SetEventType()
    {
        switch (ReferenceHolder.GetHospital().globalEventController.GlobalEventType)
        {
            case GlobalEvent.GlobalEventType.Default:
                break;
            case GlobalEvent.GlobalEventType.Contribution:
                SetContribution();
                break;
            case GlobalEvent.GlobalEventType.Activity:
                SetActivity();
                break;
            default:
                break;
        }
    }

    void SetContribution()
    {
        /*Sprite sprite = ReferenceHolder.GetHospital().globalEventController.GetSpriteForContributionItem();
        itemIconFillBar.sprite = sprite;
        itemIconHover.sprite = sprite;
        itemIconContribute.sprite = sprite;

        itemIconContribute.gameObject.SetActive(false);
        SetButtonsInteractable(false);
        contributionPanel.SetActive(true);
        activityPanel.SetActive(false);
        SetContributionHoverActive(false);
        contributeAttractText.gameObject.SetActive(true);
        itemIconFillBar.gameObject.SetActive(true);
        itemIconActivity.gameObject.SetActive(false);
        */
    }

    void SetActivity()
    {
        itemIconFillBar.gameObject.SetActive(false);

        // do stuff on obj, for example you can get sprite, info etc. / for future

        itemIconActivity.gameObject.SetActive(true);

        ActivityArt.SetActive(false);
        PumpkinArt.SetActive(false);
        contributionPanel.SetActive(false);
        activityPanel.SetActive(true);

        ActivityGlobalEvent.ActivityArt artType = ActivityGlobalEvent.ActivityArt.Default;
        ActivityGlobalEvent.ActivityType activityType = ActivityGlobalEvent.ActivityType.Default;

        var obj = ReferenceHolder.GetHospital().globalEventController.GetGlobalActivityItem(out activityType, out artType);

        if (docActivityPopupSprites == null)
        {
            docActivityPopupSprites = new Dictionary<string, ShopRoomInfo>();
        }

        if (docActivityPopupSprites.Count == 0)
        {
            var list = HospitalAreasMapController.HospitalMap.drawerDatabase.DrawerItems;
            list.ForEach((x) => { if (x.dummyType == BuildDummyType.DoctorRoom) docActivityPopupSprites.Add(x.Tag, x); });
        }

        if (artType != ActivityGlobalEvent.ActivityArt.Default)
        {    
            PumpkinArt.SetActive(true);
        }
        else
        {
            ActivityArt.SetActive(true);
            if (activityType == ActivityGlobalEvent.ActivityType.HealPatientInDoctorRoom)
            {
                treatmentRoomAdditionalGraphicsWrapper.SetActive(false);

                string tag = GlobalEventParser.Instance.CurrentGlobalEventConfig.OtherParameters.RotatableTag;
                if (tag.Equals(anyDocEventTag))
                {
                    ActivityArt.GetComponent<Image>().sprite = docActivityPopupSprites[defaultAnyDocSpriteTag].ShopImage;
                }
                else
                {
                    ShopRoomInfo info = null;
                    if (docActivityPopupSprites.TryGetValue(tag, out info))
                        ActivityArt.GetComponent<Image>().sprite = info.ShopImage;
                    else
                        Debug.LogError("Unrecognized tag in the event string");
                }
            }
            else
            {
                ActivityArt.GetComponent<Image>().sprite = treatmentRoomEventGraphic;
                treatmentRoomAdditionalGraphicsWrapper.SetActive(true);
            }
        }
    }

    void SetSubGoalIndicators(bool isFilling)
    {
        var unlockedGoals = ReferenceHolder.GetHospital().globalEventController.GetAllUnlockedPersonalGoals();
        var activeGoals = ReferenceHolder.GetHospital().globalEventController.PersonalGoals;

        for (int i = 0; i < subGoals.Length; i++)
            subGoals[i].gameObject.SetActive(false);

        for (int i = 0; i < subGoals.Length; i++)
        {
            if (i < activeGoals.Length)
            {
                float goalPos = Mathf.Lerp(progressStartPosX, progressEndPosX, (float)(i + 1) / subGoals.Length);
                bool hasLine = i + 1 != subGoals.Length; //activeGoals.Length;
                bool unlocksGlobal = ReferenceHolder.GetHospital().globalEventController.DoesGoalUnlockGlobal(activeGoals[i]);
                float unlockRequirements = i * subGoals.Length / 1.0f;

                if (i >= unlockedGoals.Count)
                    subGoals[i].Setup(unlockRequirements, unlocksGlobal, false, null, goalPos, hasLine, activeGoals[i], isFilling);
                else
                    subGoals[i].Setup(unlockRequirements, unlocksGlobal, true, unlockedGoals[i].GetGlobalEventGift().GetMainImageForGift() /*GetSprite()*/, goalPos, hasLine, activeGoals[i], isFilling);
            }
        }
    }

    void SetButtonsInteractable(bool isInteractable)
    {
        Material grayscaleMaterial = ResourcesHolder.Get().GrayscaleMaterial;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = isInteractable;
            buttons[i].image.material = isInteractable ? null : grayscaleMaterial;
        }
    }

    private void SetContributionHoverActive(bool setActive)
    {
        contributionHover.SetActive(setActive);
        if (contributionPanel.GetComponent<Image>() != null)
        {
            contributionPanel.GetComponent<Image>().raycastTarget = setActive;
        }
    }

    IEnumerator CountTimer()
    {
        int timeLeft = 0;
        while (true)
        {
            timeLeft = ReferenceHolder.GetHospital().globalEventController.GetCurrentGlobalEventEndTime - (int)ServerTime.getTime();
            SetTimeLeft(timeLeft);
            yield return new WaitForSeconds(1.0f);
        }
    }

    void SetTimeLeft(int secondsRemaining)
    {
        if (contributionPanel.activeSelf)
            timerText[0].text = UIController.GetFormattedShortTime(secondsRemaining);
        else if (activityPanel.activeSelf)
            timerText[1].text = UIController.GetFormattedShortTime(secondsRemaining);
    }

    public void ButtonContributeBox()
    {
        if (!itemIconContribute.gameObject.activeSelf)
        {
            SetContributionHoverActive(!contributionHover.activeSelf);
            tapToAdd.SetActive(!contributionHover.activeSelf);
            contributeAttractText.gameObject.SetActive(!contributionHover.activeSelf);
            int availableAmount = ReferenceHolder.GetHospital().globalEventController.GetAmountOfAvailableContributeResources();
            itemTooltipText.text = availableAmount.ToString();

            if (availableAmount <= 0)
                itemIconHover.material = ResourcesHolder.Get().GrayscaleMaterial;
            else
                itemIconHover.material = null;
        }
    }

    public void ButtonDeclickHover()
    {
        if (contributionHover.activeSelf)
        {
            SetContributionHoverActive(false);
            contributeAttractText.gameObject.SetActive(!contributionHover.activeSelf);
            tapToAdd.SetActive(true);
        }
    }


    bool draggingItem = false;
    public void PickupContributionItem()
    {
        if (ReferenceHolder.GetHospital().globalEventController.GetAmountOfAvailableContributeResources() <= 0)
            MessageController.instance.ShowMessage(2);
        else
            draggingItem = true;
    }

    public void DropContributionItem()
    {
        bool isOverContributeBox = false;
        if (RectTransformUtility.RectangleContainsScreenPoint(contributionBox, Input.mousePosition))
            isOverContributeBox = true;

        if (isOverContributeBox && ReferenceHolder.GetHospital().globalEventController.GetAmountOfAvailableContributeResources() > 0)
        {
            contributeAmount = 1;
            itemIconContribute.gameObject.SetActive(true);
            SetContributionHoverActive(false);
            SetButtonsInteractable(true);
            SetContributionAmountText();
            SoundsController.Instance.PlayDecoSelect();
        }
        draggingItem = false;
        itemIconHover.rectTransform.anchoredPosition = new Vector2(0.4f, 15.4f);
    }

    void Update()
    {
        CheckQuickPlusMinus();
        if (draggingItem)
            itemIconHover.transform.position = Input.mousePosition;
    }

    void SetContributionAmountText()
    {
        if (contributeAmount > 0)
            contributeAmountText.text = "x" + contributeAmount;
        else
            contributeAmountText.text = "";
    }

    public void ButtonContribute()
    {
        Debug.LogError("Contribute clicked");
        int pastPersonalProgress = ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress;
        ReferenceHolder.GetHospital().globalEventController.IncrementContributionGoal(contributeAmount);

        Vector2 startPoint = new Vector2((itemIconContribute.transform.position.x - Screen.width / 2) / UIController.get.canvas.transform.localScale.x, (itemIconContribute.transform.position.y - Screen.height / 2) / UIController.get.canvas.transform.localScale.y);

        ReferenceHolder.Get().giftSystem.SetUIElementsPos();
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.GlobalEventPersonal, startPoint,
            contributeAmount, 0f, 1.5f, Vector3.one * 1.5f, Vector3.one * 2f, itemIconHover.sprite, null, null);

        contributeAmount = 0;
        tapToAdd.SetActive(true);
        itemIconContribute.gameObject.SetActive(false);
        SetContributionHoverActive(false);
        SetButtonsInteractable(false);
        SetContributionAmountText();

        try
        { 
            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        int currentPersonalProgress = ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress;
        int maxPersonalProgress = ReferenceHolder.GetHospital().globalEventController.PersonalGoals[ReferenceHolder.GetHospital().globalEventController.PersonalGoals.Length - 1];
        int nextPersonalProgress = GetNextPersonalGoalProgress(currentPersonalProgress);

        fillCoroutine = StartCoroutine(Fill(pastPersonalProgress, currentPersonalProgress, maxPersonalProgress));
        if (currentPersonalProgress > nextPersonalProgress)
        {
            personalProgressText.text = string.Format("{0}", currentPersonalProgress);
        }
        else
        {
            personalProgressText.text = string.Format("{0} / {1}", Mathf.Clamp(currentPersonalProgress, 0, nextPersonalProgress), nextPersonalProgress);
        }
        contributeAttractText.gameObject.SetActive(!contributionHover.activeSelf);
    }

    IEnumerator Fill(float lastProgress, float currentProgress, float maxProgress)
    {
        float fillDuration = 1f;
        float fillTimer = 0f;
        float step = GetStep();
        int nextPersonalProgress = GetNextPersonalGoalProgress((int)currentProgress);
        int prevPersonalProgress = GetPrevPersonalGoalProgress((int)currentProgress);
        int currentPersonalGoalIndex = GetCurrentPersonalGoalIndex((int)currentProgress);

        float ratio = Mathf.Clamp(step * currentPersonalGoalIndex + ((float)(currentProgress - prevPersonalProgress) / Math.Max(nextPersonalProgress - prevPersonalProgress, 1)) * step, 0f, 1f);

        int nextLastPersonalProgress = GetNextPersonalGoalProgress((int)lastProgress);
        int prevLastPersonalProgress = GetPrevPersonalGoalProgress((int)lastProgress);
        int PersonalLastGoalIndex = GetCurrentPersonalGoalIndex((int)lastProgress);

        float startRatio = Mathf.Clamp(step * PersonalLastGoalIndex + ((float)(lastProgress - prevLastPersonalProgress) / Math.Max(nextLastPersonalProgress - prevLastPersonalProgress, 1)) * step, 0f, 1f);

        while (fillTimer < fillDuration)
        {
            SetBar(Mathf.Lerp(startRatio, ratio, fillTimer / fillDuration));

            fillTimer += Time.deltaTime;
            yield return null;
        }

        SetBar(ratio);
        SetSubGoalIndicators(true);
        SetContribution();
    }

    void SetBar(float progress)
    {
        float xPos = Mathf.Lerp(progressStartPosX, progressEndPosX, progress);
        fillBar.sizeDelta = new Vector2(xPos, fillBar.sizeDelta.y);
        fillBarEndSlice.anchoredPosition = new Vector2(xPos + 10.88f, fillBarEndSlice.anchoredPosition.y);  //10.8f is offset for end slice position, due to hierarchy and stuff      
        progressTooltip.anchoredPosition = new Vector2(xPos + 8f, progressTooltip.anchoredPosition.y);
    }

    public Vector2 GetRewardPosition(int goal)
    {
        Vector2 startPoint = Vector2.zero;

        for (int i = 0; i < subGoals.Length; i++)
        {
            if ((int)subGoals[i].goal == goal)
                return new Vector2((subGoals[i].rewardIcon.transform.transform.position.x - Screen.width / 2) / UIController.get.canvas.transform.localScale.x,
                                   (subGoals[i].rewardIcon.transform.transform.position.y - Screen.height / 2) / UIController.get.canvas.transform.localScale.y);
        }
        Debug.LogError("Did not find a reward on UI which corresponds to given requirements " + goal);
        return Vector2.zero;
    }

    public Vector2 GetParticleStartPoint()
    {
        return new Vector2((itemIconContribute.transform.position.x - Screen.width / 2) / UIController.get.canvas.transform.localScale.x, (itemIconContribute.transform.position.y - Screen.height / 2) / UIController.get.canvas.transform.localScale.y);
    }

    bool plusDown;
    bool minusDown;
    float buttonDownTimer;

    public void PlusAmountDown()
    {
        plusDown = true;
        buttonDownTimer = 0;
        IncreaseAmount();
    }

    public void PlusAmountUp()
    {
        plusDown = false;
    }

    public void MinusAmountDown()
    {
        minusDown = true;
        buttonDownTimer = 0;
        DecreaseAmount();
    }

    public void MinusAmountUp()
    {
        minusDown = false;
    }

    void IncreaseAmount()
    {
        if (ReferenceHolder.GetHospital().globalEventController.GetAmountOfAvailableContributeResources() > contributeAmount && buttons[0].interactable)
        {
            GlobalEventController gec = ReferenceHolder.GetHospital().globalEventController;
            if (gec.CurrentGlobalEvent is MedicineContributeGlobalEvent)
            {
                MedicineContributeGlobalEvent gevent = gec.CurrentGlobalEvent as MedicineContributeGlobalEvent;
                int currentPersonalProgress = gec.GlobalEventPersonalProgress;
                MedicineRef medicine = gevent.Medicine;
                int progressIndex = GetCurrentPersonalGoalIndex(currentPersonalProgress);
                if (currentPersonalProgress + contributeAmount < GetNextPersonalGoalProgress(currentPersonalProgress) ||
                    progressIndex >= gec.PersonalGoals.Length - 1)
                {
                    contributeAmount++;
                    SetContributionAmountText();
                }
            }
            else
            {
                contributeAmount++;
                SetContributionAmountText();
            }
        }
    }

    void DecreaseAmount()
    {
        if (contributeAmount > 1 && buttons[0].interactable)
        {
            contributeAmount--;
            SetContributionAmountText();
        }
    }

    void CheckQuickPlusMinus()
    {
        if (!minusDown && !plusDown)
            return;

        buttonDownTimer += Time.deltaTime;
        if (buttonDownTimer >= .5f)
        {
            if (minusDown)
            {
                DecreaseAmount();
                buttonDownTimer -= .15f;
            }
            else if (plusDown)
            {
                IncreaseAmount();
                buttonDownTimer -= .15f;
            }
        }
    }

    public void OnInfoButtonDown()
    {
        eventExtendedInfoAnim.SetBool("Show", true);
    }

    public void OnInfoButtonUP()
    {
        eventExtendedInfoAnim.SetBool("Show", false);
    }
}