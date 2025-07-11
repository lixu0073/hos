using UnityEngine;
using System.Collections.Generic;
using System;
using IsoEngine;
using SimpleUI;
using MovementEffects;
using UnityEngine.UI;
using TMPro;

public class BubbleBoyMinigameUI : UIElement
{
    public List<BubbleBoyRevealObject> revealObjects = new List<BubbleBoyRevealObject>();
    public List<BubbleBoyRewardUI> bubbleRewards = new List<BubbleBoyRewardUI>();
    private int[] reveal_ids = new int[7];

    private BubbleBoyMinigameController bubbleBoyMinigameController;

    private BubbleOpenFee openFee;
#pragma warning disable 0649
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button revealButton;
    [SerializeField] private Animator revealItemAnim;
    [SerializeField] private Animator lotteryAnimator;
    [SerializeField] private Animator boyAnimator;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Animator infoTextAnimator;
    [SerializeField] private Animator buttonPlayAnimator;
    [SerializeField] private RuntimeAnimatorController buttonPlayAnimDefault;
    [SerializeField] private RuntimeAnimatorController buttonPlayAnimBlinking;
    [SerializeField] private ParticleSystem rewardBubbles;

    [SerializeField] private GameObject freeButtonContent;
    [SerializeField] private GameObject coinButtonContent;
    [SerializeField] private GameObject diamondButtonContent;
    [SerializeField] private GameObject buttonVideoContent;

    [Header("To be adjusted for iphone X")]
    [SerializeField] private RectTransform infoTextRect;
    [SerializeField] private RectTransform playButtonRect;
#pragma warning restore 0649
    private Animator revealButtonAnimator;
    private IEnumerator<float> shuffle;
    private IEnumerator<float> dellayedButton = null;

    private bool secondGame = false;
    private bool isButtonActive = false;
    private bool isAdAvailable = false;

    public bool loteryBlocked = false;

