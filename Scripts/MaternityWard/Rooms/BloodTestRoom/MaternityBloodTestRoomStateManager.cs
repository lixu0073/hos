using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityBloodTestRoomStateManager<Parent> where Parent : IStateChangable
{
    Parent parent;
    public BloodTestIState State
    {
        get
        {
            return state;
        }

        set
        {
            if (state == value)
                return;

            if (state != null)
                state.OnExit();

            state = value;

            if (state != null)
                state.OnEnter();

            if (parent != null)
                parent.NotifyStateChanged();
        }
    }
    private BloodTestIState state;

    public MaternityBloodTestRoomStateManager(Parent parent)
    {
        this.parent = parent;
    }

    public void Update()
    {
        if (state != null)
            state.OnUpdate();
    }
}
