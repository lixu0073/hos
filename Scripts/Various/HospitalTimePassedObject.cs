using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HospitalTimePassedObject : TimePassedObject
{
    public HospitalTimePassedObject(long hospitalSaveTime, long maternitySaveTime) : base(hospitalSaveTime, maternitySaveTime)
    {
    }

    public override long GetSaveTime()
    {
        return hospitalSaveTime;
    }

    public override long GetTimePassed()
    {
        if (hospitalTimePassed < 0)
        {
            hospitalTimePassed = 0;
        }
        return hospitalTimePassed;
    }
}