    public void OnEnable()
    {
        bubbleBoyMinigameController = ReferenceHolder.GetHospital().bubbleBoyMinigameController;

        if (ExtendedCanvasScaler.HasNotch())
        {
            infoTextRect.offsetMin = new Vector2(-400, 32.5f);
            infoTextRect.offsetMax = new Vector2(400, 67.5f);
            playButtonRect.offsetMin = new Vector2(-100, -35);
            playButtonRect.offsetMax = new Vector2(100, 35);
        }

        CancelInvoke("CheckAdsAvailability");

        if (shuffle != null)
        {
            Timing.KillCoroutine(shuffle);
            shuffle = null;
        }

        if (dellayedButton != null)
        {
            Timing.KillCoroutine(dellayedButton);
            dellayedButton = null;
        }

        //Bubbleboy condition not met
        if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_intro, true))
        {
            return;
        }

        isBlockedGame = true;
        secondGame = false;

        playButton.interactable = false;
        playButton.gameObject.SetActive(false);
        revealButton.gameObject.SetActive(false);

        InitializeRewards();
        DisableAllBubblesActive();

        if (bubbleRewards != null && bubbleRewards.Count > 0)
        {
            for (int i = 0; i < bubbleRewards.Count; i++)
            {
                bubbleRewards[i].IdleState();
            }
        }

        isAdAvailable = false;
        lotteryAnimator.SetTrigger("Idle");
        revealItemAnim.SetTrigger("Idle");
        openFee = ResourcesHolder.GetHospital().bubbleBoyDatabase.GetBubbleFee(BubbleBoyDataSynchronizer.Instance.BubbleOpened);
        UpdateFeeButton(openFee);
        HospitalUIPrefabController.Instance.HideMainUI(false);

        dellayedButton = Timing.RunCoroutine(DelayedPlayButton());

        SetInfoText("", false);

        StartCoroutine(base.Open(true, false));
    }

    public void CloseMinigame()
    {
        if (!isBlockedGame)
        {
            if (shuffle != null)
            {
                Timing.KillCoroutine(shuffle);
                shuffle = null;
            }

            if (dellayedButton != null)
            {
                Timing.KillCoroutine(dellayedButton);
                dellayedButton = null;
            }

            BubbleBoyDataSynchronizer.Instance.BubbleOpened = 0;
            secondGame = false;
            Exit();
        }

        CancelInvoke("CheckAdsAvailability");
    }

    public void ButtonPrizesToReveal()
    {
        UpdatePrizesToReveal();

        isButtonActive = !isButtonActive;

        if (isButtonActive)
            revealItemAnim.SetTrigger("Open");
        else
            revealItemAnim.SetTrigger("Close");
    }

    private void RandomIds()
    {
        for (int i = 0; i < 7; ++i)
            reveal_ids[i] = i;

        for (int i = 0; i < 7; ++i)
        {
            int idx = BaseGameState.RandomNumber(i, 7);

            //swap elements
            int tmp = reveal_ids[i];
            reveal_ids[i] = reveal_ids[idx];
            reveal_ids[idx] = tmp;
        }
    }

    public void UpdatePrizesToReveal()
    {
        if (bubbleRewards != null && bubbleRewards.Count > 0)
        {
            for (int i = 0; i < bubbleRewards.Count; ++i)
            {
                var rew = bubbleRewards[revealObjects[reveal_ids[i]].bubbleID].GetReward();

                if (rew != null && rew.collected == false)
                {
                    revealObjects[reveal_ids[i]].SetRevealObject(rew);
                    revealObjects[reveal_ids[i]].gameObject.SetActive(true);
                }
                else
                    revealObjects[reveal_ids[i]].gameObject.SetActive(false);
            }
        }
        else
        {
            if (isButtonActive)
                revealItemAnim.SetTrigger("Close");
            isButtonActive = false;
        }
    }

    public void InitializeRewards()
    {
        SoundsController.Instance.PlayShortBubble();

        bubbleBoyMinigameController.GenerateMinigame();

        var rewards = bubbleBoyMinigameController.GetAllRandomizedRewards();

        if (rewards != null && rewards.Count > 0)
        {
            if (rewards.Count == bubbleRewards.Count)
            {
                for (int i = 0; i < bubbleRewards.Count; ++i)
                {
                    bubbleRewards[i].Setup(rewards[i], true);
                    bubbleRewards[i].ShowItem();
                }
                RandomIds();
                SetRevealObjectsBubbleIDs();
            }
            else
            {
                throw new IsoException("List size of rewards and UI object is different so ... error!");
            }
        }
        else throw new IsoException("Ther is no randomized rewards !");
    }

    public void ButtonPlayMiniGame()
    {
        if (openFee != null)
        {
            if (openFee.currencyType == ResourceType.Coin)
            {
                if (Game.Instance.gameState().GetCoinAmount() >= openFee.amount)
                {
                    GameState.Get().RemoveCoins(openFee.amount, EconomySource.BubbleBoy);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(Input.mousePosition, openFee.amount, 0f, ReferenceHolder.Get().giftSystem.particleSprites[0], false);
                    AnalyticsController.instance.ReportBubbleBoy(AnalyticsBubbleBoyAction.PopMore, false, -1, openFee.entryLevel, openFee.amount, 0);

                    //if (isButonActive)
                    //    revealItemAnim.SetTrigger("Close");

                    exitButton.gameObject.SetActive(false);
                    Shuffle();
                    openFee = ResourcesHolder.GetHospital().bubbleBoyDatabase.GetBubbleFee(BubbleBoyDataSynchronizer.Instance.BubbleOpened + 1);
                    UpdateFeeButton(openFee);

                    //set bubble IDs

                    UpdatePrizesToReveal();
                }
                else
                {
                    UIController.get.BuyResourcesPopUp.Open(openFee.amount - Game.Instance.gameState().GetCoinAmount(), () =>
                    {
                        ButtonPlayMiniGame();
                    }, null, Hospital.BuyResourcesPopUp.missingResourceType.coin);
                }
            }
            else if (openFee.currencyType == ResourceType.Diamonds)
            {
                if (isAdAvailable)
                {
                    isAdAvailable = false;
                    CancelInvoke("CheckAdsAvailability");
                    AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_bubbleboy);
                    playButton.interactable = false;
                }
                else if (Game.Instance.gameState().GetDiamondAmount() >= openFee.amount)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(openFee.amount, delegate
                    {
                        GameState.Get().RemoveDiamonds(openFee.amount, EconomySource.BubbleBoy);
                        ReferenceHolder.Get().giftSystem.CreateItemUsed(Input.mousePosition, openFee.amount, 0f, ReferenceHolder.Get().giftSystem.particleSprites[1], false);
                        PopMore();
                    }, this);
                }
                else
                {
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }
        }
    }

    public void PopMore()
    {
        AnalyticsController.instance.ReportBubbleBoy(AnalyticsBubbleBoyAction.PopMore, false, -1, openFee.entryLevel, 0, openFee.amount);

        //if (isButonActive)
        //    revealItemAnim.SetTrigger("Close");
        exitButton.gameObject.SetActive(false);
        Shuffle();
        secondGame = true;
        openFee = ResourcesHolder.GetHospital().bubbleBoyDatabase.GetBubbleFee(BubbleBoyDataSynchronizer.Instance.BubbleOpened + 1);
        UpdateFeeButton(openFee);
        UpdatePrizesToReveal();
    }

    public void DisableAllBubblesActive()
    {
        for (int i = 0; i < bubbleRewards.Count; ++i)
        {
            bubbleRewards[i].gameObject.GetComponent<Button>().interactable = false;
        }
    }

    public void EnableExitAndPlayAgain()
    {
        if (dellayedButton != null)
        {
            Timing.KillCoroutine(dellayedButton);
            dellayedButton = null;
        }

        dellayedButton = Timing.RunCoroutine(DelayedPlayButton());

        UIController.getHospital.bubbleBoyMinigameUI.DisableAllBubblesActive();

        UpdatePrizesToReveal();
    }

    private void SetRevealObjectsBubbleIDs()
    {
        List<int> freeIDs = new List<int>();
        for (int i = 0; i < bubbleRewards.Count; ++i)
        {
            freeIDs.Add(i);
        }
        for (int i = 0; i < revealObjects.Count; ++i)
        {
            revealObjects[reveal_ids[i]].bubbleID = freeIDs[GameState.RandomNumber(freeIDs.Count)];
            freeIDs.Remove(revealObjects[reveal_ids[i]].bubbleID);
        }
        freeIDs.Clear();
        freeIDs.Capacity = 0;
    }

    private void Shuffle()
    {
        for (int i = 0; i < bubbleRewards.Count; ++i)
            bubbleRewards[i].DisableButton();

        playButton.interactable = false;
        playButton.gameObject.SetActive(false);

        Timing.KillCoroutine(dellayedButton);
        Timing.KillCoroutine(shuffle);

        SetInfoText("", false);
        shuffle = Timing.RunCoroutine(DelayedShuffle());
    }

    private void UpdateFeeButton(BubbleOpenFee fee)
    {
        coinButtonContent.SetActive(false);
        diamondButtonContent.SetActive(false);
        freeButtonContent.SetActive(false);
        buttonVideoContent.SetActive(false);
        buttonPlayAnimator.runtimeAnimatorController = buttonPlayAnimDefault;

        if (fee != null)
        {
            if (fee.amount != 0)
            {
                if (fee.entryLevel == 2)
                {
                    diamondButtonContent.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = fee.amount.ToString();
                    CheckAdsAvailability();
                }
                else if (fee.currencyType == ResourceType.Coin)
                {
                    coinButtonContent.SetActive(true);
                    coinButtonContent.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = fee.amount.ToString();
                }
                else if (fee.currencyType == ResourceType.Diamonds)
                {
                    diamondButtonContent.SetActive(true);
                    diamondButtonContent.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = fee.amount.ToString();
                }
            }
            else
            {
                freeButtonContent.SetActive(true);
                buttonPlayAnimator.runtimeAnimatorController = buttonPlayAnimBlinking;
            }
        }
    }

    void CheckAdsAvailability()
    {
        //CancelInvoke("CheckAdsAvailability");
        isAdAvailable = AdsController.instance.IsAdAvailable(AdsController.AdType.rewarded_ad_bubbleboy);

        if (!isAdAvailable)
        {
            diamondButtonContent.SetActive(true);
            buttonVideoContent.SetActive(false);
            // Invoke("CheckAdsAvailability", 2f);
        }
        else
        {
            diamondButtonContent.SetActive(false);
            buttonVideoContent.SetActive(true);
        }
    }

    IEnumerator<float> DelayedShuffle()
    {
        if (!secondGame)
        {
            lotteryAnimator.SetTrigger("Lottery");
            boyAnimator.SetTrigger("Blow");

            for (int i = 0; i < bubbleRewards.Count; ++i)
            {
                bubbleRewards[i].ShowMark();
                yield return Timing.WaitForSeconds(0.1f);
            }

            int n = bubbleRewards.Count;
            int k;
            while (n > 0)
            {
                k = GameState.RandomNumber(n);
                --n;
                BubbleBoyReward temp = bubbleRewards[n].GetReward();
                BubbleBoyReward temp2;
                int stopCounter = 0;
                do
                {
                    temp2 = bubbleRewards[k].GetReward();
                    if (temp2.collected)
                    {
                        ++k;
                        stopCounter++;
                        if (k >= bubbleRewards.Count)
                            k = 0;
                    }

                    if (stopCounter > bubbleRewards.Count + 2)
                    {
                        //  ButtonExit();
                        yield break;
                    }
                } while (temp2.collected);

                if (!temp.collected && !temp2.collected)
                {
                    bubbleRewards[n].Setup(bubbleRewards[k].GetReward(), true, true);
                    bubbleRewards[k].Setup(temp, true, true);

                    bubbleRewards[n].DisableButton();
                    bubbleRewards[k].DisableButton();
                }
            }
        }

        if (!secondGame)
            yield return Timing.WaitForSeconds(1.25f);
        else
            yield return Timing.WaitForSeconds(.1f);

        for (int i = 0; i < bubbleRewards.Count; ++i)
        {
            BubbleBoyReward temp = bubbleRewards[i].GetReward();
            if (!temp.collected)
                bubbleRewards[i].Setup(temp, true, true, true);
        }

        SetInfoText(I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_MINIGAME_PICK_BUBBLE"), true);
        if (!revealButton.gameObject.activeSelf)
        {
            revealButtonAnimator = revealButton.gameObject.GetComponent<Animator>();
            revealButtonAnimator.ResetTrigger("Normal");
            revealButtonAnimator.SetTrigger("Entry");
            revealButton.gameObject.SetActive(true);
        }
        isButtonActive = false;
        ButtonPrizesToReveal();
        secondGame = true;
    }

    IEnumerator<float> DelayedPlayButton()
    {
        yield return Timing.WaitForSeconds(1f);
        if (BubbleBoyDataSynchronizer.Instance.BubbleOpened != 0)
            exitButton.gameObject.SetActive(true);

        playButton.interactable = true;
        playButton.gameObject.SetActive(true);

        if (secondGame)
            SetInfoText(I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_MINIGAME_TRY_AGAIN"), false);
        else
            SetInfoText(I2.Loc.ScriptLocalization.Get("BUBBLE_BOY_MINIGAME_CHECK_PRIZES"), false);

        yield return 0;
    }

    public void SetInfoText(string text, bool isBlinking)
    {
        try
        {
            infoTextAnimator.SetBool("IsBlinking", isBlinking);
            infoTextAnimator.Play("Show", 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
        infoText.text = text;
    }

    public void PlayRewardBubbles()
    {
        rewardBubbles.Play();
    }

    public bool IsExitButtonActive()
    {
        return exitButton.gameObject.activeInHierarchy;
    }
}
