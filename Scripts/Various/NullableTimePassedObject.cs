using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullableTimePassedObject : TimePassedObject
{
    public NullableTimePassedObject(long hospitalSaveTime, long maternitySaveTime) : base(hospitalSaveTime, maternitySaveTime)
    {
        hospitalTimePassed = 0;
        maternityTimePassed = 0;
    }

    public override long GetSaveTime()
    {
        return 0;
    }

    public override long GetTimePassed()
    {
        return 0;
    }
}
