using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;
using Hospital;
using System.Collections;

public class BubbleBoyEntryOverlayUI : UIElement
{
#pragma warning disable 0649
    [SerializeField] private TextMeshProUGUI entryFreeText;
    [SerializeField] private TextMeshProUGUI entryTimerText;
    [SerializeField] private TextMeshProUGUI bubbleText;
    [SerializeField] private Animator cloud;
    [SerializeField] private BubbleBoyEntryOverlayController bubbleBoyEntryOverlayController;
    [SerializeField] private BubbleBoyFriendsFeedController bubbleBoyFriendsFeedController;
    [SerializeField] private FriendFeedUI friendFeedUI;
#pragma warning restore 0649
    [SerializeField] private GameObject entryContent = null;

#pragma warning disable 0649
    [SerializeField] Button exitButton;
    [SerializeField] private GameObject buttonFreeContent;
    [SerializeField] private GameObject buttonPriceContent;
#pragma warning restore 0649
    [SerializeField] public FriendFeedUI friendsFeed;
#pragma warning disable 0649
    [SerializeField] private Animator buttonPlayAnimator;
    [SerializeField] private RuntimeAnimatorController buttonPlayAnimDefault;
    [SerializeField] private RuntimeAnimatorController buttonPlayAnimBlinking;
#pragma warning restore 0649
    [SerializeField] private GameObject bubbleBoyMinigame = null;

#pragma warning disable 0649
    [SerializeField] private GameObject notRefundButtonContent;
    [SerializeField] private GameObject refundButtonContent;
    [SerializeField] private RectTransform ribbonRectTransform;
#pragma warning restore 0649
    private int entryFee = 0;
    private int currentTimeFeed;
    private bool refundAnimEnd = true;
    private bool playClicked = false;

    private bool wasInGameCloudVisible = false;
    private bool wasArrowGameCloudVisible = false;

    public IEnumerator Open(int entryFee = 0)
    {
        HideTutorialCloud();

        if (ExtendedCanvasScaler.HasNotch())
        {
            ribbonRectTransform.offsetMin = new Vector2(-50f, 22.98701f);
            ribbonRectTransform.offsetMax = new Vector2(50f, 22.98701f);
        }

        refundAnimEnd = true;
        playClicked = false;

        entryContent.SetActive(true);
        bubbleBoyMinigame.SetActive(false);

        notRefundButtonContent.SetActive(true);
        refundButtonContent.SetActive(false);

        if (BubbleBoyDataSynchronizer.Instance.RefundExist)
        {
            notRefundButtonContent.SetActive(false);
            refundButtonContent.SetActive(true);
        }

        //Debug.LogError("TutorialController.Instance.CurrentTutorialStepTag = " + TutorialController.Instance.CurrentTutorialStepTag);
        if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.level_7) && BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
            exitButton.gameObject.SetActive(false);
        else
            exitButton.gameObject.SetActive(true);

        buttonFreeContent.gameObject.SetActive(false);
        buttonPriceContent.gameObject.SetActive(false);

        // setFriendsFeed
        bubbleBoyFriendsFeedController.InitFriendsFeed(AccountManager.Instance.FbFriends);
        currentTimeFeed = bubbleBoyFriendsFeedController.timeToNextFeed;
        friendFeedUI.Setup(bubbleBoyFriendsFeedController.GetCurrentFriendFeed());

        Timing.KillCoroutine(UpdateTimeToNextFeed().GetType());
        Timing.RunCoroutine(UpdateTimeToNextFeed());

        // setEntryText etc.
        if (entryFee == 0)
        {
            buttonFreeContent.gameObject.SetActive(true);
            buttonPlayAnimator.runtimeAnimatorController = buttonPlayAnimBlinking;
        }
        else
        {
            buttonPriceContent.gameObject.SetActive(true);
            buttonPlayAnimator.runtimeAnimatorController = buttonPlayAnimDefault;
        }
        yield return null;

        SetEntryTimerText();
        this.entryFee = entryFee;
        entryFreeText.text = entryFee.ToString();
        Timing.RunCoroutine(UpdateTimeCoroutine());
        isBlockedGame = true;

        yield return base.Open(true, true);

        HospitalUIPrefabController.Instance.HideMainUI(false);
        UIController.get.CountersToFront();

        SetBubble();

