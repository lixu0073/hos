using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class EventButton : MonoBehaviour {
    
    public TextMeshProUGUI timerText;
    public GameObject ribbonBackground;
    Coroutine timerCoroutine;


    void Awake()
    {
        Hide();
        GameEventsController.OnEventStarted += Setup;
        GameEventsController.OnEventEnded += Setup;
    }

    void OnDestroy()
    {
        GameEventsController.OnEventStarted -= Setup;
        GameEventsController.OnEventEnded -= Setup;
        if (timerCoroutine != null) {
            try { 
                StopCoroutine(timerCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void Setup()
    {
        bool isAcitve = GameEventsController.Instance.IsEventOfTypeActive(GameEventType.CancerDay);

        if (GameState.Get().IAPCancerDayCount < 3
            && GameEventsController.Instance.IsEventOfTypeActive(GameEventType.CancerDay)
            && !Hospital.VisitingController.Instance.IsVisiting
            && Game.Instance.gameState().GetHospitalLevel() > 7)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        if (ExtendedCanvasScaler.HasNotch())
        {
            ribbonBackground.SetActive(false);
        }
        gameObject.SetActive(true);

        if (timerCoroutine != null)
        {
            try { 
                StopCoroutine(timerCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    void Hide()
    {
        gameObject.SetActive(false);

        if (timerCoroutine != null)
        {
            try { 
                StopCoroutine(timerCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    IEnumerator UpdateTimer()
    {
        while (true)
        {
            long timeTillNextDay = GameEventsController.Instance.currentEvent.GetTimeToEnd();
            //if (timeTillNextDay < 0)
            //    Hide();

            timerText.text = UIController.GetFormattedShortTime(timeTillNextDay);
            yield return new WaitForSeconds(1f);
        }
    }
}