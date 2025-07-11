using IsoEngine;
using Maternity;
using Maternity.PatientStates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseBaseState : ISubState<MaternityPatientWaitingForLaborState>
{
    public MaternityPatientAI client;
    public MaternityPatientWaitingForLaborState superiorState;
    public Func<Vector2i> destinationGetter;
    public ExerciseDataHolder.Exercises exercise;

    public ExerciseBaseState(MaternityPatientWaitingForLaborState superiorState, MaternityPatientAI client, Func<Vector2i> destinationGetter)
    {
        this.superiorState = superiorState;
        this.client = client;
        this.destinationGetter = destinationGetter;
    }

    public virtual void Notify(int id, object parameters)
    {
    }

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnUpdate()
    {
    }
}
