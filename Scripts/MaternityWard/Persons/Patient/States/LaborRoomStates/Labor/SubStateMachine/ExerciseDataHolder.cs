using Maternity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using IsoEngine;
using Maternity.PatientStates;

public struct ExerciseDataHolder
{
    public Dictionary<Exercises, Func<Vector2i>> excercisesData;
    private readonly int totalExerciseTimeLenght;
    private float singleExerciseTime;
    private int amountOfExercises;

    public ExerciseDataHolder(Func<Vector2i> stretchPositionGetter, Func<Vector2i> ballPositionGetter, Func<Vector2i> yogaPositionGetter, int preLabourTIme)
    {
        excercisesData = new Dictionary<Exercises, Func<Vector2i>>
        {
            {Exercises.Stretch,stretchPositionGetter },
            {Exercises.Yoga,yogaPositionGetter },
            {Exercises.Ball, ballPositionGetter }
        };
        totalExerciseTimeLenght = preLabourTIme;
        amountOfExercises = Enum.GetValues(typeof(Exercises)).Length;
        singleExerciseTime = (float)totalExerciseTimeLenght / amountOfExercises;
    }

    public ExerciseBaseState CreateFullLenghtExerciseSubState(Exercises exercise, MaternityPatientWaitingForLaborState superiorState, MaternityPatientAI client)
    {
        return CreateExerciseSubState(exercise, superiorState, client, singleExerciseTime);
    }

    private ExerciseBaseState CreateExerciseSubState(Exercises exercise, MaternityPatientWaitingForLaborState superiorState, MaternityPatientAI client, float timeLenght)
    {
        switch (exercise)
        {
            case Exercises.Stretch:
                return new ExercisesStretchState(superiorState, client, excercisesData[exercise], timeLenght);
            case Exercises.Yoga:
                return new ExercisesYogaState(superiorState, client, excercisesData[exercise], timeLenght);
            case Exercises.Ball:
                return new ExercisesBallState(superiorState, client, excercisesData[exercise], timeLenght);
            default:
                return null;
        }
    }

    public ExerciseBaseState GetExerciseDependingOnTime(float totalTimeHasLeft, MaternityPatientWaitingForLaborState superiorState, MaternityPatientAI client)
    {
        float timeHasPassed = totalExerciseTimeLenght - totalTimeHasLeft;
        int indexOfExercise = 0;// Mathf.Clamp((int)(amountOfExercises * (1 - (totalTimeHasLeft / totalExerciseTimeLenght))), 0, Enum.GetValues(typeof(Exercises)).Length - 1);
        float timeLeftForExercise = 0;
        for (int i = 0; i < Enum.GetValues(typeof(Exercises)).Length; i++)
        {
            float timesum = singleExerciseTime + singleExerciseTime * i;
            if (timesum > timeHasPassed)
            {
                timeLeftForExercise = timesum - timeHasPassed;
                indexOfExercise = i;
                break;
            }
        }
        return CreateExerciseSubState((Exercises)indexOfExercise, superiorState, client, timeLeftForExercise);
    }


    public enum Exercises
    {
        Stretch,
        Yoga,
        Ball
    }

}
