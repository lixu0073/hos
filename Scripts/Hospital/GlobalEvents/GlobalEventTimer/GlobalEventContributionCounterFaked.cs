using MovementEffects;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventContributionCounterFaked : GlobalEventTimer
{
    private int eventEndTime;
    private System.Random random;
    private float speedUpFactor;
    //private int maxSpeedUpFactorAsInt = 20;
    private int gameEventMaxGoal;
    private int quarterOfMaxGoal;
    private int eventLength;
    private int quarterOfEventLenght;
    private int develop_timeMultiplier = 1;
    private int timeBefore;

    private Coroutine contributionCounterCoroutine;

    public GlobalEventContributionCounterFaked(int eventStartTime, int eventEndTime, int GameEventMaxGoal, int gameEventMaxGoalWithNoise)
    {
        currentContribution = 0;
        this.eventEndTime = eventEndTime;
        eventLength = eventEndTime - eventStartTime;
        quarterOfEventLenght = eventLength / 4;
        speedUpFactor = (gameEventMaxGoalWithNoise - GameEventMaxGoal) / ((float)GameEventMaxGoal);
        gameEventMaxGoal = GameEventMaxGoal;
        quarterOfMaxGoal = GameEventMaxGoal / 4;
    }

    public void devOnlySpeedUpContributionCounter(float multiplier)
    {
        develop_timeMultiplier = (int)multiplier;
    }

    public override void StartCountingContribution()
    {
        StopCountingContribution();
        Timing.RunCoroutine(StartCounting());
    }

    public override void StopCountingContribution()
    {
        Timing.KillCoroutine(StartCounting());
    }

    private IEnumerator<float> StartCounting()
    {
        int timeBefore = eventEndTime - (int)ServerTime.getTime();
        int personal = 0;
        int attemptNewContribution = 0;
        while (true)
        {
            int timeLeft = eventEndTime - (int)ServerTime.getTime();
            if (timeBefore - timeLeft >= 1)
            {
                timeLeft -= (timeBefore - timeLeft);
                timeBefore = timeLeft;                
            }

            if (timeLeft <= 0)
                timeLeft = 0;

            attemptNewContribution = GetCurrentFakedContribution(timeLeft);
            personal = ReferenceHolder.GetHospital().globalEventController.GlobalEventPersonalProgress;

            int timeDependant = Mathf.CeilToInt( gameEventMaxGoal * 0.04f * (eventLength - timeLeft + 1) / eventLength);
            int mixPersonalAndCurrent = Mathf.CeilToInt(2 * personal + 0.88f * currentContribution);
            
            currentContribution = 
                Mathf.Min(Mathf.Max(timeDependant, currentContribution, attemptNewContribution, personal, mixPersonalAndCurrent, 1), 
                          Mathf.CeilToInt(gameEventMaxGoal * (1+speedUpFactor))
                   );
            /*
            Possibilities:
                1. The interpolation returned a proper value of new contribution
                2. The interpolation returned a proper value of new contribution, but it's less than player's personal goal
                3. The interpolation returned value that is lower than previously displayed contribution value

                Case 3 is interpolation dependant, thus it depends on the values in interpolation tables (or the pairs [x, f(x)] defining the interpolation)
                Case 2 may happen in case of a long event or relatively small global event goal

                */
            yield return Timing.WaitForSeconds(0.85f);
        }
    }

    private float[] InterpolationXs;
    private float[] InterpolationYs;

    private int GetCurrentFakedContribution(int timeLeft)
    {
        float xVal = 1 - ((float)timeLeft) / eventLength;

        return Mathf.CeilToInt( gameEventMaxGoal * Interpolate(xVal));
    }

    private float Interpolate(float x)
    {
        InstantiateInterpolationArrays();

        float result = 0f;

        for(int i = 0; i < InterpolationXs.Length; ++i)
        {
            result += InterpolationYs[i] * CalculateLagrangePolynomial(x, i);
        }

        return result;
    }

    private float CalculateLagrangePolynomial(float x, int i)
    {
        float result = 1;
        float x_i = InterpolationXs[i];
        float x_j;

        for (int j = 0; j < InterpolationXs.Length; ++j)
        {
            if (j != i)
            {
                x_j = InterpolationXs[j];
                result *= (x - x_j) / (x_i - x_j);
            }
        }

        return result;
    }

    private void InstantiateInterpolationArrays()
    {
        if (InterpolationXs == null || InterpolationYs == null)
        {
            List<float> xs, ys;

            Hospital.FakedContributionConfig.GetInterpolationPointsLists(out xs, out ys);

            xs.Add(10.0f / eventLength);
            ys.Add(3.0f / gameEventMaxGoal);

            xs.Add(1f / (1+speedUpFactor) );
            ys.Add(1);

            xs.Add(1);
            ys.Add(1 + speedUpFactor);

            InterpolationXs = xs.ToArray(); //new float[6] { 0, 10.0f / eventLength, 0.3333f, 0.6667f, 1/(1+speedUpFactor), 1 };
            InterpolationYs = ys.ToArray(); //new float[6] { 0, 3.0f / gameEventMaxGoal, 0.1f, 0.61f, 1, 1 + speedUpFactor };
        }
    }
}