        AnalyticsController.instance.ReportBubbleBoy(AnalyticsBubbleBoyAction.Open, entryFee == 0, entryFee, -1, -1, -1);
    }

    public void ButtonExit()
    {
        isBlockedGame = false;
        entryContent.SetActive(true);
        bubbleBoyMinigame.SetActive(false);
        Exit();
    }

    public void ButtonPlay()
    {
        if (IsVisible)
        {
            if (Game.Instance.gameState().GetCoinAmount() >= entryFee)
            {
                playClicked = true;

                exitButton.gameObject.SetActive(false);
                cloud.SetTrigger("Hide");

                GameState.Get().RemoveCoins(entryFee, EconomySource.BubbleBoy, refundAnimEnd);

                ReferenceHolder.Get().giftSystem.CreateItemUsed(Input.mousePosition, entryFee, 0f, ReferenceHolder.Get().giftSystem.particleSprites[0], false);

                bubbleBoyEntryOverlayController.EnterMinigame(entryFee);
                Timing.KillCoroutine(UpdateTimeCoroutine().GetType());
                entryContent.SetActive(false);
                bubbleBoyMinigame.SetActive(true);

                AchievementNotificationCenter.Instance.BubblePopped.Invoke(new TimedAchievementProgressEventArgs(1, Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds)));

                AnalyticsController.instance.ReportBubbleBoy(AnalyticsBubbleBoyAction.GameStarted, entryFee == 0, entryFee, -1, -1, -1);
            }
            else
            {
                UIController.get.BuyResourcesPopUp.Open(entryFee - Game.Instance.gameState().GetCoinAmount(), () =>
                {
                    ButtonPlay();

                }, null, BuyResourcesPopUp.missingResourceType.coin);
            }
        }
    }

    public void ButtonRefund()
    {
        if (BubbleBoyDataSynchronizer.Instance.RefundExist)
        {
            int refoundCoinsValue = BubbleBoyDataSynchronizer.Instance.RefundAmount;
            Canvas canvas = UIController.get.canvas;
            Vector2 startPoint = new Vector2((refundButtonContent.transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (refundButtonContent.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);
            int currentAmount = Game.Instance.gameState().GetCoinAmount();
            GameState.Get().AddResource(ResourceType.Coin, refoundCoinsValue, EconomySource.BubbleBoy, false);
            refundAnimEnd = false;
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, startPoint, refoundCoinsValue, 0f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), ResourcesHolder.GetHospital().bbCoinSprite, null, () =>
            {
                if (!playClicked)
                {
                    GameState.Get().UpdateCounter(ResourceType.Coin, refoundCoinsValue, currentAmount);
                }

                refundAnimEnd = true;
            });

            BubbleBoyDataSynchronizer.Instance.ResetRefund();
        }

        notRefundButtonContent.SetActive(true);
        refundButtonContent.SetActive(false);
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        Timing.KillCoroutine(UpdateTimeCoroutine().GetType());
        Timing.KillCoroutine(UpdateTimeToNextFeed().GetType());

        base.Exit(hidePopupWithShowMainUI);

        bubbleBoyMinigame.GetComponent<BubbleBoyMinigameUI>().isBlockedGame = false;
        bubbleBoyMinigame.GetComponent<BubbleBoyMinigameUI>().CloseMinigame();
        UIController.get.CountersToBack();

        if (BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable() && TutorialController.Instance.IsTutorialStepEqual(StepTag.bubble_boy_available))
            NotificationCenter.Instance.BubbleBoyAvailable.Invoke(new BaseNotificationEventArgs());

        if (UIController.get.ActivePopUps.Contains(UIController.getHospital.bubbleBoyMinigameUI))
            UIController.get.ActivePopUps.Remove(UIController.getHospital.bubbleBoyMinigameUI);

        ShowTutorialCloudAgain();
        NotificationCenter.Instance.BubbleBoyGameClosed.Invoke(null);
    }

    IEnumerator<float> UpdateTimeCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(1f);

            if (BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable() || BubbleBoyDataSynchronizer.Instance.TotalEntries == 0)
            {
                buttonFreeContent.gameObject.SetActive(true);
                buttonPriceContent.gameObject.SetActive(false);
                this.entryFee = 0;
            }
            SetEntryTimerText();
        }
    }

    public IEnumerator<float> UpdateTimeToNextFeed()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(1f);

            --currentTimeFeed;

            if (currentTimeFeed <= 0)
            {
                bubbleBoyFriendsFeedController.currentFeedVisibleID++;
                currentTimeFeed = bubbleBoyFriendsFeedController.timeToNextFeed;

                if (bubbleBoyFriendsFeedController.currentFeedVisibleID >= bubbleBoyFriendsFeedController.friendsFeedSize)
                    bubbleBoyFriendsFeedController.currentFeedVisibleID = 0;

                friendFeedUI.Setup(bubbleBoyFriendsFeedController.GetCurrentFriendFeed());
            }
        }
    }

    private void SetEntryTimerText()
    {
        if (BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
            entryTimerText.text = "";
        else
            entryTimerText.text = I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_FREE_IN") + " " + UIController.GetFormattedShortTime(BubbleBoyDataSynchronizer.Instance.NextFreeEntryDate - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds));
    }

    void SetBubble()
    {
        string bubbleText = "";
        if (bubbleBoyEntryOverlayController.IsRefundNeeded())
            bubbleText = I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_CLOUD_REFUND").ToUpper();
        else
        {
            int r = UnityEngine.Random.Range(0, 7);     //there are 7 different texts in lockit
            bubbleText = I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_CLOUD_DEFAULT_" + r.ToString()).ToUpper();
        }

        SetBubbleText(bubbleText);
        cloud.SetTrigger("Show");
    }

    private void SetBubbleText(string text)
    {
        bubbleText.text = text;
    }

    public bool IsExitButtonActive()
    {
        return exitButton.gameObject.activeInHierarchy;
    }

    public void HideTutorialCloud()
    {
        if (TutorialUIController.Instance.InGameCloud.subCloud.activeSelf && TutorialController.Instance.IsTutorialStepCompleted(StepTag.bubble_boy_arrow))
        {
            wasInGameCloudVisible = true;
            TutorialUIController.Instance.InGameCloud.Hide();
        }
        else
            wasInGameCloudVisible = false;

        if (TutorialUIController.Instance.tutorialArrowUI.isShown && TutorialController.Instance.IsTutorialStepCompleted(StepTag.bubble_boy_arrow))
        {
            wasArrowGameCloudVisible = true;
            //TutorialUIController.Instance.tutorialArrowUI.Hide();
        }
        else
            wasArrowGameCloudVisible = false;
    }

    public void ShowTutorialCloudAgain()
    {
        if (wasInGameCloudVisible)
        {
            TutorialUIController.Instance.InGameCloud.Show();
            wasInGameCloudVisible = false;
        }
        if (wasArrowGameCloudVisible)
        {
            //TutorialUIController.Instance.tutorialArrowUI.Show();
            wasArrowGameCloudVisible = false;
        }
    }
}
