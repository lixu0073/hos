using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityStatusTimerController
{
    private List<MaternityStatusTimer> timerList;

    public MaternityStatusTimerController()
    {
        timerList = new List<MaternityStatusTimer>();
    }

    public void RequestTimerForState(Maternity.PatientStates.MaternityPatientStateTag state, float timerLenght, float timerStep, Action onTimerEnd)
    {
        Debug.LogError("Requesting timer for state: " + state);
        timerList.Add(new MaternityStatusTimer(this, state, timerLenght, timerStep, onTimerEnd));
    }

    public void TimerElapsed(MaternityStatusTimer timer, Maternity.PatientStates.MaternityPatientStateTag state)
    {
        timerList.Remove(timer);
    }

    public bool Update()
    {
        if (timerList == null && timerList.Count == 0)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < timerList.Count; i++)
            {
                timerList[i].UpdateTimer();
            }
            return true;
        }
    }
}