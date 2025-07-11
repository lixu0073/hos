using I2.Loc;
using SimpleUI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyRewardCardLayout : UIElement
{
#pragma warning disable 0649
    [SerializeField] private RectTransform thisRecttransform;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private Image upperBackgroundBar;
    [SerializeField] private Image starImage;
    [SerializeField] private Image greenCheckmark;
    [SerializeField] private Image xRedMark;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI amountOfRewardText;
    [SerializeField] private TextMeshProUGUI bigPrizeName;
    [SerializeField] private Image bigPrizeIcon;
    [SerializeField] private Image tileBackground;
    [SerializeField] private GameObject GetForAddGroup;
    [SerializeField] private Image todayGreenCheckMark;
#pragma warning restore 0649
    private UnityAction onClickAction;

    public void SetAmountOfRewardText(int amount)
    {
        if (amountOfRewardText != null)
            amountOfRewardText.text = "x" + amount;
    }

    public void SetAmountOfRewardTextActive(bool setActive)
    {
        if (amountOfRewardText != null)
            amountOfRewardText.gameObject.SetActive(setActive);
    }

    public void SetTodayGreenCheckMarkActive(bool setActive)
    {
        todayGreenCheckMark.gameObject.SetActive(setActive);
    }

    public void ClearCard()
    {
        SetDayTextActive(false);
        SetGreenCheckMarkActive(false);
        SetXMarkActive(false);
        SetStarImageActive(false);
        SetRewardBoxImageActive(false);
        SetRewardIconActive(false);
        SetUpperBarColorToGray(false);
        SetBackgroundColorToGray(false);
        SetGetForAddPanelActive(false);
        SetAmountOfRewardTextActive(false);
        SetTodayGreenCheckMarkActive(false);
        SetBigNamePrizeTextActive(false);
    }

    public void SetRewardBoxImageActive(bool setActive)
    {
        bigPrizeIcon.gameObject.SetActive(setActive);
    }

    public void SetRewardBoxImage(Sprite sprite)
    {
        bigPrizeIcon.sprite = sprite;
    }

    public void SetOtherDayTextThanToday(int dayNumber)
    {
        dayText.text = string.Format(ScriptLocalization.Get("DAILY_QUESTS/DAY_X"), dayNumber + 1);
    }

    public void SetTodayText()
    {
        dayText.text = ScriptLocalization.Get("TODAY");
    }

    public void SetBigNamePrizeText(string text)
    {
        bigPrizeName.text = text;
    }

    public void SetBigNamePrizeTextActive(bool setActive)
    {
        bigPrizeName.gameObject.SetActive(setActive);
    }

    public void SetDayTextActive(bool setActive)
    {
        dayText.gameObject.SetActive(setActive);
    }

    public void SetBackgroundColorToGray(bool toGray)
    {
        tileBackground.material = toGray == true ? ResourcesHolder.Get().GrayscaleMaterial : null;
    }

    public void SetGreenCheckMarkActive(bool setactive)
    {
        greenCheckmark.gameObject.SetActive(setactive);
    }

    public void SetXMarkActive(bool setActive)
    {
        xRedMark.gameObject.SetActive(setActive);
    }

    public void SetRewardIcon(Sprite sprite)
    {
        if (rewardIcon != null)
            rewardIcon.sprite = sprite;
    }

    public void SetRewardIconActive(bool setActive)
    {
        if (rewardIcon != null)
            rewardIcon.gameObject.SetActive(setActive);
    }

    public void SetUpperBarColorToGray(bool toGray)
    {
        upperBackgroundBar.material = toGray == true ? ResourcesHolder.Get().GrayscaleMaterial : null;
    }

    public void SetUpperBarActive(bool setActive)
    {
        upperBackgroundBar.gameObject.SetActive(setActive);
    }

    public void SetStarImageActive(bool setActive)
    {
        starImage.gameObject.SetActive(setActive);
    }

    public void SetButtonOnClickAction(UnityAction action)
    {
        onClickAction = action;
    }

    public void OnButtonClick()
    {
        if (onClickAction != null)
        {
            onClickAction.Invoke();
        }
        else
            Debug.LogError("Action is null");
    }

    public void SetGetForAddPanelActive(bool setActive)
    {
        GetForAddGroup.SetActive(setActive);
    }
}
