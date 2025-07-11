using System.Collections;
using System.Collections.Generic;
using Maternity;
using Maternity.PatientStates;
using UnityEngine;
using IsoEngine;
using System;

public class ExercisesStretchState : ExerciseBaseState
{
    float timeLeft;
    public ExercisesStretchState(MaternityPatientWaitingForLaborState superiorState, MaternityPatientAI client, Func<Vector2i> destinationGetter, float timeLeft) : base(superiorState, client, destinationGetter)
    {
        this.timeLeft = timeLeft;
        exercise = ExerciseDataHolder.Exercises.Stretch;
    }

    public override void OnEnter()
    {
        client.TeleportTo(destinationGetter.Invoke());
        try { 
            client.anim.Play(AnimHash.Mother_Stretch, 0, 0.0f);
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
                    client.anim.Play(AnimHash.Mother_Stretch, 0, 0.0f);
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
                    client.anim.Play(AnimHash.Mother_Stretch, 0, 0.0f);
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
