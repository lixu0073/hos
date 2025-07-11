using System;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;
using TMPro;

public class GEPersonalPanelUI : MonoBehaviour
{
    [TermsPopup]
    [SerializeField]
    private string timerTerm = "-";
    [TermsPopup]
    [SerializeField]
    private string unlockLevelTerm = "-";

    [SerializeField]
    private Image[] toGrayscale = null;

    [SerializeField]
    private GameObject lockedGEPanel = null;
    [SerializeField]
    private GameObject progressTooltip = null;

    [SerializeField]
    private Image progressItemImage = null;
    [SerializeField]
    private Image progressFill = null;
    [SerializeField]
    private Image goalRewardImage = null;

    [SerializeField]
    private TextMeshProUGUI goalCounter = null;
    [SerializeField]
    private TextMeshProUGUI progressTooltipText = null;
    [SerializeField]
    private TextMeshProUGUI goalRewardAmountText = null;
    [SerializeField]
    private TextMeshProUGUI eventTimeLeft = null;
    [SerializeField]
    private TextMeshProUGUI unlockLevelText = null;

    [SerializeField]
    private Localize eventTitle = null;
    [SerializeField]
    private TextMeshProUGUI eventDescriptionText = null;

    [Space(10)]
    [SerializeField]
    private GameObject rewardsSlot = null;
    [SerializeField]
    private GameObject xpContainer = null;
    [SerializeField]
    private TextMeshProUGUI xpPerContribution = null;
    [SerializeField]
    private GameObject coinsContainer = null;
    [SerializeField]
    private TextMeshProUGUI coinsPerContribution = null;

    [Space(10)]
    [SerializeField]
    private GEContributionPanelController contributionPanel = null;

    [Space(10)]
    [SerializeField]
    private Material grayscaleMaterial = null;
    [TermsPopup]
    [SerializeField]
    private string goalCounterTerm = "-";

    public void SetLockedGEPanelActive(bool setActive)
    {
        lockedGEPanel.SetActive(setActive);
    }

    public void SetProgressTooltipActive(bool setActive)
    {
        progressTooltip.SetActive(setActive);
    }

    public void SetGoalSprite(Sprite goalSprite)
    {
        progressItemImage.sprite = goalSprite;
    }

    public void SetGoalCounter(int currentGoal)
    {
        goalCounter.text = string.Format(I2.Loc.ScriptLocalization.Get(goalCounterTerm), currentGoal);
    }

    public void SetProgressTooltipTextCounter(int number, int max)
    {
        progressTooltipText.text = string.Format("{0}/{1}", number, max);
    }

    public void SetProgress(int current, int max)
    {
        progressFill.fillAmount = Mathf.Clamp((float)current / (float)max, 0.128f, 1.0f);
        Vector3 pos = progressTooltip.transform.localPosition;
        progressTooltip.transform.localPosition = new Vector3(-307.6f + progressFill.fillAmount * 609.4f, pos.y, pos.z);
    }

    public void SetRewardSprite(Sprite sprite)
    {
        goalRewardImage.sprite = sprite;
    }

    public void SetRewardAmount(int number)
    {
        goalRewardAmountText.text = string.Format("x{0}", number);
    }

    public void SetEventTitle(string term)
    {
        eventTitle.SetTerm(term);
    }

    public void SetEventDescriptionText(string text)
    {
        eventDescriptionText.text = text;
    }

    public void SetTimeLeft(int secondsLeft)
    {
        eventTimeLeft.text = string.Format("{0} {1}", I2.Loc.ScriptLocalization.Get(timerTerm), UIController.GetFormattedShortTime(secondsLeft));
    }

    public void SetUnlockLevel(int unlockLevel)
    {
        unlockLevelText.text = string.Format(I2.Loc.ScriptLocalization.Get(unlockLevelTerm), unlockLevel);
    }

    public void SetSingleContributionReward(int coinsNumber, int xpNumber)
    {
        rewardsSlot.SetActive(!(coinsNumber == 0 && xpNumber == 0));

        coinsContainer.SetActive(coinsNumber > 0);
        xpContainer.SetActive(xpNumber > 0);

        coinsPerContribution.text = coinsNumber.ToString();
        xpPerContribution.text = xpNumber.ToString();
    }

    public void SetContributionPanel(GEContributionPanelData data)
    {
        contributionPanel.Initialize(data);
    }

    public void SetImagesGrayscale(bool isGrayscale)
    {
        if (toGrayscale == null)
        {
            return;
        }

        for (int i = 0; i < toGrayscale.Length; ++i)
        {
            toGrayscale[i].material = isGrayscale ? grayscaleMaterial : null;
        }
    }
}
