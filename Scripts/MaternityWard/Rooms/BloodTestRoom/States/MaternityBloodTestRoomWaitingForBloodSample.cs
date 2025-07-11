using System;
using System.Collections;
using System.Collections.Generic;
using Maternity;
using UnityEngine;

public class MaternityBloodTestWaitingForBloodSample : MaternityBloodTestBaseState
{
    public MaternityBloodTestWaitingForBloodSample(MaternityBloodTestRoom parent) : base(parent)
    {
        stateTag = BloodTestRoomState.WFBS;
    }

    public override void OnEnter()
    {
        parent.StartIdleAnimation();
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        throw new NotImplementedException();
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
                parent.WorkingStateManager.State = new MaternityBloodTestRoomTestingBloodSample(parent,parent.GetFirstPatient());
                break;
            case BloodTestRoomNotifications.QueueEmpty:
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
