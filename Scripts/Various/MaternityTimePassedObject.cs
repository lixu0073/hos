using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityTimePassedObject : TimePassedObject
{
    public MaternityTimePassedObject(long hospitalSaveTime, long maternitySaveTime) : base(hospitalSaveTime, maternitySaveTime)
    {
    }

    public override long GetSaveTime()
    {
        return maternitySaveTime;
    }

    public override long GetTimePassed()
    {
        if (maternityTimePassed < 0)
        {
            maternityTimePassed = 0;
        }
        return maternityTimePassed;
    }
}
