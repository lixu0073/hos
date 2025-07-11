using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class SimpleTimer
{
    private double timerLenght = 0;
    private ITimerClient timerClient;
    private Timer timer;

    public SimpleTimer(ITimerClient timerClient, double timerLenght)
    {
        this.timerLenght = timerLenght;
        if (timerLenght <= 0)
        {
            timerLenght = 0;
        }
        this.timerClient = timerClient;
        SetupTimer();
    }

    private void SetupTimer()
    {
        timer = new Timer();
        timer.Interval = timerLenght;
        timer.Elapsed += Timer_Elapsed;
        timer.AutoReset = false;
        timer.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Debug.LogError("Timer has stopped");
        timer.Stop();
        timer.Dispose();
        timerClient.TimeHasElapsed(e);
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public void SpeedUpTimer(int speedupFactor)
    {
        double currentInterval = timer.Interval;
        currentInterval /= speedupFactor;
        timer.Interval = currentInterval;
    }
#endif

}
