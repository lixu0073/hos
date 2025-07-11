using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using SimpleUI;

public class TimedOffersScreenUI : UIElement
{
    [SerializeField]
    private TextMeshProUGUI timer = null;

    [SerializeField]
    private ButtonUI closeButton = null;
    [SerializeField]
    private ButtonUI previousOfferButton = null;
    [SerializeField]
    private ButtonUI nextOfferButton = null;

    [SerializeField]
    private PaginationIndicatorController paginationIndicator = null;

    public void SetTimerActive(bool setActive)
    {
        timer.gameObject.SetActive(setActive);
    }

    public void SetTimer(int timeLeft)
    {
        timer.text = UIController.GetFormattedShortTime(timeLeft);
    }

    public TextMeshProUGUI GetTimer()
    {
        return timer;
    }

    public void SetPreviousOfferButtonActive(bool setActive)
    {
        previousOfferButton.gameObject.SetActive(setActive);
    }

    public void SetPreviousOfferButton(UnityAction onClick)
    {
        previousOfferButton.SetButton(onClick);
    }

    public void SetNextOfferButtonActive(bool setActive)
    {
        nextOfferButton.gameObject.SetActive(setActive);
    }

    public void SetNextOfferButton(UnityAction onClick)
    {
        nextOfferButton.SetButton(onClick);
    }

    public void SetCloseButton(UnityAction onClick)
    {
        closeButton.SetButton(onClick);
    }

    public void SetPaginationIndicator(int currentPageId, int totalPagesCount)
    {
        paginationIndicator.gameObject.SetActive(totalPagesCount > 1);
        paginationIndicator.SetPaginationIndicator(currentPageId, totalPagesCount);
    }
}
