using System;
using System.Collections;
using System.Collections.Generic;
using Maternity;
using UnityEngine;

public class MaternityBloodTestRoomTestingBloodSample : MaternityBloodTestBaseState
{
    IMaternityFacilityPatient patient;
    public MaternityBloodTestRoomTestingBloodSample(MaternityBloodTestRoom parent, IMaternityFacilityPatient patient) : base(parent)
    {
        stateTag = BloodTestRoomState.TBS;
        this.patient = patient;
        patient.GetPatientAI().onStateChanged += parent.OnPatientHealed;
    }

    public override void OnEnter()
    {
        parent.StartHealingAnimation();
    }

    public override void OnExit()
    {
        patient.GetPatientAI().onStateChanged -= parent.OnPatientHealed;
    }

    public override void OnUpdate()
    {

    }

    public override void Notify(int id, object parameters)
    {
        switch ((BloodTestRoomNotifications)id)
        {
            case BloodTestRoomNotifications.OfficeUnanchored:
                break;
            case BloodTestRoomNotifications.OfficeAnchored:
                break;
            case BloodTestRoomNotifications.PatientAdded:
                break;
            case BloodTestRoomNotifications.QueueEmpty:
                parent.WorkingStateManager.State = new MaternityBloodTestWaitingForBloodSample(parent);
                break;
            default:
                break;
        }
    }

    public override string SaveToString()
    {
        return base.SaveToString();
    }
}
