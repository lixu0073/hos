using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameEventsController : MonoBehaviour {

    #region Static

    private static GameEventsController instance;

    public static GameEventsController Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning("There is no GameEventsController instance on scene!");
            }

            return instance;
        }
    }

    Coroutine checkCoroutine;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("There are possibly multiple instances of GameEventsController on scene!");
        }
        instance = this;
    }

    #endregion

    public BaseGameEventInfo currentEvent;

    public delegate void ChangeState();

    public static event ChangeState OnEventStarted;
    public static event ChangeState OnEventEnded;

    #region API

    public bool IsEventOfTypeActive(GameEventType type)
    {
        return currentEvent != null && currentEvent.type == type && currentEvent.IsActive();
    }

    public bool IsAnyEventActive()
    {
        return currentEvent != null && currentEvent.IsActive();
    }


    public void Initialize()
    {
        if (IsAnyEventActive())
        {
            OnEventStart();
        }
    }

    #endregion

    #region Private Methods

    private void OnDisable()
    {
        if (checkCoroutine != null) {
            try { 
                StopCoroutine(checkCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    private void OnEventStart()
    {
        if (checkCoroutine != null)
        {
            try { 
            StopCoroutine(checkCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
        checkCoroutine = StartCoroutine(CheckForEventEnd());

        OnEventStarted?.Invoke();
    }

    IEnumerator CheckForEventEnd()
    {
        while (true)
        {
            long timeTillNextDay = currentEvent.GetTimeToEnd();
            if (!IsEventOfTypeActive(currentEvent.type))
            {
                OnEventEnded?.Invoke();
                checkCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    #endregion

}
