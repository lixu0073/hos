using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MaternityStatusTimer
{

    private Guid ID;
    private Maternity.PatientStates.MaternityPatientStateTag status;
    float timeLeft;
    float timerStep;
    Action onTimerEnd;
    private MaternityStatusTimerController client;

    public MaternityStatusTimer(MaternityStatusTimerController timerController, Maternity.PatientStates.MaternityPatientStateTag status, float timerLenght, float timerStep, Action onTimerEnd)
    {
        ID = Guid.NewGuid();
        this.status = status;
        timeLeft = timerLenght;
        client = timerController;
        this.timerStep = timerStep;
        this.onTimerEnd = onTimerEnd;
    }

    public void UpdateTimer()
    {
        timeLeft -= timerStep;
        if (timeLeft <= 0 || timeLeft > int.MaxValue)
        {
            timeLeft = 0;
            onTimerEnd?.Invoke();
            client.TimerElapsed(this, status);
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        MaternityStatusTimer timer = (MaternityStatusTimer)obj;
        return timer.ID == ID;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

}
