using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimerTester : MonoBehaviour
{
    public int EndEventEpochTime = 0;
    public int MaxGoal = 0;
    public int MaxGoalFakedResult = 0;
    public GlobalEventContributionCounterFaked timer;
    private bool count = false;

    public void CreateContributorCounter()
    {
        //timer = new GlobalEventContributionCounterFaked(EndEventEpochTime, MaxGoal, MaxGoalFakedResult);
    }

    public void StartCounter()
    {
        timer.StartCountingContribution();
        count = true;
    }

    public void StopCounting()
    {
        timer.StopCountingContribution();
        count = false;
    }

    private void Update()
    {
        if (count)
        {
            Debug.LogError(timer.CurrentContribution);
        }
    }

}
