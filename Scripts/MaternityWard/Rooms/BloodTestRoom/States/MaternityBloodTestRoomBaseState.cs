using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class MaternityBloodTestBaseState:BloodTestIState
{
    protected MaternityBloodTestRoom parent;
    protected BloodTestRoomState stateTag;

    public MaternityBloodTestBaseState(MaternityBloodTestRoom parent)
    {
        this.parent = parent;
    }

    public void BroadcastData()
    {

    }

    public void EmulateTime(TimePassedObject timePassed)
    {

    }

    public BloodTestRoomState GetTag()
    {
        return stateTag;
    }

    public abstract void Notify(int id, object parameters);
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnUpdate();
    public virtual string SaveToString()
    {
        return stateTag.ToString();
    }
}

public enum BloodTestRoomNotifications
{
    OfficeUnanchored,
    OfficeAnchored,
    PatientAdded,
    QueueEmpty,
}
