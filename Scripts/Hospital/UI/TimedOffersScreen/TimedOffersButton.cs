using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class TimedOffersButton : MonoBehaviour
{
    [SerializeField]
    private Image offersBadge = null;
    [SerializeField]
    private TextMeshProUGUI timerText = null;

    [SerializeField]
    private Sprite defaultBadgeSprite = null;
    private Sprite customBadgeSprite = null;

    private bool isWaitingForCustomSprite = false;

    public void Start()
    {
        TimedOffersController.Instance.onTimedOffersUpdated -= OnTimedOffersUpdated;
        TimedOffersController.Instance.onTimedOffersUpdated += OnTimedOffersUpdated;
    }

    public void OnEnable()
    {
        TimedOffersController.Instance.onTimedOffersUpdated -= OnTimedOffersUpdated;
        TimedOffersController.Instance.onTimedOffersUpdated += OnTimedOffersUpdated;
    }

    private void OnDisable()
    {
        StopTimerCoroutine();
    }

    private void OnDestroy()
    {
        TimedOffersController.Instance.onTimedOffersUpdated -= OnTimedOffersUpdated;
    }

    public void SetOffersBadge(Sprite badge)
    {
        offersBadge.sprite = badge;
    }

    public void SetTimerText(int timeLeft)
    {
        timerText.text = UIController.GetFormattedShortTime(timeLeft);
    }

    public void OnTimedOffersUpdated()
    {
        if (TimedOffersController.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (TimedOffersController.Instance.timedOffers.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        StartTimerCoroutine();
        SetButtonBadge();
    }

    public void OnTimedOffersButtonClicked()
    {
        UIController.getHospital.TimerOffersScreen.GetComponent<TimedOffersScreenInitializer>().Initialize(null, null, 0);
    }

    Coroutine timerCoroutine = null;

    private void StopTimerCoroutine()
    {
        try
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void StartTimerCoroutine()
    {
        StopTimerCoroutine();
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    IEnumerator TimerCoroutine()
    {
        TimedOffersController.Instance.timedOffers.Sort((x, y) => x.offerOrderNo.CompareTo(y.offerOrderNo));
        while (true)
        {
            if (TimedOffersController.Instance.timedOffers[0] != null)
            {
                int time = TimedOffersController.Instance.timedOffers[0].timedOfferEndDate - (int)ServerTime.UnixTime(DateTime.UtcNow);
                timerText.text = UIController.GetFormattedShortTime(time);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void SetButtonBadge()
    {
        if (TimedOffersDeltaConfig.timedOfferUIBadge != "-")
        {
            SetCustomBadge(TimedOffersDeltaConfig.timedOfferUIBadge);
            return;
        }

        SetDefaultBadge();
    }

    private void SetDefaultBadge()
    {
        offersBadge.sprite = defaultBadgeSprite;
    }

    private void SetCustomBadge(string decisionPoint)
    {
        Debug.LogError("Timed offers were in DDNA " + decisionPoint);
        //if (isWaitingForCustomSprite)
        //{
        //    return;
        //}

        //if (customBadgeSprite != null)
        //{
        //    offersBadge.sprite = customBadgeSprite;
        //    return;
        //}

        //isWaitingForCustomSprite = true;

        //DecisionPointCalss.RequestSprite(decisionPoint, (sprite) => {
        //    if (sprite != null)
        //    {
        //        isWaitingForCustomSprite = false;
        //        customBadgeSprite = sprite;
        //        offersBadge.sprite = customBadgeSprite;
        //    }
        //}, null);
    }
}
