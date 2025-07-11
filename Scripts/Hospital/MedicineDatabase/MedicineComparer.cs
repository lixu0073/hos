using UnityEngine;
using System.Collections;
using Hospital;
using System.Collections.Generic;
using System;

public class MedicineComparer : IComparer<MedicineDatabaseEntry>
{
    public int Compare(MedicineDatabaseEntry x, MedicineDatabaseEntry y)
    {
        if (x.minimumLevel > y.minimumLevel)
        {
            return -1;
        }
        else if (x.minimumLevel < y.minimumLevel)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
