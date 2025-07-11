using IsoEngine;
using Maternity;
using Maternity.PatientStates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExercisesYogaState : ExerciseBaseState
{
    float timeLeft;
    public ExercisesYogaState(MaternityPatientWaitingForLaborState superiorState, MaternityPatientAI client, Func<Vector2i> destinationGetter, float timeLeft) : base(superiorState, client, destinationGetter)
    {
        client.TeleportTo(destinationGetter.Invoke());
        this.timeLeft = timeLeft;
        exercise = ExerciseDataHolder.Exercises.Yoga;
    }

    public override void OnEnter()
    {
        try { 
            client.anim.Play(AnimHash.Mother_Yoga, 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    public override void OnUpdate()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            superiorState.SetNextExercise(exercise);
            return;
        }
    }

    public override void Notify(int id, object parameters)
    {
        switch ((ExerciseStateNotification)id)
        {
            case ExerciseStateNotification.roomAnanchored:
                client.TeleportTo(destinationGetter.Invoke());
                try { 
                    client.anim.Play(AnimHash.Mother_Yoga, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                break;
            case ExerciseStateNotification.roomUnanchored:
                break;
            case ExerciseStateNotification.roomMoved:
                client.TeleportTo(destinationGetter.Invoke());
                try { 
                    client.anim.Play(AnimHash.Mother_Yoga, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                break;
            default:
                break;
        }
    }
}
