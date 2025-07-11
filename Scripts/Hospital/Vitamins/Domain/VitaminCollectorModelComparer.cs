using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitaminCollectorModelComparer : IComparer<VitaminCollectorModel>
{
    public int Compare(VitaminCollectorModel x, VitaminCollectorModel y)
    {
        if (x.FillRatio < y.FillRatio)
        {
            return -1;
        }
        else if (x.FillRatio > y.FillRatio)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
